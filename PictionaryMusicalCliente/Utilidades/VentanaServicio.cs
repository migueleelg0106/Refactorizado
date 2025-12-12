using System;
using System.Linq;
using System.Windows;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using PictionaryMusicalCliente.Vista;
using PictionaryMusicalCliente.VistaModelo.InicioSesion;
using PictionaryMusicalCliente.VistaModelo.Ajustes;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using PictionaryMusicalCliente.VistaModelo.Salas;
using PictionaryMusicalCliente.VistaModelo.Sesion;
using PictionaryMusicalCliente.VistaModelo.VentanaPrincipal;

namespace PictionaryMusicalCliente.Utilidades
{
    /// <summary>
    /// Implementacion concreta del servicio de ventanas para WPF.
    /// Utiliza la ventana personalizada 'Avisos' para notificaciones.
    /// </summary>
    public class VentanaServicio : IVentanaServicio
    {
        /// <summary>
        /// Muestra una ventana no modal asociada a un ViewModel especifico.
        /// </summary>
        /// <param name="vistaModelo">El ViewModel que define el contenido y logica de la ventana.
        /// </param>
        public void MostrarVentana(object vistaModelo)
        {
            var ventana = CrearVentana(vistaModelo);
            ventana.Show();
        }

        /// <summary>
        /// Muestra una ventana en modo dialogo (bloqueante) asociada a un ViewModel.
        /// </summary>
        /// <param name="vistaModelo">El ViewModel que define el contenido de la ventana.</param>
        /// <returns>El resultado del dialogo (true, false o null) al cerrarse.</returns>
        public bool? MostrarVentanaDialogo(object vistaModelo)
        {
            var ventana = CrearVentana(vistaModelo);
            return ventana.ShowDialog();
        }

        /// <summary>
        /// Cierra la ventana activa que este vinculada al ViewModel proporcionado.
        /// </summary>
        /// <param name="vistaModelo">El ViewModel cuya ventana asociada se debe cerrar.</param>
        public void CerrarVentana(object vistaModelo)
        {
            var ventana = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(v => v.DataContext == vistaModelo);

            ventana?.Close();
        }

        /// <summary>
        /// Muestra un mensaje emergente informativo al usuario utilizando la ventana de Avisos.
        /// </summary>
        /// <param name="titulo">El titulo del mensaje (se usa para contexto si es necesario).
        /// </param>
        /// <param name="mensaje">El contenido textual del mensaje a mostrar.</param>
        public void MostrarMensaje(string titulo, string mensaje)
        {
            var aviso = new Avisos(mensaje);
            ConfigurarPropietario(aviso);
            aviso.ShowDialog();
        }

        /// <summary>
        /// Muestra un mensaje de error critico o de validacion al usuario.
        /// </summary>
        /// <param name="mensaje">El texto descriptivo del error.</param>
        public void MostrarError(string mensaje)
        {
            var aviso = new Avisos(mensaje);
            ConfigurarPropietario(aviso);
            aviso.ShowDialog();
        }

        private void ConfigurarPropietario(Window ventanaHija)
        {
            if (Application.Current != null &&
                Application.Current.MainWindow != null &&
                Application.Current.MainWindow.IsVisible)
            {
                ventanaHija.Owner = Application.Current.MainWindow;
            }
        }

        private Window CrearVentana(object vistaModelo)
        {
            if (vistaModelo == null) throw new ArgumentNullException(nameof(vistaModelo));

            Window ventana = null;

            switch (vistaModelo)
            {
                case InicioSesionVistaModelo:
                    ventana = new InicioSesion();
                    break;
                case CreacionCuentaVistaModelo:
                    ventana = new CreacionCuenta();
                    break;
                case AjustesPartidaVistaModelo:
                    ventana = new AjustesPartida();
                    break;
                case AjustesVistaModelo:
                    ventana = new Ajustes();
                    break;
                case BusquedaAmigoVistaModelo:
                    ventana = new BusquedaAmigo();
                    break;
                case EliminacionAmigoVistaModelo:
                    ventana = new EliminacionAmigo();
                    break;
                case InvitarAmigosVistaModelo:
                    ventana = new InvitarAmigos();
                    break;
                case SolicitudesVistaModelo:
                    ventana = new Solicitudes();
                    break;
                case CambioContrasenaVistaModelo:
                    ventana = new CambioContrasena();
                    break;
                case PerfilVistaModelo:
                    ventana = new Perfil();
                    break;
                case SeleccionAvatarVistaModelo:
                    ventana = new SeleccionAvatar();
                    break;
                case VerificacionCodigoVistaModelo:
                    ventana = new VerificacionCodigo();
                    break;
                case ExpulsionJugadorVistaModelo:
                    ventana = new ExpulsionJugador();
                    break;
                case IngresoPartidaInvitadoVistaModelo:
                    ventana = new IngresoPartidaInvitado();
                    break;
                case ReportarJugadorVistaModelo:
                    ventana = new ReportarJugador();
                    break;
                case SalaVistaModelo:
                    ventana = new Sala();
                    break;
                case TerminacionSesionVistaModelo:
                    ventana = new TerminacionSesion();
                    break;
                case ClasificacionVistaModelo:
                    ventana = new Clasificacion();
                    break;
                case VentanaPrincipalVistaModelo:
                    ventana = new VentanaPrincipal();
                    break;
                default:
                    throw new InvalidOperationException($"No existe vista registrada para {
                        vistaModelo.GetType().Name}");
            }

            ventana.DataContext = vistaModelo;
            return ventana;
        }
    }
}