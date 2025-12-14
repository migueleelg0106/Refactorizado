using Datos.Modelo;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Notificadores;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.ServiceModel;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de gestion de amistades entre usuarios.
    /// Maneja suscripciones para notificaciones, envio y respuesta de solicitudes de amistad,
    /// y eliminacion de relaciones de amistad con notificaciones en tiempo real via callbacks.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode =
        ConcurrencyMode.Multiple)]
    public class AmigosManejador : IAmigosManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AmigosManejador));

        private readonly ManejadorCallback<IAmigosManejadorCallback> _manejadorCallback;
        private readonly INotificadorAmigos _notificador;
        private readonly INotificadorListaAmigos _notificadorListaAmigos;
        private readonly IContextoFactoria _contextoFactory;
        private readonly IAmistadServicio _amistadServicio;
        private readonly IValidadorNombreUsuario _validadorUsuario;

        public AmigosManejador() : this(
            new ContextoFactoria(),
            new AmistadServicio(new ContextoFactoria()),
            new NotificadorListaAmigos(
                new ManejadorCallback<IListaAmigosManejadorCallback>(
                    StringComparer.OrdinalIgnoreCase),
                new AmistadServicio(new ContextoFactoria()),
                new UsuarioRepositorio(new ContextoFactoria().CrearContexto())),
            new ValidadorNombreUsuario())
        {
        }

        /// <summary>
        /// Constructor por defecto para compatibilidad con WCF.
        /// </summary>
        public AmigosManejador(
            IContextoFactoria contextoFactory,
            IAmistadServicio amistadServicio,
            INotificadorListaAmigos notificadorLista,
            IValidadorNombreUsuario validadorUsuario)
        {
            _contextoFactory = contextoFactory ??
                throw new ArgumentNullException(nameof(contextoFactory));

            _amistadServicio = amistadServicio ??
                throw new ArgumentNullException(nameof(amistadServicio));

            _validadorUsuario = validadorUsuario ??
                throw new ArgumentNullException(nameof(validadorUsuario));

            _notificadorListaAmigos = notificadorLista ??
                throw new ArgumentNullException(nameof(notificadorLista));

            _manejadorCallback = new ManejadorCallback<IAmigosManejadorCallback>(
                StringComparer.OrdinalIgnoreCase);

            _notificador = new NotificadorAmigos(_manejadorCallback, _amistadServicio);
        }

        /// <summary>
        /// Suscribe un usuario para recibir notificaciones de solicitudes de amistad.
        /// Normaliza el nombre de usuario, registra el callback y notifica solicitudes pendientes.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario a suscribir.</param>
        public void Suscribir(string nombreUsuario)
        {
            ValidarEntradaSuscripcion(nombreUsuario);

            try
            {
                var datosUsuario = ObtenerDatosUsuarioSuscripcion(nombreUsuario);

                RegistrarCallback(nombreUsuario, datosUsuario.NombreNormalizado);

                _notificador.NotificarSolicitudesPendientesAlSuscribir(
                    datosUsuario.NombreNormalizado,
                    datosUsuario.IdUsuario);
            }
            catch (EntityException excepcion)
            {
                _logger.Error(
                    "Error de base de datos al suscribir a notificaciones de amistad.",
                    excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarSolicitudes);
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error de datos al suscribir a notificaciones de amistad.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarSolicitudes);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error de datos al suscribir a notificaciones de amistad.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarSolicitudes);
            }
        }

        /// <summary>
        /// Cancela la suscripcion de un usuario de notificaciones de amistad.
        /// Elimina el callback del usuario del manejador de callbacks.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario que cancela la suscripcion.</param>
        public void CancelarSuscripcion(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new FaultException(
                    MensajesError.Cliente.NombreUsuarioObligatorioCancelar);
            }

            _manejadorCallback.Desuscribir(nombreUsuario);
        }

        /// <summary>
        /// Envia una solicitud de amistad de un usuario a otro.
        /// Valida que ambos usuarios existan, crea la solicitud y notifica al receptor.
        /// </summary>
        /// <param name="nombreUsuarioEmisor">Nombre del usuario que envia la solicitud.</param>
        /// <param name="nombreUsuarioReceptor">Nombre del usuario que recibe la solicitud.</param>
        public void EnviarSolicitudAmistad(
            string nombreUsuarioEmisor,
            string nombreUsuarioReceptor)
        {
            try
            {
                ValidarEntradasInteraccion(nombreUsuarioEmisor, nombreUsuarioReceptor);

                var usuarios = EjecutarCreacionSolicitudEnBaseDatos(
                    nombreUsuarioEmisor,
                    nombreUsuarioReceptor);

                NotificarSolicitudNueva(
                    usuarios.Emisor,
                    nombreUsuarioEmisor,
                    usuarios.Receptor,
                    nombreUsuarioReceptor);
            }
            catch (KeyNotFoundException excepcion)
            {
                _logger.Warn("Intento de enviar solicitud a usuario inexistente.", excepcion);
                throw new FaultException(MensajesError.Cliente.UsuariosEspecificadosNoExisten);
            }
            catch (FaultException excepcion)
            {
                _logger.Warn("Error de validacion al enviar solicitud de amistad.", excepcion);
                throw;
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Warn("Regla de negocio violada al enviar solicitud de amistad.", excepcion);
                throw new FaultException(excepcion.Message);
            }
            catch (ArgumentException excepcion)
            {
                _logger.Warn("Datos invalidos al enviar solicitud de amistad.", excepcion);
                throw new FaultException(excepcion.Message);
            }
            catch (DbUpdateException excepcion)
            {
                _logger.Error("Error inesperado al enviar solicitud de amistad.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorAlmacenarSolicitud);
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error inesperado al enviar solicitud de amistad.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorAlmacenarSolicitud);
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error inesperado al enviar solicitud de amistad.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorAlmacenarSolicitud);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error inesperado al enviar solicitud de amistad.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorAlmacenarSolicitud);
            }
        }

        /// <summary>
        /// Responde una solicitud de amistad aceptandola.
        /// Actualiza la solicitud, notifica a ambos usuarios y actualiza sus listas de amigos.
        /// </summary>
        /// <param name="nombreUsuarioEmisor">Usuario que envio la solicitud original.</param>
        /// <param name="nombreUsuarioReceptor">Usuario que responde la solicitud.</param>
        public void ResponderSolicitudAmistad(
            string nombreUsuarioEmisor,
            string nombreUsuarioReceptor)
        {
            try
            {
                ValidarEntradasInteraccion(nombreUsuarioEmisor, nombreUsuarioReceptor);

                var nombresNormalizados = EjecutarAceptacionSolicitudEnBaseDatos(
                    nombreUsuarioEmisor,
                    nombreUsuarioReceptor);

                EjecutarNotificacionesRespuesta(nombresNormalizados);
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Warn("Regla de negocio violada al aceptar solicitud de amistad.", excepcion);
                throw new FaultException(excepcion.Message);
            }
            catch (ArgumentException excepcion)
            {
                _logger.Warn("Datos invalidos al aceptar solicitud de amistad.", excepcion);
                throw new FaultException(excepcion.Message);
            }
            catch (DbUpdateException excepcion)
            {
                _logger.Error("Error al responder solicitud de amistad.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorActualizarSolicitud);
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error al responder solicitud de amistad.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorActualizarSolicitud);
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error al responder solicitud de amistad.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorActualizarSolicitud);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error al responder solicitud de amistad.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorActualizarSolicitud);
            }
        }

        /// <summary>
        /// Elimina la relacion de amistad entre dos usuarios.
        /// Elimina la amistad, notifica a ambos usuarios y actualiza sus listas de amigos.
        /// </summary>
        /// <param name="nombreUsuarioA">Nombre del primer usuario.</param>
        /// <param name="nombreUsuarioB">Nombre del segundo usuario.</param>
        public void EliminarAmigo(string nombreUsuarioA, string nombreUsuarioB)
        {
            try
            {
                ValidarEntradasInteraccion(nombreUsuarioA, nombreUsuarioB);

                var resultadoEliminacion = EjecutarEliminacionEnBaseDatos(
                    nombreUsuarioA,
                    nombreUsuarioB);

                EjecutarNotificacionesEliminacion(resultadoEliminacion);
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Warn("Regla de negocio violada al eliminar amistad.", excepcion);
                throw new FaultException(excepcion.Message);
            }
            catch (DbUpdateException excepcion)
            {
                _logger.Error("Error inesperado al eliminar amistad.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorEliminarAmistad);
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error inesperado al eliminar amistad.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorEliminarAmistad);
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error inesperado al eliminar amistad.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorEliminarAmistad);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error inesperado al eliminar amistad.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorEliminarAmistad);
            }
        }

        private void ValidarEntradaSuscripcion(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new FaultException(
                    MensajesError.Cliente.NombreUsuarioObligatorioSuscripcion);
            }
        }

        private (int IdUsuario, string NombreNormalizado) ObtenerDatosUsuarioSuscripcion(
            string nombreUsuario)
        {
            using (var contexto = _contextoFactory.CrearContexto())
            {
                var usuarioRepositorio = new UsuarioRepositorio(contexto);
                var usuario = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuario);

                if (usuario == null)
                {
                    throw new FaultException(MensajesError.Cliente.UsuarioNoEncontrado);
                }

                string nombreNormalizado = _validadorUsuario.ObtenerNombreNormalizado(
                    usuario.Nombre_Usuario,
                    nombreUsuario);

                if (string.IsNullOrWhiteSpace(nombreNormalizado))
                {
                    throw new FaultException(MensajesError.Cliente.UsuarioNoEncontrado);
                }

                return (usuario.idUsuario, nombreNormalizado);
            }
        }

        private void RegistrarCallback(string nombreUsuario, string nombreNormalizado)
        {
            var callback = ManejadorCallback<IAmigosManejadorCallback>.ObtenerCallbackActual();
            _manejadorCallback.Suscribir(nombreNormalizado, callback);

            if (!string.Equals(
                nombreUsuario,
                nombreNormalizado,
                StringComparison.Ordinal))
            {
                _manejadorCallback.Desuscribir(nombreUsuario);
            }

            _manejadorCallback.ConfigurarEventosCanal(nombreNormalizado);
        }

        private void ValidarEntradasInteraccion(string usuarioA, string usuarioB)
        {
            _validadorUsuario.Validar(usuarioA, nameof(usuarioA));
            _validadorUsuario.Validar(usuarioB, nameof(usuarioB));
        }

        private (Usuario Emisor, Usuario Receptor) EjecutarCreacionSolicitudEnBaseDatos(
            string nombreEmisor,
            string nombreReceptor)
        {
            using (var contexto = _contextoFactory.CrearContexto())
            {
                var (emisor, receptor) = ObtenerUsuariosParaInteraccion(
                    contexto,
                    nombreEmisor,
                    nombreReceptor);

                _amistadServicio.CrearSolicitud(emisor.idUsuario, receptor.idUsuario);

                return (emisor, receptor);
            }
        }

        private (string NormalizadoEmisor, string NormalizadoReceptor)
            EjecutarAceptacionSolicitudEnBaseDatos(
                string nombreEmisor,
                string nombreReceptor)
        {
            using (var contexto = _contextoFactory.CrearContexto())
            {
                var (usuarioEmisor, usuarioReceptor) = ObtenerUsuariosParaInteraccion(
                    contexto,
                    nombreEmisor,
                    nombreReceptor);

                _amistadServicio.AceptarSolicitud(
                    usuarioEmisor.idUsuario,
                    usuarioReceptor.idUsuario);

                string normEmisor = _validadorUsuario.ObtenerNombreNormalizado(
                    usuarioEmisor.Nombre_Usuario,
                    nombreEmisor);

                string normReceptor = _validadorUsuario.ObtenerNombreNormalizado(
                    usuarioReceptor.Nombre_Usuario,
                    nombreReceptor);

                return (normEmisor, normReceptor);
            }
        }

        private ResultadoEliminacionAmistad EjecutarEliminacionEnBaseDatos(
            string nombreA,
            string nombreB)
        {
            using (var contexto = _contextoFactory.CrearContexto())
            {
                var (usuarioA, usuarioB) = ObtenerUsuariosParaInteraccion(
                    contexto,
                    nombreA,
                    nombreB);

                var relacion = _amistadServicio.EliminarAmistad(
                    usuarioA.idUsuario,
                    usuarioB.idUsuario);

                string normA = _validadorUsuario.ObtenerNombreNormalizado(
                    usuarioA.Nombre_Usuario,
                    nombreA);

                string normB = _validadorUsuario.ObtenerNombreNormalizado(
                    usuarioB.Nombre_Usuario,
                    nombreB);

                return new ResultadoEliminacionAmistad
                {
                    Relacion = relacion,
                    NombreANormalizado = normA,
                    NombreBNormalizado = normB
                };
            }
        }

        private (Usuario Emisor, Usuario Receptor) ObtenerUsuariosParaInteraccion(
            BaseDatosPruebaEntities contexto,
            string nombreEmisor,
            string nombreReceptor)
        {
            var usuarioRepositorio = new UsuarioRepositorio(contexto);
            var usuarioEmisor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreEmisor);
            var usuarioReceptor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreReceptor);

            ValidarUsuariosExistentes(usuarioEmisor, usuarioReceptor);

            return (usuarioEmisor, usuarioReceptor);
        }

        private void ValidarUsuariosExistentes(Usuario emisor, Usuario receptor)
        {
            if (emisor == null)
            {
                throw new FaultException(
                    MensajesError.Cliente.UsuariosEspecificadosNoExisten);
            }
            if (receptor == null)
            {
                throw new FaultException(
                    MensajesError.Cliente.UsuariosEspecificadosNoExisten);
            }
        }

        private void NotificarSolicitudNueva(
            Usuario emisor,
            string nombreEmisorInput,
            Usuario receptor,
            string nombreReceptorInput)
        {
            string nombreEmisor = _validadorUsuario.ObtenerNombreNormalizado(
                emisor.Nombre_Usuario,
                nombreEmisorInput);

            string nombreReceptor = _validadorUsuario.ObtenerNombreNormalizado(
                receptor.Nombre_Usuario,
                nombreReceptorInput);

            var solicitud = new SolicitudAmistadDTO
            {
                UsuarioEmisor = nombreEmisor,
                UsuarioReceptor = nombreReceptor,
                SolicitudAceptada = false
            };

            _notificador.NotificarSolicitudActualizada(nombreReceptor, solicitud);
        }

        private void EjecutarNotificacionesRespuesta(
            (string Emisor, string Receptor) nombres)
        {
            NotificarSolicitudAceptada(nombres.Emisor, nombres.Receptor);
            _notificadorListaAmigos.NotificarCambioAmistad(nombres.Emisor);
            _notificadorListaAmigos.NotificarCambioAmistad(nombres.Receptor);
        }

        private void NotificarSolicitudAceptada(string emisor, string receptor)
        {
            var solicitud = new SolicitudAmistadDTO
            {
                UsuarioEmisor = emisor,
                UsuarioReceptor = receptor,
                SolicitudAceptada = true
            };

            _notificador.NotificarSolicitudActualizada(emisor, solicitud);
            _notificador.NotificarSolicitudActualizada(receptor, solicitud);
        }

        private void EjecutarNotificacionesEliminacion(ResultadoEliminacionAmistad resultado)
        {
            NotificarEliminacion(
                resultado.Relacion,
                resultado.NombreANormalizado,
                resultado.NombreBNormalizado);

            _notificadorListaAmigos.NotificarCambioAmistad(resultado.NombreANormalizado);
            _notificadorListaAmigos.NotificarCambioAmistad(resultado.NombreBNormalizado);
        }

        private void NotificarEliminacion(Amigo relacion, string usuarioA, string usuarioB)
        {
            if (relacion == null)
            {
                _logger.Warn("Se solicito notificar una eliminacion de amistad sin relacion.");
                return;
            }

            var solicitud = new SolicitudAmistadDTO
            {
                UsuarioEmisor = usuarioA,
                UsuarioReceptor = usuarioB,
                SolicitudAceptada = false
            };

            _notificador.NotificarAmistadEliminada(usuarioA, solicitud);
            _notificador.NotificarAmistadEliminada(usuarioB, solicitud);
        }

        /// <summary>
        /// Clase auxiliar interna para transportar datos de eliminacion.
        /// </summary>
        private class ResultadoEliminacionAmistad
        {
            public Amigo Relacion { get; set; }
            public string NombreANormalizado { get; set; }
            public string NombreBNormalizado { get; set; }
        }
    }
}
