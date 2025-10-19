using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Sesiones;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    public class EliminarAmigoVistaModelo : BaseVistaModelo
    {
        private readonly IAmigosService _amigosService;
        private readonly Amigo _amigo;
        private string _mensajeConfirmacion;
        private bool _estaProcesando;

        public EliminarAmigoVistaModelo(IAmigosService amigosService, Amigo amigo)
        {
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));
            _amigo = amigo ?? throw new ArgumentNullException(nameof(amigo));

            MensajeConfirmacion = string.Format(
                CultureInfo.CurrentCulture,
                "{0}{1}?",
                Lang.eliminarAmigoTextoConfirmacion,
                _amigo.NombreParaMostrar);

            EliminarAmigoCommand = new ComandoAsincrono(_ => EliminarAsync(), _ => !EstaProcesando);
            CancelarCommand = new ComandoDelegado(_ => Cancelar());
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
                    ((IComandoNotificable)EliminarAmigoCommand).NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono EliminarAmigoCommand { get; }

        public ICommand CancelarCommand { get; }

        public Action<string> MostrarMensaje { get; set; }

        public Action<Amigo> AmigoEliminado { get; set; }

        public Action CerrarAccion { get; set; }

        private async Task EliminarAsync()
        {
            UsuarioSesion usuarioActual = SesionUsuarioActual.Instancia.Usuario;
            if (usuarioActual == null || usuarioActual.JugadorId <= 0)
            {
                MostrarMensaje?.Invoke(Lang.errorTextoCuentaDatosNoDisponibles);
                return;
            }

            var solicitud = new OperacionAmistad
            {
                JugadorId = usuarioActual.JugadorId,
                AmigoId = _amigo.JugadorId
            };

            EstaProcesando = true;

            try
            {
                ResultadoOperacion resultado = await _amigosService
                    .EliminarAmigoAsync(solicitud)
                    .ConfigureAwait(true);

                if (resultado == null)
                {
                    MostrarMensaje?.Invoke(Lang.errorTextoServidorNoDisponible);
                    return;
                }

                if (resultado.Exito)
                {
                    AmigoEliminado?.Invoke(_amigo);
                    MostrarMensaje?.Invoke(resultado.Mensaje ?? Lang.avisoTextoAmistadEliminada);
                    CerrarAccion?.Invoke();
                }
                else
                {
                    MostrarMensaje?.Invoke(resultado.Mensaje ?? Lang.errorTextoAmistadEliminar);
                }
            }
            catch (ServicioException ex)
            {
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoAmistadEliminar);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private void Cancelar()
        {
            CerrarAccion?.Invoke();
        }
    }
}
