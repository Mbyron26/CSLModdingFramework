using System;
using ColossalFramework;
using CSLModdingFramework.UI.Animations;
using UnityEngine;

namespace CSLModdingFramework.UI;

public sealed class ScrollModel {
    private LiteFloatAnimation _animation;
    private bool _hasAnimation;

    public event Action<float> ScrollPositionChanged;
    public event Action<float> ContentSizeChanged;
    public event Action<float> ViewportSizeChanged;

    public float ContentSize { get; private set; }
    public float ViewportSize { get; private set; }
    public float ScrollPosition { get; private set; }

    public EasingType ScrollEasingType { get; set; } = EasingType.Linear;
    public float ScrollEasingTime { get; set; } = 1f;
    public float PageScrollAmount => ViewportSize;
    public float MaxScrollOffset => ContentSize - ViewportSize;

    public void ScrollToStart() => SetScrollPosition(0f);
    public void ScrollToEnd() => SetScrollPosition(MaxScrollOffset);
    public void ScrollImmediateTo(float position) => SetScrollPosition(position);
    public void ScrollImmediate(float delta) => SetScrollPosition(ScrollPosition + delta);
    public void ScrollToPercentage(float p) => SetScrollPosition(MaxScrollOffset * p);
    public void ScrollEaseToPercentage(float p) => ScrollEaseTo(MaxScrollOffset * Mathf.Clamp01(p));

    public void OnUpdate(float deltaTime) {
        if (!_hasAnimation) return;

        _animation.Update(deltaTime);
        SetScrollPosition(_animation.Value);

        if (!_animation.IsRunning)
            _hasAnimation = false;
    }

    public void ScrollEaseTo(float position) {
        var target = ClampScrollPosition(position);

        if (!_hasAnimation) {
            _animation.Start(ScrollPosition, target, ScrollEasingTime, ScrollEasingType);
            _hasAnimation = true;
        }
        else {
            _animation.UpdateTarget(target);
        }
    }

    public void ScrollEase(float delta) {
        var target = ClampScrollPosition(ScrollPosition + delta);

        if (!_hasAnimation) {
            _animation.Start(ScrollPosition, target, ScrollEasingTime, ScrollEasingType);
            _hasAnimation = true;
        }
        else {
            _animation.UpdateTarget(target);
        }
    }

    public void SetContentSize(float size) {
        size = Mathf.Max(0f, size);
        if (Mathf.Approximately(size, ContentSize)) return;

        ContentSize = size;
        ContentSizeChanged?.Invoke(size);
        SetScrollPosition(ScrollPosition);
    }

    public void SetViewportSize(float size) {
        size = Mathf.Max(0f, size);
        if (Mathf.Approximately(size, ViewportSize)) return;

        ViewportSize = size;
        ViewportSizeChanged?.Invoke(size);
        SetScrollPosition(ScrollPosition);
    }

    public void SetScrollPosition(float pos) {
        var newPos = ClampScrollPosition(pos);
        if (Mathf.Approximately(newPos, ScrollPosition)) return;

        ScrollPosition = newPos;
        ScrollPositionChanged?.Invoke(newPos);
    }

    private float ClampScrollPosition(float pos) {
        var max = MaxScrollOffset;
        if (max <= 0f)
            return 0f;

        if (pos < 0f)
            return 0f;

        if (pos > max)
            return max;

        return pos;
    }
}