using System;
using System.Globalization;
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
    /// Lógica de interacción para EliminarAmigo.xaml
    /// </summary>
    public partial class EliminarAmigo : Window
    {
        private readonly IAmigosService _amigosService;
        private readonly string _nombreAmigo;

        public EliminarAmigo(string nombreAmigo)
            : this(nombreAmigo, AmigosService.Instancia)
        {
        }

        public EliminarAmigo(string nombreAmigo, IAmigosService amigosService)
        {
            if (string.IsNullOrWhiteSpace(nombreAmigo))
            {
                throw new ArgumentException("El nombre del amigo es obligatorio.", nameof(nombreAmigo));
            }

            _nombreAmigo = nombreAmigo.Trim();
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));

            InitializeComponent();
            EstablecerMensajeConfirmacion();
        }

        private void EstablecerMensajeConfirmacion()
        {
            string mensaje = string.Format(
                CultureInfo.CurrentCulture,
                "{0}{1}?",
                Lang.eliminarAmigoTextoConfirmacion,
                _nombreAmigo);

            bloqueTextoMensaje.Text = mensaje;
        }

        private async void BotonAceptar(object sender, RoutedEventArgs e)
        {
            try
            {
                ResultadoOperacion resultado = await _amigosService.EliminarAmigoAsync(_nombreAmigo);

                string mensaje = resultado?.Mensaje;
                if (resultado?.Exito == true)
                {
                    if (string.IsNullOrWhiteSpace(mensaje))
                    {
                        mensaje = Lang.amigosTextoAmigoEliminado;
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