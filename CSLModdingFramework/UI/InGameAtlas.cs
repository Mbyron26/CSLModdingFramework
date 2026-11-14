namespace CSLModdingFramework.UI;

public class InGameAtlas {
    private static TextureAtlas _instance;

    public static TextureAtlas Instance {
        get {
            if (_instance != null) return _instance;

            _instance = TextureAtlas.Create("InGameAtlas");
            var inGameAtlas = TextureAtlas.GetAtlas("Ingame");
            if (inGameAtlas == null) return _instance;

            foreach (var spriteInfo in inGameAtlas.sprites) {
                _instance.AddSprite(spriteInfo);
            }
            _instance.Build(4096);

            return _instance;
        }
    }
}