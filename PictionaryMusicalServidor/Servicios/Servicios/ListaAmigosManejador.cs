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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ListaAmigosManejador : IListaAmigosManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ListaAmigosManejador));
        private static readonly ManejadorCallback<IListaAmigosManejadorCallback> _manejadorCallback = new(StringComparer.OrdinalIgnoreCase);
        private static readonly NotificadorListaAmigos _notificador = new(_manejadorCallback);

        public void Suscribir(string nombreUsuario)
        {
            ValidadorNombreUsuario.Validar(nombreUsuario, nameof(nombreUsuario));

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

            IListaAmigosManejadorCallback callback = ManejadorCallback<IListaAmigosManejadorCallback>.ObtenerCallbackActual();
            _manejadorCallback.Suscribir(nombreUsuario, callback);
            _manejadorCallback.ConfigurarEventosCanal(nombreUsuario);

            _notificador.NotificarLista(nombreUsuario, amigosActuales);
        }

        public void CancelarSuscripcion(string nombreUsuario)
        {
            ValidadorNombreUsuario.Validar(nombreUsuario, nameof(nombreUsuario));
            _manejadorCallback.Desuscribir(nombreUsuario);
        }

        public List<AmigoDTO> ObtenerAmigos(string nombreUsuario)
        {
            ValidadorNombreUsuario.Validar(nombreUsuario, nameof(nombreUsuario));

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