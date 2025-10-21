using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
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
    public class AmigosManejador : IAmigosManejador
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AmigosManejador));
        private static readonly ConcurrentDictionary<int, CallbackCliente> ClientesConectados = new ConcurrentDictionary<int, CallbackCliente>();

        public void Suscribirse(string nombreUsuario)
        {
            string nombreNormalizado = nombreUsuario?.Trim();
            if (string.IsNullOrWhiteSpace(nombreNormalizado))
            {
                throw new FaultException("El nombre de usuario es obligatorio para suscribirse a las notificaciones de amistad.");
            }

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    Usuario usuario = ObtenerUsuarioPorNombre(contexto, nombreNormalizado);
                    if (usuario == null)
                    {
                        throw new FaultException("El usuario especificado no existe.");
                    }

                    IAmigosManejadorCallback callback = OperationContext.Current.GetCallbackChannel<IAmigosManejadorCallback>();
                    ICommunicationObject canal = callback as ICommunicationObject;

                    RemoverClientePorId(usuario.idUsuario);

                    if (canal != null)
                    {
                        canal.Closed += CanalCerradoOCancelado;
                        canal.Faulted += CanalCerradoOCancelado;
                    }

                    ClientesConectados[usuario.idUsuario] = new CallbackCliente(callback, canal);
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (CommunicationException ex)
            {
                Logger.Error("Error de comunicación al suscribir el usuario a las notificaciones de amistad", ex);
                throw new FaultException("Se produjo un error de comunicación al suscribirse a las notificaciones de amistad.");
            }
            catch (TimeoutException ex)
            {
                Logger.Error("La suscripción a las notificaciones de amistad excedió el tiempo de espera", ex);
                throw new FaultException("La suscripción a las notificaciones de amistad excedió el tiempo de espera.");
            }
            catch (DataException ex)
            {
                Logger.Error("Error de base de datos al suscribir el usuario a las notificaciones de amistad", ex);
                throw new FaultException("Ocurrió un error al acceder a la base de datos durante la suscripción a las notificaciones de amistad.");
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Operación inválida al suscribir el usuario a las notificaciones de amistad", ex);
                throw new FaultException("No fue posible completar la suscripción a las notificaciones de amistad.");
            }
        }

        public void Desuscribirse(string nombreUsuario)
        {
            string nombreNormalizado = nombreUsuario?.Trim();
            if (string.IsNullOrWhiteSpace(nombreNormalizado))
            {
                return;
            }

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    Usuario usuario = ObtenerUsuarioPorNombre(contexto, nombreNormalizado);
                    if (usuario == null)
                    {
                        return;
                    }

                    RemoverClientePorId(usuario.idUsuario);
                }
            }
            catch (CommunicationException ex)
            {
                Logger.Warn("Error de comunicación al desuscribir al usuario de las notificaciones de amistad", ex);
            }
            catch (TimeoutException ex)
            {
                Logger.Warn("La desuscripción de las notificaciones de amistad excedió el tiempo de espera", ex);
            }
            catch (DataException ex)
            {
                Logger.Warn("Error de base de datos al desuscribir al usuario de las notificaciones de amistad", ex);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn("Operación inválida al desuscribir al usuario de las notificaciones de amistad", ex);
            }
        }

        public ResultadoOperacionDTO EnviarSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor)
        {
            string emisorNormalizado = nombreUsuarioEmisor?.Trim();
            string receptorNormalizado = nombreUsuarioReceptor?.Trim();

            ResultadoOperacionDTO validacion = ValidarNombresUsuarios(emisorNormalizado, receptorNormalizado);
            if (!validacion.OperacionExitosa)
            {
                return validacion;
            }

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    Usuario emisor = ObtenerUsuarioPorNombre(contexto, emisorNormalizado);
                    Usuario receptor = ObtenerUsuarioPorNombre(contexto, receptorNormalizado);

                    if (emisor == null || receptor == null)
                    {
                        return CrearResultadoFallo("No se encontró alguno de los usuarios especificados.");
                    }

                    if (emisor.idUsuario == receptor.idUsuario)
                    {
                        return CrearResultadoFallo("No puedes enviarte una solicitud de amistad a ti mismo.");
                    }

                    var amigoRepositorio = new AmigoRepositorio(contexto);

                    if (amigoRepositorio.ExisteRelacion(emisor.idUsuario, receptor.idUsuario))
                    {
                        return CrearResultadoFallo("Ya existe una solicitud o amistad entre los usuarios indicados.");
                    }

                    amigoRepositorio.CrearSolicitud(emisor.idUsuario, receptor.idUsuario);

                    SolicitudAmistadDTO solicitud = new SolicitudAmistadDTO
                    {
                        UsuarioEmisor = emisor.Nombre_Usuario,
                        UsuarioReceptor = receptor.Nombre_Usuario,
                        EstaAceptada = false
                    };

                    NotificarAUsuarios(new[] { receptor.idUsuario, emisor.idUsuario }, callback => callback.NotificarSolicitudAmistad(solicitud));

                    return CrearResultadoExitoso("Solicitud de amistad enviada correctamente.");
                }
            }
            catch (DataException ex)
            {
                Logger.Error("Error de base de datos al enviar la solicitud de amistad", ex);
                return CrearResultadoFallo("No fue posible enviar la solicitud de amistad debido a un error de base de datos.");
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Operación inválida al enviar la solicitud de amistad", ex);
                return CrearResultadoFallo("No fue posible enviar la solicitud de amistad debido a un estado inesperado del sistema.");
            }
            catch (TimeoutException ex)
            {
                Logger.Error("La solicitud de amistad excedió el tiempo de espera", ex);
                return CrearResultadoFallo("No fue posible enviar la solicitud de amistad porque la operación excedió el tiempo de espera.");
            }
        }

        public ResultadoOperacionDTO ResponderSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor, bool aceptarSolicitud)
        {
            string emisorNormalizado = nombreUsuarioEmisor?.Trim();
            string receptorNormalizado = nombreUsuarioReceptor?.Trim();

            ResultadoOperacionDTO validacion = ValidarNombresUsuarios(emisorNormalizado, receptorNormalizado);
            if (!validacion.OperacionExitosa)
            {
                return validacion;
            }

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    Usuario emisor = ObtenerUsuarioPorNombre(contexto, emisorNormalizado);
                    Usuario receptor = ObtenerUsuarioPorNombre(contexto, receptorNormalizado);

                    if (emisor == null || receptor == null)
                    {
                        return CrearResultadoFallo("No se encontró alguno de los usuarios especificados.");
                    }

                    if (emisor.idUsuario == receptor.idUsuario)
                    {
                        return CrearResultadoFallo("La solicitud de amistad debe involucrar a dos usuarios distintos.");
                    }

                    var amigoRepositorio = new AmigoRepositorio(contexto);
                    Amigo solicitud = amigoRepositorio.ObtenerRelacion(emisor.idUsuario, receptor.idUsuario);

                    if (solicitud == null)
                    {
                        return CrearResultadoFallo("No existe una solicitud de amistad pendiente entre los usuarios indicados.");
                    }

                    if (solicitud.UsuarioReceptor != receptor.idUsuario)
                    {
                        return CrearResultadoFallo("Solo el destinatario original de la solicitud puede responderla.");
                    }

                    if (aceptarSolicitud)
                    {
                        if (solicitud.Estado)
                        {
                            return CrearResultadoExitoso("La solicitud de amistad ya había sido aceptada anteriormente.");
                        }

                        solicitud.Estado = true;
                        amigoRepositorio.GuardarCambios();

                        RespuestaSolicitudAmistadDTO respuesta = new RespuestaSolicitudAmistadDTO
                        {
                            UsuarioEmisor = emisor.Nombre_Usuario,
                            UsuarioReceptor = receptor.Nombre_Usuario,
                            SolicitudAceptada = true
                        };

                        NotificarAUsuarios(new[] { emisor.idUsuario, receptor.idUsuario }, callback => callback.NotificarRespuestaSolicitud(respuesta));

                        return CrearResultadoExitoso("Solicitud de amistad aceptada correctamente.");
                    }
                    else
                    {
                        if (solicitud.Estado)
                        {
                            return CrearResultadoFallo("No es posible rechazar una solicitud de amistad que ya fue aceptada.");
                        }

                        // Un rechazo elimina la fila de la tabla Amigo para permitir que el emisor envíe una nueva solicitud en el futuro.
                        bool eliminada = amigoRepositorio.EliminarAmistad(emisor.idUsuario, receptor.idUsuario);
                        if (!eliminada)
                        {
                            return CrearResultadoFallo("No fue posible rechazar la solicitud de amistad.");
                        }

                        RespuestaSolicitudAmistadDTO respuesta = new RespuestaSolicitudAmistadDTO
                        {
                            UsuarioEmisor = emisor.Nombre_Usuario,
                            UsuarioReceptor = receptor.Nombre_Usuario,
                            SolicitudAceptada = false
                        };

                        NotificarAUsuarios(new[] { emisor.idUsuario, receptor.idUsuario }, callback => callback.NotificarRespuestaSolicitud(respuesta));

                        return CrearResultadoExitoso("Solicitud de amistad rechazada correctamente.");
                    }
                }
            }
            catch (DataException ex)
            {
                Logger.Error("Error de base de datos al responder la solicitud de amistad", ex);
                return CrearResultadoFallo("No fue posible responder la solicitud de amistad debido a un error de base de datos.");
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Operación inválida al responder la solicitud de amistad", ex);
                return CrearResultadoFallo("No fue posible responder la solicitud de amistad debido a un estado inesperado del sistema.");
            }
            catch (TimeoutException ex)
            {
                Logger.Error("La respuesta a la solicitud de amistad excedió el tiempo de espera", ex);
                return CrearResultadoFallo("No fue posible responder la solicitud de amistad porque la operación excedió el tiempo de espera.");
            }
        }

        public ResultadoOperacionDTO EliminarAmigo(string nombreUsuarioA, string nombreUsuarioB)
        {
            string usuarioANormalizado = nombreUsuarioA?.Trim();
            string usuarioBNormalizado = nombreUsuarioB?.Trim();

            ResultadoOperacionDTO validacion = ValidarNombresUsuarios(usuarioANormalizado, usuarioBNormalizado);
            if (!validacion.OperacionExitosa)
            {
                return validacion;
            }

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    Usuario usuarioA = ObtenerUsuarioPorNombre(contexto, usuarioANormalizado);
                    Usuario usuarioB = ObtenerUsuarioPorNombre(contexto, usuarioBNormalizado);

                    if (usuarioA == null || usuarioB == null)
                    {
                        return CrearResultadoFallo("No se encontró alguno de los usuarios especificados.");
                    }

                    if (usuarioA.idUsuario == usuarioB.idUsuario)
                    {
                        return CrearResultadoFallo("No es posible eliminar una amistad consigo mismo.");
                    }

                    var amigoRepositorio = new AmigoRepositorio(contexto);
                    // Al eliminar manualmente a un amigo quitamos el registro persistido de la relación.
                    bool eliminada = amigoRepositorio.EliminarAmistad(usuarioA.idUsuario, usuarioB.idUsuario);

                    if (!eliminada)
                    {
                        return CrearResultadoFallo("No existe una amistad registrada entre los usuarios indicados.");
                    }

                    AmistadEliminadaDTO amistadEliminada = new AmistadEliminadaDTO
                    {
                        UsuarioA = usuarioA.Nombre_Usuario,
                        UsuarioB = usuarioB.Nombre_Usuario
                    };

                    NotificarAUsuarios(new[] { usuarioA.idUsuario, usuarioB.idUsuario }, callback => callback.NotificarAmistadEliminada(amistadEliminada));

                    return CrearResultadoExitoso("Amistad eliminada correctamente.");
                }
            }
            catch (DataException ex)
            {
                Logger.Error("Error de base de datos al eliminar la amistad", ex);
                return CrearResultadoFallo("No fue posible eliminar la amistad debido a un error de base de datos.");
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Operación inválida al eliminar la amistad", ex);
                return CrearResultadoFallo("No fue posible eliminar la amistad debido a un estado inesperado del sistema.");
            }
            catch (TimeoutException ex)
            {
                Logger.Error("La eliminación de la amistad excedió el tiempo de espera", ex);
                return CrearResultadoFallo("No fue posible eliminar la amistad porque la operación excedió el tiempo de espera.");
            }
        }

        private static ResultadoOperacionDTO ValidarNombresUsuarios(string nombreUsuarioA, string nombreUsuarioB)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuarioA) || string.IsNullOrWhiteSpace(nombreUsuarioB))
            {
                return CrearResultadoFallo("Los nombres de usuario son obligatorios.");
            }

            if (string.Equals(nombreUsuarioA, nombreUsuarioB, StringComparison.OrdinalIgnoreCase))
            {
                return CrearResultadoFallo("Debes proporcionar dos usuarios distintos.");
            }

            return CrearResultadoExitoso(string.Empty);
        }

        private static void NotificarAUsuarios(IEnumerable<int> usuariosDestino, Action<IAmigosManejadorCallback> notificacion)
        {
            if (usuariosDestino == null || notificacion == null)
            {
                return;
            }

            foreach (int idUsuario in usuariosDestino.Distinct())
            {
                if (!ClientesConectados.TryGetValue(idUsuario, out CallbackCliente cliente))
                {
                    continue;
                }

                try
                {
                    notificacion(cliente.Callback);
                }
                catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException || ex is ObjectDisposedException)
                {
                    Logger.Warn($"No se pudo notificar al usuario con id {idUsuario}. Se eliminará de la lista de suscriptores.", ex);
                    RemoverClientePorId(idUsuario);
                }
            }
        }

        private static void RemoverClientePorId(int idUsuario)
        {
            if (ClientesConectados.TryRemove(idUsuario, out CallbackCliente cliente) && cliente.Channel != null)
            {
                cliente.Channel.Closed -= CanalCerradoOCancelado;
                cliente.Channel.Faulted -= CanalCerradoOCancelado;
            }
        }

        private static void CanalCerradoOCancelado(object sender, EventArgs e)
        {
            if (sender is ICommunicationObject canal)
            {
                foreach (KeyValuePair<int, CallbackCliente> registro in ClientesConectados.Where(par => ReferenceEquals(par.Value.Channel, canal)).ToList())
                {
                    ClientesConectados.TryRemove(registro.Key, out _);
                }

                canal.Closed -= CanalCerradoOCancelado;
                canal.Faulted -= CanalCerradoOCancelado;
            }
        }

        private static Usuario ObtenerUsuarioPorNombre(BaseDatosPruebaEntities1 contexto, string nombreUsuario)
        {
            return contexto.Usuario.FirstOrDefault(u => u.Nombre_Usuario == nombreUsuario);
        }

        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }

        private static ResultadoOperacionDTO CrearResultadoExitoso(string mensaje)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = true,
                Mensaje = mensaje
            };
        }

        private static ResultadoOperacionDTO CrearResultadoFallo(string mensaje)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = mensaje
            };
        }

        private sealed class CallbackCliente
        {
            public CallbackCliente(IAmigosManejadorCallback callback, ICommunicationObject channel)
            {
                Callback = callback;
                Channel = channel;
            }

            public IAmigosManejadorCallback Callback { get; }

            public ICommunicationObject Channel { get; }
        }
    }
}
