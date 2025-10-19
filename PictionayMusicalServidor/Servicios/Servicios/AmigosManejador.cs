using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using Datos.DAL.Implementaciones;
using Datos.Modelo;
using Datos.Utilidades;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using log4net;

namespace Servicios.Servicios
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AmigosManejador : IAmigosManejador
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AmigosManejador));
        private static readonly ConcurrentDictionary<string, IAmigosManejadorCallback> ClientesConectados =
            new ConcurrentDictionary<string, IAmigosManejadorCallback>(StringComparer.OrdinalIgnoreCase);

        public ResultadoOperacionDTO EnviarSolicitudAmistad(string nombreUsuarioRemitente, string nombreUsuarioReceptor)
        {
            ObtenerCanalCallback(nombreUsuarioRemitente);

            nombreUsuarioRemitente = nombreUsuarioRemitente?.Trim();
            nombreUsuarioReceptor = nombreUsuarioReceptor?.Trim();

            if (string.IsNullOrWhiteSpace(nombreUsuarioRemitente) || string.IsNullOrWhiteSpace(nombreUsuarioReceptor))
            {
                return CrearResultadoFallo("Se requiere el nombre de usuario del remitente y del receptor.");
            }

            if (string.Equals(nombreUsuarioRemitente, nombreUsuarioReceptor, StringComparison.OrdinalIgnoreCase))
            {
                return CrearResultadoFallo("No es posible enviarse una solicitud de amistad a sí mismo.");
            }

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    var solicitudRepositorio = new SolicitudAmistadRepositorio(contexto);

                    Jugador remitente = solicitudRepositorio.ObtenerJugadorPorNombreUsuario(nombreUsuarioRemitente);
                    if (remitente == null)
                    {
                        return CrearResultadoFallo("No se encontró al remitente especificado.");
                    }

                    Jugador receptor = solicitudRepositorio.ObtenerJugadorPorNombreUsuario(nombreUsuarioReceptor);
                    if (receptor == null)
                    {
                        return CrearResultadoFallo("No se encontró al receptor especificado.");
                    }

                    Solicitud existente = solicitudRepositorio.ObtenerSolicitudEntre(remitente.idJugador, receptor.idJugador);
                    if (existente != null)
                    {
                        if (EsSolicitudAceptada(existente))
                        {
                            return CrearResultadoFallo("Los jugadores ya son amigos.");
                        }

                        return CrearResultadoFallo("Ya existe una solicitud de amistad pendiente entre los jugadores.");
                    }

                    solicitudRepositorio.CrearSolicitud(remitente.idJugador, receptor.idJugador);
                }

                NotificarSolicitudEnviada(nombreUsuarioRemitente, nombreUsuarioReceptor);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = true,
                    Mensaje = "Solicitud de amistad enviada correctamente."
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Error al enviar la solicitud de amistad", ex);
                return CrearResultadoFallo("No fue posible enviar la solicitud de amistad.");
            }
        }

        public ResultadoOperacionDTO ResponderSolicitudAmistad(string nombreUsuarioRemitente, string nombreUsuarioReceptor, bool aceptada)
        {
            ObtenerCanalCallback(nombreUsuarioReceptor);

            nombreUsuarioRemitente = nombreUsuarioRemitente?.Trim();
            nombreUsuarioReceptor = nombreUsuarioReceptor?.Trim();

            if (string.IsNullOrWhiteSpace(nombreUsuarioRemitente) || string.IsNullOrWhiteSpace(nombreUsuarioReceptor))
            {
                return CrearResultadoFallo("Se requiere el nombre de usuario del remitente y del receptor.");
            }

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    var solicitudRepositorio = new SolicitudAmistadRepositorio(contexto);

                    Jugador remitente = solicitudRepositorio.ObtenerJugadorPorNombreUsuario(nombreUsuarioRemitente);
                    if (remitente == null)
                    {
                        return CrearResultadoFallo("No se encontró al remitente especificado.");
                    }

                    Jugador receptor = solicitudRepositorio.ObtenerJugadorPorNombreUsuario(nombreUsuarioReceptor);
                    if (receptor == null)
                    {
                        return CrearResultadoFallo("No se encontró al receptor especificado.");
                    }

                    Solicitud solicitud = solicitudRepositorio.ObtenerSolicitudEntre(remitente.idJugador, receptor.idJugador);
                    if (solicitud == null)
                    {
                        return CrearResultadoFallo("No existe una solicitud de amistad pendiente entre los jugadores.");
                    }

                    if (EsSolicitudAceptada(solicitud))
                    {
                        return CrearResultadoFallo("La solicitud de amistad ya fue atendida.");
                    }

                    if (aceptada)
                    {
                        solicitudRepositorio.ActualizarEstado(solicitud, true);
                    }
                    else
                    {
                        solicitudRepositorio.EliminarSolicitud(solicitud);
                    }
                }

                NotificarRespuestaSolicitud(nombreUsuarioRemitente, nombreUsuarioReceptor, aceptada);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = true,
                    Mensaje = aceptada
                        ? "Solicitud de amistad aceptada correctamente."
                        : "Solicitud de amistad rechazada."
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Error al responder la solicitud de amistad", ex);
                return CrearResultadoFallo("No fue posible procesar la respuesta a la solicitud de amistad.");
            }
        }

        public ResultadoOperacionDTO EliminarAmigo(string nombreUsuarioRemitente, string nombreUsuarioReceptor)
        {
            ObtenerCanalCallback(nombreUsuarioRemitente);

            nombreUsuarioRemitente = nombreUsuarioRemitente?.Trim();
            nombreUsuarioReceptor = nombreUsuarioReceptor?.Trim();

            if (string.IsNullOrWhiteSpace(nombreUsuarioRemitente) || string.IsNullOrWhiteSpace(nombreUsuarioReceptor))
            {
                return CrearResultadoFallo("Se requiere el nombre de usuario de ambos jugadores.");
            }

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    var solicitudRepositorio = new SolicitudAmistadRepositorio(contexto);

                    Jugador remitente = solicitudRepositorio.ObtenerJugadorPorNombreUsuario(nombreUsuarioRemitente);
                    if (remitente == null)
                    {
                        return CrearResultadoFallo("No se encontró al jugador especificado.");
                    }

                    Jugador receptor = solicitudRepositorio.ObtenerJugadorPorNombreUsuario(nombreUsuarioReceptor);
                    if (receptor == null)
                    {
                        return CrearResultadoFallo("No se encontró al amigo especificado.");
                    }

                    Solicitud solicitud = solicitudRepositorio.ObtenerSolicitudEntre(remitente.idJugador, receptor.idJugador);
                    if (solicitud == null || !EsSolicitudAceptada(solicitud))
                    {
                        return CrearResultadoFallo("Los jugadores no tienen una relación de amistad registrada.");
                    }

                    solicitudRepositorio.EliminarSolicitud(solicitud);
                }

                NotificarEliminacionAmistad(nombreUsuarioRemitente, nombreUsuarioReceptor);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = true,
                    Mensaje = "Amistad eliminada correctamente."
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Error al eliminar la amistad", ex);
                return CrearResultadoFallo("No fue posible eliminar la amistad.");
            }
        }

        private static ResultadoOperacionDTO CrearResultadoFallo(string mensaje)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = mensaje
            };
        }

        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }

        private static bool EsSolicitudAceptada(Solicitud solicitud)
        {
            if (solicitud?.Estado == null || solicitud.Estado.Length == 0)
            {
                return false;
            }

            return solicitud.Estado[0] != 0;
        }

        private static IAmigosManejadorCallback ObtenerCanalCallback(string nombreUsuario)
        {
            OperationContext contextoOperacion = OperationContext.Current;
            if (contextoOperacion == null)
            {
                return null;
            }

            IAmigosManejadorCallback callback = contextoOperacion.GetCallbackChannel<IAmigosManejadorCallback>();
            if (!string.IsNullOrWhiteSpace(nombreUsuario) && callback != null)
            {
                ClientesConectados[nombreUsuario] = callback;
                IContextChannel canal = contextoOperacion.Channel;
                canal.Faulted += (sender, args) => ClientesConectados.TryRemove(nombreUsuario, out _);
                canal.Closed += (sender, args) => ClientesConectados.TryRemove(nombreUsuario, out _);
            }

            return callback;
        }

        private static void NotificarSolicitudEnviada(string remitente, string receptor)
        {
            var notificacion = new SolicitudAmistadNotificacionDTO
            {
                Remitente = remitente,
                Receptor = receptor
            };

            NotificarCallback(receptor, callback => callback.SolicitudAmistadRecibida(notificacion));
            NotificarCallback(remitente, callback => callback.SolicitudAmistadRecibida(notificacion));
        }

        private static void NotificarRespuestaSolicitud(string remitente, string receptor, bool aceptada)
        {
            var notificacion = new RespuestaSolicitudAmistadNotificacionDTO
            {
                Remitente = remitente,
                Receptor = receptor,
                Aceptada = aceptada
            };

            NotificarCallback(remitente, callback => callback.SolicitudAmistadRespondida(notificacion));
            NotificarCallback(receptor, callback => callback.SolicitudAmistadRespondida(notificacion));
        }

        private static void NotificarEliminacionAmistad(string jugador, string amigo)
        {
            var notificacion = new AmistadEliminadaNotificacionDTO
            {
                Jugador = jugador,
                Amigo = amigo
            };

            NotificarCallback(jugador, callback => callback.AmistadEliminada(notificacion));
            NotificarCallback(amigo, callback => callback.AmistadEliminada(new AmistadEliminadaNotificacionDTO
            {
                Jugador = amigo,
                Amigo = jugador
            }));
        }

        private static void NotificarCallback(string nombreUsuario, Action<IAmigosManejadorCallback> accion)
        {
            if (!ClientesConectados.TryGetValue(nombreUsuario, out IAmigosManejadorCallback callback))
            {
                return;
            }

            try
            {
                accion(callback);
            }
            catch (Exception ex)
            {
                Logger.Warn($"No fue posible enviar una notificación al jugador {nombreUsuario}", ex);
            }
        }
    }
}
