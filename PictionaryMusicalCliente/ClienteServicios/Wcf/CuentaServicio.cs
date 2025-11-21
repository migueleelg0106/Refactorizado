using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Servicio encargado de la creacion de nuevas cuentas de usuario.
    /// </summary>
    public class CuentaServicio : ICuentaServicio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CuentaServicio));
        private const string CuentaEndpoint = "BasicHttpBinding_ICuentaManejador";

        /// <summary>
        /// Envia una solicitud de registro al servidor.
        /// </summary>
        public async Task<DTOs.ResultadoRegistroCuentaDTO> RegistrarCuentaAsync(
            DTOs.NuevaCuentaDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            var cliente = new PictionaryServidorServicioCuenta.CuentaManejadorClient
                (CuentaEndpoint);

            try
            {
                var resultado = await WcfClienteAyudante
                    .UsarAsincronoAsync(cliente, c => c.RegistrarCuentaAsync(solicitud))
                    .ConfigureAwait(false);

                if (resultado != null && resultado.RegistroExitoso)
                {
                    _logger.Info("Registro de cuenta exitoso para: {0}", solicitud.Correo);
                }
                else
                {
                    _logger.Warn("Registro de cuenta fallido para: {0}. " +
                        "Raz�n: {1}", solicitud.Correo, resultado?.Mensaje);
                }

                return resultado;
            }
            catch (FaultException ex)
            {
                _logger.Warn("Servidor rechaz� el registro (Validaci�n/Negocio).", ex);
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                    ex,
                    Lang.errorTextoRegistrarCuentaMasTarde);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                _logger.Error("Endpoint de cuenta no encontrado.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout al registrar cuenta.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicaci�n al registrar cuenta.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operaci�n inv�lida al registrar cuenta.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }
    }
}