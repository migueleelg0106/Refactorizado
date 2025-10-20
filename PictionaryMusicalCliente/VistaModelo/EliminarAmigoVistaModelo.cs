using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Sesiones;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class EliminarAmigoVistaModelo : BaseVistaModelo
    {
        private readonly IAmigosService _amigosService;
        private readonly string _nombreAmigo;
        private bool _estaProcesando;
        private string _mensajeConfirmacion;

        public EliminarAmigoVistaModelo(string nombreAmigo, IAmigosService amigosService)
        {
            if (string.IsNullOrWhiteSpace(nombreAmigo))
            {
                throw new ArgumentException("El nombre del amigo es obligatorio.", nameof(nombreAmigo));
            }

            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));
            _nombreAmigo = nombreAmigo;

            MensajeConfirmacion = string.Format(
                CultureInfo.CurrentCulture,
                "{0}{1}?",
                Lang.eliminarAmigoTextoConfirmacion,
                nombreAmigo);

            ConfirmarEliminacionCommand = new ComandoAsincrono(_ => ConfirmarEliminacionAsync(), _ => !EstaProcesando);
            CancelarCommand = new ComandoDelegado(_ => CerrarAccion?.Invoke());
        }

        public string MensajeConfirmacion
        {
            get => _mensajeConfirmacion;
            private set => EstablecerPropiedad(ref _mensajeConfirmacion, value);
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ConfirmarEliminacionCommand?.NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono ConfirmarEliminacionCommand { get; }

        public ICommand CancelarCommand { get; }

        public Action CerrarAccion { get; set; }

        public Action<string> AmigoEliminado { get; set; }

        public Action<string> MostrarMensaje { get; set; }

        private async Task ConfirmarEliminacionAsync()
        {
            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;

            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            EstaProcesando = true;

            try
            {
                ResultadoOperacion resultado = await _amigosService
                    .EliminarAmigoAsync(usuarioActual, _nombreAmigo)
                    .ConfigureAwait(true);

                if (resultado == null)
                {
                    MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                    return;
                }

                string mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? (resultado.Exito ? Lang.eliminarAmigoTextoExito : Lang.errorTextoErrorProcesarSolicitud)
                    : resultado.Mensaje;

                MostrarMensaje?.Invoke(mensaje);

                if (resultado.Exito)
                {
                    AmigoEliminado?.Invoke(_nombreAmigo);
                    CerrarAccion?.Invoke();
                }
            }
            catch (ServicioException ex)
            {
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
            catch (Exception)
            {
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
            }
            finally
            {
                EstaProcesando = false;
            }
        }
    }
}
