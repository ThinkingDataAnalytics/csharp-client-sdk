using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.IO.Compression;
using Newtonsoft.Json.Converters;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Forms;



namespace ThinkingData.Analytics
{
    public enum TALogging {
        TALoggingNone,
        TALoggingLog
    }

    public static class TACommon
    {  
        private static string libName = "CSharp";
        private static string libVersion = "1.1.0";
        private static TALogging logType = TALogging.TALoggingLog;

        public static string LibName
        {
            get { return libName; }
        }
        public static string LibVersion
        {
            get { return libVersion; }
        }
        public static TALogging LogType {
            get { return logType; }
            set { logType = value;  }
        }
    }

    public interface IConsumer
    {
        void Send(Dictionary<string, object> message);

        void Flush();

        void Close();
    }

    
    public class BatchConsumer : IConsumer
    {
        private const int MaxFlushBatchSize = 20;
        private const int DefaultTimeOutSecond = 30;

        private readonly List<Dictionary<string, object>> _messageList;

        private readonly IsoDateTimeConverter _timeConverter = new IsoDateTimeConverter
        {
            DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff"
        };

        private readonly string _url;
        private readonly string _appId;
        private readonly int _batchSize;
        private readonly int _requestTimeoutMillisecond;
        private readonly bool _throwException;
        private readonly bool _compress;

        public BatchConsumer(string serverUrl, string appId) : this(serverUrl, appId, MaxFlushBatchSize,
            DefaultTimeOutSecond, false, true)
        {
        }

        /**
         * 数据是否需要压缩，compress 内网可设置 false
         */
        public BatchConsumer(string serverUrl, string appId, bool compress) : this(serverUrl, appId, MaxFlushBatchSize,
            DefaultTimeOutSecond, false, compress)
        {
        }

        /**
         * batchSize 每次flush到TA的数据条数，默认20条
         */
        public BatchConsumer(string serverUrl, string appId, int batchSize) : this(serverUrl, appId, batchSize,
            DefaultTimeOutSecond)
        {
        }

        /**
         * batchSize 每次flush到TA的数据条数，默认20条
         * requestTimeoutSecond 发送服务器请求时间设置，默认30s
         */
        public BatchConsumer(string serverUrl, string appId, int batchSize, int requestTimeoutSecond) : this(serverUrl,
            appId, batchSize, requestTimeoutSecond, false)
        {
        }

        public BatchConsumer(string serverUrl, string appId, int batchSize, int requestTimeoutSecond,
            bool throwException, bool compress = true)
        {
            _messageList = new List<Dictionary<string, object>>();
            var relativeUri = new Uri("/sync", UriKind.Relative);
            _url = new Uri(new Uri(serverUrl), relativeUri).AbsoluteUri;
            this._appId = appId;
            this._batchSize = Math.Min(MaxFlushBatchSize, batchSize);
            this._throwException = throwException;
            this._compress = compress;
            this._requestTimeoutMillisecond = requestTimeoutSecond * 1000;
        }

        public void Send(Dictionary<string, object> message)
        {
            lock (_messageList)
            {
                _messageList.Add(message);
                if (_messageList.Count >= _batchSize)
                {
                    Flush();
                }
            }
        }

        public void Flush()
        {
            lock (_messageList)
            {
                while (_messageList.Count != 0)
                {
                    var batchRecordCount = Math.Min(_batchSize, _messageList.Count);
                    var batchList = _messageList.GetRange(0, batchRecordCount);

                    var finalDic = new Dictionary<string, object>();
                    finalDic.Add("#app_id", this._appId);
                    finalDic.Add("data", batchList);


                    string sendingData;
                    try
                    {
                        sendingData = JsonConvert.SerializeObject(finalDic, _timeConverter);
                    }
                    catch (Exception exception)
                    {
                        _messageList.RemoveRange(0, batchRecordCount);
                        if (_throwException)
                        {
                            throw new SystemException("Failed to serialize data.", exception);
                        }

                        continue;
                    }

                    try
                    {
                        if (TACommon.LogType == TALogging.TALoggingLog)
                        {
                            Console.WriteLine("flush datas:");
                            Console.WriteLine(sendingData);
                        }
                        SendToServer(sendingData, batchList.Count);
                        _messageList.RemoveRange(0, batchRecordCount);
                    }
                    catch (Exception exception)
                    {
                        if (_throwException)
                        {
                            throw new SystemException("Failed to send message with BatchConsumer.", exception);
                        }

                        return;
                    }
                }
            }
        }

        private void SendToServer(string dataStr, int batchCount)
        {
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

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(dataBody, 0, dataBody.Length);
                    stream.Flush();
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();


                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new SystemException("C# SDK send response is not 200, content: " + responseString);
                }

                response.Close();
                var resultJson = JsonConvert.DeserializeObject<Dictionary<object, object>>(responseString);

                int code = Convert.ToInt32(resultJson["code"]);

                if (code != 0)
                {
                    if (code == -1)
                    {
                        throw new SystemException("error msg:" +
                                                  (resultJson.ContainsKey("msg")
                                                      ? resultJson["msg"]
                                                      : "invalid data format"));
                    }
                    else if (code == -2)
                    {
                        throw new SystemException("error msg:" +
                                                  (resultJson.ContainsKey("msg")
                                                      ? resultJson["msg"]
                                                      : "APP ID doesn't exist"));
                    }
                    else if (code == -3)
                    {
                        throw new SystemException("error msg:" +
                                                  (resultJson.ContainsKey("msg")
                                                      ? resultJson["msg"]
                                                      : "invalid ip transmission"));
                    }
                    else
                    {
                        throw new SystemException("Unexpected response return code: " + code);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "\n  Cannot post message to " + _url);
                throw;
            }
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

        public void Close()
        {
            Flush();
        }
    }

    
    public delegate Dictionary<string, object> TADynamicPropertyAction();

    public class ThinkingdataAnalytics
    {
        private string _appid;
        private string _serverurl;

        private string _device_id;
        private string _account_id;
        private string _distinct_id;

        private readonly Dictionary<string, object> _pubicProperties;

        public TADynamicPropertyAction dynamicPropertyAction;

        private static readonly Regex KeyPattern =
            new Regex("^(#[a-z][a-z0-9_]{0,49})|([a-z][a-z0-9_]{0,50})$", RegexOptions.IgnoreCase);

        private readonly IConsumer _consumer;

        
        public static void setLoggingType(TALogging type)
        {
            TACommon.LogType = type;
        }


        /*
         * 实例化tga类，接收一个Consumer的一个实例化对象
         * @param consumer	BatchConsumer,LoggerConsumer实例
        */
        public ThinkingdataAnalytics(string appid, string url)
        {
            this._consumer = new BatchConsumer(url, appid);

            if (string.IsNullOrEmpty(appid))
            {
                throw new SystemException("The appid must be provided.");
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new SystemException("The url must be provided.");
            }

            _appid = appid;
            _serverurl = url;

            // 1.还原 deviceID 
            _device_id = readIdentityId(filePathDeviceId());
            if (string.IsNullOrEmpty(_device_id) )
            {
                _device_id = Guid.NewGuid().ToString();
                updateIdentityId(filePathDeviceId(), _device_id);
            }


            // 2. 还原distinctID
            _distinct_id = readIdentityId(filePathDistinctId());
            if (string.IsNullOrEmpty(_distinct_id))
            {
                _distinct_id = _device_id;
                updateIdentityId(filePathDistinctId(), _distinct_id);
            }


            // 3. 还原accountID
            _account_id = readIdentityId(filePathAccountId());
            if (string.IsNullOrEmpty(_account_id))
            {
                _account_id = "";
                updateIdentityId(filePathAccountId(), _account_id);
            }

            // 4. 公告属性
            _pubicProperties = readsuperProperty(filePathSuperProperty());

        }


        

        /*
        * 公共属性只用于track接口，其他接口无效，且每次都会自动向track事件中添加公共属性
        * @param properties	公共属性
        */
        public void SetPublicProperties(Dictionary<string, object> properties)
        {
            lock (_pubicProperties)
            {
                if (properties == null || properties.Keys.Count == 0)
                {
                    ClearPublicProperties();
                    _pubicProperties.Clear();
                }
                else if (properties.Keys.Count > 0)
                {
                    foreach (var kvp in properties)
                    {
                        _pubicProperties[kvp.Key] = kvp.Value;
                    }
                    updateSuperProperty(filePathSuperProperty(), _pubicProperties);
                }

            }
        }

        /*
	     * 清理掉公共属性，之后track属性将不会再加入公共属性
	     */
        public void ClearPublicProperties()
        {
            lock (_pubicProperties)
            {
                _pubicProperties.Clear();
                updateSuperProperty(filePathSuperProperty(), _pubicProperties);
            }
        }

        public void SetIdentity(string token)
        {
            updateIdentityId(filePathDistinctId(), token);
            _distinct_id = token;
        }

        public void Login(string token)
        {
            updateIdentityId(filePathAccountId(), token);
            _account_id = token;
        }
        
        public void Logout()
        {
            updateIdentityId(filePathAccountId(), "");
            _account_id = "";
        }

        public string GetDeviceId()
        {
            return readIdentityId(filePathDeviceId());
        }


        private string filePathDeviceId()
        {
            return Application.StartupPath + "\\_ta_deviceid_" + _appid + ".txt";
        }


        private string filePathAccountId()
        {
            return Application.StartupPath + "\\_ta_accountid_" + _appid + ".txt";
        }


        private string filePathDistinctId()
        {
            return Application.StartupPath + "\\_ta_distinctid_" + _appid + ".txt";
        }

        private string filePathSuperProperty()
        {
            return Application.StartupPath + "\\_ta_superproperty_" + _appid + ".txt";
        }

        private void updateIdentityId(string path, string idid)
        {
            File.WriteAllText(path, idid);
        }

       
        private void updateSuperProperty(string path, Dictionary<string, object> idid)
        {
            string Contentjson = JsonConvert.SerializeObject(idid);
            File.WriteAllText(path, Contentjson);
        }

        private string readIdentityId(string path)
        {
            
            string idid = "";
            
            if (!System.IO.File.Exists(path))
            {
                System.IO.File.WriteAllText(path, "");
                return "";
            }
            else
            {
                idid = File.ReadAllText(path);
            }
            if (idid == null)
            {
                idid = "";
            }
            return idid;

        }

        
        private Dictionary<string, object> readsuperProperty(string path)
        {
            
            Dictionary<string, object> superDic = new Dictionary<string, object>();

            if (!System.IO.File.Exists(path))
            {
                System.IO.File.WriteAllText(path, "");
                return superDic;
            }
            else
            {

                string idid = File.ReadAllText(path);
               
                Dictionary <string, object> DicContent = JsonConvert.DeserializeObject < Dictionary<string, object>> (idid);
                return DicContent;
            }

           

        }


        // 记录一个没有任何属性的事件
        public void Track(string event_name)
        {
            _Add( "track", event_name, null, null, null);
        }

        /*
	    * 用户事件属性(注册)
	    * @param	account_id	账号ID
	    * @param	distinct_id	匿名ID
	    * @param	event_name	事件名称
	    * @param	properties	事件属性
	    */
        public void Track(string event_name,
            Dictionary<string, object> properties)
        {
            if (string.IsNullOrEmpty(event_name))
            {
                throw new SystemException("The event name must be provided.");
            }

            _Add( "track", event_name, null, null, properties);
        }

        public void TrackFirst(string event_name, string first_check_id,
            Dictionary<string, object> properties)
        {
            if (string.IsNullOrEmpty(event_name))
            {
                throw new SystemException("The event name must be provided.");
            }

            if (string.IsNullOrEmpty(first_check_id))
            {
                throw new SystemException("The first check id must be provided.");
            }
            
            _Add( "track", event_name, null, first_check_id, properties);
        }

        public void TrackFirst(string event_name, Dictionary<string, object> properties)
        {
            TrackFirst(event_name, _device_id, properties);
        }

        

        /*
	    * 可更新事件属性
	    * @param	account_id	账号ID
	    * @param	distinct_id	匿名ID
	    * @param	event_name	事件名称
        * @param    event_id    事件ID
	    * @param	properties	事件属性
	    */
        public void TrackUpdate(string event_name, string event_id,
            Dictionary<string, object> properties)
        {
            if (string.IsNullOrEmpty(event_name))
            {
                throw new SystemException("The event name must be provided.");
            }

            if (string.IsNullOrEmpty(event_id))
            {
                throw new SystemException("The event id must be provided.");
            }

            _Add( "track_update", event_name, event_id, null, properties);
        }

        /*
	    * 可重写事件属性
	    * @param	account_id	账号ID
	    * @param	distinct_id	匿名ID
	    * @param	event_name	事件名称
        * @param    event_id    事件ID
	    * @param	properties	事件属性
	    */
        public void TrackOverwrite(string event_name, string event_id,
            Dictionary<string, object> properties)
        {
            if (string.IsNullOrEmpty(event_name))
            {
                throw new SystemException("The event name must be provided.");
            }

            if (string.IsNullOrEmpty(event_id))
            {
                throw new SystemException("The event id must be provided.");
            }

            _Add("track_overwrite", event_name, event_id, null, properties);
        }

        /*
         * 设置用户属性，如果已经存在，则覆盖，否则，新创建
         * @param	account_id	账号ID
         * @param	distinct_id	匿名ID
         * @param	properties	增加的用户属性
	     */
        public void UserSet(Dictionary<string, object> properties)
        {
            _Add( "user_set", properties);
        }

        /*
        * 删除用户属性
        * @param account_id 账号 ID
        * @param distinct_id 访客 ID
        * @param properties 用户属性
        */
        public void UserUnSet( List<string> properties)
        {
            var props = properties.ToDictionary<string, string, object>(property => property, property => 0);
            _Add( "user_unset", props);
        }

        /**
          * 设置用户属性，首次设置用户的属性,如果该属性已经存在,该操作为无效.
          * @param	account_id	账号ID
          * @param	distinct_id	匿名ID
          * @param	properties	增加的用户属性
         */
        public void UserSetOnce(Dictionary<string, object> properties)
        {
            _Add( "user_setOnce", properties);
        }

        /**
          * 首次设置用户的属性。这个接口只能设置单个key对应的内容。
          * @param	account_id	账号ID
          * @param	distinct_id	匿名ID
          * @param	properties	增加的用户属性
         */
        public void UserSetOnce(string property, object value)
        {
            var properties = new Dictionary<string, object> { { property, value } };
            _Add( "user_setOnce", properties);
        }

        /*
          * 用户属性修改，只支持数字属性增加的接口
          * @param	account_id	账号ID
          * @param	distinct_id	匿名ID
          * @param	properties	增加的用户属性
         */
        public void UserAdd(Dictionary<string, object> properties)
        {
            _Add( "user_add", properties);
        }

        public void UserAdd(string property, long value)
        {
            var properties = new Dictionary<string, object> { { property, value } };
            _Add( "user_add", properties);
        }

        /*
          * 追加用户的集合类型的一个或多个属性
          * @param	account_id	账号ID
          * @param	distinct_id	匿名ID
          * @param	properties	增加的用户属性
         */
        public void UserAppend(Dictionary<string, object> properties)
        {
            _Add( "user_append", properties);
        }

        /**
         * 用户删除,此操作不可逆
         * @param 	account_id	账号ID
         * @param	distinct_id	匿名ID
        */
        public void UserDelete()
        {
            _Add( "user_del", new Dictionary<string, object>());
        }

        /// 立即发送缓存中的所有日志
        public void Flush()
        {
            _consumer.Flush();
        }

        //关闭并退出 sdk 所有线程，停止前会清空所有本地数据
        public void Close()
        {
            _consumer.Close();
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
                    if (!IsNumber(value) && !(value is string) && !(value is DateTime) && !(value is bool) &&
                        !(value is IList) && !(value is IDictionary))
                    {
                        throw new ArgumentException(
                            "The supported data type including: Number, String, Date, Boolean,List. Invalid property: {key}");
                    }

                    // IList<object> list = value as List<object>;
                    // if (list != null)
                    //     for (var i = 0; i < list.Count; i++)
                    //     {
                    //         if (list[i] is DateTime)
                    //         {
                    //             list[i] = (DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss.fff");
                    //         }
                    //     }

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
            _Add( type, null, null, null, properties);
        }

        private void _Add( string type, string event_name, string event_id,string first_check_id,
            IDictionary<string, object> properties)
        {
            var evt = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(_account_id))
            {
                evt.Add("#account_id", _account_id);
            }

            if (!string.IsNullOrEmpty(_distinct_id))
            {
                evt.Add("#distinct_id", _distinct_id);
            }

            if (event_name != null)
            {
                evt.Add("#event_name", event_name);
            }

            if (event_id != null)
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

            evt.Add("#time", DateTime.Now);
            evt.Add("#type", type);
            evt.Add("#uuid", Guid.NewGuid().ToString("D"));

            // if (properties != null)
            // {
            //     foreach (var kvp in properties)
            //     {
            //         if (kvp.Value is DateTime time)
            //         {
            //             eventProperties[kvp.Key] = time.ToString("yyyy-MM-dd HH:mm:ss.fff");
            //         }
            //     }
            // }

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


                if (_pubicProperties != null)
                    lock (_pubicProperties)
                    {
                        foreach (var kvp in _pubicProperties)
                        {
                            if (!eventProperties.ContainsKey(kvp.Key))
                            {
                                eventProperties.Add(kvp.Key, kvp.Value);
                            }
                        }
                    }


                if (_pubicProperties != null)
                    lock (_pubicProperties)
                    {
                        foreach (var kvp in _pubicProperties)
                        {
                            if (!eventProperties.ContainsKey(kvp.Key))
                            {
                                eventProperties.Add(kvp.Key, kvp.Value);
                            }
                        }
                    }

                eventProperties.Add("#lib_version", TACommon.LibVersion);
                eventProperties.Add("#lib", TACommon.LibName);
                eventProperties.Add("#os", "Windows");
                eventProperties.Add("#device_id", _device_id);
            }
            AssertProperties(type, eventProperties);
            
            evt.Add("properties", eventProperties);

            _consumer.Send(evt);
        }
    }
}


