using System.Net;
using System.Net.Http.Json;
using EmpresaApi.Data;
using EmpresaApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmpresaApi.Tests;

public class ContactosEndpointsTests
{
    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ContactosDbContext>));
                    
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    
                    // Add InMemory database for testing with a unique name per factory instance
                    var dbName = "TestDb_" + Guid.NewGuid().ToString();
                    services.AddDbContext<ContactosDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(dbName);
                    });
                });
            });
    }

    [Fact]
    public async Task GetContactos_ReturnsEmptyList_WhenNoContactos()
    {
        // Arrange
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/contactos");

        // Assert
        response.EnsureSuccessStatusCode();
        var contactos = await response.Content.ReadFromJsonAsync<List<Contacto>>();
        Assert.NotNull(contactos);
        Assert.Empty(contactos);
    }

    [Fact]
    public async Task GetContactos_ReturnsAllContactos()
    {
        // Arrange
        await using var factory = CreateFactory();
        var client = factory.CreateClient();
        
        // Add some test data
        var contacto1 = new Contacto { Nombre = "Juan", Apellido = "Pérez", Telefono = "1234567890", Correo = "juan@test.com" };
        var contacto2 = new Contacto { Nombre = "María", Apellido = "García", Telefono = "0987654321", Correo = "maria@test.com" };
        
        await client.PostAsJsonAsync("/contactos", contacto1);
        await client.PostAsJsonAsync("/contactos", contacto2);

        // Act
        var response = await client.GetAsync("/contactos");

        // Assert
        response.EnsureSuccessStatusCode();
        var contactos = await response.Content.ReadFromJsonAsync<List<Contacto>>();
        Assert.NotNull(contactos);
        Assert.Equal(2, contactos.Count);
    }

    [Fact]
    public async Task GetContactoById_ReturnsContacto_WhenContactoExists()
    {
        // Arrange
        await using var factory = CreateFactory();
        var client = factory.CreateClient();
        
        var nuevoContacto = new Contacto 
        { 
            Nombre = "Carlos", 
            Apellido = "López", 
            Telefono = "5555555555", 
            Correo = "carlos@test.com" 
        };
        
        var createResponse = await client.PostAsJsonAsync("/contactos", nuevoContacto);
        var createdContacto = await createResponse.Content.ReadFromJsonAsync<Contacto>();

        // Act
        var response = await client.GetAsync($"/contactos/{createdContacto!.ID}");

        // Assert
        response.EnsureSuccessStatusCode();
        var contacto = await response.Content.ReadFromJsonAsync<Contacto>();
        Assert.NotNull(contacto);
        Assert.Equal(nuevoContacto.Nombre, contacto.Nombre);
        Assert.Equal(nuevoContacto.Apellido, contacto.Apellido);
        Assert.Equal(nuevoContacto.Telefono, contacto.Telefono);
        Assert.Equal(nuevoContacto.Correo, contacto.Correo);
    }

    [Fact]
    public async Task GetContactoById_ReturnsNotFound_WhenContactoDoesNotExist()
    {
        // Arrange
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/contactos/9999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostContacto_CreatesNewContacto()
    {
        // Arrange
        await using var factory = CreateFactory();
        var client = factory.CreateClient();
        
        var nuevoContacto = new Contacto 
        { 
            Nombre = "Ana", 
            Apellido = "Martínez", 
            Telefono = "1112223333", 
            Correo = "ana@test.com" 
        };

        // Act
        var response = await client.PostAsJsonAsync("/contactos", nuevoContacto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var contacto = await response.Content.ReadFromJsonAsync<Contacto>();
        Assert.NotNull(contacto);
        Assert.True(contacto.ID > 0);
        Assert.Equal(nuevoContacto.Nombre, contacto.Nombre);
        Assert.Equal(nuevoContacto.Apellido, contacto.Apellido);
        Assert.Equal(nuevoContacto.Telefono, contacto.Telefono);
        Assert.Equal(nuevoContacto.Correo, contacto.Correo);
        Assert.Contains($"/contactos/{contacto.ID}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task PutContacto_UpdatesExistingContacto()
    {
        // Arrange
        await using var factory = CreateFactory();
        var client = factory.CreateClient();
        
        var nuevoContacto = new Contacto 
        { 
            Nombre = "Pedro", 
            Apellido = "Rodríguez", 
            Telefono = "4445556666", 
            Correo = "pedro@test.com" 
        };
        
        var createResponse = await client.PostAsJsonAsync("/contactos", nuevoContacto);
        var createdContacto = await createResponse.Content.ReadFromJsonAsync<Contacto>();

        // Modify the contact
        createdContacto!.Nombre = "Pedro Actualizado";
        createdContacto.Apellido = "Rodríguez Actualizado";
        createdContacto.Telefono = "9998887777";
        createdContacto.Correo = "pedro.updated@test.com";

        // Act
        var response = await client.PutAsJsonAsync($"/contactos/{createdContacto.ID}", createdContacto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the update
        var getResponse = await client.GetAsync($"/contactos/{createdContacto.ID}");
        var updatedContacto = await getResponse.Content.ReadFromJsonAsync<Contacto>();
        Assert.NotNull(updatedContacto);
        Assert.Equal("Pedro Actualizado", updatedContacto.Nombre);
        Assert.Equal("Rodríguez Actualizado", updatedContacto.Apellido);
        Assert.Equal("9998887777", updatedContacto.Telefono);
        Assert.Equal("pedro.updated@test.com", updatedContacto.Correo);
    }

    [Fact]
    public async Task PutContacto_ReturnsBadRequest_WhenIdMismatch()
    {
        // Arrange
        await using var factory = CreateFactory();
        var client = factory.CreateClient();
        
        var contacto = new Contacto 
        { 
            ID = 1,
            Nombre = "Test", 
            Apellido = "User", 
            Telefono = "1234567890", 
            Correo = "test@test.com" 
        };

        // Act - Try to update with a different ID in the URL
        var response = await client.PutAsJsonAsync("/contactos/999", contacto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutContacto_ReturnsNotFound_WhenContactoDoesNotExist()
    {
        // Arrange
        await using var factory = CreateFactory();
        var client = factory.CreateClient();
        
        var contacto = new Contacto 
        { 
            ID = 9999,
            Nombre = "Test", 
            Apellido = "User", 
            Telefono = "1234567890", 
            Correo = "test@test.com" 
        };

        // Act
        var response = await client.PutAsJsonAsync("/contactos/9999", contacto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteContacto_RemovesContacto_WhenContactoExists()
    {
        // Arrange
        await using var factory = CreateFactory();
        var client = factory.CreateClient();
        
        var nuevoContacto = new Contacto 
        { 
            Nombre = "Luis", 
            Apellido = "Fernández", 
            Telefono = "7778889999", 
            Correo = "luis@test.com" 
        };
        
        var createResponse = await client.PostAsJsonAsync("/contactos", nuevoContacto);
        var createdContacto = await createResponse.Content.ReadFromJsonAsync<Contacto>();

        // Act
        var response = await client.DeleteAsync($"/contactos/{createdContacto!.ID}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deletion
        var getResponse = await client.GetAsync($"/contactos/{createdContacto.ID}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteContacto_ReturnsNotFound_WhenContactoDoesNotExist()
    {
        // Arrange
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/contactos/9999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
