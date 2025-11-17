using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.VentanaPrincipal
{
    public class ClasificacionVistaModelo : BaseVistaModelo
    {
        private readonly IClasificacionServicio _clasificacionServicio;
        private IReadOnlyList<DTOs.ClasificacionUsuarioDTO> _clasificacionOriginal;
        private ObservableCollection<DTOs.ClasificacionUsuarioDTO> _clasificacion;
        private bool _estaCargando;

        public ClasificacionVistaModelo(IClasificacionServicio clasificacionServicio)
        {
            _clasificacionServicio = clasificacionServicio ?? throw new ArgumentNullException(nameof(clasificacionServicio));

            _clasificacionOriginal = Array.Empty<DTOs.ClasificacionUsuarioDTO>();
            _clasificacion = new ObservableCollection<DTOs.ClasificacionUsuarioDTO>();

            OrdenarPorRondasComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                OrdenarPorRondas();
            }, _ => PuedeOrdenar());

            OrdenarPorPuntosComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                OrdenarPorPuntos();
            }, _ => PuedeOrdenar());

            CerrarComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                CerrarAccion?.Invoke();
            });
        }

        public ObservableCollection<DTOs.ClasificacionUsuarioDTO> Clasificacion
        {
            get => _clasificacion;
            private set
            {
                if (EstablecerPropiedad(ref _clasificacion, value))
                {
                    NotificarCambio(nameof(HayResultados));
                    NotificarEstadoComandosOrdenamiento();
                }
            }
        }

        public bool EstaCargando
        {
            get => _estaCargando;
            private set
            {
                if (EstablecerPropiedad(ref _estaCargando, value))
                {
                    NotificarEstadoComandosOrdenamiento();
                }
            }
        }

        public bool HayResultados => Clasificacion?.Count > 0;

        public IComandoNotificable OrdenarPorRondasComando { get; }

        public IComandoNotificable OrdenarPorPuntosComando { get; }

        public IComandoNotificable CerrarComando { get; }

        public Action CerrarAccion { get; set; }

        public async Task CargarClasificacionAsync()
        {
            EstaCargando = true;

            try
            {
                IReadOnlyList<DTOs.ClasificacionUsuarioDTO> clasificacion =
                    await _clasificacionServicio.ObtenerTopJugadoresAsync().ConfigureAwait(true);

                _clasificacionOriginal = clasificacion ?? Array.Empty<DTOs.ClasificacionUsuarioDTO>();
                ActualizarClasificacion(_clasificacionOriginal);
            }
            catch (ServicioExcepcion ex)
            {
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private void ActualizarClasificacion(IEnumerable<DTOs.ClasificacionUsuarioDTO> clasificacion)
        {
            Clasificacion = new ObservableCollection<DTOs.ClasificacionUsuarioDTO>(
                clasificacion?.Where(c => c != null) ?? Enumerable.Empty<DTOs.ClasificacionUsuarioDTO>());
        }

        private void OrdenarPorRondas()
        {
            if (!PuedeOrdenar())
            {
                return;
            }

            IEnumerable<DTOs.ClasificacionUsuarioDTO> ordenados = _clasificacionOriginal
                .Where(c => c != null)
                .OrderByDescending(c => c.RondasGanadas)
                .ThenByDescending(c => c.Puntos)
                .ThenBy(c => c.Usuario);

            ActualizarClasificacion(ordenados);
        }

        private void OrdenarPorPuntos()
        {
            if (!PuedeOrdenar())
            {
                return;
            }

            IEnumerable<DTOs.ClasificacionUsuarioDTO> ordenados = _clasificacionOriginal
                .Where(c => c != null)
                .OrderByDescending(c => c.Puntos)
                .ThenByDescending(c => c.RondasGanadas)
                .ThenBy(c => c.Usuario);

            ActualizarClasificacion(ordenados);
        }

        private bool PuedeOrdenar()
        {
            return !EstaCargando && _clasificacionOriginal?.Count > 0;
        }

        private void NotificarEstadoComandosOrdenamiento()
        {
            OrdenarPorRondasComando?.NotificarPuedeEjecutar();
            OrdenarPorPuntosComando?.NotificarPuedeEjecutar();
        }
    }
}
