using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Entity.Core;
using System.ServiceModel;
using Datos.DAL.Implementaciones;
using Datos.Utilidades;
using Datos.Modelo;
using log4net;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using System.Collections.Generic;
using Servicios.Servicios.Constantes;

namespace Servicios.Servicios
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AmigosManejador : IAmigosManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AmigosManejador));
        private static readonly ConcurrentDictionary<string, IAmigosManejadorCallback> _suscripciones = new(StringComparer.OrdinalIgnoreCase);

        public void Suscribir(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new FaultException("El nombre de usuario es obligatorio para suscribirse a las notificaciones.");
            }

            Usuario usuario;
            string nombreNormalizado;
            IAmigosManejadorCallback callback;

            try
            {
                using (var contexto = CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    usuario = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuario);

                    if (usuario == null)
                    {
                        throw new FaultException("No se encontró el usuario especificado.");
                    }

                    nombreNormalizado = ObtenerNombreNormalizado(usuario.Nombre_Usuario, nombreUsuario);
                }

                if (string.IsNullOrWhiteSpace(nombreNormalizado))
                {
                    throw new FaultException("No se encontró el usuario especificado.");
                }

                callback = ObtenerCallbackActual();
                _suscripciones.AddOrUpdate(nombreNormalizado, callback, (_, __) => callback);

                if (!string.Equals(nombreUsuario, nombreNormalizado, StringComparison.Ordinal))
                {
                    RemoverSuscripcion(nombreUsuario);
                }

                var canal = OperationContext.Current?.Channel;
                if (canal != null)
                {
                    canal.Closed += (_, __) => RemoverSuscripcion(nombreNormalizado);
                    canal.Faulted += (_, __) => RemoverSuscripcion(nombreNormalizado);
                }

                NotificarSolicitudesPendientesAlSuscribir(nombreNormalizado, usuario.idUsuario);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.AmistadSuscribirErrorBD, ex);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarSolicitudes);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.AmistadSuscribirErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorRecuperarSolicitudes);
            }
        }


        private void NotificarSolicitudesPendientesAlSuscribir(string nombreNormalizado, int usuarioId)
        {
            try
            {
                List<SolicitudAmistadDTO> solicitudesDTO = ServicioAmistad.ObtenerSolicitudesPendientesDTO(usuarioId);

                if (solicitudesDTO == null || solicitudesDTO.Count == 0)
                {
                    return;
                }

                foreach (var dto in solicitudesDTO)
                {
                    NotificarSolicitud(nombreNormalizado, dto);
                }
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al recuperar las solicitudes pendientes de amistad", ex);
            }
        }


        public void CancelarSuscripcion(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new FaultException("El nombre de usuario es obligatorio para cancelar la suscripción.");
            }

            RemoverSuscripcion(nombreUsuario);
        }

        public void EnviarSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor)
        {
            ValidarNombreUsuario(nombreUsuarioEmisor, nameof(nombreUsuarioEmisor));
            ValidarNombreUsuario(nombreUsuarioReceptor, nameof(nombreUsuarioReceptor));


            try
            {
                Usuario usuarioEmisor;
                Usuario usuarioReceptor;

                using (var contexto = CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    usuarioEmisor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioEmisor);
                    usuarioReceptor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioReceptor);

                    if (usuarioEmisor == null || usuarioReceptor == null)
                    {
                        throw new FaultException("Alguno de los usuarios especificados no existe.");
                    }

                    ServicioAmistad.CrearSolicitud(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario);
                }

                string nombreEmisor = ObtenerNombreNormalizado(usuarioEmisor.Nombre_Usuario, nombreUsuarioEmisor);
                string nombreReceptor = ObtenerNombreNormalizado(usuarioReceptor.Nombre_Usuario, nombreUsuarioReceptor);

                var solicitud = new SolicitudAmistadDTO
                {
                    UsuarioEmisor = nombreEmisor,
                    UsuarioReceptor = nombreReceptor,
                    SolicitudAceptada = false
                };

                NotificarSolicitud(nombreReceptor, solicitud);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Regla de negocio violada al enviar solicitud de amistad", ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al enviar la solicitud de amistad", ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.AmistadEnviarSolicitudErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorAlmacenarSolicitud);
            }
        }

        public void ResponderSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor)
        {
            ValidarNombreUsuario(nombreUsuarioEmisor, nameof(nombreUsuarioEmisor));
            ValidarNombreUsuario(nombreUsuarioReceptor, nameof(nombreUsuarioReceptor));

            string nombreEmisorNormalizado;
            string nombreReceptorNormalizado;

            try
            {
                using (var contexto = CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    Usuario usuarioEmisor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioEmisor);
                    Usuario usuarioReceptor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioReceptor);

                    if (usuarioEmisor == null || usuarioReceptor == null)
                    {
                        throw new FaultException("Alguno de los usuarios especificados no existe.");
                    }

                    ServicioAmistad.AceptarSolicitud(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario);

                    nombreEmisorNormalizado = ObtenerNombreNormalizado(usuarioEmisor.Nombre_Usuario, nombreUsuarioEmisor);
                    nombreReceptorNormalizado = ObtenerNombreNormalizado(usuarioReceptor.Nombre_Usuario, nombreUsuarioReceptor);

                    var solicitud = new SolicitudAmistadDTO
                    {
                        UsuarioEmisor = nombreEmisorNormalizado,
                        UsuarioReceptor = nombreReceptorNormalizado,
                        SolicitudAceptada = true
                    };

                    NotificarSolicitud(nombreEmisorNormalizado, solicitud);
                    NotificarSolicitud(nombreReceptorNormalizado, solicitud);
                }

                ListaAmigosManejador.NotificarCambioAmistad(nombreEmisorNormalizado);
                ListaAmigosManejador.NotificarCambioAmistad(nombreReceptorNormalizado);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Regla de negocio violada al aceptar solicitud de amistad", ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al aceptar la solicitud de amistad", ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.AmistadResponderSolicitudErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorActualizarSolicitud);
            }
        }

        public void EliminarAmigo(string nombreUsuarioA, string nombreUsuarioB)
        {
            ValidarNombreUsuario(nombreUsuarioA, nameof(nombreUsuarioA));
            ValidarNombreUsuario(nombreUsuarioB, nameof(nombreUsuarioB));

            string nombreUsuarioANormalizado;
            string nombreUsuarioBNormalizado;

            try
            {
                Amigo relacionEliminada;
                int idUsuarioA;

                using (var contexto = CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    Usuario usuarioA = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioA);
                    Usuario usuarioB = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioB);

                    if (usuarioA == null || usuarioB == null)
                    {
                        throw new FaultException("Alguno de los usuarios especificados no existe.");
                    }

                    idUsuarioA = usuarioA.idUsuario;

                    relacionEliminada = ServicioAmistad.EliminarAmistad(usuarioA.idUsuario, usuarioB.idUsuario);

                    nombreUsuarioANormalizado = ObtenerNombreNormalizado(usuarioA.Nombre_Usuario, nombreUsuarioA);
                    nombreUsuarioBNormalizado = ObtenerNombreNormalizado(usuarioB.Nombre_Usuario, nombreUsuarioB);
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

                NotificarEliminacion(nombreUsuarioANormalizado, solicitud);
                NotificarEliminacion(nombreUsuarioBNormalizado, solicitud);

                ListaAmigosManejador.NotificarCambioAmistad(nombreUsuarioANormalizado);
                ListaAmigosManejador.NotificarCambioAmistad(nombreUsuarioBNormalizado);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Regla de negocio violada al eliminar amistad", ex);
                throw new FaultException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al eliminar la relación de amistad", ex);
                throw new FaultException(ex.Message);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.AmistadEliminarErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorEliminarAmistad);
            }
        }


        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }

        private static void ValidarNombreUsuario(string nombreUsuario, string parametro)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new FaultException($"El parámetro {parametro} es obligatorio.");
            }
        }

        private static string ObtenerNombreNormalizado(string nombreBaseDatos, string nombreAlterno)
        {
            string nombre = nombreBaseDatos?.Trim();

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                return nombre;
            }

            return nombreAlterno?.Trim();
        }

        private static IAmigosManejadorCallback ObtenerCallbackActual()
        {
            var contexto = OperationContext.Current;
            if (contexto != null)
            {
                var callback = contexto.GetCallbackChannel<IAmigosManejadorCallback>();
                if (callback != null)
                {
                    return callback;
                }

                throw new FaultException("No se pudo obtener el canal de retorno para el usuario.");
            }

            throw new FaultException("No se pudo obtener el contexto de la operación para suscribir el callback.");
        }

        private static void RemoverSuscripcion(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            _suscripciones.TryRemove(nombreUsuario, out _);
        }

        private void NotificarSolicitud(string nombreUsuario, SolicitudAmistadDTO solicitud)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            if (!_suscripciones.TryGetValue(nombreUsuario, out var callback))
            {
                return;
            }

            try
            {
                callback.NotificarSolicitudActualizada(solicitud);
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
                _logger.Warn($"Error al notificar la solicitud de amistad al usuario {nombreUsuario}", ex);
            }
        }

        private void NotificarEliminacion(string nombreUsuario, SolicitudAmistadDTO solicitud)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            if (!_suscripciones.TryGetValue(nombreUsuario, out var callback))
            {
                return;
            }

            try
            {
                callback.NotificarAmistadEliminada(solicitud);
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
                _logger.Warn($"Error al notificar la eliminación de amistad al usuario {nombreUsuario}", ex);
            }
        }
    }
}