using Api.Middleware;
using Application.Commands.CrearSubasta;
using Application.Commands.Depositar;
using Application.Commands.EliminarSubasta;
using Application.Commands.ModificarSubasta;
using Application.Commands.Ofertar;
using Application.Interfaces;
using Application.Queries.ListarSubastas;
using Application.Queries.ObtenerBalance;
using Application.Queries.ObtenerSubasta;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SubastaYaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISubastaRepository, SubastaRepository>();
builder.Services.AddScoped<IBilleteraRepository, BilleteraRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SubastaYaDbContext>();
    DbSeeder.Seed(db);
}

app.Run();