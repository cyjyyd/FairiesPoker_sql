using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FairiesPoker
{
    internal static class ThemeAssetResolver
    {
        public const int DefaultTheme = 5;

        public static int NormalizeTheme(string value)
        {
            if (!int.TryParse(value, out int theme))
            {
                return DefaultTheme;
            }

            return NormalizeTheme(theme);
        }

        public static int NormalizeTheme(int theme)
        {
            return theme >= 1 && theme <= 6 ? theme : DefaultTheme;
        }

        public static string GetThemeFolderName(int theme)
        {
            return Enum.GetName(typeof(Path), NormalizeTheme(theme)) ?? Path.UI_PF.ToString();
        }

        public static Image LoadThemeImage(int theme, string fileName)
        {
            return LoadImageFromCandidates(GetThemeAssetCandidates(theme, fileName));
        }

        public static Image LoadCardImage(int theme, string fileName)
        {
            return LoadImageFromCandidates(GetCardAssetCandidates(theme, fileName));
        }

        public static string ResolveThemeAssetPath(int theme, string fileName)
        {
            return ResolveFirstExistingPath(GetThemeAssetCandidates(theme, fileName));
        }

        public static string ResolveResultAssetPath(int theme, string fileName)
        {
            return ResolveFirstExistingPath(GetNumberedAssetCandidates("Results", theme, fileName));
        }

        public static Image LoadResultImage(int theme, string fileName)
        {
            return LoadImageFromCandidates(GetNumberedAssetCandidates("Results", theme, fileName));
        }

        private static IEnumerable<string> GetThemeAssetCandidates(int theme, string fileName)
        {
            int normalizedTheme = NormalizeTheme(theme);
            yield return System.IO.Path.Combine(Application.StartupPath, GetThemeFolderName(normalizedTheme), fileName);

            if (normalizedTheme != DefaultTheme)
            {
                yield return System.IO.Path.Combine(Application.StartupPath, GetThemeFolderName(DefaultTheme), fileName);
            }
        }

        private static IEnumerable<string> GetCardAssetCandidates(int theme, string fileName)
        {
            foreach (string path in GetNumberedAssetCandidates("Pokers", theme, fileName))
            {
                yield return path;
            }

            yield return System.IO.Path.Combine(Application.StartupPath, "Pokers", fileName);
        }

        private static IEnumerable<string> GetNumberedAssetCandidates(string assetFolder, int theme, string fileName)
        {
            int normalizedTheme = NormalizeTheme(theme);
            yield return System.IO.Path.Combine(Application.StartupPath, assetFolder, normalizedTheme.ToString(), fileName);
            yield return System.IO.Path.Combine(Application.StartupPath, assetFolder, GetThemeFolderName(normalizedTheme), fileName);

            if (normalizedTheme != DefaultTheme)
            {
                yield return System.IO.Path.Combine(Application.StartupPath, assetFolder, DefaultTheme.ToString(), fileName);
                yield return System.IO.Path.Combine(Application.StartupPath, assetFolder, GetThemeFolderName(DefaultTheme), fileName);
            }
        }

        private static string ResolveFirstExistingPath(IEnumerable<string> candidates)
        {
            foreach (string candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static Image LoadImageFromCandidates(IEnumerable<string> candidates)
        {
            foreach (string candidate in candidates)
            {
                if (!File.Exists(candidate))
                {
                    continue;
                }

                try
                {
                    using (var stream = new FileStream(candidate, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var image = Image.FromStream(stream))
                    {
                        return new Bitmap(image);
                    }
                }
                catch
                {
                }
            }

            return null;
        }
    }
}
