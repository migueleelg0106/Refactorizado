using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using Datos.DAL.Implementaciones;
using Datos.Modelo;
using Datos.Utilidades;
using log4net;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;

namespace Servicios.Servicios
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ListaAmigosManejador : IListaAmigosManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ListaAmigosManejador));
        private static readonly ConcurrentDictionary<string, IListaAmigosManejadorCallback> _suscripciones =
            new(StringComparer.OrdinalIgnoreCase);

        public void Suscribir(string nombreUsuario)
        {
            ValidarNombreUsuario(nombreUsuario, nameof(nombreUsuario));

            List<AmigoDTO> amigosActuales;
            try
            {
                amigosActuales = ObtenerAmigosInterno(nombreUsuario);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Warn("Identificador inválido al suscribirse a la lista de amigos", ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al suscribirse a la lista de amigos", ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al suscribirse a la lista de amigos", ex);
                throw new FaultException("No fue posible suscribirse a la lista de amigos.");
            }

            IListaAmigosManejadorCallback callback = ObtenerCallbackActual();
            _suscripciones.AddOrUpdate(nombreUsuario, callback, (_, __) => callback);

            var canal = OperationContext.Current?.Channel;
            if (canal != null)
            {
                canal.Closed += (_, __) => RemoverSuscripcion(nombreUsuario);
                canal.Faulted += (_, __) => RemoverSuscripcion(nombreUsuario);
            }

            NotificarLista(nombreUsuario, amigosActuales);
        }

        public void CancelarSuscripcion(string nombreUsuario)
        {
            ValidarNombreUsuario(nombreUsuario, nameof(nombreUsuario));
            RemoverSuscripcion(nombreUsuario);
        }

        public List<AmigoDTO> ObtenerAmigos(string nombreUsuario)
        {
            ValidarNombreUsuario(nombreUsuario, nameof(nombreUsuario));

            try
            {
                return ObtenerAmigosInterno(nombreUsuario);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Warn("Identificador inválido al obtener la lista de amigos", ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al obtener la lista de amigos", ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al obtener la lista de amigos", ex);
                throw new FaultException("No fue posible recuperar la lista de amigos.");
            }
        }

        internal static void NotificarCambioAmistad(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            NotificarLista(nombreUsuario);
        }

        private static List<AmigoDTO> ObtenerAmigosInterno(string nombreUsuario)
        {
            using var contexto = CrearContexto();
            var usuarioRepositorio = new UsuarioRepositorio(contexto);
            var amigoRepositorio = new AmigoRepositorio(contexto);

            Usuario usuario = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuario);

            if (usuario != null)
            {
                IList<Usuario> amigos = amigoRepositorio.ObtenerAmigos(usuario.idUsuario);

                var resultado = new List<AmigoDTO>(amigos.Count);
                foreach (var amigo in amigos)
                {
                    if (amigo == null)
                    {
                        continue;
                    }

                    resultado.Add(new AmigoDTO
                    {
                        UsuarioId = amigo.idUsuario,
                        NombreUsuario = amigo.Nombre_Usuario
                    });
                }

                return resultado;
            }

            throw new FaultException("El usuario especificado no existe.");
        }

        private static void NotificarLista(string nombreUsuario)
        {
            try
            {
                var amigos = ObtenerAmigosInterno(nombreUsuario);
                NotificarLista(nombreUsuario, amigos);
            }
            catch (FaultException ex)
            {
                _logger.Warn($"No se pudo obtener la lista de amigos del usuario {nombreUsuario} para notificar", ex);
                RemoverSuscripcion(nombreUsuario);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Warn($"Identificador inválido al actualizar la lista de amigos del usuario {nombreUsuario}", ex);
                RemoverSuscripcion(nombreUsuario);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn($"Datos inválidos al actualizar la lista de amigos del usuario {nombreUsuario}", ex);
                RemoverSuscripcion(nombreUsuario);
            }
            catch (DataException ex)
            {
                _logger.Error($"Error de datos al obtener la lista de amigos del usuario {nombreUsuario}", ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn($"Error inesperado al obtener la lista de amigos del usuario {nombreUsuario}", ex);
            }
        }

        private static void NotificarLista(string nombreUsuario, List<AmigoDTO> amigos)
        {
            if (!_suscripciones.TryGetValue(nombreUsuario, out var callback))
            {
                return;
            }

            try
            {
                callback.NotificarListaAmigosActualizada(amigos);
            }
            catch (CommunicationException)
            {
                RemoverSuscripcion(nombreUsuario);
            }
            catch (TimeoutException)
            {
                RemoverSuscripcion(nombreUsuario);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn($"Error inesperado al notificar la lista de amigos del usuario {nombreUsuario}", ex);
            }
        }

        private static IListaAmigosManejadorCallback ObtenerCallbackActual()
        {
            var contexto = OperationContext.Current;
            if (contexto != null)
            {
                var callback = contexto.GetCallbackChannel<IListaAmigosManejadorCallback>();
                if (callback != null)
                {
                    return callback;
                }

                throw new FaultException("No se pudo obtener el canal de retorno para la lista de amigos.");
            }

            throw new FaultException("No se pudo obtener el contexto de la operación para suscribirse a la lista de amigos.");
        }

        private static void RemoverSuscripcion(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            _suscripciones.TryRemove(nombreUsuario, out _);
        }

        private static void ValidarNombreUsuario(string nombreUsuario, string parametro)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new FaultException($"El parámetro {parametro} es obligatorio.");
            }
        }

        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }
    }
}