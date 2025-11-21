using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Datos.Utilidades;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using System.ServiceModel;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de gestion de invitaciones a salas de juego.
    /// Maneja el envio de invitaciones por correo electronico a usuarios para unirse a salas.
    /// </summary>
    public class InvitacionesManejador : IInvitacionesManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(InvitacionesManejador));
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(500);

        private static readonly Regex CorreoRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,RegexTimeout);

        /// <summary>
        /// Envia una invitacion a una sala de juego a un usuario via correo electronico.
        /// Valida el correo, verifica que la sala exista y que el usuario no este ya en la sala.
        /// </summary>
        /// <param name="invitacion">Datos de la invitacion con codigo de sala y correo destino.</param>
        /// <returns>Resultado del envio indicando exito o fallo con mensaje descriptivo.</returns>
        public async Task<ResultadoOperacionDTO> EnviarInvitacionAsync(InvitacionSalaDTO invitacion)
        {
            try
            {
                if (invitacion == null)
                {
                    throw new ArgumentException(MensajesError.Cliente.SolicitudInvitacionInvalida);
                }

                string codigoSala = invitacion.CodigoSala?.Trim();
                string correo = invitacion.Correo?.Trim();
                string idioma = invitacion.Idioma;

                if (string.IsNullOrWhiteSpace(codigoSala) || string.IsNullOrWhiteSpace(correo))
                {
                    throw new ArgumentException(MensajesError.Cliente.DatosInvitacionInvalidos);
                }

                if (!CorreoRegex.IsMatch(correo))
                {
                    throw new ArgumentException(MensajesError.Cliente.CorreoInvalido);
                }

                var sala = SalasManejador.ObtenerSalaPorCodigo(codigoSala);
                if (sala == null)
                {
                    throw new InvalidOperationException(MensajesError.Cliente.SalaNoEncontrada);
                }

                if (sala.Jugadores != null && sala.Jugadores.Count > 0)
                {
                    using (var contexto = CrearContexto())
                    {
                        var usuario = contexto.Usuario
                            .Include(u => u.Jugador)
                            .FirstOrDefault(u => u.Jugador.Correo == correo);

                        if (!string.IsNullOrWhiteSpace(usuario?.Nombre_Usuario)
                            && sala.Jugadores.Contains(usuario.Nombre_Usuario, StringComparer.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException(MensajesError.Cliente.CorreoJugadorEnSala);
                        }
                    }
                }

                bool enviado = await CorreoInvitacionNotificador.EnviarInvitacionAsync(
                    correo,
                    sala.Codigo,
                    sala.Creador,
                    idioma).ConfigureAwait(false);

                if (!enviado)
                {
                    return CrearFallo(MensajesError.Cliente.ErrorEnviarInvitacionCorreo);
                }

                _logger.Info($"Invitación enviada a '{correo}' para la sala {codigoSala}.");

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = true,
                    Mensaje = MensajesError.Cliente.InvitacionEnviadaExito
                };
            }
            catch (FaultException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.InvitacionOperacionInvalida, ex);
                return CrearFallo(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.InvitacionOperacionInvalida, ex);
                return CrearFallo(ex.Message);
            }
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.InvitacionErrorBD, ex);
                return CrearFallo(MensajesError.Cliente.ErrorProcesarInvitacion);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.InvitacionErrorDatos, ex);
                return CrearFallo(MensajesError.Cliente.ErrorProcesarInvitacion);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.InvitacionOperacionInvalida, ex);
                return CrearFallo(MensajesError.Cliente.ErrorInesperadoInvitacion);
            }
        }

        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string cadenaConexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(cadenaConexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(cadenaConexion);
        }

        private static ResultadoOperacionDTO CrearFallo(string mensaje)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = mensaje
            };
        }
    }
}
