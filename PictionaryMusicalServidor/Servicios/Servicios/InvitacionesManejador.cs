using log4net;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Notificadores;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;
using System.Collections.Generic;
using System.Data;
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
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(InvitacionesManejador));

        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(500);

        private static readonly Regex CorreoRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            RegexTimeout);

        private readonly IContextoFactoria _contextoFactory;
        private readonly ISalasManejador _salasManejador;
        private readonly ICorreoInvitacionNotificador _correoNotificador;

        public InvitacionesManejador() : this(
            new ContextoFactoria(),
            new SalasManejador(),
            new CorreoInvitacionNotificador())
        {
        }

        public InvitacionesManejador(
            IContextoFactoria contextoFactory,
            ISalasManejador salasManejador,
            ICorreoInvitacionNotificador correoNotificador)
        {
            _contextoFactory = contextoFactory
                ?? throw new ArgumentNullException(nameof(contextoFactory));

            _salasManejador = salasManejador
                ?? throw new ArgumentNullException(nameof(salasManejador));

            _correoNotificador = correoNotificador
                ?? throw new ArgumentNullException(nameof(correoNotificador));
        }

        /// <summary>
        /// Envia una invitacion a una sala de juego a un usuario via correo electronico.
        /// </summary>
        public async Task<ResultadoOperacionDTO> EnviarInvitacionAsync(
            InvitacionSalaDTO invitacion)
        {
            try
            {
                ValidarDatosEntrada(invitacion);

                string codigoSala = invitacion.CodigoSala.Trim();
                string correo = invitacion.Correo.Trim();

                var sala = ObtenerYValidarSala(codigoSala);

                if (await VerificarUsuarioEnSala(correo, sala))
                {
                    throw new InvalidOperationException(
                        MensajesError.Cliente.CorreoJugadorEnSala);
                }

                return await EjecutarEnvioCorreo(correo, sala, invitacion.Idioma);
            }
            catch (FaultException excepcion)
            {
                _logger.Warn("Error de validacion al enviar invitacion.", excepcion);
                throw;
            }
            catch (ArgumentException excepcion)
            {
                _logger.Warn("Datos invalidos al enviar invitacion.", excepcion);
                return CrearFallo(excepcion.Message);
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Warn("Operacion invalida al enviar invitacion.", excepcion);
                return CrearFallo(excepcion.Message);
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error de base de datos al enviar invitacion.", excepcion);
                return CrearFallo(MensajesError.Cliente.ErrorProcesarInvitacion);
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error de datos al enviar invitacion.", excepcion);
                return CrearFallo(MensajesError.Cliente.ErrorProcesarInvitacion);
            }
            catch (RegexMatchTimeoutException excepcion)
            {
                _logger.Error("Timeout al validar el formato del correo de invitacion.", excepcion);
                return CrearFallo(MensajesError.Cliente.ErrorInesperadoInvitacion);
            }
            catch (AggregateException excepcion)
            {
                _logger.Error("Error inesperado al enviar invitacion.", excepcion);
                return CrearFallo(MensajesError.Cliente.ErrorInesperadoInvitacion);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error inesperado al enviar invitacion.", excepcion);
                return CrearFallo(MensajesError.Cliente.ErrorInesperadoInvitacion);
            }
        }

        private void ValidarDatosEntrada(InvitacionSalaDTO invitacion)
        {
            ValidarNulosVacios(invitacion);
            ValidarFormatoCorreo(invitacion.Correo);
        }

        private void ValidarNulosVacios(InvitacionSalaDTO invitacion)
        {
            if (invitacion == null)
            {
                throw new ArgumentException(
                    MensajesError.Cliente.SolicitudInvitacionInvalida);
            }

            if (string.IsNullOrWhiteSpace(invitacion.CodigoSala) ||
                string.IsNullOrWhiteSpace(invitacion.Correo))
            {
                throw new ArgumentException(
                    MensajesError.Cliente.DatosInvitacionInvalidos);
            }
        }

        private void ValidarFormatoCorreo(string correo)
        {
            if (!CorreoRegex.IsMatch(correo.Trim()))
            {
                throw new ArgumentException(MensajesError.Cliente.CorreoInvalido);
            }
        }

        private SalaDTO ObtenerYValidarSala(string codigoSala)
        {
            var sala = _salasManejador.ObtenerSalaPorCodigo(codigoSala);
            if (sala == null)
            {
                throw new InvalidOperationException(MensajesError.Cliente.SalaNoEncontrada);
            }
            return sala;
        }

        private async Task<bool> VerificarUsuarioEnSala(string correo, SalaDTO sala)
        {
            if (sala.Jugadores == null || sala.Jugadores.Count == 0)
            {
                return false;
            }

            return await UsuarioYaEnSalaAsync(correo, sala.Jugadores);
        }

        private async Task<bool> UsuarioYaEnSalaAsync(
            string correo,
            IEnumerable<string> jugadoresSala)
        {
            using (var contexto = _contextoFactory.CrearContexto())
            {
                IUsuarioRepositorio repositorio = new UsuarioRepositorio(contexto);
                var usuario = await repositorio.ObtenerPorCorreoAsync(correo);

                if (string.IsNullOrWhiteSpace(usuario?.Nombre_Usuario))
                {
                    return false;
                }

                return jugadoresSala.Contains(
                    usuario.Nombre_Usuario,
                    StringComparer.OrdinalIgnoreCase);
            }
        }

        private async Task<ResultadoOperacionDTO> EjecutarEnvioCorreo(
            string correo,
            SalaDTO sala,
            string idioma)
        {
            bool enviado = await _correoNotificador.EnviarInvitacionAsync(
                correo,
                sala.Codigo,
                sala.Creador,
                idioma).ConfigureAwait(false);

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
