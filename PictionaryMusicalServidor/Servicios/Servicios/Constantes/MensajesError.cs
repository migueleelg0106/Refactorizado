namespace Servicios.Servicios.Constantes
{
    /// <summary>
    /// Mensajes de error centralizados para evitar repetición de cadenas de texto.
    /// </summary>
    internal static class MensajesError
    {
        // Mensajes de base de datos
        public const string ErrorBaseDatos = "No fue posible completar la operación debido a un problema con la base de datos.";
        public const string ErrorBaseDatosConsulta = "Ocurrió un problema al consultar la información.";
        public const string ErrorBaseDatosActualizacion = "No fue posible actualizar la información en la base de datos.";
        public const string ErrorBaseDatosAlmacenamiento = "No fue posible almacenar la información.";

        // Mensajes de validación
        public const string DatosInvalidos = "Los datos proporcionados no son válidos.";
        public const string CredencialesInvalidas = "Credenciales inválidas";
        public const string CredencialesIncorrectas = "Credenciales incorrectas.";

        // Mensajes de usuario
        public const string UsuarioNoEncontrado = "No se encontró el usuario especificado.";
        public const string UsuariosNoEncontrados = "Alguno de los usuarios especificados no existe.";
        public const string JugadorNoEncontrado = "No existe un jugador asociado al usuario especificado.";

        // Mensajes de cuenta
        public const string CuentaNoVerificada = "La cuenta no ha sido verificada.";
        public const string AvatarInvalido = "Avatar no válido.";
        public const string CuentaNoEncontrada = "No se encontró una cuenta con el usuario o correo proporcionado.";

        // Mensajes de perfil
        public const string ErrorObtenerPerfil = "Ocurrió un problema al consultar la información del perfil.";
        public const string ErrorActualizarPerfil = "No fue posible actualizar el perfil.";
        public const string PerfilActualizadoExito = "Perfil actualizado correctamente.";

        // Mensajes de amistad
        public const string ErrorRecuperarSolicitudes = "No fue posible recuperar las solicitudes de amistad.";
        public const string ErrorAlmacenarSolicitud = "No fue posible almacenar la solicitud de amistad.";
        public const string ErrorActualizarSolicitud = "No fue posible actualizar la solicitud de amistad.";
        public const string ErrorEliminarAmistad = "No fue posible eliminar la relación de amistad en la base de datos.";
        public const string ErrorRecuperarListaAmigos = "No fue posible recuperar la lista de amigos.";
        public const string ErrorSuscripcionAmigos = "No fue posible suscribirse a la lista de amigos.";

        // Mensajes de recuperación de cuenta
        public const string ErrorRecuperarCuenta = "No fue posible procesar la recuperación de la cuenta.";
        public const string ErrorReenviarCodigo = "No fue posible reenviar el código de recuperación.";
        public const string ErrorConfirmarCodigo = "No fue posible confirmar el código de recuperación.";
        public const string ErrorActualizarContrasena = "No fue posible actualizar la contraseña.";
        public const string DatosRecuperacionInvalidos = "Los datos proporcionados no son válidos para recuperar la cuenta.";
        public const string DatosReenvioCodigo = "Los datos proporcionados no son válidos para reenviar el código.";
        public const string DatosConfirmacionInvalidos = "Los datos proporcionados no son válidos para confirmar el código.";
        public const string DatosActualizacionContrasena = "Los datos proporcionados no son válidos para actualizar la contraseña.";

        // Mensajes de verificación de registro
        public const string ErrorSolicitudVerificacion = "No fue posible procesar la solicitud de verificación.";
        public const string ErrorReenviarCodigoVerificacion = "No fue posible reenviar el código de verificación.";

        // Mensajes de salas
        public const string ErrorCrearSala = "No se pudo crear la sala.";
        public const string ErrorInesperadoCrearSala = "Ocurrió un error inesperado al crear la sala.";
        public const string ErrorInesperadoUnirse = "Ocurrió un error inesperado al unirse a la sala.";
        public const string ErrorInesperadoAbandonar = "Ocurrió un error inesperado al abandonar la sala.";
        public const string ErrorInesperadoExpulsar = "Ocurrió un error inesperado al expulsar al jugador.";
        public const string ErrorInesperadoSuscripcion = "Ocurrió un error inesperado al suscribirse a la lista de salas.";
        public const string SalaNoEncontrada = "No se encontró la sala especificada.";
        public const string ErrorGenerarCodigo = "No se pudo generar un código único para la sala.";

        // Mensajes de invitaciones
        public const string InvitacionInvalida = "La solicitud de invitación no es válida.";
        public const string DatosInvitacionInvalidos = "Los datos proporcionados no son válidos para enviar la invitación.";
        public const string CorreoInvalido = "El correo electrónico proporcionado no es válido.";
        public const string ErrorEnviarInvitacion = "No fue posible enviar la invitación por correo electrónico.";
        public const string ErrorProcesarInvitacion = "Ocurrió un problema al procesar la invitación.";
        public const string ErrorInesperadoInvitacion = "No fue posible enviar la invitación por un error inesperado.";
        public const string JugadorYaEnSala = "El jugador con el correo ingresado ya está en la sala.";
        public const string InvitacionEnviadaExito = "Invitación enviada correctamente.";

        // Mensajes de callbacks
        public const string ErrorObtenerCallback = "No se pudo obtener el canal de retorno para el usuario.";
        public const string ErrorObtenerCallbackAmigos = "No se pudo obtener el canal de retorno para la lista de amigos.";
        public const string ErrorContextoOperacion = "No se pudo obtener el contexto de la operación para suscribir el callback.";
        public const string ErrorContextoOperacionAmigos = "No se pudo obtener el contexto de la operación para suscribirse a la lista de amigos.";

        // Mensajes de parámetros
        public const string ParametroObligatorio = "El parámetro {0} es obligatorio.";
        public const string NombreUsuarioObligatorio = "El nombre de usuario es obligatorio para suscribirse a las notificaciones.";
        public const string NombreUsuarioObligatorioCancelar = "El nombre de usuario es obligatorio para cancelar la suscripción.";
        public const string CodigoSalaObligatorio = "El parámetro codigoSala es obligatorio.";

        // Mensajes de configuración de partida
        public const string ConfiguracionObligatoria = "La configuración de la partida es obligatoria.";
        public const string NumeroRondasInvalido = "El número de rondas debe ser mayor a cero.";
        public const string TiempoRondaInvalido = "El tiempo por ronda debe ser mayor a cero.";
        public const string IdiomaObligatorio = "El idioma de las canciones es obligatorio.";
        public const string DificultadObligatoria = "La dificultad es obligatoria.";

        // Mensajes de comunicación
        public const string ErrorComunicacion = "Error de comunicación con el cliente.";
        public const string ErrorTimeout = "Tiempo de espera agotado al comunicarse con el cliente.";

        // Mensajes de inicio de sesión
        public const string ErrorInicioSesion = "No fue posible iniciar sesión debido a un problema del sistema.";

        // Mensajes de registro
        public const string ErrorRegistrarCuenta = "No fue posible completar el registro de la cuenta.";
    }
}
