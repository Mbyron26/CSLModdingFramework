using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class RgbColorField : UIInteractiveControl, IThemeable {
    protected IAtlas _atlas;
    protected float _foregroundScale = 0.75f;
    protected Vector2 _foregroundSize = new(32, 32);
    protected string _foregroundSpriteName;
    protected Color _selectedColor = Color.white;
    protected ColorPickerPosition _pickerVerticalPosition = ColorPickerPosition.RightAbove;
    protected RgbColorPicker _rgbColorPickerPopup;
    protected ThemeKind _theme;

    public SpriteStateValue BackgroundSprites { get; private set; }
    public ColorStateValue BackgroundColors { get; private set; }

    public event UIEventHandler<RgbColorField, Color> SelectedColorChanged;
    public event UIEventHandler<RgbColorPicker> RgbColorPickerPopupCreated;


    public virtual ThemeKind Theme {
        get => _theme;
        set {
            if (value == _theme) return;

            _theme = value;
            Atlas = DefaultAtlas.Instance;
            BackgroundSprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);
            ForegroundSpriteName = DefaultAtlas.Rectangle;
            if (_theme == ThemeKind.ClassicLight) {
                BackgroundColors.SetValues(UIColorPalette.ClassicLightPalette.SettingItemControlColors);
            }
            else {
                BackgroundColors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlColors);
            }
        }
    }

    public IAtlas Atlas {
        get => _atlas;
        set {
            if (Equals(value, _atlas)) return;

            _atlas = value;
            Invalidate();
        }
    }

    public Vector2 ForegroundSize {
        get => _foregroundSize;
        set {
            if (value == _foregroundSize) return;
            _foregroundSize = value;
            Invalidate();
        }
    }

    public float ForegroundScale {
        get => _foregroundScale;
        set {
            if (Mathf.Approximately(value, _foregroundScale)) return;
            _foregroundScale = value;
            Invalidate();
        }
    }

    public string ForegroundSpriteName {
        get => _foregroundSpriteName;
        set {
            if (string.Equals(value, _foregroundSpriteName)) return;
            _foregroundSpriteName = value;
            Invalidate();
        }
    }

    public Color SelectedColor {
        get => _selectedColor;
        set => SetSelectedColor(value);
    }

    public ColorPickerPosition PickerPosition {
        get => _pickerVerticalPosition;
        set {
            if (value == _pickerVerticalPosition) return;

            _pickerVerticalPosition = value;
            if (_rgbColorPickerPopup != null)
                SetPopupPosition();
            Invalidate();
        }
    }

    public virtual bool SetSelectedColorWithoutNotify(Color value) => SetSelectedColor(value, false);

    public virtual bool SetSelectedColor(Color value, bool notify = true) {
        if (_selectedColor == value) return false;

        _selectedColor = value;
        if (notify)
            OnSelectedColorChanged();

        Invalidate();
        return true;
    }

    protected virtual void RenderBackground() {
        if (!_atlas.TryGetSprite(BackgroundSprites.GetValue(_state), out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);
        SpriteRenderData spriteRenderData = new() {
            Atlas = _atlas,
            Color = BackgroundColors.GetValue(_state),
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = Vector3.zero,
            UnitsPerPixel = UnitsPerPixel,
            Size = m_Size,
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
    }

    protected virtual void OnSelectedColorChanged() => SelectedColorChanged?.Invoke(this, SelectedColor);

    protected virtual void RenderForeground() {
        if (!_atlas.TryGetSprite(_foregroundSpriteName, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);
        var renderSize = Vector2.Min(_foregroundSize * _foregroundScale, m_Size);
        var offset = pivot.TransformToUpperLeft(m_Size, arbitraryPivotOffset) + (Vector3)LayoutUtility.CalculateAlignedOffset(UIHorizontalAlignment.Center, UIVerticalAlignment.Middle, m_Size, renderSize);

        SpriteRenderData spriteRenderData = new() {
            Atlas = _atlas,
            Color = _selectedColor,
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = offset,
            UnitsPerPixel = UnitsPerPixel,
            Size = renderSize,
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
    }

    public override void OnDisable() {
        base.OnDisable();

        HidePopup();
    }

    public override void OnDestroy() {
        base.OnDestroy();

        DestroyPopup();
    }

    public override void Update() {
        base.Update();

        CheckForPopupClose();
    }

    public override void Awake() {
        base.Awake();

        size = new Vector2(32, 32);
        BackgroundSprites = new SpriteStateValue(this);
        BackgroundColors = new ColorStateValue(this);
    }

    protected override void OnMouseDown(UIMouseEventParameter p) {
        if (p.buttons == UIMouseButton.Left) {
            if (_rgbColorPickerPopup == null) {
                OpenPopup();
            }
            else {
                if (_rgbColorPickerPopup.isVisible) {
                    HidePopup();
                }
                else {
                    OpenPopup();
                }
            }

            p.Use();
        }

        base.OnMouseDown(p);
    }

    protected override void OnRebuildRenderData() {
        if (_atlas is null) return;

        renderData.material = _atlas.Material;
        RenderBackground();
        RenderForeground();
    }

    public void HidePopup() {
        var popup = GetPopup();
        popup.Unfocus();
        popup.isVisible = false;
    }

    public void OpenPopup() {
        var popup = GetPopup();
        popup.isVisible = true;
        popup.Focus();
        SetPopupPosition();
    }

    public RgbColorPicker GetPopup() {
        if (_rgbColorPickerPopup != null) return _rgbColorPickerPopup;

        var rootContainer = GetRootContainer();
        _rgbColorPickerPopup = rootContainer.AddUIComponent<RgbColorPicker>();
        _rgbColorPickerPopup.RgbColor = SelectedColor;
        _rgbColorPickerPopup.ColorChanged += PopupColorChanged;
        _rgbColorPickerPopup.eventKeyDown += OnPopupKeyDown;
        RgbColorPickerPopupCreated?.Invoke(_rgbColorPickerPopup);
        return _rgbColorPickerPopup;
    }

    private void DestroyPopup() {
        if (_rgbColorPickerPopup == null) return;

        _rgbColorPickerPopup.ColorChanged -= PopupColorChanged;
        _rgbColorPickerPopup.eventKeyDown -= OnPopupKeyDown;
        _rgbColorPickerPopup.DestroyChildrenAndSelf();
        _rgbColorPickerPopup = null;
    }

    private void SetPopupPosition() {
        var unitsPerPixel = UnitsPerPixel;
        var vector = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
        var vector2 = transform.position + vector * unitsPerPixel;
        var scaledDirection = GetScaledDirection(Vector3.down);
        var scaledDirection2 = GetScaledDirection(Vector3.right);
        var vector3 = Vector3.zero;
        if (PickerPosition == ColorPickerPosition.RightAbove) {
            vector3 = vector2 - scaledDirection * (_rgbColorPickerPopup.height * unitsPerPixel);
        }
        else if (PickerPosition == ColorPickerPosition.LeftAbove) {
            vector3 = vector2 - scaledDirection * (_rgbColorPickerPopup.height * unitsPerPixel) - scaledDirection2 * ((_rgbColorPickerPopup.size.x - size.x) * unitsPerPixel);
        }
        else if (PickerPosition == ColorPickerPosition.RightBelow) {
            vector3 = vector2 + scaledDirection * (height * unitsPerPixel);
        }
        else if (PickerPosition == ColorPickerPosition.LeftBelow) {
            vector3 = vector2 + scaledDirection * (height * unitsPerPixel) - scaledDirection2 * ((_rgbColorPickerPopup.width - width) * unitsPerPixel);
        }

        var vector4 = _rgbColorPickerPopup.transform.parent.position / unitsPerPixel + _rgbColorPickerPopup.parent.pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
        var vector5 = vector4 + scaledDirection * _rgbColorPickerPopup.parent.size.y + scaledDirection2 * _rgbColorPickerPopup.parent.size.x;
        var vector6 = vector3 / unitsPerPixel;
        var vector7 = vector6 + scaledDirection * _rgbColorPickerPopup.size.y + scaledDirection2 * _rgbColorPickerPopup.size.x;
        if (vector6.x < vector4.x) {
            vector3.x = vector2.x;
        }
        else if (vector7.x > vector5.x) {
            vector3.x = vector2.x - (_rgbColorPickerPopup.size.x - size.x) * unitsPerPixel;
        }

        if (vector6.y > vector4.y) {
            vector3.y = vector2.y - size.y * unitsPerPixel;
        }
        else if (vector7.y < vector5.y) {
            vector3.y = vector2.y + _rgbColorPickerPopup.size.y * unitsPerPixel;
        }

        _rgbColorPickerPopup.transform.position = vector3;
    }

    private void CheckForPopupClose() {
        if (_rgbColorPickerPopup == null || !_rgbColorPickerPopup.isVisible || !Input.GetMouseButtonDown(0)) return;

        var ray = GetCamera().ScreenPointToRay(Input.mousePosition);
        if (_rgbColorPickerPopup.Raycast(ray) || Raycast(ray)) return;

        HidePopup();
    }

    private void PopupColorChanged(RgbColorPicker sender, Color value) => SelectedColor = value;

    private void OnPopupKeyDown(UIComponent comp, UIKeyEventParameter p) {
        if (p.keycode == KeyCode.Escape) {
            HidePopup();
            p.Use();
        }
    }
}