using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ScriptsMirror
{
    private static string RepoRoot => Directory.GetParent(Application.dataPath)!.FullName;
    private static string SrcRoot => Path.Combine(Application.dataPath, "_Project", "Scripts"); // prireikus praplėsk
    private static string DstRoot => Path.Combine(RepoRoot, "ScriptsMirror");

    [MenuItem("IdleBiz/Export/Sync Scripts Mirror %#m")]
    public static void SyncNow()
    {
        if (!Directory.Exists(SrcRoot)) { Debug.LogWarning("[ScriptsMirror] Source not found: " + SrcRoot); return; }
        if (!Directory.Exists(DstRoot)) Directory.CreateDirectory(DstRoot);

        foreach (var f in Directory.GetFiles(DstRoot, "*.cs", SearchOption.AllDirectories))
            if (!f.Replace('\\', '/').Contains("/.git/")) File.Delete(f);

        var files = Directory.GetFiles(SrcRoot, "*.cs", SearchOption.AllDirectories);
        foreach (var src in files)
        {
            var rel = src.Substring(SrcRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var dst = Path.Combine(DstRoot, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
            File.Copy(src, dst, true);
        }

        var toc = new StringBuilder();
        toc.AppendLine("=== IdleBiz Scripts Mirror (read-only) ===");
        toc.AppendLine("Source: " + SrcRoot);
        toc.AppendLine("Generated: " + System.DateTime.Now);
        toc.AppendLine();
        foreach (var p in Directory.GetFiles(DstRoot, "*.cs", SearchOption.AllDirectories).OrderBy(p => p))
            toc.AppendLine(p.Substring(DstRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        File.WriteAllText(Path.Combine(DstRoot, "_SCRIPTS_TOC.txt"), toc.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        Debug.Log($"[ScriptsMirror] Synced {files.Length} scripts → {DstRoot}");
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded() => SyncNow();
}
