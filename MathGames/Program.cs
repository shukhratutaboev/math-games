using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MathGames;
using MathGames.Games;
using MathGames.Shared;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

// Register ThemeService as a singleton
builder.Services.AddSingleton<ThemeService>();

builder.Services.AddScoped<GameOfLifeBase>((ctx) => new GameOfLifeBase(35, 35));
builder.Services.AddScoped<LangtonsAntBase>((ctx) => new LangtonsAntBase(40, 40));
builder.Services.AddScoped<FoxAndRabbitBase>((ctx) => new FoxAndRabbitBase(40, 30));
builder.Services.AddScoped<MazeBase>((ctx) => new MazeBase(31, 23));

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
