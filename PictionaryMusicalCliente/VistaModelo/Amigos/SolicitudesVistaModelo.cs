using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    /// <summary>
    /// Gestiona la visualizacion y respuesta a las solicitudes de amistad pendientes.
    /// </summary>
    public sealed class SolicitudesVistaModelo : BaseVistaModelo, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAmigosServicio _amigosServicio;
        private readonly string _usuarioActual;
        private bool _estaProcesando;

        /// <summary>
        /// Inicializa el ViewModel y se suscribe a eventos de actualizacion.
        /// </summary>
        /// <param name="amigosServicio">Servicio para operaciones de amistad.</param>
        public SolicitudesVistaModelo(IAmigosServicio amigosServicio)
        {
            _amigosServicio = amigosServicio ??
                throw new ArgumentNullException(nameof(amigosServicio));
            _usuarioActual = SesionUsuarioActual.Usuario?.NombreUsuario ?? string.Empty;

            Solicitudes = new ObservableCollection<SolicitudAmistadEntrada>();

            AceptarSolicitudComando = new ComandoAsincrono(async param =>
            {
                SonidoManejador.ReproducirClick();
                await ResponderSolicitudAsync(param as SolicitudAmistadEntrada);
            }, param => PuedeAceptar(param as SolicitudAmistadEntrada));

            RechazarSolicitudComando = new ComandoAsincrono(async param =>
            {
                SonidoManejador.ReproducirClick();
                await RechazarSolicitudAsync(param as SolicitudAmistadEntrada);
            }, param => PuedeRechazar(param as SolicitudAmistadEntrada));

            CerrarComando = new ComandoDelegado(_ =>
            {
                SonidoManejador.ReproducirClick();
                Cerrar?.Invoke();
            });

            _amigosServicio.SolicitudesActualizadas += SolicitudesActualizadas;
            ActualizarSolicitudes(_amigosServicio.SolicitudesPendientes);
        }

        /// <summary>
        /// Coleccion observable de solicitudes para enlazar a la vista.
        /// </summary>
        public ObservableCollection<SolicitudAmistadEntrada> Solicitudes { get; }

        /// <summary>
        /// Comando para aceptar una solicitud de amistad.
        /// </summary>
        public IComandoAsincrono AceptarSolicitudComando { get; }

        /// <summary>
        /// Comando para rechazar o cancelar una solicitud de amistad.
        /// </summary>
        public IComandoAsincrono RechazarSolicitudComando { get; }

        /// <summary>
        /// Comando para cerrar la ventana.
        /// </summary>
        public ICommand CerrarComando { get; }

        /// <summary>
        /// Accion delegada para cerrar la ventana desde la vista.
        /// </summary>
        public Action Cerrar { get; set; }

        /// <inheritdoc />
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

        private void SolicitudesActualizadas(
            object sender,
            IReadOnlyCollection<DTOs.SolicitudAmistadDTO> solicitudes)
        {
            EjecutarEnDispatcher(() => ActualizarSolicitudes(solicitudes));
        }

        private void ActualizarSolicitudes(
            IReadOnlyCollection<DTOs.SolicitudAmistadDTO> solicitudes)
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

                bool esEmisorActual = string.Equals(
                    solicitud.UsuarioEmisor,
                    _usuarioActual,
                    StringComparison.OrdinalIgnoreCase);

                bool esReceptorActual = string.Equals(
                    solicitud.UsuarioReceptor,
                    _usuarioActual,
                    StringComparison.OrdinalIgnoreCase);

                if (!esEmisorActual && !esReceptorActual)
                {
                    continue;
                }

                string nombreMostrado = esEmisorActual
                    ? solicitud.UsuarioReceptor
                    : solicitud.UsuarioEmisor;

                nombreMostrado = nombreMostrado?.Trim();

                if (string.IsNullOrWhiteSpace(nombreMostrado))
                {
                    continue;
                }

                bool puedeAceptar = esReceptorActual;

                Solicitudes.Add(new SolicitudAmistadEntrada(
                    solicitud,
                    nombreMostrado,
                    puedeAceptar));
            }
        }

        private async Task ResponderSolicitudAsync(SolicitudAmistadEntrada entrada)
        {
            if (entrada == null)
            {
                Log.Warn("Intento de responder solicitud con entrada nula.");
                return;
            }

            EstaProcesando = true;

            try
            {
                Log.InfoFormat("Aceptando solicitud de amistad de: {0}",
                    entrada.Solicitud.UsuarioEmisor);
                await _amigosServicio.ResponderSolicitudAsync(
                    entrada.Solicitud.UsuarioEmisor,
                    entrada.Solicitud.UsuarioReceptor).ConfigureAwait(true);

                SonidoManejador.ReproducirExito();
                AvisoAyudante.Mostrar(Lang.amigosTextoSolicitudAceptada);
            }
            catch (ServicioExcepcion ex)
            {
                Log.Error("Error al aceptar solicitud de amistad.", ex);
                SonidoManejador.ReproducirError();
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
                Log.Warn("Intento de rechazar solicitud con entrada nula.");
                return;
            }

            EstaProcesando = true;

            try
            {
                Log.InfoFormat("Rechazando/Cancelando solicitud con: {0}",
                    entrada.NombreUsuario);
                await _amigosServicio.EliminarAmigoAsync(
                    entrada.Solicitud.UsuarioEmisor,
                    entrada.Solicitud.UsuarioReceptor).ConfigureAwait(true);

                SonidoManejador.ReproducirExito();
                AvisoAyudante.Mostrar(Lang.amigosTextoSolicitudCancelada);
            }
            catch (ServicioExcepcion ex)
            {
                Log.Error("Error al rechazar/cancelar solicitud de amistad.", ex);
                SonidoManejador.ReproducirError();
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
    }
}