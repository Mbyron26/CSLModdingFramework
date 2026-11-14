using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using CSLModdingFramework.Core;
using CSLModdingFramework.Localization;
using CSLModdingFramework.UI.Controls;
using CSLModdingFramework.UI.Fonts;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Dialogs;

public class ChangelogDialog : Dialog {
    public override Vector2 DefaultSize => new(800, 200);
    public override ScrollViewer.ScrollSizePolicy ScrollContainerSizeMode => ScrollViewer.ScrollSizePolicy.Fixed;

    protected override void OnAwake() {
        base.OnAwake();

        _scrollViewer.SetHeightIfChanged(500);
        _contentContainer.RowGap = 16;
    }

    private void FillChangelog(ModDomain domain) {
        var modSystem = domain?.ModSystem;

        var changelog = modSystem?.Changelog;
        if (changelog is null) return;

        var localizationSystem = modSystem.ModLocalizationSystem;
        if (localizationSystem is null) return;

        Title = modSystem.ModName + " " + modSystem.ModLocalizationSystem.GetString(LocalizationStrings.Changelog);
        ChangelogList firstChangelogList = null;
        for (var i = 0; i < changelog.Versions.Count; i++) {
            var changelogVersion = changelog.Versions[i];
            var changelogList = _contentContainer.AddUIComponent<ChangelogList>();
            changelogList.width = _contentContainer.width;
            changelogList.SuspendAutoLayout();
            var version = changelogVersion.Version;
            string text;
            if (version.Revision != -1 && version.Build != -1)
                text = version.ToString(4);
            else if (version.Revision == -1 && version.Build != -1)
                text = version.ToString(3);
            else
                text = version.ToString(3);
            changelogList.VersionText = text;
            changelogList.DateText = changelogVersion.Date.ToString("yyyy/MM/dd");
            foreach (var entry in changelogVersion.Entries) {
                var kind = localizationSystem.GetString("LogMessageBox_" + entry.Kind);
                var description = localizationSystem.GetString(entry.Description);
                changelogList.AddItem(kind, description, entry.Kind);
            }

            changelogList.ResumeAutoLayout();
            if (firstChangelogList == null) {
                firstChangelogList = changelogList;
                firstChangelogList.IsMinimize = false;
            }
        }

        var button = AddButton(domain.ModSystem.ModLocalizationSystem.GetString(LocalizationStrings.OK));
        button.Clicked += () => Close(this);
        InvalidateAutoLayout();
        CenterToParent();
    }

    public static ChangelogDialog ShowChangelogDialog(ModDomain domain) => Show<ChangelogDialog>(v => v.FillChangelog(domain));

    public sealed class ChangelogList : UIInteractiveControl, IAutoLayout {
        private const int MinimumHeight = 50;
        private IAtlas _atlas = DefaultAtlas.Instance;
        private EdgeInsetsHorizontal _horizontalPadding = new(16);
        private int _autoLayoutSuspendCount;
        private DynamicFont _font;
        private bool _isMinimize = true;
        private List<ChangelogItem> _items = new();
        private float _textScale = 1.1f;
        private Vector3 _versionTextRenderPosition;
        private int _arrowSize = 16;
        private int _dateArrowGap = 10;
        private UIRenderData _textRenderData;
        private string _versionText;
        private string _dateText;
        private Vector3 _dateRenderPosition;
        private ThemeKind _theme;
        private Color32 _versionStringColor = UIColorPalette.White60Percent;
        private EdgeInsetsVertical _listVerticalPadding = new(0, 16);
        private int _listGap = 8;

        public bool IsPerformingAutoLayout { get; private set; }
        public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;
        public DynamicFont Font => _font ??= FontManager.OpenSansRegularFont;

        public SpriteStateValue BackgroundSprites { get; private set; }
        public ColorStateValue BackgroundColors { get; private set; }

        public ThemeKind Theme {
            get => _theme;
            set => SetTheme(value);
        }

        public int ListGap {
            get => _listGap;
            set {
                if (value == _listGap) return;

                _listGap = value;
                InvalidateAutoLayout();
            }
        }

        public EdgeInsetsVertical ListVerticalPadding {
            get => _listVerticalPadding;
            set {
                if (value.EqualsApprox(_listVerticalPadding)) return;

                _listVerticalPadding = value;
                InvalidateAutoLayout();
            }
        }

        public string VersionText {
            get => _versionText;
            set {
                if (string.Equals(value, _versionText)) return;

                _versionText = value;
                InvalidateAutoLayout();
            }
        }

        public Color32 VersionStringColor {
            get => _versionStringColor;
            set {
                if (value.EqualsColor(_versionStringColor)) return;

                _versionStringColor = value;
                Invalidate();
            }
        }

        public string DateText {
            get => _dateText;
            set {
                if (string.Equals(value, _dateText)) return;

                _dateText = value;
                InvalidateAutoLayout();
            }
        }

        public int DateArrowGap {
            get => _dateArrowGap;
            set {
                if (value == _dateArrowGap) return;

                _dateArrowGap = value;
                InvalidateAutoLayout();
            }
        }

        public int ArrowSize {
            get => _arrowSize;
            set {
                if (value == _arrowSize) return;

                _arrowSize = value;
                InvalidateAutoLayout();
            }
        }

        public bool IsMinimize {
            get => _isMinimize;
            set {
                _isMinimize = value;
                foreach (var item in _items) {
                    item.isVisible = value;
                }

                InvalidateAutoLayout();
            }
        }

        public float TextScale {
            get => _textScale;
            set {
                if (Mathf.Approximately(value, _textScale)) return;

                _textScale = value;
                InvalidateAutoLayout();
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

        public EdgeInsetsHorizontal Padding {
            get => _horizontalPadding;
            set {
                if (value.EqualsApprox(_horizontalPadding)) return;

                _horizontalPadding = value;
                InvalidateAutoLayout();
            }
        }

        public override void Awake() {
            base.Awake();

            BackgroundSprites = new SpriteStateValue(this);
            BackgroundColors = new ColorStateValue(this);
            SetTheme(ThemeKind.ClassicDark);
        }

        protected override void OnRebuildRenderData() {
            RenderBackground();
            RenderText();
        }

        protected override void OnClick(UIMouseEventParameter p) {
            var hitPosition = GetHitPosition(p);
            if (ContainsPoint(hitPosition, Vector2.zero, new Vector2(m_Size.x, MinimumHeight))) {
                p.Use();
                IsMinimize = !IsMinimize;
            }

            base.OnClick(p);
        }

        public void AddItem(string logType, string description, ChangelogType kind, Action<ChangelogItem> setup = null) {
            var item = AddUIComponent<ChangelogItem>();
            item.SuspendAutoLayout();
            item.width = width;
            item.LogTypeString = logType;
            item.Description = description;
            item.LogKind = kind;
            item.ResumeAutoLayout();
            setup?.Invoke(item);
            _items.Add(item);
            InvalidateAutoLayout();
        }

        public void SuspendAutoLayout() => _autoLayoutSuspendCount++;

        public void ResumeAutoLayout(bool performNow = true) {
            if (_autoLayoutSuspendCount == 0) return;

            _autoLayoutSuspendCount--;

            if (_autoLayoutSuspendCount != 0 || !performNow) return;

            PerformAutoLayout();
            Invalidate();
        }

        public void PerformAutoLayout() {
            if (IsPerformingAutoLayout) return;

            IsPerformingAutoLayout = true;

            if (!string.IsNullOrEmpty(_versionText)) {
                using var fontRenderer = RentVersionTextRenderer();
                var versionInfoWidth = fontRenderer.GetRenderedSize(VersionText);
                _versionTextRenderPosition = new Vector3(_horizontalPadding.Left, (MinimumHeight - versionInfoWidth.y) / 2f);
            }

            if (!string.IsNullOrEmpty(_dateText)) {
                using var fontRenderer = RentDateTextRenderer();
                var dateTextRenderSize = fontRenderer.GetRenderedSize(_dateText);
                _dateRenderPosition = new Vector3(m_Size.x - _horizontalPadding.Right - _arrowSize - _dateArrowGap - dateTextRenderSize.x, (MinimumHeight - dateTextRenderSize.y) / 2f);
            }

            foreach (var item in _items) {
                item.isVisible = !_isMinimize;
            }

            if (_isMinimize) {
                SetHeightIfChanged(MinimumHeight);
            }
            else {
                var positionY = MinimumHeight + _listVerticalPadding.Top;
                for (var index = 0; index < _items.Count; index++) {
                    var item = _items[index];
                    if (!item.isVisibleSelf) continue;

                    item.relativePosition = new Vector3(0, positionY);
                    positionY += item.height;
                    if (index != _items.Count - 1)
                        positionY += _listGap;
                }

                SetHeightIfChanged(positionY + _listVerticalPadding.Bottom);
            }

            IsPerformingAutoLayout = false;
        }

        public void SetTheme(ThemeKind theme) {
            if (theme == _theme) return;

            _theme = theme;
            Atlas = DefaultAtlas.Instance;
            BackgroundSprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);
            BackgroundColors.SetValues(theme == ThemeKind.ClassicLight ? UIColorPalette.ClassicLightPalette.SettingItemControlColors : UIColorPalette.ClassicDarkPalette.SettingsSectionColors);
        }

        private void RenderText() {
            if (Font is not { IsValid: true }) return;

            if (_textRenderData != null) {
                _textRenderData.Clear();
            }
            else {
                _textRenderData = UIRenderData.Obtain();
                m_RenderData.Add(_textRenderData);
            }

            if (isLayoutSuspended) return;

            if (!string.IsNullOrEmpty(VersionText)) {
                using var fontRenderer = RentVersionTextRenderer();
                fontRenderer.Render(VersionText, _textRenderData);
            }

            if (!string.IsNullOrEmpty(_dateText)) {
                using var fontRender = RentDateTextRenderer();
                fontRender.Render(_dateText, _textRenderData);
            }
        }

        private DynamicFontRenderer RentDateTextRenderer() {
            var fontRenderer = Font.RentRenderer();
            fontRenderer.WordWrap = false;
            fontRenderer.MaxSize = new Vector2(4096, 4096);
            fontRenderer.PixelRatio = UnitsPerPixel;
            fontRenderer.TextScale = 1f;
            fontRenderer.LayoutOffset = _dateRenderPosition;
            fontRenderer.ProcessMarkup = false;
            fontRenderer.DefaultColor = _versionStringColor;
            fontRenderer.Opacity = 1f;
            return fontRenderer;
        }

        private DynamicFontRenderer RentVersionTextRenderer() {
            var fontRenderer = Font.RentRenderer();
            fontRenderer.WordWrap = false;
            fontRenderer.MaxSize = new Vector2(4096, 4096);
            fontRenderer.PixelRatio = UnitsPerPixel;
            fontRenderer.TextScale = _textScale;
            fontRenderer.LayoutOffset = _versionTextRenderPosition;
            fontRenderer.ProcessMarkup = false;
            fontRenderer.DefaultColor = UIColorPalette.White;
            fontRenderer.Opacity = 1f;
            return fontRenderer;
        }

        private void RenderBackground() {
            if (_atlas == null || IsAutoLayoutSuspended) return;

            renderData.material = _atlas.Material;

            if (_atlas.TryGetSprite(BackgroundSprites.GetValue(_state), out int spriteIndex)) {
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

                if (spriteMetadata.IsSliced)
                    SlicedSpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
                else
                    SpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
            }

            var arrowSpriteName = _isMinimize ? DefaultAtlas.UpArrow : DefaultAtlas.DownArrow;
            if (_atlas.TryGetSprite(arrowSpriteName, out int arrowSpriteIndex)) {
                ref readonly var spriteMetadata = ref _atlas.GetSprite(arrowSpriteIndex);
                SpriteRenderData spriteRenderData = new() {
                    Atlas = _atlas,
                    Color = _versionStringColor,
                    FillAmount = 1f,
                    Flip = UISpriteFlip.None,
                    Offset = new Vector3(width - _horizontalPadding.Right - _arrowSize, (MinimumHeight - _arrowSize) / 2f),
                    UnitsPerPixel = UnitsPerPixel,
                    Size = new Vector2(_arrowSize, _arrowSize),
                    SpriteMetadata = spriteMetadata
                };

                if (spriteMetadata.IsSliced) {
                    SlicedSpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
                    return;
                }

                SpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
            }
        }

        private void InvalidateAutoLayout() {
            if (IsAutoLayoutSuspended) return;

            PerformAutoLayout();
            Invalidate();
        }
    }

    public sealed class ChangelogItem : UIControl, IAutoLayout {
        private const float HeaderFixedHeight = 18;
        private int _autoLayoutSuspendCount;
        private float _headerWidth = 150;
        private int _gap = 8;
        private DynamicFont _font;
        private float _textScale = 1f;
        private EdgeInsets _padding = new(16, 16, 0, 0);
        private EdgeInsets _headerBackgroundPadding = new(2);
        private string _logTypeString;
        private ChangelogType _logKind;
        private string _description;
        private IAtlas _atlas = DefaultAtlas.Instance;
        private string _logTypeBackgroundSpriteName = DefaultAtlas.SmoothRectangleRadius6;
        private Color32 _textDividerColor = UIColorPalette.White60Percent;
        private UIRenderData _textRenderData;
        private Vector3 _headerRenderPosition;
        private Vector2 _headerRenderSize;

        public ILocalizationProvider LocalizationProvider { get; set; }

        public ChangelogType LogKind {
            get => _logKind;
            set {
                if (value.Equals(_logKind)) return;

                _logKind = value;
                InvalidateAutoLayout();
            }
        }

        public Color32 TextDividerColor {
            get => _textDividerColor;
            set {
                if (value.EqualsColor(_textDividerColor)) return;

                _textDividerColor = value;
                Invalidate();
            }
        }

        public string LogTypeBackgroundSpriteName {
            get => _logTypeBackgroundSpriteName;
            set {
                if (string.Equals(value, _logTypeBackgroundSpriteName)) return;

                _logTypeBackgroundSpriteName = value;
                Invalidate();
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

        public EdgeInsets Padding {
            get => _padding;
            set {
                if (value.EqualsApprox(_padding)) return;

                _padding = value;
                InvalidateAutoLayout();
            }
        }

        public EdgeInsets LogTypeBackgroundPadding {
            get => _headerBackgroundPadding;
            set {
                if (value.EqualsApprox(_headerBackgroundPadding)) return;

                _headerBackgroundPadding = value;
                InvalidateAutoLayout();
            }
        }

        public string Description {
            get => _description;
            set {
                if (string.Equals(_description, value)) return;

                _description = value;
                InvalidateAutoLayout();
            }
        }

        public string LogTypeString {
            get => _logTypeString;
            set {
                if (string.Equals(_logTypeString, value)) return;

                _logTypeString = value;
                InvalidateAutoLayout();
            }
        }

        public int Gap {
            get => _gap;
            set {
                if (value.Equals(_gap)) return;

                _gap = value;
                InvalidateAutoLayout();
            }
        }

        public float HeaderWidth {
            get => _headerWidth;
            set {
                if (value.Equals(_headerWidth)) return;

                _headerWidth = value;
                InvalidateAutoLayout();
            }
        }

        public float TextScale {
            get => _textScale;
            set {
                value = Mathf.Max(0.1f, value);
                if (Mathf.Approximately(_textScale, value)) return;

                _textScale = value;
                InvalidateAutoLayout();
            }
        }

        public bool IsPerformingAutoLayout { get; private set; }
        public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;
        public DynamicFont Font => _font ??= FontManager.OpenSansRegularFont;

        public override void OnEnable() {
            base.OnEnable();

            UnityEngine.Font.textureRebuilt += OnUnityFontTextureRebuilt;
        }

        public override void OnDisable() {
            base.OnDisable();

            UnityEngine.Font.textureRebuilt -= OnUnityFontTextureRebuilt;
        }

        protected override void OnSizeChanged() {
            if (!IsAutoLayoutSuspended) {
                PerformAutoLayout();
            }

            base.OnSizeChanged();
        }

        protected override void OnRebuildRenderData() {
            RenderBackground();
            RenderText();
        }

        public void SuspendAutoLayout() => _autoLayoutSuspendCount++;

        public void ResumeAutoLayout(bool performNow = true) {
            if (_autoLayoutSuspendCount == 0) return;

            _autoLayoutSuspendCount--;

            if (_autoLayoutSuspendCount != 0 || !performNow) return;

            PerformAutoLayout();
            Invalidate();
        }

        public void PerformAutoLayout() {
            if (IsPerformingAutoLayout) return;

            IsPerformingAutoLayout = true;

            var newHeight = _padding.Vertical;
            var headerHeight = HeaderFixedHeight + _headerBackgroundPadding.Vertical + _padding.Vertical;
            var descriptionHeight = 0f;

            if (!string.IsNullOrEmpty(_logTypeString)) {
                using var changelogTypeRenderer = RentHeaderRenderer();
                _headerRenderSize = changelogTypeRenderer.GetRenderedSize(_logTypeString);

                var headerRenderWidth = _headerRenderSize.x;
                if (headerRenderWidth >= _headerWidth) {
                    _headerRenderPosition = new Vector3(_padding.Left + _headerBackgroundPadding.Left, _padding.Top + _headerBackgroundPadding.Top);
                }
                else {
                    _headerRenderPosition = new Vector3((_headerWidth - _headerBackgroundPadding.Horizontal - headerRenderWidth) / 2f + _padding.Left + _headerBackgroundPadding.Left, _padding.Top + _headerBackgroundPadding.Top);
                }
            }
            else {
                _headerRenderPosition = Vector3.zero;
            }

            if (!string.IsNullOrEmpty(_description)) {
                using var descriptionRenderer = RentDescriptionRenderer();
                var descriptionRenderSize = descriptionRenderer.GetRenderedSize(_description);
                descriptionHeight = descriptionRenderSize.y + _headerBackgroundPadding.Top + _padding.Top;
            }

            var textHeight = Mathf.Max(headerHeight, descriptionHeight);
            SetHeightIfChanged(newHeight + textHeight);

            IsPerformingAutoLayout = false;
        }

        private void RenderText() {
            if (Font is not { IsValid: true }) return;

            if (_textRenderData != null) {
                _textRenderData.Clear();
            }
            else {
                _textRenderData = UIRenderData.Obtain();
                m_RenderData.Add(_textRenderData);
            }

            if (IsAutoLayoutSuspended) return;

            if (!string.IsNullOrEmpty(_logTypeString)) {
                using var fontRenderer = RentHeaderRenderer();
                fontRenderer.Render(_logTypeString, _textRenderData);
            }

            if (!string.IsNullOrEmpty(_description)) {
                using var fontRender = RentDescriptionRenderer();
                fontRender.Render(_description, _textRenderData);
            }
        }

        private void RenderBackground() {
            if (_atlas == null || IsAutoLayoutSuspended) return;

            renderData.material = _atlas.Material;

            if (!_atlas.TryGetSprite(_logTypeBackgroundSpriteName, out int spriteIndex)) return;

            ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);

            SpriteRenderData spriteRenderData = new() {
                Atlas = _atlas,
                Color = GetChangelogTypeColor(),
                FillAmount = 1f,
                Flip = UISpriteFlip.None,
                Offset = new Vector3(_padding.Left, _padding.Top),
                UnitsPerPixel = UnitsPerPixel,
                Size = new Vector2(_headerWidth, HeaderFixedHeight + _headerBackgroundPadding.Vertical),
                SpriteMetadata = spriteMetadata
            };

            if (spriteMetadata.IsSliced) {
                SlicedSpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
                return;
            }

            SpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
        }

        private DynamicFontRenderer RentDescriptionRenderer() {
            var fontRenderer = Font.RentRenderer();
            fontRenderer.WordWrap = true;
            fontRenderer.MaxSize = new Vector2(width - _headerWidth - _gap - _padding.Horizontal, 4096);
            fontRenderer.PixelRatio = UnitsPerPixel;
            fontRenderer.TextScale = _textScale;
            fontRenderer.LayoutOffset = new Vector3(_padding.Left + _headerWidth + _gap, _padding.Top + _headerBackgroundPadding.Top);
            fontRenderer.ProcessMarkup = false;
            fontRenderer.CharacterSpacing = _characterSpacing;
            fontRenderer.DefaultColor = UIColorPalette.White;
            fontRenderer.Opacity = 1f;
            return fontRenderer;
        }

        private int _characterSpacing = 1;

        public int CharacterSpacing {
            get => _characterSpacing;
            set {
                if (value.Equals(_characterSpacing)) return;

                _characterSpacing = value;
                InvalidateAutoLayout();
            }
        }

        private DynamicFontRenderer RentHeaderRenderer() {
            var fontRenderer = Font.RentRenderer();
            fontRenderer.WordWrap = false;
            fontRenderer.MaxSize = new Vector2(_headerWidth - _headerBackgroundPadding.Horizontal, 4096);
            fontRenderer.PixelRatio = UnitsPerPixel;
            fontRenderer.TextScale = _textScale;
            fontRenderer.LayoutOffset = _headerRenderPosition;
            fontRenderer.ProcessMarkup = false;
            fontRenderer.DefaultColor = UIColorPalette.White;
            fontRenderer.Opacity = 1f;
            return fontRenderer;
        }

        private Color32 GetChangelogTypeColor() => _logKind switch {
            ChangelogType.Added => new Color32(38, 158, 62, 255),
            ChangelogType.Removed => new Color32(146, 50, 128, 255),
            ChangelogType.Updated => new Color32(58, 86, 190, 255),
            ChangelogType.Fixed => new Color32(216, 136, 30, 255),
            ChangelogType.Optimized => new Color32(113, 30, 160, 255),
            ChangelogType.Translation => new Color32(8, 150, 150, 255),
            ChangelogType.Attention => new Color32(146, 4, 10, 255),
            _ => new Color32(160, 160, 160, 255)
        };

        private void InvalidateAutoLayout() {
            if (IsAutoLayoutSuspended) return;

            PerformAutoLayout();
            Invalidate();
        }

        private void OnUnityFontTextureRebuilt(Font font) => Invalidate();
    }
}