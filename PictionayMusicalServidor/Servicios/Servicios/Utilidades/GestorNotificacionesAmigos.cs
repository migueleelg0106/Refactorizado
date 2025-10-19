using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;

namespace Servicios.Servicios.Utilidades
{
    internal static class GestorNotificacionesAmigos
    {
        private static readonly ConcurrentDictionary<int, IAmigosCallback> Suscriptores = new ConcurrentDictionary<int, IAmigosCallback>();

        public static void RegistrarCliente(int jugadorId, IAmigosCallback callback)
        {
            if (jugadorId <= 0 || callback == null)
            {
                return;
            }

            Suscriptores.AddOrUpdate(jugadorId, callback, (_, __) => callback);
        }

        public static void NotificarSolicitudRecibida(int jugadorId, SolicitudAmistadNotificacionDTO solicitud)
        {
            Notificar(jugadorId, callback => callback.SolicitudRecibida(solicitud));
        }

        public static void NotificarSolicitudActualizada(int jugadorId, SolicitudAmistadEstadoDTO estado)
        {
            Notificar(jugadorId, callback => callback.SolicitudActualizada(estado));
        }

        public static void NotificarAmigoAgregado(int jugadorId, AmigoDTO amigo)
        {
            Notificar(jugadorId, callback => callback.AmigoAgregado(amigo));
        }

        public static void NotificarAmistadEliminada(int jugadorId, AmistadEliminadaDTO eliminacion)
        {
            Notificar(jugadorId, callback => callback.AmistadEliminada(eliminacion));
        }

        private static void Notificar(int jugadorId, Action<IAmigosCallback> accion)
        {
            if (!Suscriptores.TryGetValue(jugadorId, out IAmigosCallback callback))
            {
                return;
            }

            try
            {
                accion(callback);
            }
            catch (CommunicationException)
            {
                Suscriptores.TryRemove(jugadorId, out _);
            }
            catch (TimeoutException)
            {
                Suscriptores.TryRemove(jugadorId, out _);
            }
            catch (ObjectDisposedException)
            {
                Suscriptores.TryRemove(jugadorId, out _);
            }
            catch (Exception)
            {
                Suscriptores.TryRemove(jugadorId, out _);
            }
        }
    }
}
