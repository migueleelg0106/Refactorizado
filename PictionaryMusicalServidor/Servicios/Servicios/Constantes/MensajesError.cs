namespace Servicios.Servicios.Constantes
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
            // Mensajes generales
            public const string ErrorInesperado = "Ocurrió un error inesperado. Por favor, intente nuevamente.";
            public const string OperacionExitosa = "La operación se completó correctamente.";

            // Mensajes de validación
            public const string DatosInvalidos = "Los datos proporcionados no son válidos. Por favor, verifique la información.";
            public const string CredencialesInvalidas = "Las credenciales proporcionadas no son válidas.";
            public const string CredencialesIncorrectas = "Usuario o contraseña incorrectos.";

            // Mensajes de usuario y cuenta
            public const string UsuarioNoEncontrado = "No se encontró el usuario especificado.";
            public const string UsuariosNoEncontrados = "No se encontraron todos los usuarios especificados.";
            public const string JugadorNoEncontrado = "No se encontró la información del jugador.";
            public const string CuentaNoVerificada = "La cuenta no ha sido verificada. Por favor, verifique su correo.";
            public const string CuentaNoEncontrada = "No se encontró una cuenta con los datos proporcionados.";
            public const string AvatarInvalido = "El avatar seleccionado no es válido.";

            // Mensajes de registro y autenticación
            public const string ErrorRegistrarCuenta = "No fue posible completar el registro. Por favor, intente nuevamente.";
            public const string ErrorInicioSesion = "No fue posible iniciar sesión. Por favor, intente nuevamente.";

            // Mensajes de perfil
            public const string ErrorObtenerPerfil = "No fue posible obtener la información del perfil.";
            public const string ErrorActualizarPerfil = "No fue posible actualizar el perfil. Por favor, intente nuevamente.";
            public const string PerfilActualizadoExito = "Perfil actualizado correctamente.";

            // Mensajes de amistad
            public const string ErrorRecuperarSolicitudes = "No fue posible recuperar las solicitudes de amistad.";
            public const string ErrorAlmacenarSolicitud = "No fue posible enviar la solicitud de amistad.";
            public const string ErrorActualizarSolicitud = "No fue posible actualizar la solicitud de amistad.";
            public const string ErrorEliminarAmistad = "No fue posible eliminar la relación de amistad.";
            public const string ErrorRecuperarListaAmigos = "No fue posible recuperar la lista de amigos.";
            public const string ErrorSuscripcionAmigos = "No fue posible suscribirse a las actualizaciones de amigos.";

            // Mensajes de recuperación de cuenta
            public const string ErrorRecuperarCuenta = "No fue posible procesar la recuperación de cuenta.";
            public const string ErrorReenviarCodigo = "No fue posible reenviar el código de verificación.";
            public const string ErrorConfirmarCodigo = "No fue posible confirmar el código de verificación.";
            public const string ErrorActualizarContrasena = "No fue posible actualizar la contraseña.";
            public const string DatosRecuperacionInvalidos = "Los datos de recuperación no son válidos.";
            public const string DatosReenvioCodigo = "Los datos para reenviar el código no son válidos.";
            public const string DatosConfirmacionInvalidos = "Los datos de confirmación no son válidos.";
            public const string DatosActualizacionContrasena = "Los datos de actualización no son válidos.";

            // Mensajes de verificación de registro
            public const string ErrorSolicitudVerificacion = "No fue posible procesar la solicitud de verificación.";
            public const string ErrorReenviarCodigoVerificacion = "No fue posible reenviar el código de verificación.";

            // Mensajes de salas
            public const string ErrorCrearSala = "No fue posible crear la sala.";
            public const string ErrorInesperadoCrearSala = "Ocurrió un error al crear la sala.";
            public const string ErrorInesperadoUnirse = "Ocurrió un error al unirse a la sala.";
            public const string ErrorInesperadoAbandonar = "Ocurrió un error al abandonar la sala.";
            public const string ErrorInesperadoExpulsar = "Ocurrió un error al expulsar al jugador.";
            public const string ErrorInesperadoSuscripcion = "Ocurrió un error al suscribirse a las salas.";
            public const string SalaNoEncontrada = "No se encontró la sala especificada.";
            public const string ErrorGenerarCodigo = "No fue posible generar un código para la sala.";

            // Mensajes de invitaciones
            public const string InvitacionInvalida = "La invitación no es válida.";
            public const string DatosInvitacionInvalidos = "Los datos de la invitación no son válidos.";
            public const string CorreoInvalido = "El correo electrónico no es válido.";
            public const string ErrorEnviarInvitacion = "No fue posible enviar la invitación.";
            public const string ErrorProcesarInvitacion = "Ocurrió un problema al procesar la invitación.";
            public const string ErrorInesperadoInvitacion = "Ocurrió un error al enviar la invitación.";
            public const string JugadorYaEnSala = "El jugador ya está en la sala.";
            public const string InvitacionEnviadaExito = "Invitación enviada correctamente.";

            // Mensajes de callbacks y comunicación
            public const string ErrorObtenerCallback = "No fue posible establecer la conexión con el servidor.";
            public const string ErrorObtenerCallbackAmigos = "No fue posible establecer la conexión para amigos.";
            public const string ErrorContextoOperacion = "No fue posible establecer el contexto de la operación.";
            public const string ErrorContextoOperacionAmigos = "No fue posible establecer el contexto para amigos.";

            // Mensajes de parámetros
            public const string ParametroObligatorio = "El parámetro {0} es obligatorio.";
            public const string NombreUsuarioObligatorio = "El nombre de usuario es obligatorio.";
            public const string NombreUsuarioObligatorioCancelar = "El nombre de usuario es obligatorio para cancelar la suscripción.";
            public const string CodigoSalaObligatorio = "El código de sala es obligatorio.";

            // Mensajes de configuración de partida
            public const string ConfiguracionObligatoria = "La configuración de la partida es obligatoria.";
            public const string NumeroRondasInvalido = "El número de rondas debe ser mayor a cero.";
            public const string TiempoRondaInvalido = "El tiempo por ronda debe ser mayor a cero.";
            public const string IdiomaObligatorio = "El idioma de las canciones es obligatorio.";
            public const string DificultadObligatoria = "La dificultad es obligatoria.";
        }

        /// <summary>
        /// Mensajes detallados para el log.
        /// Deben incluir información técnica específica para facilitar el diagnóstico.
        /// </summary>
        public static class Log
        {
            // Mensajes de base de datos - Registro de cuenta
            public const string RegistroCuentaValidacionEntidad = "Validación de entidad fallida durante el registro de cuenta. Los datos de la entidad no cumplen con las reglas de validación.";
            public const string RegistroCuentaActualizacionBD = "Error de actualización de base de datos durante el registro de cuenta. Posible conflicto de concurrencia o restricción violada.";
            public const string RegistroCuentaErrorBD = "Error de base de datos durante el registro de cuenta. Fallo en la conexión o ejecución de consulta SQL.";
            public const string RegistroCuentaErrorDatos = "Error de datos durante el registro de cuenta. Los datos no se pudieron procesar correctamente.";
            public const string RegistroCuentaOperacionInvalida = "Operación inválida durante el registro de cuenta. El estado del contexto no permite la operación.";

            // Mensajes de base de datos - Inicio de sesión
            public const string InicioSesionErrorBD = "Error de base de datos durante el inicio de sesión. Fallo en la consulta de usuario.";
            public const string InicioSesionErrorDatos = "Error de datos durante el inicio de sesión. Los datos del usuario no se pudieron recuperar.";
            public const string InicioSesionOperacionInvalida = "Operación inválida durante el inicio de sesión. Estado inconsistente del contexto.";

            // Mensajes de base de datos - Clasificación
            public const string ClasificacionErrorBD = "Error de base de datos al obtener la clasificación. Fallo en la consulta de jugadores.";
            public const string ClasificacionErrorDatos = "Error de datos al obtener la clasificación. Los datos de clasificación no se pudieron procesar.";
            public const string ClasificacionOperacionInvalida = "Operación inválida al obtener la clasificación. Secuencia de operaciones incorrecta.";

            // Mensajes de salas - Operaciones
            public const string SalaCrearOperacionInvalida = "Operación inválida al crear sala. El estado del sistema no permite crear más salas o los datos son inconsistentes.";
            public const string SalaCrearComunicacion = "Error de comunicación WCF al crear sala. El canal de callback no está disponible o falló.";
            public const string SalaUnirseOperacionInvalida = "Operación inválida al unirse a sala. La sala puede estar llena o el usuario ya está en otra sala.";
            public const string SalaUnirseComunicacion = "Error de comunicación WCF al unirse a sala. Fallo en el canal de callback del cliente.";
            public const string SalaAbandonarOperacionInvalida = "Operación inválida al abandonar sala. El usuario no está en la sala o la sala ya no existe.";
            public const string SalaExpulsarOperacionInvalida = "Operación inválida al expulsar jugador. El usuario no tiene permisos o el jugador no está en la sala.";
            public const string SalaObtenerListaOperacionInvalida = "Operación inválida al obtener lista de salas. Error en la enumeración de salas activas.";
            public const string SalaSuscripcionOperacionInvalida = "Operación inválida al suscribirse a lista de salas. No se pudo obtener el canal de callback.";
            public const string SalaSuscripcionComunicacion = "Error de comunicación WCF al suscribirse a lista de salas. Fallo en la obtención del canal de callback.";
            public const string SalaCancelarSuscripcionOperacionInvalida = "Operación inválida al cancelar suscripción a lista de salas. El callback no está registrado.";
            public const string SalaCancelarSuscripcionComunicacion = "Error de comunicación WCF al cancelar suscripción. Fallo al obtener el canal de callback.";

            // Mensajes de invitaciones
            public const string InvitacionErrorBD = "Error de base de datos al enviar invitación. Fallo en la consulta de verificación de usuario.";
            public const string InvitacionErrorDatos = "Error de datos al enviar invitación. No se pudo procesar la información del destinatario.";
            public const string InvitacionOperacionInvalida = "Operación inválida al enviar invitación. Estado inconsistente o validación fallida.";

            // Mensajes de correo electrónico
            public const string CorreoSmtp = "Error SMTP al enviar correo electrónico. Fallo en la conexión o autenticación con el servidor de correo.";
            public const string CorreoOperacionInvalida = "Operación inválida al enviar correo. Configuración de SMTP incorrecta o estado del cliente inválido.";
            public const string CorreoArgumentoInvalido = "Argumentos inválidos para enviar correo. Dirección de email, asunto o cuerpo del mensaje incorrectos.";

            // Mensajes de perfil
            public const string PerfilObtenerErrorBD = "Error de base de datos al obtener perfil. Fallo en la consulta de información de usuario.";
            public const string PerfilObtenerErrorDatos = "Error de datos al obtener perfil. Los datos del perfil no se pudieron recuperar correctamente.";
            public const string PerfilObtenerOperacionInvalida = "Operación inválida al obtener perfil. Estado inconsistente del contexto de datos.";
            public const string PerfilActualizarValidacionEntidad = "Validación de entidad fallida al actualizar perfil. Los datos no cumplen con las restricciones.";
            public const string PerfilActualizarActualizacionBD = "Error de actualización de base de datos al actualizar perfil. Conflicto de concurrencia detectado.";
            public const string PerfilActualizarErrorBD = "Error de base de datos al actualizar perfil. Fallo en la ejecución de la actualización.";
            public const string PerfilActualizarErrorDatos = "Error de datos al actualizar perfil. Los datos del perfil no se pudieron procesar.";
            public const string PerfilActualizarOperacionInvalida = "Operación inválida al actualizar perfil. Secuencia de operaciones incorrecta.";

            // Mensajes de amistad
            public const string AmistadSuscribirErrorBD = "Error de base de datos al suscribir a notificaciones de amistad. Fallo en la consulta de usuario o solicitudes.";
            public const string AmistadSuscribirErrorDatos = "Error de datos al suscribir a notificaciones de amistad. No se pudieron recuperar las solicitudes pendientes.";
            public const string AmistadEnviarSolicitudErrorDatos = "Error de datos al enviar solicitud de amistad. No se pudo almacenar la solicitud en la base de datos.";
            public const string AmistadResponderSolicitudErrorDatos = "Error de datos al responder solicitud de amistad. No se pudo actualizar el estado de la solicitud.";
            public const string AmistadEliminarErrorDatos = "Error de datos al eliminar amistad. No se pudo eliminar la relación en la base de datos.";

            // Mensajes de lista de amigos
            public const string ListaAmigosSuscribirErrorDatos = "Error de datos al suscribirse a lista de amigos. No se pudo recuperar la lista de amigos del usuario.";
            public const string ListaAmigosObtenerErrorDatos = "Error de datos al obtener lista de amigos. Fallo en la consulta de amigos del usuario.";

            // Mensajes de recuperación de contraseña
            public const string RecuperacionSolicitarArgumentoNulo = "Argumento nulo al solicitar código de recuperación. Los datos de solicitud son nulos.";
            public const string RecuperacionSolicitarErrorBD = "Error de base de datos al solicitar código de recuperación. Fallo en la búsqueda de usuario.";
            public const string RecuperacionSolicitarErrorDatos = "Error de datos al solicitar código de recuperación. No se pudo procesar la solicitud.";
            public const string RecuperacionReenviarArgumentoNulo = "Argumento nulo al reenviar código de recuperación. Los datos de reenvío son nulos.";
            public const string RecuperacionReenviarErrorBD = "Error de base de datos al reenviar código de recuperación. Fallo en la verificación de solicitud.";
            public const string RecuperacionReenviarErrorDatos = "Error de datos al reenviar código de recuperación. No se pudo procesar el reenvío.";
            public const string RecuperacionConfirmarArgumentoNulo = "Argumento nulo al confirmar código de recuperación. Los datos de confirmación son nulos.";
            public const string RecuperacionConfirmarErrorBD = "Error de base de datos al confirmar código de recuperación. Fallo en la verificación de código.";
            public const string RecuperacionConfirmarErrorDatos = "Error de datos al confirmar código de recuperación. No se pudo confirmar el código.";
            public const string RecuperacionActualizarArgumentoNulo = "Argumento nulo al actualizar contraseña. Los datos de actualización son nulos.";
            public const string RecuperacionActualizarValidacionEntidad = "Validación de entidad fallida al actualizar contraseña. La nueva contraseña no cumple con las restricciones.";
            public const string RecuperacionActualizarActualizacionBD = "Error de actualización de base de datos al actualizar contraseña. Conflicto al guardar la nueva contraseña.";
            public const string RecuperacionActualizarErrorBD = "Error de base de datos al actualizar contraseña. Fallo en la ejecución de la actualización.";
            public const string RecuperacionActualizarErrorDatos = "Error de datos al actualizar contraseña. No se pudo procesar la actualización.";

            // Mensajes de comunicación WCF
            public const string ComunicacionTimeout = "Timeout de comunicación WCF. El cliente no respondió en el tiempo esperado.";
            public const string ComunicacionError = "Error de comunicación WCF. El canal de comunicación falló o se cerró inesperadamente.";
            public const string ComunicacionOperacionInvalida = "Operación inválida en comunicación WCF. El canal no está en el estado correcto para la operación.";
        }
    }
}
