using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ProgramPulse.Web;
using ProgramPulse.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<UsersApiClient>();
builder.Services.AddScoped<FaqsApiClient>();
builder.Services.AddScoped<ProgrammesApiClient>();
builder.Services.AddScoped<ObjectivesApiClient>();
builder.Services.AddScoped<KpisApiClient>();
builder.Services.AddScoped<MeasurementsApiClient>();

// Mock data source for the Programmes UI (swap for an API-backed source later).
builder.Services.AddSingleton<SampleData>();

await builder.Build().RunAsync();
