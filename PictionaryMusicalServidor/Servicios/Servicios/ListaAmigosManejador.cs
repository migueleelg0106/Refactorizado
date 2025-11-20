using System;
using System.Collections.Generic;
using System.Data;
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
    /// Implementacion del servicio de gestion de lista de amigos.
    /// Valida nombres de usuario, gestiona suscripciones y notifica actualizaciones de la lista de amigos.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ListaAmigosManejador : IListaAmigosManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ListaAmigosManejador));
        private static readonly ManejadorCallback<IListaAmigosManejadorCallback> _manejadorCallback = new(StringComparer.OrdinalIgnoreCase);
        private static readonly NotificadorListaAmigos _notificador = new(_manejadorCallback);

        /// <summary>
        /// Suscribe a un usuario para recibir actualizaciones de su lista de amigos.
        /// Valida el nombre de usuario y notifica la lista actual.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario a suscribir.</param>
        /// <exception cref="FaultException">Se lanza cuando hay errores de validacion o de datos.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Se captura cuando el identificador es invalido.</exception>
        /// <exception cref="ArgumentException">Se captura cuando hay datos invalidos.</exception>
        /// <exception cref="DataException">Se captura cuando hay errores relacionados con los datos.</exception>
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
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosSuscribirIdentificadorInvalido, ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosSuscribirDatosInvalidos, ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.ListaAmigosSuscribirErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorSuscripcionAmigos);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.ListaAmigosSuscribirErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorSuscripcionAmigos);
            }
        }

        /// <summary>
        /// Cancela la suscripcion de un usuario para dejar de recibir actualizaciones.
        /// Valida el nombre de usuario antes de cancelar la suscripcion.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario que cancela su suscripcion.</param>
        /// <exception cref="FaultException">Se lanza cuando hay errores de validacion.</exception>
        /// <exception cref="ArgumentException">Se captura cuando hay datos invalidos.</exception>
        public void CancelarSuscripcion(string nombreUsuario)
        {
            try
            {
                ValidadorNombreUsuario.Validar(nombreUsuario, nameof(nombreUsuario));
                _manejadorCallback.Desuscribir(nombreUsuario);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosObtenerDatosInvalidos, ex);
                throw new FaultException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.ListaAmigosObtenerInesperado, ex);
                throw new FaultException(MensajesError.Cliente.ErrorSuscripcionAmigos);
            }
        }

        /// <summary>
        /// Obtiene la lista de amigos de un usuario.
        /// Valida el nombre de usuario y recupera las amistades aceptadas.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario.</param>
        /// <returns>Lista de amigos del usuario.</returns>
        /// <exception cref="FaultException">Se lanza cuando hay errores de validacion o de datos.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Se captura cuando el identificador es invalido.</exception>
        /// <exception cref="ArgumentException">Se captura cuando hay datos invalidos.</exception>
        /// <exception cref="DataException">Se captura cuando hay errores relacionados con los datos.</exception>
        public List<AmigoDTO> ObtenerAmigos(string nombreUsuario)
        {
            try
            {
                ValidadorNombreUsuario.Validar(nombreUsuario, nameof(nombreUsuario));

                return ObtenerAmigosPorNombre(nombreUsuario);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosObtenerIdentificadorInvalido, ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosObtenerDatosInvalidos, ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.ListaAmigosObtenerErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarListaAmigos);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.ListaAmigosObtenerInesperado, ex);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarListaAmigos);
            }
        }

        internal static void NotificarCambioAmistad(string nombreUsuario)
        {
            _notificador.NotificarCambioAmistad(nombreUsuario);
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