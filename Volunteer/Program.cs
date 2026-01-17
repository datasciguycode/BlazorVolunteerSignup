using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Volunteer;
using Volunteer.Models;
using Volunteer.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
    Timeout = TimeSpan.FromSeconds(30)
});
builder.Services.AddFluentUIComponents();

// Configure Supabase settings from appsettings.json
builder.Services.Configure<SupabaseSettings>(
    builder.Configuration.GetSection("Supabase"));

// Configure Email settings from appsettings.json
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email"));

// Register Supabase service
builder.Services.AddScoped<ISupabaseService, SupabaseService>();

await builder.Build().RunAsync();
