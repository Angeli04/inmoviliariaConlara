using System.Text;
using Inmobiliaria.Models;
using InmobiliariaConlara.Models; // Asegúrate que el namespace sea el correcto
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Registrar servicios MVC y Session
builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider();

builder.Services.AddSession();

// 🔹 Registrar repositorios (y otros que necesites)
builder.Services.AddScoped<RepositorioUsuario>();
builder.Services.AddScoped<RepositorioInmuebles>();
builder.Services.AddScoped<RepositorioTipoInmueble>();
// ... Agrega aquí el resto de tus repositorios

// 🔹 Configuración de autenticación para Cookies (Web) y JWT (API)
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

// 🔹 Políticas de autorización
builder.Services.AddAuthorization(options =>
{
    // Políticas existentes para el lado web (usan cookies por defecto)
    options.AddPolicy("Empleado", policy => policy.RequireRole("Empleado", "Administrador"));
    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
    
    // --- NUEVA POLÍTICA PARA LA API MÓVIL ---
    // Esta política se usará en los endpoints de la API que son solo para propietarios.
    options.AddPolicy("EsPropietarioApp", policy =>
    {
        // 1. Exige que la autenticación sea SÍ O SÍ con un token JWT (Bearer)
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        // 2. Además, exige que el usuario tenga el rol "Propietario"
        policy.RequireRole("Propietario");
    });
});

var app = builder.Build();

// --- Configuración del Pipeline de Middlewares ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilitar sesión
app.UseSession();

// 🚨 ¡El orden es crucial aquí!
// 1. UseAuthentication: Identifica quién es el usuario (lee cookie o token)
app.UseAuthentication();
// 2. UseAuthorization: Verifica si el usuario identificado tiene permiso
app.UseAuthorization();

// 🔹 Rutas para los controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();