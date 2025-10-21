using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    public class SolicitudesVistaModelo : BaseVistaModelo
    {
        private readonly IAmigosService _amigosService;

        public SolicitudesVistaModelo(IAmigosService amigosService)
        {
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));

            Solicitudes = new ObservableCollection<SolicitudAmistadItem>();

            AceptarSolicitudCommand = new ComandoAsincrono(
                parametro => ProcesarSolicitudAsync(parametro as SolicitudAmistadItem, true),
                parametro => parametro is SolicitudAmistadItem);

            RechazarSolicitudCommand = new ComandoAsincrono(
                parametro => ProcesarSolicitudAsync(parametro as SolicitudAmistadItem, false),
                parametro => parametro is SolicitudAmistadItem);

            _amigosService.SolicitudRecibida += AmigosService_SolicitudRecibida;
            _amigosService.SolicitudRespondida += AmigosService_SolicitudRespondida;
        }

        public ObservableCollection<SolicitudAmistadItem> Solicitudes { get; }

        public IComandoAsincrono AceptarSolicitudCommand { get; }

        public IComandoAsincrono RechazarSolicitudCommand { get; }

        public Action CerrarAccion { get; set; }

        public Action<string> MostrarMensaje { get; set; }

        public void CargarSolicitudes()
        {
            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                return;
            }

            var pendientes = _amigosService.ObtenerSolicitudesPendientes();

            EjecutarEnDispatcher(() =>
            {
                Solicitudes.Clear();

                if (pendientes == null)
                {
                    return;
                }

                foreach (SolicitudAmistad solicitud in pendientes)
                {
                    if (string.Equals(solicitud.UsuarioReceptor, usuarioActual, StringComparison.OrdinalIgnoreCase))
                    {
                        Solicitudes.Add(new SolicitudAmistadItem(solicitud.UsuarioEmisor, solicitud.UsuarioReceptor, usuarioActual));
                    }
                }
            });
        }

        public void Liberar()
        {
            _amigosService.SolicitudRecibida -= AmigosService_SolicitudRecibida;
            _amigosService.SolicitudRespondida -= AmigosService_SolicitudRespondida;
        }

        private async Task ProcesarSolicitudAsync(SolicitudAmistadItem solicitud, bool aceptar)
        {
            if (solicitud == null)
            {
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                MostrarMensaje?.Invoke(Lang.errorTextoAmigosOperacion);
                return;
            }

            try
            {
                ResultadoOperacion resultado = await _amigosService
                    .ResponderSolicitudAmistadAsync(solicitud.UsuarioEmisor, solicitud.UsuarioReceptor, aceptar)
                    .ConfigureAwait(false);

                if (resultado == null)
                {
                    MostrarMensaje?.Invoke(Lang.errorTextoAmigosOperacion);
                    return;
                }

                string mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? Lang.errorTextoAmigosOperacion
                    : resultado.Mensaje;

                MostrarMensaje?.Invoke(mensaje);

                if (resultado.Exito)
                {
                    EjecutarEnDispatcher(() => Solicitudes.Remove(solicitud));
                }
            }
            catch (ServicioException ex)
            {
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoAmigosOperacion);
            }
        }

        private void AmigosService_SolicitudRecibida(object sender, SolicitudAmistadEventArgs e)
        {
            if (e?.Solicitud == null)
            {
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                return;
            }

            if (!string.Equals(e.Solicitud.UsuarioReceptor, usuarioActual, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var item = new SolicitudAmistadItem(e.Solicitud.UsuarioEmisor, e.Solicitud.UsuarioReceptor, usuarioActual);

            EjecutarEnDispatcher(() =>
            {
                if (!Solicitudes.Any(s => s.CoincideCon(item)))
                {
                    Solicitudes.Add(item);
                }
            });
        }

        private void AmigosService_SolicitudRespondida(object sender, RespuestaSolicitudAmistadEventArgs e)
        {
            if (e?.Respuesta == null)
            {
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                return;
            }

            if (!e.Respuesta.InvolucraUsuario(usuarioActual))
            {
                return;
            }

            EjecutarEnDispatcher(() =>
            {
                SolicitudAmistadItem existente = Solicitudes.FirstOrDefault(s => s.CoincideCon(e.Respuesta.UsuarioEmisor, e.Respuesta.UsuarioReceptor));
                if (existente != null)
                {
                    Solicitudes.Remove(existente);
                }
            });
        }

        private static void EjecutarEnDispatcher(Action accion)
        {
            if (accion == null)
            {
                return;
            }

            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(accion);
                return;
            }

            accion();
        }

        public sealed class SolicitudAmistadItem
        {
            public SolicitudAmistadItem(string usuarioEmisor, string usuarioReceptor, string usuarioActual)
            {
                UsuarioEmisor = usuarioEmisor ?? throw new ArgumentNullException(nameof(usuarioEmisor));
                UsuarioReceptor = usuarioReceptor ?? throw new ArgumentNullException(nameof(usuarioReceptor));
                NombreUsuario = string.Equals(usuarioActual, usuarioEmisor, StringComparison.OrdinalIgnoreCase)
                    ? usuarioReceptor
                    : usuarioEmisor;
            }

            public string UsuarioEmisor { get; }

            public string UsuarioReceptor { get; }

            public string NombreUsuario { get; }

            public bool CoincideCon(SolicitudAmistadItem otra)
            {
                return otra != null && CoincideCon(otra.UsuarioEmisor, otra.UsuarioReceptor);
            }

            public bool CoincideCon(string usuarioEmisor, string usuarioReceptor)
            {
                return string.Equals(UsuarioEmisor, usuarioEmisor, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(UsuarioReceptor, usuarioReceptor, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
