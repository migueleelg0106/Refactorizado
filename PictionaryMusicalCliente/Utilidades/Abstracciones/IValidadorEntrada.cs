using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Utilidades
{
    /// <summary>
    /// Define el contrato para validar la entrada de datos del usuario.
    /// </summary>
    public interface IValidadorEntrada
    {
        /// <summary>
        /// Valida que el nombre de usuario no este vacio.
        /// </summary>
        DTOs.ResultadoOperacionDTO ValidarUsuario(string usuario);

        /// <summary>
        /// Valida que el nombre real no este vacio.
        /// </summary>
        DTOs.ResultadoOperacionDTO ValidarNombre(string nombre);

        /// <summary>
        /// Valida que el apellido no este vacio.
        /// </summary>
        DTOs.ResultadoOperacionDTO ValidarApellido(string apellido);

        /// <summary>
        /// Valida el formato y presencia del correo electronico.
        /// </summary>
        DTOs.ResultadoOperacionDTO ValidarCorreo(string correo);

        /// <summary>
        /// Valida que la contrasena cumpla con los requisitos de seguridad.
        /// </summary>
        DTOs.ResultadoOperacionDTO ValidarContrasena(string contrasena);
    }
}