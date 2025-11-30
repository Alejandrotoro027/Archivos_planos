namespace Archivos_planos.Models
{
    public class Usuario
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public int IntentosFallidos { get; set; } = 0;
    }
}