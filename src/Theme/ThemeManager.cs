using System.Drawing;

namespace PROJECT_SBV1.Theme
{
    public enum AppTheme
    {
        Dark,
        Light
    }

    public static class ThemeManager
    {
        public static AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;

        public static event Action<AppTheme>? OnThemeChanged;

        public static void SetTheme(AppTheme theme)
        {
            CurrentTheme = theme;
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
