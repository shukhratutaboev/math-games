using MudBlazor;

namespace MathGames.Shared
{
    public static class MathGamesTheme
    {
        public static MudTheme DefaultTheme => new MudTheme()
        {
            PaletteDark = new PaletteDark()
            {
                Primary = "#3a0647",
                Secondary = "#052767",
                Tertiary = "#7e6fff",
                AppbarBackground = "#3a0647",
                Background = "#f5f5f5",
                DrawerBackground = "#FFF",
                DrawerText = "rgba(0,0,0, 0.7)",
                Success = "#007E33"
            }
        };
    }
}
