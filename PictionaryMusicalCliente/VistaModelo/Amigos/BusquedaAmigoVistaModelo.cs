using System;
using System.Threading.Tasks;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    public class BusquedaAmigoVistaModelo : BaseVistaModelo
    {
        private readonly IAmigosServicio _amigosServicio;
        private readonly string _usuarioActual;
        private string _nombreUsuarioBusqueda;
        private bool _estaProcesando;

        public BusquedaAmigoVistaModelo(IAmigosServicio amigosServicio)
        {
            _amigosServicio = amigosServicio ?? throw new ArgumentNullException(nameof(amigosServicio));
            _usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario ?? string.Empty;

            EnviarSolicitudComando = new ComandoAsincrono(_ => EnviarSolicitudAsync(), _ => PuedeEnviarSolicitud());
            CancelarComando = new ComandoDelegado(_ => Cancelado?.Invoke());
        }

        public string NombreUsuarioBusqueda
        {
            get => _nombreUsuarioBusqueda;
            set
            {
                if (EstablecerPropiedad(ref _nombreUsuarioBusqueda, value))
                {
                    EnviarSolicitudComando?.NotificarPuedeEjecutar();
                }
            }
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    EnviarSolicitudComando?.NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono EnviarSolicitudComando { get; }

        public ICommand CancelarComando { get; }

        public Action SolicitudEnviada { get; set; }

        public Action Cancelado { get; set; }

        private bool PuedeEnviarSolicitud()
        {
            return !EstaProcesando
                && !string.IsNullOrWhiteSpace(NombreUsuarioBusqueda);
        }

        private async Task EnviarSolicitudAsync()
        {
            string nombreAmigo = NombreUsuarioBusqueda?.Trim();

            if (string.IsNullOrWhiteSpace(nombreAmigo))
            {
                AvisoAyudante.Mostrar(Lang.buscarAmigoTextoIngreseUsuario);
                return;
            }

            if (string.IsNullOrWhiteSpace(_usuarioActual))
            {
                AvisoAyudante.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            EstaProcesando = true;

            try
            {
                await _amigosServicio.EnviarSolicitudAsync(_usuarioActual, nombreAmigo).ConfigureAwait(true);
                AvisoAyudante.Mostrar(Lang.amigosTextoSolicitudEnviada);
                SolicitudEnviada?.Invoke();
            }
            catch (ExcepcionServicio ex)
            {
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
            finally
            {
                EstaProcesando = false;
            }
        }
    }
}
