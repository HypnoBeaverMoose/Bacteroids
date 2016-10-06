using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

[InitializeOnLoad]
public class LogHandler : ILogHandler
{
	public delegate void MessageLogged(LogInfo data);
    public static event MessageLogged OnMessageLogged;

	[Serializable]
    public class LogInfo
    {
        public LogInfo(LogType type, string format, object[] args, UnityEngine.Object context, DateTime time, string trace)
        {
            Type = type;
            Format = format;
            Args = args;
            Context = context;
            Time = time;
            Trace = trace;
        }

        public LogType Type;
        public string Format;
        public object[] Args;
        public UnityEngine.Object Context;
        public DateTime Time;
        public string Trace;
    }

    public static LogHandler Handler { get { return handler; } }
	public static ILogHandler DefaultHandler;
    private static LogHandler handler;
    static LogHandler()
    {
		DefaultHandler = Debug.logger.logHandler;
        handler = new LogHandler();
		//Debug.logger.logHandler = handler;
    }

    public bool SaveTrace = true;
    
    public LogHandler()
    { 
    }

    public void LogException(Exception exception, UnityEngine.Object context)
    {              
		if (OnMessageLogged != null) 
		{
			OnMessageLogged (new LogInfo (LogType.Exception, "", new object[] { exception.Message }, context, DateTime.Now, SaveTrace ? StackTraceUtility.ExtractStringFromException (exception) : ""));
		}
    }

    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
    {
		if (OnMessageLogged != null) 
		{
			OnMessageLogged (new LogInfo (logType, format, args, context, DateTime.Now, SaveTrace ? StackTraceUtility.ExtractStackTrace () : ""));
		}
    }
}
