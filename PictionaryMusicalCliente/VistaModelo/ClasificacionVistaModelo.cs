using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades;
using ClasificacionSrv = PictionaryMusicalCliente.PictionaryServidorServicioClasificacion;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class ClasificacionVistaModelo : BaseVistaModelo
    {
        private readonly IClasificacionService _clasificacionService;
        private IReadOnlyList<ClasificacionSrv.ClasificacionUsuarioDTO> _clasificacionOriginal;
        private ObservableCollection<ClasificacionSrv.ClasificacionUsuarioDTO> _clasificacion;
        private bool _estaCargando;

        public ClasificacionVistaModelo(IClasificacionService clasificacionService)
        {
            _clasificacionService = clasificacionService ?? throw new ArgumentNullException(nameof(clasificacionService));

            _clasificacionOriginal = Array.Empty<ClasificacionSrv.ClasificacionUsuarioDTO>();
            _clasificacion = new ObservableCollection<ClasificacionSrv.ClasificacionUsuarioDTO>();

            OrdenarPorRondasCommand = new ComandoDelegado(_ => OrdenarPorRondas(), _ => PuedeOrdenar());
            OrdenarPorPuntosCommand = new ComandoDelegado(_ => OrdenarPorPuntos(), _ => PuedeOrdenar());
            CerrarCommand = new ComandoDelegado(_ => CerrarAccion?.Invoke());
        }

        public ObservableCollection<ClasificacionSrv.ClasificacionUsuarioDTO> Clasificacion
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

        public IComandoNotificable OrdenarPorRondasCommand { get; }

        public IComandoNotificable OrdenarPorPuntosCommand { get; }

        public IComandoNotificable CerrarCommand { get; }

        public Action CerrarAccion { get; set; }

        public async Task CargarClasificacionAsync()
        {
            EstaCargando = true;

            try
            {
                IReadOnlyList<ClasificacionSrv.ClasificacionUsuarioDTO> clasificacion =
                    await _clasificacionService.ObtenerTopJugadoresAsync().ConfigureAwait(true);

                _clasificacionOriginal = clasificacion ?? Array.Empty<ClasificacionSrv.ClasificacionUsuarioDTO>();
                ActualizarClasificacion(_clasificacionOriginal);
            }
            catch (ServicioException ex)
            {
                AvisoHelper.Mostrar(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
            catch (Exception)
            {
                AvisoHelper.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private void ActualizarClasificacion(IEnumerable<ClasificacionSrv.ClasificacionUsuarioDTO> clasificacion)
        {
            Clasificacion = new ObservableCollection<ClasificacionSrv.ClasificacionUsuarioDTO>(
                clasificacion?.Where(c => c != null) ?? Enumerable.Empty<ClasificacionSrv.ClasificacionUsuarioDTO>());
        }

        private void OrdenarPorRondas()
        {
            if (!PuedeOrdenar())
            {
                return;
            }

            IEnumerable<ClasificacionSrv.ClasificacionUsuarioDTO> ordenados = _clasificacionOriginal
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

            IEnumerable<ClasificacionSrv.ClasificacionUsuarioDTO> ordenados = _clasificacionOriginal
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
            OrdenarPorRondasCommand?.NotificarPuedeEjecutar();
            OrdenarPorPuntosCommand?.NotificarPuedeEjecutar();
        }
    }
}
