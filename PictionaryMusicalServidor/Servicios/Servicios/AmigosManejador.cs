using System;
using System.Data;
using System.Data.Entity.Core;
using System.ServiceModel;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Datos.Modelo;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using PictionaryMusicalServidor.Servicios.Servicios.Notificadores;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de gestion de amistades entre usuarios.
    /// Maneja suscripciones para notificaciones, envio y respuesta de solicitudes de amistad,
    /// y eliminacion de relaciones de amistad con notificaciones en tiempo real via callbacks.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AmigosManejador : IAmigosManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AmigosManejador));
        private static readonly ManejadorCallback<IAmigosManejadorCallback> _manejadorCallback = new(StringComparer.OrdinalIgnoreCase);
        private static readonly NotificadorAmigos _notificador = new(_manejadorCallback);

        /// <summary>
        /// Suscribe un usuario para recibir notificaciones de solicitudes de amistad.
        /// Normaliza el nombre de usuario, registra el callback y notifica solicitudes pendientes.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario a suscribir.</param>
        /// <exception cref="FaultException">Se lanza si el nombre de usuario es invalido, no existe, o hay errores de base de datos.</exception>
        public void Suscribir(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new FaultException(MensajesError.Cliente.NombreUsuarioObligatorioSuscripcion);
            }

            Usuario usuario;
            string nombreNormalizado;
            IAmigosManejadorCallback callback;

            try
            {
                using (var contexto = ContextoFactory.CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    usuario = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuario);

                    if (usuario == null)
                    {
                        throw new FaultException(MensajesError.Cliente.UsuarioNoEncontrado);
                    }

                    nombreNormalizado = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuario.Nombre_Usuario, nombreUsuario);
                }

                if (string.IsNullOrWhiteSpace(nombreNormalizado))
                {
                    throw new FaultException(MensajesError.Cliente.UsuarioNoEncontrado);
                }

                callback = ManejadorCallback<IAmigosManejadorCallback>.ObtenerCallbackActual();
                _manejadorCallback.Suscribir(nombreNormalizado, callback);

                if (!string.Equals(nombreUsuario, nombreNormalizado, StringComparison.Ordinal))
                {
                    _manejadorCallback.Desuscribir(nombreUsuario);
                }

                _manejadorCallback.ConfigurarEventosCanal(nombreNormalizado);

                _notificador.NotificarSolicitudesPendientesAlSuscribir(nombreNormalizado, usuario.idUsuario);

                _logger.Info($"Usuario '{nombreNormalizado}' suscrito a notificaciones de amistad.");
            }
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.AmistadSuscribirErrorBD, ex);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarSolicitudes);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.AmistadSuscribirErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarSolicitudes);
            }
        }

        /// <summary>
        /// Cancela la suscripcion de un usuario de notificaciones de amistad.
        /// Elimina el callback del usuario del manejador de callbacks.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario que cancela la suscripcion.</param>
        /// <exception cref="FaultException">Se lanza si el nombre de usuario es invalido.</exception>
        public void CancelarSuscripcion(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new FaultException(MensajesError.Cliente.NombreUsuarioObligatorioCancelar);
            }

            _manejadorCallback.Desuscribir(nombreUsuario);
        }

        /// <summary>
        /// Envia una solicitud de amistad de un usuario a otro.
        /// Valida que ambos usuarios existan, crea la solicitud en la base de datos y notifica al receptor.
        /// </summary>
        /// <param name="nombreUsuarioEmisor">Nombre del usuario que envia la solicitud.</param>
        /// <param name="nombreUsuarioReceptor">Nombre del usuario que recibe la solicitud.</param>
        /// <exception cref="FaultException">Se lanza si los nombres son invalidos, los usuarios no existen, o hay errores de base de datos.</exception>
        public void EnviarSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor)
        {
            try
            {
                ValidadorNombreUsuario.Validar(nombreUsuarioEmisor, nameof(nombreUsuarioEmisor));
                ValidadorNombreUsuario.Validar(nombreUsuarioReceptor, nameof(nombreUsuarioReceptor));

                Usuario usuarioEmisor;
                Usuario usuarioReceptor;

                using (var contexto = ContextoFactory.CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    usuarioEmisor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioEmisor);
                    usuarioReceptor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioReceptor);

                    if (usuarioEmisor == null)
                    {
                        throw new FaultException(MensajesError.Cliente.JugadorNoAsociado);
                    }

                    if (usuarioReceptor == null)
                    {
                        throw new FaultException(MensajesError.Cliente.UsuarioNoEncontrado);
                    }

                    ServicioAmistad.CrearSolicitud(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario);
                }

                string nombreEmisor = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioEmisor.Nombre_Usuario, nombreUsuarioEmisor);
                string nombreReceptor = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioReceptor.Nombre_Usuario, nombreUsuarioReceptor);

                var solicitud = new SolicitudAmistadDTO
                {
                    UsuarioEmisor = nombreEmisor,
                    UsuarioReceptor = nombreReceptor,
                    SolicitudAceptada = false
                };

                _logger.Info($"Solicitud de amistad enviada de '{nombreEmisor}' a '{nombreReceptor}'.");
                _notificador.NotificarSolicitudActualizada(nombreReceptor, solicitud);
            }
            catch (FaultException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.AmistadEnviarSolicitudReglaNegocio, ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.AmistadEnviarSolicitudDatosInvalidos, ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.AmistadEnviarSolicitudErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorAlmacenarSolicitud);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.AmistadEnviarSolicitudErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorAlmacenarSolicitud);
            }
        }

        /// <summary>
        /// Responde una solicitud de amistad aceptandola.
        /// Actualiza la solicitud en la base de datos, notifica a ambos usuarios y actualiza sus listas de amigos.
        /// </summary>
        /// <param name="nombreUsuarioEmisor">Nombre del usuario que envio la solicitud original.</param>
        /// <param name="nombreUsuarioReceptor">Nombre del usuario que responde la solicitud.</param>
        /// <exception cref="FaultException">Se lanza si los nombres son invalidos, los usuarios no existen, o hay errores de base de datos.</exception>
        public void ResponderSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor)
        {
            string nombreEmisorNormalizado;
            string nombreReceptorNormalizado;

            try
            {
                ValidadorNombreUsuario.Validar(nombreUsuarioEmisor, nameof(nombreUsuarioEmisor));
                ValidadorNombreUsuario.Validar(nombreUsuarioReceptor, nameof(nombreUsuarioReceptor));

                using (var contexto = ContextoFactory.CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    Usuario usuarioEmisor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioEmisor);
                    Usuario usuarioReceptor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioReceptor);

                    if (usuarioEmisor == null || usuarioReceptor == null)
                    {
                        throw new FaultException(MensajesError.Cliente.UsuariosEspecificadosNoExisten);
                    }

                    ServicioAmistad.AceptarSolicitud(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario);

                    nombreEmisorNormalizado = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioEmisor.Nombre_Usuario, nombreUsuarioEmisor);
                    nombreReceptorNormalizado = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioReceptor.Nombre_Usuario, nombreUsuarioReceptor);

                    var solicitud = new SolicitudAmistadDTO
                    {
                        UsuarioEmisor = nombreEmisorNormalizado,
                        UsuarioReceptor = nombreReceptorNormalizado,
                        SolicitudAceptada = true
                    };

                    _logger.Info($"Solicitud de amistad aceptada entre '{nombreEmisorNormalizado}' y '{nombreReceptorNormalizado}'.");
                    _notificador.NotificarSolicitudActualizada(nombreEmisorNormalizado, solicitud);
                    _notificador.NotificarSolicitudActualizada(nombreReceptorNormalizado, solicitud);
                }

                ListaAmigosManejador.NotificarCambioAmistad(nombreEmisorNormalizado);
                ListaAmigosManejador.NotificarCambioAmistad(nombreReceptorNormalizado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.AmistadResponderSolicitudReglaNegocio, ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.AmistadResponderSolicitudDatosInvalidos, ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.AmistadResponderSolicitudErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorActualizarSolicitud);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.AmistadResponderSolicitudErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorActualizarSolicitud);
            }
        }

        /// <summary>
        /// Elimina la relacion de amistad entre dos usuarios.
        /// Elimina la amistad de la base de datos, notifica a ambos usuarios y actualiza sus listas de amigos.
        /// </summary>
        /// <param name="nombreUsuarioA">Nombre del primer usuario.</param>
        /// <param name="nombreUsuarioB">Nombre del segundo usuario.</param>
        /// <exception cref="FaultException">Se lanza si los nombres son invalidos, los usuarios no existen, o hay errores de base de datos.</exception>
        public void EliminarAmigo(string nombreUsuarioA, string nombreUsuarioB)
        {
            string nombreUsuarioANormalizado;
            string nombreUsuarioBNormalizado;

            try
            {
                ValidadorNombreUsuario.Validar(nombreUsuarioA, nameof(nombreUsuarioA));
                ValidadorNombreUsuario.Validar(nombreUsuarioB, nameof(nombreUsuarioB));

                Amigo relacionEliminada;
                int idUsuarioA;

                using (var contexto = ContextoFactory.CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    Usuario usuarioA = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioA);
                    Usuario usuarioB = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioB);

                    if (usuarioA == null || usuarioB == null)
                    {
                        throw new FaultException(MensajesError.Cliente.UsuariosEspecificadosNoExisten);
                    }

                    idUsuarioA = usuarioA.idUsuario;

                    relacionEliminada = ServicioAmistad.EliminarAmistad(usuarioA.idUsuario, usuarioB.idUsuario);

                    nombreUsuarioANormalizado = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioA.Nombre_Usuario, nombreUsuarioA);
                    nombreUsuarioBNormalizado = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioB.Nombre_Usuario, nombreUsuarioB);
                }

                bool usuarioAEsEmisor = relacionEliminada.UsuarioEmisor == idUsuarioA;
                string emisor = usuarioAEsEmisor ? nombreUsuarioANormalizado : nombreUsuarioBNormalizado;
                string receptor = usuarioAEsEmisor ? nombreUsuarioBNormalizado : nombreUsuarioANormalizado;

                var solicitud = new SolicitudAmistadDTO
                {
                    UsuarioEmisor = emisor,
                    UsuarioReceptor = receptor,
                    SolicitudAceptada = false
                };

                _notificador.NotificarAmistadEliminada(nombreUsuarioANormalizado, solicitud);
                _notificador.NotificarAmistadEliminada(nombreUsuarioBNormalizado, solicitud);

                _logger.Info($"Amistad eliminada entre '{nombreUsuarioANormalizado}' y '{nombreUsuarioBNormalizado}'.");
                ListaAmigosManejador.NotificarCambioAmistad(nombreUsuarioANormalizado);
                ListaAmigosManejador.NotificarCambioAmistad(nombreUsuarioBNormalizado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.AmistadEliminarReglaNegocio, ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.AmistadEliminarDatosInvalidos, ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.AmistadEliminarErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorEliminarAmistad);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.AmistadEliminarErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorEliminarAmistad);
            }
        }
    }
}