using MudBlazor;

namespace MathGames.Shared
{
    public static class MathGamesTheme
    {
        public static MudTheme Theme => new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = "#594ae2",
                Secondary = "#ff6b6b",
                Tertiary = "#4ecdc4",
                AppbarBackground = "#594ae2",
                Background = "#f5f5f5",
                Surface = "#ffffff",
                DrawerBackground = "#ffffff",
                DrawerText = "rgba(0,0,0, 0.87)",
                AppbarText = "#ffffff",
                TextPrimary = "rgba(0,0,0, 0.87)",
                TextSecondary = "rgba(0,0,0, 0.54)",
                Success = "#06d6a0",
                Info = "#3a86ff",
                Warning = "#ffd60a",
                Error = "#ef476f",
                Dark = "#212529"
            },
            PaletteDark = new PaletteDark()
            {
                Primary = "#7c3aed",
                Secondary = "#f43f5e",
                Tertiary = "#14b8a6",
                AppbarBackground = "#1e1b4b",
                Background = "#0f172a",
                Surface = "#1e293b",
                DrawerBackground = "#1e293b",
                DrawerText = "rgba(255,255,255, 0.87)",
                AppbarText = "#ffffff",
                TextPrimary = "rgba(255,255,255, 0.87)",
                TextSecondary = "rgba(255,255,255, 0.54)",
                Success = "#10b981",
                Info = "#3b82f6",
                Warning = "#f59e0b",
                Error = "#ef4444",
                Dark = "#0f172a"
            }
        };
    }
}
