using log4net;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Perfil
{
    public class PerfilVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string RedSocialInstagram = "Instagram";
        private const string RedSocialFacebook = "Facebook";
        private const string RedSocialX = "X";
        private const string RedSocialDiscord = "Discord";

        private const int LongitudMaximaRedSocial = 50;
        private readonly IPerfilServicio _perfilServicio;
        private readonly ISeleccionarAvatarServicio _seleccionarAvatarServicio;
        private readonly ICambioContrasenaServicio _cambioContrasenaServicio;
        private readonly IRecuperacionCuentaServicio 
            _recuperacionCuentaDialogoServicio;
        private readonly IAvisoServicio _avisoServicio;
        private readonly SonidoManejador _sonidoManejador;
        private readonly IUsuarioAutenticado _usuarioSesion;
        private readonly ICatalogoAvatares _catalogoAvatares;
        private readonly ICatalogoImagenesPerfil _catalogoPerfil;

        private readonly Dictionary<string, RedSocialItemVistaModelo> 
            _redesPorNombre;

        private int _usuarioId;
        private string _usuario;
        private string _correo;
        private string _nombre;
        private string _apellido;
        private string _avatarSeleccionadoNombre;
        private int _avatarSeleccionadoId;
        private ImageSource _avatarSeleccionadoImagen;
        private bool _estaProcesando;
        private bool _estaCambiandoContrasena;

        public PerfilVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            IPerfilServicio perfilServicio,
            ISeleccionarAvatarServicio seleccionarAvatarServicio,
            ICambioContrasenaServicio cambioContrasenaServicio,
            IRecuperacionCuentaServicio recuperacionCuentaDialogoServicio,
            IAvisoServicio avisoServicio,
            SonidoManejador sonidoManejador,
            IUsuarioAutenticado usuarioSesion,
            ICatalogoAvatares catalogoAvatares,
            ICatalogoImagenesPerfil catalogoPerfil)
            : base(ventana, localizador)
        {
            _perfilServicio = perfilServicio ??
                throw new ArgumentNullException(nameof(perfilServicio));
            _seleccionarAvatarServicio = seleccionarAvatarServicio ??
                throw new ArgumentNullException(nameof(seleccionarAvatarServicio));
            _cambioContrasenaServicio = cambioContrasenaServicio ??
                throw new ArgumentNullException(nameof(cambioContrasenaServicio));
            _recuperacionCuentaDialogoServicio = recuperacionCuentaDialogoServicio ??
                throw new ArgumentNullException(
                    nameof(recuperacionCuentaDialogoServicio));
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            _usuarioSesion = usuarioSesion ??
                throw new ArgumentNullException(nameof(usuarioSesion));
            _catalogoAvatares = catalogoAvatares ??
                throw new ArgumentNullException(nameof(catalogoAvatares));
            _catalogoPerfil = catalogoPerfil ??
                throw new ArgumentNullException(nameof(catalogoPerfil));

            RedesSociales = CrearRedesSociales();
            _redesPorNombre = RedesSociales.ToDictionary(
                r => r.Nombre,
                StringComparer.OrdinalIgnoreCase);

            GuardarCambiosComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await GuardarCambiosAsync();
            }, _ => !EstaProcesando);

            SeleccionarAvatarComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await SeleccionarAvatarAsync();
            }, _ => !EstaProcesando);

            CambiarContrasenaComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await CambiarContrasenaAsync();
            }, _ => !EstaProcesando && !EstaCambiandoContrasena);

            CerrarComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                _ventana.CerrarVentana(this);
            });
        }

        public string Usuario 
        { 
            get => _usuario;
            private set => EstablecerPropiedad(ref _usuario, value);
        }

        public string Correo
        {
            get => _correo;
            private set => EstablecerPropiedad(ref _correo, value);
        }

        public string Nombre
        {
            get => _nombre;
            set => EstablecerPropiedad(ref _nombre, value);
        }

        public string Apellido
        {
            get => _apellido;
            set => EstablecerPropiedad(ref _apellido, value);
        }

        public string AvatarSeleccionadoNombre
        {
            get => _avatarSeleccionadoNombre;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoNombre, value);
        }

        public int AvatarSeleccionadoId
        {
            get => _avatarSeleccionadoId;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoId, value);
        }

        public ImageSource AvatarSeleccionadoImagen
        {
            get => _avatarSeleccionadoImagen;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoImagen, value);
        }

        public ObservableCollection<RedSocialItemVistaModelo> RedesSociales { get; }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ((IComandoNotificable)GuardarCambiosComando)
                        .NotificarPuedeEjecutar();
                    ((IComandoNotificable)SeleccionarAvatarComando)
                        .NotificarPuedeEjecutar();
                    ((IComandoNotificable)CambiarContrasenaComando)
                        .NotificarPuedeEjecutar();
                }
            }
        }

        public bool EstaCambiandoContrasena
        {
            get => _estaCambiandoContrasena;
            private set
            {
                if (EstablecerPropiedad(ref _estaCambiandoContrasena, value))
                {
                    ((IComandoNotificable)CambiarContrasenaComando)
                        .NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono GuardarCambiosComando { get; }

        public IComandoAsincrono SeleccionarAvatarComando { get; }

        public IComandoAsincrono CambiarContrasenaComando { get; }

        public ICommand CerrarComando { get; }

        public Action<IList<string>> MostrarCamposInvalidos { get; set; }

        public Action SolicitarReinicioSesion { get; set; }

        public bool RequiereReinicioSesion { get; private set; }

        public async Task CargarPerfilAsync()
        {
            if (!ValidarSesionActiva())
            {
                return;
            }

            EstaProcesando = true;

            await EjecutarOperacionAsync(async () =>
            {
                DTOs.UsuarioDTO perfil = await ObtenerPerfilDesdeServidorAsync();
                
                if (!ValidarPerfilObtenido(perfil))
                {
                    return;
                }

                AplicarPerfil(perfil);
            },
            ex =>
            {
                _logger.Error("Error de servicio al obtener perfil.", ex);
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(ex.Message ?? 
                    Lang.errorTextoServidorObtenerPerfil);
                EstaProcesando = false;
            });

            EstaProcesando = false;
        }

        private bool ValidarSesionActiva()
        {
            if (_usuarioSesion == null || _usuarioSesion.IdUsuario <= 0)
            {
                _logger.Warn("Intento de cargar perfil sin sesion valida.");
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(Lang.errorTextoPerfilActualizarInformacion);
                _ventana.CerrarVentana(this);
                return false;
            }

            return true;
        }

        private async Task<DTOs.UsuarioDTO> ObtenerPerfilDesdeServidorAsync()
        {
            return await _perfilServicio
                .ObtenerPerfilAsync(_usuarioSesion.IdUsuario)
                .ConfigureAwait(true);
        }

        private bool ValidarPerfilObtenido(DTOs.UsuarioDTO perfil)
        {
            if (perfil == null)
            {
                _logger.ErrorFormat("Perfil obtenido es nulo para ID: {0}",
                    _usuarioSesion.IdUsuario);
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(Lang.errorTextoServidorObtenerPerfil);
                return false;
            }

            return true;
        }

        private async Task SeleccionarAvatarAsync()
        {
            ObjetoAvatar avatar = await _seleccionarAvatarServicio
                .SeleccionarAvatarAsync(AvatarSeleccionadoId).ConfigureAwait(true);

            if (avatar == null)
            {
                return;
            }

            EstablecerAvatar(avatar);
        }

        private async Task GuardarCambiosAsync()
        {
            LimpiarEstadosValidacion();

            if (!ValidarDatosFormulario(out var camposInvalidos))
            {
                MostrarErroresValidacion(camposInvalidos);
                return;
            }

            DTOs.ActualizacionPerfilDTO solicitud = CrearSolicitudActualizacion();
            EstaProcesando = true;

            await EjecutarOperacionAsync(async () =>
            {
                DTOs.ResultadoOperacionDTO resultado = 
                    await EnviarActualizacionAsync(solicitud);
                
                ProcesarResultadoActualizacion(resultado);
            },
            ex =>
            {
                _logger.Error("Excepcion de servicio al actualizar perfil.", ex);
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(ex.Message ?? 
                    Lang.errorTextoServidorActualizarPerfil);
                EstaProcesando = false;
            });

            EstaProcesando = false;
        }

        private void LimpiarEstadosValidacion()
        {
            MostrarCamposInvalidos?.Invoke(Array.Empty<string>());
            LimpiarErroresRedesSociales();
        }

        private bool ValidarDatosFormulario(out List<string> camposInvalidos)
        {
            var (sonCamposValidos, errorCampos, invalidos) = 
                ValidarCamposPrincipales();
            var (sonRedesValidas, errorRedes) = ValidarRedesSociales();

            camposInvalidos = invalidos ?? new List<string>();
            
            if (!sonRedesValidas)
            {
                camposInvalidos.Add("RedesSociales");
            }

            return sonCamposValidos && sonRedesValidas;
        }

        private void MostrarErroresValidacion(List<string> camposInvalidos)
        {
            _sonidoManejador.ReproducirError();
            MostrarCamposInvalidos?.Invoke(camposInvalidos);

            string mensaje = ObtenerMensajeError(camposInvalidos);
            _avisoServicio.Mostrar(mensaje);
        }

        private string ObtenerMensajeError(List<string> camposInvalidos)
        {
            if (camposInvalidos == null || camposInvalidos.Count == 0)
            {
                return Lang.errorTextoCamposInvalidosGenerico;
            }

            if (camposInvalidos.Count == 1)
            {
                return ValidarCampoIndividual(camposInvalidos[0]);
            }

            return Lang.errorTextoCamposInvalidosGenerico;
        }

        private string ValidarCampoIndividual(string campo)
        {
            if (campo == nameof(Nombre))
            {
                return ValidadorEntrada.ValidarNombre(Nombre?.Trim())?.Mensaje 
                    ?? Lang.errorTextoCamposInvalidosGenerico;
            }
            
            if (campo == nameof(Apellido))
            {
                return ValidadorEntrada.ValidarApellido(Apellido?.Trim())?.Mensaje 
                    ?? Lang.errorTextoCamposInvalidosGenerico;
            }

            return Lang.errorTextoCamposInvalidosGenerico;
        }

        private DTOs.ActualizacionPerfilDTO CrearSolicitudActualizacion()
        {
            return new DTOs.ActualizacionPerfilDTO
            {
                UsuarioId = _usuarioId,
                Nombre = Nombre.Trim(),
                Apellido = Apellido.Trim(),
                AvatarId = AvatarSeleccionadoId,
                Instagram = ObtenerIdentificador(RedSocialInstagram),
                Facebook = ObtenerIdentificador(RedSocialFacebook),
                X = ObtenerIdentificador(RedSocialX),
                Discord = ObtenerIdentificador(RedSocialDiscord)
            };
        }

        private async Task<DTOs.ResultadoOperacionDTO> 
            EnviarActualizacionAsync(DTOs.ActualizacionPerfilDTO solicitud)
        {
            _logger.InfoFormat("Guardando cambios de perfil para usuario ID: {0}",
                _usuarioId);
            
            return await _perfilServicio
                .ActualizarPerfilAsync(solicitud)
                .ConfigureAwait(true);
        }

        private void ProcesarResultadoActualizacion(
            DTOs.ResultadoOperacionDTO resultado)
        {
            if (resultado == null)
            {
                _logger.Error(
                    "El servicio de actualizacion de perfil devolvio null.");
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(Lang.errorTextoServidorActualizarPerfil);
                return;
            }

            string mensajeResultado = _localizador.Localizar(
                resultado.Mensaje,
                resultado.OperacionExitosa
                    ? Lang.avisoTextoPerfilActualizado
                    : Lang.errorTextoActualizarPerfil);

            _avisoServicio.Mostrar(mensajeResultado);

            if (resultado.OperacionExitosa)
            {
                _sonidoManejador.ReproducirNotificacion();
                ActualizarSesion();
            }
            else
            {
                _logger.WarnFormat("Error al guardar perfil: {0}",
                    resultado.Mensaje);
            }
        }

        private (bool EsValido, string MensajeError, List<string> CamposInvalidos)
            ValidarCamposPrincipales()
        {
            var camposInvalidos = new List<string>();
            string primerError = null;

            ValidarCampo(
                ValidadorEntrada.ValidarNombre(Nombre?.Trim()),
                nameof(Nombre),
                camposInvalidos,
                ref primerError);

            ValidarCampo(
                ValidadorEntrada.ValidarApellido(Apellido?.Trim()),
                nameof(Apellido),
                camposInvalidos,
                ref primerError);

            if (AvatarSeleccionadoId <= 0)
            {
                camposInvalidos.Add("Avatar");
                primerError ??= Lang.errorTextoSeleccionAvatarValido;
            }

            return (camposInvalidos.Count == 0, primerError, camposInvalidos);
        }

        private static void ValidarCampo(
            DTOs.ResultadoOperacionDTO resultado,
            string nombreCampo,
            List<string> invalidos,
            ref string primerError)
        {
            if (resultado?.OperacionExitosa != true)
            {
                invalidos.Add(nombreCampo);
                primerError ??= resultado?.Mensaje;
            }
        }

        private (bool EsValido, string MensajeError) ValidarRedesSociales()
        {
            string primerMensaje = null;
            bool algunaInvalida = false;

            foreach (RedSocialItemVistaModelo item in RedesSociales)
            {
                string valor = item.Identificador;
                if (string.IsNullOrWhiteSpace(valor))
                {
                    item.TieneError = false;
                    continue;
                }

                string normalizado = valor.Trim();
                if (normalizado.Length > LongitudMaximaRedSocial)
                {
                    item.TieneError = true;
                    algunaInvalida = true;
                    primerMensaje ??= string.Format(
                            CultureInfo.CurrentCulture,
                            Lang.errorTextoIdentificadorRedSocialLongitud,
                            item.Nombre,
                            LongitudMaximaRedSocial);
                }
                else
                {
                    item.TieneError = false;
                }
            }
            return (!algunaInvalida, primerMensaje);
        }

        private async Task CambiarContrasenaAsync()
        {
            if (!ValidarCorreoParaCambioContrasena())
            {
                return;
            }

            EstaProcesando = true;
            EstaCambiandoContrasena = true;

            await EjecutarOperacionAsync(async () =>
            {
                _logger.InfoFormat(
                    "Iniciando solicitud de cambio de contrasena para: {0}",
                    Correo);
                
                DTOs.ResultadoOperacionDTO resultado = 
                    await SolicitarCambioContrasenaAsync();

                ProcesarResultadoCambioContrasena(resultado);
            },
            ex =>
            {
                _logger.Error("Excepcion al cambiar contrasena.", ex);
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(ex.Message ?? 
                    Lang.errorTextoIniciarCambioContrasena);
                EstaCambiandoContrasena = false;
                EstaProcesando = false;
            });

            EstaCambiandoContrasena = false;
            EstaProcesando = false;
        }

        private bool ValidarCorreoParaCambioContrasena()
        {
            if (string.IsNullOrWhiteSpace(Correo))
            {
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(Lang.errorTextoIniciarCambioContrasena);
                return false;
            }

            return true;
        }

        private async Task<DTOs.ResultadoOperacionDTO> 
            SolicitarCambioContrasenaAsync()
        {
            return await _recuperacionCuentaDialogoServicio
                .RecuperarCuentaAsync(
                    Correo,
                    _cambioContrasenaServicio)
                .ConfigureAwait(true);
        }

        private void ProcesarResultadoCambioContrasena(
            DTOs.ResultadoOperacionDTO resultado)
        {
            if (resultado?.OperacionExitosa == false &&
                !string.IsNullOrWhiteSpace(resultado.Mensaje))
            {
                _logger.WarnFormat("Error en cambio de contrasena: {0}",
                    resultado.Mensaje);
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(resultado.Mensaje);
            }
            else if (resultado?.OperacionExitosa == true)
            {
                _logger.Info("Cambio de contrasena finalizado correctamente.");
                _sonidoManejador.ReproducirNotificacion();
                FinalizarSesionPorCambioContrasena();
            }
        }

        private void FinalizarSesionPorCambioContrasena()
        {
            _avisoServicio.Mostrar(Lang.avisoTextoReinicioSesion);
            _usuarioSesion.Limpiar();
            RequiereReinicioSesion = true;
            SolicitarReinicioSesion?.Invoke();
            _ventana.CerrarVentana(this);
        }

        private void AplicarPerfil(DTOs.UsuarioDTO perfil)
        {
            _usuarioId = perfil.UsuarioId;
            Usuario = perfil.NombreUsuario;
            Correo = perfil.Correo;
            Nombre = perfil.Nombre;
            Apellido = perfil.Apellido;

            EstablecerAvatarPorId(perfil.AvatarId);

            EstablecerIdentificador(RedSocialInstagram, perfil.Instagram);
            EstablecerIdentificador(RedSocialFacebook, perfil.Facebook);
            EstablecerIdentificador(RedSocialX, perfil.X);
            EstablecerIdentificador(RedSocialDiscord, perfil.Discord);

            ActualizarSesion(perfil);
        }

        private void EstablecerAvatarPorId(int avatarId)
        {
            var avatares = _catalogoAvatares.ObtenerAvatares();
            ObjetoAvatar avatar = _catalogoAvatares.ObtenerPorId(avatarId);

            if (avatar == null && avatares != null && avatares.Count > 0)
            {
                avatar = avatares[0];
            }

            if (avatar != null)
            {
                EstablecerAvatar(avatar);
            }
        }
        private void EstablecerAvatar(ObjetoAvatar avatar)
        {
            if (avatar == null)
            {
                return;
            }

            AvatarSeleccionadoNombre = avatar.Nombre;
            AvatarSeleccionadoId = avatar.Id;
            AvatarSeleccionadoImagen = avatar.Imagen;
        }

        private void EstablecerIdentificador(string redSocial, string valor)
        {
            if (_redesPorNombre.TryGetValue(redSocial, out RedSocialItemVistaModelo item))
            {
                item.Identificador = valor;
                item.TieneError = false;
            }
        }
        private string ObtenerIdentificador(string redSocial)
        {
            if (_redesPorNombre.TryGetValue(redSocial, out RedSocialItemVistaModelo item))
            {
                string valor = item.Identificador?.Trim();
                return string.IsNullOrWhiteSpace(valor) ? null : valor;
            }
            return null;
        }


        private void LimpiarErroresRedesSociales()
        {
            foreach (RedSocialItemVistaModelo redSocial in RedesSociales)
            {
                redSocial.TieneError = false;
            }
        }

        private ObservableCollection<RedSocialItemVistaModelo> CrearRedesSociales()
        {
            return new ObservableCollection<RedSocialItemVistaModelo>
            {
                CrearRedSocial(RedSocialInstagram),
                CrearRedSocial(RedSocialFacebook),
                CrearRedSocial(RedSocialX),
                CrearRedSocial(RedSocialDiscord)
            };
        }
        private RedSocialItemVistaModelo CrearRedSocial(string nombre)
        {
            ImageSource icono = _catalogoPerfil.ObtenerIconoRedSocial(nombre);
            return new RedSocialItemVistaModelo(nombre, icono);
        }

        private void ActualizarSesion()
        {
            if (_usuarioSesion == null || _usuarioSesion.IdUsuario <= 0)
            {
                return;
            }

            var dto = new DTOs.UsuarioDTO
            {
                UsuarioId = _usuarioId,
                JugadorId = _usuarioSesion.JugadorId,
                NombreUsuario = Usuario,
                Nombre = Nombre?.Trim(),
                Apellido = Apellido?.Trim(),
                Correo = Correo,
                AvatarId = AvatarSeleccionadoId,
                Instagram = ObtenerIdentificador(RedSocialInstagram),
                Facebook = ObtenerIdentificador(RedSocialFacebook),
                X = ObtenerIdentificador(RedSocialX),
                Discord = ObtenerIdentificador(RedSocialDiscord)
            };
            _usuarioSesion.CargarDesdeDTO(dto);
        }
        private void ActualizarSesion(DTOs.UsuarioDTO perfil)
        {
            if (perfil == null)
            {
                return;
            }

            _usuarioSesion.CargarDesdeDTO(perfil);
        }

        public class RedSocialItemVistaModelo
        {
            private string _identificador;
            private bool _tieneError;

            public RedSocialItemVistaModelo(string nombre, ImageSource icono)
            {
                Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
                RutaIcono = icono;
            }

            public string Nombre { get; }

            public ImageSource RutaIcono { get; }

            public string Identificador
            {
                get => _identificador;
                set => _identificador = value;
            }

            public bool TieneError
            {
                get => _tieneError;
                set => _tieneError = value;
            }
        }
    }
}