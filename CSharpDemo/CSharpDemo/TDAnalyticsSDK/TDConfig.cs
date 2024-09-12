using System;
namespace ThinkingData.Analytics
{
    public class TDConfig
    {
        /// <summary>
        /// app id
        /// </summary>
        public string AppId = string.Empty;

        /// <summary>
        /// server url
        /// </summary>
        public string ServerUrl = string.Empty;

        /// <summary>
        /// enable encrypt
        /// </summary>
        public bool EnableEncrypt = false;

        /// <summary>
        /// encrypt version
        /// </summary>
        public int Version;

        /// <summary>
        /// encrypt publickey
        /// </summary>
        public string PublicKey = string.Empty;

        /// <summary>
        /// SDK mode
        /// </summary>
        public TDMode Mode = TDMode.Normal;

        /// <summary>
        /// database limit
        /// </summary>
        public int DatabaseLimit = 0;

        /// <summary>
        /// data expression
        /// </summary>
        public int DataExpression = 0;

        /// <summary>
        /// custom database path
        /// </summary>
        public string DatabasePath = string.Empty;

        /// <summary>
        /// custom zone offset
        /// </summary>
        public object ZoneOffset;

        public void Check()
        {
            AppId = AppId ?? string.Empty;
            ServerUrl = ServerUrl ?? string.Empty;
            PublicKey = PublicKey ?? string.Empty;
            DatabasePath = DatabasePath ?? string.Empty;
        }
    }
}