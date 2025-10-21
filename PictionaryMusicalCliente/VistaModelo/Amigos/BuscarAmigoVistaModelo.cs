using System;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    public class BuscarAmigoVistaModelo : BaseVistaModelo
    {
        private readonly IAmigosService _amigosService;
        private string _nombreUsuarioBusqueda;
        private bool _estaProcesando;

        public BuscarAmigoVistaModelo(IAmigosService amigosService)
        {
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));
            EnviarSolicitudCommand = new ComandoAsincrono(_ => EnviarSolicitudAsync(), _ => PuedeEnviar());
        }

        public string NombreUsuarioBusqueda
        {
            get => _nombreUsuarioBusqueda;
            set
            {
                if (EstablecerPropiedad(ref _nombreUsuarioBusqueda, value))
                {
                    EnviarSolicitudCommand.NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono EnviarSolicitudCommand { get; }

        public Action CerrarAccion { get; set; }

        public Action<string> MostrarMensaje { get; set; }

        private bool EstaProcesando
        {
            get => _estaProcesando;
            set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    EnviarSolicitudCommand.NotificarPuedeEjecutar();
                }
            }
        }

        private bool PuedeEnviar()
        {
            return !EstaProcesando && !string.IsNullOrWhiteSpace(NombreUsuarioBusqueda);
        }

        private async Task EnviarSolicitudAsync()
        {
            string usuarioBusqueda = NombreUsuarioBusqueda?.Trim();
            if (string.IsNullOrWhiteSpace(usuarioBusqueda))
            {
                MostrarMensaje?.Invoke("Validación: nombre de usuario a buscar vacío.");
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                MostrarMensaje?.Invoke("Validación: usuario actual no disponible en la sesión.");
                return;
            }

            try
            {
                EstaProcesando = true;

                ResultadoOperacion resultado = await _amigosService
                    .EnviarSolicitudAmistadAsync(usuarioActual, usuarioBusqueda)
                    .ConfigureAwait(false);

                if (resultado == null)
                {
                    MostrarMensaje?.Invoke("Error: el servicio devolvió un resultado nulo.");
                    return;
                }

                string mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? (resultado.Exito ? "Operación exitosa sin mensaje." : "Error: operación fallida sin detalle.")
                    : resultado.Mensaje;

                MostrarMensaje?.Invoke(mensaje);

                if (resultado.Exito)
                {
                    NombreUsuarioBusqueda = string.Empty;
                    CerrarAccion?.Invoke();
                }
            }
            catch (ServicioException ex)
            {
                string mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? "Excepción de servicio sin mensaje."
                    : $"ServicioException: {ex.Message}";
                MostrarMensaje?.Invoke(mensaje);
            }
            catch (Exception ex)
            {
                string mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? "Excepción inesperada sin mensaje."
                    : $"Excepción inesperada: {ex.Message}";
                MostrarMensaje?.Invoke(mensaje);
            }
            finally
            {
                EstaProcesando = false;
            }
        }
    }
}
