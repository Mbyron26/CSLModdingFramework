using System.Collections.Generic;

namespace CSLModdingFramework.Localization;

public static class Languages {
    public const string FallbackLanguage = "en-US";

    public static readonly HashSet<string> BuiltIn = new() {
        "ar-SA",
        "cs-CZ",
        "de-DE",
        "en-US",
        "es-ES",
        "fr-FR",
        "it-IT",
        "ja-JP",
        "ko-KR",
        "nl-NL",
        "pl-PL",
        "pt-BR",
        "pt-PT",
        "ru-RU",
        "sk-SK",
        "th-TH",
        "tr-TR",
        "zh-CN",
        "zh-TW",
    };
}