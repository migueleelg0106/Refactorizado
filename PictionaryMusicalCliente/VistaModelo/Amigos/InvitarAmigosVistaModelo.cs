using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    /// <summary>
    /// Controla la logica para invitar amigos conectados a una sala de juego.
    /// </summary>
    public class InvitarAmigosVistaModelo : BaseVistaModelo
    {
        private static readonly ILog Log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IInvitacionesServicio _invitacionesServicio;
        private readonly IPerfilServicio _perfilServicio;
        private readonly string _codigoSala;
        private readonly Action<int> _registrarAmigoInvitado;
        private readonly Action<string> _mostrarMensaje;

        /// <summary>
        /// Inicializa el ViewModel con la lista de amigos y servicios necesarios.
        /// </summary>
        public InvitarAmigosVistaModelo(
            IEnumerable<DTOs.AmigoDTO> amigos,
            IInvitacionesServicio invitacionesServicio,
            IPerfilServicio perfilServicio,
            string codigoSala,
            Func<int, bool> amigoInvitado,
            Action<int> registrarAmigoInvitado,
            Action<string> mostrarMensaje)
        {
            _invitacionesServicio = invitacionesServicio ??
                throw new ArgumentNullException(nameof(invitacionesServicio));
            _perfilServicio = perfilServicio ??
                throw new ArgumentNullException(nameof(perfilServicio));

            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                throw new ArgumentException("El código de la sala es obligatorio.",
                    nameof(codigoSala));
            }

            _codigoSala = codigoSala;
            _registrarAmigoInvitado = registrarAmigoInvitado;
            _mostrarMensaje = mostrarMensaje;

            Amigos = new ObservableCollection<AmigoInvitacionItemVistaModelo>(
                CrearElementos(amigos, amigoInvitado));
        }

        /// <summary>
        /// Coleccion de amigos disponibles para invitar.
        /// </summary>
        public ObservableCollection<AmigoInvitacionItemVistaModelo> Amigos { get; }

        internal async Task InvitarAsync(AmigoInvitacionItemVistaModelo amigo)
        {
            if (amigo == null)
            {
                return;
            }

            if (amigo.InvitacionEnviada)
            {
                SonidoManejador.ReproducirError();
                _mostrarMensaje?.Invoke(Lang.invitarAmigosTextoYaInvitado);
                return;
            }

            if (amigo.UsuarioId <= 0)
            {
                SonidoManejador.ReproducirError();
                _mostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            amigo.EstaProcesando = true;

            try
            {
                Log.InfoFormat("Obteniendo perfil para invitar amigo ID: {0}",
                    amigo.UsuarioId);
                DTOs.UsuarioDTO perfil = await _perfilServicio
                    .ObtenerPerfilAsync(amigo.UsuarioId)
                    .ConfigureAwait(true);

                if (perfil == null || string.IsNullOrWhiteSpace(perfil.Correo))
                {
                    Log.WarnFormat("Perfil o correo no disponible para amigo ID: {0}",
                        amigo.UsuarioId);
                    SonidoManejador.ReproducirError();
                    _mostrarMensaje?.Invoke(Lang.invitarAmigosTextoCorreoNoDisponible);
                    return;
                }

                Log.InfoFormat("Enviando invitación por correo a: {0}",
                    perfil.Correo);
                DTOs.ResultadoOperacionDTO resultado = await _invitacionesServicio
                    .EnviarInvitacionAsync(_codigoSala, perfil.Correo)
                    .ConfigureAwait(true);

                if (resultado != null && resultado.OperacionExitosa)
                {
                    SonidoManejador.ReproducirExito();
                    amigo.MarcarInvitacionEnviada();
                    _registrarAmigoInvitado?.Invoke(amigo.UsuarioId);
                    _mostrarMensaje?.Invoke(Lang.invitarCorreoTextoEnviado);
                }
                else
                {
                    Log.WarnFormat("Fallo al enviar invitación: {0}",
                        resultado?.Mensaje);
                    SonidoManejador.ReproducirError();
                    string mensaje = MensajeServidorAyudante.Localizar(
                        resultado?.Mensaje,
                        Lang.errorTextoEnviarCorreo);
                    _mostrarMensaje?.Invoke(mensaje);
                }
            }
            catch (ServicioExcepcion ex)
            {
                Log.Error("Error de servicio al enviar invitación.", ex);
                SonidoManejador.ReproducirError();
                _mostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoEnviarCorreo);
            }
            catch (ArgumentException ex)
            {
                Log.Error("Error de argumento inválido al invitar.", ex);
                SonidoManejador.ReproducirError();
                _mostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoEnviarCorreo);
            }
            finally
            {
                amigo.EstaProcesando = false;
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

            foreach (DTOs.AmigoDTO amigo in amigos.Where(a => a != null && a.UsuarioId > 0))
            {
                if (!invitados.Add(amigo.UsuarioId))
                {
                    continue;
                }

                bool yaInvitado = amigoInvitado?.Invoke(amigo.UsuarioId) ?? false;
                elementos.Add(new AmigoInvitacionItemVistaModelo(amigo, this, yaInvitado));
            }

            return elementos;
        }
    }

    /// <summary>
    /// Representa un item individual en la lista de amigos para invitar.
    /// </summary>
    public class AmigoInvitacionItemVistaModelo : BaseVistaModelo
    {
        private readonly InvitarAmigosVistaModelo _padre;
        private bool _invitacionEnviada;
        private bool _estaProcesando;

        /// <summary>
        /// Crea una instancia del item de invitacion.
        /// </summary>
        public AmigoInvitacionItemVistaModelo(
            DTOs.AmigoDTO amigo,
            InvitarAmigosVistaModelo padre,
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

            InvitarComando = new ComandoAsincrono(async () =>
            {
                SonidoManejador.ReproducirClick();
                await _padre.InvitarAsync(this).ConfigureAwait(true);
            }, () => !EstaProcesando);
        }

        /// <summary>
        /// Identificador unico del usuario amigo.
        /// </summary>
        public int UsuarioId { get; }

        /// <summary>
        /// Nombre de usuario a mostrar.
        /// </summary>
        public string NombreUsuario { get; }

        /// <summary>
        /// Indica si la invitacion ya ha sido enviada exitosamente.
        /// </summary>
        public bool InvitacionEnviada
        {
            get => _invitacionEnviada;
            private set
            {
                if (EstablecerPropiedad(ref _invitacionEnviada, value))
                {
                    NotificarCambio(nameof(TextoBoton));
                    InvitarComando.NotificarPuedeEjecutar();
                }
            }
        }

        /// <summary>
        /// Indica si hay una operacion en curso para este item.
        /// </summary>
        public bool EstaProcesando
        {
            get => _estaProcesando;
            set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    InvitarComando.NotificarPuedeEjecutar();
                }
            }
        }

        /// <summary>
        /// Texto dinamico del boton (Invitar / Invitado).
        /// </summary>
        public string TextoBoton => InvitacionEnviada
            ? Lang.invitarAmigosTextoInvitado
            : Lang.globalTextoInvitar;

        /// <summary>
        /// Comando para ejecutar el envio de la invitacion.
        /// </summary>
        public IComandoAsincrono InvitarComando { get; }

        internal void MarcarInvitacionEnviada()
        {
            InvitacionEnviada = true;
        }
    }
}