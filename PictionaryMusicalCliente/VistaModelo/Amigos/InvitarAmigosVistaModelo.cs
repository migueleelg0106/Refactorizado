using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    public class InvitarAmigosVistaModelo : BaseVistaModelo
    {
        private readonly IInvitacionesServicio _invitacionesServicio;
        private readonly IPerfilServicio _perfilServicio;
        private readonly string _codigoSala;
        private readonly Action<int> _registrarAmigoInvitado;
        private readonly Action<string> _mostrarMensaje;

        public InvitarAmigosVistaModelo(
            IEnumerable<DTOs.AmigoDTO> amigos,
            IInvitacionesServicio invitacionesServicio,
            IPerfilServicio perfilServicio,
            string codigoSala,
            Func<int, bool> amigoInvitado,
            Action<int> registrarAmigoInvitado,
            Action<string> mostrarMensaje)
        {
            _invitacionesServicio = invitacionesServicio ?? throw new ArgumentNullException(nameof(invitacionesServicio));
            _perfilServicio = perfilServicio ?? throw new ArgumentNullException(nameof(perfilServicio));

            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                throw new ArgumentException("El código de la sala es obligatorio.", nameof(codigoSala));
            }

            _codigoSala = codigoSala;
            _registrarAmigoInvitado = registrarAmigoInvitado;
            _mostrarMensaje = mostrarMensaje;

            Amigos = new ObservableCollection<AmigoInvitacionItemVistaModelo>(
                CrearElementos(amigos, amigoInvitado));
        }

        public ObservableCollection<AmigoInvitacionItemVistaModelo> Amigos { get; }

        internal async Task InvitarAsync(AmigoInvitacionItemVistaModelo amigo)
        {
            if (amigo == null)
            {
                return;
            }

            if (amigo.InvitacionEnviada)
            {
                ManejadorSonido.ReproducirError();
                _mostrarMensaje?.Invoke(Lang.invitarAmigosTextoYaInvitado);
                return;
            }

            if (amigo.UsuarioId <= 0)
            {
                ManejadorSonido.ReproducirError();
                _mostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            amigo.EstaProcesando = true;

            try
            {
                DTOs.UsuarioDTO perfil = await _perfilServicio
                    .ObtenerPerfilAsync(amigo.UsuarioId)
                    .ConfigureAwait(true);

                if (perfil == null || string.IsNullOrWhiteSpace(perfil.Correo))
                {
                    ManejadorSonido.ReproducirError();
                    _mostrarMensaje?.Invoke(Lang.invitarAmigosTextoCorreoNoDisponible);
                    return;
                }

                DTOs.ResultadoOperacionDTO resultado = await _invitacionesServicio
                    .EnviarInvitacionAsync(_codigoSala, perfil.Correo)
                    .ConfigureAwait(true);

                if (resultado != null && resultado.OperacionExitosa)
                {
                    ManejadorSonido.ReproducirExito();
                    amigo.MarcarInvitacionEnviada();
                    _registrarAmigoInvitado?.Invoke(amigo.UsuarioId);
                    _mostrarMensaje?.Invoke(Lang.invitarCorreoTextoEnviado);
                }
                else
                {
                    ManejadorSonido.ReproducirError();
                    string mensaje = MensajeServidorAyudante.Localizar(
                        resultado?.Mensaje,
                        Lang.errorTextoEnviarCorreo);
                    _mostrarMensaje?.Invoke(mensaje);
                }
            }
            catch (ServicioExcepcion ex)
            {
                Debug.WriteLine($"[Error Invitaciones Amigos]: {ex.Message}");
                ManejadorSonido.ReproducirError();
                _mostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoEnviarCorreo);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"[Error Invitaciones Amigos - Argumento inválido]: {ex.Message}");
                ManejadorSonido.ReproducirError();
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

    public class AmigoInvitacionItemVistaModelo : BaseVistaModelo
    {
        private readonly InvitarAmigosVistaModelo _padre;
        private bool _invitacionEnviada;
        private bool _estaProcesando;

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
                ManejadorSonido.ReproducirClick();
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
                if (EstablecerPropiedad(ref _invitacionEnviada, value))
                {
                    NotificarCambio(nameof(TextoBoton));
                    InvitarComando.NotificarPuedeEjecutar();
                }
            }
        }

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
