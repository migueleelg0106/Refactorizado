using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using LangResources = PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante
{
    /// <summary>
    /// Centraliza la traduccion de mensajes de error provenientes del servidor a recursos locales.
    /// </summary>
    public static class MensajeServidorAyudante
    {
        private static readonly Regex EsperaCodigoRegex = new Regex(
            @"^Debe esperar (\d+) segundos para solicitar un nuevo código\.$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

        private static readonly Regex IdentificadorRedSocialRegex = new Regex(
            @"^El identificador de (.+) no debe exceder (\d+) caracteres\.$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

        private static readonly Dictionary<string, Func<string>> MapaMensajes =
            new Dictionary<string, Func<string>>(StringComparer.Ordinal)
            {
                ["No fue posible procesar la solicitud de verificación."]
                    = () => LangResources.Lang.errorTextoProcesarSolicitudVerificacion,
                ["No fue posible reenviar el código de verificación."]
                    = () => LangResources.Lang.errorTextoServidorReenviarCodigo,
                ["No se encontró una solicitud de verificación activa."]
                    = () => LangResources.Lang.errorTextoSolicitudVerificacionActiva,
                ["El código de verificación ha expirado. Inicie el proceso nuevamente."]
                    = () => LangResources.Lang.avisoTextoCodigoExpirado,
                ["El código ingresado no es correcto."]
                    = () => LangResources.Lang.errorTextoCodigoIncorrecto,
                ["El correo o usuario ya está registrado."]
                    = () => LangResources.Lang.errorTextoCorreoEnUso,
                ["No fue posible procesar la recuperación de cuenta."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible reenviar el código de recuperación."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible confirmar el código de recuperación."]
                    = () => LangResources.Lang.errorTextoServidorValidarCodigo,
                ["No fue posible actualizar la contraseña."]
                    = () => LangResources.Lang.errorTextoActualizarContrasena,
                ["Los datos de recuperación no son válidos."]
                    = () => LangResources.Lang.errorTextoServidorSolicitudCambioContrasena,
                ["Los datos para reenviar el código no son válidos."]
                    = () => LangResources.Lang.errorTextoServidorSolicitudCambioContrasena,
                ["Los datos de confirmación no son válidos."]
                    = () => LangResources.Lang.errorTextoSolicitudVerificacionInvalida,
                ["Los datos de actualización no son válidos."]
                    = () => LangResources.Lang.errorTextoPrepararSolicitudCambioContrasena,
                ["Los datos proporcionados no son válidos para solicitar el código."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Debe proporcionar el usuario o correo registrado y no debe exceder 50 caracteres."]
                    = () => LangResources.Lang.errorTextoIdentificadorRecuperacionRequerido,
                ["No se encontró una cuenta con el usuario o correo proporcionado."]
                    = () => LangResources.Lang.errorTextoCuentaNoRegistrada,
                ["No se encontró una solicitud de recuperación activa."]
                    = () => LangResources.Lang.errorTextoSolicitudRecuperacionActiva,
                ["El código de verificación ha expirado. Solicite uno nuevo."]
                    = () => LangResources.Lang.errorTextoCodigoExpiradoSolicitarNuevo,
                ["No hay una solicitud de recuperación vigente."]
                    = () => LangResources.Lang.errorTextoSolicitudRecuperacionVigente,
                ["La solicitud de recuperación no es válida."]
                    = () => LangResources.Lang.errorTextoSolicitudRecuperacionInvalida,
                ["No fue posible completar el registro. Por favor, intente nuevamente."]
                    = () => LangResources.Lang.errorTextoRegistrarCuentaMasTarde,
                ["No fue posible iniciar sesión. Por favor, intente nuevamente."]
                    = () => LangResources.Lang.errorTextoServidorInicioSesion,
                ["Usuario o contraseña incorrectos."]
                    = () => LangResources.Lang.errorTextoCredencialesIncorrectas,
                ["Las credenciales proporcionadas no son válidas."]
                    = () => LangResources.Lang.errorTextoCredencialesIncorrectas,
                ["El correo electrónico es obligatorio, debe tener un formato válido y no debe exceder 50 caracteres."]
                    = () => LangResources.Lang.errorTextoCorreoInvalido,
                ["El nombre de usuario es obligatorio y no debe exceder 50 caracteres."]
                    = () => LangResources.Lang.errorTextoIdentificadorUsuarioInvalido,
                ["El nombre es obligatorio y no debe exceder 50 caracteres."]
                    = () => LangResources.Lang.errorTextoNombreObligatorioLongitud,
                ["El apellido es obligatorio y no debe exceder 50 caracteres."]
                    = () => LangResources.Lang.errorTextoApellidoObligatorioLongitud,
                ["No se encontró el usuario especificado."]
                    = () => LangResources.Lang.errorTextoUsuarioNoEncontrado,
                ["No se encontró la información del jugador."]
                    = () => LangResources.Lang.errorTextoJugadorNoExiste,
                ["No existe un jugador asociado al usuario especificado."]
                    = () => LangResources.Lang.errorTextoJugadorNoExiste,
                ["El avatar seleccionado no es válido."]
                    = () => LangResources.Lang.errorTextoSeleccionAvatarValido,
                ["No fue posible obtener la información del perfil."]
                    = () => LangResources.Lang.errorTextoServidorObtenerPerfil,
                ["No fue posible actualizar el perfil. Por favor, intente nuevamente."]
                    = () => LangResources.Lang.errorTextoActualizarPerfil,
                ["Perfil actualizado correctamente."]
                    = () => LangResources.Lang.avisoTextoPerfilActualizado,
                ["No fue posible recuperar las solicitudes de amistad."]
                    = () => LangResources.Lang.amigosErrorRecuperarSolicitudes,
                ["No es posible enviarse una solicitud de amistad a sí mismo."]
                    = () => LangResources.Lang.amigosErrorAutoSolicitud,
                ["Alguno de los usuarios especificados no existe."]
                    = () => LangResources.Lang.amigosErrorUsuarioNoExiste,
                ["Ya existe una solicitud o relación de amistad entre los usuarios."]
                    = () => LangResources.Lang.amigosErrorRelacionExiste,
                ["No fue posible enviar la solicitud de amistad."]
                    = () => LangResources.Lang.amigosErrorCompletarSolicitud,
                ["No fue posible actualizar la solicitud de amistad."]
                    = () => LangResources.Lang.amigosErrorActualizarSolicitud,
                ["No existe una solicitud de amistad entre los usuarios."]
                    = () => LangResources.Lang.amigosErrorSolicitudNoExiste,
                ["No fue posible aceptar la solicitud de amistad."]
                    = () => LangResources.Lang.amigosErrorAceptarSolicitud,
                ["La solicitud de amistad ya fue aceptada con anterioridad."]
                    = () => LangResources.Lang.amigosErrorSolicitudAceptada,
                ["No existe una relación de amistad entre los usuarios."]
                    = () => LangResources.Lang.amigosErrorRelacionNoExiste,
                ["No fue posible eliminar la relación de amistad."]
                    = () => LangResources.Lang.amigosErrorEliminarRelacion,
                ["No fue posible recuperar la lista de amigos."]
                    = () => LangResources.Lang.amigosErrorRecuperarSolicitudes,
                ["La invitación no es válida."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Los datos de la invitación no son válidos."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["El correo electrónico no es válido."]
                    = () => LangResources.Lang.errorTextoCorreoInvalido,
                ["No fue posible enviar la invitación."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrió un problema al procesar la invitación."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrió un error al enviar la invitación."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Invitación enviada correctamente."]
                    = () => LangResources.Lang.invitarCorreoTextoEnviado,
                ["El jugador con el correo ingresado ya está en la sala."]
                    = () => LangResources.Lang.invitarCorreoTextoJugadorYaEnSala,
                ["No fue posible enviar la invitación por correo electrónico."]
                    = () => LangResources.Lang.errorTextoEnviarCorreo,
                ["No se encontró la sala especificada."]
                    = () => LangResources.Lang.errorTextoNoEncuentraPartida,
                ["La sala está llena."]
                    = () => LangResources.Lang.errorTextoSalaLlena,
                ["No fue posible crear la sala."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrió un error al unirse a la sala."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrió un error al abandonar la sala."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrió un error al expulsar al jugador."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrió un error al suscribirse a las salas."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible generar un código para la sala."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Solo el creador de la sala puede expulsar jugadores."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["El creador de la sala no puede ser expulsado."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["El jugador especificado no está en la sala."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["El jugador ya está en la sala."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Los datos proporcionados no son válidos. Por favor, verifique la información."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrió un error inesperado. Por favor, intente nuevamente."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["No se encontró una cuenta con los datos proporcionados."]
                    = () => LangResources.Lang.errorTextoCuentaNoRegistrada,
                ["La cuenta no ha sido verificada. Por favor, verifique su correo."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["La operación se completó correctamente."]
                    = () => LangResources.Lang.avisoTextoPerfilActualizado,
                ["La solicitud de invitación no es válida."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible confirmar el código de verificación."]
                    = () => LangResources.Lang.errorTextoServidorValidarCodigo,
                ["No fue posible establecer el contexto de la operación."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible establecer el contexto para amigos."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible establecer la conexión con el servidor."]
                    = () => LangResources.Lang.errorTextoServidorNoDisponible,
                ["No fue posible establecer la conexión para amigos."]
                    = () => LangResources.Lang.errorTextoServidorNoDisponible,
                ["No fue posible notificar la actualización de la solicitud de amistad."]
                    = () => LangResources.Lang.amigosErrorActualizarSolicitud,
                ["No fue posible notificar la eliminación de la relación de amistad."]
                    = () => LangResources.Lang.amigosErrorEliminarRelacion,
                ["No fue posible suscribirse a las actualizaciones de amigos."]
                    = () => LangResources.Lang.amigosErrorRecuperarSolicitudes,
                ["No se encontraron todos los usuarios especificados."]
                    = () => LangResources.Lang.amigosErrorUsuarioNoExiste,
                ["Ocurrió un error al crear la sala."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["El código de sala es obligatorio."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["El idioma de las canciones es obligatorio."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["El nombre de usuario es obligatorio para cancelar la suscripción."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["El nombre de usuario es obligatorio para suscribirse a las notificaciones."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["El nombre de usuario es obligatorio."]
                    = () => LangResources.Lang.errorTextoIdentificadorUsuarioInvalido,
                ["El número de rondas debe ser mayor a cero."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["El parámetro {0} es obligatorio."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["El tiempo por ronda debe ser mayor a cero."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["La configuración de la partida es obligatoria."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["La contraseña debe tener entre 8 y 15 caracteres, incluir una letra mayúscula, un número y un carácter especial."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["La dificultad es obligatoria."]
                    = () => LangResources.Lang.errorTextoErrorProcesarSolicitud
            };

        /// <summary>
        /// Intenta traducir un mensaje del servidor usando el mapa o expresiones regulares.
        /// </summary>
        public static string Localizar(string mensaje, string mensajePredeterminado)
        {
            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                string mensajeNormalizado = mensaje.Trim();

                if (TryLocalizarMensajeDinamico(mensajeNormalizado, out string mensajeTraducido))
                {
                    return mensajeTraducido;
                }

                if (MapaMensajes.TryGetValue(mensajeNormalizado, out Func<string> traductor))
                {
                    return traductor();
                }
            }

            if (!string.IsNullOrWhiteSpace(mensajePredeterminado))
            {
                return mensajePredeterminado;
            }

            return LangResources.Lang.errorTextoErrorProcesarSolicitud;
        }

        private static bool TryLocalizarMensajeDinamico(string mensaje, out string traducido)
        {
            Match espera = EsperaCodigoRegex.Match(mensaje);
            if (espera.Success)
            {
                traducido = string.Format(
                    CultureInfo.CurrentCulture,
                    LangResources.Lang.errorTextoTiempoEsperaCodigo,
                    espera.Groups[1].Value);
                return true;
            }

            Match identificador = IdentificadorRedSocialRegex.Match(mensaje);
            if (identificador.Success)
            {
                traducido = string.Format(
                    CultureInfo.CurrentCulture,
                    LangResources.Lang.errorTextoIdentificadorRedSocialLongitud,
                    identificador.Groups[1].Value,
                    identificador.Groups[2].Value);
                return true;
            }

            traducido = null;
            return false;
        }
    }
}