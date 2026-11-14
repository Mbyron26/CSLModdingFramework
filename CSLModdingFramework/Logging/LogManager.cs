using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CSLModdingFramework.Logging;

public static class LogManager {
    private static readonly Dictionary<string, ILog> Loggers = new();

    public static string LogDirectory { get; } = Path.Combine(Application.dataPath, "Logs");

    static LogManager() {
        if (!Directory.Exists(LogDirectory))
            Directory.CreateDirectory(LogDirectory);
    }

    public static ILog GetLogger(string name, bool unityLogger = false) {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Logger name cannot be null or empty.", nameof(name));

        if (Loggers.TryGetValue(name, out var log)) return log;
        ILog logger;
        if (unityLogger) {
            logger = new UnityLogger(name);
        }
        else {
            logger = new Logger(name, LogDirectory);
        }

        Loggers.Add(name, logger);
        return logger;
    }

    public static ILog GetLogger() => GetLogger(nameof(CSLModdingFramework), true);

    public static void RemoveLogger(string name) {
        if (!Loggers.TryGetValue(name, out var log)) return;

        log.Dispose();
        Loggers.Remove(name);
    }

    public static IEnumerable<ILog> GetAllLoggers() => Loggers.Values;

    public static void ClearAllLoggers() {
        foreach (var logger in Loggers.Values)
            logger.Dispose();

        Loggers.Clear();
    }
}