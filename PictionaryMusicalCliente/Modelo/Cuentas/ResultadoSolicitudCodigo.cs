namespace PictionaryMusicalCliente.Modelo.Cuentas
{
    public class ResultadoSolicitudCodigo
    {
        public bool CodigoEnviado { get; set; }
        public bool UsuarioYaRegistrado { get; set; }
        public bool CorreoYaRegistrado { get; set; }
        public string Mensaje { get; set; }
        public string TokenCodigo { get; set; }
    }
}
