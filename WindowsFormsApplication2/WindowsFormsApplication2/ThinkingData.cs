using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.IO.Compression;
using Newtonsoft.Json.Converters;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace ThinkingData.Analytics
{
    public enum TALogging
    {
        TALoggingNone,
        TALoggingLog
    }

    public static class TACommon
    {
        private static string libName = "CSharp";
        private static string libVersion = "1.3.0-beta.1";
        private static TALogging logType = TALogging.TALoggingNone;

        public static string LibName
        {
            get { return libName; }
        }
        public static string LibVersion
        {
            get { return libVersion; }
        }
        public static TALogging LogType
        {
            get { return logType; }
            set { logType = value; }
        }
    }

    public class HttpSender
    {
        private const int MaxFlushBatchSize = 20;
        private const int DefaultTimeOutSecond = 30;

        private readonly string _url;
        private readonly string _appId;
        private readonly int _batchSize;
        private readonly int _requestTimeoutMillisecond;
        private readonly bool _throwException;
        private readonly List<Dictionary<string, object>> _messageList;

        private readonly IsoDateTimeConverter _timeConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff" };


        static bool isNetworking = false;
        static Mutex ta_networking_mutex = new Mutex();

        public HttpSender(string serverUrl, string appId) : this(serverUrl, appId, MaxFlushBatchSize,
            DefaultTimeOutSecond, false)
        {
        }

        public HttpSender(string serverUrl, string appId, int batchSize, int requestTimeoutSecond,
            bool throwException)
        {
            _messageList = new List<Dictionary<string, object>>();
            var relativeUri = new Uri("/sync", UriKind.Relative);
            _url = new Uri(new Uri(serverUrl), relativeUri).AbsoluteUri;
            this._appId = appId;
            this._batchSize = Math.Min(MaxFlushBatchSize, batchSize);
            this._throwException = throwException;
            this._requestTimeoutMillisecond = 3 * 1000;
        }

        public void Send(Dictionary<string, object> message)
        {
            lock (_messageList)
            {
                _messageList.Add(message);
                _Flush();
            }
        }


        public async Task<bool> _Flush()
        {

            await Task.Run(async () =>
            {
                ta_networking_mutex.WaitOne();
                if (isNetworking)
                {
                    ta_networking_mutex.ReleaseMutex();
                    return;
                }
                isNetworking = true;
                ta_networking_mutex.ReleaseMutex();

                Int64 messageCount;
                lock (_messageList)
                {
                    messageCount = _messageList.Count;
                }

                while (messageCount > 0)
                {
                    List<Dictionary<string, object>> batchList = new List<Dictionary<string, object>> { };
                    try
                    {
                        batchList.Add(_messageList[0]);
                    }
                    catch (Exception ex)
                    {
                        if (_throwException)
                        {
                            throw new SystemException("[ThinkingEngine] Failed to get batchList.", ex);
                        }
                        continue;
                    }

                    var finalDic = new Dictionary<string, object>();
                    finalDic.Add("#app_id", this._appId);
                    finalDic.Add("data", batchList);

                    // Serialize
                    string sendingData;
                    try
                    {
                        sendingData = JsonConvert.SerializeObject(finalDic, _timeConverter);
                    }
                    catch (Exception exception)
                    {
                        try
                        {
                            if (_messageList.Count > 0)
                            {
                                _messageList.RemoveAt(0);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (_throwException)
                            {
                                throw new SystemException("[ThinkingEngine] Failed to remove first message.", ex);
                            }
                            continue;
                        }

                        if (_throwException)
                        {
                            throw new SystemException("[ThinkingEngine] Failed to serialize data.", exception);
                        }
                        continue;
                    }

                    var result = await SendToServer(sendingData, batchList.Count);
                    if (result)
                    {
                        if (TACommon.LogType == TALogging.TALoggingLog)
                        {
                            Console.WriteLine("[ThinkingData] Debug: Send event, Request = ");
                            Console.WriteLine(sendingData);                         
                        }

                        try
                        {
                            if (_messageList.Count > 0)
                            {
                                _messageList.RemoveAt(0);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (_throwException)
                            {
                                throw new SystemException("[ThinkingEngine] Failed to remove first message.", ex);
                            }
                        }
                    }
                    else
                    {
                        ta_networking_mutex.WaitOne();
                        isNetworking = false;
                        ta_networking_mutex.ReleaseMutex();
                        break;
                    }
                    lock (_messageList)
                    {
                        messageCount = _messageList.Count;
                    }
                }
                ta_networking_mutex.WaitOne();
                isNetworking = false;
                ta_networking_mutex.ReleaseMutex();
            });

            return true;
        }

        private async Task<bool> SendToServer(string dataStr, int batchCount)
        {
            bool sendSuccess = false;
            try
            {
                var dataCompressed = Gzip(dataStr);
                var base64string = System.Convert.ToBase64String(dataCompressed);
                var dataBody = Encoding.UTF8.GetBytes(base64string);

                var request = (HttpWebRequest)WebRequest.Create(this._url);
                request.Method = "POST";
                request.ReadWriteTimeout = _requestTimeoutMillisecond;
                request.Timeout = _requestTimeoutMillisecond;
                request.ContentType = "text/plain";
                request.ContentLength = dataBody.Length;
                request.Headers.Add("TA-Integration-Type", TACommon.LibName);
                request.Headers.Add("TA-Integration-Version", TACommon.LibVersion);
                request.Headers.Add("TA-Integration-Extra", TACommon.LibName);
                request.Headers.Add("TA-Integration-Count", batchCount.ToString());

                using (var stream = await request.GetRequestStreamAsync())
                {
                    stream.Write(dataBody, 0, dataBody.Length);
                    stream.Flush();
                }

                var response = await request.GetResponseAsync() as HttpWebResponse;

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                Console.WriteLine("[ThinkingData] Debug: Send event, Response = " + responseString);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    if (_throwException)
                    {
                        throw new SystemException("[ThinkingEngine] error msg: send response is not 200, content: " + responseString);
                    }
                }

                response.Close();
                var resultJson = JsonConvert.DeserializeObject<Dictionary<object, object>>(responseString);

                int code = Convert.ToInt32(resultJson["code"]);

                if (code != 0)
                {
                    if (code == -1)
                    {
                        if (_throwException)
                        {
                            throw new SystemException("[ThinkingEngine] error msg:" +
                                                    (resultJson.ContainsKey("msg")
                                                        ? resultJson["msg"]
                                                        : "invalid data format"));
                        }
                    }
                    else if (code == -2)
                    {
                        if (_throwException)
                        {
                            throw new SystemException("[ThinkingEngine] error msg:" +
                                                    (resultJson.ContainsKey("msg")
                                                        ? resultJson["msg"]
                                                        : "APP ID doesn't exist"));
                        }
                    }
                    else if (code == -3)
                    {
                        if (_throwException)
                        {
                            throw new SystemException("[ThinkingEngine] error msg:" +
                                                    (resultJson.ContainsKey("msg")
                                                        ? resultJson["msg"]
                                                        : "invalid ip transmission"));
                        }
                    }
                    else
                    {
                        if (_throwException)
                        {
                            throw new SystemException("[ThinkingEngine] Unexpected response return code: " + code);
                        }
                    }
                }
                else
                {
                    sendSuccess = true;
                }
            }
            catch (Exception)
            {
                if (_throwException)
                {
                    throw new SystemException("[ThinkingEngine] Unexpected sendToServer error ");
                }
            }

            return sendSuccess;
        }

        private static byte[] Gzip(string inputStr)
        {
            var inputBytes = Encoding.UTF8.GetBytes(inputStr);
            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    gzipStream.Write(inputBytes, 0, inputBytes.Length);
                return outputStream.ToArray();
            }
        }
    }

    public delegate Dictionary<string, object> TADynamicPropertyAction();

    public class ThinkingdataAnalytics
    {
        private string _appId;
        private string _serverUrl;
        private string _deviceId;
        private string _accountId;
        private string _distinctId;
        private readonly Dictionary<string, object> _pubicProperties;
        private readonly HttpSender _httpSender;
        private static int systemTickCount;
        private static long calibratedTime = 0;

        static Mutex _taIdMutex = new Mutex();
        static Mutex _taSuperPropertyMutex = new Mutex();

        public TADynamicPropertyAction dynamicPropertyAction;

        private static readonly Regex KeyPattern =
            new Regex("^(#[a-z][a-z0-9_]{0,49})|([a-z][a-z0-9_]{0,50})$", RegexOptions.IgnoreCase);

        public static void setLoggingType(TALogging type)
        {
            TACommon.LogType = type;
        }

        public static void CalibrateTime(long timeStamp) {
            calibratedTime = timeStamp;
            systemTickCount = Environment.TickCount;
        }

        /**
         * After the SDK initialization is complete, the saved instance can be obtained through this api
         *
         * @return SDK instance
         */
        public ThinkingdataAnalytics(string appid, string url)
        {
            // init httpSender
            this._httpSender = new HttpSender(url, appid);

            // throw appid and url are null 
            if (string.IsNullOrEmpty(appid))
            {
                throw new SystemException("[ThinkingData] Error: The appid must be provided.");
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new SystemException("[ThinkingData] Error: The url must be provided.");
            }

            _appId = appid;
            _serverUrl = url;

            // get deviceid
            _taIdMutex.WaitOne();
            _deviceId = readIdentityId(filePathDeviceId());
            if (string.IsNullOrEmpty(_deviceId))
            {
                _deviceId = Guid.NewGuid().ToString();
                updateIdentityId(filePathDeviceId(), _deviceId);
            }
            _taIdMutex.ReleaseMutex();

            // get distinctid
            _taIdMutex.WaitOne();
            _distinctId = readIdentityId(filePathDistinctId());
            if (string.IsNullOrEmpty(_distinctId))
            {
                _distinctId = _deviceId;
                updateIdentityId(filePathDistinctId(), _distinctId);
            }
            _taIdMutex.ReleaseMutex();

            // get accountid
            _taIdMutex.WaitOne();
            _accountId = readIdentityId(filePathAccountId());
            if (string.IsNullOrEmpty(_accountId))
            {
                _accountId = "";
                updateIdentityId(filePathAccountId(), _accountId);
            }
            _taIdMutex.ReleaseMutex();

            // get superproperty
            _taSuperPropertyMutex.WaitOne();
            _pubicProperties = readSuperProperty(filePathSuperProperty());
            if (_pubicProperties == null)
            {
                _pubicProperties = new Dictionary<string, object>();
            }
            _taSuperPropertyMutex.ReleaseMutex();

            if (TACommon.LogType == TALogging.TALoggingLog)
            {
                Console.WriteLine("[ThinkingData] Info: TDAnalytics SDK initialize success, AppId = " + appid + ", ServerUrl = " + url + ", Mode = Normal, DeviceId = " + _deviceId + ", Lib = " + TACommon.LibName + ", LibVersion = " + TACommon.LibVersion);
            }
        }

        /**
        * Set the public event attribute, which will be included in every event uploaded after that. The public event properties are saved without setting them each time.
        *
        * @param properties public event attribute
        */
        public void SetPublicProperties(Dictionary<string, object> properties)
        {
            if (properties == null || properties.Keys.Count == 0)
            {
                ClearPublicProperties();
            }
            else if (properties.Keys.Count > 0)
            {
                _taSuperPropertyMutex.WaitOne();
                foreach (var kvp in properties)
                {
                    _pubicProperties[kvp.Key] = kvp.Value;
                }
                updateSuperProperty(filePathSuperProperty(), _pubicProperties);
                _taSuperPropertyMutex.ReleaseMutex();
            }
        }

        public Dictionary<string, object> GetSuperProperties() {
            Dictionary<string, object> superProperties;
            _taSuperPropertyMutex.WaitOne();
            superProperties = readSuperProperty(filePathSuperProperty());
            _taSuperPropertyMutex.ReleaseMutex();
            return superProperties;
        }

        /**
           *  Clear all public event attributes.
           */
        public void ClearPublicProperties()
        {
            _taSuperPropertyMutex.WaitOne();
            _pubicProperties.Clear();
            updateSuperProperty(filePathSuperProperty(), _pubicProperties);
            _taSuperPropertyMutex.ReleaseMutex();
        }

        /**
         * Set the distinct ID to replace the default UUID distinct ID.
         *
         * @param token distinct ID
         */
        public void SetIdentity(string token)
        {
            _taIdMutex.WaitOne();
            updateIdentityId(filePathDistinctId(), token);
            _distinctId = token;
            if (TACommon.LogType == TALogging.TALoggingLog)
            {
                Console.WriteLine("[ThinkingData] Info: Setting distinct ID, DistinctId = " + token);
            }
            _taIdMutex.ReleaseMutex();

        }

        public string GetDistinctId() {
            string distinctId = "";
            _taIdMutex.WaitOne();
            distinctId = readIdentityId(filePathDistinctId());
            _taIdMutex.ReleaseMutex();
            return distinctId;
        }

        /**
         * Set the account ID. Each setting overrides the previous value. Login events will not be uploaded.
         *
         * @param token account ID
         */
        public void Login(string token)
        {
            _taIdMutex.WaitOne();
            updateIdentityId(filePathAccountId(), token);
            _accountId = token;
            if (TACommon.LogType == TALogging.TALoggingLog)
            {
                Console.WriteLine("[ThinkingData] Info: Login SDK, AccountId = " + token);
            }
            _taIdMutex.ReleaseMutex();

        }

        /**
         * Clearing the account ID will not upload user logout events.
         */
        public void Logout()
        {
            _taIdMutex.WaitOne();
            updateIdentityId(filePathAccountId(), "");
            _accountId = "";
            if (TACommon.LogType == TALogging.TALoggingLog)
            {
                Console.WriteLine("[ThinkingData] Info: Logout SDK");
            }
            _taIdMutex.ReleaseMutex();

        }


        public string GetDeviceId()
        {
            string deviceId = "";
            _taIdMutex.WaitOne();
            deviceId = readIdentityId(filePathDeviceId());
            _taIdMutex.ReleaseMutex();
            return deviceId;
        }


        private string filePathDeviceId()
        {
            return Path.Combine(AppContext.BaseDirectory, "_ta_deviceid_" + _appId + ".txt");
        }


        private string filePathAccountId()
        {
            return Path.Combine(AppContext.BaseDirectory, "_ta_accountid_" + _appId + ".txt");
        }


        private string filePathDistinctId()
        {
            return Path.Combine(AppContext.BaseDirectory, "_ta_distinctid_" + _appId + ".txt");
        }

        private string filePathSuperProperty()
        {
            return Path.Combine(AppContext.BaseDirectory, "_ta_superproperty_" + _appId + ".txt");
        }


        private void updateIdentityId(string path, string identityId)
        {
            try
            {
                File.WriteAllText(path, identityId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ThinkingEngine] File WriteAllText Error");
            }

        }

        private void updateSuperProperty(string path, Dictionary<string, object> property)
        {
            try
            {
                string Contentjson = JsonConvert.SerializeObject(property);
                File.WriteAllText(path, Contentjson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ThinkingEngine] File WriteAllText Error");
            }
        }

        private string readIdentityId(string path)
        {
            string token = "";

            try
            {
                if (!System.IO.File.Exists(path))
                {
                    System.IO.File.WriteAllText(path, "");
                }
                else
                {
                    token = File.ReadAllText(path);
                }

                if (string.IsNullOrEmpty(token))
                {
                    token = "";
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[ThinkingEngine] File Read Token Error");
            }
            return token;

        }


        private Dictionary<string, object> readSuperProperty(string path)
        {
            Dictionary<string, object> superDic = new Dictionary<string, object>();
            try
            {
                if (!System.IO.File.Exists(path))
                {
                    System.IO.File.WriteAllText(path, "");
                }
                else
                {
                    string superPropertyString = File.ReadAllText(path);
                    superDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(superPropertyString);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[ThinkingEngine] File Read SuperProperty Error");
            }

            return superDic;
        }

        /**
         * track a event
         *
         * @param event_name event name
         */
        public void Track(string event_name)
        {
            _Add("track", event_name, null, null, null);
        }

        /**
         * track a event
         *
         * @param event_name event name
         * @param properties event properties
         */
        public void Track(string event_name, Dictionary<string, object> properties)
        {
            if (string.IsNullOrEmpty(event_name))
            {
                throw new SystemException("[ThinkingEngine] The event name must be provided.");
            }
            _Add("track", event_name, null, null, properties);
        }

        /**
        * The first event refers to the ID of a device or other dimension, which will only be recorded once.
        */
        public void TrackFirst(string event_name, string first_check_id, Dictionary<string, object> properties)
        {
            if (string.IsNullOrEmpty(event_name))
            {
                throw new SystemException("[ThinkingEngine] The event name must be provided.");
            }

            if (string.IsNullOrEmpty(first_check_id))
            {
                first_check_id = _deviceId;
            }

            _Add("track", event_name, null, first_check_id, properties);
        }

        /**
        * The first event refers to the ID of a device or other dimension, which will only be recorded once.
        */

        public void TrackFirst(string event_name, Dictionary<string, object> properties)
        {
            TrackFirst(event_name, _deviceId, properties);
        }

        /**
        * You can implement the requirement to modify event data in a specific scenario through updatable events. Updatable events need to specify an ID that identifies the event and pass it in when the updatable event object is created.
        */
        public void TrackUpdate(string event_name, string event_id, Dictionary<string, object> properties)
        {
            if (string.IsNullOrEmpty(event_name))
            {
                throw new SystemException("[ThinkingEngine] The event name must be provided.");
            }

            if (string.IsNullOrEmpty(event_id))
            {
                throw new SystemException("[ThinkingEngine] The event id must be provided.");
            }

            _Add("track_update", event_name, event_id, null, properties);
        }

        /**
         * Rewritable events will completely cover historical data with the latest data, which is equivalent to deleting the previous data and storing the latest data in effect. 
        */
        public void TrackOverwrite(string event_name, string event_id, Dictionary<string, object> properties)
        {
            if (string.IsNullOrEmpty(event_name))
            {
                throw new SystemException("[ThinkingEngine] The event name must be provided.");
            }

            if (string.IsNullOrEmpty(event_id))
            {
                throw new SystemException("[ThinkingEngine] The event id must be provided.");
            }

            _Add("track_overwrite", event_name, event_id, null, properties);
        }

        /**
         * Sets the user property, replacing the original value with the new value if the property already exists.
         *
         * @param properties user property
         */
        public void UserSet(Dictionary<string, object> properties)
        {
            _Add("user_set", properties);
        }

        /**
          * Reset user properties.
          *
          * @param properties user properties
          */
        public void UserUnSet(List<string> properties)
        {
            var props = properties.ToDictionary<string, string, object>(property => property, property => 0);
            _Add("user_unset", props);
        }

        /**
          *  Sets a single user attribute, ignoring the new attribute value if the attribute already exists.
          *
          * @param properties user property
          */
        public void UserSetOnce(Dictionary<string, object> properties)
        {
            _Add("user_setOnce", properties);
        }

        /**
        *  Sets a single user attribute, ignoring the new attribute value if the attribute already exists.
        */
        public void UserSetOnce(string property, object value)
        {
            var properties = new Dictionary<string, object> { { property, value } };
            _Add("user_setOnce", properties);
        }

        /**
           * Adds the numeric type user attributes.
           *
           * @param properties user property
           */
        public void UserAdd(Dictionary<string, object> properties)
        {
            _Add("user_add", properties);
        }

        /**
           * Adds the numeric type user attributes.
           */

        public void UserAdd(string property, long value)
        {
            var properties = new Dictionary<string, object> { { property, value } };
            _Add("user_add", properties);
        }

        /**
       * Append a user attribute of the List type.
       *
       * @param properties user property
       */
        public void UserAppend(Dictionary<string, object> properties)
        {
            _Add("user_append", properties);
        }

        public void UserUniqAppend(Dictionary<string, object> properties) {
            _Add("user_uniq_append", properties);
        }

        /**
           * Delete the user attributes,This operation is not reversible and should be performed with caution.
           */
        public void UserDelete()
        {
            _Add("user_del", new Dictionary<string, object>());
        }

        /**
         * Empty the cache queue. When this api is called, the data in the current cache queue will attempt to be reported.
         * If the report succeeds, local cache data will be deleted.
         */
        public void Flush()
        {
            _httpSender._Flush();
        }

        private static bool IsNumber(object value)
        {
            return (value is sbyte) || (value is short) || (value is int) || (value is long) || (value is byte)
                   || (value is ushort) || (value is uint) || (value is ulong) || (value is decimal) ||
                   (value is float) || (value is double);
        }

        private static void AssertProperties(string type, IDictionary<string, object> properties)
        {
            if (null == properties)
            {
                return;
            }

            foreach (var kvp in properties)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                if (null == value)
                {
                    continue;
                }

                if (KeyPattern.IsMatch(key))
                {
                    if (!IsNumber(value) && !(value is string) && !(value is DateTime) && !(value is bool) && !(value is IList) && !(value is IDictionary))
                    {
                        throw new ArgumentException("The supported data type including: Number, String, Date, Boolean, List, IDictionary. Invalid property: {key}");
                    }

                    if (type == "user_add" && !IsNumber(value))
                    {
                        throw new ArgumentException("Only Number is allowed for user_add. Invalid property:" + key);
                    }
                }
                else
                {
                    throw new ArgumentException("The " + type + "'" + key + "' is invalid.");
                }
            }
        }

        private void _Add(string type, IDictionary<string, object> properties)
        {
            _Add(type, null, null, null, properties);
        }

        private void _Add(string type, string event_name, string event_id, string first_check_id, IDictionary<string, object> properties)
        {
            var evt = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(_accountId))
            {
                evt.Add("#account_id", _accountId);
            }

            if (!string.IsNullOrEmpty(_distinctId))
            {
                evt.Add("#distinct_id", _distinctId);
            }
            else
            {
                evt.Add("#distinct_id", _deviceId);
            }


            if (!string.IsNullOrEmpty(event_name))
            {
                evt.Add("#event_name", event_name);
            }

            if (!string.IsNullOrEmpty(event_id))
            {
                if (type == "track_update" || type == "track_overwrite")
                {
                    evt.Add("#event_id", event_id);
                }
            }

            if (!string.IsNullOrEmpty(first_check_id))
            {
                evt.Add("#first_check_id", first_check_id);
            }

            
            if (calibratedTime == 0)
            {
                evt.Add("#time", DateTime.Now);
            }
            else {

                long timestampInMilliseconds = calibratedTime + Environment.TickCount - systemTickCount;
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                dateTime = dateTime.AddMilliseconds(timestampInMilliseconds).ToLocalTime();
                evt.Add("#time", dateTime);
            }

            evt.Add("#type", type);
            evt.Add("#uuid", Guid.NewGuid().ToString("D"));

            if (properties == null)
            {
                properties = new Dictionary<string, object>();
            }

            var eventProperties = new Dictionary<string, object>(properties);
            if (type == "track" || type == "track_update" || type == "track_overwrite" || type == "track_first")
            {
                Dictionary<string, object> dynamicProperties = new Dictionary<string, object>();
                if (dynamicPropertyAction != null)
                {
                    dynamicProperties = dynamicPropertyAction();
                }

                if (dynamicProperties.Keys.Count != 0)
                {
                    foreach (var kvp in dynamicProperties)
                    {
                        if (!eventProperties.ContainsKey(kvp.Key))
                        {
                            eventProperties.Add(kvp.Key, kvp.Value);
                        }
                    }
                }

                _taSuperPropertyMutex.WaitOne();
                if (_pubicProperties != null)
                {
                    foreach (var kvp in _pubicProperties)
                    {
                        if (!eventProperties.ContainsKey(kvp.Key))
                        {
                            eventProperties.Add(kvp.Key, kvp.Value);
                        }
                    }
                }
                _taSuperPropertyMutex.ReleaseMutex();

                eventProperties.Add("#lib_version", TACommon.LibVersion);
                eventProperties.Add("#lib", TACommon.LibName);

                if (Environment.OSVersion.Platform == System.PlatformID.Win32Windows)
                {
                    eventProperties.Add("#os", "Windows");
                }
                else if (Environment.OSVersion.Platform == System.PlatformID.Win32S)
                {
                    eventProperties.Add("#os", "Windows");
                }
                else if (Environment.OSVersion.Platform == System.PlatformID.Win32NT)
                {
                    eventProperties.Add("#os", "Windows");
                }
                else if (Environment.OSVersion.Platform == System.PlatformID.WinCE)
                {
                    eventProperties.Add("#os", "Windows");
                }
                else if (Environment.OSVersion.Platform == System.PlatformID.MacOSX)
                {
                    eventProperties.Add("#os", "Mac");
                }
                eventProperties.Add("#device_id", _deviceId);
            }

            AssertProperties(type, eventProperties);

            evt.Add("properties", eventProperties);

            if (TACommon.LogType == TALogging.TALoggingLog)
            {
                IsoDateTimeConverter _timeConverter = new IsoDateTimeConverter
                {
                    DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff"
                };
                Console.WriteLine("[ThinkingData] Info: Enqueue data,");
                Console.WriteLine(JsonConvert.SerializeObject(evt, _timeConverter));
            }

            _httpSender.Send(evt);
        }
    }

    public class TDEventModel
    {
        public string eventName;
        public Dictionary<string, object> properties;
        public TDEventModel(string eventName, Dictionary<string, object> properties)
        {
            this.eventName = eventName;
            this.properties = properties;
        }
    }

    public class TDFirstEventModel : TDEventModel
    {
        public string firstCheckId;
        public TDFirstEventModel(string eventName, Dictionary<string, object> properties, string firstCheckId = "") : base(eventName, properties)
        {
            this.firstCheckId = firstCheckId;
        }
    }

    public class TDUpdatableEventModel : TDEventModel
    {
        public string eventId;
        public TDUpdatableEventModel(string eventName, Dictionary<string, object> properties,string eventId) : base(eventName, properties)
        {
            this.eventId = eventId;
        }
    }

    public class TDOverwritableEventModel : TDEventModel
    {
        public string eventId;
        public TDOverwritableEventModel(string eventName, Dictionary<string, object> properties, string eventId) : base(eventName, properties)
        {
            this.eventId = eventId;
        }
    }

    /// <summary>
    /// The packaging class of ThinkingAnalyticsSDK provides static methods, which is more convenient for customers to use
    /// </summary>
    public class TDAnalytics
    {
        private static ThinkingdataAnalytics _instance;

        /// <summary>
        /// time calibration with timestamp
        /// </summary>
        /// <param name="timeStamp">timestamp</param>
        public static void CalibrateTime(long timeStamp) {
            ThinkingdataAnalytics.CalibrateTime(timeStamp);
        }

        /// <summary>
        /// Initialize the SDK. The track function is not available until this interface is invoked.
        /// </summary>
        /// <param name="appId">app id</param>
        /// <param name="serverUrl">server url</param>
        public static void Init(string appId, string serverUrl)
        {
            if (_instance == null)
            {
                _instance = new ThinkingdataAnalytics(appId, serverUrl);
            }
        }

        /// <summary>
        /// Upload a single event, containing only preset properties and set public properties.
        /// </summary>
        /// <param name="eventName">event name</param>
        /// <param name="properties">event properties</param>
        public static void Track(string eventName, Dictionary<string, object> properties = null)
        {
            if (_instance == null) return;
            if (properties == null) {
                properties = new Dictionary<string, object>();
            }
            _instance?.Track(eventName, properties);
        }

        /// <summary>
        /// Upload a special type of event.
        /// </summary>
        /// <param name="model">Event Object TDFirstEventModel / TDOverWritableEventModel / TDUpdatableEventModel</param>
        public static void Track(TDEventModel model) {
            if (_instance == null) return;
            if (model is TDFirstEventModel firstEventModel)
            {
                _instance.TrackFirst(firstEventModel.eventName, firstEventModel.firstCheckId, firstEventModel.properties);
            }
            else if (model is TDUpdatableEventModel updatableEventModel)
            {
                _instance.TrackUpdate(updatableEventModel.eventName, updatableEventModel.eventId, updatableEventModel.properties);

            }
            else if (model is TDOverwritableEventModel overwritableEventModel) {
                _instance.TrackOverwrite(overwritableEventModel.eventName, overwritableEventModel.eventId, overwritableEventModel.properties);
            }
        }

        /// <summary>
        /// Sets the user property, replacing the original value with the new value if the property already exists.
        /// </summary>
        /// <param name="properties">user property</param>
        public static void UserSet(Dictionary<string, object> properties)
        {
            if (_instance == null) return;
            _instance.UserSet(properties);
        }

        /// <summary>
        /// Sets a single user attribute, ignoring the new attribute value if the attribute already exists.
        /// </summary>
        /// <param name="properties">user property</param>
        public static void UserSetOnce(Dictionary<string, object> properties)
        {
            if (_instance == null) return;
            _instance.UserSetOnce(properties);
        }

        /// <summary>
        /// Reset user properties.
        /// </summary>
        /// <param name="property">user property</param>
        public static void UserUnset(string property)
        {
            List<string> list = new List<string>() {
                property
            };
            _instance?.UserUnSet(list);
        }

        /// <summary>
        /// Reset user properties.
        /// </summary>
        /// <param name="properties">list of user property</param>
        public static void UserUnset(List<string> properties)
        {
            if (properties == null) return;
            _instance.UserUnSet(properties);
        }

        /// <summary>
        /// Only one attribute is set when the user attributes of a numeric type are added.
        /// </summary>
        /// <param name="properties">user property</param>
        public static void UserAdd(Dictionary<string, object> properties)
        {
            if (_instance == null) return;
            _instance.UserAdd(properties);
        }

        /// <summary>
        /// Append a user attribute of the List type.
        /// </summary>
        /// <param name="properties">user property</param>
        public static void UserAppend(Dictionary<string, object> properties)
        {
            if (_instance == null) return;
            _instance.UserAppend(properties);
        }

        /// <summary>
        /// The element appended to the library needs to be done to remove the processing, remove the support, and then import.
        /// </summary>
        /// <param name="properties">user property</param>
        public static void UserUniqAppend(Dictionary<string, object> properties) {
            if (_instance == null) return;
            _instance.UserUniqAppend(properties);
        }

        /// <summary>
        /// Delete the user attributes, but retain the uploaded event data. This operation is not reversible and should be performed with caution.
        /// </summary>
        public static void UserDelete()
        {
            if (_instance == null) return;
            _instance.UserDelete();
        }

        /// <summary>
        /// Set the public event attribute, which will be included in every event uploaded after that. The public event properties are saved without setting them each time.
        /// </summary>
        /// <param name="properties">super properties</param>
        public static void SetSuperProperties(Dictionary<string, object> properties)
        {
            if (_instance == null) return;
            _instance.SetPublicProperties(properties);
        }

        /// <summary>
        /// Gets the public event properties that have been set.
        /// </summary>
        /// <returns>public event properties that have been set</returns>
        public static Dictionary<string, object> GetSuperProperties() {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            if (_instance != null) {
                properties = _instance.GetSuperProperties();
            }
            return properties;
        }

        /// <summary>
        /// Clear all public event attributes.
        /// </summary>
        public static void ClearSuperProperties()
        {
            if (_instance == null) return;
            _instance.ClearPublicProperties();
        }

        /// <summary>
        /// Set the account ID. Each setting overrides the previous value. Login events will not be uploaded.
        /// </summary>
        /// <param name="account"></param>
        public static void Login(string account)
        {
            if (_instance == null) return;
            _instance.Login(account);
        }

        /// <summary>
        /// Clearing the account ID will not upload user logout events.
        /// </summary>
        public static void Logout()
        {
            if (_instance == null) return;
            _instance.Logout();
        }

        /// <summary>
        /// Set the distinct ID to replace the default UUID distinct ID.
        /// </summary>
        /// <param name="distinctId">distinct id</param>
        public static void SetDistinctId(string distinctId)
        {
            if (_instance == null) return;
            _instance.SetIdentity(distinctId);
        }

        /// <summary>
        /// Obtain the device ID.
        /// </summary>
        /// <returns>device ID</returns>
        public static string GetDeviceId()
        {
            if (_instance == null)
            {
                return "";
            }
            return _instance.GetDeviceId();
        }

        /// <summary>
        /// Get a visitor ID: The #distinct_id value in the reported data.
        /// </summary>
        /// <returns>distinct ID</returns>
        public static string GetDistinctId() {
            if (_instance == null)
            {
                return "";
            }
            return _instance.GetDistinctId();
        }

        /// <summary>
        /// Empty the cache queue. When this function is called, the data in the current cache queue will attempt to be reported.
        /// </summary>
        public static void Flush()
        {
            if (_instance == null) return;
            _instance.Flush();
        }

        /// <summary>
        /// enable debug logging
        /// </summary>
        /// <param name="enable">log switch</param>
        public static void EnableLog(bool enable)
        {
            if (enable)
            {
                ThinkingdataAnalytics.setLoggingType(TALogging.TALoggingLog);
            }
            else
            {
                ThinkingdataAnalytics.setLoggingType(TALogging.TALoggingNone);
            }
        }

    }
}


