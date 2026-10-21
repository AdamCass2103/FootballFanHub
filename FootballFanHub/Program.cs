using FootballFanHub.Components;
using FootballFanHub.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMemoryCache();

// Register HttpClient
builder.Services.AddScoped(sp => new HttpClient());

// Register our service
builder.Services.AddScoped<IBalldontlieService, BalldontlieService>();

builder.Services.AddSingleton<INbaFavouritesRepository>(_ =>
    new NbaFavouritesRepository(Path.Combine(builder.Environment.ContentRootPath, "Data", "nba-favourites.json")));

Action<HttpClient> configureFootballData = client =>
{
    client.BaseAddress = new Uri(builder.Configuration["FootballData:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("X-Auth-Token", builder.Configuration["FootballData:ApiKey"] ?? "");
    client.Timeout = TimeSpan.FromSeconds(10);
};

builder.Services.AddHttpClient<IStandingsService, FootballDataStandingsService>(configureFootballData);
builder.Services.AddHttpClient<ITeamService, FootballDataTeamService>(configureFootballData);

builder.Services.AddSingleton<IFavouritesRepository>(_ =>
    new JsonFavouritesRepository(Path.Combine(builder.Environment.ContentRootPath, "Data", "favourites.json")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();