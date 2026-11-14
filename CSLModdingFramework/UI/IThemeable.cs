namespace CSLModdingFramework.UI;

public interface IThemeable {
    ThemeKind Theme { get; set; }
}

/*
 
protected ThemeKind _theme;

public virtual ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }
    
*/