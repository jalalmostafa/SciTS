using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace BenchmarkTool
{
    public class LoggingEventSource : EventSource
    {
        public static LoggingEventSource Logger = new LoggingEventSource();

        public class Keywords
        {
            public const EventKeywords Database = (EventKeywords)1;
            public const EventKeywords DataIngestion = (EventKeywords)2;
            public const EventKeywords DataQuery = (EventKeywords)3;
            public const EventKeywords Diagnostic = (EventKeywords)4;
            public const EventKeywords CSV = (EventKeywords)5;
            public const EventKeywords General = (EventKeywords)6;

        }

        [Event(1, Message = "Exception: {0}", Level = EventLevel.Error, Keywords = Keywords.Diagnostic)]
        public void Failure(string message) { WriteEvent(1, message); }

        [Event(2, Message = "Database connection error: {0}", Level = EventLevel.Error, Keywords = Keywords.Database)]
        public void DBConnectionFailure(string message) { WriteEvent(2, message); }

        [Event(3, Message = "Data Ingestion Error: {0}", Level = EventLevel.Error, Keywords = Keywords.DataIngestion)]
        public void DBIngectionFailure(string message) { WriteEvent(3, message); }

        [Event(4, Message = "Data Query Error: {0}", Level = EventLevel.Error, Keywords = Keywords.DataQuery)]
        public void DBQueryFailure(string message) { WriteEvent(4, message); }

        [Event(5, Message = "Export to CSV Error: {0}", Level = EventLevel.Error, Keywords = Keywords.CSV)]
        public void ExportCSVFailure(string message) { WriteEvent(5, message); }

        [Event(6, Message = "{0}", Level = EventLevel.Warning, Keywords = Keywords.General)]
        public void LogWarning(string message) { WriteEvent(6, message); }
    }
}
