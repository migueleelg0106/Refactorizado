using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using Datos.Modelo;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Servicio interno para la logica de negocio de recuperacion y cambio de contrasena.
    /// Maneja el almacenamiento temporal de solicitudes, generacion y validacion de codigos.
    /// Delega el acceso a datos a los repositorios correspondientes.
    /// </summary>
    public class RecuperacionCuentaServicio : IRecuperacionCuentaServicio
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(RecuperacionCuentaServicio));

        private const int MinutosExpiracionCodigo = 5;

        private static readonly ConcurrentDictionary<string, SolicitudRecuperacionPendiente>
            _solicitudesRecuperacion =
            new ConcurrentDictionary<string, SolicitudRecuperacionPendiente>();

        private readonly IContextoFactoria _contextoFactoria;
        private readonly IRepositorioFactoria _repositorioFactoria;
        private readonly INotificacionCodigosServicio _notificacionServicio;

        /// <summary>
        /// Constructor con inyeccion de dependencias para pruebas unitarias.
        /// </summary>
        /// <param name="contextoFactoria">Factoria para crear contextos de base de datos.</param>
        /// <param name="repositorioFactoria">Factoria para crear repositorios.</param>
        /// <param name="notificacionServicio">Servicio de notificacion de codigos.</param>
        public RecuperacionCuentaServicio(
            IContextoFactoria contextoFactoria,
            IRepositorioFactoria repositorioFactoria,
            INotificacionCodigosServicio notificacionServicio)
        {
            _contextoFactoria = contextoFactoria ??
                throw new ArgumentNullException(nameof(contextoFactoria));

            _repositorioFactoria = repositorioFactoria ??
                throw new ArgumentNullException(nameof(repositorioFactoria));

            _notificacionServicio = notificacionServicio ??
                throw new ArgumentNullException(nameof(notificacionServicio));
        }

        /// <summary>
        /// Constructor por defecto para uso en WCF (compatibilidad hacia atras).
        /// </summary>
        public RecuperacionCuentaServicio(
            IContextoFactoria contextoFactoria,
            INotificacionCodigosServicio notificacionServicio) 
            : this(contextoFactoria, new RepositorioFactoria(), notificacionServicio)
        {
        }

        /// <summary>
        /// Solicita un codigo de recuperacion para una cuenta de usuario.
        /// </summary>
        public ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(
            SolicitudRecuperarCuentaDTO solicitud)
        {
            if (!ValidarSolicitudEntrada(solicitud))
            {
                return CrearFalloSolicitud(
                    MensajesError.Cliente.SolicitudRecuperacionIdentificadorObligatorio);
            }

            var usuario = BuscarUsuarioParaRecuperacion(solicitud.Identificador);
            if (usuario == null)
            {
                return CrearFalloSolicitud(
                    MensajesError.Cliente.SolicitudRecuperacionCuentaNoEncontrada);
            }

            LimpiarSolicitudesRecuperacion(usuario.idUsuario);

            var generacion = GenerarYEnviarCodigo(usuario, solicitud.Idioma);
            if (!generacion.Exito)
            {
                return CrearFalloSolicitud(MensajesError.Cliente.ErrorRecuperarCuenta);
            }

            AlmacenarSolicitud(generacion.Token, generacion.Pendiente);

            return new ResultadoSolicitudRecuperacionDTO
            {
                CuentaEncontrada = true,
                CodigoEnviado = true,
                CorreoDestino = generacion.Pendiente.Correo,
                TokenCodigo = generacion.Token
            };
        }

        /// <summary>
        /// Reenvia un codigo de recuperacion previamente solicitado.
        /// </summary>
        public ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenvioCodigoDTO solicitud)
        {
            if (!ValidarReenvioEntrada(solicitud))
            {
                return CrearFalloReenvio(MensajesError.Cliente.DatosReenvioCodigo);
            }

            if (!_solicitudesRecuperacion.TryGetValue(
                solicitud.TokenCodigo,
                out SolicitudRecuperacionPendiente pendiente))
            {
                return CrearFalloReenvio(
                    MensajesError.Cliente.SolicitudRecuperacionNoEncontrada);
            }

            if (pendiente.Expira < DateTime.UtcNow)
            {
                _solicitudesRecuperacion.TryRemove(solicitud.TokenCodigo, out _);
                return CrearFalloReenvio(MensajesError.Cliente.CodigoRecuperacionExpirado);
            }

            return ProcesarReenvio(solicitud.TokenCodigo, pendiente);
        }

        /// <summary>
        /// Confirma el codigo de recuperacion ingresado por el usuario.
        /// </summary>
        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion(
            ConfirmacionCodigoDTO confirmacion)
        {
            if (!ValidarConfirmacionEntrada(confirmacion))
            {
                return CrearFalloOperacion(MensajesError.Cliente.DatosConfirmacionInvalidos);
            }

            if (!_solicitudesRecuperacion.TryGetValue(
                confirmacion.TokenCodigo,
                out SolicitudRecuperacionPendiente pendiente))
            {
                return CrearFalloOperacion(
                    MensajesError.Cliente.SolicitudRecuperacionNoEncontrada);
            }

            return VerificarCodigo(pendiente, confirmacion.TokenCodigo, 
                confirmacion.CodigoIngresado);
        }

        /// <summary>
        /// Actualiza la contrasena de un usuario despues de confirmar el codigo de recuperacion.
        /// </summary>
        public ResultadoOperacionDTO ActualizarContrasena(ActualizacionContrasenaDTO solicitud)
        {
            if (!ValidarActualizacionEntrada(solicitud))
            {
                return CrearFalloOperacion(MensajesError.Cliente.DatosActualizacionContrasena);
            }

            var validacionToken = VerificarTokenYExpiracion(solicitud.TokenCodigo);
            if (!validacionToken.Exito)
            {
                return CrearFalloOperacion(validacionToken.MensajeError);
            }

            var pendiente = validacionToken.Pendiente;
            if (!pendiente.Confirmado)
            {
                return CrearFalloOperacion(
                    MensajesError.Cliente.SolicitudRecuperacionNoVigente);
            }

            return EjecutarCambioContrasena(
                pendiente.UsuarioId,
                solicitud.NuevaContrasena,
                solicitud.TokenCodigo);
        }

        private bool ValidarSolicitudEntrada(SolicitudRecuperarCuentaDTO solicitud)
        {
            if (solicitud == null) return false;
            string identificador = EntradaComunValidador.NormalizarTexto(solicitud.Identificador);
            return EntradaComunValidador.EsLongitudValida(identificador);
        }

        private Usuario BuscarUsuarioParaRecuperacion(string identificador)
        {
            using (var contexto = _contextoFactoria.CrearContexto())
            {
                IUsuarioRepositorio repositorio = 
                    _repositorioFactoria.CrearUsuarioRepositorio(contexto);
                string idNormalizado = EntradaComunValidador.NormalizarTexto(identificador);
                var usuarioPorNombre = repositorio.ObtenerPorNombreConJugador(idNormalizado);

                if (usuarioPorNombre != null)
                {
                    return usuarioPorNombre;
                }

                return repositorio.ObtenerPorCorreo(idNormalizado);
            }
        }

        private (bool Exito, string Token, SolicitudRecuperacionPendiente Pendiente)
            GenerarYEnviarCodigo(Usuario usuario, string idioma)
        {
            string token = TokenGenerador.GenerarToken();
            string codigo = CodigoVerificacionGenerador.GenerarCodigo();

            var pendiente = new SolicitudRecuperacionPendiente
            {
                UsuarioId = usuario.idUsuario,
                Correo = usuario.Jugador?.Correo,
                NombreUsuario = usuario.Nombre_Usuario,
                Codigo = codigo,
                Expira = DateTime.UtcNow.AddMinutes(MinutosExpiracionCodigo),
                Confirmado = false,
                Idioma = idioma
            };

            bool enviado = _notificacionServicio.EnviarNotificacion(
                pendiente.Correo,
                codigo,
                pendiente.NombreUsuario,
                pendiente.Idioma);

            if (!enviado)
            {
                _logger.Error("Fallo critico al enviar correo de recuperacion.");
                return (false, null, null);
            }

            return (true, token, pendiente);
        }

        private void AlmacenarSolicitud(string token, SolicitudRecuperacionPendiente pendiente)
        {
            _solicitudesRecuperacion[token] = pendiente;
        }

        private ResultadoSolicitudRecuperacionDTO CrearFalloSolicitud(string mensaje)
        {
            return new ResultadoSolicitudRecuperacionDTO
            {
                CuentaEncontrada = false,
                CodigoEnviado = false,
                Mensaje = mensaje
            };
        }

        private static void LimpiarSolicitudesRecuperacion(int usuarioId)
        {
            var registros = _solicitudesRecuperacion
                .Where(solicitud => solicitud.Value.UsuarioId == usuarioId)
                .ToList();

            foreach (var registro in registros)
            {
                _solicitudesRecuperacion.TryRemove(registro.Key, out _);
            }
        }

        private bool ValidarReenvioEntrada(ReenvioCodigoDTO solicitud)
        {
            if (solicitud == null) return false;
            string token = EntradaComunValidador.NormalizarTexto(solicitud.TokenCodigo);
            return EntradaComunValidador.EsTokenValido(token);
        }

        private ResultadoSolicitudCodigoDTO ProcesarReenvio(
            string token,
            SolicitudRecuperacionPendiente pendiente)
        {
            string codigoAnterior = pendiente.Codigo;
            DateTime expiracionAnterior = pendiente.Expira;
            bool confirmadoAnterior = pendiente.Confirmado;

            string nuevoCodigo = CodigoVerificacionGenerador.GenerarCodigo();
            pendiente.Codigo = nuevoCodigo;
            pendiente.Expira = DateTime.UtcNow.AddMinutes(MinutosExpiracionCodigo);
            pendiente.Confirmado = false;

            bool enviado = _notificacionServicio.EnviarNotificacion(
                pendiente.Correo,
                nuevoCodigo,
                pendiente.NombreUsuario,
                pendiente.Idioma);

            if (!enviado)
            {
                pendiente.Codigo = codigoAnterior;
                pendiente.Expira = expiracionAnterior;
                pendiente.Confirmado = confirmadoAnterior;

                _logger.Error("Fallo critico al reenviar correo de recuperacion.");
                return CrearFalloReenvio(MensajesError.Cliente.ErrorReenviarCodigoRecuperacion);
            }

            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = true,
                TokenCodigo = token
            };
        }

        private ResultadoSolicitudCodigoDTO CrearFalloReenvio(string mensaje)
        {
            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = false,
                Mensaje = mensaje
            };
        }

        private bool ValidarConfirmacionEntrada(ConfirmacionCodigoDTO confirmacion)
        {
            if (confirmacion == null) return false;
            string token = EntradaComunValidador.NormalizarTexto(confirmacion.TokenCodigo);
            string codigo = EntradaComunValidador.NormalizarTexto(confirmacion.CodigoIngresado);

            return EntradaComunValidador.EsTokenValido(token) &&
                   EntradaComunValidador.EsCodigoVerificacionValido(codigo);
        }

        private ResultadoOperacionDTO VerificarCodigo(
            SolicitudRecuperacionPendiente pendiente,
            string token,
            string codigoIngresado)
        {
            if (pendiente.Expira < DateTime.UtcNow)
            {
                _solicitudesRecuperacion.TryRemove(token, out _);
                return CrearFalloOperacion(MensajesError.Cliente.CodigoRecuperacionExpirado);
            }

            if (!string.Equals(
                pendiente.Codigo,
                codigoIngresado,
                StringComparison.OrdinalIgnoreCase))
            {
                return CrearFalloOperacion(MensajesError.Cliente.CodigoRecuperacionIncorrecto);
            }

            pendiente.Confirmado = true;
            pendiente.Codigo = null;
            pendiente.Expira = DateTime.UtcNow.AddMinutes(MinutosExpiracionCodigo);

            return new ResultadoOperacionDTO { OperacionExitosa = true };
        }

        private bool ValidarActualizacionEntrada(ActualizacionContrasenaDTO solicitud)
        {
            if (solicitud == null) return false;
            string token = EntradaComunValidador.NormalizarTexto(solicitud.TokenCodigo);
            string pass = EntradaComunValidador.NormalizarTexto(solicitud.NuevaContrasena);

            return EntradaComunValidador.EsTokenValido(token) &&
                   EntradaComunValidador.EsContrasenaValida(pass);
        }

        private (bool Exito, SolicitudRecuperacionPendiente Pendiente, string MensajeError)
            VerificarTokenYExpiracion(string token)
        {
            if (!_solicitudesRecuperacion.TryGetValue(
                token,
                out SolicitudRecuperacionPendiente pendiente))
            {
                return (false, null, MensajesError.Cliente.SolicitudRecuperacionNoEncontrada);
            }

            if (pendiente.Expira < DateTime.UtcNow)
            {
                _solicitudesRecuperacion.TryRemove(token, out _);
                return (false, null, MensajesError.Cliente.SolicitudRecuperacionInvalida);
            }

            return (true, pendiente, null);
        }

        private ResultadoOperacionDTO EjecutarCambioContrasena(
            int usuarioId,
            string nuevaContrasena,
            string token)
        {
            try
            {
                using (var contexto = _contextoFactoria.CrearContexto())
                {
                    IUsuarioRepositorio repositorio = 
                        _repositorioFactoria.CrearUsuarioRepositorio(contexto);
                    string hash = BCrypt.Net.BCrypt.HashPassword(nuevaContrasena);

                    repositorio.ActualizarContrasena(usuarioId, hash);
                }

                _solicitudesRecuperacion.TryRemove(token, out _);
                return new ResultadoOperacionDTO { OperacionExitosa = true };
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error al actualizar contrasena.", excepcion);
                return CrearFalloOperacion(MensajesError.Cliente.ErrorActualizarContrasena);
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error al actualizar contrasena.", excepcion);
                return CrearFalloOperacion(MensajesError.Cliente.ErrorActualizarContrasena);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error al actualizar contrasena.", excepcion);
                return CrearFalloOperacion(MensajesError.Cliente.ErrorActualizarContrasena);
            }
        }

        private ResultadoOperacionDTO CrearFalloOperacion(string mensaje)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = mensaje
            };
        }

        private sealed class SolicitudRecuperacionPendiente
        {
            public int UsuarioId { get; set; }
            public string Correo { get; set; }
            public string NombreUsuario { get; set; }
            public string Codigo { get; set; }
            public DateTime Expira { get; set; }
            public bool Confirmado { get; set; }
            public string Idioma { get; set; }
        }
    }
}
