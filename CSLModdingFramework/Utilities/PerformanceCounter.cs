using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CSLModdingFramework.Collections;

namespace CSLModdingFramework.Utilities;

public class PerformanceCounter : IDisposable {
    private readonly Stopwatch _stopwatch = new();
    private readonly Action<PerformanceCounter> _callback;

    private TimeSpan _lastSegmentTime;
    private readonly List<Segment> _segments;

    public TimeSpan Elapsed => _stopwatch.Elapsed;
    public string Seconds => $"{Elapsed.TotalSeconds:F3}s";
    public string Milliseconds => $"{Elapsed.TotalMilliseconds:F3}ms";
    public string Microseconds => $"{Elapsed.Ticks * 0.1:F1}µs";
    public string Nanoseconds => $"{Elapsed.Ticks * 100}ns";

    public IReadOnlyList<Segment> Segments { get; }

    public PerformanceCounter(Action<PerformanceCounter> callback = null) {
        _segments = new List<Segment>();
        Segments = new ReadOnlyListWrapper<Segment>(_segments);
        _callback = callback;
        _stopwatch.Start();
        _lastSegmentTime = TimeSpan.Zero;
    }

    public static PerformanceCounter Start(Action<PerformanceCounter> callback = null)
        => new(callback);

    public void Pause() => _stopwatch.Stop();
    public void Resume() => _stopwatch.Start();

    public void Lap(string name = null) {
        var now = _stopwatch.Elapsed;
        var duration = now - _lastSegmentTime;
        _segments.Add(new Segment(name, duration, now));
        _lastSegmentTime = now;
    }

    public void Dispose() {
        _stopwatch.Stop();
        _callback?.Invoke(this);
    }

    public string Summary(TimeUnit unit = TimeUnit.Milliseconds) {
        var sb = new StringBuilder();
        sb.AppendLine("Performance Report:");
        sb.AppendLine($"Total: {FormatTime(_stopwatch.Elapsed, unit)}");

        if (_segments.Count == 0)
            return sb.ToString();

        var nameWidth = _segments.Max(s => string.IsNullOrEmpty(s.Name) ? 0 : s.Name.Length);
        var durWidth = _segments.Max(s => FormatTime(s.Duration, unit).Length);

        foreach (var seg in _segments) {
            var name = string.IsNullOrEmpty(seg.Name) ? "" : seg.Name;
            var dur = FormatTime(seg.Duration, unit);
            var tot = FormatTime(seg.TotalElapsed, unit);

            sb.Append(name.PadRight(nameWidth));
            if (nameWidth > 0) sb.Append("  ");

            sb.Append(dur.PadRight(durWidth));
            sb.Append("   ");

            sb.Append("(Total: ");
            sb.Append(tot);
            sb.AppendLine(")");
        }

        return sb.ToString();
    }


    private string FormatTime(TimeSpan ts, TimeUnit unit) => unit switch {
        TimeUnit.Seconds => $"{ts.TotalSeconds:F6}s",
        TimeUnit.Milliseconds => $"{ts.TotalMilliseconds:F3}ms",
        TimeUnit.Microseconds => $"{ts.Ticks * 0.1:F1}µs",
        TimeUnit.Nanoseconds => $"{ts.Ticks * 100}ns",
        _ => $"{ts.TotalMilliseconds:F3}ms",
    };

    public class Segment {
        public string Name { get; }
        public TimeSpan Duration { get; }
        public TimeSpan TotalElapsed { get; }

        public string DurationSeconds => $"{Duration.TotalSeconds:F6}s";
        public string DurationMilliseconds => $"{Duration.TotalMilliseconds:F3}ms";
        public string DurationMicroseconds => $"{Duration.Ticks * 0.1:F1}µs";
        public string DurationNanoseconds => $"{Duration.Ticks * 100}ns";

        public string TotalElapsedSeconds => $"{TotalElapsed.TotalSeconds:F6}s";
        public string TotalElapsedMilliseconds => $"{TotalElapsed.TotalMilliseconds:F3}ms";
        public string TotalElapsedMicroseconds => $"{TotalElapsed.Ticks * 0.1:F1}µs";
        public string TotalElapsedNanoseconds => $"{TotalElapsed.Ticks * 100}ns";

        public Segment(string name, TimeSpan duration, TimeSpan totalElapsed) {
            Name = name;
            Duration = duration;
            TotalElapsed = totalElapsed;
        }

        public override string ToString() => ToString();

        public string ToString(TimeUnit unit = TimeUnit.Milliseconds) {
            var dur = FormatTime(Duration, unit);
            var tot = FormatTime(TotalElapsed, unit);
            return string.IsNullOrEmpty(Name) ? $"Duration: {dur}, Total: {tot}" : $"{Name}: {dur} (Total: {tot})";
        }

        private string FormatTime(TimeSpan ts, TimeUnit unit) => unit switch {
            TimeUnit.Seconds => $"{ts.TotalSeconds:F6}s",
            TimeUnit.Milliseconds => $"{ts.TotalMilliseconds:F3}ms",
            TimeUnit.Microseconds => $"{ts.Ticks * 0.1:F1}µs",
            TimeUnit.Nanoseconds => $"{ts.Ticks * 100}ns",
            _ => $"{ts.TotalMilliseconds:F3}ms",
        };
    }
}