using System;
using ColossalFramework.PlatformServices;
using UnityEngine;

namespace CSLModdingFramework.Utilities;

public static class URLHelper {
    public static void OpenURL(string url) {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));

        Application.OpenURL(url);
    }

    public static void OpenSteamURL(string url) {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));

        PlatformService.ActivateGameOverlayToWebPage(url);
    }
}