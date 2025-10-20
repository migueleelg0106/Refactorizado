using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Sesiones;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class SolicitudesVistaModelo : BaseVistaModelo
    {
        private readonly IAmigosService _amigosService;
        private SolicitudAmistadItemVistaModelo _solicitudSeleccionada;

        public SolicitudesVistaModelo(IAmigosService amigosService)
        {
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));

            Solicitudes = new ObservableCollection<SolicitudAmistadItemVistaModelo>();

            AceptarSolicitudCommand = new ComandoAsincrono(
                parametro => ResponderSolicitudAsync(parametro as SolicitudAmistadItemVistaModelo, true),
                parametro => PuedeResponder(parametro as SolicitudAmistadItemVistaModelo));

            RechazarSolicitudCommand = new ComandoAsincrono(
                parametro => ResponderSolicitudAsync(parametro as SolicitudAmistadItemVistaModelo, false),
                parametro => PuedeResponder(parametro as SolicitudAmistadItemVistaModelo));

            AceptarSolicitudSeleccionadaCommand = new ComandoAsincrono(
                _ => ResponderSolicitudAsync(SolicitudSeleccionada, true),
                _ => PuedeResponder(SolicitudSeleccionada));

            RechazarSolicitudSeleccionadaCommand = new ComandoAsincrono(
                _ => ResponderSolicitudAsync(SolicitudSeleccionada, false),
                _ => PuedeResponder(SolicitudSeleccionada));

            CancelarCommand = new ComandoDelegado(_ => CerrarAccion?.Invoke());
        }

        public ObservableCollection<SolicitudAmistadItemVistaModelo> Solicitudes
        {
            get => _solicitudes;
            private set
            {
                if (_solicitudes != null)
                {
                    _solicitudes.CollectionChanged -= SolicitudesCollectionChanged;
                    foreach (SolicitudAmistadItemVistaModelo solicitud in _solicitudes)
                    {
                        solicitud.PropertyChanged -= SolicitudPropertyChanged;
                    }
                }

                if (EstablecerPropiedad(ref _solicitudes, value))
                {
                    if (_solicitudes != null)
                    {
                        _solicitudes.CollectionChanged += SolicitudesCollectionChanged;
                        foreach (SolicitudAmistadItemVistaModelo solicitud in _solicitudes)
                        {
                            solicitud.PropertyChanged += SolicitudPropertyChanged;
                        }
                    }
                }
            }
        }

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

        public IComandoAsincrono AceptarSolicitudCommand { get; }

        public IComandoAsincrono RechazarSolicitudCommand { get; }

        public IComandoAsincrono AceptarSolicitudSeleccionadaCommand { get; }

        public IComandoAsincrono RechazarSolicitudSeleccionadaCommand { get; }

        public ICommand CancelarCommand { get; }

        public Action CerrarAccion { get; set; }

        public Action<string> MostrarMensaje { get; set; }

        public Action<string> SolicitudAceptada { get; set; }

        public void ActualizarSolicitudes(IEnumerable<string> solicitudes)
        {
            IEnumerable<string> nombresValidos = solicitudes?
                .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
                .Select(nombre => nombre.Trim())
                .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(nombre => nombre, StringComparer.CurrentCultureIgnoreCase)
                ?? Enumerable.Empty<string>();

            var nuevasSolicitudes = new ObservableCollection<SolicitudAmistadItemVistaModelo>(
                nombresValidos.Select(nombre => new SolicitudAmistadItemVistaModelo(nombre)));

            Solicitudes = nuevasSolicitudes;
            SolicitudSeleccionada = null;
            ActualizarEstadoComandos();
        }

        public void AgregarSolicitud(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            string nombreNormalizado = nombreUsuario.Trim();
            if (Solicitudes.Any(s => string.Equals(s.NombreUsuario, nombreNormalizado, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            var solicitud = new SolicitudAmistadItemVistaModelo(nombreNormalizado);
            Solicitudes.Add(solicitud);
            ActualizarEstadoComandos();
        }

        public void EliminarSolicitud(string nombreUsuario)
        {
            if (Solicitudes == null || string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            SolicitudAmistadItemVistaModelo existente = Solicitudes
                .FirstOrDefault(s => string.Equals(s.NombreUsuario, nombreUsuario, StringComparison.OrdinalIgnoreCase));

            if (existente == null)
            {
                return;
            }

            Solicitudes.Remove(existente);

            if (ReferenceEquals(SolicitudSeleccionada, existente))
            {
                SolicitudSeleccionada = null;
            }

            ActualizarEstadoComandos();
        }

        private async Task ResponderSolicitudAsync(SolicitudAmistadItemVistaModelo solicitud, bool aceptada)
        {
            if (solicitud == null)
            {
                MostrarMensaje?.Invoke(Lang.errorTextoCamposInvalidosGenerico);
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;

            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            solicitud.EstaProcesando = true;
            ActualizarEstadoComandos();

            try
            {
                ResultadoOperacion resultado = await _amigosService
                    .ResponderSolicitudAmistadAsync(
                        solicitud.NombreUsuario,
                        usuarioActual,
                        aceptada)
                    .ConfigureAwait(true);

                if (resultado == null)
                {
                    MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                    return;
                }

                string mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? (resultado.Exito
                        ? (aceptada
                            ? Lang.solicitudesTextoSolicitudAceptada
                            : Lang.solicitudesTextoSolicitudRechazada)
                        : Lang.solicitudesErrorProcesarRespuesta)
                    : resultado.Mensaje;

                MostrarMensaje?.Invoke(mensaje);

                if (resultado.Exito)
                {
                    if (aceptada)
                    {
                        SolicitudAceptada?.Invoke(solicitud.NombreUsuario);
                    }

                    Solicitudes.Remove(solicitud);

                    if (ReferenceEquals(SolicitudSeleccionada, solicitud))
                    {
                        SolicitudSeleccionada = null;
                    }
                }
            }
            catch (ServicioException ex)
            {
                MostrarMensaje?.Invoke(ex.Message ?? Lang.solicitudesErrorProcesarRespuesta);
            }
            catch (Exception)
            {
                MostrarMensaje?.Invoke(Lang.solicitudesErrorProcesarRespuesta);
            }
            finally
            {
                solicitud.EstaProcesando = false;
                ActualizarEstadoComandos();
            }
        }

        private bool PuedeResponder(SolicitudAmistadItemVistaModelo solicitud)
        {
            return solicitud != null && !solicitud.EstaProcesando;
        }

        private void SolicitudesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e?.OldItems != null)
            {
                foreach (SolicitudAmistadItemVistaModelo solicitud in e.OldItems.OfType<SolicitudAmistadItemVistaModelo>())
                {
                    solicitud.PropertyChanged -= SolicitudPropertyChanged;
                }
            }

            if (e?.NewItems != null)
            {
                foreach (SolicitudAmistadItemVistaModelo solicitud in e.NewItems.OfType<SolicitudAmistadItemVistaModelo>())
                {
                    solicitud.PropertyChanged += SolicitudPropertyChanged;
                }
            }

            ActualizarEstadoComandos();
        }

        private void SolicitudPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(SolicitudAmistadItemVistaModelo.EstaProcesando), StringComparison.Ordinal))
            {
                ActualizarEstadoComandos();
            }
        }

        private void ActualizarEstadoComandos()
        {
            AceptarSolicitudCommand?.NotificarPuedeEjecutar();
            RechazarSolicitudCommand?.NotificarPuedeEjecutar();
            AceptarSolicitudSeleccionadaCommand?.NotificarPuedeEjecutar();
            RechazarSolicitudSeleccionadaCommand?.NotificarPuedeEjecutar();
        }

        private ObservableCollection<SolicitudAmistadItemVistaModelo> _solicitudes;
    }
}
