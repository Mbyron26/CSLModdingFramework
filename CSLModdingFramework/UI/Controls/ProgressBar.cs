using ColossalFramework;
using ColossalFramework.UI;
using CSLModdingFramework.Extensions;
using CSLModdingFramework.UI.Animations;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class ProgressBar : UIInteractiveControl {
    protected float _value = 0.25f;
    protected float _minValue;
    protected float _maxValue = 1f;
    protected UIFillMode _fillMode = UIFillMode.Fill;
    protected EdgeInsets _padding;
    protected Color32 _fillNormalColor = new(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
    protected Color32 _fillDisabledColor = UIColorPalette.White60Percent;
    protected string _fillSprite;
    protected IAtlas _fillAtlas;
    protected UIRenderData _fillRenderData;
    private LiteFloatAnimation _anim;
    private BackgroundRenderer _backgroundRenderer;

    public event UIEventHandler<ProgressBar, float> ValueChanged;

    public BackgroundRenderer BackgroundRenderer => _backgroundRenderer ??= new BackgroundRenderer(this);

    public EasingType ProgressEasingType { get; set; } = EasingType.Linear;
    public float EasingDuration { get; set; } = 0.5f;

    public IAtlas FillAtlas {
        get => _fillAtlas;
        set {
            if (Equals(value, _fillAtlas)) return;

            _fillAtlas = value;
            Invalidate();
        }
    }

    public string FillSprite {
        get => _fillSprite;
        set {
            if (value == _fillSprite) return;

            _fillSprite = value;
            Invalidate();
        }
    }

    public Color32 FillDisabledColor {
        get => _fillDisabledColor;
        set {
            if (value.EqualsColor(_fillDisabledColor)) return;

            _fillDisabledColor = value;
            Invalidate();
        }
    }

    public Color32 FillNormalColor {
        get => _fillNormalColor;
        set {
            if (value.EqualsColor(_fillNormalColor)) return;

            _fillNormalColor = value;
            Invalidate();
        }
    }

    public float Value {
        get => _value;
        set => SetValue(value);
    }

    public float MinValue {
        get => _minValue;
        set {
            if (Mathf.Approximately(value, _minValue)) return;

            _minValue = value;
            if (_value < value) {
                Value = value;
            }

            Invalidate();
        }
    }

    public float MaxValue {
        get => _maxValue;
        set {
            if (Mathf.Approximately(value, _maxValue)) return;

            _maxValue = value;
            if (_value > value) {
                Value = value;
            }

            Invalidate();
        }
    }

    public UIFillMode FillMode {
        get => _fillMode;
        set {
            if (value == _fillMode) return;

            _fillMode = value;
            Invalidate();
        }
    }

    public EdgeInsets Padding {
        get => _padding;
        set {
            if (value.Equals(_padding)) return;

            _padding = value;
            Invalidate();
        }
    }

    public bool SetValue(float value, bool notify = true) {
        var newValue = ClampToRange(value);
        if (Mathf.Approximately(_value, newValue)) return false;
        _value = newValue;

        if (notify)
            OnValueChanged();
        Invalidate();
        return true;
    }

    protected virtual void OnValueChanged() => ValueChanged?.Invoke(this, Value);

    protected virtual void RenderProgressFill() {
        if (_fillAtlas == null) return;

        if (_fillRenderData is not null) {
            _fillRenderData.Clear();
        }
        else {
            _fillRenderData = UIRenderData.Obtain();
            m_RenderData.Add(_fillRenderData);
        }

        if (string.IsNullOrEmpty(_fillSprite)) return;

        _fillRenderData.material = _fillAtlas.Material;

        if (!_fillAtlas.TryGetSprite(_fillSprite, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _fillAtlas.GetSprite(spriteIndex);

        var contentOffset = new Vector3(Padding.Left, -Padding.Top);
        var contentSize = new Vector2(
            size.x - Padding.Horizontal,
            size.y - Padding.Vertical
        );

        var range = MaxValue - MinValue;
        var normalizedValue = range > 0f ? (_value - MinValue) / range : 0f;
        normalizedValue = Mathf.Clamp01(normalizedValue);

        var fillAmount = 1f;
        var renderSize = contentSize;

        switch (FillMode) {
            case UIFillMode.Fill:
                fillAmount = normalizedValue;
                break;

            case UIFillMode.Stretch:
            default:
                var minWidth = spriteMetadata.Region.width;
                var targetWidth = contentSize.x * normalizedValue;

                renderSize.x = Mathf.Max(minWidth, targetWidth);
                break;
        }

        SpriteRenderData spriteRenderData = new() {
            Atlas = FillAtlas,
            SpriteMetadata = spriteMetadata,
            Color = isEnabled ? FillNormalColor : FillDisabledColor,
            Offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset) + contentOffset,
            Size = renderSize,
            FillAmount = fillAmount,
            Flip = UISpriteFlip.None,
            UnitsPerPixel = PixelsToUnits()
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(_fillRenderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(_fillRenderData, spriteRenderData);
    }

    public override void Update() {
        base.Update();

        if (!_anim.IsRunning) return;
        _anim.Update(Time.deltaTime);
        SetValueWithoutNotify(_anim.Value);
    }

    protected override void OnRebuildRenderData() {
        _backgroundRenderer?.Render();
        RenderProgressFill();
    }

    public bool SetValueWithoutNotify(float value) => SetValue(value, false);

    public void AnimateAddPercent(float percent) {
        var range = MaxValue - MinValue;
        var delta = range * percent;
        AnimateAdd(delta);
    }

    public void AnimateAdd(float delta) => AnimateTo(Value + delta);

    public void AnimateTo(float target) {
        target = Mathf.Clamp(target, MinValue, MaxValue);

        if (_anim.IsRunning)
            _anim.StartTo(target, EasingDuration, ProgressEasingType);
        else
            _anim.Start(Value, target, EasingDuration, ProgressEasingType);
    }

    protected float ClampToRange(float value) => Mathf.Max(MinValue, Mathf.Min(MaxValue, value));
}