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
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AmigosManejador));
        private static readonly ConcurrentDictionary<string, IAmigosManejadorCallback> Suscripciones = new ConcurrentDictionary<string, IAmigosManejadorCallback>(StringComparer.OrdinalIgnoreCase);

        public void Suscribir(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new FaultException("El nombre de usuario es obligatorio para suscribirse a las notificaciones.");
            }

            IAmigosManejadorCallback callback = ObtenerCallbackActual();
            Suscripciones.AddOrUpdate(nombreUsuario, callback, (_, __) => callback);

            var canal = OperationContext.Current?.Channel;
            if (canal != null)
            {
                canal.Closed += (_, __) => RemoverSuscripcion(nombreUsuario);
                canal.Faulted += (_, __) => RemoverSuscripcion(nombreUsuario);
            }

            try
            {
                using (var contexto = CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    Usuario usuario = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuario);

                    if (usuario == null)
                    {
                        RemoverSuscripcion(nombreUsuario);
                        throw new FaultException("El usuario especificado no existe.");
                    }

                    var amigoRepositorio = new AmigoRepositorio(contexto);
                    var solicitudesPendientes = amigoRepositorio.ObtenerSolicitudesPendientes(usuario.idUsuario);

                    if (solicitudesPendientes == null || solicitudesPendientes.Count == 0)
                    {
                        return;
                    }

                    foreach (var solicitud in solicitudesPendientes)
                    {
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

                        NotificarSolicitud(nombreUsuario, dto);
                    }
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                Logger.Warn("Datos inválidos al recuperar las solicitudes pendientes de amistad", ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Estado inválido al recuperar las solicitudes pendientes de amistad", ex);
                throw new FaultException("No fue posible recuperar las solicitudes de amistad.");
            }
            catch (DataException ex)
            {
                Logger.Error("Error de datos al recuperar las solicitudes pendientes de amistad", ex);
                throw new FaultException("No fue posible recuperar las solicitudes de amistad.");
            }
            catch (Exception ex)
            {
                Logger.Error("Error inesperado al recuperar las solicitudes pendientes de amistad", ex);
                throw new FaultException("Ocurrió un error al recuperar las solicitudes de amistad.");
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

                    if (amigoRepositorio.ExisteRelacion(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario))
                    {
                        throw new FaultException("Ya existe una solicitud o relación de amistad entre los usuarios.");
                    }

                    amigoRepositorio.CrearSolicitud(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario);

                    var solicitud = new SolicitudAmistadDTO
                    {
                        UsuarioEmisor = nombreUsuarioEmisor,
                        UsuarioReceptor = nombreUsuarioReceptor,
                        SolicitudAceptada = false
                    };

                    NotificarSolicitud(nombreUsuarioEmisor, solicitud);
                    NotificarSolicitud(nombreUsuarioReceptor, solicitud);
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                Logger.Warn("Datos inválidos al enviar la solicitud de amistad", ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Estado inválido al enviar la solicitud de amistad", ex);
                throw new FaultException("No fue posible completar la solicitud de amistad.");
            }
            catch (DataException ex)
            {
                Logger.Error("Error de datos al enviar la solicitud de amistad", ex);
                throw new FaultException("No fue posible almacenar la solicitud de amistad.");
            }
            catch (Exception ex)
            {
                Logger.Error("Error inesperado al enviar la solicitud de amistad", ex);
                throw new FaultException("Ocurrió un error al enviar la solicitud de amistad.");
            }
        }

        public void ResponderSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor)
        {
            ValidarNombreUsuario(nombreUsuarioEmisor, nameof(nombreUsuarioEmisor));
            ValidarNombreUsuario(nombreUsuarioReceptor, nameof(nombreUsuarioReceptor));

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

                    if (relacion == null)
                    {
                        throw new FaultException("No existe una solicitud de amistad entre los usuarios.");
                    }

                    if (relacion.UsuarioReceptor != usuarioReceptor.idUsuario)
                    {
                        throw new FaultException("Solo el receptor de la solicitud puede aceptarla.");
                    }

                    if (relacion.Estado)
                    {
                        throw new FaultException("La solicitud de amistad ya fue aceptada con anterioridad.");
                    }

                    amigoRepositorio.ActualizarEstado(relacion, true);

                    var solicitud = new SolicitudAmistadDTO
                    {
                        UsuarioEmisor = nombreUsuarioEmisor,
                        UsuarioReceptor = nombreUsuarioReceptor,
                        SolicitudAceptada = true
                    };

                    NotificarSolicitud(nombreUsuarioEmisor, solicitud);
                    NotificarSolicitud(nombreUsuarioReceptor, solicitud);
                }

                ListaAmigosManejador.NotificarCambioAmistad(nombreUsuarioEmisor);
                ListaAmigosManejador.NotificarCambioAmistad(nombreUsuarioReceptor);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                Logger.Warn("Datos inválidos al aceptar la solicitud de amistad", ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Estado inválido al aceptar la solicitud de amistad", ex);
                throw new FaultException("No fue posible aceptar la solicitud de amistad.");
            }
            catch (DataException ex)
            {
                Logger.Error("Error de datos al aceptar la solicitud de amistad", ex);
                throw new FaultException("No fue posible actualizar la solicitud de amistad.");
            }
            catch (Exception ex)
            {
                Logger.Error("Error inesperado al aceptar la solicitud de amistad", ex);
                throw new FaultException("Ocurrió un error al aceptar la solicitud de amistad.");
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

                    if (relacion == null)
                    {
                        throw new FaultException("No existe una relación de amistad entre los usuarios.");
                    }

                    amigoRepositorio.EliminarRelacion(relacion);

                    var solicitud = new SolicitudAmistadDTO
                    {
                        UsuarioEmisor = relacion.UsuarioEmisor == usuarioA.idUsuario ? nombreUsuarioA : nombreUsuarioB,
                        UsuarioReceptor = relacion.UsuarioEmisor == usuarioA.idUsuario ? nombreUsuarioB : nombreUsuarioA,
                        SolicitudAceptada = false
                    };

                    NotificarEliminacion(nombreUsuarioA, solicitud);
                    NotificarEliminacion(nombreUsuarioB, solicitud);
                }

                ListaAmigosManejador.NotificarCambioAmistad(nombreUsuarioA);
                ListaAmigosManejador.NotificarCambioAmistad(nombreUsuarioB);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                Logger.Warn("Datos inválidos al eliminar la relación de amistad", ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Estado inválido al eliminar la relación de amistad", ex);
                throw new FaultException("No fue posible eliminar la relación de amistad.");
            }
            catch (DataException ex)
            {
                Logger.Error("Error de datos al eliminar la relación de amistad", ex);
                throw new FaultException("No fue posible eliminar la relación de amistad en la base de datos.");
            }
            catch (Exception ex)
            {
                Logger.Error("Error inesperado al eliminar la relación de amistad", ex);
                throw new FaultException("Ocurrió un error al eliminar la relación de amistad.");
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

        private static IAmigosManejadorCallback ObtenerCallbackActual()
        {
            var contexto = OperationContext.Current;
            if (contexto == null)
            {
                throw new FaultException("No se pudo obtener el contexto de la operación para suscribir el callback.");
            }

            var callback = contexto.GetCallbackChannel<IAmigosManejadorCallback>();
            if (callback == null)
            {
                throw new FaultException("No se pudo obtener el canal de retorno para el usuario.");
            }

            return callback;
        }

        private static void RemoverSuscripcion(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            Suscripciones.TryRemove(nombreUsuario, out _);
        }

        private void NotificarSolicitud(string nombreUsuario, SolicitudAmistadDTO solicitud)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            if (!Suscripciones.TryGetValue(nombreUsuario, out var callback))
            {
                return;
            }

            try
            {
                callback.SolicitudActualizada(solicitud);
            }
            catch (CommunicationException)
            {
                RemoverSuscripcion(nombreUsuario);
            }
            catch (TimeoutException)
            {
                RemoverSuscripcion(nombreUsuario);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error al notificar la solicitud de amistad al usuario {nombreUsuario}", ex);
            }
        }

        private void NotificarEliminacion(string nombreUsuario, SolicitudAmistadDTO solicitud)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            if (!Suscripciones.TryGetValue(nombreUsuario, out var callback))
            {
                return;
            }

            try
            {
                callback.AmistadEliminada(solicitud);
            }
            catch (CommunicationException)
            {
                RemoverSuscripcion(nombreUsuario);
            }
            catch (TimeoutException)
            {
                RemoverSuscripcion(nombreUsuario);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error al notificar la eliminación de amistad al usuario {nombreUsuario}", ex);
            }
        }
    }
}
