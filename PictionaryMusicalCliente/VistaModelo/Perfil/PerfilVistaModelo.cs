using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo; 
using PictionaryMusicalCliente.Modelo.Catalogos; 
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante; 
using PictionaryMusicalCliente.Sesiones; 
using PictionaryMusicalCliente.Utilidades; 
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
        private const string RedSocialInstagram = "Instagram";
        private const string RedSocialFacebook = "Facebook";
        private const string RedSocialX = "X";
        private const string RedSocialDiscord = "Discord";

        private const int LongitudMaximaRedSocial = 50;
        private readonly IPerfilServicio _perfilServicio;
        private readonly ISeleccionarAvatarServicio _seleccionarAvatarServicio;
        private readonly ICambioContrasenaServicio _cambioContrasenaServicio;
        private readonly IRecuperacionCuentaServicio _recuperacionCuentaDialogoServicio;

        private readonly Dictionary<string, RedSocialItemVistaModelo> _redesPorNombre;

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
            IPerfilServicio perfilServicio,
            ISeleccionarAvatarServicio seleccionarAvatarServicio,
            ICambioContrasenaServicio cambioContrasenaServicio,
            IRecuperacionCuentaServicio recuperacionCuentaDialogoServicio)
        {
            _perfilServicio = perfilServicio ?? throw new ArgumentNullException(nameof(perfilServicio));
            _seleccionarAvatarServicio = seleccionarAvatarServicio ?? throw new ArgumentNullException(nameof(seleccionarAvatarServicio));
            _cambioContrasenaServicio = cambioContrasenaServicio ?? throw new ArgumentNullException(nameof(cambioContrasenaServicio));
            _recuperacionCuentaDialogoServicio = recuperacionCuentaDialogoServicio ?? throw new ArgumentNullException(nameof(recuperacionCuentaDialogoServicio));

            RedesSociales = CrearRedesSociales();
            _redesPorNombre = RedesSociales.ToDictionary(r => r.Nombre, StringComparer.OrdinalIgnoreCase);

            GuardarCambiosComando = new ComandoAsincrono(async _ =>
            {
                ManejadorSonido.ReproducirClick();
                await GuardarCambiosAsync();
            }, _ => !EstaProcesando);

            SeleccionarAvatarComando = new ComandoAsincrono(async _ =>
            {
                ManejadorSonido.ReproducirClick();
                await SeleccionarAvatarAsync();
            }, _ => !EstaProcesando);

            CambiarContrasenaComando = new ComandoAsincrono(async _ =>
            {
                ManejadorSonido.ReproducirClick();
                await CambiarContrasenaAsync();
            }, _ => !EstaProcesando && !EstaCambiandoContrasena);

            CerrarComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                CerrarAccion?.Invoke();
            });
        }

        public string Usuario { get => _usuario; private set => EstablecerPropiedad(ref _usuario, value); }
        public string Correo { get => _correo; private set => EstablecerPropiedad(ref _correo, value); }
        public string Nombre { get => _nombre; set => EstablecerPropiedad(ref _nombre, value); }
        public string Apellido { get => _apellido; set => EstablecerPropiedad(ref _apellido, value); }
        public string AvatarSeleccionadoNombre { get => _avatarSeleccionadoNombre; private set => EstablecerPropiedad(ref _avatarSeleccionadoNombre, value); }
        public int AvatarSeleccionadoId { get => _avatarSeleccionadoId; private set => EstablecerPropiedad(ref _avatarSeleccionadoId, value); }
        public ImageSource AvatarSeleccionadoImagen { get => _avatarSeleccionadoImagen; private set => EstablecerPropiedad(ref _avatarSeleccionadoImagen, value); }
        public ObservableCollection<RedSocialItemVistaModelo> RedesSociales { get; }
        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ((IComandoNotificable)GuardarCambiosComando).NotificarPuedeEjecutar();
                    ((IComandoNotificable)SeleccionarAvatarComando).NotificarPuedeEjecutar();
                    ((IComandoNotificable)CambiarContrasenaComando).NotificarPuedeEjecutar();
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
                    ((IComandoNotificable)CambiarContrasenaComando).NotificarPuedeEjecutar();
                }
            }
        }
        public IComandoAsincrono GuardarCambiosComando { get; }
        public IComandoAsincrono SeleccionarAvatarComando { get; }
        public IComandoAsincrono CambiarContrasenaComando { get; }
        public ICommand CerrarComando { get; }
        public Action CerrarAccion { get; set; }
        public Action<IList<string>> MostrarCamposInvalidos { get; set; }


        public async Task CargarPerfilAsync()
        {
            UsuarioAutenticado sesion = SesionUsuarioActual.Usuario;

            if (sesion == null || sesion.IdUsuario <= 0)
            {
                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(Lang.errorTextoPerfilActualizarInformacion);
                CerrarAccion?.Invoke();
                return;
            }

            EstaProcesando = true;

            try
            {
                DTOs.UsuarioDTO perfil = await _perfilServicio
                    .ObtenerPerfilAsync(sesion.IdUsuario).ConfigureAwait(true);

                if (perfil == null)
                {
                    ManejadorSonido.ReproducirError();
                    AvisoAyudante.Mostrar(Lang.errorTextoServidorObtenerPerfil);
                    return;
                }

                AplicarPerfil(perfil);
            }
            catch (ServicioExcepcion ex)
            {
                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoServidorObtenerPerfil);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private async Task SeleccionarAvatarAsync()
        {
            ObjetoAvatar avatar = await _seleccionarAvatarServicio
                .SeleccionarAvatarAsync(AvatarSeleccionadoId).ConfigureAwait(true);

            if (avatar == null) return; 

            EstablecerAvatar(avatar);
        }

        private async Task GuardarCambiosAsync()
        {
            MostrarCamposInvalidos?.Invoke(Array.Empty<string>());
            LimpiarErroresRedesSociales();

            var (sonCamposValidos, errorCampos, camposInvalidos) = ValidarCamposPrincipales();
            var (sonRedesValidas, errorRedes) = ValidarRedesSociales();

            if (!sonCamposValidos || !sonRedesValidas)
            {
                ManejadorSonido.ReproducirError();
                var todosInvalidos = camposInvalidos ?? Enumerable.Empty<string>();
                if (!sonRedesValidas) todosInvalidos = todosInvalidos.Concat(new[] 
                { 
                    "RedesSociales" 
                });

                MostrarCamposInvalidos?.Invoke(todosInvalidos.ToList());

                string mensajeMostrar = Lang.errorTextoCamposInvalidosGenerico;
                if (todosInvalidos.Count() == 1) 
                {
                    mensajeMostrar = errorCampos ?? errorRedes ?? mensajeMostrar;
                }
                AvisoAyudante.Mostrar(mensajeMostrar);
                return;
            }

            var solicitud = new DTOs.ActualizacionPerfilDTO
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

            EstaProcesando = true;

            try
            {
                DTOs.ResultadoOperacionDTO resultado = await _perfilServicio
                    .ActualizarPerfilAsync(solicitud).ConfigureAwait(true);

                if (resultado == null)
                {
                    ManejadorSonido.ReproducirError();
                    AvisoAyudante.Mostrar(Lang.errorTextoServidorActualizarPerfil);
                    return;
                }

                string mensajeResultado = MensajeServidorAyudante.Localizar(
                    resultado.Mensaje,
                    resultado.OperacionExitosa ? Lang.avisoTextoPerfilActualizado : Lang.errorTextoActualizarPerfil);

                AvisoAyudante.Mostrar(mensajeResultado);

                if (resultado.OperacionExitosa)
                {
                    ManejadorSonido.ReproducirExito();
                    ActualizarSesion(); 
                }
            }
            catch (ServicioExcepcion ex)
            {
                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoServidorActualizarPerfil);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private (bool EsValido, string MensajeError, List<string> CamposInvalidos) ValidarCamposPrincipales()
        {
            var camposInvalidos = new List<string>();
            string primerError = null;

            ValidarCampo(ValidacionEntrada.ValidarNombre(Nombre?.Trim()), nameof(Nombre), camposInvalidos, ref primerError);
            ValidarCampo(ValidacionEntrada.ValidarApellido(Apellido?.Trim()), nameof(Apellido), camposInvalidos, ref primerError);

            if (AvatarSeleccionadoId <= 0)
            {
                camposInvalidos.Add("Avatar");
                primerError ??= Lang.errorTextoSeleccionAvatarValido;
            }

            return (camposInvalidos.Count == 0, primerError, camposInvalidos);
        }

        private static void ValidarCampo(DTOs.ResultadoOperacionDTO resultado, string nombreCampo, List<string> invalidos, ref string primerError)
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
            if (string.IsNullOrWhiteSpace(Correo))
            {
                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(Lang.errorTextoIniciarCambioContrasena);
                return;
            }

            EstaProcesando = true;
            EstaCambiandoContrasena = true;

            try
            {
                DTOs.ResultadoOperacionDTO resultado = await _recuperacionCuentaDialogoServicio
                    .RecuperarCuentaAsync(Correo, _cambioContrasenaServicio).ConfigureAwait(true);

                if (resultado?.OperacionExitosa == false && !string.IsNullOrWhiteSpace(resultado.Mensaje))
                {
                    ManejadorSonido.ReproducirError();
                    AvisoAyudante.Mostrar(resultado.Mensaje);
                }
                else if(resultado?.OperacionExitosa == true)
                {
                    ManejadorSonido.ReproducirExito();
                }
            }
            catch (ServicioExcepcion ex)
            {
                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoIniciarCambioContrasena);
            }
            finally
            {
                EstaCambiandoContrasena = false; 
                EstaProcesando = false;
            }
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
            var avatares = CatalogoAvataresLocales.ObtenerAvatares();
            ObjetoAvatar avatar = CatalogoAvataresLocales.ObtenerPorId(avatarId);

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
            if (avatar == null) return;
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

        private static ObservableCollection<RedSocialItemVistaModelo> CrearRedesSociales()
        {
            return new ObservableCollection<RedSocialItemVistaModelo>
            {
                CrearRedSocial(RedSocialInstagram),
                CrearRedSocial(RedSocialFacebook),
                CrearRedSocial(RedSocialX),
                CrearRedSocial(RedSocialDiscord)
            };
        }
        private static RedSocialItemVistaModelo CrearRedSocial(string nombre)
        {
            ImageSource icono = CatalogoImagenesPerfilLocales.ObtenerIconoRedSocial(nombre);
            return new RedSocialItemVistaModelo(nombre, icono);
        }

        private void ActualizarSesion()
        {
            UsuarioAutenticado sesion = SesionUsuarioActual.Usuario;
            if (sesion == null || sesion.IdUsuario <= 0) return;

            var dto = new DTOs.UsuarioDTO
            {
                UsuarioId = _usuarioId,
                JugadorId = sesion.JugadorId,
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
            SesionUsuarioActual.EstablecerUsuario(dto);
        }
        private static void ActualizarSesion(DTOs.UsuarioDTO perfil)
        {
            if (perfil == null) return;
            SesionUsuarioActual.EstablecerUsuario(perfil);
        }

        public class RedSocialItemVistaModelo(string nombre, ImageSource icono) : BaseVistaModelo
        {
            private string _identificador;
            private bool _tieneError;

            public string Nombre { get; } = nombre ?? throw new ArgumentNullException(nameof(nombre));
            public ImageSource RutaIcono { get; } = icono;
            public string Identificador { get => _identificador; set => EstablecerPropiedad(ref _identificador, value); }
            public bool TieneError { get => _tieneError; set => EstablecerPropiedad(ref _tieneError, value); }
        }
    }
}