using System;
using System.Data;
using System.Data.Entity.Core;
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
    public class AmigosManejador : IAmigosManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AmigosManejador));
        private static readonly ManejadorCallback<IAmigosManejadorCallback> _manejadorCallback = new(StringComparer.OrdinalIgnoreCase);
        private static readonly NotificadorAmigos _notificador = new(_manejadorCallback);

        public void Suscribir(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new FaultException(MensajesError.Cliente.NombreUsuarioObligatorioSuscripcion);
            }

            Usuario usuario;
            string nombreNormalizado;
            IAmigosManejadorCallback callback;

            try
            {
                using (var contexto = ContextoFactory.CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    usuario = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuario);

                    if (usuario == null)
                    {
                        throw new FaultException(MensajesError.Cliente.UsuarioNoEncontrado);
                    }

                    nombreNormalizado = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuario.Nombre_Usuario, nombreUsuario);
                }

                if (string.IsNullOrWhiteSpace(nombreNormalizado))
                {
                    throw new FaultException(MensajesError.Cliente.UsuarioNoEncontrado);
                }

                callback = ManejadorCallback<IAmigosManejadorCallback>.ObtenerCallbackActual();
                _manejadorCallback.Suscribir(nombreNormalizado, callback);

                if (!string.Equals(nombreUsuario, nombreNormalizado, StringComparison.Ordinal))
                {
                    _manejadorCallback.Desuscribir(nombreUsuario);
                }

                _manejadorCallback.ConfigurarEventosCanal(nombreNormalizado);

                _notificador.NotificarSolicitudesPendientesAlSuscribir(nombreNormalizado, usuario.idUsuario);
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

        public void CancelarSuscripcion(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new FaultException(MensajesError.Cliente.NombreUsuarioObligatorioCancelar);
            }

            _manejadorCallback.Desuscribir(nombreUsuario);
        }

        public void EnviarSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor)
        {
            ValidadorNombreUsuario.Validar(nombreUsuarioEmisor, nameof(nombreUsuarioEmisor));
            ValidadorNombreUsuario.Validar(nombreUsuarioReceptor, nameof(nombreUsuarioReceptor));

            try
            {
                Usuario usuarioEmisor;
                Usuario usuarioReceptor;

                using (var contexto = ContextoFactory.CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    usuarioEmisor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioEmisor);
                    usuarioReceptor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioReceptor);

                    if (usuarioEmisor == null || usuarioReceptor == null)
                    {
                        throw new FaultException(MensajesError.Cliente.UsuariosEspecificadosNoExisten);
                    }

                    ServicioAmistad.CrearSolicitud(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario);
                }

                string nombreEmisor = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioEmisor.Nombre_Usuario, nombreUsuarioEmisor);
                string nombreReceptor = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioReceptor.Nombre_Usuario, nombreUsuarioReceptor);

                var solicitud = new SolicitudAmistadDTO
                {
                    UsuarioEmisor = nombreEmisor,
                    UsuarioReceptor = nombreReceptor,
                    SolicitudAceptada = false
                };

                _notificador.NotificarSolicitudActualizada(nombreReceptor, solicitud);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.AmistadEnviarSolicitudReglaNegocio, ex);
                string mensaje = !string.IsNullOrWhiteSpace(ex.Message) ? ex.Message : MensajesError.Cliente.ErrorAlmacenarSolicitud;
                throw new FaultException(mensaje);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.AmistadEnviarSolicitudDatosInvalidos, ex);
                throw new FaultException(MensajesError.Cliente.DatosInvalidos);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.AmistadEnviarSolicitudErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorAlmacenarSolicitud);
            }
        }

        public void ResponderSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor)
        {
            ValidadorNombreUsuario.Validar(nombreUsuarioEmisor, nameof(nombreUsuarioEmisor));
            ValidadorNombreUsuario.Validar(nombreUsuarioReceptor, nameof(nombreUsuarioReceptor));

            string nombreEmisorNormalizado;
            string nombreReceptorNormalizado;

            try
            {
                using (var contexto = ContextoFactory.CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    Usuario usuarioEmisor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioEmisor);
                    Usuario usuarioReceptor = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioReceptor);

                    if (usuarioEmisor == null || usuarioReceptor == null)
                    {
                        throw new FaultException(MensajesError.Cliente.UsuariosEspecificadosNoExisten);
                    }

                    ServicioAmistad.AceptarSolicitud(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario);

                    nombreEmisorNormalizado = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioEmisor.Nombre_Usuario, nombreUsuarioEmisor);
                    nombreReceptorNormalizado = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioReceptor.Nombre_Usuario, nombreUsuarioReceptor);

                    var solicitud = new SolicitudAmistadDTO
                    {
                        UsuarioEmisor = nombreEmisorNormalizado,
                        UsuarioReceptor = nombreReceptorNormalizado,
                        SolicitudAceptada = true
                    };

                    _notificador.NotificarSolicitudActualizada(nombreEmisorNormalizado, solicitud);
                    _notificador.NotificarSolicitudActualizada(nombreReceptorNormalizado, solicitud);
                }

                ListaAmigosManejador.NotificarCambioAmistad(nombreEmisorNormalizado);
                ListaAmigosManejador.NotificarCambioAmistad(nombreReceptorNormalizado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.AmistadResponderSolicitudReglaNegocio, ex);
                string mensaje = !string.IsNullOrWhiteSpace(ex.Message) ? ex.Message : MensajesError.Cliente.ErrorActualizarSolicitud;
                throw new FaultException(mensaje);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.AmistadResponderSolicitudDatosInvalidos, ex);
                throw new FaultException(MensajesError.Cliente.DatosInvalidos);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.AmistadResponderSolicitudErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorActualizarSolicitud);
            }
        }

        public void EliminarAmigo(string nombreUsuarioA, string nombreUsuarioB)
        {
            ValidadorNombreUsuario.Validar(nombreUsuarioA, nameof(nombreUsuarioA));
            ValidadorNombreUsuario.Validar(nombreUsuarioB, nameof(nombreUsuarioB));

            string nombreUsuarioANormalizado;
            string nombreUsuarioBNormalizado;

            try
            {
                Amigo relacionEliminada;
                int idUsuarioA;

                using (var contexto = ContextoFactory.CrearContexto())
                {
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    Usuario usuarioA = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioA);
                    Usuario usuarioB = usuarioRepositorio.ObtenerPorNombreUsuario(nombreUsuarioB);

                    if (usuarioA == null || usuarioB == null)
                    {
                        throw new FaultException(MensajesError.Cliente.UsuariosEspecificadosNoExisten);
                    }

                    idUsuarioA = usuarioA.idUsuario;

                    relacionEliminada = ServicioAmistad.EliminarAmistad(usuarioA.idUsuario, usuarioB.idUsuario);

                    nombreUsuarioANormalizado = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioA.Nombre_Usuario, nombreUsuarioA);
                    nombreUsuarioBNormalizado = ValidadorNombreUsuario.ObtenerNombreNormalizado(usuarioB.Nombre_Usuario, nombreUsuarioB);
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

                _notificador.NotificarAmistadEliminada(nombreUsuarioANormalizado, solicitud);
                _notificador.NotificarAmistadEliminada(nombreUsuarioBNormalizado, solicitud);

                ListaAmigosManejador.NotificarCambioAmistad(nombreUsuarioANormalizado);
                ListaAmigosManejador.NotificarCambioAmistad(nombreUsuarioBNormalizado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.AmistadEliminarReglaNegocio, ex);
                string mensaje = !string.IsNullOrWhiteSpace(ex.Message) ? ex.Message : MensajesError.Cliente.ErrorEliminarAmistad;
                throw new FaultException(mensaje);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.AmistadEliminarDatosInvalidos, ex);
                throw new FaultException(MensajesError.Cliente.DatosInvalidos);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.AmistadEliminarErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorEliminarAmistad);
            }
        }
    }
}