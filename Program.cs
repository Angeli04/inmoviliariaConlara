using System.Text;
using Inmobiliaria.Models;
using InmobiliariaConlara.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider();

builder.Services.AddSession();


builder.Services.AddScoped<RepositorioUsuario>();
builder.Services.AddScoped<RepositorioInmuebles>();
builder.Services.AddScoped<RepositorioTipoInmueble>();
builder.Services.AddScoped<RepositorioInquilino>();
builder.Services.AddScoped<RepositorioContratos>();
builder.Services.AddScoped<RepositorioPagos>();


//Configuración de autenticación para Cookies (Web) y JWT (API)
builder.Services.AddAuthentication(options =>
{
    // Establece el esquema de cookies como el predeterminado para el lado web
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        // Configuración para la autenticación web tradicional
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Home/Restringido";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    })
    .AddJwtBearer(options =>
    {
        // Configuración para la autenticación de la API vía tokens
        options.RequireHttpsMetadata = false; // Cambiar a 'true' en producción
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


builder.Services.AddAuthorization(options =>
{
    
    options.AddPolicy("Empleado", policy => policy.RequireRole("Empleado", "Administrador"));
    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
    
    
    
    options.AddPolicy("EsPropietarioApp", policy =>
    {
        // agrego esquema de JWT
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireRole("Propietario");
    });
});

var app = builder.Build();



if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilitar sesion
app.UseSession();


app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();