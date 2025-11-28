using System;
using System.Data;
using System.Data.Entity.Core;
using System.ServiceModel;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using Datos.Modelo;
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

        private readonly IContextoFactory _contextoFactory;
        private readonly IAmistadServicio _amistadServicio;

        /// <summary>
        /// Constructor por defecto para compatibilidad con WCF.
        /// </summary>
        public AmigosManejador() : this(CrearDependenciasPorDefecto())
        {
        }

        /// <summary>
        /// Crea las dependencias por defecto, reutilizando la misma instancia de ContextoFactory.
        /// </summary>
        private static (IContextoFactory, IAmistadServicio) CrearDependenciasPorDefecto()
        {
            var contextoFactory = new ContextoFactory();
            var amistadServicio = new AmistadServicio(contextoFactory);
            return (contextoFactory, amistadServicio);
        }

        /// <summary>
        /// Constructor interno que recibe una tupla con las dependencias.
        /// </summary>
        private AmigosManejador((IContextoFactory contextoFactory, IAmistadServicio amistadServicio) dependencias)
            : this(dependencias.contextoFactory, dependencias.amistadServicio)
        {
        }

        /// <summary>
        /// Constructor que permite inyectar dependencias para pruebas unitarias.
        /// </summary>
        /// <param name="contextoFactory">Factoría para crear contextos de base de datos.</param>
        /// <param name="amistadServicio">Servicio de logica de negocio de amistades.</param>
        public AmigosManejador(IContextoFactory contextoFactory, IAmistadServicio amistadServicio)
        {
            _contextoFactory = contextoFactory ?? throw new ArgumentNullException(nameof(contextoFactory));
            _amistadServicio = amistadServicio ?? throw new ArgumentNullException(nameof(amistadServicio));
        }

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
                using (var contexto = _contextoFactory.CrearContexto())
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

                _logger.InfoFormat("Usuario '{0}' suscrito a notificaciones de amistad.", nombreNormalizado);
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al suscribir a notificaciones de amistad. Fallo en la consulta de usuario o solicitudes.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarSolicitudes);
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al suscribir a notificaciones de amistad. No se pudieron recuperar las solicitudes pendientes.", ex);
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

                using (var contexto = _contextoFactory.CrearContexto())
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

                    _amistadServicio.CrearSolicitud(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario);
                }

                string nombreEmisor = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioEmisor.Nombre_Usuario, nombreUsuarioEmisor);
                string nombreReceptor = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioReceptor.Nombre_Usuario, nombreUsuarioReceptor);

                var solicitud = new SolicitudAmistadDTO
                {
                    UsuarioEmisor = nombreEmisor,
                    UsuarioReceptor = nombreReceptor,
                    SolicitudAceptada = false
                };

                _logger.InfoFormat("Solicitud de amistad enviada de '{0}' a '{1}'.", nombreEmisor, nombreReceptor);
                _notificador.NotificarSolicitudActualizada(nombreReceptor, solicitud);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Regla de negocio violada al enviar solicitud de amistad.", ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al enviar solicitud de amistad.", ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al enviar solicitud de amistad. No se pudo almacenar la solicitud en la base de datos.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorAlmacenarSolicitud);
            }
            catch (Exception ex)
            {
                _logger.Error("Error de datos al enviar solicitud de amistad. No se pudo almacenar la solicitud en la base de datos.", ex);
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

                using (var contexto = _contextoFactory.CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    Usuario usuarioEmisor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioEmisor);
                    Usuario usuarioReceptor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioReceptor);

                    if (usuarioEmisor == null || usuarioReceptor == null)
                    {
                        throw new FaultException(MensajesError.Cliente.UsuariosEspecificadosNoExisten);
                    }

                    _amistadServicio.AceptarSolicitud(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario);

                    nombreEmisorNormalizado = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioEmisor.Nombre_Usuario, nombreUsuarioEmisor);
                    nombreReceptorNormalizado = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioReceptor.Nombre_Usuario, nombreUsuarioReceptor);

                    var solicitud = new SolicitudAmistadDTO
                    {
                        UsuarioEmisor = nombreEmisorNormalizado,
                        UsuarioReceptor = nombreReceptorNormalizado,
                        SolicitudAceptada = true
                    };

                    _logger.InfoFormat("Solicitud de amistad aceptada entre '{0}' y '{1}'.", nombreEmisorNormalizado, nombreReceptorNormalizado);
                    _notificador.NotificarSolicitudActualizada(nombreEmisorNormalizado, solicitud);
                    _notificador.NotificarSolicitudActualizada(nombreReceptorNormalizado, solicitud);
                }

                ListaAmigosManejador.NotificarCambioAmistad(nombreEmisorNormalizado);
                ListaAmigosManejador.NotificarCambioAmistad(nombreReceptorNormalizado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Regla de negocio violada al aceptar solicitud de amistad.", ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al aceptar solicitud de amistad.", ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al responder solicitud de amistad. No se pudo actualizar el estado de la solicitud.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorActualizarSolicitud);
            }
            catch (Exception ex)
            {
                _logger.Error("Error de datos al responder solicitud de amistad. No se pudo actualizar el estado de la solicitud.", ex);
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

                using (var contexto = _contextoFactory.CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    Usuario usuarioA = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioA);
                    Usuario usuarioB = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioB);

                    if (usuarioA == null || usuarioB == null)
                    {
                        throw new FaultException(MensajesError.Cliente.UsuariosEspecificadosNoExisten);
                    }

                    idUsuarioA = usuarioA.idUsuario;

                    relacionEliminada = _amistadServicio.EliminarAmistad(usuarioA.idUsuario, usuarioB.idUsuario);

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

                _logger.InfoFormat("Amistad eliminada entre '{0}' y '{1}'.", nombreUsuarioANormalizado, nombreUsuarioBNormalizado);
                ListaAmigosManejador.NotificarCambioAmistad(nombreUsuarioANormalizado);
                ListaAmigosManejador.NotificarCambioAmistad(nombreUsuarioBNormalizado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Regla de negocio violada al eliminar amistad.", ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al eliminar la relación de amistad.", ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al eliminar amistad. No se pudo eliminar la relación en la base de datos.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorEliminarAmistad);
            }
            catch (Exception ex)
            {
                _logger.Error("Error de datos al eliminar amistad. No se pudo eliminar la relación en la base de datos.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorEliminarAmistad);
            }
        }
    }
}