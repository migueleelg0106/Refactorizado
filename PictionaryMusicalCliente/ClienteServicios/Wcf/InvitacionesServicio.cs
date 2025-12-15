using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Properties.Langs;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Maneja el envio de invitaciones a partidas mediante correo electronico.
    /// </summary>
    public class InvitacionesServicio : IInvitacionesServicio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(InvitacionesServicio));
        private readonly IWcfClienteEjecutor _ejecutor;
        private readonly IWcfClienteFabrica _fabricaClientes;
        private readonly IManejadorErrorServicio _manejadorError;
        private readonly ILocalizadorServicio _localizador;

        /// <summary>
        /// Inicializa el servicio de invitaciones.
        /// </summary>
        public InvitacionesServicio(
            IWcfClienteEjecutor ejecutor,
            IWcfClienteFabrica fabricaClientes,
            IManejadorErrorServicio manejadorError,
            ILocalizadorServicio localizador)
        {
            _ejecutor = ejecutor ?? throw new ArgumentNullException(nameof(ejecutor));
            _fabricaClientes = fabricaClientes ??
                throw new ArgumentNullException(nameof(fabricaClientes));
            _manejadorError = manejadorError ??
                throw new ArgumentNullException(nameof(manejadorError));
            _localizador = localizador ?? throw new ArgumentNullException(nameof(localizador));
        }

        /// <summary>
        /// Envia una invitacion por correo para unirse a una sala especifica.
        /// </summary>
        public async Task<DTOs.ResultadoOperacionDTO> EnviarInvitacionAsync(
            string codigoSala,
            string correoDestino)
        {
            ValidarDatosEntrada(codigoSala, correoDestino);

            var solicitud = new DTOs.InvitacionSalaDTO
            {
                CodigoSala = codigoSala.Trim(),
                Correo = correoDestino.Trim(),
                Idioma = ObtenerCodigoIdiomaActual()
            };

            try
            {
                var resultado = await _ejecutor.EjecutarAsincronoAsync(
                    _fabricaClientes.CrearClienteInvitaciones(),
                    c => c.EnviarInvitacionAsync(solicitud)
                ).ConfigureAwait(false);

                _logger.InfoFormat("Invitacion enviada a '{0}' para sala '{1}'.",
                    correoDestino, codigoSala);
                return resultado;
            }
            catch (FaultException excepcion)
            {
                _logger.Warn("El servidor rechazo la invitacion.", excepcion);
                string mensaje = _manejadorError.ObtenerMensaje(excepcion, 
                    Lang.errorTextoEnviarCorreo);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, excepcion);
            }
            catch (CommunicationException excepcion)
            {
                _logger.Error("Error WCF al enviar invitacion.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    excepcion);
            }
            catch (TimeoutException excepcion)
            {
                _logger.Error("Timeout al enviar invitacion.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    excepcion);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error inesperado en invitaciones.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    excepcion);
            }
        }

        private static void ValidarDatosEntrada(string codigoSala, string correo)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
                throw new ArgumentException("Codigo sala obligatorio.", nameof(codigoSala));

            if (string.IsNullOrWhiteSpace(correo))
                throw new ArgumentException("Correo destino obligatorio.", nameof(correo));
        }

        private string ObtenerCodigoIdiomaActual()
        {
            var culturaActual = Lang.Culture;
            if (culturaActual != null)
            {
                return culturaActual.Name;
            }

            return System.Globalization.CultureInfo.CurrentUICulture.Name;
        }
    }
}