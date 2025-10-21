namespace Servicios.Contratos.DTOs
{
    public class SolicitudAmistadDTO
    {
        public string UsuarioEmisor { get; set; }

        public string UsuarioReceptor { get; set; }

        public bool SolicitudAceptada { get; set; }
    }
}
