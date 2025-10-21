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
    public class EliminarAmigoVistaModelo : BaseVistaModelo
    {
        private readonly IAmigosService _amigosService;
        private bool _estaProcesando;

        public EliminarAmigoVistaModelo(string nombreAmigo, IAmigosService amigosService)
        {
            if (string.IsNullOrWhiteSpace(nombreAmigo))
            {
                throw new ArgumentException("El nombre del amigo es obligatorio.", nameof(nombreAmigo));
            }

            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));

            NombreAmigo = nombreAmigo;
            ConfirmarCommand = new ComandoAsincrono(_ => EliminarAmigoAsync(), _ => !EstaProcesando);
        }

        public string NombreAmigo { get; }

        public string MensajeConfirmacion => string.Concat(Lang.eliminarAmigoTextoConfirmacion, NombreAmigo, "?");

        public IComandoAsincrono ConfirmarCommand { get; }

        public Action CerrarAccion { get; set; }

        public Action<string> MostrarMensaje { get; set; }

        private bool EstaProcesando
        {
            get => _estaProcesando;
            set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ConfirmarCommand.NotificarPuedeEjecutar();
                }
            }
        }

        private async Task EliminarAmigoAsync()
        {
            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                MostrarMensaje?.Invoke(Lang.errorTextoAmigosOperacion);
                return;
            }

            try
            {
                EstaProcesando = true;

                ResultadoOperacion resultado = await _amigosService
                    .EliminarAmigoAsync(usuarioActual, NombreAmigo)
                    .ConfigureAwait(false);

                if (resultado == null)
                {
                    MostrarMensaje?.Invoke(Lang.errorTextoAmigosOperacion);
                    return;
                }

                string mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? (resultado.Exito ? Lang.amigosTextoAmistadEliminada : Lang.errorTextoAmigosOperacion)
                    : resultado.Mensaje;

                MostrarMensaje?.Invoke(mensaje);

                if (resultado.Exito)
                {
                    CerrarAccion?.Invoke();
                }
            }
            catch (ServicioException ex)
            {
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoAmigosOperacion);
            }
            finally
            {
                EstaProcesando = false;
            }
        }
    }
}
