using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ListaAmigosManejador : IListaAmigosManejador
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ListaAmigosManejador));

        public IList<AmigoDTO> ObtenerListaAmigos(int jugadorId)
        {
            if (jugadorId <= 0)
            {
                throw new FaultException("El identificador de jugador es inválido.");
            }

            RegistrarCallbackSeguro(jugadorId);

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    var jugadorRepositorio = new JugadorRepositorio(contexto);
                    Jugador jugador = jugadorRepositorio.ObtenerJugadorConAvatar(jugadorId);
                    if (jugador == null)
                    {
                        throw new FaultException("No se encontró el jugador especificado.");
                    }

                    var solicitudRepositorio = new SolicitudRepositorio(contexto);
                    IList<Solicitud> solicitudes = solicitudRepositorio.ObtenerSolicitudesPorJugador(jugadorId);

                    List<AmigoDTO> amigos = new List<AmigoDTO>();

                    foreach (Solicitud solicitud in solicitudes)
                    {
                        if (!AmigosHelper.EstaAceptada(solicitud))
                        {
                            continue;
                        }

                        Jugador amigo = solicitud.Jugador_idJugador == jugadorId ? solicitud.Jugador1 : solicitud.Jugador;
                        AmigoDTO amigoDto = AmigosHelper.CrearAmigoDTO(amigo);
                        if (amigoDto != null)
                        {
                            amigos.Add(amigoDto);
                        }
                    }

                    return amigos;
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("Error al obtener la lista de amigos.", ex);
                throw new FaultException("No fue posible obtener la lista de amigos.");
            }
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

        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }
    }
}
