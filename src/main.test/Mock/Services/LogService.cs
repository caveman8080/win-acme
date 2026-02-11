using PKISharp.WACS.Services;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PKISharp.WACS.UnitTests.Mock.Services
{
    internal class LogService : ILogService
    {
        private readonly Logger _logger;
        private readonly bool _throwErrors;
        public ConcurrentQueue<string> DebugMessages { get; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> WarningMessages { get; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> InfoMessages { get; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> ErrorMessages { get; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> VerboseMessages { get; } = new ConcurrentQueue<string>();

        private static object?[] SanitizeItems(object?[] items)
        {
            if (items == null || items.Length == 0)
            {
                return items ?? Array.Empty<object?>();
            }

            var sanitized = new object?[items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] is string s)
                {
                    // Remove newlines to prevent log forging via user-controlled values
                    sanitized[i] = s
                        .Replace("\r", string.Empty)
                        .Replace("\n", string.Empty);
                }
                else
                {
                    sanitized[i] = items[i];
                }
            }

            return sanitized;
        }

        public LogService() : this(false) {}

        public LogService(bool throwErrors)
        {
            _throwErrors = throwErrors;
            _logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: " [{Level:u4}] {Message:l}{NewLine}{Exception}")
                .CreateLogger();
        }

        public bool Dirty { get; set; }

        public IEnumerable<MemoryEntry> Lines => new List<MemoryEntry>();

        public void Debug(string message, params object?[] items)
        {
            DebugMessages.Enqueue(message);
            _logger.Debug(message, SanitizeItems(items));
        }
        public void Error(Exception ex, string message, params object?[] items)
        {
            ErrorMessages.Enqueue(message);
            _logger.Error(ex, message, SanitizeItems(items));
            if (_throwErrors)
            {
                throw ex;
            }
        }
        public void Error(string message, params object?[] items)
        {
            ErrorMessages.Enqueue(message);
            _logger.Error(message, SanitizeItems(items));
            if (_throwErrors)
            {
                throw new Exception(message);
            }
        }

        public void Information(LogType logType, string message, params object?[] items)
        {
            InfoMessages.Enqueue(message);
            _logger.Information(message, SanitizeItems(items));
        }

        public void Information(string message, params object?[] items) => Information(LogType.All, message, items);

        public void SetVerbose() { }

        public void Verbose(string message, params object?[] items)
        {
            VerboseMessages.Enqueue(message);
            _logger.Verbose(message, SanitizeItems(items));
        }
        public void Verbose(LogType logType, string message, params object?[] items)
        {
            VerboseMessages.Enqueue(message);
            _logger.Verbose(message, SanitizeItems(items));
        }
        public void Warning(string message, params object?[] items)
        {
            WarningMessages.Enqueue(message);
            _logger.Warning(message, SanitizeItems(items));
        }

        public void Reset() { }

        public void SetDiskLoggingPath(string logPath) {}
    }
}
