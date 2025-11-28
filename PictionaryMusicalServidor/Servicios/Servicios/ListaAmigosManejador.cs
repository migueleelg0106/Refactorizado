using System;
using System.Collections.Generic;
using System.Data;
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
    /// Maneja suscripciones para notificaciones de cambios en listas de amigos con notificaciones en tiempo real via callbacks.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ListaAmigosManejador : IListaAmigosManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ListaAmigosManejador));
        private static readonly ManejadorCallback<IListaAmigosManejadorCallback> _manejadorCallback = new(StringComparer.OrdinalIgnoreCase);
        private static readonly IContextoFactory _contextoFactoryInstancia = new ContextoFactory();
        private static readonly IAmistadServicio _amistadServicioInstancia = new AmistadServicio(_contextoFactoryInstancia);
        private static readonly NotificadorListaAmigos _notificador = new(_manejadorCallback, _contextoFactoryInstancia);

        /// <summary>
        /// Suscribe un usuario para recibir notificaciones sobre cambios en su lista de amigos.
        /// Obtiene la lista actual de amigos, registra el callback y notifica inmediatamente.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario a suscribir.</param>
        /// <exception cref="FaultException">Se lanza si el nombre de usuario es invalido, no existe, o hay errores de base de datos.</exception>
        public void Suscribir(string nombreUsuario)
        {
            List<AmigoDTO> amigosActuales;
            try
            {
                ValidadorNombreUsuario.Validar(nombreUsuario, nameof(nombreUsuario));

                amigosActuales = ObtenerAmigosPorNombre(nombreUsuario);

                IListaAmigosManejadorCallback callback = ManejadorCallback<IListaAmigosManejadorCallback>.ObtenerCallbackActual();
                _manejadorCallback.Suscribir(nombreUsuario, callback);
                _manejadorCallback.ConfigurarEventosCanal(nombreUsuario);

                _notificador.NotificarLista(nombreUsuario, amigosActuales);

                _logger.InfoFormat("Usuario '{0}' se suscribió a notificaciones de lista de amigos.", nombreUsuario);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Warn("Identificador inválido al suscribirse a la lista de amigos.", ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al suscribirse a la lista de amigos.", ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al suscribirse a lista de amigos. No se pudo recuperar la lista de amigos del usuario.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorSuscripcionAmigos);
            }
            catch (Exception ex)
            {
                _logger.Error("Error de datos al suscribirse a lista de amigos. No se pudo recuperar la lista de amigos del usuario.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorSuscripcionAmigos);
            }
        }

        /// <summary>
        /// Cancela la suscripcion de un usuario de notificaciones de lista de amigos.
        /// Elimina el callback del usuario del manejador de callbacks.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario que cancela la suscripcion.</param>
        /// <exception cref="FaultException">Se lanza si el nombre de usuario es invalido o hay errores.</exception>
        public void CancelarSuscripcion(string nombreUsuario)
        {
            try
            {
                ValidadorNombreUsuario.Validar(nombreUsuario, nameof(nombreUsuario));
                _manejadorCallback.Desuscribir(nombreUsuario);

                _logger.InfoFormat("Usuario '{0}' canceló su suscripción a lista de amigos.", nombreUsuario);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al obtener la lista de amigos.", ex);
                throw new FaultException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al obtener la lista de amigos del usuario.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorSuscripcionAmigos);
            }
        }

        /// <summary>
        /// Obtiene la lista de amigos de un usuario especifico.
        /// Recupera todos los amigos del usuario desde la base de datos.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario cuya lista de amigos se desea obtener.</param>
        /// <returns>Lista de amigos del usuario.</returns>
        /// <exception cref="FaultException">Se lanza si el nombre de usuario es invalido, no existe, o hay errores de base de datos.</exception>
        public List<AmigoDTO> ObtenerAmigos(string nombreUsuario)
        {
            try
            {
                ValidadorNombreUsuario.Validar(nombreUsuario, nameof(nombreUsuario));
                return ObtenerAmigosPorNombre(nombreUsuario);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Warn("Identificador inválido al obtener la lista de amigos.", ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al obtener la lista de amigos.", ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al obtener lista de amigos. Fallo en la consulta de amigos del usuario.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarListaAmigos);
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al obtener la lista de amigos del usuario.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarListaAmigos);
            }
        }

        internal static void NotificarCambioAmistad(string nombreUsuario)
        {
            _notificador.NotificarCambioAmistad(nombreUsuario);
        }

        private static List<AmigoDTO> ObtenerAmigosPorNombre(string nombreUsuario)
        {
            using var contexto = _contextoFactoryInstancia.CrearContexto();
            var usuarioRepositorio = new UsuarioRepositorio(contexto);

            Usuario usuario = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuario);

            if (usuario == null)
            {
                throw new FaultException(MensajesError.Cliente.UsuarioNoEncontrado);
            }

            return _amistadServicioInstancia.ObtenerAmigosDTO(usuario.idUsuario);
        }
    }
}