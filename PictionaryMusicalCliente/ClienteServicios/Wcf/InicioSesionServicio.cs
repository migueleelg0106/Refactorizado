using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
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
        private const string Endpoint = "BasicHttpBinding_IInicioSesionManejador";

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

            var cliente = new PictionaryServidorServicioInicioSesion
                .InicioSesionManejadorClient(Endpoint);

            try
            {
                var resultadoDto = await WcfClienteAyudante
                    .UsarAsincronoAsync(cliente, c => c.IniciarSesionAsync(solicitud))
                    .ConfigureAwait(false);

                if (resultadoDto == null)
                {
                    _logger.Warn("El servicio de inicio de sesión retornó null.");
                    return null;
                }

                UsuarioMapeador.ActualizarSesion(resultadoDto.Usuario);

                resultadoDto.Mensaje = MensajeServidorAyudante.Localizar(
                    resultadoDto.Mensaje,
                    resultadoDto.Mensaje);

                if (resultadoDto.Usuario != null)
                {
                    UsuarioAutenticado.Instancia.CargarDesdeDTO(resultadoDto.Usuario);
                    _logger.InfoFormat("Usuario '{0}' inició sesión exitosamente.", 
                        resultadoDto.Usuario.NombreUsuario);
                }
                else
                {
                    _logger.Warn("Inicio de sesión fallido: Credenciales incorrectas o cuenta no" +
                        " encontrada.");
                }

                return resultadoDto;
            }
            catch (FaultException ex)
            {
                _logger.Warn("Error de lógica en el servidor durante inicio de sesión.", ex);
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorInicioSesion);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                _logger.Error("No se pudo conectar con el endpoint de inicio de sesión.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Tiempo de espera agotado durante inicio de sesión.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicación WCF durante inicio de sesión.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operación inválida al iniciar sesión (Estado del cliente " +
                    "incorrecto).", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }
    }
}