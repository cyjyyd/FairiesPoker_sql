#if ANDROID
using System;
using System.IO;
using IOPath = System.IO.Path;

namespace FairiesPoker.MG.AndroidPlatform;

internal static class AndroidAssetExtractor
{
    private const string AssetVersion = "mobile-stage2-3";
    private static global::Android.Content.Res.AssetManager _assetManager;
    private static string _resourceRoot = string.Empty;

    private static readonly string[] StartupAssetRoots =
    {
        "Fonts",
        "Resources",
        "UI_PF"
    };

    private static readonly string[] RequiredStartupFiles =
    {
        "Fonts/HarmonyOS_Sans_SC_Regular.ttf",
        "Resources/main seq.jpg",
        "UI_PF/main seq.jpg"
    };

    public static void ExtractRequired(global::Android.Content.Res.AssetManager assetManager, string resourceRoot)
    {
        if (assetManager == null || string.IsNullOrWhiteSpace(resourceRoot))
            return;

        Initialize(assetManager, resourceRoot);

        string fullRoot = IOPath.GetFullPath(resourceRoot);
        string markerPath = IOPath.Combine(fullRoot, ".asset-version");
        if (File.Exists(markerPath) &&
            SafeReadAllText(markerPath) == AssetVersion &&
            HasRequiredStartupAssets(fullRoot))
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(fullRoot);
            ClearDirectory(fullRoot);
        }
        catch
        {
            return;
        }

        bool copiedAll = true;
        foreach (string root in StartupAssetRoots)
        {
            copiedAll &= TryCopyAssetDirectory(assetManager, root, fullRoot);
        }

        if (copiedAll && HasRequiredStartupAssets(fullRoot))
        {
            try
            {
                File.WriteAllText(markerPath, AssetVersion);
            }
            catch
            {
            }
        }
    }

    public static void Initialize(global::Android.Content.Res.AssetManager assetManager, string resourceRoot)
    {
        _assetManager = assetManager;
        _resourceRoot = string.IsNullOrWhiteSpace(resourceRoot)
            ? string.Empty
            : IOPath.GetFullPath(resourceRoot);
    }

    public static bool TryExtractAsset(string relativePath)
    {
        if (_assetManager == null || string.IsNullOrWhiteSpace(_resourceRoot) || string.IsNullOrWhiteSpace(relativePath))
            return false;

        string assetPath = NormalizeAssetPath(relativePath);
        string outputPath = ToOutputPath(_resourceRoot, assetPath);
        if (File.Exists(outputPath))
            return true;

        return TryCopyAssetFile(_assetManager, assetPath, _resourceRoot);
    }

    private static bool TryCopyAssetDirectory(global::Android.Content.Res.AssetManager assetManager, string assetPath, string outputRoot)
    {
        string[] children;
        try
        {
            children = assetManager.List(assetPath) ?? Array.Empty<string>();
        }
        catch
        {
            return false;
        }

        if (children.Length == 0)
        {
            return TryCopyAssetFile(assetManager, assetPath, outputRoot);
        }

        string outputDirectory = ToOutputPath(outputRoot, assetPath);
        try
        {
            EnsureWithinRoot(outputRoot, outputDirectory);
            Directory.CreateDirectory(outputDirectory);
        }
        catch
        {
            return false;
        }

        bool copiedAll = true;
        foreach (string child in children)
        {
            string childAssetPath = string.IsNullOrEmpty(assetPath) ? child : assetPath + "/" + child;
            copiedAll &= TryCopyAssetDirectory(assetManager, childAssetPath, outputRoot);
        }

        return copiedAll;
    }

    private static bool TryCopyAssetFile(global::Android.Content.Res.AssetManager assetManager, string assetPath, string outputRoot)
    {
        string outputPath = ToOutputPath(outputRoot, assetPath);

        try
        {
            EnsureWithinRoot(outputRoot, outputPath);
            string directory = IOPath.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            using Stream input = assetManager.Open(assetPath);
            using FileStream output = File.Create(outputPath);
            input.CopyTo(output);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeAssetPath(string relativePath)
    {
        return relativePath.Replace('\\', '/').TrimStart('/');
    }

    private static string SafeReadAllText(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool HasRequiredStartupAssets(string root)
    {
        foreach (string relativePath in RequiredStartupFiles)
        {
            string path = ToOutputPath(root, relativePath);
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length <= 0)
                    return false;
            }
            catch
            {
                return false;
            }
        }

        return true;
    }

    private static string ToOutputPath(string outputRoot, string assetPath)
    {
        return IOPath.Combine(outputRoot, assetPath.Replace('/', IOPath.DirectorySeparatorChar));
    }

    private static void ClearDirectory(string root)
    {
        string fullRoot = IOPath.GetFullPath(root);
        if (!Directory.Exists(fullRoot))
            return;

        foreach (string filePath in Directory.EnumerateFiles(fullRoot))
        {
            EnsureWithinRoot(fullRoot, filePath);
            File.Delete(filePath);
        }

        foreach (string directoryPath in Directory.EnumerateDirectories(fullRoot))
        {
            EnsureWithinRoot(fullRoot, directoryPath);
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    private static void EnsureWithinRoot(string root, string path)
    {
        string fullRoot = EnsureTrailingSeparator(IOPath.GetFullPath(root));
        string fullPath = IOPath.GetFullPath(path);
        if (!fullPath.StartsWith(fullRoot, StringComparison.Ordinal))
            throw new InvalidOperationException("Resolved asset path escapes the Android resource directory.");
    }

    private static string EnsureTrailingSeparator(string path)
    {
        char separator = IOPath.DirectorySeparatorChar;
        return path.EndsWith(separator) ? path : path + separator;
    }

}
#endif
