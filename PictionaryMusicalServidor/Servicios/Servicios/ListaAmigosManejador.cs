using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Datos.Utilidades;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using System.Globalization;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios
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
                amigosActuales = ObtenerAmigosPorNombre(nombreUsuario);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosSuscribirIdentificadorInvalido, ex);
                throw new FaultException(MensajesError.Cliente.DatosInvalidos);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosSuscribirDatosInvalidos, ex);
                throw new FaultException(MensajesError.Cliente.DatosInvalidos);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.ListaAmigosSuscribirErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorSuscripcionAmigos);
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
                return ObtenerAmigosPorNombre(nombreUsuario);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosObtenerIdentificadorInvalido, ex);
                throw new FaultException(MensajesError.Cliente.DatosInvalidos);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosObtenerDatosInvalidos, ex);
                throw new FaultException(MensajesError.Cliente.DatosInvalidos);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.ListaAmigosObtenerErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarListaAmigos);
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


        private static List<AmigoDTO> ObtenerAmigosPorNombre(string nombreUsuario)
        {
            using var contexto = CrearContexto();
            var usuarioRepositorio = new UsuarioRepositorio(contexto);

            Usuario usuario = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuario);

            if (usuario == null)
            {
                throw new FaultException(MensajesError.Cliente.UsuarioNoEncontrado);
            }

            return ServicioAmistad.ObtenerAmigosDTO(usuario.idUsuario);
        }


        private static void NotificarLista(string nombreUsuario)
        {
            try
            {
                var amigos = ObtenerAmigosPorNombre(nombreUsuario);
                NotificarLista(nombreUsuario, amigos);
            }
            catch (FaultException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosNotificarObtenerError, ex);
                RemoverSuscripcion(nombreUsuario);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosActualizarIdentificadorInvalido, ex);
                RemoverSuscripcion(nombreUsuario);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.ListaAmigosActualizarDatosInvalidos, ex);
                RemoverSuscripcion(nombreUsuario);
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
                _logger.Warn(MensajesError.Log.ListaAmigosNotificarError, ex);
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

                throw new FaultException(MensajesError.Cliente.ErrorObtenerCallback);
            }

            throw new FaultException(MensajesError.Cliente.ErrorContextoOperacion);
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
                string mensaje = string.Format(CultureInfo.CurrentCulture, MensajesError.Cliente.ParametroObligatorio, parametro);
                throw new FaultException(mensaje);
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