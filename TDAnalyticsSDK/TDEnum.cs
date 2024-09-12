using System;
namespace ThinkingData.Analytics
{
    public enum TDMode
    {
        Normal,
        Debug,
        DebugOnly
    };

    public enum TDLogType
    {
        LogNone = 1,
        LogConsole = 2,
        LogTxt = 3
    };

    public enum LogLevel
    {
        Debug,
        Info,
        Error
    }
}

