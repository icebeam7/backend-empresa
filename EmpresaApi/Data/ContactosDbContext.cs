using EmpresaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmpresaApi.Data;

public class ContactosDbContext : DbContext
{
    public ContactosDbContext(DbContextOptions<ContactosDbContext> options) : base(options)
    {
    }

    public DbSet<Contacto> Contactos => Set<Contacto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Contacto>().ToTable("Contactos");
    }
}