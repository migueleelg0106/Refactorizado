using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Recursos = PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Centraliza la traduccion de mensajes de error provenientes del servidor a recursos locales.
    /// </summary>
    public class LocalizadorServicio : ILocalizadorServicio
    {
        private static readonly Regex EsperaCodigoRegex = new Regex(
            @"^Debe esperar (\d+) segundos para solicitar un nuevo codigo\.$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

        private static readonly Regex IdentificadorRedSocialRegex = new Regex(
            @"^El identificador de (.+) no debe exceder (\d+) caracteres\.$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

        private static readonly Dictionary<string, Func<string>> MapaMensajes =
            new Dictionary<string, Func<string>>(StringComparer.Ordinal)
            {
                ["La partida ya comenzo"]
                    = () => Recursos.Lang.errorTextoPartidaYaIniciada,
                ["Partida cancelada por falta de jugadores."]
                    = () => Recursos.Lang.partidaTextoJugadoresInsuficientes,
                ["El anfitrion de la sala abandono la partida."]
                    = () => Recursos.Lang.partidaTextoHostCanceloSala,
                ["Reporte enviado correctamente."]
                    = () => Recursos.Lang.reportarJugadorTextoExito,
                ["No fue posible registrar el reporte."]
                    = () => Recursos.Lang.errorTextoReportarJugador,
                ["Ya has reportado a este jugador."]
                    = () => Recursos.Lang.reportarJugadorTextoDuplicado,
                ["El motivo del reporte es obligatorio."]
                    = () => Recursos.Lang.reportarJugadorTextoMotivoRequerido,
                ["El motivo del reporte no debe exceder 100 caracteres."]
                    = () => Recursos.Lang.reportarJugadorTextoMotivoLongitud,
                ["El mensaje supera el limite de caracteres."]
                    = () => Recursos.Lang.MensajeChatTextoMotivoLongitud,
                ["No puedes reportarte a ti mismo."]
                    = () => Recursos.Lang.reportarJugadorTextoAutoReporte,
                ["No fue posible procesar la solicitud de verificacion."]
                    = () => Recursos.Lang.errorTextoProcesarSolicitudVerificacion,
                ["No fue posible reenviar el codigo de verificacion."]
                    = () => Recursos.Lang.errorTextoServidorReenviarCodigo,
                ["No se encontro una solicitud de verificacion activa."]
                    = () => Recursos.Lang.errorTextoSolicitudVerificacionActiva,
                ["El codigo de verificacion ha expirado. Inicie el proceso nuevamente."]
                    = () => Recursos.Lang.avisoTextoCodigoExpirado,
                ["El codigo ingresado no es correcto."]
                    = () => Recursos.Lang.errorTextoCodigoIncorrecto,
                ["El correo o usuario ya esta registrado."]
                    = () => Recursos.Lang.errorTextoCorreoEnUso,
                ["No fue posible procesar la recuperacion de cuenta."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible reenviar el codigo de recuperacion."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible confirmar el codigo de recuperacion."]
                    = () => Recursos.Lang.errorTextoServidorValidarCodigo,
                ["No fue posible actualizar la contrasena."]
                    = () => Recursos.Lang.errorTextoActualizarContrasena,
                ["Los datos de recuperacion no son validos."]
                    = () => Recursos.Lang.errorTextoServidorSolicitudCambioContrasena,
                ["Los datos para reenviar el codigo no son validos."]
                    = () => Recursos.Lang.errorTextoServidorSolicitudCambioContrasena,
                ["Los datos de confirmacion no son validos."]
                    = () => Recursos.Lang.errorTextoSolicitudVerificacionInvalida,
                ["Los datos de actualizacion no son validos."]
                    = () => Recursos.Lang.errorTextoPrepararSolicitudCambioContrasena,
                ["Los datos proporcionados no son validos para solicitar el codigo."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["Debe proporcionar el usuario o correo registrado y no debe exceder 50 caracteres."]
                    = () => Recursos.Lang.errorTextoIdentificadorRecuperacionRequerido,
                ["No se encontro una cuenta con el usuario o correo proporcionado."]
                    = () => Recursos.Lang.errorTextoCuentaNoRegistrada,
                ["No se encontro una solicitud de recuperacion activa."]
                    = () => Recursos.Lang.errorTextoSolicitudRecuperacionActiva,
                ["El codigo de verificacion ha expirado. Solicite uno nuevo."]
                    = () => Recursos.Lang.errorTextoCodigoExpiradoSolicitarNuevo,
                ["No hay una solicitud de recuperacion vigente."]
                    = () => Recursos.Lang.errorTextoSolicitudRecuperacionVigente,
                ["La solicitud de recuperacion no es valida."]
                    = () => Recursos.Lang.errorTextoSolicitudRecuperacionInvalida,
                ["No fue posible completar el registro. Por favor, intente nuevamente."]
                    = () => Recursos.Lang.errorTextoRegistrarCuentaMasTarde,
                ["No fue posible iniciar sesion. Por favor, intente nuevamente."]
                    = () => Recursos.Lang.errorTextoServidorInicioSesion,
                ["Has sido baneado del juego por mala conducta."]
                    = () => Recursos.Lang.errorTextoUsuarioBaneado,
                ["Usuario o contrasena incorrectos."]
                    = () => Recursos.Lang.errorTextoCredencialesIncorrectas,
                ["Las credenciales proporcionadas no son validas."]
                    = () => Recursos.Lang.errorTextoCredencialesIncorrectas,
                ["El correo electronico es obligatorio, debe tener un formato valido y no debe exceder 50 caracteres."]
                    = () => Recursos.Lang.errorTextoCorreoInvalido,
                ["El nombre de usuario es obligatorio y no debe exceder 50 caracteres."]
                    = () => Recursos.Lang.errorTextoIdentificadorUsuarioInvalido,
                ["El nombre es obligatorio y no debe exceder 50 caracteres."]
                    = () => Recursos.Lang.errorTextoNombreObligatorioLongitud,
                ["El apellido es obligatorio y no debe exceder 50 caracteres."]
                    = () => Recursos.Lang.errorTextoApellidoObligatorioLongitud,
                ["No se encontro el usuario especificado."]
                    = () => Recursos.Lang.errorTextoUsuarioNoEncontrado,
                ["No se encontro la informacion del jugador."]
                    = () => Recursos.Lang.errorTextoJugadorNoExiste,
                ["No existe un jugador asociado al usuario especificado."]
                    = () => Recursos.Lang.errorTextoJugadorNoExiste,
                ["El avatar seleccionado no es valido."]
                    = () => Recursos.Lang.errorTextoSeleccionAvatarValido,
                ["No fue posible obtener la informacion del perfil."]
                    = () => Recursos.Lang.errorTextoServidorObtenerPerfil,
                ["No fue posible actualizar el perfil. Por favor, intente nuevamente."]
                    = () => Recursos.Lang.errorTextoActualizarPerfil,
                ["Perfil actualizado correctamente."]
                    = () => Recursos.Lang.avisoTextoPerfilActualizado,
                ["No fue posible recuperar las solicitudes de amistad."]
                    = () => Recursos.Lang.amigosErrorRecuperarSolicitudes,
                ["No es posible enviarse una solicitud de amistad a si mismo."]
                    = () => Recursos.Lang.amigosErrorAutoSolicitud,
                ["Alguno de los usuarios especificados no existe."]
                    = () => Recursos.Lang.amigosErrorUsuarioNoExiste,
                ["Ya existe una solicitud o relacion de amistad entre los usuarios."]
                    = () => Recursos.Lang.amigosErrorRelacionExiste,
                ["No fue posible enviar la solicitud de amistad."]
                    = () => Recursos.Lang.amigosErrorCompletarSolicitud,
                ["No fue posible actualizar la solicitud de amistad."]
                    = () => Recursos.Lang.amigosErrorActualizarSolicitud,
                ["No existe una solicitud de amistad entre los usuarios."]
                    = () => Recursos.Lang.amigosErrorSolicitudNoExiste,
                ["No fue posible aceptar la solicitud de amistad."]
                    = () => Recursos.Lang.amigosErrorAceptarSolicitud,
                ["La solicitud de amistad ya fue aceptada con anterioridad."]
                    = () => Recursos.Lang.amigosErrorSolicitudAceptada,
                ["No existe una relacion de amistad entre los usuarios."]
                    = () => Recursos.Lang.amigosErrorRelacionNoExiste,
                ["No fue posible eliminar la relacion de amistad."]
                    = () => Recursos.Lang.amigosErrorEliminarRelacion,
                ["No fue posible recuperar la lista de amigos."]
                    = () => Recursos.Lang.amigosErrorRecuperarSolicitudes,
                ["La invitacion no es valida."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["Los datos de la invitacion no son validos."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["El correo electronico no es valido."]
                    = () => Recursos.Lang.errorTextoCorreoInvalido,
                ["No fue posible enviar la invitacion."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrio un problema al procesar la invitacion."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrio un error al enviar la invitacion."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["Invitacion enviada correctamente."]
                    = () => Recursos.Lang.invitarCorreoTextoEnviado,
                ["El jugador con el correo ingresado ya esta en la sala."]
                    = () => Recursos.Lang.invitarCorreoTextoJugadorYaEnSala,
                ["No fue posible enviar la invitacion por correo electronico."]
                    = () => Recursos.Lang.errorTextoEnviarCorreo,
                ["No se encontro la sala especificada."]
                    = () => Recursos.Lang.errorTextoNoEncuentraPartida,
                ["La sala esta llena."]
                    = () => Recursos.Lang.errorTextoSalaLlena,
                ["No fue posible crear la sala."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrio un error al unirse a la sala."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrio un error al abandonar la sala."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrio un error al expulsar al jugador."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrio un error al suscribirse a las salas."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible generar un codigo para la sala."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["Solo el creador de la sala puede expulsar jugadores."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["El creador de la sala no puede ser expulsado."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["El jugador especificado no esta en la sala."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["El jugador ya esta en la sala."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["Los datos proporcionados no son validos. Por favor, verifique la informacion."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrio un error inesperado. Por favor, intente nuevamente."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["No se encontro una cuenta con los datos proporcionados."]
                    = () => Recursos.Lang.errorTextoCuentaNoRegistrada,
                ["La cuenta no ha sido verificada. Por favor, verifique su correo."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["La operacion se completo correctamente."]
                    = () => Recursos.Lang.avisoTextoPerfilActualizado,
                ["La solicitud de invitacion no es valida."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible confirmar el codigo de verificacion."]
                    = () => Recursos.Lang.errorTextoServidorValidarCodigo,
                ["No fue posible establecer el contexto de la operacion."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible establecer el contexto para amigos."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible establecer la conexion con el servidor."]
                    = () => Recursos.Lang.errorTextoServidorNoDisponible,
                ["No fue posible establecer la conexion para amigos."]
                    = () => Recursos.Lang.errorTextoServidorNoDisponible,
                ["No fue posible notificar la actualizacion de la solicitud de amistad."]
                    = () => Recursos.Lang.amigosErrorActualizarSolicitud,
                ["No fue posible notificar la eliminacion de la relacion de amistad."]
                    = () => Recursos.Lang.amigosErrorEliminarRelacion,
                ["No fue posible suscribirse a las actualizaciones de amigos."]
                    = () => Recursos.Lang.amigosErrorRecuperarSolicitudes,
                ["No se encontraron todos los usuarios especificados."]
                    = () => Recursos.Lang.amigosErrorUsuarioNoExiste,
                ["Ocurrio un error al crear la sala."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["El codigo de sala es obligatorio."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["El idioma de las canciones es obligatorio."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["El nombre de usuario es obligatorio para cancelar la suscripcion."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["El nombre de usuario es obligatorio para suscribirse a las notificaciones."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["El nombre de usuario es obligatorio."]
                    = () => Recursos.Lang.errorTextoIdentificadorUsuarioInvalido,
                ["El numero de rondas debe ser mayor a cero."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["El parametro {0} es obligatorio."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["El tiempo por ronda debe ser mayor a cero."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["La configuracion de la partida es obligatoria."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["La contrasena debe tener entre 8 y 15 caracteres, incluir una letra mayuscula, un numero y un caracter especial."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud,
                ["La dificultad es obligatoria."]
                    = () => Recursos.Lang.errorTextoErrorProcesarSolicitud
            };

        /// <summary>
        /// Intenta traducir un mensaje del servidor usando el mapa o expresiones regulares.
        /// </summary>
        /// <param name="mensaje">Mensaje recibido del servidor.</param>
        /// <param name="mensajePredeterminado">Mensaje alternativo si no hay traduccion.</param>
        /// <returns>Mensaje traducido o el predeterminado.</returns>
        public string Localizar(string mensaje, string mensajePredeterminado)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return ObtenerMensajePredeterminado(mensajePredeterminado);
            }

            string mensajeNormalizado = mensaje.Trim();

            if (TryLocalizarMensajeDinamico(mensajeNormalizado, out string mensajeTraducido))
            {
                return mensajeTraducido;
            }

            if (TryLocalizarMensajeEstatico(mensajeNormalizado, out string mensajeEstatico))
            {
                return mensajeEstatico;
            }

            return ObtenerMensajePredeterminado(mensajePredeterminado);
        }

        private bool TryLocalizarMensajeDinamico(string mensaje, out string traducido)
        {
            if (IntentarCoincidenciaEsperaCodigo(mensaje, out traducido))
            {
                return true;
            }

            if (IntentarCoincidenciaRedSocial(mensaje, out traducido))
            {
                return true;
            }

            traducido = null;
            return false;
        }

        private bool IntentarCoincidenciaEsperaCodigo(string mensaje, out string traducido)
        {
            Match espera = EsperaCodigoRegex.Match(mensaje);
            if (espera.Success)
            {
                traducido = string.Format(
                    CultureInfo.CurrentCulture,
                    Recursos.Lang.errorTextoTiempoEsperaCodigo,
                    espera.Groups[1].Value);
                return true;
            }

            traducido = null;
            return false;
        }

        private bool IntentarCoincidenciaRedSocial(string mensaje, out string traducido)
        {
            Match identificador = IdentificadorRedSocialRegex.Match(mensaje);
            if (identificador.Success)
            {
                traducido = string.Format(
                    CultureInfo.CurrentCulture,
                    Recursos.Lang.errorTextoIdentificadorRedSocialLongitud,
                    identificador.Groups[1].Value,
                    identificador.Groups[2].Value);
                return true;
            }

            traducido = null;
            return false;
        }

        private bool TryLocalizarMensajeEstatico(string mensaje, out string traducido)
        {
            if (MapaMensajes.TryGetValue(mensaje, out Func<string> traductor))
            {
                traducido = traductor();
                return true;
            }

            traducido = null;
            return false;
        }

        private string ObtenerMensajePredeterminado(string mensajePredeterminado)
        {
            if (!string.IsNullOrWhiteSpace(mensajePredeterminado))
            {
                return mensajePredeterminado;
            }

            return Recursos.Lang.errorTextoErrorProcesarSolicitud;
        }
    }
}