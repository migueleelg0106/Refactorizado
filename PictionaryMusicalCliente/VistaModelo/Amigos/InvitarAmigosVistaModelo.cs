using log4net;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    /// <summary>
    /// Controla la logica para invitar amigos conectados a una sala de juego.
    /// </summary>
    public class InvitarAmigosVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IInvitacionesServicio _invitacionesServicio;
        private readonly IPerfilServicio _perfilServicio;
        private readonly ISonidoManejador _sonidoManejador;
        private readonly IAvisoServicio _avisoServicio;
        private readonly string _codigoSala;
        private readonly Action<int> _registrarAmigoInvitado;

        public InvitarAmigosVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            IEnumerable<DTOs.AmigoDTO> amigos,
            IInvitacionesServicio invitacionesServicio,
            IPerfilServicio perfilServicio,
            ISonidoManejador sonidoManejador,
            IAvisoServicio avisoServicio,
            string codigoSala,
            Func<int, bool> amigoInvitado,
            Action<int> registrarAmigoInvitado)
            : base(ventana, localizador)
        {
            _invitacionesServicio = invitacionesServicio ??
                throw new ArgumentNullException(nameof(invitacionesServicio));
            _perfilServicio = perfilServicio ??
                throw new ArgumentNullException(nameof(perfilServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));

            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                throw new ArgumentException(
                    "El codigo de la sala es obligatorio.",
                    nameof(codigoSala));
            }

            _codigoSala = codigoSala;
            _registrarAmigoInvitado = registrarAmigoInvitado;

            Amigos = new ObservableCollection<AmigoInvitacionItemVistaModelo>(
                CrearElementos(amigos, amigoInvitado));
        }

        public ObservableCollection<AmigoInvitacionItemVistaModelo> Amigos { get; }

        internal async Task InvitarAsync(AmigoInvitacionItemVistaModelo amigo)
        {
            if (!ValidarAmigo(amigo))
            {
                return;
            }

            amigo.EstaProcesando = true;

            await EjecutarOperacionAsync(async () =>
            {
                DTOs.UsuarioDTO perfil = await ObtenerPerfilAmigoAsync(
                    amigo.UsuarioId);
                
                if (!ValidarPerfil(perfil, amigo.UsuarioId))
                {
                    return;
                }

                await EnviarInvitacionPorCorreoAsync(perfil.Correo, amigo);
            },
            ex =>
            {
                _logger.Error("Error al enviar invitacion.", ex);
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(ex.Message ?? Lang.errorTextoEnviarCorreo);
                amigo.EstaProcesando = false;
            });

            amigo.EstaProcesando = false;
        }

        private bool ValidarAmigo(AmigoInvitacionItemVistaModelo amigo)
        {
            if (amigo == null)
            {
                return false;
            }

            if (amigo.InvitacionEnviada)
            {
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(Lang.invitarAmigosTextoYaInvitado);
                return false;
            }

            if (amigo.UsuarioId <= 0)
            {
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
                return false;
            }

            return true;
        }

        private async Task<DTOs.UsuarioDTO> ObtenerPerfilAmigoAsync(int usuarioId)
        {
            _logger.InfoFormat("Obteniendo perfil para invitar amigo ID: {0}",
                usuarioId);
            
            return await _perfilServicio
                .ObtenerPerfilAsync(usuarioId)
                .ConfigureAwait(true);
        }

        private bool ValidarPerfil(DTOs.UsuarioDTO perfil, int usuarioId)
        {
            if (perfil == null || string.IsNullOrWhiteSpace(perfil.Correo))
            {
                _logger.WarnFormat(
                    "Perfil o correo no disponible para amigo ID: {0}",
                    usuarioId);
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(Lang.invitarAmigosTextoCorreoNoDisponible);
                return false;
            }

            return true;
        }

        private async Task EnviarInvitacionPorCorreoAsync(
            string correo,
            AmigoInvitacionItemVistaModelo amigo)
        {
            _logger.InfoFormat("Enviando invitacion por correo a: {0}", correo);
            
            DTOs.ResultadoOperacionDTO resultado = await _invitacionesServicio
                .EnviarInvitacionAsync(_codigoSala, correo)
                .ConfigureAwait(true);

            ProcesarResultadoInvitacion(resultado, amigo);
        }

        private void ProcesarResultadoInvitacion(
            DTOs.ResultadoOperacionDTO resultado,
            AmigoInvitacionItemVistaModelo amigo)
        {
            if (resultado != null && resultado.OperacionExitosa)
            {
                _sonidoManejador.ReproducirNotificacion();
                amigo.MarcarInvitacionEnviada();
                _registrarAmigoInvitado?.Invoke(amigo.UsuarioId);
                _avisoServicio.Mostrar(Lang.invitarCorreoTextoEnviado);
            }
            else
            {
                _logger.WarnFormat("Fallo al enviar invitacion: {0}",
                    resultado?.Mensaje);
                _sonidoManejador.ReproducirError();
                string mensaje = _localizador.Localizar(
                    resultado?.Mensaje,
                    Lang.errorTextoEnviarCorreo);
                _avisoServicio.Mostrar(mensaje);
            }
        }

        private IEnumerable<AmigoInvitacionItemVistaModelo> CrearElementos(
            IEnumerable<DTOs.AmigoDTO> amigos,
            Func<int, bool> amigoInvitado)
        {
            if (amigos == null)
            {
                return Array.Empty<AmigoInvitacionItemVistaModelo>();
            }

            var invitados = new HashSet<int>();
            var elementos = new List<AmigoInvitacionItemVistaModelo>();

            foreach (DTOs.AmigoDTO amigo in amigos.Where(
                a => a != null && a.UsuarioId > 0))
            {
                if (!invitados.Add(amigo.UsuarioId))
                {
                    continue;
                }

                bool yaInvitado = amigoInvitado?.Invoke(amigo.UsuarioId) 
                    ?? false;
                elementos.Add(new AmigoInvitacionItemVistaModelo(
                    amigo, 
                    this, 
                    _sonidoManejador, 
                    yaInvitado));
            }

            return elementos;
        }
    }

    /// <summary>
    /// Representa un item individual en la lista de amigos para invitar.
    /// </summary>
    public class AmigoInvitacionItemVistaModelo
    {
        private readonly InvitarAmigosVistaModelo _padre;
        private readonly ISonidoManejador _sonidoManejador;
        private bool _invitacionEnviada;
        private bool _estaProcesando;

        public AmigoInvitacionItemVistaModelo(
            DTOs.AmigoDTO amigo,
            InvitarAmigosVistaModelo padre,
            ISonidoManejador sonidoManejador,
            bool invitacionEnviada)
        {
            if (amigo == null)
            {
                throw new ArgumentNullException(nameof(amigo));
            }

            _padre = padre ?? throw new ArgumentNullException(nameof(padre));
            UsuarioId = amigo.UsuarioId;
            NombreUsuario = amigo.NombreUsuario ?? string.Empty;
            _invitacionEnviada = invitacionEnviada;
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));

            InvitarComando = new ComandoAsincrono(async () =>
            {
                _sonidoManejador.ReproducirClick();
                await _padre.InvitarAsync(this).ConfigureAwait(true);
            }, () => !EstaProcesando);
        }

        public int UsuarioId { get; }

        public string NombreUsuario { get; }

        public bool InvitacionEnviada
        {
            get => _invitacionEnviada;
            private set
            {
                if (_invitacionEnviada != value)
                {
                    _invitacionEnviada = value;
                    InvitarComando.NotificarPuedeEjecutar();
                }
            }
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            set
            {
                if (_estaProcesando != value)
                {
                    _estaProcesando = value;
                    InvitarComando.NotificarPuedeEjecutar();
                }
            }
        }

        public string TextoBoton => InvitacionEnviada
            ? Lang.invitarAmigosTextoInvitado
            : Lang.globalTextoInvitar;

        public IComandoAsincrono InvitarComando { get; }

        internal void MarcarInvitacionEnviada()
        {
            InvitacionEnviada = true;
        }
    }
}