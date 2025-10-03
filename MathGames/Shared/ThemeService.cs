using Microsoft.JSInterop;

namespace MathGames.Shared;

public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isDarkMode = false;

    public event EventHandler? ThemeChanged;

    public bool IsDarkMode => _isDarkMode;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var storedTheme = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "theme");
            _isDarkMode = storedTheme == "dark";
        }
        catch
        {
            // If localStorage is not available, use default (light mode)
            _isDarkMode = false;
        }
    }

    public async Task ToggleThemeAsync()
    {
        _isDarkMode = !_isDarkMode;
        await SaveThemePreferenceAsync();
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task SaveThemePreferenceAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme", _isDarkMode ? "dark" : "light");
        }
        catch
        {
            // Ignore if localStorage is not available
        }
    }
}
