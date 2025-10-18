namespace PictionaryMusicalCliente.Modelo.Cuentas
{
    public class ResultadoRegistroCuenta
    {
        public bool RegistroExitoso { get; set; }

        public bool UsuarioYaRegistrado { get; set; }

        public bool CorreoYaRegistrado { get; set; }

        public string Mensaje { get; set; }
    }
}
