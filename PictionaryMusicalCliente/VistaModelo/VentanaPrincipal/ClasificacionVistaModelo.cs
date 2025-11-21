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
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.VentanaPrincipal
{
    /// <summary>
    /// Gestiona la logica de presentacion para la tabla de clasificacion de jugadores.
    /// </summary>
    public class ClasificacionVistaModelo : BaseVistaModelo
    {
        private static readonly ILog Log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IClasificacionServicio _clasificacionServicio;
        private IReadOnlyList<DTOs.ClasificacionUsuarioDTO> _clasificacionOriginal;
        private ObservableCollection<DTOs.ClasificacionUsuarioDTO> _clasificacion;
        private bool _estaCargando;

        /// <summary>
        /// Inicializa el ViewModel con el servicio de clasificacion.
        /// </summary>
        /// <param name="clasificacionServicio">Servicio para obtener los datos del ranking.
        /// </param>
        public ClasificacionVistaModelo(IClasificacionServicio clasificacionServicio)
        {
            _clasificacionServicio = clasificacionServicio ??
                throw new ArgumentNullException(nameof(clasificacionServicio));

            _clasificacionOriginal = Array.Empty<DTOs.ClasificacionUsuarioDTO>();
            _clasificacion = new ObservableCollection<DTOs.ClasificacionUsuarioDTO>();

            OrdenarPorRondasComando = new ComandoDelegado(_ =>
            {
                SonidoManejador.ReproducirClick();
                OrdenarPorRondas();
            }, _ => PuedeOrdenar());

            OrdenarPorPuntosComando = new ComandoDelegado(_ =>
            {
                SonidoManejador.ReproducirClick();
                OrdenarPorPuntos();
            }, _ => PuedeOrdenar());

            CerrarComando = new ComandoDelegado(_ =>
            {
                SonidoManejador.ReproducirClick();
                CerrarAccion?.Invoke();
            });
        }

        /// <summary>
        /// Coleccion observable de la clasificacion actual para mostrar en la vista.
        /// </summary>
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

        /// <summary>
        /// Indica si se estan recuperando datos del servidor.
        /// </summary>
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

        /// <summary>
        /// Indica si existen resultados para mostrar en la tabla.
        /// </summary>
        public bool HayResultados => Clasificacion?.Count > 0;

        /// <summary>
        /// Comando para ordenar la lista por partidas ganadas.
        /// </summary>
        public IComandoNotificable OrdenarPorRondasComando { get; }

        /// <summary>
        /// Comando para ordenar la lista por puntuacion total acumulada.
        /// </summary>
        public IComandoNotificable OrdenarPorPuntosComando { get; }

        /// <summary>
        /// Comando para cerrar la ventana.
        /// </summary>
        public IComandoNotificable CerrarComando { get; }

        /// <summary>
        /// Accion para cerrar la vista asociada.
        /// </summary>
        public Action CerrarAccion { get; set; }

        /// <summary>
        /// Recupera la informacion de clasificacion desde el servicio.
        /// </summary>
        public async Task CargarClasificacionAsync()
        {
            EstaCargando = true;

            try
            {
                Log.Info("Solicitando tabla de clasificación al servidor.");
                IReadOnlyList<DTOs.ClasificacionUsuarioDTO> clasificacion =
                    await _clasificacionServicio.ObtenerTopJugadoresAsync().ConfigureAwait(true);

                _clasificacionOriginal = clasificacion ?? Array.Empty
                    <DTOs.ClasificacionUsuarioDTO>();
                ActualizarClasificacion(_clasificacionOriginal);
            }
            catch (ServicioExcepcion ex)
            {
                Log.Error("Error al obtener clasificación.", ex);
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private void ActualizarClasificacion(
            IEnumerable<DTOs.ClasificacionUsuarioDTO> clasificacion)
        {
            Clasificacion = new ObservableCollection<DTOs.ClasificacionUsuarioDTO>(
                clasificacion?.Where(c => c != null)
                ?? Enumerable.Empty<DTOs.ClasificacionUsuarioDTO>());
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