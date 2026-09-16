using System.Text;
using Api.Middleware;
using Application.Commands.CrearSubasta;
using Application.Commands.Depositar;
using Application.Commands.EliminarSubasta;
using Application.Commands.Login;
using Application.Commands.ModificarSubasta;
using Application.Commands.Ofertar;
using Application.Interfaces;
using Application.Queries.ListarCategorias;
using Application.Queries.ListarPujas;
using Application.Queries.ListarSubastas;
using Application.Queries.ListarUsuarios;
using Application.Queries.ObtenerBalance;
using Application.Queries.ObtenerSubasta;
using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Application.Queries.ListarMovimientos;
using Application.Queries.ListarMisPublicaciones;
using Application.Queries.ListarMisPujas;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegá acá el token, así: Bearer {tu token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<SubastaYaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISubastaRepository, SubastaRepository>();
builder.Services.AddScoped<IBilleteraRepository, BilleteraRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ListarCategoriasQueryHandler>();
builder.Services.AddScoped<ListarUsuariosQueryHandler>();
builder.Services.AddScoped<ListarSubastasQueryHandler>();
builder.Services.AddScoped<ObtenerSubastaQueryHandler>();
builder.Services.AddScoped<OfertarCommandHandler>();
builder.Services.AddScoped<ObtenerBalanceQueryHandler>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<CrearSubastaCommandHandler>();
builder.Services.AddHostedService<Infrastructure.Workers.LiquidacionWorker>();
builder.Services.AddScoped<DepositarCommandHandler>();
builder.Services.AddScoped<ModificarSubastaCommandHandler>();
builder.Services.AddScoped<EliminarSubastaCommandHandler>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<ListarPujasQueryHandler>();
builder.Services.AddScoped<ListarMovimientosQueryHandler>();
builder.Services.AddScoped<ListarMisPublicacionesQueryHandler>();
builder.Services.AddScoped<ListarMisPujasQueryHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Falta configurar Jwt:Key.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSection["Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("PermitirFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SubastaYaDbContext>();
    DbSeeder.Seed(db);
}

app.Run();