using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    public class SolicitudesVistaModelo : BaseVistaModelo, IDisposable
    {
        private readonly IAmigosService _amigosService;
        private bool _estaProcesando;
        private bool _haySolicitudes;
        private SolicitudAmistadItemVistaModelo _solicitudSeleccionada;
        private bool _estaDispuesto;

        public SolicitudesVistaModelo(IAmigosService amigosService)
        {
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));

            SolicitudesPendientes = new ObservableCollection<SolicitudAmistadItemVistaModelo>();
            SolicitudesPendientes.CollectionChanged += SolicitudesPendientes_CollectionChanged;

            AceptarSolicitudCommand = new ComandoAsincrono(parametro => ResponderSolicitudAsync(parametro as SolicitudAmistadItemVistaModelo, true), PuedeResponderSolicitud);
            RechazarSolicitudCommand = new ComandoAsincrono(parametro => ResponderSolicitudAsync(parametro as SolicitudAmistadItemVistaModelo, false), PuedeResponderSolicitud);
            CerrarCommand = new ComandoDelegado(_ => CerrarAccion?.Invoke());

            _amigosService.SolicitudAmistadRecibida += AmigosService_SolicitudAmistadRecibida;
            _amigosService.SolicitudAmistadRespondida += AmigosService_SolicitudAmistadRespondida;
        }

        public ObservableCollection<SolicitudAmistadItemVistaModelo> SolicitudesPendientes { get; }

        public SolicitudAmistadItemVistaModelo SolicitudSeleccionada
        {
            get => _solicitudSeleccionada;
            set
            {
                if (EstablecerPropiedad(ref _solicitudSeleccionada, value))
                {
                    ActualizarEstadoComandos();
                }
            }
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ActualizarEstadoComandos();
                }
            }
        }

        public bool HaySolicitudes
        {
            get => _haySolicitudes;
            private set => EstablecerPropiedad(ref _haySolicitudes, value);
        }

        public IComandoAsincrono AceptarSolicitudCommand { get; }

        public IComandoAsincrono RechazarSolicitudCommand { get; }

        public ICommand CerrarCommand { get; }

        public Action CerrarAccion { get; set; }

        public Action<string> MostrarMensaje { get; set; }

        public void Dispose()
        {
            if (_estaDispuesto)
            {
                return;
            }

            _estaDispuesto = true;

            SolicitudesPendientes.CollectionChanged -= SolicitudesPendientes_CollectionChanged;
            _amigosService.SolicitudAmistadRecibida -= AmigosService_SolicitudAmistadRecibida;
            _amigosService.SolicitudAmistadRespondida -= AmigosService_SolicitudAmistadRespondida;
        }

        public void EstablecerSolicitudesIniciales(System.Collections.Generic.IEnumerable<string> solicitudes)
        {
            if (solicitudes == null)
            {
                return;
            }

            EjecutarEnDispatcher(() =>
            {
                SolicitudesPendientes.Clear();

                foreach (string solicitud in solicitudes)
                {
                    if (!string.IsNullOrWhiteSpace(solicitud))
                    {
                        SolicitudesPendientes.Add(new SolicitudAmistadItemVistaModelo(solicitud));
                    }
                }

                if (SolicitudesPendientes.Count > 0)
                {
                    SolicitudSeleccionada = SolicitudesPendientes.First();
                }
            });
        }

        private async Task ResponderSolicitudAsync(SolicitudAmistadItemVistaModelo solicitud, bool aceptada)
        {
            if (solicitud == null)
            {
                return;
            }

            string nombreRemitente = solicitud.NombreUsuario;
            if (string.IsNullOrWhiteSpace(nombreRemitente))
            {
                return;
            }

            EstaProcesando = true;
            solicitud.EstaProcesando = true;
            ActualizarEstadoComandos();

            try
            {
                ResultadoOperacion resultado = await _amigosService.ResponderSolicitudAsync(nombreRemitente, aceptada).ConfigureAwait(true);

                if (resultado?.Exito == true)
                {
                    EliminarSolicitud(nombreRemitente);

                    if (!string.IsNullOrWhiteSpace(resultado.Mensaje))
                    {
                        MostrarMensajeVista(resultado.Mensaje, usarPredeterminado: false);
                    }
                }
                else
                {
                    string mensaje = string.IsNullOrWhiteSpace(resultado?.Mensaje)
                        ? Lang.errorTextoErrorProcesarSolicitud
                        : resultado.Mensaje;
                    MostrarMensajeVista(mensaje);
                }
            }
            catch (ServicioException ex)
            {
                string mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? Lang.errorTextoErrorProcesarSolicitud
                    : ex.Message;
                MostrarMensajeVista(mensaje);
            }
            catch (Exception)
            {
                MostrarMensajeVista(Lang.errorTextoErrorProcesarSolicitud);
            }
            finally
            {
                solicitud.EstaProcesando = false;
                EstaProcesando = false;
                ActualizarEstadoComandos();
            }
        }

        private bool PuedeResponderSolicitud(object parametro)
        {
            if (EstaProcesando || parametro is not SolicitudAmistadItemVistaModelo solicitud)
            {
                return false;
            }

            return !solicitud.EstaProcesando;
        }

        private void SolicitudesPendientes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HaySolicitudes = SolicitudesPendientes.Any();
            if (!HaySolicitudes)
            {
                SolicitudSeleccionada = null;
            }
        }

        private void AmigosService_SolicitudAmistadRecibida(object sender, SolicitudAmistadNotificacion e)
        {
            if (e == null)
            {
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                return;
            }

            if (!string.Equals(usuarioActual, e.Receptor, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            EjecutarEnDispatcher(() =>
            {
                if (SolicitudesPendientes.Any(s => string.Equals(s.NombreUsuario, e.Remitente, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                var solicitud = new SolicitudAmistadItemVistaModelo(e.Remitente);
                SolicitudesPendientes.Add(solicitud);

                if (SolicitudSeleccionada == null)
                {
                    SolicitudSeleccionada = solicitud;
                }
            });
        }

        private void AmigosService_SolicitudAmistadRespondida(object sender, RespuestaSolicitudAmistadNotificacion e)
        {
            if (e == null)
            {
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                return;
            }

            if (!string.Equals(usuarioActual, e.Receptor, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            EliminarSolicitud(e.Remitente);
        }

        private void EliminarSolicitud(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            EjecutarEnDispatcher(() =>
            {
                SolicitudAmistadItemVistaModelo solicitud = SolicitudesPendientes
                    .FirstOrDefault(s => string.Equals(s.NombreUsuario, nombreUsuario, StringComparison.OrdinalIgnoreCase));

                if (solicitud != null)
                {
                    SolicitudesPendientes.Remove(solicitud);

                    if (Equals(SolicitudSeleccionada, solicitud))
                    {
                        SolicitudSeleccionada = SolicitudesPendientes.FirstOrDefault();
                    }
                }
            });
        }

        private void EjecutarEnDispatcher(Action accion)
        {
            if (accion == null)
            {
                return;
            }

            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(accion);
            }
            else
            {
                accion();
            }
        }

        private void ActualizarEstadoComandos()
        {
            ((IComandoNotificable)AceptarSolicitudCommand).NotificarPuedeEjecutar();
            ((IComandoNotificable)RechazarSolicitudCommand).NotificarPuedeEjecutar();
        }

        private void MostrarMensajeVista(string mensaje, bool usarPredeterminado = true)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                if (!usarPredeterminado)
                {
                    return;
                }

                mensaje = Lang.errorTextoErrorProcesarSolicitud;
            }

            (MostrarMensaje ?? AvisoHelper.Mostrar).Invoke(mensaje);
        }

        public class SolicitudAmistadItemVistaModelo : BaseVistaModelo
        {
            private bool _estaProcesando;

            public SolicitudAmistadItemVistaModelo(string nombreUsuario)
            {
                NombreUsuario = nombreUsuario;
            }

            public string NombreUsuario { get; }

            public bool EstaProcesando
            {
                get => _estaProcesando;
                set => EstablecerPropiedad(ref _estaProcesando, value);
            }
        }
    }
}
