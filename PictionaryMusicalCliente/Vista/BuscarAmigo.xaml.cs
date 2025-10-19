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
    /// Lógica de interacción para BuscarAmigo.xaml
    /// </summary>
    public partial class BuscarAmigo : Window
    {
        private readonly IAmigosService _amigosService;

        public BuscarAmigo()
            : this(new AmigosService())
        {
        }

        public BuscarAmigo(IAmigosService amigosService)
        {
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));
            InitializeComponent();
        }

        public VentanaPrincipalVistaModelo VistaModeloPrincipal { get; set; }

        private async void BotonEnviarSolicitud(object sender, RoutedEventArgs e)
        {
            string nombreAmigo = entradaNombreUsuario.Text?.Trim();

            if (string.IsNullOrWhiteSpace(nombreAmigo))
            {
                AvisoHelper.Mostrar(Lang.amigosTextoNombreVacio);
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;

            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                AvisoHelper.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            if (string.Equals(usuarioActual, nombreAmigo, StringComparison.OrdinalIgnoreCase))
            {
                AvisoHelper.Mostrar(Lang.amigosTextoAutoSolicitud);
                return;
            }

            try
            {
                ResultadoOperacion resultado = await _amigosService
                    .EnviarSolicitudAmistadAsync(usuarioActual, nombreAmigo)
                    .ConfigureAwait(true);

                if (resultado == null)
                {
                    AvisoHelper.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
                    return;
                }

                string mensaje = !string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? resultado.Mensaje
                    : resultado.Exito
                        ? Lang.amigosTextoSolicitudEnviada
                        : Lang.errorTextoErrorProcesarSolicitud;

                AvisoHelper.Mostrar(mensaje);

                if (resultado.Exito)
                {
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
    }
}
