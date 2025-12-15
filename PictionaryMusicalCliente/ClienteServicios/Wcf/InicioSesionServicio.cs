using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Gestiona la autenticacion del usuario en el sistema.
    /// </summary>
    public class InicioSesionServicio : IInicioSesionServicio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(InicioSesionServicio));
        private readonly IWcfClienteEjecutor _ejecutor;
        private readonly IWcfClienteFabrica _fabricaClientes;
        private readonly IManejadorErrorServicio _manejadorError;
        private readonly IUsuarioMapeador _usuarioMapeador;
        private readonly ILocalizadorServicio _localizador;

        /// <summary>
        /// Inicializa el servicio de inicio de sesion.
        /// </summary>
        public InicioSesionServicio(
            IWcfClienteEjecutor ejecutor,
            IWcfClienteFabrica fabricaClientes,
            IManejadorErrorServicio manejadorError,
            IUsuarioMapeador usuarioMapeador,
            ILocalizadorServicio localizador)
        {
            _ejecutor = ejecutor ?? throw new ArgumentNullException(nameof(ejecutor));
            _fabricaClientes = fabricaClientes ??
                throw new ArgumentNullException(nameof(fabricaClientes));
            _manejadorError = manejadorError ??
                throw new ArgumentNullException(nameof(manejadorError));
            _usuarioMapeador = usuarioMapeador ??
                throw new ArgumentNullException(nameof(usuarioMapeador));
            _localizador = localizador ?? throw new ArgumentNullException(nameof(localizador));
        }

        /// <summary>
        /// Valida las credenciales del usuario e inicia la sesion local si es correcto.
        /// </summary>
        public async Task<DTOs.ResultadoInicioSesionDTO> IniciarSesionAsync(
            DTOs.CredencialesInicioSesionDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            try
            {
                var resultado = await _ejecutor.EjecutarAsincronoAsync(
                    _fabricaClientes.CrearClienteInicioSesion(),
                    c => c.IniciarSesionAsync(solicitud)
                ).ConfigureAwait(false);

                ProcesarResultadoSesion(resultado);
                return resultado;
            }
            catch (FaultException excepcion)
            {
                _logger.Warn("Error de logica servidor en inicio de sesion.", excepcion);
                string mensaje = _manejadorError.ObtenerMensaje(
                    excepcion,
                    Lang.errorTextoServidorInicioSesion);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, excepcion);
            }
            catch (CommunicationException excepcion)
            {
                _logger.Error("Error WCF en inicio de sesion.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    excepcion);
            }
            catch (TimeoutException excepcion)
            {
                _logger.Error("Timeout en inicio de sesion.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    excepcion);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error inesperado en inicio de sesion.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    excepcion);
            }
        }

        private void ProcesarResultadoSesion(DTOs.ResultadoInicioSesionDTO resultado)
        {
            if (resultado == null)
            {
                _logger.Warn("Servicio retorno null en inicio de sesion.");
                return;
            }

            _usuarioMapeador.ActualizarSesion(resultado.Usuario);
            resultado.Mensaje = _localizador.Localizar(resultado.Mensaje, resultado.Mensaje);

            if (resultado.Usuario != null)
            {
                _logger.InfoFormat("Usuario con id '{0}' inicio sesion.", 
                    resultado.Usuario.UsuarioId);
            }
            else
            {
                _logger.Warn("Log in fallido: Credenciales incorrectas o cuenta no encontrada.");
            }
        }
    }
}