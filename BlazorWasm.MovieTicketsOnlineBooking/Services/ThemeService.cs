using Microsoft.JSInterop;

namespace BlazorWasm.MovieTicketsOnlineBooking.Services;

public class ThemeService
{
    private readonly IJSRuntime _js;
    private string _currentTheme = "dark";

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    public string CurrentTheme => _currentTheme;
    public bool IsDark => _currentTheme == "dark";

    public event Action? OnThemeChanged;

    public async Task InitAsync()
    {
        _currentTheme = await _js.InvokeAsync<string>("cinematix.getTheme");
        OnThemeChanged?.Invoke();
    }

    public async Task ToggleThemeAsync()
    {
        _currentTheme = await _js.InvokeAsync<string>("cinematix.toggleTheme");
        OnThemeChanged?.Invoke();
    }

    public async Task SetThemeAsync(string theme)
    {
        _currentTheme = await _js.InvokeAsync<string>("cinematix.setTheme", theme);
        OnThemeChanged?.Invoke();
    }
}
