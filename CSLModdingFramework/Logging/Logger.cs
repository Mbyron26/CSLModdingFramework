using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ColossalFramework.UI;
using CSLModdingFramework.UI.Dialogs;

namespace CSLModdingFramework.Logging;

public class Logger : LoggerBase {
    public const string LogFileExtension = ".log";
    public static readonly LogLevel[] LogLevelValues = (LogLevel[])Enum.GetValues(typeof(LogLevel));

    private readonly object _fileLock = new();
    private readonly StreamWriter _writer;

    public Logger(string name, string logDirectory) : base(name, false) {
        LogPath = Path.Combine(logDirectory, Name + LogFileExtension);

        if (File.Exists(LogPath))
            File.WriteAllText(LogPath, string.Empty);

        _writer = new StreamWriter(LogPath, true, Encoding.UTF8);
        _writer.WriteLine($"{GetTag(LogLevel.Info)} {GetEnvironmentInfo()}");
        _writer.Flush();
    }

    public override void Log(LogLevel logLevel, string message = "", Exception exception = null) {
        if (Level == LogLevel.Disabled || logLevel < Level) return;

        lock (_fileLock) {
            var stack = string.Empty;

            if (exception != null) {
                stack = $"{exception.Message}\n{exception.StackTrace}";
            }
            else if (logLevel is LogLevel.Error or LogLevel.Fatal && CaptureStackOnError) {
                var trace = new StackTrace(1, true);
                stack = trace.ToString();
            }

            _writer.WriteLine($"{GetTag(logLevel)} {message}{(stack.Length > 0 ? "\n" + stack : "")}");
            _writer.Flush();

            if (!ShowsErrorsInUI || UIView.GetAView() == null) return;

            if (logLevel is LogLevel.Error or LogLevel.Fatal)
                ExceptionDialog.ShowExceptionDialog(Name ?? string.Empty, message, stack);
            else if (logLevel is LogLevel.Warn)
                ExceptionDialog.ShowExceptionDialog(Name ?? string.Empty, message, stack, ExceptionDialogSeverity.Warning);
        }
    }

    public override void Dispose() => _writer?.Dispose();
}