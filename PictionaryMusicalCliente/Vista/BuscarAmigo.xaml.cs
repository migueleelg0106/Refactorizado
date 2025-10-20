using System;
using System.Windows;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;

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

        private void BotonEnviarSolicitud(object sender, RoutedEventArgs e)
        {
            _ = EnviarSolicitudAsync();
        }

        private void BotonCancelar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async System.Threading.Tasks.Task EnviarSolicitudAsync()
        {
            string nombreBuscado = textoNombreUsuario?.Text?.Trim();

            if (string.IsNullOrWhiteSpace(nombreBuscado))
            {
                AvisoHelper.Mostrar(string.Format(Lang.errorTextoCampoObligatorio, Lang.globalTextoUsuario));
                return;
            }

            string remitente = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;

            if (string.IsNullOrWhiteSpace(remitente))
            {
                AvisoHelper.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            try
            {
                ResultadoOperacion resultado = await _amigosService
                    .EnviarSolicitudAsync(remitente, nombreBuscado)
                    .ConfigureAwait(true);

                if (resultado == null)
                {
                    AvisoHelper.Mostrar(Lang.errorTextoServidorNoDisponible);
                    return;
                }

                if (resultado.Exito)
                {
                    if (!string.IsNullOrWhiteSpace(resultado.Mensaje))
                    {
                        AvisoHelper.Mostrar(resultado.Mensaje);
                    }

                    Close();
                }
                else
                {
                    string mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                        ? Lang.errorTextoErrorProcesarSolicitud
                        : resultado.Mensaje;
                    AvisoHelper.Mostrar(mensaje);
                }
            }
            catch (ServicioException ex)
            {
                AvisoHelper.Mostrar(ex.Message ?? Lang.errorTextoServidorNoDisponible);
            }
            catch (Exception ex)
            {
                AvisoHelper.Mostrar(string.IsNullOrWhiteSpace(ex.Message)
                    ? Lang.errorTextoServidorNoDisponible
                    : ex.Message);
            }
        }
    }
}
