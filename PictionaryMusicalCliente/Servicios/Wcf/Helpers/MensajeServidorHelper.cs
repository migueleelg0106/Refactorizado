using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using LangResources = PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente.Servicios.Wcf.Helpers
{
    public static class MensajeServidorHelper
    {
        private static readonly Regex EsperaCodigoRegex = new Regex(
            @"^Debe esperar (\d+) segundos para solicitar un nuevo código\.$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex IdentificadorRedSocialRegex = new Regex(
            @"^El identificador de (.+) no debe exceder (\d+) caracteres\.$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Dictionary<string, Func<string>> MapaMensajes =
            new Dictionary<string, Func<string>>(StringComparer.Ordinal)
            {
                ["No fue posible procesar la solicitud de verificación."] = () => LangResources.Lang.errorTextoProcesarSolicitudVerificacion,
                ["Se envió un código de verificación al correo proporcionado."] = () => LangResources.Lang.avisoTextoCodigoEnviado,
                ["La solicitud de verificación no es válida."] = () => LangResources.Lang.errorTextoSolicitudVerificacionInvalida,
                ["No se encontró una solicitud de verificación activa."] = () => LangResources.Lang.errorTextoSolicitudVerificacionActiva,
                ["El código de verificación ha expirado. Inicie el proceso nuevamente."] = () => LangResources.Lang.avisoTextoCodigoExpirado,
                ["Se envió un nuevo código de verificación."] = () => LangResources.Lang.avisoTextoCodigoReenviado,
                ["El código de verificación es inválido."] = () => LangResources.Lang.errorTextoCodigoIncorrecto,
                ["No hay una solicitud de verificación vigente."] = () => LangResources.Lang.errorTextoSolicitudVerificacionVigente,
                ["El código de verificación ha expirado."] = () => LangResources.Lang.avisoTextoCodigoExpirado,
                ["El código ingresado no es correcto."] = () => LangResources.Lang.errorTextoCodigoIncorrecto,
                ["Código incorrecto"] = () => LangResources.Lang.errorTextoCodigoIncorrecto,
                ["Los datos de la cuenta están incompletos."] = () => LangResources.Lang.errorTextoEnvioCodigoVerificacionDatos,
                ["El correo y el usuario ya están registrados."] = () => string.Concat(
                    LangResources.Lang.errorTextoCorreoEnUso,
                    " ",
                    LangResources.Lang.errorTextoUsuarioEnUso),
                ["El correo electrónico ya está asociado a otra cuenta."] = () => LangResources.Lang.errorTextoCorreoEnUso,
                ["El nombre de usuario ya se encuentra en uso."] = () => LangResources.Lang.errorTextoUsuarioEnUso,
                ["Los datos de la cuenta no están disponibles."] = () => LangResources.Lang.errorTextoCuentaDatosNoDisponibles,
                ["El correo o usuario ya está registrado."] = () => LangResources.Lang.errorTextoCorreoEnUso,
                ["Cuenta registrada correctamente."] = () => LangResources.Lang.avisoTextoRegistroCompletado,
                ["No se pudo registrar la cuenta. Intente más tarde."] = () => LangResources.Lang.errorTextoRegistrarCuentaMasTarde,
                ["Credenciales incorrectas."] = () => LangResources.Lang.errorTextoCredencialesIncorrectas,
                ["Debe proporcionar el usuario o correo registrado."] = () => LangResources.Lang.errorTextoIdentificadorRecuperacionRequerido,
                ["No se encontró una cuenta con el usuario o correo proporcionado."] = () => LangResources.Lang.errorTextoCuentaNoRegistrada,
                ["No fue posible iniciar la recuperación de la cuenta."] = () => LangResources.Lang.errorTextoIniciarRecuperacion,
                ["Se envió un código de verificación al correo registrado."] = () => LangResources.Lang.avisoTextoCodigoEnviado,
                ["La solicitud de recuperación no es válida."] = () => LangResources.Lang.errorTextoSolicitudRecuperacionInvalida,
                ["No se encontró una solicitud de recuperación activa."] = () => LangResources.Lang.errorTextoSolicitudRecuperacionActiva,
                ["El código de verificación ha expirado. Solicite uno nuevo."] = () => LangResources.Lang.errorTextoCodigoExpiradoSolicitarNuevo,
                ["Código verificado correctamente. Continúe con el cambio de contraseña."] = () => LangResources.Lang.avisoTextoCodigoVerificadoCambio,
                ["La solicitud de actualización de contraseña no es válida."] = () => LangResources.Lang.errorTextoPrepararSolicitudCambioContrasena,
                ["No hay una solicitud de recuperación vigente."] = () => LangResources.Lang.errorTextoSolicitudRecuperacionVigente,
                ["No fue posible actualizar la contraseña."] = () => LangResources.Lang.errorTextoActualizarContrasena,
                ["La contraseña se actualizó correctamente."] = () => LangResources.Lang.avisoTextoContrasenaActualizada,
                ["La solicitud de actualización es obligatoria."] = () => LangResources.Lang.errorTextoSolicitudActualizacionObligatoria,
                ["El identificador de usuario es inválido."] = () => LangResources.Lang.errorTextoIdentificadorUsuarioInvalido,
                ["El nombre es obligatorio y no debe exceder 50 caracteres."] = () => LangResources.Lang.errorTextoNombreObligatorioLongitud,
                ["El apellido es obligatorio y no debe exceder 50 caracteres."] = () => LangResources.Lang.errorTextoApellidoObligatorioLongitud,
                ["Selecciona un avatar válido."] = () => LangResources.Lang.errorTextoSeleccionAvatarValido,
                ["No se encontró el usuario especificado."] = () => LangResources.Lang.errorTextoUsuarioNoEncontrado,
                ["No existe un jugador asociado al usuario especificado."] = () => LangResources.Lang.errorTextoJugadorNoExiste,
                ["El avatar seleccionado no existe."] = () => LangResources.Lang.errorTextoAvatarNoExiste,
                ["No fue posible actualizar el perfil."] = () => LangResources.Lang.errorTextoActualizarPerfil,
                ["Perfil actualizado correctamente."] = () => LangResources.Lang.avisoTextoPerfilActualizado,
                ["Los datos proporcionados no son válidos para recuperar la cuenta."] = () => LangResources.Lang.errorTextoServidorSolicitudCambioContrasena,
                ["No fue posible procesar la recuperación de la cuenta."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrió un problema al iniciar la recuperación de la cuenta."] = () => LangResources.Lang.errorTextoServidorInicioRecuperacion,
                ["Ocurrió un error inesperado al solicitar la recuperación de la cuenta."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Los datos proporcionados no son válidos para reenviar el código."] = () => LangResources.Lang.errorTextoServidorSolicitudCambioContrasena,
                ["No fue posible reenviar el código de recuperación."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrió un problema al reenviar el código de recuperación."] = () => LangResources.Lang.errorTextoServidorReenviarCodigo,
                ["Ocurrió un error inesperado al reenviar el código de recuperación."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Los datos proporcionados no son válidos para confirmar el código."] = () => LangResources.Lang.errorTextoSolicitudVerificacionInvalida,
                ["No fue posible confirmar el código de recuperación."] = () => LangResources.Lang.errorTextoServidorValidarCodigo,
                ["Ocurrió un problema al confirmar el código de recuperación."] = () => LangResources.Lang.errorTextoServidorValidarCodigo,
                ["Ocurrió un error inesperado al confirmar el código de recuperación."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Los datos proporcionados no son válidos para actualizar la contraseña."] = () => LangResources.Lang.errorTextoPrepararSolicitudCambioContrasena,
                ["Ocurrió un problema al actualizar la contraseña."] = () => LangResources.Lang.errorTextoServidorActualizarContrasena,
                ["Ocurrió un error inesperado al actualizar la contraseña."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible obtener los avatares solicitados."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible recuperar el catálogo de avatares."] = () => LangResources.Lang.errorTextoServidorNoDisponible,
                ["Ocurrió un problema al obtener el catálogo de avatares."] = () => LangResources.Lang.errorTextoServidorNoDisponible,
                ["Ocurrió un error inesperado al obtener los avatares."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible obtener la clasificación solicitada."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible recuperar la clasificación de jugadores."] = () => LangResources.Lang.errorTextoServidorNoDisponible,
                ["Ocurrió un problema al obtener la clasificación de jugadores."] = () => LangResources.Lang.errorTextoServidorNoDisponible,
                ["Ocurrió un error inesperado al obtener la clasificación."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Los datos proporcionados no son válidos para reenviar el código."] = () => LangResources.Lang.errorTextoServidorSolicitudCambioContrasena,
                ["No fue posible reenviar el código de verificación."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrió un problema al reenviar el código de verificación."] = () => LangResources.Lang.errorTextoServidorReenviarCodigo,
                ["Ocurrió un error inesperado al reenviar el código de verificación."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Los datos proporcionados no son válidos para obtener el perfil."] = () => LangResources.Lang.errorTextoServidorNoDisponible,
                ["No fue posible obtener el perfil del usuario."] = () => LangResources.Lang.errorTextoServidorObtenerPerfil,
                ["Ocurrió un problema al consultar la información del perfil."] = () => LangResources.Lang.errorTextoServidorObtenerPerfil,
                ["Ocurrió un error inesperado al obtener el perfil."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Los datos proporcionados no son válidos para actualizar el perfil."] = () => LangResources.Lang.errorTextoPerfilActualizarInformacion,
                ["No fue posible actualizar el perfil del usuario."] = () => LangResources.Lang.errorTextoServidorActualizarPerfil,
                ["Ocurrió un problema con la base de datos al actualizar el perfil."] = () => LangResources.Lang.errorTextoServidorActualizarPerfil,
                ["Ocurrió un error inesperado al actualizar el perfil."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Los datos proporcionados no son válidos para solicitar el código."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible generar el código de verificación."] = () => LangResources.Lang.errorTextoServidorCodigoVerificacion,
                ["No se pudo solicitar el código de verificación por un problema interno."] = () => LangResources.Lang.errorTextoServidorCodigoVerificacion,
                ["Ocurrió un error inesperado al solicitar el código de verificación."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Los datos proporcionados no son válidos para confirmar el código."] = () => LangResources.Lang.errorTextoSolicitudVerificacionInvalida,
                ["No se pudo confirmar el código de verificación."] = () => LangResources.Lang.errorTextoServidorValidarCodigo,
                ["No se pudo confirmar el código de verificación por un problema interno."] = () => LangResources.Lang.errorTextoServidorValidarCodigo,
                ["Ocurrió un error inesperado al confirmar el código de verificación."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Los datos de la cuenta no son válidos."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible completar el registro de la cuenta."] = () => LangResources.Lang.errorTextoRegistrarCuentaMasTarde,
                ["Ocurrió un problema al registrar la cuenta."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Ocurrió un error inesperado al registrar la cuenta."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Los datos proporcionados no son válidos para iniciar sesión."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["No fue posible completar el inicio de sesión."] = () => LangResources.Lang.errorTextoServidorInicioSesion,
                ["Ocurrió un problema al iniciar sesión."] = () => LangResources.Lang.errorTextoServidorInicioSesion,
                ["Ocurrió un error inesperado al iniciar sesión."] = () => LangResources.Lang.errorTextoErrorProcesarSolicitud,
                ["Solicitud de amistad enviada correctamente."] = () => LangResources.Lang.amigosTextoSolicitudEnviada,
                ["No fue posible enviar la solicitud de amistad."] = () => LangResources.Lang.amigosTextoSolicitudEnviarFallo,
                ["No puedes enviarte una solicitud de amistad a ti mismo."] = () => LangResources.Lang.amigosTextoSolicitudNoPosibleMismoUsuario,
                ["Ya existe una solicitud o amistad entre los usuarios indicados."] = () => LangResources.Lang.amigosTextoSolicitudRelacionExistente,
                ["No se encontró alguno de los usuarios especificados."] = () => LangResources.Lang.amigosTextoSolicitudUsuariosNoEncontrados,
                ["Solicitud de amistad aceptada correctamente."] = () => LangResources.Lang.amigosTextoSolicitudAceptarExito,
                ["La solicitud de amistad ya había sido aceptada anteriormente."] = () => LangResources.Lang.amigosTextoSolicitudYaAceptada,
                ["No es posible rechazar una solicitud de amistad que ya fue aceptada."] = () => LangResources.Lang.amigosTextoSolicitudRechazarNoPosible,
                ["No existe una solicitud de amistad pendiente entre los usuarios indicados."] = () => LangResources.Lang.amigosTextoSolicitudNoPendiente,
                ["Solo el destinatario original de la solicitud puede responderla."] = () => LangResources.Lang.amigosTextoSolicitudSoloDestinatario,
                ["Solicitud de amistad rechazada correctamente."] = () => LangResources.Lang.amigosTextoSolicitudRechazada,
                ["No fue posible rechazar la solicitud de amistad."] = () => LangResources.Lang.amigosTextoSolicitudRechazarFallo,
                ["No fue posible responder la solicitud de amistad."] = () => LangResources.Lang.amigosTextoSolicitudResponderFallo,
                ["Amistad eliminada correctamente."] = () => LangResources.Lang.amigosTextoAmistadEliminada,
                ["No existe una amistad registrada entre los usuarios indicados."] = () => LangResources.Lang.amigosTextoAmistadNoExiste,
                ["No fue posible eliminar la amistad."] = () => LangResources.Lang.amigosTextoAmistadNoEliminar,
                ["No es posible eliminar una amistad consigo mismo."] = () => LangResources.Lang.amigosTextoAmistadMismoUsuario,
                ["Los nombres de usuario son obligatorios."] = () => LangResources.Lang.amigosTextoUsuariosObligatorios,
                ["Debes proporcionar dos usuarios distintos."] = () => LangResources.Lang.amigosTextoUsuariosDistintos,
                ["La solicitud de amistad debe involucrar a dos usuarios distintos."] = () => LangResources.Lang.amigosTextoUsuariosDistintos,
                ["El nombre de usuario es obligatorio para suscribirse a las notificaciones de amistad."] = () => LangResources.Lang.amigosTextoSuscripcionUsuarioObligatorio,
                ["No fue posible completar la suscripción a las notificaciones de amistad."] = () => LangResources.Lang.errorTextoAmigosSuscripcion
            };

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
