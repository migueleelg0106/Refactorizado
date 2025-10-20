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
    /// Lógica de interacción para EliminarAmigo.xaml
    /// </summary>
    public partial class EliminarAmigo : Window
    {
        private readonly IAmigosService _amigosService;
        private readonly string _amigo;

        public EliminarAmigo(string amigo)
            : this(amigo, new AmigosService())
        {
        }

        public EliminarAmigo(string amigo, IAmigosService amigosService)
        {
            if (string.IsNullOrWhiteSpace(amigo))
            {
                throw new ArgumentException("El amigo es obligatorio", nameof(amigo));
            }

            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));
            _amigo = amigo;

            InitializeComponent();

            bloqueTextoMensaje.Text = string.Concat(Lang.eliminarAmigoTextoConfirmacion, _amigo, "?");
        }

        private void BotonAceptar(object sender, RoutedEventArgs e)
        {
            _ = EliminarAmigoAsync();
        }

        private void BotonCancelar(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async System.Threading.Tasks.Task EliminarAmigoAsync()
        {
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
                    AvisoHelper.Mostrar(Lang.errorTextoServidorNoDisponible);
                    return;
                }

                string mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? (resultado.Exito ? Lang.eliminarAmigoTextoEliminar : Lang.errorTextoErrorProcesarSolicitud)
                    : resultado.Mensaje;

                AvisoHelper.Mostrar(mensaje);

                if (resultado.Exito)
                {
                    Close();
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
