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
    public class BuscarAmigoVistaModelo : BaseVistaModelo
    {
        private readonly IAmigosService _amigosService;
        private string _nombreUsuario;
        private bool _estaProcesando;

        public BuscarAmigoVistaModelo(IAmigosService amigosService)
        {
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));

            EnviarSolicitudCommand = new ComandoAsincrono(_ => EnviarSolicitudAsync(), _ => !EstaProcesando);
            CancelarCommand = new ComandoDelegado(_ => Cancelar());
        }

        public string NombreUsuario
        {
            get => _nombreUsuario;
            set => EstablecerPropiedad(ref _nombreUsuario, value);
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ((IComandoNotificable)EnviarSolicitudCommand).NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono EnviarSolicitudCommand { get; }

        public ICommand CancelarCommand { get; }

        public Action<bool> MarcarUsuarioInvalido { get; set; }

        public Action<string> MostrarMensaje { get; set; }

        public Action CerrarAccion { get; set; }

        private async Task EnviarSolicitudAsync()
        {
            MarcarUsuarioInvalido?.Invoke(false);

            string identificador = NombreUsuario?.Trim();
            if (string.IsNullOrWhiteSpace(identificador))
            {
                MarcarUsuarioInvalido?.Invoke(true);
                MostrarMensaje?.Invoke(Lang.errorTextoCamposInvalidosGenerico);
                return;
            }

            if (!int.TryParse(identificador, NumberStyles.Integer, CultureInfo.InvariantCulture, out int destinatarioId) || destinatarioId <= 0)
            {
                MarcarUsuarioInvalido?.Invoke(true);
                MostrarMensaje?.Invoke(Lang.errorTextoIdentificadorUsuarioInvalido);
                return;
            }

            UsuarioSesion usuarioActual = SesionUsuarioActual.Instancia.Usuario;
            if (usuarioActual == null || usuarioActual.JugadorId <= 0)
            {
                MostrarMensaje?.Invoke(Lang.errorTextoCuentaDatosNoDisponibles);
                return;
            }

            var solicitud = new SolicitudAmistad
            {
                RemitenteId = usuarioActual.JugadorId,
                DestinatarioId = destinatarioId
            };

            EstaProcesando = true;

            try
            {
                ResultadoOperacion resultado = await _amigosService
                    .EnviarSolicitudAmistadAsync(solicitud)
                    .ConfigureAwait(true);

                if (resultado == null)
                {
                    MostrarMensaje?.Invoke(Lang.errorTextoServidorNoDisponible);
                    return;
                }

                if (resultado.Exito)
                {
                    MostrarMensaje?.Invoke(resultado.Mensaje ?? Lang.avisoTextoSolicitudAmistadEnviada);
                    CerrarAccion?.Invoke();
                }
                else
                {
                    MarcarUsuarioInvalido?.Invoke(true);
                    MostrarMensaje?.Invoke(resultado.Mensaje ?? Lang.errorTextoAmistadEnviar);
                }
            }
            catch (ServicioException ex)
            {
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoAmistadEnviar);
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
