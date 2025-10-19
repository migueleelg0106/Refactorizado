using System;
using System.Windows;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
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
            : this(AmigosService.Instancia)
        {
        }

        public BuscarAmigo(IAmigosService amigosService)
        {
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));

            InitializeComponent();
        }

        private async void BotonEnviarSolicitud(object sender, RoutedEventArgs e)
        {
            string nombreUsuario = BloqueUsuario?.Text?.Trim();

            ResultadoOperacion validacion = ValidacionEntradaHelper.ValidarUsuario(nombreUsuario);

            if (!validacion.Exito)
            {
                AvisoHelper.Mostrar(validacion.Mensaje ?? Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            try
            {
                ResultadoOperacion resultado = await _amigosService.EnviarSolicitudAsync(nombreUsuario);

                string mensaje = resultado?.Mensaje;
                if (resultado?.Exito == true)
                {
                    if (string.IsNullOrWhiteSpace(mensaje))
                    {
                        mensaje = Lang.amigosTextoSolicitudEnviada;
                    }

                    AvisoHelper.Mostrar(mensaje);
                    Close();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(mensaje))
                    {
                        mensaje = Lang.errorTextoErrorProcesarSolicitud;
                    }

                    AvisoHelper.Mostrar(mensaje);
                }
            }
            catch (ServicioException ex)
            {
                string mensaje = ex.Message;
                if (string.IsNullOrWhiteSpace(mensaje))
                {
                    mensaje = Lang.errorTextoServidorNoDisponible;
                }

                AvisoHelper.Mostrar(mensaje);
            }
            catch (Exception)
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