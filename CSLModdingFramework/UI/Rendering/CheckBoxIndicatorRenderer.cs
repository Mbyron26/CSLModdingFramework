using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Rendering;

public sealed class CheckBoxIndicatorRenderer {
    private UIRenderData _backgroundRenderData;
    private UIRenderData _foregroundRenderData;
    private IAtlas _atlas;
    private bool _isEnabled = true;
    private string _backgroundSprite = string.Empty;
    private string _foregroundUncheckedSprite = string.Empty;
    private string _foregroundCheckedSprite = string.Empty;
    private string _foregroundIndeterminateSprite = string.Empty;
    private float _foregroundScale = 1f;
    private Vector2 _foregroundCustomSize = new(20, 20);

    public Checkbox Owner { get; }
    public ColorStateValue BackgroundUncheckedColors { get; }
    public ColorStateValue BackgroundCheckedColors { get; }
    public ColorStateValue BackgroundIndeterminateColors { get; }
    public ColorStateValue ForegroundUncheckedColors { get; }
    public ColorStateValue ForegroundCheckedColors { get; }
    public ColorStateValue ForegroundIndeterminateColors { get; }

    public Vector2 ForegroundCustomSize {
        get => _foregroundCustomSize;
        set {
            if (value == _foregroundCustomSize) return;

            _foregroundCustomSize = value;
            RequestRebuild();
        }
    }

    public float ForegroundScale {
        get => _foregroundScale;
        set {
            if (Mathf.Approximately(value, _foregroundScale)) return;

            _foregroundScale = value;
            RequestRebuild();
        }
    }

    public string ForegroundIndeterminateSprite {
        get => _foregroundIndeterminateSprite;
        set {
            if (string.Equals(value, _foregroundIndeterminateSprite)) return;

            _foregroundIndeterminateSprite = value;
            RequestRebuild();
        }
    }

    public string ForegroundCheckedSprite {
        get => _foregroundCheckedSprite;
        set {
            if (string.Equals(value, _foregroundCheckedSprite)) return;

            _foregroundCheckedSprite = value;
            RequestRebuild();
        }
    }

    public string ForegroundUncheckedSprite {
        get => _foregroundUncheckedSprite;
        set {
            if (string.Equals(value, _foregroundUncheckedSprite)) return;

            _foregroundUncheckedSprite = value;
            RequestRebuild();
        }
    }

    public string BackgroundSprite {
        get => _backgroundSprite;
        set {
            if (string.Equals(value, _backgroundSprite)) return;

            _backgroundSprite = value;
            RequestRebuild();
        }
    }

    public bool IsEnabled {
        get => _isEnabled;
        set {
            if (value.Equals(_isEnabled)) return;

            _isEnabled = value;
            RequestRebuild();
        }
    }

    public IAtlas Atlas {
        get => _atlas;
        set {
            if (Equals(value, _atlas)) return;
            _atlas = value;
            RequestRebuild();
        }
    }

    public CheckBoxIndicatorRenderer(Checkbox owner) {
        Owner = owner;
        BackgroundUncheckedColors = new ColorStateValue(owner);
        BackgroundCheckedColors = new ColorStateValue(owner);
        BackgroundIndeterminateColors = new ColorStateValue(owner);
        ForegroundUncheckedColors = new ColorStateValue(owner);
        ForegroundCheckedColors = new ColorStateValue(owner);
        ForegroundIndeterminateColors = new ColorStateValue(owner);
    }

    public void Render() {
        if (Owner.IsAutoLayoutSuspended || _atlas == null) return;

        if (_backgroundRenderData is not null) {
            _backgroundRenderData.Clear();
        }
        else {
            _backgroundRenderData = UIRenderData.Obtain();
            Owner.RenderData.Add(_backgroundRenderData);
        }

        if (_foregroundRenderData is not null) {
            _foregroundRenderData.Clear();
        }
        else {
            _foregroundRenderData = UIRenderData.Obtain();
            Owner.RenderData.Add(_foregroundRenderData);
        }
        
        if (!IsEnabled) return;

        _backgroundRenderData.material = _atlas.Material;
        _foregroundRenderData.material = _atlas.Material;

        var indicatorSize = Owner.IndicatorSize;
        var indicatorState = Owner.CheckState;
        var interactiveState = Owner.State;

        if (_atlas.TryGetSprite(_backgroundSprite, out int backgroundSpriteIndex)) {
            ref readonly var backgroundSpriteMetadata = ref _atlas.GetSprite(backgroundSpriteIndex);

            var backgroundColor = indicatorState switch {
                CheckState.Checked => BackgroundCheckedColors.GetValue(interactiveState),
                CheckState.Indeterminate => BackgroundIndeterminateColors.GetValue(interactiveState),
                _ => BackgroundUncheckedColors.GetValue(interactiveState),
            };

            SpriteRenderData backgroundSpriteRenderData = new() {
                Atlas = _atlas,
                Color = backgroundColor,
                FillAmount = 1f,
                Flip = UISpriteFlip.None,
                Offset = new Vector3(Owner.LayoutPadding.Left, Owner.LayoutPadding.Top, 0),
                UnitsPerPixel = Owner.UnitsPerPixel,
                Size = indicatorSize,
                SpriteMetadata = backgroundSpriteMetadata
            };

            if (backgroundSpriteMetadata.IsSliced) {
                SlicedSpriteMeshBuilder.RenderSprite(_backgroundRenderData, backgroundSpriteRenderData);
            }
            else {
                SpriteMeshBuilder.RenderSprite(_backgroundRenderData, backgroundSpriteRenderData);
            }
        }

        var foregroundSpriteName = indicatorState switch {
            CheckState.Checked => _foregroundCheckedSprite,
            CheckState.Indeterminate => _foregroundIndeterminateSprite,
            _ => _foregroundUncheckedSprite,
        };

        if (_atlas.TryGetSprite(foregroundSpriteName, out int spriteIndex)) {
            ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);

            var renderSize = _foregroundCustomSize * _foregroundScale;
            var offset = new Vector3(Owner.LayoutPadding.Left + (indicatorSize.x - renderSize.x) * 0.5f, Owner.LayoutPadding.Top + (indicatorSize.y - renderSize.y) * 0.5f, 0);

            var color = indicatorState switch {
                CheckState.Checked => ForegroundCheckedColors.GetValue(interactiveState),
                CheckState.Indeterminate => ForegroundIndeterminateColors.GetValue(interactiveState),
                _ => ForegroundUncheckedColors.GetValue(interactiveState),
            };

            SpriteRenderData spriteRenderData = new() {
                Atlas = _atlas,
                Color = color,
                FillAmount = 1f,
                Flip = UISpriteFlip.None,
                Offset = offset,
                UnitsPerPixel = Owner.UnitsPerPixel,
                Size = renderSize,
                SpriteMetadata = spriteMetadata
            };

            if (spriteMetadata.IsSliced) {
                SlicedSpriteMeshBuilder.RenderSprite(_foregroundRenderData, spriteRenderData);
            }
            else {
                SpriteMeshBuilder.RenderSprite(_foregroundRenderData, spriteRenderData);
            }
        }
    }

    private void RequestRebuild() => Owner?.Invalidate();
}