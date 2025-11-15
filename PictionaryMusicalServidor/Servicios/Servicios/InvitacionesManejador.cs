using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Datos.Utilidades;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    public class InvitacionesManejador : IInvitacionesManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(InvitacionesManejador));
        private static readonly Regex CorreoRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public ResultadoOperacionDTO EnviarInvitacion(InvitacionSalaDTO invitacion)
        {
            if (invitacion == null)
            {
                return CrearFallo(MensajesError.Cliente.SolicitudInvitacionInvalida);
            }

            string codigoSala = invitacion.CodigoSala?.Trim();
            string correo = invitacion.Correo?.Trim();

            if (string.IsNullOrWhiteSpace(codigoSala) || string.IsNullOrWhiteSpace(correo))
            {
                return CrearFallo(MensajesError.Cliente.DatosInvitacionInvalidos);
            }

            if (!CorreoRegex.IsMatch(correo))
            {
                return CrearFallo(MensajesError.Cliente.CorreoInvalido);
            }

            var sala = SalasManejador.ObtenerSalaPorCodigo(codigoSala);
            if (sala == null)
            {
                return CrearFallo(MensajesError.Cliente.SalaNoEncontrada);
            }

            try
            {
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
                            return CrearFallo(MensajesError.Cliente.CorreoJugadorEnSala);
                        }
                    }
                }

                bool enviado = CorreoInvitacionNotificador.EnviarInvitacion(correo, sala.Codigo, sala.Creador);

                if (!enviado)
                {
                    return CrearFallo(MensajesError.Cliente.ErrorEnviarInvitacionCorreo);
                }

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = true,
                    Mensaje = MensajesError.Cliente.InvitacionEnviadaExito
                };
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
            catch (InvalidOperationException ex)
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
