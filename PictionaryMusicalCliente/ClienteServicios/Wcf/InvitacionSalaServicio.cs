using log4net;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Implementacion
{
    /// <summary>
    /// Encapsula la logica de invitaciones para reducir la carga del ViewModel principal.
    /// </summary>
    public class InvitacionSalaServicio : IInvitacionSalaServicio
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(InvitacionSalaServicio));

        private readonly IInvitacionesServicio _invitacionesServicio;
        private readonly IListaAmigosServicio _listaAmigosServicio;
        private readonly IPerfilServicio _perfilServicio;
        private readonly IValidadorEntrada _validador;
        private readonly ISonidoManejador _sonidoManejador;
        private readonly IAvisoServicio _aviso;
        private bool _disposed;

        /// <summary>
        /// Inicializa el servicio facade de invitaciones de sala.
        /// </summary>
        public InvitacionSalaServicio(
            IInvitacionesServicio invitacionesServicio,
            IListaAmigosServicio listaAmigosServicio,
            IPerfilServicio perfilServicio,
            IValidadorEntrada validador,
            ISonidoManejador sonidoManejador,
            IAvisoServicio aviso)
        {
            _invitacionesServicio = invitacionesServicio ??
                throw new ArgumentNullException(nameof(invitacionesServicio));
            _listaAmigosServicio = listaAmigosServicio ??
                throw new ArgumentNullException(nameof(listaAmigosServicio));
            _perfilServicio = perfilServicio ??
                throw new ArgumentNullException(nameof(perfilServicio));
            _validador = validador ??
                throw new ArgumentNullException(nameof(validador));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            _aviso = aviso ??
                throw new ArgumentNullException(nameof(aviso));
        }

        /// <summary>
        /// Envia una invitacion por correo validando el formato previamente.
        /// </summary>
        public async Task<InvitacionCorreoResultado> InvitarPorCorreoAsync(
            string codigoSala,
            string correo)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                throw new ArgumentNullException(nameof(codigoSala));
            }

            string correoLimpio = correo?.Trim();

            var validacion = _validador.ValidarCorreo(correoLimpio);

            if (!validacion.OperacionExitosa)
            {
                return InvitacionCorreoResultado.Fallo(
                    validacion.Mensaje ?? Lang.errorTextoCorreoInvalido);
            }

            return await ProcesarEnvioCorreoAsync(codigoSala, correoLimpio).ConfigureAwait(false);
        }

        /// <summary>
        /// Prepara el ViewModel para invitar amigos conectados.
        /// </summary>
        public async Task<InvitacionAmigosResultado> ObtenerInvitacionAmigosAsync(
            string codigoSala,
            string nombreUsuarioSesion,
            ISet<int> amigosInvitados,
            Action<string> mostrarMensaje)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuarioSesion))
            {
                _logger.Warn("Intento de invitar amigos sin usuario de sesion.");
                return InvitacionAmigosResultado.Fallo(Lang.errorTextoErrorProcesarSolicitud);
            }

            try
            {
                var amigos = await _listaAmigosServicio
                    .ObtenerAmigosAsync(nombreUsuarioSesion)
                    .ConfigureAwait(false);

                if (amigos == null || amigos.Count == 0)
                {
                    return InvitacionAmigosResultado.Fallo(Lang.invitarAmigosTextoSinAmigos);
                }

                var vm = new InvitarAmigosVistaModelo(
                    App.VentanaServicio,
                    App.Localizador,
                    amigos,
                    _invitacionesServicio,
                    _perfilServicio,
                    _sonidoManejador,
                    _aviso,
                    codigoSala,
                    id => amigosInvitados?.Contains(id) ?? false,
                    id => amigosInvitados?.Add(id)
                );

                return InvitacionAmigosResultado.Exito(vm);
            }
            catch (Exception ex)
            {
                _logger.Error("Error al obtener lista de amigos para invitar.", ex);
                return InvitacionAmigosResultado.Fallo(
                    ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
        }

        private async Task<InvitacionCorreoResultado> ProcesarEnvioCorreoAsync(
            string codigoSala,
            string correo)
        {
            try
            {
                _logger.InfoFormat("Enviando invitacion a: {0}", correo);
                var resultado = await _invitacionesServicio
                    .EnviarInvitacionAsync(codigoSala, correo)
                    .ConfigureAwait(false);

                if (resultado != null && resultado.OperacionExitosa)
                {
                    return InvitacionCorreoResultado.Exito(Lang.invitarCorreoTextoEnviado);
                }

                return InvitacionCorreoResultado.Fallo(
                    resultado?.Mensaje ?? Lang.errorTextoEnviarCorreo);
            }
            catch (Exception ex)
            {
                _logger.Error("Error al procesar envio de correo.", ex);
                return InvitacionCorreoResultado.Fallo(Lang.errorTextoErrorProcesarSolicitud);
            }
        }
    }
}