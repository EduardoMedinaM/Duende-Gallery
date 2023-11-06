using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(configure => 
        configure.JsonSerializerOptions.PropertyNamingPolicy = null);

/**
 * To get claims like Claim type: sub - Claim value: b7539694-97e7-4dfe-84da-b4256e1ff5c7 (the original claim now is capted)
 * and avoid mapping them through the middleware dictionary
 * The default was used before for backwards compatibility with MS WS standards
 */
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// create an HttpClient used for accessing the API
builder.Services.AddHttpClient("APIClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ImageGalleryAPIRoot"]);
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
});

// Configures the Authenthication middleware
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    /* 
     * Cookie based Authentication. It means:
     * once the identity token is validated and transformed into a claims identity,
     * it will be stored in an encrypted cookie. The cookie is then used
     * on subsequent requests to the web app to check
     */
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    /*
     * 
     * OpenIdConnect handler: Enables our application to support the OpenID Connect
     * authentication workflow => Code Flow
     * handles: Authorization requests, token requests, and other requests
     * It also ensures that the identity token validation happens.
     */
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        // Duende.IDP is the Authority
        options.Authority = "https://localhost:5001";
        // Needs to match the ClientId registered in the Duende.IDP project
        options.ClientId = "imagegalleryclient";
        options.ClientSecret = "secret";
        // PKCE
        options.ResponseType = "code";
        /*
         * Scopes.You don't have to declare them
         * since openid and profile
         * are requested by the middleware
         */
        options.Scope.Add("openid");
        options.Scope.Add("profile");
		// needs to be defined here since it's part of the validation. Default: signin-oidc
		options.CallbackPath = new PathString("/signin-oidc");
        /* to later
         * var identityToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
         * 
         * options.CallbackPath = new PathString("signin-oidc");
         * SignedOutCallbackPath: default = host:port/signout-callback-oidc.
         * Must match with the post logout redirect URI at IDP client config if
         * you want to automatically return to the application after logging out of Identity Server
         * To change, set SignedOutCallbackPath
         * eg: options.SignedOutCallbackPath = "";
         */

        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;

        /*
         * This helps to reduce the cookie size
         */
        // Removes filters
        options.ClaimActions.Remove("aud");
        // Deletes a claim
        options.ClaimActions.DeleteClaim("sid");
        options.ClaimActions.DeleteClaim("idp");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

/*
 * Sets the pipeline. Please set here
 * to effectively block access to the controller
 */
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Gallery}/{action=Index}/{id?}");

app.Run();
