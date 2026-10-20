using System;
using System.Drawing;
using System.IO;

namespace PROJECT_SBV1.Theme
{
    public enum AppTheme
    {
        Dark,
        Light
    }

    public static class ThemeManager
    {
        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme_settings.json");
        private static readonly string LocalAppDataFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "STOMS", "theme_settings.json");

        public static AppTheme CurrentTheme { get; private set; } = LoadSavedTheme();

        public static event Action<AppTheme>? OnThemeChanged;

        private static AppTheme LoadSavedTheme()
        {
            try
            {
                string? path = null;
                if (File.Exists(LocalAppDataFilePath))
                {
                    path = LocalAppDataFilePath;
                }
                else if (File.Exists(SettingsFilePath))
                {
                    path = SettingsFilePath;
                }

                if (path != null)
                {
                    string content = File.ReadAllText(path).Trim();
                    if (content.Contains("Light", StringComparison.OrdinalIgnoreCase))
                    {
                        return AppTheme.Light;
                    }
                    if (content.Contains("Dark", StringComparison.OrdinalIgnoreCase))
                    {
                        return AppTheme.Dark;
                    }
                }
            }
            catch
            {
                // Fallback to default
            }

            return AppTheme.Dark;
        }

        public static void SaveTheme(AppTheme theme)
        {
            try
            {
                string json = $"{{\n  \"Theme\": \"{theme}\"\n}}";

                // Save to BaseDirectory (near executable)
                try
                {
                    File.WriteAllText(SettingsFilePath, json);
                }
                catch { }

                // Save to %LOCALAPPDATA%\STOMS
                try
                {
                    string dir = Path.GetDirectoryName(LocalAppDataFilePath)!;
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllText(LocalAppDataFilePath, json);
                }
                catch { }
            }
            catch
            {
                // Failsafe silent fallback
            }
        }

        public static void SetTheme(AppTheme theme)
        {
            CurrentTheme = theme;
            SaveTheme(theme);
            OnThemeChanged?.Invoke(CurrentTheme);
        }

        public static void ToggleTheme()
        {
            SetTheme(CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark);
        }

        // Dynamic Color Properties based on CurrentTheme
        public static Color Background => CurrentTheme == AppTheme.Dark 
            ? Color.FromArgb(23, 24, 31) 
            : Color.FromArgb(244, 246, 251);

        public static Color SidebarBg => CurrentTheme == AppTheme.Dark 
            ? Color.FromArgb(19, 20, 27) 
            : Color.FromArgb(255, 255, 255);

        public static Color CardBg => CurrentTheme == AppTheme.Dark 
            ? Color.FromArgb(30, 31, 42) 
            : Color.FromArgb(255, 255, 255);

        public static Color CardBorder => CurrentTheme == AppTheme.Dark 
            ? Color.FromArgb(42, 45, 60) 
            : Color.FromArgb(226, 232, 240);

        public static Color TextPrimary => CurrentTheme == AppTheme.Dark 
            ? Color.FromArgb(255, 255, 255) 
            : Color.FromArgb(30, 41, 59);

        public static Color TextSecondary => CurrentTheme == AppTheme.Dark 
            ? Color.FromArgb(142, 146, 188) 
            : Color.FromArgb(100, 116, 139);

        public static Color AccentBlue => Color.FromArgb(76, 111, 255);
        public static Color AccentPurple => Color.FromArgb(140, 82, 255);
        public static Color AccentGreen => Color.FromArgb(16, 185, 129);
        public static Color AccentOrange => Color.FromArgb(245, 158, 11);
        public static Color AccentRed => Color.FromArgb(239, 68, 68);
        public static Color AccentCyan => Color.FromArgb(6, 182, 212);
        public static Color AccentPink => Color.FromArgb(236, 72, 153);

        public static Color TableHeaderBg => CurrentTheme == AppTheme.Dark 
            ? Color.FromArgb(33, 35, 48) 
            : Color.FromArgb(248, 250, 252);

        public static Color TableRowAlt => CurrentTheme == AppTheme.Dark 
            ? Color.FromArgb(26, 27, 36) 
            : Color.FromArgb(241, 245, 249);
    }
}
