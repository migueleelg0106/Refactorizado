using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    public class ListaAmigosManejador : IListaAmigosManejador
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ListaAmigosManejador));
        private static readonly ConcurrentDictionary<string, IListaAmigosManejadorCallback> Suscriptores =
            new ConcurrentDictionary<string, IListaAmigosManejadorCallback>(StringComparer.OrdinalIgnoreCase);

        public ListaAmigosDTO ObtenerListaAmigos(string nombreUsuario)
        {
            nombreUsuario = nombreUsuario?.Trim();
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return CrearResultadoFallo("Se requiere el nombre de usuario para obtener la lista de amigos.", nombreUsuario);
            }

            try
            {
                ObtenerCanalCallback(nombreUsuario);

                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    var repositorio = new SolicitudAmistadRepositorio(contexto);
                    Jugador jugador = repositorio.ObtenerJugadorPorNombreUsuario(nombreUsuario);
                    if (jugador == null)
                    {
                        Suscriptores.TryRemove(nombreUsuario, out _);
                        return CrearResultadoFallo("No se encontró al jugador especificado.", nombreUsuario);
                    }

                    return CrearResultadoExitoso(nombreUsuario, repositorio, jugador);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al obtener la lista de amigos", ex);
                Suscriptores.TryRemove(nombreUsuario, out _);
                return CrearResultadoFallo("No fue posible obtener la lista de amigos.", nombreUsuario);
            }
        }

        internal static void NotificarActualizacionAmigos(params string[] nombresUsuarios)
        {
            if (nombresUsuarios == null || nombresUsuarios.Length == 0)
            {
                return;
            }

            foreach (string nombreNormalizado in nombresUsuarios
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!Suscriptores.ContainsKey(nombreNormalizado))
                {
                    continue;
                }

                try
                {
                    ListaAmigosDTO lista = ObtenerListaAmigosInterno(nombreNormalizado);
                    if (lista == null)
                    {
                        Suscriptores.TryRemove(nombreNormalizado, out _);
                        continue;
                    }

                    if (lista.OperacionExitosa)
                    {
                        NotificarCallback(nombreNormalizado, callback => callback.ListaAmigosActualizada(lista));
                    }
                    else
                    {
                        Suscriptores.TryRemove(nombreNormalizado, out _);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error al notificar la actualización de amigos para {nombreNormalizado}", ex);
                }
            }
        }

        private static ListaAmigosDTO ObtenerListaAmigosInterno(string nombreUsuario)
        {
            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    var repositorio = new SolicitudAmistadRepositorio(contexto);
                    Jugador jugador = repositorio.ObtenerJugadorPorNombreUsuario(nombreUsuario);
                    if (jugador == null)
                    {
                        return null;
                    }

                    return CrearResultadoExitoso(nombreUsuario, repositorio, jugador);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error al reconstruir la lista de amigos de {nombreUsuario}", ex);
                return null;
            }
        }

        private static ListaAmigosDTO CrearResultadoExitoso(string nombreUsuario, SolicitudAmistadRepositorio repositorio, Jugador jugador)
        {
            IEnumerable<string> nombresUsuarios = repositorio
                .ObtenerNombresAmigosDe(jugador.idJugador)
                ?? Enumerable.Empty<string>();

            List<string> nombresAmigos = nombresUsuarios
                .Select(nombre => nombre?.Trim())
                .Where(nombre => !string.IsNullOrWhiteSpace(nombre)
                    && !string.Equals(nombre, nombreUsuario, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new ListaAmigosDTO
            {
                OperacionExitosa = true,
                Mensaje = "Lista de amigos obtenida correctamente.",
                Jugador = nombreUsuario,
                Amigos = nombresAmigos
            };
        }

        private static ListaAmigosDTO CrearResultadoFallo(string mensaje, string nombreUsuario = null)
        {
            return new ListaAmigosDTO
            {
                OperacionExitosa = false,
                Mensaje = mensaje,
                Jugador = nombreUsuario,
                Amigos = new List<string>()
            };
        }

        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }

        private static void ObtenerCanalCallback(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            try
            {
                OperationContext contextoOperacion = OperationContext.Current;
                if (contextoOperacion == null)
                {
                    return;
                }

                IListaAmigosManejadorCallback callback = contextoOperacion.GetCallbackChannel<IListaAmigosManejadorCallback>();
                if (callback == null)
                {
                    return;
                }

                string clave = nombreUsuario.Trim();
                if (clave.Length == 0)
                {
                    return;
                }

                Suscriptores[clave] = callback;
                IContextChannel canal = contextoOperacion.Channel;
                canal.Faulted += (sender, args) => Suscriptores.TryRemove(clave, out _);
                canal.Closed += (sender, args) => Suscriptores.TryRemove(clave, out _);
            }
            catch (Exception ex)
            {
                Logger.Warn($"No fue posible registrar el canal de callbacks para {nombreUsuario}", ex);
            }
        }

        private static void NotificarCallback(string nombreUsuario, Action<IListaAmigosManejadorCallback> accion)
        {
            if (!Suscriptores.TryGetValue(nombreUsuario, out IListaAmigosManejadorCallback callback))
            {
                return;
            }

            try
            {
                accion(callback);
            }
            catch (Exception ex)
            {
                Logger.Warn($"No fue posible enviar la lista actualizada al jugador {nombreUsuario}", ex);
                Suscriptores.TryRemove(nombreUsuario, out _);
            }
        }
    }
}
