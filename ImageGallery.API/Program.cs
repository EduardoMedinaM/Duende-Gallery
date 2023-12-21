using ImageGallery.API.DbContexts;
using ImageGallery.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(configure => configure.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddDbContext<GalleryContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:ImageGalleryDBConnectionString"]);
});

// register the repository
builder.Services.AddScoped<IGalleryRepository, GalleryRepository>();

// register AutoMapper-related services
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

/**
 * To get claims like Claim type: sub - Claim value: b7539694-97e7-4dfe-84da-b4256e1ff5c7 (the original claim now is capted)
 * and avoid mapping them through the middleware dictionary
 * The default was used before for backwards compatibility with MS WS standards
 */
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        /**
         * Middleware uses to read metadata
         * Caches the result
         * Responsible for validating the access token
         * Makes sure that "imagegalleryapi" is checked as an "audience" value in the token.
         * Checks the Type header of the token
         */
        options.Authority = "https://localhost:5001";
        options.Audience = "imagegalleryapi";

        // tells the framework where to find the role
        options.TokenValidationParameters = new()
        {
            NameClaimType = "given_name",
            RoleClaimType = "role",
            ValidTypes = new[] { "at+jwt" }
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseStaticFiles();

/**
 * Middleware checks if API access is allowed before to touch
 * controllers
 */
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
