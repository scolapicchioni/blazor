using Duende.Bff.Yarp;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddReverseProxy().AddTransforms<AccessTokenTransformProvider>().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddBff();

builder.Services.AddAuthentication(options => {
    options.DefaultScheme = "cookie";
    options.DefaultChallengeScheme = "oidc";
    options.DefaultSignOutScheme = "oidc";
})
.AddCookie("cookie", options => {
    options.Cookie.Name = "__Host-blazor";
    options.Cookie.SameSite = SameSiteMode.Strict;
})
.AddOpenIdConnect("oidc", options => {
    options.Authority = "https://photosharingapplication.identityprovider";

    options.ClientId = "photosharing.bff";
    options.ClientSecret = "A9B27D26-E71C-4C53-89A8-3DAB53CE1854"; // replace this with the GUID you generated as `ClientSecrets` in the Config.cs file of the IdentityProvider project
    options.ResponseType = "code";
    options.ResponseMode = "query";

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("photosrest");
    options.Scope.Add("commentsgrpc");
    options.Scope.Add("offline_access");

    options.MapInboundClaims = false;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.SaveTokens = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseWebAssemblyDebugging();
} else {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseBff();
app.UseAuthorization();
app.MapBffManagementEndpoints();

app.MapRazorPages();
app.MapReverseProxy();
app.MapFallbackToFile("index.html");

app.Run();