namespace PictionaryMusicalCliente.Modelo.Cuentas
{
    public class ResultadoInicioSesion
    {
        public bool InicioSesionExitoso { get; set; }

        public bool CuentaNoEncontrada { get; set; }

        public bool ContrasenaIncorrecta { get; set; }

        public string Mensaje { get; set; }

        public UsuarioSesion Usuario { get; set; }
    }
}
