using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using InmobiliariaConlara.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Registrar servicios MVC y Session
builder.Services.AddControllersWithViews()
       .AddSessionStateTempDataProvider();

builder.Services.AddSession();

// 🔹 Registrar repositorios
builder.Services.AddScoped<RepositorioUsuario>();

// 🔹 Configuración JWT
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

// 🔹 Configuración de autenticación (cookies + JWT)
builder.Services.AddAuthentication(options =>
{
    // Cookies como default para la web
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Home/Restringido";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.SlidingExpiration = true;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false; // solo para desarrollo
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// 🔹 Políticas de autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Empleado", policy => policy.RequireRole("Empleado", "Administrador"));
    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("Propietario", policy => policy.RequireRole("Propietario"));
    
});

var app = builder.Build();

// 🔹 Middleware de sesión
app.UseSession();

// 🔹 Middleware de errores
app.UseMiddleware<Inmobiliaria.Middleware.ErrorHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔹 Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Rutas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
