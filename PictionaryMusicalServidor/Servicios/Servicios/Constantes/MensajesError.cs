namespace PictionaryMusicalServidor.Servicios.Servicios.Constantes
{
    /// <summary>
    /// Mensajes de error centralizados para evitar repeticion de cadenas de texto.
    /// Separados en mensajes para el cliente (informativos) y mensajes para el log (detallados).
    /// </summary>
    internal static class MensajesError
    {
        /// <summary>
        /// Mensajes informativos para el cliente.
        /// Deben ser claros y utiles sin exponer detalles tecnicos internos.
        /// </summary>
        public static class Cliente
        {
            public const string ErrorInesperado = "Ocurrio un error inesperado. Por favor, intente nuevamente.";
            public const string OperacionExitosa = "La operacion se completo correctamente.";

            public const string ErrorCrearReporte = "No fue posible registrar el reporte.";
            public const string ReporteRegistrado = "Reporte enviado correctamente.";
            public const string ReporteDuplicado = "Ya has reportado a este jugador.";
            public const string ReporteMotivoObligatorio = "El motivo del reporte es obligatorio.";
            public const string ReporteMismoUsuario = "No puedes reportarte a ti mismo.";
            public const string ReporteMotivoLongitud = "El motivo del reporte no debe exceder 100 caracteres.";

            public const string DatosInvalidos = "Los datos proporcionados no son validos. Por favor, verifique la informacion.";
            public const string CredencialesInvalidas = "Las credenciales proporcionadas no son validas.";
            public const string CredencialesIncorrectas = "Usuario o contrasena incorrectos.";

            public const string UsuarioNoEncontrado = "No se encontro el usuario especificado.";
            public const string UsuariosNoEncontrados = "No se encontraron todos los usuarios especificados.";
            public const string JugadorNoEncontrado = "No se encontro la informacion del jugador.";
            public const string UsuariosEspecificadosNoExisten = "Alguno de los usuarios especificados no existe.";
            public const string JugadorNoAsociado = "No existe un jugador asociado al usuario especificado.";
            public const string CuentaNoVerificada = "La cuenta no ha sido verificada. Por favor, verifique su correo.";
            public const string CuentaNoEncontrada = "No se encontro una cuenta con los datos proporcionados.";
            public const string AvatarInvalido = "El avatar seleccionado no es valido.";

            public const string ErrorRegistrarCuenta = "No fue posible completar el registro. Por favor, intente nuevamente.";
            public const string ErrorInicioSesion = "No fue posible iniciar sesion. Por favor, intente nuevamente.";
            public const string UsuarioRegistroInvalido = "El nombre de usuario es obligatorio y no debe exceder 50 caracteres.";
            public const string NombreRegistroInvalido = "El nombre es obligatorio y no debe exceder 50 caracteres.";
            public const string ApellidoRegistroInvalido = "El apellido es obligatorio y no debe exceder 50 caracteres.";
            public const string CorreoRegistroInvalido = "El correo electronico es obligatorio, debe tener un formato valido y no debe exceder 50 caracteres.";
            public const string ContrasenaRegistroInvalida = "La contrasena debe tener entre 8 y 15 caracteres, incluir una letra mayuscula, un numero y un caracter especial.";

            public const string ErrorObtenerPerfil = "No fue posible obtener la informacion del perfil.";
            public const string ErrorActualizarPerfil = "No fue posible actualizar el perfil. Por favor, intente nuevamente.";
            public const string PerfilActualizadoExito = "Perfil actualizado correctamente.";

            public const string ErrorRecuperarSolicitudes = "No fue posible recuperar las solicitudes de amistad.";
            public const string ErrorAlmacenarSolicitud = "No fue posible enviar la solicitud de amistad.";
            public const string ErrorActualizarSolicitud = "No fue posible actualizar la solicitud de amistad.";
            public const string ErrorEliminarAmistad = "No fue posible eliminar la relacion de amistad.";
            public const string ErrorRecuperarListaAmigos = "No fue posible recuperar la lista de amigos.";
            public const string ErrorSuscripcionAmigos = "No fue posible suscribirse a las actualizaciones de amigos.";
            public const string ErrorNotificarSolicitud = "No fue posible notificar la actualizacion de la solicitud de amistad.";
            public const string ErrorNotificarEliminacion = "No fue posible notificar la eliminacion de la relacion de amistad.";
            public const string SolicitudAmistadMismoUsuario = "No es posible enviarse una solicitud de amistad a si mismo.";
            public const string RelacionAmistadExistente = "Ya existe una solicitud o relacion de amistad entre los usuarios.";
            public const string SolicitudAmistadNoExiste = "No existe una solicitud de amistad entre los usuarios.";
            public const string ErrorAceptarSolicitud = "No fue posible aceptar la solicitud de amistad.";
            public const string SolicitudAmistadYaAceptada = "La solicitud de amistad ya fue aceptada con anterioridad.";
            public const string RelacionAmistadNoExiste = "No existe una relacion de amistad entre los usuarios.";

            public const string ErrorRecuperarCuenta = "No fue posible procesar la recuperacion de cuenta.";
            public const string ErrorReenviarCodigo = "No fue posible reenviar el codigo de verificacion.";
            public const string ErrorConfirmarCodigo = "No fue posible confirmar el codigo de verificacion.";
            public const string ErrorActualizarContrasena = "No fue posible actualizar la contrasena.";
            public const string DatosRecuperacionInvalidos = "Los datos de recuperacion no son validos.";
            public const string DatosReenvioCodigo = "Los datos para reenviar el codigo no son validos.";
            public const string DatosConfirmacionInvalidos = "Los datos de confirmacion no son validos.";
            public const string DatosActualizacionContrasena = "Los datos de actualizacion no son validos.";
            public const string ErrorConfirmarCodigoRecuperacion = "No fue posible confirmar el codigo de recuperacion.";
            public const string DatosSolicitudVerificacionInvalidos = "Los datos proporcionados no son validos para solicitar el codigo.";
            public const string SolicitudRecuperacionIdentificadorObligatorio = "Debe proporcionar el usuario o correo registrado y no debe exceder 50 caracteres.";
            public const string SolicitudRecuperacionCuentaNoEncontrada = "No se encontro una cuenta con el usuario o correo proporcionado.";
            public const string SolicitudRecuperacionNoEncontrada = "No se encontro una solicitud de recuperacion activa.";
            public const string CodigoRecuperacionExpirado = "El codigo de verificacion ha expirado. Solicite uno nuevo.";
            public const string ErrorReenviarCodigoRecuperacion = "No fue posible reenviar el codigo de recuperacion.";
            public const string CodigoRecuperacionIncorrecto = "El codigo ingresado no es correcto.";
            public const string SolicitudRecuperacionNoVigente = "No hay una solicitud de recuperacion vigente.";
            public const string SolicitudRecuperacionInvalida = "La solicitud de recuperacion no es valida.";

            public const string ErrorSolicitudVerificacion = "No fue posible procesar la solicitud de verificacion.";
            public const string ErrorReenviarCodigoVerificacion = "No fue posible reenviar el codigo de verificacion.";
            public const string SolicitudVerificacionNoEncontrada = "No se encontro una solicitud de verificacion activa.";
            public const string CodigoVerificacionExpirado = "El codigo de verificacion ha expirado. Inicie el proceso nuevamente.";
            public const string CodigoVerificacionIncorrecto = "El codigo ingresado no es correcto.";
            public const string UsuarioOCorreoRegistrado = "El correo o usuario ya esta registrado.";

            public const string UsuarioBaneadoPorReportes = "Has sido baneado del juego por mala conducta.";

            public const string ErrorCrearSala = "No fue posible crear la sala.";
            public const string ErrorInesperadoCrearSala = "Ocurrio un error al crear la sala.";
            public const string ErrorInesperadoUnirse = "Ocurrio un error al unirse a la sala.";
            public const string ErrorInesperadoAbandonar = "Ocurrio un error al abandonar la sala.";
            public const string ErrorInesperadoExpulsar = "Ocurrio un error al expulsar al jugador.";
            public const string ErrorInesperadoSuscripcion = "Ocurrio un error al suscribirse a las salas.";
            public const string SalaNoEncontrada = "No se encontro la sala especificada.";
            public const string ErrorGenerarCodigo = "No fue posible generar un codigo para la sala.";
            public const string SalaLlena = "La sala esta llena.";
            public const string SalaExpulsionRestringida = "Solo el creador de la sala puede expulsar jugadores.";
            public const string SalaCreadorNoExpulsable = "El creador de la sala no puede ser expulsado.";
            public const string SalaJugadorNoExiste = "El jugador especificado no esta en la sala.";

            public const string InvitacionInvalida = "La invitacion no es valida.";
            public const string DatosInvitacionInvalidos = "Los datos de la invitacion no son validos.";
            public const string CorreoInvalido = "El correo electronico no es valido.";
            public const string ErrorEnviarInvitacion = "No fue posible enviar la invitacion.";
            public const string ErrorProcesarInvitacion = "Ocurrio un problema al procesar la invitacion.";
            public const string ErrorInesperadoInvitacion = "Ocurrio un error al enviar la invitacion.";
            public const string JugadorYaEnSala = "El jugador ya esta en la sala.";
            public const string InvitacionEnviadaExito = "Invitacion enviada correctamente.";
            public const string SolicitudInvitacionInvalida = "La solicitud de invitacion no es valida.";
            public const string CorreoJugadorEnSala = "El jugador con el correo ingresado ya esta en la sala.";
            public const string ErrorEnviarInvitacionCorreo = "No fue posible enviar la invitacion por correo electronico.";

            public const string ErrorObtenerCallback = "No fue posible establecer la conexion con el servidor.";
            public const string ErrorObtenerCallbackAmigos = "No fue posible establecer la conexion para amigos.";
            public const string ErrorContextoOperacion = "No fue posible establecer el contexto de la operacion.";
            public const string ErrorContextoOperacionAmigos = "No fue posible establecer el contexto para amigos.";

            public const string ParametroObligatorio = "El parametro {0} es obligatorio.";
            public const string NombreUsuarioObligatorio = "El nombre de usuario es obligatorio.";
            public const string NombreUsuarioObligatorioCancelar = "El nombre de usuario es obligatorio para cancelar la suscripcion.";
            public const string NombreUsuarioObligatorioSuscripcion = "El nombre de usuario es obligatorio para suscribirse a las notificaciones.";
            public const string CodigoSalaObligatorio = "El codigo de sala es obligatorio.";

            public const string ConfiguracionObligatoria = "La configuracion de la partida es obligatoria.";
            public const string NumeroRondasInvalido = "El numero de rondas debe ser mayor a cero.";
            public const string TiempoRondaInvalido = "El tiempo por ronda debe ser mayor a cero.";
            public const string IdiomaObligatorio = "El idioma de las canciones es obligatorio.";
            public const string DificultadObligatoria = "La dificultad es obligatoria.";

            public const string PartidaCanceladaFaltaJugadores = "Partida cancelada por falta de jugadores.";
            public const string PartidaCanceladaHostSalio = "El anfitrion de la sala abandono la partida.";
            public const string PartidaYaIniciada = "Partida ya iniciada.";
            public const string PartidaComenzo = "La partida ya comenzo";
            public const string FaltanJugadores = "Faltan jugadores.";
            public const string SoloHost = "Solo Host.";
        }
    }
}