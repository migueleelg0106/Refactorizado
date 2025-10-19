using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using PictionaryMusicalCliente.Servicios;
using AmigosSrv = PictionaryMusicalCliente.PictionaryServidorServicioAmigos;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class AmigosService : IAmigosService, AmigosSrv.IAmigosManejadorCallback
    {
        private const string AmigosEndpoint = "NetTcpBinding_IAmigosManejador";

        private const string OperacionEliminar = "ELIMINAR";

        private readonly SynchronizationContext _synchronizationContext;
        private readonly ConcurrentDictionary<string, byte> _operacionesLocales;

        public AmigosService()
        {
            _synchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();
            _operacionesLocales = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        }

        public event EventHandler<SolicitudAmistadNotificacion> SolicitudAmistadNotificada;

        public event EventHandler<RespuestaSolicitudAmistadNotificacion> SolicitudAmistadRespondidaNotificada;

        public event EventHandler<AmistadEliminadaNotificacion> AmistadEliminadaNotificada;

        public async Task<ResultadoOperacion> EnviarSolicitudAmistadAsync(
            string nombreUsuarioRemitente,
            string nombreUsuarioReceptor)
        {
            ValidarNombre(nombreUsuarioRemitente, nameof(nombreUsuarioRemitente));
            ValidarNombre(nombreUsuarioReceptor, nameof(nombreUsuarioReceptor));

            var cliente = CrearCliente();

            try
            {
                AmigosSrv.ResultadoOperacionDTO resultado = await WcfClientHelper
                    .UsarAsync(cliente, c => c.EnviarSolicitudAmistadAsync(nombreUsuarioRemitente, nombreUsuarioReceptor))
                    .ConfigureAwait(false);

                return ConvertirResultado(
                    resultado,
                    Lang.amigosTextoSolicitudEnviada,
                    Lang.errorTextoErrorProcesarSolicitud);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorNoDisponible);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        public async Task<ResultadoOperacion> ResponderSolicitudAmistadAsync(
            string nombreUsuarioRemitente,
            string nombreUsuarioReceptor,
            bool aceptada)
        {
            ValidarNombre(nombreUsuarioRemitente, nameof(nombreUsuarioRemitente));
            ValidarNombre(nombreUsuarioReceptor, nameof(nombreUsuarioReceptor));

            var cliente = CrearCliente();

            try
            {
                AmigosSrv.ResultadoOperacionDTO resultado = await WcfClientHelper
                    .UsarAsync(
                        cliente,
                        c => c.ResponderSolicitudAmistadAsync(nombreUsuarioRemitente, nombreUsuarioReceptor, aceptada))
                    .ConfigureAwait(false);

                return ConvertirResultado(resultado, null, Lang.errorTextoErrorProcesarSolicitud);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorNoDisponible);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        public async Task<ResultadoOperacion> EliminarAmigoAsync(
            string nombreUsuarioRemitente,
            string nombreUsuarioReceptor)
        {
            ValidarNombre(nombreUsuarioRemitente, nameof(nombreUsuarioRemitente));
            ValidarNombre(nombreUsuarioReceptor, nameof(nombreUsuarioReceptor));

            string claveOperacion = CrearClaveOperacion(OperacionEliminar, nombreUsuarioRemitente, nombreUsuarioReceptor);
            if (!string.IsNullOrWhiteSpace(claveOperacion))
            {
                _operacionesLocales[claveOperacion] = 0;
            }

            var cliente = CrearCliente();

            try
            {
                AmigosSrv.ResultadoOperacionDTO resultado = await WcfClientHelper
                    .UsarAsync(cliente, c => c.EliminarAmigoAsync(nombreUsuarioRemitente, nombreUsuarioReceptor))
                    .ConfigureAwait(false);

                ResultadoOperacion conversion = ConvertirResultado(
                    resultado,
                    Lang.amigosTextoEliminacionExitosa,
                    Lang.errorTextoErrorProcesarSolicitud);

                if (conversion == null || !conversion.Exito)
                {
                    RemoverOperacionLocal(claveOperacion);
                }

                return conversion;
            }
            catch (FaultException ex)
            {
                RemoverOperacionLocal(claveOperacion);
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorNoDisponible);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                RemoverOperacionLocal(claveOperacion);
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                RemoverOperacionLocal(claveOperacion);
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                RemoverOperacionLocal(claveOperacion);
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                RemoverOperacionLocal(claveOperacion);
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        public void AmistadEliminada(AmigosSrv.AmistadEliminadaNotificacionDTO notificacion)
        {
            bool operacionLocal = false;

            if (notificacion != null)
            {
                string clave = CrearClaveOperacion(OperacionEliminar, notificacion.Jugador, notificacion.Amigo);
                if (!string.IsNullOrWhiteSpace(clave))
                {
                    operacionLocal = _operacionesLocales.TryRemove(clave, out _);
                }
            }

            AmistadEliminadaNotificacion conversion = ConvertirNotificacion(notificacion, operacionLocal);
            Publicar(AmistadEliminadaNotificada, conversion);
        }

        public void SolicitudAmistadRecibida(AmigosSrv.SolicitudAmistadNotificacionDTO notificacion)
        {
            SolicitudAmistadNotificacion conversion = ConvertirNotificacion(notificacion);
            Publicar(SolicitudAmistadNotificada, conversion);
        }

        public void SolicitudAmistadRespondida(AmigosSrv.RespuestaSolicitudAmistadNotificacionDTO notificacion)
        {
            RespuestaSolicitudAmistadNotificacion conversion = ConvertirNotificacion(notificacion);
            Publicar(SolicitudAmistadRespondidaNotificada, conversion);
        }

        private AmigosSrv.AmigosManejadorClient CrearCliente()
        {
            var contexto = new InstanceContext(this);
            return new AmigosSrv.AmigosManejadorClient(contexto, AmigosEndpoint);
        }

        private static void ValidarNombre(string valor, string nombreParametro)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new ArgumentException("El nombre de usuario es obligatorio.", nombreParametro);
            }
        }

        private static string CrearClaveOperacion(string tipo, string usuarioPrincipal, string usuarioSecundario)
        {
            if (string.IsNullOrWhiteSpace(tipo)
                || string.IsNullOrWhiteSpace(usuarioPrincipal)
                || string.IsNullOrWhiteSpace(usuarioSecundario))
            {
                return null;
            }

            return string.Concat(
                tipo.Trim().ToUpperInvariant(),
                ":",
                usuarioPrincipal.Trim().ToUpperInvariant(),
                "|",
                usuarioSecundario.Trim().ToUpperInvariant());
        }

        private void RemoverOperacionLocal(string claveOperacion)
        {
            if (string.IsNullOrWhiteSpace(claveOperacion))
            {
                return;
            }

            _operacionesLocales.TryRemove(claveOperacion, out _);
        }

        private void Publicar<TArgumento>(EventHandler<TArgumento> manejador, TArgumento argumento)
            where TArgumento : class
        {
            if (manejador == null || argumento == null)
            {
                return;
            }

            void Ejecutar(object state) => manejador(this, argumento);

            _synchronizationContext.Post(Ejecutar, null);
        }

        private static SolicitudAmistadNotificacion ConvertirNotificacion(AmigosSrv.SolicitudAmistadNotificacionDTO dto)
        {
            if (dto == null)
            {
                return null;
            }

            try
            {
                return new SolicitudAmistadNotificacion(dto.Remitente, dto.Receptor);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static RespuestaSolicitudAmistadNotificacion ConvertirNotificacion(
            AmigosSrv.RespuestaSolicitudAmistadNotificacionDTO dto)
        {
            if (dto == null)
            {
                return null;
            }

            try
            {
                return new RespuestaSolicitudAmistadNotificacion(dto.Remitente, dto.Receptor, dto.Aceptada);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static AmistadEliminadaNotificacion ConvertirNotificacion(
            AmigosSrv.AmistadEliminadaNotificacionDTO dto,
            bool operacionLocal)
        {
            if (dto == null)
            {
                return null;
            }

            try
            {
                return new AmistadEliminadaNotificacion(dto.Jugador, dto.Amigo, operacionLocal);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static ResultadoOperacion ConvertirResultado(
            AmigosSrv.ResultadoOperacionDTO resultado,
            string mensajeExito,
            string mensajeFallo)
        {
            if (resultado == null)
            {
                string mensaje = string.IsNullOrWhiteSpace(mensajeFallo)
                    ? Lang.errorTextoErrorProcesarSolicitud
                    : mensajeFallo;
                return ResultadoOperacion.Fallo(mensaje);
            }

            if (resultado.OperacionExitosa)
            {
                string mensaje = !string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? resultado.Mensaje
                    : mensajeExito;

                return string.IsNullOrWhiteSpace(mensaje)
                    ? ResultadoOperacion.Exitoso()
                    : ResultadoOperacion.Exitoso(mensaje);
            }

            string mensajeError = !string.IsNullOrWhiteSpace(resultado.Mensaje)
                ? resultado.Mensaje
                : mensajeFallo;

            if (string.IsNullOrWhiteSpace(mensajeError))
            {
                mensajeError = Lang.errorTextoErrorProcesarSolicitud;
            }

            return ResultadoOperacion.Fallo(mensajeError);
        }
    }
}
