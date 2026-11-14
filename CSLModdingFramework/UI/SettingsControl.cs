using ColossalFramework.UI;
using CSLModdingFramework.UI.Fonts;
using UnityEngine;

namespace CSLModdingFramework.UI;

public abstract class SettingsControl : UIControl, IAutoLayout {
    protected string _header;
    protected string _description;
    protected EdgeInsetsHorizontal _textHorizontalPadding;
    protected UIRenderData _headerRenderData;
    protected UIRenderData _descriptionRenderData;
    protected DynamicFont _font;
    protected float _headerScale = 1f;
    protected float _descriptionScale = 0.8f;
    protected Color32 _headerNormalColor = UIColorPalette.White;
    protected Color32 _headerDisabledColor = UIColorPalette.ClassicDarkPalette.HeaderTextDisabledColor;
    protected Color32 _descriptionNormalColor = UIColorPalette.ClassicDarkPalette.DescriptionTextColor;
    protected Color32 _descriptionDisabledColor = UIColorPalette.ClassicDarkPalette.DescriptionTextDisabledColor;
    protected int _autoLayoutSuspendCount;

    public bool IsPerformingAutoLayout { get; protected set; }
    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;
    public DynamicFont Font => _font ??= FontManager.OpenSansRegularFont;

    public virtual Color32 DescriptionNormalColor {
        get => _descriptionNormalColor;
        set {
            if (_descriptionNormalColor.EqualsColor(value)) return;

            _descriptionNormalColor = value;
            Invalidate();
        }
    }

    public virtual Color32 DescriptionDisabledColor {
        get => _descriptionDisabledColor;
        set {
            if (_descriptionDisabledColor.EqualsColor(value)) return;

            _descriptionDisabledColor = value;
            Invalidate();
        }
    }

    public virtual Color32 HeaderNormalColor {
        get => _headerNormalColor;
        set {
            if (_headerNormalColor.EqualsColor(value)) return;

            _headerNormalColor = value;
            Invalidate();
        }
    }

    public virtual Color32 HeaderDisabledColor {
        get => _headerDisabledColor;
        set {
            if (_headerDisabledColor.EqualsColor(value)) return;

            _headerDisabledColor = value;
            Invalidate();
        }
    }

    public virtual float DescriptionScale {
        get => _descriptionScale;
        set {
            if (Mathf.Approximately(_descriptionScale, value)) return;

            _descriptionScale = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float HeaderScale {
        get => _headerScale;
        set {
            if (Mathf.Approximately(_headerScale, value)) return;

            _headerScale = value;
            InvalidateAutoLayout();
        }
    }

    public virtual EdgeInsetsHorizontal TextHorizontalPadding {
        get => _textHorizontalPadding;
        set {
            if (_textHorizontalPadding.Equals(value)) return;

            _textHorizontalPadding = value;
            InvalidateAutoLayout();
        }
    }

    public virtual string Header {
        get => _header;
        set {
            if (value == null || value.Equals(_header)) return;

            _header = value;
            InvalidateAutoLayout();
        }
    }

    public virtual string Description {
        get => _description;
        set {
            if (value == null || value.Equals(_description)) return;

            _description = value;
            InvalidateAutoLayout();
        }
    }

    public abstract void PerformAutoLayout();

    public virtual void SuspendAutoLayout() => _autoLayoutSuspendCount++;

    public virtual void ResumeAutoLayout(bool performNow = true) {
        if (_autoLayoutSuspendCount == 0) return;

        _autoLayoutSuspendCount--;

        if (_autoLayoutSuspendCount == 0 && performNow) {
            PerformAutoLayout();
            Invalidate();
        }
    }

    public override void OnEnable() {
        base.OnEnable();

        UnityEngine.Font.textureRebuilt += OnUnityFontTextureRebuilt;
    }

    public override void OnDisable() {
        base.OnDisable();

        UnityEngine.Font.textureRebuilt -= OnUnityFontTextureRebuilt;
    }

    protected void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Invalidate();
    }

    private void OnUnityFontTextureRebuilt(Font font) => Invalidate();
}