using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;

namespace PictionaryMusicalServidor.Servicios.Servicios.Notificadores
{
    /// <summary>
    /// Servicio especializado en notificaciones de cambios en lista de amigos.
    /// Gestiona las notificaciones cuando la lista de amigos de un usuario cambia.
    /// </summary>
    internal class NotificadorListaAmigos
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NotificadorListaAmigos));
        private readonly ManejadorCallback<IListaAmigosManejadorCallback> _manejadorCallback;

        public NotificadorListaAmigos(ManejadorCallback<IListaAmigosManejadorCallback> manejadorCallback)
        {
            _manejadorCallback = manejadorCallback;
        }

        /// <summary>
        /// Notifica a un usuario sobre cambios en su lista de amigos.
        /// </summary>
        /// <param name="nombreUsuario">Usuario a notificar.</param>
        public void NotificarCambioAmistad(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            try
            {
                List<AmigoDTO> amigos = ObtenerAmigosPorNombre(nombreUsuario);

                _logger.Info($"Enviando notificación de actualización de lista de amigos a '{nombreUsuario}'. Total amigos: {amigos.Count}");
                NotificarLista(nombreUsuario, amigos);
            }
            catch (FaultException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosNotificarObtenerError, ex);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosActualizarIdentificadorInvalido, ex);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosActualizarDatosInvalidos, ex);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.ListaAmigosObtenerErrorDatos, ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosObtenerInesperado, ex);
            }
        }

        /// <summary>
        /// Notifica la lista actualizada de amigos a un usuario.
        /// </summary>
        /// <param name="nombreUsuario">Usuario a notificar.</param>
        /// <param name="amigos">Lista actualizada de amigos.</param>
        public void NotificarLista(string nombreUsuario, List<AmigoDTO> amigos)
        {
            _manejadorCallback.Notificar(nombreUsuario, callback =>
            {
                callback.NotificarListaAmigosActualizada(amigos);
            });
        }

        private static List<AmigoDTO> ObtenerAmigosPorNombre(string nombreUsuario)
        {
            using var contexto = ContextoFactory.CrearContexto();
            var usuarioRepositorio = new UsuarioRepositorio(contexto);

            Usuario usuario = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuario);

            if (usuario == null)
            {
                throw new FaultException(MensajesError.Cliente.UsuarioNoEncontrado);
            }

            return ServicioAmistad.ObtenerAmigosDTO(usuario.idUsuario);
        }
    }
}