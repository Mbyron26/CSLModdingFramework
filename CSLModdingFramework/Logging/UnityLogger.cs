using System;
using System.Diagnostics;
using System.IO;
using ColossalFramework.UI;
using CSLModdingFramework.UI.Dialogs;
using UnityEngine;

namespace CSLModdingFramework.Logging;

public class UnityLogger : LoggerBase {
    public UnityLogger(string name) : base(name, true) => LogPath = Path.Combine(Application.dataPath, "output_log.txt");

    public override void Log(LogLevel logLevel, string message = "", Exception exception = null) {
        if (Level == LogLevel.Disabled || logLevel < Level) return;

        var stack = string.Empty;

        if (exception != null) {
            stack = $"{exception.Message}\n{exception.StackTrace}";
        }
        else if (logLevel is LogLevel.Error or LogLevel.Fatal && CaptureStackOnError) {
            var trace = new StackTrace(1, true);
            stack = trace.ToString();
        }

        var finalMessage = Name + $" {GetTag(logLevel)} {message}{(stack.Length > 0 ? "\n" + stack : "")}";
        switch (logLevel) {
            case LogLevel.Verbose:
            case LogLevel.Debug:
            case LogLevel.Info:
                UnityEngine.Debug.Log(finalMessage);
                break;
            case LogLevel.Warn:
                UnityEngine.Debug.LogWarning(finalMessage);
                break;
            case LogLevel.Error:
            case LogLevel.Fatal:
                UnityEngine.Debug.LogError(finalMessage);
                break;
        }

        if (!ShowsErrorsInUI || UIView.GetAView() == null) return;

        if (logLevel is LogLevel.Error or LogLevel.Fatal)
            ExceptionDialog.ShowExceptionDialog(Name ?? string.Empty, message, stack);
        else if (logLevel is LogLevel.Warn)
            ExceptionDialog.ShowExceptionDialog(Name ?? string.Empty, message, stack, ExceptionDialogSeverity.Warning);
    }

    public override void Dispose() => UnityEngine.Debug.Log($"Disposed {Name}");
}