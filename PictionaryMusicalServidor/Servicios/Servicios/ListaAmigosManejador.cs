using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.ServiceModel;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
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
    /// Implementacion del servicio de gestion de listas de amigos.
    /// Maneja suscripciones para notificaciones de cambios en listas de amigos con notificaciones
    /// en tiempo real via callbacks.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ListaAmigosManejador : IListaAmigosManejador
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(ListaAmigosManejador));

        private readonly ManejadorCallback<IListaAmigosManejadorCallback> _manejadorCallback;
        private readonly IContextoFactoria _contextoFactory;
        private readonly IAmistadServicio _amistadServicio;
        private readonly INotificadorListaAmigos _notificador;
        private readonly IValidadorNombreUsuario _validadorUsuario;

        /// <summary>
        /// Constructor vacio utilizado por el Host de WCF.
        /// Inicializa las dependencias manualmente.
        /// </summary>
        public ListaAmigosManejador() : this(
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
        /// Constructor principal.
        /// </summary>
        public ListaAmigosManejador(
            IContextoFactoria contextoFactory,
            IAmistadServicio amistadServicio,
            INotificadorListaAmigos notificador,
            IValidadorNombreUsuario validadorUsuario)
        {
            _contextoFactory = contextoFactory ??
                throw new ArgumentNullException(nameof(contextoFactory));
            _amistadServicio = amistadServicio ??
                throw new ArgumentNullException(nameof(amistadServicio));
            _validadorUsuario = validadorUsuario ??
                throw new ArgumentNullException(nameof(validadorUsuario));
            _notificador = notificador ??
                throw new ArgumentNullException(nameof(notificador));

            _manejadorCallback = new ManejadorCallback<IListaAmigosManejadorCallback>(
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Suscribe un usuario para recibir notificaciones sobre cambios en su lista de amigos.
        /// Obtiene la lista actual de amigos, registra el callback y notifica inmediatamente.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario a suscribir.</param>
        public void Suscribir(string nombreUsuario)
        {
            try
            {
                _validadorUsuario.Validar(nombreUsuario, nameof(nombreUsuario));

                var amigosActuales = ObtenerAmigosPorNombre(nombreUsuario);

                IListaAmigosManejadorCallback callback =
                    ManejadorCallback<IListaAmigosManejadorCallback>.ObtenerCallbackActual();

                _manejadorCallback.Suscribir(nombreUsuario, callback);
                _manejadorCallback.ConfigurarEventosCanal(nombreUsuario);

                _notificador.NotificarLista(nombreUsuario, amigosActuales);
            }
            catch (ArgumentOutOfRangeException excepcion)
            {
                _logger.Warn("Identificador invalido al suscribirse a la lista de amigos.", excepcion);
                throw new FaultException(excepcion.Message);
            }
            catch (ArgumentException excepcion)
            {
                _logger.Warn("Datos invalidos al suscribirse a la lista de amigos.", excepcion);
                throw new FaultException(excepcion.Message);
            }
            catch (DbUpdateException excepcion)
            {
                _logger.Error(
                    "Error de datos al suscribirse. Fallo recuperar lista de amigos.",
                    excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorSuscripcionAmigos);
            }
            catch (EntityException excepcion)
            {
                _logger.Error(
                    "Error de datos al suscribirse. Fallo recuperar lista de amigos.",
                    excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorSuscripcionAmigos);
            }
            catch (DataException excepcion)
            {
                _logger.Error(
                    "Error de datos al suscribirse. Fallo recuperar lista de amigos.",
                    excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorSuscripcionAmigos);
            }
            catch (Exception excepcion)
            {
                _logger.Error(
                    "Error de datos al suscribirse. Fallo recuperar lista de amigos.",
                    excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorSuscripcionAmigos);
            }
        }

        /// <summary>
        /// Cancela la suscripcion de un usuario de notificaciones de lista de amigos.
        /// Elimina el callback del usuario del manejador de callbacks.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario que cancela la suscripcion.</param>
        public void CancelarSuscripcion(string nombreUsuario)
        {
            try
            {
                _validadorUsuario.Validar(nombreUsuario, nameof(nombreUsuario));
                _manejadorCallback.Desuscribir(nombreUsuario);
            }
            catch (ArgumentException excepcion)
            {
                _logger.Warn("Datos invalidos al cancelar suscripcion.", excepcion);
                throw new FaultException(excepcion.Message);
            }
            catch (Exception excepcion)
            {
                _logger.Warn("Datos invalidos al cancelar suscripcion.", excepcion);
                throw new FaultException(excepcion.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de amigos de un usuario especifico.
        /// Recupera todos los amigos del usuario desde la base de datos.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario cuya lista se desea obtener.</param>
        /// <returns>Lista de amigos del usuario.</returns>
        public List<AmigoDTO> ObtenerAmigos(string nombreUsuario)
        {
            try
            {
                _validadorUsuario.Validar(nombreUsuario, nameof(nombreUsuario));
                return ObtenerAmigosPorNombre(nombreUsuario);
            }
            catch (ArgumentException excepcion)
            {
                _logger.Warn("Datos invalidos al obtener la lista de amigos.", excepcion);
                throw new FaultException(excepcion.Message);
            }
            catch (DbUpdateException excepcion)
            {
                _logger.Error("Error inesperado al obtener la lista de amigos.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarListaAmigos);
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error inesperado al obtener la lista de amigos.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarListaAmigos);
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error inesperado al obtener la lista de amigos.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarListaAmigos);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error inesperado al obtener la lista de amigos.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarListaAmigos);
            }
        }

        private List<AmigoDTO> ObtenerAmigosPorNombre(string nombreUsuario)
        {
            using (var contexto = _contextoFactory.CrearContexto())
            {
                var usuarioRepositorio = new UsuarioRepositorio(contexto);
                Usuario usuario = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuario);

                if (usuario == null)
                {
                    throw new FaultException(MensajesError.Cliente.UsuarioNoEncontrado);
                }

                return _amistadServicio.ObtenerAmigosDTO(usuario.idUsuario);
            }
        }
    }
}
