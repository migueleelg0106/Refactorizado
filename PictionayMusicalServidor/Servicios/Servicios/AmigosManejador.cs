using System;
using System.ServiceModel;
using Datos.DAL.Implementaciones;
using Datos.Modelo;
using Datos.Utilidades;
using log4net;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using Servicios.Servicios.Utilidades;

namespace Servicios.Servicios
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AmigosManejador : IAmigosManejador
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AmigosManejador));

        public ResultadoOperacionDTO EnviarSolicitudAmistad(SolicitudAmistadDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new FaultException("La solicitud es obligatoria.");
            }

            RegistrarCallbackSeguro(solicitud.RemitenteId);

            if (solicitud.RemitenteId <= 0 || solicitud.DestinatarioId <= 0)
            {
                return CrearResultadoFallo("Los datos proporcionados no son válidos.");
            }

            if (solicitud.RemitenteId == solicitud.DestinatarioId)
            {
                return CrearResultadoFallo("No puedes enviarte una solicitud de amistad.");
            }

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    var jugadorRepositorio = new JugadorRepositorio(contexto);
                    var solicitudRepositorio = new SolicitudRepositorio(contexto);

                    Jugador remitente = jugadorRepositorio.ObtenerJugadorConAvatar(solicitud.RemitenteId);
                    Jugador destinatario = jugadorRepositorio.ObtenerJugadorConAvatar(solicitud.DestinatarioId);

                    if (remitente == null || destinatario == null)
                    {
                        return CrearResultadoFallo("No se encontró alguno de los jugadores especificados.");
                    }

                    Solicitud solicitudExistente = solicitudRepositorio.ObtenerSolicitudEntre(
                        solicitud.RemitenteId,
                        solicitud.DestinatarioId);

                    if (solicitudExistente != null)
                    {
                        if (AmigosHelper.EstaAceptada(solicitudExistente))
                        {
                            return CrearResultadoFallo("Los jugadores ya son amigos.");
                        }

                        if (solicitudExistente.Jugador_idJugador == solicitud.RemitenteId)
                        {
                            return CrearResultadoFallo("Ya enviaste una solicitud a este jugador.");
                        }

                        return CrearResultadoFallo("Tienes una solicitud pendiente de este jugador.");
                    }

                    Solicitud nuevaSolicitud = new Solicitud
                    {
                        Jugador_idJugador = solicitud.RemitenteId,
                        Jugador_idJugador1 = solicitud.DestinatarioId,
                        Estado = AmigosHelper.ConvertirEstado(false)
                    };

                    solicitudRepositorio.CrearSolicitud(nuevaSolicitud);

                    GestorNotificacionesAmigos.NotificarSolicitudRecibida(
                        solicitud.DestinatarioId,
                        new SolicitudAmistadNotificacionDTO
                        {
                            RemitenteId = solicitud.RemitenteId,
                            DestinatarioId = solicitud.DestinatarioId,
                            Remitente = AmigosHelper.CrearAmigoDTO(remitente)
                        });

                    return CrearResultadoExitoso("Solicitud de amistad enviada correctamente.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al enviar la solicitud de amistad.", ex);
                return CrearResultadoFallo("No fue posible enviar la solicitud de amistad.");
            }
        }

        public ResultadoOperacionDTO ResponderSolicitudAmistad(RespuestaSolicitudAmistadDTO respuesta)
        {
            if (respuesta == null)
            {
                throw new FaultException("La respuesta es obligatoria.");
            }

            RegistrarCallbackSeguro(respuesta.DestinatarioId);

            if (respuesta.RemitenteId <= 0 || respuesta.DestinatarioId <= 0)
            {
                return CrearResultadoFallo("Los datos proporcionados no son válidos.");
            }

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    var jugadorRepositorio = new JugadorRepositorio(contexto);
                    var solicitudRepositorio = new SolicitudRepositorio(contexto);

                    Jugador remitente = jugadorRepositorio.ObtenerJugadorConAvatar(respuesta.RemitenteId);
                    Jugador destinatario = jugadorRepositorio.ObtenerJugadorConAvatar(respuesta.DestinatarioId);

                    if (remitente == null || destinatario == null)
                    {
                        return CrearResultadoFallo("No se encontró alguno de los jugadores especificados.");
                    }

                    Solicitud solicitud = solicitudRepositorio.ObtenerSolicitudEntre(
                        respuesta.RemitenteId,
                        respuesta.DestinatarioId);

                    if (solicitud == null)
                    {
                        return CrearResultadoFallo("La solicitud de amistad no existe.");
                    }

                    if (!respuesta.Aceptada)
                    {
                        solicitudRepositorio.EliminarSolicitud(solicitud);

                        GestorNotificacionesAmigos.NotificarSolicitudActualizada(
                            respuesta.RemitenteId,
                            new SolicitudAmistadEstadoDTO
                            {
                                RemitenteId = respuesta.RemitenteId,
                                DestinatarioId = respuesta.DestinatarioId,
                                Aceptada = false
                            });

                        return CrearResultadoExitoso("Solicitud de amistad rechazada.");
                    }

                    solicitud.Estado = AmigosHelper.ConvertirEstado(true);
                    solicitudRepositorio.ActualizarSolicitud(solicitud);

                    GestorNotificacionesAmigos.NotificarSolicitudActualizada(
                        respuesta.RemitenteId,
                        new SolicitudAmistadEstadoDTO
                        {
                            RemitenteId = respuesta.RemitenteId,
                            DestinatarioId = respuesta.DestinatarioId,
                            Aceptada = true
                        });

                    GestorNotificacionesAmigos.NotificarSolicitudActualizada(
                        respuesta.DestinatarioId,
                        new SolicitudAmistadEstadoDTO
                        {
                            RemitenteId = respuesta.RemitenteId,
                            DestinatarioId = respuesta.DestinatarioId,
                            Aceptada = true
                        });

                    GestorNotificacionesAmigos.NotificarAmigoAgregado(
                        respuesta.RemitenteId,
                        AmigosHelper.CrearAmigoDTO(destinatario));

                    GestorNotificacionesAmigos.NotificarAmigoAgregado(
                        respuesta.DestinatarioId,
                        AmigosHelper.CrearAmigoDTO(remitente));

                    return CrearResultadoExitoso("Solicitud de amistad aceptada.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al responder la solicitud de amistad.", ex);
                return CrearResultadoFallo("No fue posible actualizar la solicitud de amistad.");
            }
        }

        public ResultadoOperacionDTO EliminarAmigo(OperacionAmistadDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new FaultException("La solicitud es obligatoria.");
            }

            RegistrarCallbackSeguro(solicitud.JugadorId);

            if (solicitud.JugadorId <= 0 || solicitud.AmigoId <= 0)
            {
                return CrearResultadoFallo("Los datos proporcionados no son válidos.");
            }

            if (solicitud.JugadorId == solicitud.AmigoId)
            {
                return CrearResultadoFallo("No puedes eliminarte como amigo.");
            }

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    var solicitudRepositorio = new SolicitudRepositorio(contexto);

                    Solicitud registro = solicitudRepositorio.ObtenerSolicitudEntre(
                        solicitud.JugadorId,
                        solicitud.AmigoId);

                    if (registro == null || !AmigosHelper.EstaAceptada(registro))
                    {
                        return CrearResultadoFallo("Los jugadores no tienen una amistad registrada.");
                    }

                    solicitudRepositorio.EliminarSolicitud(registro);

                    GestorNotificacionesAmigos.NotificarAmistadEliminada(
                        solicitud.JugadorId,
                        new AmistadEliminadaDTO
                        {
                            JugadorId = solicitud.JugadorId,
                            AmigoId = solicitud.AmigoId
                        });

                    GestorNotificacionesAmigos.NotificarAmistadEliminada(
                        solicitud.AmigoId,
                        new AmistadEliminadaDTO
                        {
                            JugadorId = solicitud.AmigoId,
                            AmigoId = solicitud.JugadorId
                        });

                    return CrearResultadoExitoso("Amigo eliminado correctamente.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al eliminar al amigo.", ex);
                return CrearResultadoFallo("No fue posible eliminar al amigo.");
            }
        }

        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }

        private static ResultadoOperacionDTO CrearResultadoFallo(string mensaje)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = mensaje
            };
        }

        private static ResultadoOperacionDTO CrearResultadoExitoso(string mensaje)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = true,
                Mensaje = mensaje
            };
        }

        private static void RegistrarCallbackSeguro(int jugadorId)
        {
            if (jugadorId <= 0)
            {
                return;
            }

            try
            {
                IAmigosCallback callback = OperationContext.Current?.GetCallbackChannel<IAmigosCallback>();
                if (callback != null)
                {
                    GestorNotificacionesAmigos.RegistrarCliente(jugadorId, callback);
                }
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
