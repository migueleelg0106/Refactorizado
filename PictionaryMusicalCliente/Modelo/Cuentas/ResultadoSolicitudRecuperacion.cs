namespace PictionaryMusicalCliente.Modelo.Cuentas
{
    public class ResultadoSolicitudRecuperacion
    {
        public bool CuentaEncontrada { get; set; }

        public bool CodigoEnviado { get; set; }

        public string CorreoDestino { get; set; }

        public string Mensaje { get; set; }

        public string TokenCodigo { get; set; }
    }
}
