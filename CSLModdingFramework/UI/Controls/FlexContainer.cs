using System.Collections.Generic;
using System.Linq;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI.Controls;

public class FlexContainer : AutoLayoutContainer {
    protected FlexDirection _direction;
    protected FlexJustifyContent _justifyContent;
    protected FlexAlignItems _itemsAlign;
    protected FlexAlignContent _contentAlign;
    protected float _columnGap;
    protected float _rowGap;
    protected bool _autoFitChildrenVertically;
    protected bool _autoFitChildrenHorizontally;
    protected BackgroundRenderer _background;
    protected ForegroundRenderer _foreground;

    public BackgroundRenderer Background => _background ??= new BackgroundRenderer(this);
    public ForegroundRenderer Foreground => _foreground ??= new ForegroundRenderer(this);

    public virtual FlexDirection Direction {
        get => _direction;
        set {
            if (value.Equals(_direction)) return;

            _direction = value;
            InvalidateAutoLayout();
        }
    }

    public virtual FlexJustifyContent Justify {
        get => _justifyContent;
        set {
            if (value.Equals(_justifyContent)) return;

            _justifyContent = value;
            InvalidateAutoLayout();
        }
    }

    public virtual FlexAlignItems ItemsAlign {
        get => _itemsAlign;
        set {
            if (value.Equals(_itemsAlign)) return;

            _itemsAlign = value;
            InvalidateAutoLayout();
        }
    }

    public virtual FlexAlignContent ContentAlign {
        get => _contentAlign;
        set {
            if (value.Equals(_contentAlign)) return;

            _contentAlign = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float ColumnGap {
        get => _columnGap;
        set {
            if (Mathf.Approximately(value, _columnGap)) return;

            _columnGap = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float RowGap {
        get => _rowGap;
        set {
            if (Mathf.Approximately(value, _rowGap)) return;

            _rowGap = value;
            InvalidateAutoLayout();
        }
    }

    public virtual bool AutoFitChildrenVertically {
        get => _autoFitChildrenVertically;
        set {
            if (value == _autoFitChildrenVertically) return;

            _autoFitChildrenVertically = value;
            InvalidateAutoLayout();
        }
    }

    public virtual bool AutoFitChildrenHorizontally {
        get => _autoFitChildrenHorizontally;
        set {
            if (value == _autoFitChildrenHorizontally) return;

            _autoFitChildrenHorizontally = value;
            InvalidateAutoLayout();
        }
    }

    public override void PerformAutoLayout() {
        if (!_autoLayout || IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;
        var children = GetProcessableChildren();
        if (children.Count == 0) {
            IsPerformingAutoLayout = false;
            return;
        }

        var isRow = _direction is FlexDirection.Row or FlexDirection.RowReverse;
        var spacingMain = isRow ? _columnGap : _rowGap;
        var spacingCross = isRow ? _rowGap : _columnGap;

        var innerWidth = width - _padding.Horizontal;
        var innerHeight = height - _padding.Vertical;

        // Step1: Line/column processing
        var lines = new List<LineInfo>();
        var lineStart = 0;
        var lineMain = 0f;
        var lineCross = 0f;

        for (var i = 0; i < children.Count; i++) {
            var child = children[i];
            var childSize = child.size;
            var childMain = isRow ? childSize.x : childSize.y;
            var childCross = isRow ? childSize.y : childSize.x;

            var needWrap = lineMain > 0f && lineMain + spacingMain + childMain > (isRow ? innerWidth : innerHeight);
            if (needWrap) {
                lines.Add(new LineInfo {
                    StartIndex = lineStart,
                    Count = i - lineStart,
                    MainSize = lineMain,
                    CrossSize = lineCross
                });
                lineStart = i;
                lineMain = 0f;
                lineCross = 0f;
            }

            lineMain += (lineMain > 0f ? spacingMain : 0f) + childMain;
            lineCross = Mathf.Max(lineCross, childCross);
        }

        if (lineStart < children.Count)
            lines.Add(new LineInfo {
                StartIndex = lineStart,
                Count = children.Count - lineStart,
                MainSize = lineMain,
                CrossSize = lineCross
            });

        // Step2: Calculate the total size of the cross axis
        var totalCrossSize = lines.Sum(line => line.CrossSize);
        totalCrossSize += spacingCross * (lines.Count - 1);

        var remainingCross = (isRow ? innerHeight : innerWidth) - totalCrossSize;
        var crossExtraSpacing = spacingCross;
        var crossStart = 0f;

        switch (_contentAlign) {
            case FlexAlignContent.FlexStart: crossStart = 0f; break;
            case FlexAlignContent.FlexEnd: crossStart = remainingCross; break;
            case FlexAlignContent.Center: crossStart = remainingCross / 2f; break;
            case FlexAlignContent.SpaceBetween:
                crossExtraSpacing = lines.Count > 1 ? remainingCross / (lines.Count - 1) : 0f;
                crossStart = 0f;
                break;
            case FlexAlignContent.SpaceAround:
                crossExtraSpacing = lines.Count > 0 ? remainingCross / lines.Count : 0f;
                crossStart = crossExtraSpacing / 2f;
                break;
        }

        // Step3: Layout each row
        var crossPos = crossStart;
        foreach (var line in lines) {
            var lineMainSize = line.MainSize;
            var lineCrossSize = line.CrossSize;

            var mainStart = 0f;
            var extraMainSpacing = spacingMain;
            switch (_justifyContent) {
                case FlexJustifyContent.FlexStart: mainStart = 0f; break;
                case FlexJustifyContent.FlexEnd: mainStart = (isRow ? innerWidth : innerHeight) - lineMainSize; break;
                case FlexJustifyContent.Center: mainStart = ((isRow ? innerWidth : innerHeight) - lineMainSize) / 2f; break;
                case FlexJustifyContent.SpaceBetween:
                    extraMainSpacing = line.Count > 1 ? ((isRow ? innerWidth : innerHeight) - (lineMainSize - spacingMain * (line.Count - 1))) / (line.Count - 1) : 0f;
                    mainStart = 0f;
                    break;
                case FlexJustifyContent.SpaceAround:
                    extraMainSpacing = line.Count > 0 ? ((isRow ? innerWidth : innerHeight) - (lineMainSize - spacingMain * (line.Count - 1))) / line.Count : 0f;
                    mainStart = extraMainSpacing / 2f;
                    break;
                case FlexJustifyContent.SpaceEvenly:
                    extraMainSpacing = line.Count > 0 ? ((isRow ? innerWidth : innerHeight) - (lineMainSize - spacingMain * (line.Count - 1))) / (line.Count + 1) : 0f;
                    mainStart = extraMainSpacing;
                    break;
            }

            var posMain = mainStart;

            for (var i = 0; i < line.Count; i++) {
                var child = children[line.StartIndex + i];
                var childSize = child.size;
                var childMain = isRow ? childSize.x : childSize.y;
                var childCross = isRow ? childSize.y : childSize.x;

                var childCrossPos = 0f;
                switch (_itemsAlign) {
                    case FlexAlignItems.FlexStart: childCrossPos = 0f; break;
                    case FlexAlignItems.FlexEnd: childCrossPos = lineCrossSize - childCross; break;
                    case FlexAlignItems.Center: childCrossPos = (lineCrossSize - childCross) / 2f; break;
                    case FlexAlignItems.Stretch:
                        childCrossPos = 0f;
                        if (isRow) child.height = lineCrossSize;
                        else child.width = lineCrossSize;
                        break;
                }

                float posX, posY;
                if (isRow) {
                    posX = _padding.Left + (Direction == FlexDirection.Row ? posMain : innerWidth - posMain - childMain);
                    posY = _padding.Top + crossPos + childCrossPos;
                }
                else {
                    posY = _padding.Top + (Direction == FlexDirection.Column ? posMain : innerHeight - posMain - childMain);
                    posX = _padding.Left + crossPos + childCrossPos;
                }

                posMain += childMain + extraMainSpacing;

                child.relativePosition = new Vector3(posX, posY);
            }

            crossPos += lineCrossSize + crossExtraSpacing;
        }

        // Step4: AutoFit
        if (_autoFitChildrenHorizontally)
            width = _padding.Horizontal + (isRow ? GetMaxLineMainSize(lines) : totalCrossSize);

        if (_autoFitChildrenVertically)
            height = _padding.Vertical + (isRow ? totalCrossSize : GetMaxLineMainSize(lines));

        IsPerformingAutoLayout = false;
    }

    protected float GetMaxLineMainSize(List<LineInfo> lines) => lines.Aggregate(0f, (current, line) => Mathf.Max(current, line.MainSize));

    protected override void OnRebuildRenderData() {
        _background?.Render();
        _foreground?.Render();
    }

    public struct LineInfo {
        public int StartIndex;
        public int Count;
        public float MainSize;
        public float CrossSize;
    }
}