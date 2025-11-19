using System.ComponentModel.DataAnnotations;

namespace EmpresaApi.Models;

public class Contacto
{
    [Key]
    public int ID { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? Correo { get; set; }
}