using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    public class SolicitudesVistaModelo : BaseVistaModelo, IDisposable
    {
        private readonly IAmigosServicio _amigosServicio;
        private readonly string _usuarioActual;
        private bool _estaProcesando;

        public SolicitudesVistaModelo(IAmigosServicio amigosServicio)
        {
            _amigosServicio = amigosServicio ?? throw new ArgumentNullException(nameof(amigosServicio));
            _usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario ?? string.Empty;

            Solicitudes = new ObservableCollection<SolicitudAmistadEntrada>();

            AceptarSolicitudComando = new ComandoAsincrono(param => ResponderSolicitudAsync(param as SolicitudAmistadEntrada),
                param => PuedeAceptar(param as SolicitudAmistadEntrada));
            RechazarSolicitudComando = new ComandoAsincrono(param => RechazarSolicitudAsync(param as SolicitudAmistadEntrada),
                param => PuedeRechazar(param as SolicitudAmistadEntrada));
            CerrarComando = new ComandoDelegado(_ => Cerrar?.Invoke());

            _amigosServicio.SolicitudesActualizadas += SolicitudesActualizadas;
            ActualizarSolicitudes(_amigosServicio.SolicitudesPendientes);
        }

        public ObservableCollection<SolicitudAmistadEntrada> Solicitudes { get; }

        public IComandoAsincrono AceptarSolicitudComando { get; }

        public IComandoAsincrono RechazarSolicitudComando { get; }

        public ICommand CerrarComando { get; }

        public Action Cerrar { get; set; }

        public void Dispose()
        {
            _amigosServicio.SolicitudesActualizadas -= SolicitudesActualizadas;
        }

        private bool PuedeAceptar(SolicitudAmistadEntrada entrada)
        {
            return !EstaProcesando
                && entrada != null
                && entrada.PuedeAceptar;
        }

        private bool PuedeRechazar(SolicitudAmistadEntrada entrada)
        {
            return !EstaProcesando
                && entrada != null;
        }

        private bool EstaProcesando
        {
            get => _estaProcesando;
            set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    AceptarSolicitudComando?.NotificarPuedeEjecutar();
                    RechazarSolicitudComando?.NotificarPuedeEjecutar();
                }
            }
        }

        private void SolicitudesActualizadas(object sender, IReadOnlyCollection<DTOs.SolicitudAmistadDTO> solicitudes)
        {
            EjecutarEnDispatcher(() => ActualizarSolicitudes(solicitudes));
        }

        private void ActualizarSolicitudes(IReadOnlyCollection<DTOs.SolicitudAmistadDTO> solicitudes)
        {
            if (Solicitudes == null)
            {
                return;
            }

            Solicitudes.Clear();

            if (solicitudes == null)
            {
                return;
            }

            foreach (var solicitud in solicitudes)
            {
                if (solicitud == null || solicitud.SolicitudAceptada)
                {
                    continue;
                }

                bool esEmisorActual = string.Equals(solicitud.UsuarioEmisor, _usuarioActual, StringComparison.OrdinalIgnoreCase);
                bool esReceptorActual = string.Equals(solicitud.UsuarioReceptor, _usuarioActual, StringComparison.OrdinalIgnoreCase);

                if (!esEmisorActual && !esReceptorActual)
                {
                    continue;
                }

                string nombreMostrado = esEmisorActual ? solicitud.UsuarioReceptor : solicitud.UsuarioEmisor;
                nombreMostrado = nombreMostrado?.Trim();

                if (string.IsNullOrWhiteSpace(nombreMostrado))
                {
                    continue;
                }

                bool puedeAceptar = esReceptorActual;

                Solicitudes.Add(new SolicitudAmistadEntrada(solicitud, nombreMostrado, puedeAceptar));
            }
        }

        private async Task ResponderSolicitudAsync(SolicitudAmistadEntrada entrada)
        {
            if (entrada == null)
            {
                return;
            }

            EstaProcesando = true;

            try
            {
                await _amigosServicio
                    .ResponderSolicitudAsync(entrada.Solicitud.UsuarioEmisor, entrada.Solicitud.UsuarioReceptor)
                    .ConfigureAwait(true);

                AvisoAyudante.Mostrar(Lang.amigosTextoSolicitudAceptada);
            }
            catch (ExcepcionServicio ex)
            {
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private async Task RechazarSolicitudAsync(SolicitudAmistadEntrada entrada)
        {
            if (entrada == null)
            {
                return;
            }

            EstaProcesando = true;

            try
            {
                await _amigosServicio
                    .EliminarAmigoAsync(entrada.Solicitud.UsuarioEmisor, entrada.Solicitud.UsuarioReceptor)
                    .ConfigureAwait(true);

                AvisoAyudante.Mostrar(Lang.amigosTextoSolicitudCancelada);
            }
            catch (ExcepcionServicio ex)
            {
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private static void EjecutarEnDispatcher(Action accion)
        {
            if (accion == null)
            {
                return;
            }

            Application application = Application.Current;

            if (application?.Dispatcher == null || application.Dispatcher.CheckAccess())
            {
                accion();
            }
            else
            {
                application.Dispatcher.BeginInvoke(accion);
            }
        }

        public class SolicitudAmistadEntrada
        {
            public SolicitudAmistadEntrada(DTOs.SolicitudAmistadDTO solicitud, string nombreUsuario, bool puedeAceptar)
            {
                Solicitud = solicitud ?? throw new ArgumentNullException(nameof(solicitud));
                NombreUsuario = nombreUsuario;
                PuedeAceptar = puedeAceptar;
            }

            public DTOs.SolicitudAmistadDTO Solicitud { get; }

            public string NombreUsuario { get; }

            public bool PuedeAceptar { get; }
        }
    }
}
