using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
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
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ISonidoManejador _sonidoManejador;
        private readonly IAvisoServicio _avisoServicio;

        private readonly IClasificacionServicio _clasificacionServicio;
        private IReadOnlyList<DTOs.ClasificacionUsuarioDTO> _clasificacionOriginal;
        private ObservableCollection<DTOs.ClasificacionUsuarioDTO> _clasificacion;
        private bool _estaCargando;

        /// <summary>
        /// Inicializa el ViewModel con el servicio de clasificacion.
        /// </summary>
        /// <param name="ventana">Servicio para gestionar ventanas.</param>
        /// <param name="localizador">Servicio de localizacion.</param>
        /// <param name="clasificacionServicio">Servicio para obtener los datos del ranking.</param>
        /// <param name="avisoServicio">Servicio de avisos.</param>
        /// <param name="sonidoManejador">Servicio de sonido.</param>
        public ClasificacionVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            IClasificacionServicio clasificacionServicio,
            IAvisoServicio avisoServicio,
            ISonidoManejador sonidoManejador)
            : base(ventana, localizador)
        {
            _clasificacionServicio = clasificacionServicio ??
                throw new ArgumentNullException(nameof(clasificacionServicio));
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));

            _clasificacionOriginal = Array.Empty<DTOs.ClasificacionUsuarioDTO>();
            _clasificacion = new ObservableCollection<DTOs.ClasificacionUsuarioDTO>();

            OrdenarPorRondasComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                OrdenarPorRondas();
            }, _ => PuedeOrdenar());

            OrdenarPorPuntosComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                OrdenarPorPuntos();
            }, _ => PuedeOrdenar());

            CerrarComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                _ventana.CerrarVentana(this);
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
        /// Recupera la informacion de clasificacion desde el servicio.
        /// </summary>
        public async Task CargarClasificacionAsync()
        {
            EstaCargando = true;

            await EjecutarOperacionAsync(async () =>
            {
				_logger.Info("Solicitando tabla de clasificacion al servidor.");
                IReadOnlyList<DTOs.ClasificacionUsuarioDTO> clasificacion =
                    await _clasificacionServicio.ObtenerTopJugadoresAsync().ConfigureAwait(true);

                _clasificacionOriginal = clasificacion ?? Array.Empty
                    <DTOs.ClasificacionUsuarioDTO>();
                ActualizarClasificacion(_clasificacionOriginal);
            },
            ex =>
            {
                _logger.Error("Error al obtener clasificacion.", ex);
                _avisoServicio.Mostrar(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            });

            EstaCargando = false;
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