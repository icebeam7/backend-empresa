using EmpresaApi.Data;
using EmpresaApi.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ContactosDbContext>(options =>
	options.UseSqlite(connectionString));

var app = builder.Build();

// Crear DB si no existe
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<ContactosDbContext>();
	db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.MapGet("/", () => "API Empresa - Contactos");

// Mapear endpoints Contactos
app.MapContactosEndpoints();

app.Run();

// Make the Program class accessible for integration tests
public partial class Program { }
