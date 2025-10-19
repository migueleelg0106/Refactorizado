using System;
using System.Threading.Tasks;
using System.Windows;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para EliminarAmigo.xaml
    /// </summary>
    public partial class EliminarAmigo : Window
    {
        private readonly IAmigosService _amigosService;
        private readonly string _amigo;

        public EliminarAmigo()
            : this(string.Empty, new AmigosService())
        {
        }

        public EliminarAmigo(string amigo, IAmigosService amigosService)
        {
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));
            _amigo = amigo ?? string.Empty;

            InitializeComponent();
            ActualizarMensaje();
        }

        public VentanaPrincipalVistaModelo VistaModeloPrincipal { get; set; }

        private async void BotonAceptar(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_amigo))
            {
                AvisoHelper.Mostrar(Lang.amigosTextoNoSeleccionado);
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;

            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                AvisoHelper.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            try
            {
                ResultadoOperacion resultado = await _amigosService
                    .EliminarAmigoAsync(usuarioActual, _amigo)
                    .ConfigureAwait(true);

                if (resultado == null)
                {
                    AvisoHelper.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
                    return;
                }

                string mensaje = !string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? resultado.Mensaje
                    : resultado.Exito
                        ? Lang.amigosTextoEliminacionExitosa
                        : Lang.errorTextoErrorProcesarSolicitud;

                AvisoHelper.Mostrar(mensaje);

                if (resultado.Exito)
                {
                    if (VistaModeloPrincipal != null)
                    {
                        await VistaModeloPrincipal.RecargarAmigosAsync().ConfigureAwait(true);
                    }

                    Close();
                }
            }
            catch (ServicioException ex)
            {
                AvisoHelper.Mostrar(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
            catch (ArgumentException)
            {
                AvisoHelper.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
            }
        }

        private void BotonCancelar(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ActualizarMensaje()
        {
            bloqueTextoMensaje.Text = string.IsNullOrWhiteSpace(_amigo)
                ? Lang.amigosTextoNoSeleccionado
                : string.Concat(Lang.eliminarAmigoTextoConfirmacion, _amigo, "?");
        }
    }
}
