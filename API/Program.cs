using KobraKai.API.Services;
using KobraKai.API.Data;
using KobraKai.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// EF + Identity
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
{
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequireUppercase = false;
    opts.Password.RequireLowercase = false;
    opts.Password.RequireDigit = false;
    opts.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT auth
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new()
     {
         ValidateIssuerSigningKey = true,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidIssuer = jwtIssuer,
         ValidAudience = jwtAudience,
         ValidateLifetime = true
     };
 });

builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtTokenService>();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var jwt = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Bearer {your JWT}"
    };
    c.AddSecurityDefinition("Bearer", jwt);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwt, Array.Empty<string>() } });
});

// CORS (add your web origins here as needed)
builder.Services.AddCors(p => p.AddPolicy("Dev", policy =>
    policy.WithOrigins(
            "http://localhost:5167" // Blazor Server (your web app)
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // enable if your API uses HTTPS

app.UseCors("Dev");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();