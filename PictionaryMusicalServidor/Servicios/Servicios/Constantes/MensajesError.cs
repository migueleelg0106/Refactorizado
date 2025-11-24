namespace PictionaryMusicalServidor.Servicios.Servicios.Constantes
{
    /// <summary>
    /// Mensajes de error centralizados para evitar repetición de cadenas de texto.
    /// Separados en mensajes para el cliente (informativos) y mensajes para el log (detallados).
    /// </summary>
    internal static class MensajesError
    {
        /// <summary>
        /// Mensajes informativos para el cliente.
        /// Deben ser claros y útiles sin exponer detalles técnicos internos.
        /// </summary>
        public static class Cliente
        {
            public const string ErrorInesperado = "Ocurrió un error inesperado. Por favor, intente nuevamente.";
            public const string OperacionExitosa = "La operación se completó correctamente.";

            public const string DatosInvalidos = "Los datos proporcionados no son válidos. Por favor, verifique la información.";
            public const string CredencialesInvalidas = "Las credenciales proporcionadas no son válidas.";
            public const string CredencialesIncorrectas = "Usuario o contraseña incorrectos.";

            public const string UsuarioNoEncontrado = "No se encontró el usuario especificado.";
            public const string UsuariosNoEncontrados = "No se encontraron todos los usuarios especificados.";
            public const string JugadorNoEncontrado = "No se encontró la información del jugador.";
            public const string UsuariosEspecificadosNoExisten = "Alguno de los usuarios especificados no existe.";
            public const string JugadorNoAsociado = "No existe un jugador asociado al usuario especificado.";
            public const string CuentaNoVerificada = "La cuenta no ha sido verificada. Por favor, verifique su correo.";
            public const string CuentaNoEncontrada = "No se encontró una cuenta con los datos proporcionados.";
            public const string AvatarInvalido = "El avatar seleccionado no es válido.";

            public const string ErrorRegistrarCuenta = "No fue posible completar el registro. Por favor, intente nuevamente.";
            public const string ErrorInicioSesion = "No fue posible iniciar sesión. Por favor, intente nuevamente.";
            public const string UsuarioRegistroInvalido = "El nombre de usuario es obligatorio y no debe exceder 50 caracteres.";
            public const string NombreRegistroInvalido = "El nombre es obligatorio y no debe exceder 50 caracteres.";
            public const string ApellidoRegistroInvalido = "El apellido es obligatorio y no debe exceder 50 caracteres.";
            public const string CorreoRegistroInvalido = "El correo electrónico es obligatorio, debe tener un formato válido y no debe exceder 50 caracteres.";
            public const string ContrasenaRegistroInvalida = "La contraseña debe tener entre 8 y 15 caracteres, incluir una letra mayúscula, un número y un carácter especial.";

            public const string ErrorObtenerPerfil = "No fue posible obtener la información del perfil.";
            public const string ErrorActualizarPerfil = "No fue posible actualizar el perfil. Por favor, intente nuevamente.";
            public const string PerfilActualizadoExito = "Perfil actualizado correctamente.";

            public const string ErrorRecuperarSolicitudes = "No fue posible recuperar las solicitudes de amistad.";
            public const string ErrorAlmacenarSolicitud = "No fue posible enviar la solicitud de amistad.";
            public const string ErrorActualizarSolicitud = "No fue posible actualizar la solicitud de amistad.";
            public const string ErrorEliminarAmistad = "No fue posible eliminar la relación de amistad.";
            public const string ErrorRecuperarListaAmigos = "No fue posible recuperar la lista de amigos.";
            public const string ErrorSuscripcionAmigos = "No fue posible suscribirse a las actualizaciones de amigos.";
            public const string ErrorNotificarSolicitud = "No fue posible notificar la actualización de la solicitud de amistad.";
            public const string ErrorNotificarEliminacion = "No fue posible notificar la eliminación de la relación de amistad.";
            public const string SolicitudAmistadMismoUsuario = "No es posible enviarse una solicitud de amistad a sí mismo.";
            public const string RelacionAmistadExistente = "Ya existe una solicitud o relación de amistad entre los usuarios.";
            public const string SolicitudAmistadNoExiste = "No existe una solicitud de amistad entre los usuarios.";
            public const string ErrorAceptarSolicitud = "No fue posible aceptar la solicitud de amistad.";
            public const string SolicitudAmistadYaAceptada = "La solicitud de amistad ya fue aceptada con anterioridad.";
            public const string RelacionAmistadNoExiste = "No existe una relación de amistad entre los usuarios.";

            public const string ErrorRecuperarCuenta = "No fue posible procesar la recuperación de cuenta.";
            public const string ErrorReenviarCodigo = "No fue posible reenviar el código de verificación.";
            public const string ErrorConfirmarCodigo = "No fue posible confirmar el código de verificación.";
            public const string ErrorActualizarContrasena = "No fue posible actualizar la contraseña.";
            public const string DatosRecuperacionInvalidos = "Los datos de recuperación no son válidos.";
            public const string DatosReenvioCodigo = "Los datos para reenviar el código no son válidos.";
            public const string DatosConfirmacionInvalidos = "Los datos de confirmación no son válidos.";
            public const string DatosActualizacionContrasena = "Los datos de actualización no son válidos.";
            public const string ErrorConfirmarCodigoRecuperacion = "No fue posible confirmar el código de recuperación.";
            public const string DatosSolicitudVerificacionInvalidos = "Los datos proporcionados no son válidos para solicitar el código.";
            public const string SolicitudRecuperacionIdentificadorObligatorio = "Debe proporcionar el usuario o correo registrado y no debe exceder 50 caracteres.";
            public const string SolicitudRecuperacionCuentaNoEncontrada = "No se encontró una cuenta con el usuario o correo proporcionado.";
            public const string SolicitudRecuperacionNoEncontrada = "No se encontró una solicitud de recuperación activa.";
            public const string CodigoRecuperacionExpirado = "El código de verificación ha expirado. Solicite uno nuevo.";
            public const string ErrorReenviarCodigoRecuperacion = "No fue posible reenviar el código de recuperación.";
            public const string CodigoRecuperacionIncorrecto = "El código ingresado no es correcto.";
            public const string SolicitudRecuperacionNoVigente = "No hay una solicitud de recuperación vigente.";
            public const string SolicitudRecuperacionInvalida = "La solicitud de recuperación no es válida.";

            public const string ErrorSolicitudVerificacion = "No fue posible procesar la solicitud de verificación.";
            public const string ErrorReenviarCodigoVerificacion = "No fue posible reenviar el código de verificación.";
            public const string SolicitudVerificacionNoEncontrada = "No se encontró una solicitud de verificación activa.";
            public const string CodigoVerificacionExpirado = "El código de verificación ha expirado. Inicie el proceso nuevamente.";
            public const string CodigoVerificacionIncorrecto = "El código ingresado no es correcto.";
            public const string UsuarioOCorreoRegistrado = "El correo o usuario ya está registrado.";

            public const string ErrorCrearSala = "No fue posible crear la sala.";
            public const string ErrorInesperadoCrearSala = "Ocurrió un error al crear la sala.";
            public const string ErrorInesperadoUnirse = "Ocurrió un error al unirse a la sala.";
            public const string ErrorInesperadoAbandonar = "Ocurrió un error al abandonar la sala.";
            public const string ErrorInesperadoExpulsar = "Ocurrió un error al expulsar al jugador.";
            public const string ErrorInesperadoSuscripcion = "Ocurrió un error al suscribirse a las salas.";
            public const string SalaNoEncontrada = "No se encontró la sala especificada.";
            public const string ErrorGenerarCodigo = "No fue posible generar un código para la sala.";
            public const string SalaLlena = "La sala está llena.";
            public const string SalaExpulsionRestringida = "Solo el creador de la sala puede expulsar jugadores.";
            public const string SalaCreadorNoExpulsable = "El creador de la sala no puede ser expulsado.";
            public const string SalaJugadorNoExiste = "El jugador especificado no está en la sala.";

            public const string InvitacionInvalida = "La invitación no es válida.";
            public const string DatosInvitacionInvalidos = "Los datos de la invitación no son válidos.";
            public const string CorreoInvalido = "El correo electrónico no es válido.";
            public const string ErrorEnviarInvitacion = "No fue posible enviar la invitación.";
            public const string ErrorProcesarInvitacion = "Ocurrió un problema al procesar la invitación.";
            public const string ErrorInesperadoInvitacion = "Ocurrió un error al enviar la invitación.";
            public const string JugadorYaEnSala = "El jugador ya está en la sala.";
            public const string InvitacionEnviadaExito = "Invitación enviada correctamente.";
            public const string SolicitudInvitacionInvalida = "La solicitud de invitación no es válida.";
            public const string CorreoJugadorEnSala = "El jugador con el correo ingresado ya está en la sala.";
            public const string ErrorEnviarInvitacionCorreo = "No fue posible enviar la invitación por correo electrónico.";

            public const string ErrorObtenerCallback = "No fue posible establecer la conexión con el servidor.";
            public const string ErrorObtenerCallbackAmigos = "No fue posible establecer la conexión para amigos.";
            public const string ErrorContextoOperacion = "No fue posible establecer el contexto de la operación.";
            public const string ErrorContextoOperacionAmigos = "No fue posible establecer el contexto para amigos.";

            public const string ParametroObligatorio = "El parámetro {0} es obligatorio.";
            public const string NombreUsuarioObligatorio = "El nombre de usuario es obligatorio.";
            public const string NombreUsuarioObligatorioCancelar = "El nombre de usuario es obligatorio para cancelar la suscripción.";
            public const string NombreUsuarioObligatorioSuscripcion = "El nombre de usuario es obligatorio para suscribirse a las notificaciones.";
            public const string CodigoSalaObligatorio = "El código de sala es obligatorio.";

            public const string ConfiguracionObligatoria = "La configuración de la partida es obligatoria.";
            public const string NumeroRondasInvalido = "El número de rondas debe ser mayor a cero.";
            public const string TiempoRondaInvalido = "El tiempo por ronda debe ser mayor a cero.";
            public const string IdiomaObligatorio = "El idioma de las canciones es obligatorio.";
            public const string DificultadObligatoria = "La dificultad es obligatoria.";
        }
    }
}
