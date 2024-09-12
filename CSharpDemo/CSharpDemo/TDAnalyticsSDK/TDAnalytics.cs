
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ThinkingData.Analytics
{
    /// <summary>
    /// Wrapper Class
    /// </summary>
    public class TDAnalytics
    {
        const string LIB_NAME = "CSharp";
        const string LIB_VERSION = "2.0.0-beta.1";

        [DllImport("libCppWrapper", EntryPoint = "InitWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool InitWrapper(string appId, string serverUrl, bool enableEncrypt, int version, string publicKey, int mode, int databaseLimit, int dataExpression, string databasePath, string zoneOffset);

        [DllImport("libCppWrapper", EntryPoint = "UnInitWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UnInitWrapper();

        [DllImport("libCppWrapper", EntryPoint = "EnableLogWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void EnableLogWrapper(bool enable);

        [DllImport("libCppWrapper", EntryPoint = "EnableLogTypeWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void EnableLogTypeWrapper(int type);

        [DllImport("libCppWrapper", EntryPoint = "LoginWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void LoginWrapper(string loginId);

        [DllImport("libCppWrapper", EntryPoint = "GetAccountIdWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetAccountIdWrapper();

        [DllImport("libCppWrapper", EntryPoint = "LogoutWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void LogoutWrapper();

        [DllImport("libCppWrapper", EntryPoint = "IdentifyWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void IdentifyWrapper(string distinctId);

        [DllImport("libCppWrapper", EntryPoint = "SetSuperPropertyWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetSuperPropertyWrapper(string properties);

        [DllImport("libCppWrapper", EntryPoint = "GetSuperPropertiesWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetSuperPropertiesWrapper();

        [DllImport("libCppWrapper", EntryPoint = "UnsetSuperPropertiesWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UnsetSuperPropertiesWrapper(string propertyName);

        [DllImport("libCppWrapper", EntryPoint = "ClearSuperPropertyWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ClearSuperPropertyWrapper();

        [DllImport("libCppWrapper", EntryPoint = "TrackEventWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void TrackEventWrapper(string eventName);

        [DllImport("libCppWrapper", EntryPoint = "TrackWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void TrackWrapper(string eventName, string properties);

        [DllImport("libCppWrapper", EntryPoint = "TrackFirstEventWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void TrackFirstEventWrapper(string eventName, string properties, string extraId);

        [DllImport("libCppWrapper", EntryPoint = "TrackUpdatableEventWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void TrackUpdatableEventWrapper(string eventName, string properties, string extraId);

        [DllImport("libCppWrapper", EntryPoint = "TrackOverWritableEventWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void TrackOverWritableEventWrapper(string eventName, string properties, string extraId);

        [DllImport("libCppWrapper", EntryPoint = "UserSetWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UserSetWrapper(string properties);

        [DllImport("libCppWrapper", EntryPoint = "UserSetOnceWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UserSetOnceWrapper(string properties);

        [DllImport("libCppWrapper", EntryPoint = "UserAddWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UserAddWrapper(string properties);

        [DllImport("libCppWrapper", EntryPoint = "UserAppendWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UserAppendWrapper(string properties);

        [DllImport("libCppWrapper", EntryPoint = "UserUniqAppendWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UserUniqAppendWrapper(string properties);

        [DllImport("libCppWrapper", EntryPoint = "UserDeleteWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UserDeleteWrapper();

        [DllImport("libCppWrapper", EntryPoint = "UserUnsetWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UserUnsetWrapper(string propertyName);

        [DllImport("libCppWrapper", EntryPoint = "FlushWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void FlushWrapper();

        [DllImport("libCppWrapper", EntryPoint = "CalibrateTimeWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CalibrateTimeWrapper(long timestamp);

        [DllImport("libCppWrapper", EntryPoint = "TimeEventWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void TimeEventWrapper(string eventName);

        [DllImport("libCppWrapper", EntryPoint = "GetDistinctIdWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetDistinctIdWrapper();

        [DllImport("libCppWrapper", EntryPoint = "GetDeviceIdWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetDeviceIdWrapper();

        [DllImport("libCppWrapper", EntryPoint = "StagingFilePathWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr StagingFilePathWrapper();

        [DllImport("libCppWrapper", EntryPoint = "SetCustomLibInfoWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetCustomLibInfoWrapper(string libName, string libVersion);

        [DllImport("libCppWrapper", EntryPoint = "GetPresetPropertiesWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetPresetPropertiesWrapper();

        [DllImport("libCppWrapper", EntryPoint = "RegisterRecieveGameCallback", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RegisterRecieveGameCallback(IntPtr handlerPointer);

        [DllImport("libCppWrapper", EntryPoint = "SetDynamicPropertiesCallback", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetDynamicPropertiesCallback(IntPtr handlerPointer);

        [DllImport("libCppWrapper", EntryPoint = "SetDynamicSuperProperties", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetDynamicSuperProperties(string properties);

        [DllImport("libCppWrapper", EntryPoint = "DeleteCharWrapper", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeleteCharWrapper(IntPtr ptr);

        static readonly IsoDateTimeConverter _timeConverter = new IsoDateTimeConverter
        {
            DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff"
        };

        static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { _timeConverter },
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
        };

        /// <summary>
        /// Initialize the SDK. The track function is not available until this interface is invoked.
        /// </summary>
        /// <param name="appId">app id</param>
        /// <param name="serverUrl">server url</param>
        /// <returns></returns>
        public static bool Init(string appId, string serverUrl)
        {
            var config = new TDConfig
            {
                AppId = appId,
                ServerUrl = serverUrl
            };

            return Init(config);
        }

        /// <summary>
        /// Initialize the SDK. The track function is not available until this interface is invoked.
        /// </summary>
        /// <param name="config">TDConfig</param>
        /// <returns></returns>
        public static bool Init(TDConfig config)
        {
            SetCustomLibInfoWrapper(LIB_NAME, LIB_VERSION);
            config.Check();

            string zoneOffsetStr = string.Empty;
            if (config.ZoneOffset != null)
            {
                zoneOffsetStr = Convert.ToString(config.ZoneOffset);
            }
            return InitWrapper(config.AppId, config.ServerUrl, config.EnableEncrypt, config.Version, config.PublicKey, (int)config.Mode, config.DatabaseLimit, config.DataExpression, config.DatabasePath, zoneOffsetStr);
        }

        /// <summary>
        /// uninit the SDK.Call when not needed
        /// </summary>
        public static void UnInit()
        {
            UnInitWrapper();
        }

        /// <summary>
        /// enable debug logging
        /// </summary>
        /// <param name="enable">log switch</param>
        public static void EnableLog(bool enable)
        {
            EnableLogWrapper(enable);
        }

        /// <summary>
        /// enable logType, support txt log
        /// </summary>
        /// <param name="type"></param>
        public static void EnableLogType(TDLogType type)
        {
            TDLog.EnableLogType(type);
            EnableLogTypeWrapper((int)type);
        }

        /// <summary>
        /// Set the account ID. Each setting overrides the previous value. Login events will not be uploaded.
        /// </summary>
        public static void Login(string loginId)
        {
            if (string.IsNullOrWhiteSpace(loginId))
            {
                TDLog.Print(LogLevel.Error, TDErrorMsg.ParamIsNull);
                return;
            }
            LoginWrapper(loginId);
        }

        /// <summary>
        /// Obtain the account ID.
        /// </summary>
        /// <returns>account ID</returns>
        public static string GetAccountId()
        {
            IntPtr ptr = GetAccountIdWrapper();
            try
            {
                var accountId = Marshal.PtrToStringAnsi(ptr);
                return accountId ?? string.Empty;
            }
            finally
            {
                DeleteCharWrapper(ptr);
            }
        }

        /// <summary>
        /// Clearing the account ID will not upload user logout events.
        /// </summary>
        public static void Logout()
        {
            LogoutWrapper();
        }

        /// <summary>
        /// Set the distinct ID to replace the default UUID distinct ID.
        /// </summary>
        public static void SetDistinctId(string distinctId)
        {
            if (string.IsNullOrWhiteSpace(distinctId))
            {
                TDLog.Print(LogLevel.Error, TDErrorMsg.ParamIsNull);
                return;
            }
            IdentifyWrapper(distinctId);
        }

        /// <summary>
        /// Set the public event attribute, which will be included in every event uploaded after that. The public event properties are saved without setting them each time.
        /// </summary>
        /// <param name="properties">super properties</param>
        public static void SetSuperProperties(Dictionary<string, object> properties)
        {
            if (properties == null)
            {
                TDLog.Print(LogLevel.Error, TDErrorMsg.ParamIsNull);
                return;
            }
            SetSuperPropertyWrapper(JsonConvert.SerializeObject(properties, settings));
        }

        /// <summary>
        /// Gets the public event properties that have been set.
        /// </summary>
        /// <returns>public event properties that have been set</returns>
        public static Dictionary<string, object> GetSuperProperties()
        {
            IntPtr ptr = GetSuperPropertiesWrapper();
            try
            {
                var propertiesJson = Marshal.PtrToStringAnsi(ptr);
                if (!string.IsNullOrEmpty(propertiesJson))
                {
                    return JsonConvert.DeserializeObject<Dictionary<string, object>>(propertiesJson, settings);
                }

                return null;
            }
            finally
            {
                DeleteCharWrapper(ptr);
            }
        }

        public static void UnsetSuperProperties(string propertyName)
        {
            UnsetSuperPropertiesWrapper(propertyName);
        }

        /// <summary>
        /// Clear all public event attributes.
        /// </summary>
        public static void ClearSuperProperties()
        {
            ClearSuperPropertyWrapper();
        }

        /// <summary>
        /// Upload a single event, containing only preset properties and set public properties.
        /// </summary>
        /// <param name="eventName">event name</param>
        public static void Track(string eventName)
        {
            TrackEventWrapper(eventName);
        }

        /// <summary>
        /// Upload a single event, containing only preset properties and set public properties.
        /// </summary>
        /// <param name="eventName">event name</param>
        /// <param name="properties">event properties</param>
        public static void Track(string eventName, Dictionary<string, object> properties)
        {
            eventName = eventName ?? string.Empty;
            if (properties == null)
            {
                properties = new Dictionary<string, object>();
            }

            TrackWrapper(eventName, JsonConvert.SerializeObject(properties, settings));
        }

        /// <summary>
        /// Upload a special type of event.
        /// </summary>
        /// <param name="firstEvent">TDFirstEvent</param>
        public static void Track(TDFirstEvent firstEvent)
        {
            if (firstEvent != null)
            {
                firstEvent.Check();
                var properties = JsonConvert.SerializeObject(firstEvent.Properties, settings);
                TrackFirstEventWrapper(firstEvent.EventName, properties, firstEvent.ExtraId);
            }
            else
            {
                TDLog.Print(LogLevel.Error, TDErrorMsg.ParamIsNull);
            }
        }

        /// <summary>
        /// Upload a special type of event.
        /// </summary>
        /// <param name="updatableEvent">TDUpdatableEvent</param>
        public static void Track(TDUpdatableEvent updatableEvent)
        {
            if (updatableEvent != null)
            {
                updatableEvent.Check();
                var properties = JsonConvert.SerializeObject(updatableEvent.Properties, settings);
                TrackUpdatableEventWrapper(updatableEvent.EventName, properties, updatableEvent.ExtraId);
            }
            else
            {
                TDLog.Print(LogLevel.Error, TDErrorMsg.ParamIsNull);
            }
        }

        /// <summary>
        /// Upload a special type of event.
        /// </summary>
        /// <param name="overWritableEvent">TDOverWritableEvent</param>
        public static void Track(TDOverWritableEvent overWritableEvent)
        {
            if (overWritableEvent != null)
            {
                overWritableEvent.Check();
                var properties = JsonConvert.SerializeObject(overWritableEvent.Properties, settings);
                TrackOverWritableEventWrapper(overWritableEvent.EventName, properties, overWritableEvent.ExtraId);
            }
            else
            {
                TDLog.Print(LogLevel.Error, TDErrorMsg.ParamIsNull);
            }
        }

        /// <summary>
        /// Sets the user property, replacing the original value with the new value if the property already exists.
        /// </summary>
        /// <param name="properties">user property</param>
        public static void UserSet(Dictionary<string, object> properties)
        {
            if (properties == null)
            {
                TDLog.Print(LogLevel.Error, TDErrorMsg.ParamIsNull);
                return;
            }
            UserSetWrapper(JsonConvert.SerializeObject(properties, settings));
        }

        /// <summary>
        /// Sets a single user attribute, ignoring the new attribute value if the attribute already exists.
        /// </summary>
        /// <param name="properties">user property</param>
        public static void UserSetOnce(Dictionary<string, object> properties)
        {
            if (properties == null)
            {
                TDLog.Print(LogLevel.Error, TDErrorMsg.ParamIsNull);
                return;
            }
            UserSetOnceWrapper(JsonConvert.SerializeObject(properties, settings));
        }

        /// <summary>
        /// Only one attribute is set when the user attributes of a numeric type are added.
        /// </summary>
        /// <param name="properties">user property</param>
        public static void UserAdd(Dictionary<string, object> properties)
        {
            if (properties == null)
            {
                TDLog.Print(LogLevel.Error, TDErrorMsg.ParamIsNull);
                return;
            }
            UserAddWrapper(JsonConvert.SerializeObject(properties, settings));
        }

        /// <summary>
        /// Append a user attribute of the List type.
        /// </summary>
        /// <param name="properties">user property</param>
        public static void UserAppend(Dictionary<string, object> properties)
        {
            if (properties == null)
            {
                TDLog.Print(LogLevel.Error, TDErrorMsg.ParamIsNull);
                return;
            }
            UserAppendWrapper(JsonConvert.SerializeObject(properties, settings));
        }

        /// <summary>
        /// The element appended to the library needs to be done to remove the processing, remove the support, and then import.
        /// </summary>
        /// <param name="properties">user property</param>
        public static void UserUniqAppend(Dictionary<string, object> properties)
        {
            if (properties == null)
            {
                TDLog.Print(LogLevel.Error, TDErrorMsg.ParamIsNull);
                return;
            }
            UserUniqAppendWrapper(JsonConvert.SerializeObject(properties, settings));
        }

        /// <summary>
        /// Delete the user attributes, but retain the uploaded event data. This operation is not reversible and should be performed with caution.
        /// </summary>
        public static void UserDelete()
        {
            UserDeleteWrapper();
        }

        /// <summary>
        /// Reset user properties.
        /// </summary>
        /// <param name="property">user property</param>
        public static void UserUnset(string propertyName)
        {
            if (propertyName == null)
            {
                TDLog.Print(LogLevel.Error, TDErrorMsg.ParamIsNull);
                return;
            }
            UserUnsetWrapper(propertyName);
        }

        /// <summary>
        /// Empty the cache queue. When this function is called, the data in the current cache queue will attempt to be reported.
        /// </summary>
        public static void Flush()
        {
            FlushWrapper();
        }

        /// <summary>
        /// Calibrate Time.
        /// </summary>
        /// <param name="timestamp">timestamp</param>
        public static void CalibrateTime(long timestamp)
        {
            if (timestamp > 0)
            {
                CalibrateTimeWrapper(timestamp);
            }
        }

        /// <summary>
        /// Timing event, pass in the name of the event that needs to be timed, and the reported data will include # duration.
        /// </summary>
        /// <param name="eventName"></param>
        public static void TimeEvent(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                TDLog.Print(LogLevel.Error, TDErrorMsg.ParamIsNull);
                return;
            }
            TimeEventWrapper(eventName);
        }

        /// <summary>
        /// Get a visitor ID: The #distinct_id value in the reported data.
        /// </summary>
        /// <returns>distinct ID</returns>
        public static string GetDistinctId()
        {
            IntPtr ptr = GetDistinctIdWrapper();
            try
            {
                var distinctId = Marshal.PtrToStringAnsi(ptr);
                return distinctId ?? string.Empty;
            }
            finally
            {
                DeleteCharWrapper(ptr);
            }
        }

        /// <summary>
        /// Obtain the device ID.
        /// </summary>
        /// <returns>device ID</returns>
        public static string GetDeviceId()
        {
            IntPtr ptr = GetDeviceIdWrapper();
            try
            {
                var deviceId = Marshal.PtrToStringAnsi(ptr);
                return deviceId ?? string.Empty;
            }
            finally
            {
                DeleteCharWrapper(ptr);
            }
        }

        /// <summary>
        /// Obtain data storage path.
        /// </summary>
        /// <returns>data storage path</returns>
        public static string StagingFilePath()
        {
            IntPtr ptr = StagingFilePathWrapper();
            try
            {
                var stagingFilePath = Marshal.PtrToStringAnsi(ptr);
                return stagingFilePath ?? string.Empty;
            }
            finally
            {
                DeleteCharWrapper(ptr);
            }
        }

        /// <summary>
        /// Obtain preset properties.
        /// </summary>
        /// <returns>preset properties</returns>
        public static Dictionary<string, object> GetPresetProperties()
        {
            IntPtr ptr = GetPresetPropertiesWrapper();
            try
            {
                var propertiesJson = Marshal.PtrToStringAnsi(ptr);
                if (!string.IsNullOrEmpty(propertiesJson))
                {
                    return JsonConvert.DeserializeObject<Dictionary<string, object>>(propertiesJson, settings);
                }

                return null;
            }
            finally
            {
                DeleteCharWrapper(ptr);
            }
        }

        public static event TDGameCallback GameCallbackEvent;
        public delegate void TDGameCallback(int code, string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void TDGameCallbackWrapper(int code, IntPtr ptr);

        /// <summary>
        /// Register game callback
        /// </summary>
        /// <param name="callback">callback</param>
        public static void RegisterRecieveGameCallback(TDGameCallback callback)
        {
            GameCallbackEvent += callback;
            TDGameCallbackWrapper callbackWrapper = new TDGameCallbackWrapper(GameCallbackWrapper);
            IntPtr handlerPointer = Marshal.GetFunctionPointerForDelegate(callbackWrapper);
            RegisterRecieveGameCallback(handlerPointer);
        }

        static void GameCallbackWrapper(int code, IntPtr ptr)
        {
            try
            {
                var message = Marshal.PtrToStringAnsi(ptr);
                GameCallbackEvent.Invoke(code, message ?? string.Empty);
            }
            finally
            {
                DeleteCharWrapper(ptr);
            }
        }

        public delegate string TDPropertiesCallback();
        public delegate Dictionary<string, object> TDGetProperties();
        static TDGetProperties getPropertiesWrapper;

        /// <summary>
        /// Set dynamic super properties
        /// </summary>
        /// <param name="getProperties">method</param>
        public static void SetDynamicSuperProperties(TDGetProperties getProperties)
        {
            if (getProperties == null)
            {
                TDLog.Print(LogLevel.Error, TDErrorMsg.ParamIsNull);
                return;
            }
            getPropertiesWrapper = getProperties;
            TDPropertiesCallback callback = new TDPropertiesCallback(PropertiesCallbackWrapper);
            IntPtr handlerPointer = Marshal.GetFunctionPointerForDelegate(callback);
            SetDynamicPropertiesCallback(handlerPointer);
        }

        static string PropertiesCallbackWrapper()
        {
            if (getPropertiesWrapper != null)
            {
                var propertiesDic = getPropertiesWrapper();
                if (propertiesDic != null)
                {
                    return JsonConvert.SerializeObject(propertiesDic, settings);
                }
            }
            return string.Empty;
        }
    }
}
