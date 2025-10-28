using System;
using System.Collections.Concurrent;
using System.Data;
using System.ServiceModel;
using Datos.DAL.Implementaciones;
using Datos.Utilidades;
using Datos.Modelo;
using log4net;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;

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

            try
            {
                using var contexto = CrearContexto();
                var usuarioRepositorio = new UsuarioRepositorio(contexto);
                Usuario usuario = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuario);

                if (usuario == null)
                {
                    throw new FaultException("El usuario especificado no existe.");
                }

                string nombreNormalizado = ObtenerNombreNormalizado(usuario.Nombre_Usuario, nombreUsuario);

                if (string.IsNullOrWhiteSpace(nombreNormalizado))
                {
                    throw new FaultException("El usuario especificado no existe.");
                }

                IAmigosManejadorCallback callback = ObtenerCallbackActual();
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

                var amigoRepositorio = new AmigoRepositorio(contexto);
                var solicitudesPendientes = amigoRepositorio.ObtenerSolicitudesPendientes(usuario.idUsuario);

                if (solicitudesPendientes == null || solicitudesPendientes.Count == 0)
                {
                    return;
                }

                foreach (var solicitud in solicitudesPendientes)
                {
                    if (solicitud.UsuarioReceptor != usuario.idUsuario)
                    {
                        continue;
                    }

                    string emisor = solicitud.Usuario?.Nombre_Usuario;
                    string receptor = solicitud.Usuario1?.Nombre_Usuario;

                    if (string.IsNullOrWhiteSpace(emisor) || string.IsNullOrWhiteSpace(receptor))
                    {
                        continue;
                    }

                    var dto = new SolicitudAmistadDTO
                    {
                        UsuarioEmisor = emisor,
                        UsuarioReceptor = receptor,
                        SolicitudAceptada = solicitud.Estado
                    };

                    NotificarSolicitud(nombreNormalizado, dto);
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al recuperar las solicitudes pendientes de amistad", ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Estado inválido al recuperar las solicitudes pendientes de amistad", ex);
                throw new FaultException("No fue posible recuperar las solicitudes de amistad.");
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al recuperar las solicitudes pendientes de amistad", ex);
                throw new FaultException("No fue posible recuperar las solicitudes de amistad.");
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

            if (string.Equals(nombreUsuarioEmisor, nombreUsuarioReceptor, StringComparison.OrdinalIgnoreCase))
            {
                throw new FaultException("No es posible enviarse una solicitud de amistad a sí mismo.");
            }

            try
            {
                using var contexto = CrearContexto();
                var usuarioRepositorio = new UsuarioRepositorio(contexto);
                var amigoRepositorio = new AmigoRepositorio(contexto);

                Usuario usuarioEmisor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioEmisor);
                Usuario usuarioReceptor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioReceptor);

                if (usuarioEmisor == null || usuarioReceptor == null)
                {
                    throw new FaultException("Alguno de los usuarios especificados no existe.");
                }

                if (amigoRepositorio.ExisteRelacion(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario))
                {
                    throw new FaultException("Ya existe una solicitud o relación de amistad entre los usuarios.");
                }

                amigoRepositorio.CrearSolicitud(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario);

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
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al enviar la solicitud de amistad", ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Estado inválido al enviar la solicitud de amistad", ex);
                throw new FaultException("No fue posible completar la solicitud de amistad.");
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al enviar la solicitud de amistad", ex);
                throw new FaultException("No fue posible almacenar la solicitud de amistad.");
            }
        }

        public void ResponderSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor)
        {
            ValidarNombreUsuario(nombreUsuarioEmisor, nameof(nombreUsuarioEmisor));
            ValidarNombreUsuario(nombreUsuarioReceptor, nameof(nombreUsuarioReceptor));

            string nombreEmisorNormalizado = null;
            string nombreReceptorNormalizado = null;

            try
            {
                using (var contexto = CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    var amigoRepositorio = new AmigoRepositorio(contexto);

                    Usuario usuarioEmisor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioEmisor);
                    Usuario usuarioReceptor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioReceptor);

                    if (usuarioEmisor == null || usuarioReceptor == null)
                    {
                        throw new FaultException("Alguno de los usuarios especificados no existe.");
                    }

                    var relacion = amigoRepositorio.ObtenerRelacion(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario);

                    if (relacion != null)
                    {
                        if (relacion.UsuarioReceptor != usuarioReceptor.idUsuario)
                        {
                            throw new FaultException("Solo el receptor de la solicitud puede aceptarla.");
                        }

                        if (relacion.Estado)
                        {
                            throw new FaultException("La solicitud de amistad ya fue aceptada con anterioridad.");
                        }

                        amigoRepositorio.ActualizarEstado(relacion, true);

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
                    else
                    {
                        throw new FaultException("No existe una solicitud de amistad entre los usuarios.");
                    }
                }

                ListaAmigosManejador.NotificarCambioAmistad(nombreEmisorNormalizado);
                ListaAmigosManejador.NotificarCambioAmistad(nombreReceptorNormalizado);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al aceptar la solicitud de amistad", ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Estado inválido al aceptar la solicitud de amistad", ex);
                throw new FaultException("No fue posible aceptar la solicitud de amistad.");
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al aceptar la solicitud de amistad", ex);
                throw new FaultException("No fue posible actualizar la solicitud de amistad.");
            }
        }

        public void EliminarAmigo(string nombreUsuarioA, string nombreUsuarioB)
        {
            ValidarNombreUsuario(nombreUsuarioA, nameof(nombreUsuarioA));
            ValidarNombreUsuario(nombreUsuarioB, nameof(nombreUsuarioB));

            if (string.Equals(nombreUsuarioA, nombreUsuarioB, StringComparison.OrdinalIgnoreCase))
            {
                throw new FaultException("No es posible eliminar una relación consigo mismo.");
            }

            string nombreUsuarioANormalizado = null;
            string nombreUsuarioBNormalizado = null;

            try
            {
                using (var contexto = CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    var amigoRepositorio = new AmigoRepositorio(contexto);

                    Usuario usuarioA = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioA);
                    Usuario usuarioB = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioB);

                    if (usuarioA == null || usuarioB == null)
                    {
                        throw new FaultException("Alguno de los usuarios especificados no existe.");
                    }

                    var relacion = amigoRepositorio.ObtenerRelacion(usuarioA.idUsuario, usuarioB.idUsuario);

                    if (relacion != null)
                    {
                        amigoRepositorio.EliminarRelacion(relacion);

                        nombreUsuarioANormalizado = ObtenerNombreNormalizado(usuarioA.Nombre_Usuario, nombreUsuarioA);
                        nombreUsuarioBNormalizado = ObtenerNombreNormalizado(usuarioB.Nombre_Usuario, nombreUsuarioB);

                        bool usuarioAEsEmisor = relacion.UsuarioEmisor == usuarioA.idUsuario;
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
                    }
                    else
                    {
                        throw new FaultException("No existe una relación de amistad entre los usuarios.");
                    }
                }

                ListaAmigosManejador.NotificarCambioAmistad(nombreUsuarioANormalizado);
                ListaAmigosManejador.NotificarCambioAmistad(nombreUsuarioBNormalizado);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos inválidos al eliminar la relación de amistad", ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Estado inválido al eliminar la relación de amistad", ex);
                throw new FaultException("No fue posible eliminar la relación de amistad.");
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al eliminar la relación de amistad", ex);
                throw new FaultException("No fue posible eliminar la relación de amistad en la base de datos.");
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