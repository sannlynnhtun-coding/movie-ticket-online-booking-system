using BlazorWasm.MovieTicketsOnlineBooking;
using BlazorWasm.MovieTicketsOnlineBooking.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IDbService, MovieService>();
builder.Services.AddSingleton<PageChangeStateContainer>();

// Cinematix new services
builder.Services.AddScoped<TicketStoreService>();
builder.Services.AddScoped<ITicketStoreService>(sp => sp.GetRequiredService<TicketStoreService>());
builder.Services.AddScoped<IBrowserStoreService>(sp => sp.GetRequiredService<TicketStoreService>());
builder.Services.AddScoped<ThemeService>();

await builder.Build().RunAsync();
