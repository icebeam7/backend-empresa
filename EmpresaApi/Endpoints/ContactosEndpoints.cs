using EmpresaApi.Data;
using EmpresaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmpresaApi.Endpoints;

public static class ContactosEndpoints
{
    public static IEndpointRouteBuilder MapContactosEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/contactos").WithTags("Contactos");

        // Obtener todos
        group.MapGet("/", async (ContactosDbContext db) =>
            await db.Contactos.AsNoTracking().ToListAsync()
        );

        // Obtener por ID
        group.MapGet("/{id:int}", async (int id, ContactosDbContext db) =>
        {
            var contacto = await db.Contactos.AsNoTracking().FirstOrDefaultAsync(c => c.ID == id);
            return contacto is not null ? Results.Ok(contacto) : Results.NotFound();
        });

        // Agregar
        group.MapPost("/", async (Contacto nuevo, ContactosDbContext db) =>
        {
            db.Contactos.Add(nuevo);
            await db.SaveChangesAsync();
            return Results.Created($"/contactos/{nuevo.ID}", nuevo);
        });

        // Actualizar
        group.MapPut("/{id:int}", async (int id, Contacto entrada, ContactosDbContext db) =>
        {
            var existente = await db.Contactos.FirstOrDefaultAsync(c => c.ID == id);
            if (existente is null) return Results.NotFound();

            existente.Nombre = entrada.Nombre;
            existente.Apellido = entrada.Apellido;
            existente.Telefono = entrada.Telefono;
            existente.Correo = entrada.Correo;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // Eliminar
        group.MapDelete("/{id:int}", async (int id, ContactosDbContext db) =>
        {
            var existente = await db.Contactos.FirstOrDefaultAsync(c => c.ID == id);
            if (existente is null) return Results.NotFound();
            db.Contactos.Remove(existente);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return routes;
    }
}