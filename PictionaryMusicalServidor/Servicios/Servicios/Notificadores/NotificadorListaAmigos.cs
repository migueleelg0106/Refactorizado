using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
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
    internal class NotificadorListaAmigos : INotificadorListaAmigos
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NotificadorListaAmigos));
        private readonly ManejadorCallback<IListaAmigosManejadorCallback> _manejadorCallback;
        private readonly IAmistadServicio _amistadServicio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public NotificadorListaAmigos(
            ManejadorCallback<IListaAmigosManejadorCallback> manejadorCallback, 
            IAmistadServicio amistadServicio,
            IUsuarioRepositorio usuarioRepositorio)
        {
            _manejadorCallback = manejadorCallback;
            _amistadServicio = amistadServicio;
            _usuarioRepositorio = usuarioRepositorio;
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
                NotificarLista(nombreUsuario, amigos);
            }
            catch (FaultException excepcion)
            {
                _logger.Warn("No se pudo obtener la lista de amigos del usuario para notificar.", 
                    excepcion);
            }
            catch (ArgumentOutOfRangeException excepcion)
            {
                _logger.Warn(
                    "Identificador invalido al actualizar la lista de amigos del usuario.", excepcion);
            }
            catch (ArgumentException excepcion)
            {
                _logger.Warn("Datos invalidos al actualizar la lista de amigos del usuario.", 
                    excepcion);
            }
            catch (DataException excepcion)
            {
                _logger.Error(
                    "Error de datos al obtener lista de amigos. Fallo en la consulta de amigos del usuario.", 
                    excepcion);
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Warn("Error inesperado al obtener la lista de amigos del usuario.", excepcion);
            }
            catch (Exception excepcion)
            {
                _logger.Warn("Error inesperado al obtener la lista de amigos del usuario.", excepcion);
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

        private List<AmigoDTO> ObtenerAmigosPorNombre(string nombreUsuario)
        {
            var usuario = _usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuario);
            if (usuario == null)
            {
                throw new FaultException(MensajesError.Cliente.UsuarioNoEncontrado);
            }

            return _amistadServicio.ObtenerAmigosDTO(usuario.idUsuario);
        }
    }
}
