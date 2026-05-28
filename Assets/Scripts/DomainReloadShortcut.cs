#if UNITY_EDITOR

using UnityEditor;

public static class DomainReloadShortcut
{
    // % = Ctrl (Windows) / Cmd (macOS)
    // & = Alt (Windows) / Option (macOS)
    // This creates a shortcut: Ctrl + Alt + R
    [MenuItem("Tools/Force Domain Reload %&r")]
    public static void ForceReload()
    {
        EditorUtility.RequestScriptReload();
    }
}

#endif