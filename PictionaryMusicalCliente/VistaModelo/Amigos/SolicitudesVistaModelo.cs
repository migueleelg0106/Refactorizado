using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    public class SolicitudesVistaModelo : BaseVistaModelo, IDisposable
    {
        private readonly IAmigosService _amigosService;
        private readonly string _usuarioActual;
        private bool _estaProcesando;

        public SolicitudesVistaModelo(IAmigosService amigosService)
        {
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));
            _usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario ?? string.Empty;

            Solicitudes = new ObservableCollection<SolicitudAmistadEntrada>();

            AceptarSolicitudCommand = new ComandoAsincrono(param => ResponderSolicitudAsync(param as SolicitudAmistadEntrada),
                param => PuedeAceptar(param as SolicitudAmistadEntrada));
            RechazarSolicitudCommand = new ComandoAsincrono(param => RechazarSolicitudAsync(param as SolicitudAmistadEntrada),
                param => PuedeRechazar(param as SolicitudAmistadEntrada));
            CerrarCommand = new ComandoDelegado(_ => Cerrar?.Invoke());

            _amigosService.SolicitudesActualizadas += AmigosService_SolicitudesActualizadas;
            ActualizarSolicitudes(_amigosService.SolicitudesPendientes);
        }

        public ObservableCollection<SolicitudAmistadEntrada> Solicitudes { get; }

        public IComandoAsincrono AceptarSolicitudCommand { get; }

        public IComandoAsincrono RechazarSolicitudCommand { get; }

        public ICommand CerrarCommand { get; }

        public Action Cerrar { get; set; }

        public void Dispose()
        {
            _amigosService.SolicitudesActualizadas -= AmigosService_SolicitudesActualizadas;
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
                    AceptarSolicitudCommand?.NotificarPuedeEjecutar();
                    RechazarSolicitudCommand?.NotificarPuedeEjecutar();
                }
            }
        }

        private void AmigosService_SolicitudesActualizadas(object sender, IReadOnlyCollection<SolicitudAmistad> solicitudes)
        {
            EjecutarEnDispatcher(() => ActualizarSolicitudes(solicitudes));
        }

        private void ActualizarSolicitudes(IReadOnlyCollection<SolicitudAmistad> solicitudes)
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
                if (solicitud == null || !solicitud.EstaPendiente)
                {
                    continue;
                }

                string nombreMostrado = solicitud.ObtenerOtroUsuario(_usuarioActual);
                bool puedeAceptar = solicitud.UsuarioEsReceptor(_usuarioActual);

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
                await _amigosService
                    .ResponderSolicitudAsync(entrada.Solicitud.UsuarioEmisor, entrada.Solicitud.UsuarioReceptor)
                    .ConfigureAwait(true);

                AvisoHelper.Mostrar(Lang.amigosTextoSolicitudAceptada);
            }
            catch (ServicioException ex)
            {
                AvisoHelper.Mostrar(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
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
                await _amigosService
                    .EliminarAmigoAsync(entrada.Solicitud.UsuarioEmisor, entrada.Solicitud.UsuarioReceptor)
                    .ConfigureAwait(true);

                AvisoHelper.Mostrar(Lang.amigosTextoSolicitudCancelada);
            }
            catch (ServicioException ex)
            {
                AvisoHelper.Mostrar(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
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
            public SolicitudAmistadEntrada(SolicitudAmistad solicitud, string nombreUsuario, bool puedeAceptar)
            {
                Solicitud = solicitud ?? throw new ArgumentNullException(nameof(solicitud));
                NombreUsuario = nombreUsuario;
                PuedeAceptar = puedeAceptar;
            }

            public SolicitudAmistad Solicitud { get; }

            public string NombreUsuario { get; }

            public bool PuedeAceptar { get; }
        }
    }
}
