using System;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace ThinkingData.Analytics
{
    public class TDLog
    {
        private static readonly object _logLock = new object();
        private const int MaxLogSize = 10 * 1024 * 1024;
        private static TDLogType logType = TDLogType.LogNone;

        /// <summary>
        /// print log
        /// </summary>
        /// <param name="level"></param>
        /// <param name="log"></param>
        public static void Print(LogLevel level, string log)
        {
            string logPrefix = "[ThinkingData] ";
            string detailLog;
            switch (level)
            {
                case LogLevel.Debug:
                    detailLog = $"{logPrefix}Debug: {log}";
                    break;
                case LogLevel.Info:
                    detailLog = $"{logPrefix}Info: {log}";
                    break;
                case LogLevel.Error:
                    detailLog = $"{logPrefix}Error: {log}";
                    break;
                default:
                    detailLog = $"{logPrefix}Unknown: {log}";
                    break;
            }
            Print(detailLog);
        }

        private static void Print(string log)
        {
            if (logType == TDLogType.LogConsole)
            {
                Console.WriteLine(log);
            }
            else if (logType == TDLogType.LogTxt)
            {
                lock (_logLock)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "ta_event_log.txt");
                    using (StreamWriter logFile = new StreamWriter(path, true))
                    {
                        long currentPosition = logFile.BaseStream.Position;
                        if (currentPosition < MaxLogSize)
                        {
                            logFile.WriteLine(log);
                        }
                    }
                }
            }
        }

        public static void EnableLogType(TDLogType type)
        {
            logType = type;
        }
    }
}