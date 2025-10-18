using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class PerfilVistaModelo : BaseVistaModelo
    {
        private const int LongitudMaximaRedSocial = 50;
        private readonly IPerfilService _perfilService;
        private readonly ISeleccionarAvatarService _seleccionarAvatarService;
        private readonly ICambioContrasenaService _cambioContrasenaService;
        private readonly IRecuperacionCuentaDialogService _recuperacionCuentaDialogService;

        private readonly Dictionary<string, RedSocialItem> _redesPorNombre;

        private int _usuarioId;
        private string _usuario;
        private string _correo;
        private string _nombre;
        private string _apellido;
        private int _avatarSeleccionadoId;
        private string _avatarSeleccionadoNombre;
        private ImageSource _avatarSeleccionadoImagen;
        private bool _estaProcesando;
        private bool _estaCambiandoContrasena;

        public PerfilVistaModelo(
            IPerfilService perfilService,
            ISeleccionarAvatarService seleccionarAvatarService,
            ICambioContrasenaService cambioContrasenaService,
            IRecuperacionCuentaDialogService recuperacionCuentaDialogService)
        {
            _perfilService = perfilService ?? throw new ArgumentNullException(nameof(perfilService));
            _seleccionarAvatarService = seleccionarAvatarService ?? throw new ArgumentNullException(nameof(seleccionarAvatarService));
            _cambioContrasenaService = cambioContrasenaService ?? throw new ArgumentNullException(nameof(cambioContrasenaService));
            _recuperacionCuentaDialogService = recuperacionCuentaDialogService ?? throw new ArgumentNullException(nameof(recuperacionCuentaDialogService));

            RedesSociales = CrearRedesSociales();
            _redesPorNombre = RedesSociales.ToDictionary(r => r.Nombre, StringComparer.OrdinalIgnoreCase);

            GuardarCambiosCommand = new ComandoAsincrono(_ => GuardarCambiosAsync(), _ => !EstaProcesando);
            SeleccionarAvatarCommand = new ComandoAsincrono(_ => SeleccionarAvatarAsync(), _ => !EstaProcesando);
            CambiarContrasenaCommand = new ComandoAsincrono(_ => CambiarContrasenaAsync(), _ => !EstaProcesando && !EstaCambiandoContrasena);
            CerrarCommand = new ComandoDelegado(_ => CerrarAccion?.Invoke());
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

        public int AvatarSeleccionadoId
        {
            get => _avatarSeleccionadoId;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoId, value);
        }

        public string AvatarSeleccionadoNombre
        {
            get => _avatarSeleccionadoNombre;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoNombre, value);
        }

        public ImageSource AvatarSeleccionadoImagen
        {
            get => _avatarSeleccionadoImagen;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoImagen, value);
        }

        public ObservableCollection<RedSocialItem> RedesSociales { get; }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ((IComandoNotificable)GuardarCambiosCommand).NotificarPuedeEjecutar();
                    ((IComandoNotificable)SeleccionarAvatarCommand).NotificarPuedeEjecutar();
                    ((IComandoNotificable)CambiarContrasenaCommand).NotificarPuedeEjecutar();
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
                    ((IComandoNotificable)CambiarContrasenaCommand).NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono GuardarCambiosCommand { get; }

        public IComandoAsincrono SeleccionarAvatarCommand { get; }

        public IComandoAsincrono CambiarContrasenaCommand { get; }

        public ICommand CerrarCommand { get; }

        public Action CerrarAccion { get; set; }

        public Action<IList<string>> MostrarCamposInvalidos { get; set; }

        public async Task CargarPerfilAsync()
        {
            UsuarioSesion sesion = SesionUsuarioActual.Instancia.Usuario;

            if (sesion == null)
            {
                AvisoHelper.Mostrar(Lang.errorTextoPerfilActualizarInformacion);
                CerrarAccion?.Invoke();
                return;
            }

            EstaProcesando = true;

            try
            {
                UsuarioAutenticado perfil = await _perfilService
                    .ObtenerPerfilAsync(sesion.IdUsuario).ConfigureAwait(true);

                if (perfil == null)
                {
                    AvisoHelper.Mostrar(Lang.errorTextoServidorObtenerPerfil);
                    return;
                }

                AplicarPerfil(perfil);
            }
            catch (ServicioException ex)
            {
                AvisoHelper.Mostrar(ex.Message ?? Lang.errorTextoServidorObtenerPerfil);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private async Task SeleccionarAvatarAsync()
        {
            ObjetoAvatar avatar = await _seleccionarAvatarService
                .SeleccionarAvatarAsync(AvatarSeleccionadoId).ConfigureAwait(true);

            if (avatar == null)
            {
                return;
            }

            EstablecerAvatar(avatar);
        }

        private async Task GuardarCambiosAsync()
        {
            string nombre = Nombre?.Trim();
            string apellido = Apellido?.Trim();

            MostrarCamposInvalidos?.Invoke(Array.Empty<string>());
            LimpiarErroresRedesSociales();

            var camposInvalidos = new List<string>();
            string mensajeError = null;

            ResultadoOperacion validacionNombre = ValidacionEntradaHelper.ValidarNombre(nombre);
            if (!validacionNombre.Exito)
            {
                camposInvalidos.Add(nameof(Nombre));
                mensajeError ??= validacionNombre.Mensaje;
            }

            ResultadoOperacion validacionApellido = ValidacionEntradaHelper.ValidarApellido(apellido);
            if (!validacionApellido.Exito)
            {
                camposInvalidos.Add(nameof(Apellido));
                mensajeError ??= validacionApellido.Mensaje;
            }

            if (AvatarSeleccionadoId <= 0)
            {
                camposInvalidos.Add(nameof(AvatarSeleccionadoId));
                mensajeError ??= Lang.errorTextoSeleccionAvatarValido;
            }

            ResultadoOperacion validacionRedes = ValidarRedesSociales();
            if (!validacionRedes.Exito)
            {
                camposInvalidos.Add("RedesSociales");
                mensajeError ??= validacionRedes.Mensaje;
            }

            if (camposInvalidos.Count > 0)
            {
                if (camposInvalidos.Count > 1)
                {
                    mensajeError = Lang.errorTextoCamposInvalidosGenerico;
                }

                MostrarCamposInvalidos?.Invoke(camposInvalidos);
                AvisoHelper.Mostrar(mensajeError ?? Lang.errorTextoCamposInvalidosGenerico);
                return;
            }

            var solicitud = new ActualizarPerfilSolicitud
            {
                UsuarioId = _usuarioId,
                Nombre = nombre,
                Apellido = apellido,
                AvatarId = AvatarSeleccionadoId,
                Instagram = ObtenerIdentificador("Instagram"),
                Facebook = ObtenerIdentificador("Facebook"),
                X = ObtenerIdentificador("X"),
                Discord = ObtenerIdentificador("Discord")
            };

            EstaProcesando = true;

            try
            {
                ResultadoOperacion resultado = await _perfilService
                    .ActualizarPerfilAsync(solicitud).ConfigureAwait(true);

                if (resultado == null)
                {
                    AvisoHelper.Mostrar(Lang.errorTextoServidorActualizarPerfil);
                    return;
                }

                if (!resultado.Exito)
                {
                    AvisoHelper.Mostrar(resultado.Mensaje ?? Lang.errorTextoActualizarPerfil);
                    return;
                }

                ActualizarSesion();
                AvisoHelper.Mostrar(resultado.Mensaje ?? Lang.avisoTextoPerfilActualizado);
            }
            catch (ServicioException ex)
            {
                AvisoHelper.Mostrar(ex.Message ?? Lang.errorTextoServidorActualizarPerfil);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private async Task CambiarContrasenaAsync()
        {
            if (string.IsNullOrWhiteSpace(Correo))
            {
                AvisoHelper.Mostrar(Lang.errorTextoIniciarCambioContrasena);
                return;
            }

            EstaProcesando = true;
            EstaCambiandoContrasena = true;

            try
            {
                ResultadoOperacion resultado = await _recuperacionCuentaDialogService
                    .RecuperarCuentaAsync(Correo, _cambioContrasenaService).ConfigureAwait(true);

                if (resultado?.Exito == false && !string.IsNullOrWhiteSpace(resultado.Mensaje))
                {
                    AvisoHelper.Mostrar(resultado.Mensaje);
                }
            }
            catch (ServicioException ex)
            {
                AvisoHelper.Mostrar(ex.Message ?? Lang.errorTextoIniciarCambioContrasena);
            }
            finally
            {
                EstaCambiandoContrasena = false;
                EstaProcesando = false;
            }
        }

        private void AplicarPerfil(UsuarioAutenticado perfil)
        {
            _usuarioId = perfil.IdUsuario;
            Usuario = perfil.NombreUsuario;
            Correo = perfil.Correo;
            Nombre = perfil.Nombre;
            Apellido = perfil.Apellido;

            EstablecerAvatarDesdeId(perfil.AvatarId);

            EstablecerIdentificador("Instagram", perfil.Instagram);
            EstablecerIdentificador("Facebook", perfil.Facebook);
            EstablecerIdentificador("X", perfil.X);
            EstablecerIdentificador("Discord", perfil.Discord);

            ActualizarSesion(perfil);
        }

        private void EstablecerAvatarDesdeId(int avatarId)
        {
            ObjetoAvatar avatar = AvatarHelper.ObtenerAvatarPorId(avatarId)
                ?? AvatarHelper.ObtenerAvatarPredeterminado();

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

            AvatarSeleccionadoId = avatar.Id;
            AvatarSeleccionadoNombre = avatar.Nombre;
            AvatarSeleccionadoImagen = avatar.Imagen;
        }

        private void EstablecerIdentificador(string redSocial, string valor)
        {
            if (_redesPorNombre.TryGetValue(redSocial, out RedSocialItem item))
            {
                item.Identificador = valor;
            }
        }

        private string ObtenerIdentificador(string redSocial)
        {
            if (_redesPorNombre.TryGetValue(redSocial, out RedSocialItem item))
            {
                string valor = item.Identificador?.Trim();
                return string.IsNullOrWhiteSpace(valor) ? null : valor;
            }

            return null;
        }

        private ResultadoOperacion ValidarRedesSociales()
        {
            bool algunaInvalida = false;
            string mensaje = null;

            foreach (RedSocialItem item in RedesSociales)
            {
                string valor = item.Identificador;
                if (string.IsNullOrWhiteSpace(valor))
                {
                    continue;
                }

                string normalizado = valor.Trim();
                if (normalizado.Length > LongitudMaximaRedSocial)
                {
                    item.TieneError = true;
                    algunaInvalida = true;

                    mensaje ??= string.Format(
                        CultureInfo.CurrentCulture,
                        Lang.errorTextoIdentificadorRedSocialLongitud,
                        item.Nombre,
                        LongitudMaximaRedSocial);
                }
            }

            return algunaInvalida
                ? ResultadoOperacion.Fallo(mensaje ?? Lang.errorTextoIdentificadorRedSocialLongitud)
                : ResultadoOperacion.Exitoso();
        }

        private void LimpiarErroresRedesSociales()
        {
            foreach (RedSocialItem redSocial in RedesSociales)
            {
                redSocial.TieneError = false;
            }
        }

        private ObservableCollection<RedSocialItem> CrearRedesSociales()
        {
            return new ObservableCollection<RedSocialItem>
            {
                CrearRedSocial("Instagram"),
                CrearRedSocial("Facebook"),
                CrearRedSocial("X"),
                CrearRedSocial("Discord")
            };
        }

        private static RedSocialItem CrearRedSocial(string nombre)
        {
            ImageSource icono = CatalogoImagenesPerfilLocales.ObtenerIconoRedSocial(nombre);
            return new RedSocialItem(nombre, icono);
        }

        private void ActualizarSesion()
        {
            UsuarioSesion sesion = SesionUsuarioActual.Instancia.Usuario;
            if (sesion == null)
            {
                return;
            }

            sesion.Nombre = Nombre;
            sesion.Apellido = Apellido;
            sesion.AvatarId = AvatarSeleccionadoId;
        }

        private void ActualizarSesion(UsuarioAutenticado perfil)
        {
            var sesion = new UsuarioSesion
            {
                IdUsuario = perfil.IdUsuario,
                JugadorId = perfil.JugadorId,
                NombreUsuario = perfil.NombreUsuario,
                Nombre = perfil.Nombre,
                Apellido = perfil.Apellido,
                Correo = perfil.Correo,
                AvatarId = perfil.AvatarId
            };

            SesionUsuarioActual.Instancia.EstablecerUsuario(sesion);
        }

        public class RedSocialItem : BaseVistaModelo
        {
            private string _identificador;
            private bool _tieneError;

            public RedSocialItem(string nombre, ImageSource icono)
            {
                Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
                RutaIcono = icono;
            }

            public string Nombre { get; }

            public ImageSource RutaIcono { get; }

            public string Identificador
            {
                get => _identificador;
                set => EstablecerPropiedad(ref _identificador, value);
            }

            public bool TieneError
            {
                get => _tieneError;
                set => EstablecerPropiedad(ref _tieneError, value);
            }
        }
    }
}
