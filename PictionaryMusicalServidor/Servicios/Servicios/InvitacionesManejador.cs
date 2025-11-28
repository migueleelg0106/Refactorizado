using log4net;
using Datos.Modelo;
using PictionaryMusicalServidor.Datos.Utilidades;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                ValidarSolicitud(invitacion);

                string codigoSala = invitacion.CodigoSala.Trim();
                string correo = invitacion.Correo.Trim();
                string idioma = invitacion.Idioma;

                var sala = SalasManejador.ObtenerSalaPorCodigo(codigoSala);
                ValidarSala(sala);

                if (sala.Jugadores != null && sala.Jugadores.Count > 0 && await UsuarioYaEnSalaAsync(correo, sala))
                {
                    throw new InvalidOperationException(MensajesError.Cliente.CorreoJugadorEnSala);
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

                _logger.InfoFormat("Invitación enviada a '{0}' para la sala {1}.", correo, codigoSala);

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = true,
                    Mensaje = MensajesError.Cliente.InvitacionEnviadaExito
                };
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Operación inválida al enviar invitación. Estado inconsistente o validación fallida.", ex);
                return CrearFallo(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Operación inválida al enviar invitación. Estado inconsistente o validación fallida.", ex);
                return CrearFallo(ex.Message);
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al enviar invitación. Fallo en la consulta de verificación de usuario.", ex);
                return CrearFallo(MensajesError.Cliente.ErrorProcesarInvitacion);
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al enviar invitación. No se pudo procesar la información del destinatario.", ex);
                return CrearFallo(MensajesError.Cliente.ErrorProcesarInvitacion);
            }
            catch (Exception ex)
            {
                _logger.Error("Operación inválida al enviar invitación. Estado inconsistente o validación fallida.", ex);
                return CrearFallo(MensajesError.Cliente.ErrorInesperadoInvitacion);
            }
        }

        private static void ValidarSolicitud(InvitacionSalaDTO invitacion)
        {
            if (invitacion == null)
            {
                throw new ArgumentException(MensajesError.Cliente.SolicitudInvitacionInvalida);
            }

            string codigoSala = invitacion.CodigoSala?.Trim();
            string correo = invitacion.Correo?.Trim();

            if (string.IsNullOrWhiteSpace(codigoSala) || string.IsNullOrWhiteSpace(correo))
            {
                throw new ArgumentException(MensajesError.Cliente.DatosInvitacionInvalidos);
            }

            if (!CorreoRegex.IsMatch(correo))
            {
                throw new ArgumentException(MensajesError.Cliente.CorreoInvalido);
            }
        }

        private static void ValidarSala(dynamic sala)
        {
            if (sala == null)
            {
                throw new InvalidOperationException(MensajesError.Cliente.SalaNoEncontrada);
            }
        }

        private static async Task<bool> UsuarioYaEnSalaAsync(string correo, dynamic sala)
        {
            using (var contexto = CrearContexto())
            {
                var usuario = await contexto.Usuario
                    .Include(u => u.Jugador)
                    .FirstOrDefaultAsync(u => u.Jugador.Correo == correo);

                if (string.IsNullOrWhiteSpace(usuario?.Nombre_Usuario))
                {
                    return false;
                }

                var listaJugadores = (IEnumerable<string>)sala.Jugadores;

                return listaJugadores.Contains(usuario.Nombre_Usuario, StringComparer.OrdinalIgnoreCase);
            }
        }

        private static BaseDatosPruebaEntities CrearContexto()
        {
            string cadenaConexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(cadenaConexion)
                ? new BaseDatosPruebaEntities()
                : new BaseDatosPruebaEntities(cadenaConexion);
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
