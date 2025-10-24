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
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class PerfilVistaModelo : BaseVistaModelo
    {
        private const int LongitudMaximaRedSocial = 50;
        private readonly IPerfilServicio _perfilServicio;
        private readonly ISeleccionarAvatarServicio _seleccionarAvatarServicio;
        private readonly ICambioContrasenaServicio _cambioContrasenaServicio;
        private readonly IRecuperacionCuentaServicio _recuperacionCuentaDialogoServicio;
        private readonly IAvatarServicio _avatarServicio;

        private readonly Dictionary<string, RedSocialItem> _redesPorNombre;

        private int _usuarioId;
        private string _usuario;
        private string _correo;
        private string _nombre;
        private string _apellido;
        private string _avatarSeleccionadoNombre;
        private string _avatarSeleccionadoRutaRelativa;
        private ImageSource _avatarSeleccionadoImagen;
        private bool _estaProcesando;
        private bool _estaCambiandoContrasena;

        public PerfilVistaModelo(
            IPerfilServicio perfilServicio,
            ISeleccionarAvatarServicio seleccionarAvatarServicio,
            ICambioContrasenaServicio cambioContrasenaServicio,
            IRecuperacionCuentaServicio recuperacionCuentaDialogoServicio,
            IAvatarServicio avatarServicio)
        {
            _perfilServicio = perfilServicio ?? throw new ArgumentNullException(nameof(perfilServicio));
            _seleccionarAvatarServicio = seleccionarAvatarServicio ?? throw new ArgumentNullException(nameof(seleccionarAvatarServicio));
            _cambioContrasenaServicio = cambioContrasenaServicio ?? throw new ArgumentNullException(nameof(cambioContrasenaServicio));
            _recuperacionCuentaDialogoServicio = recuperacionCuentaDialogoServicio ?? throw new ArgumentNullException(nameof(recuperacionCuentaDialogoServicio));
            _avatarServicio = avatarServicio ?? throw new ArgumentNullException(nameof(avatarServicio));

            RedesSociales = CrearRedesSociales();
            _redesPorNombre = RedesSociales.ToDictionary(r => r.Nombre, StringComparer.OrdinalIgnoreCase);

            GuardarCambiosComando = new ComandoAsincrono(_ => GuardarCambiosAsync(), _ => !EstaProcesando);
            SeleccionarAvatarComando = new ComandoAsincrono(_ => SeleccionarAvatarAsync(), _ => !EstaProcesando);
            CambiarContrasenaComando = new ComandoAsincrono(_ => CambiarContrasenaAsync(), _ => !EstaProcesando && !EstaCambiandoContrasena);
            CerrarComando = new ComandoDelegado(_ => CerrarAccion?.Invoke());
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

        public string AvatarSeleccionadoRutaRelativa
        {
            get => _avatarSeleccionadoRutaRelativa;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoRutaRelativa, value);
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
            UsuarioAutenticado sesion = SesionUsuarioActual.Instancia.Usuario;

            if (sesion == null || sesion.IdUsuario <= 0)
            {
                AvisoAyudante.Mostrar(Lang.errorTextoPerfilActualizarInformacion);
                CerrarAccion?.Invoke();
                return;
            }

            EstaProcesando = true;

            try
            {
                await CargarCatalogoAvataresAsync().ConfigureAwait(true);

                DTOs.UsuarioDTO perfil = await _perfilServicio
                    .ObtenerPerfilAsync(sesion.IdUsuario).ConfigureAwait(true);

                if (perfil == null)
                {
                    AvisoAyudante.Mostrar(Lang.errorTextoServidorObtenerPerfil);
                    return;
                }

                AplicarPerfil(perfil);
            }
            catch (ExcepcionServicio ex)
            {
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
                .SeleccionarAvatarAsync(AvatarSeleccionadoRutaRelativa).ConfigureAwait(true);

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

            bool nombreVacio = string.IsNullOrWhiteSpace(nombre);
            bool apellidoVacio = string.IsNullOrWhiteSpace(apellido);

            if (nombreVacio || apellidoVacio)
            {
                if (nombreVacio) camposInvalidos.Add(nameof(Nombre));
                if (apellidoVacio) camposInvalidos.Add(nameof(Apellido));

                MostrarCamposInvalidos?.Invoke(camposInvalidos);
                AvisoAyudante.Mostrar(Lang.errorTextoCamposInvalidosGenerico);
                return;
            }

            string mensajeError = null;

            DTOs.ResultadoOperacionDTO validacionNombre = ValidacionEntrada.ValidarNombre(nombre);
            if (validacionNombre?.OperacionExitosa != true)
            {
                camposInvalidos.Add(nameof(Nombre));
                if (mensajeError == null)
                    mensajeError = validacionNombre?.Mensaje;
            }

            DTOs.ResultadoOperacionDTO validacionApellido = ValidacionEntrada.ValidarApellido(apellido);
            if (validacionApellido?.OperacionExitosa != true)
            {
                camposInvalidos.Add(nameof(Apellido));
                if (mensajeError == null)
                    mensajeError = validacionApellido?.Mensaje;
            }

            if (string.IsNullOrWhiteSpace(AvatarSeleccionadoRutaRelativa))
            {
                camposInvalidos.Add(nameof(AvatarSeleccionadoRutaRelativa));
                if (mensajeError == null)
                    mensajeError = Lang.errorTextoSeleccionAvatarValido;
            }

            DTOs.ResultadoOperacionDTO validacionRedes = ValidarRedesSociales();
            if (validacionRedes?.OperacionExitosa != true)
            {
                camposInvalidos.Add("RedesSociales");
                if (mensajeError == null)
                    mensajeError = validacionRedes?.Mensaje;
            }

            if (camposInvalidos.Count > 0)
            {
                if (camposInvalidos.Count > 1)
                {
                    mensajeError = Lang.errorTextoCamposInvalidosGenerico;
                }

                MostrarCamposInvalidos?.Invoke(camposInvalidos);
                AvisoAyudante.Mostrar(mensajeError ?? Lang.errorTextoCamposInvalidosGenerico);
                return;
            }

            var solicitud = new DTOs.ActualizacionPerfilDTO
            {
                UsuarioId = _usuarioId,
                Nombre = nombre,
                Apellido = apellido,
                AvatarRutaRelativa = AvatarSeleccionadoRutaRelativa,
                Instagram = ObtenerIdentificador("Instagram"),
                Facebook = ObtenerIdentificador("Facebook"),
                X = ObtenerIdentificador("X"),
                Discord = ObtenerIdentificador("Discord")
            };

            EstaProcesando = true;

            try
            {
                DTOs.ResultadoOperacionDTO resultado = await _perfilServicio
                    .ActualizarPerfilAsync(solicitud).ConfigureAwait(true);

                if (resultado == null)
                {
                    AvisoAyudante.Mostrar(Lang.errorTextoServidorActualizarPerfil);
                    return;
                }

                if (!resultado.OperacionExitosa)
                {
                    mensajeError = MensajeServidorAyudante.Localizar(
                        resultado.Mensaje,
                        Lang.errorTextoActualizarPerfil);
                    AvisoAyudante.Mostrar(mensajeError);
                    return;
                }

                ActualizarSesion();
                string mensajeExito = MensajeServidorAyudante.Localizar(
                    resultado.Mensaje,
                    Lang.avisoTextoPerfilActualizado);
                AvisoAyudante.Mostrar(mensajeExito);
            }
            catch (ExcepcionServicio ex)
            {
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoServidorActualizarPerfil);
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
                    AvisoAyudante.Mostrar(resultado.Mensaje);
                }
            }
            catch (ExcepcionServicio ex)
            {
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

            EstablecerAvatarDesdeRuta(perfil.AvatarRutaRelativa, perfil.AvatarId);

            EstablecerIdentificador("Instagram", perfil.Instagram);
            EstablecerIdentificador("Facebook", perfil.Facebook);
            EstablecerIdentificador("X", perfil.X);
            EstablecerIdentificador("Discord", perfil.Discord);

            ActualizarSesion(perfil);
        }

        private void EstablecerAvatarDesdeRuta(string rutaRelativa, int avatarId)
        {
            ObjetoAvatar avatar = AvatarAyudante.ObtenerAvatarPorRuta(rutaRelativa)
                ?? AvatarAyudante.ObtenerAvatarPorId(avatarId)
                ?? AvatarAyudante.ObtenerAvatarPredeterminado();

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
            AvatarSeleccionadoRutaRelativa = avatar.RutaRelativa;
            AvatarSeleccionadoImagen = AvatarAyudante.ObtenerImagen(avatar);
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

        private DTOs.ResultadoOperacionDTO ValidarRedesSociales()
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

                    if (mensaje == null)
                    {
                        mensaje = string.Format(
                            CultureInfo.CurrentCulture,
                            Lang.errorTextoIdentificadorRedSocialLongitud,
                            item.Nombre,
                            LongitudMaximaRedSocial);
                    }
                }
            }

            return new DTOs.ResultadoOperacionDTO
            {
                OperacionExitosa = !algunaInvalida,
                Mensaje = algunaInvalida ? mensaje ?? Lang.errorTextoIdentificadorRedSocialLongitud : null
            };
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
            UsuarioAutenticado sesion = SesionUsuarioActual.Instancia.Usuario;
            if (sesion == null || sesion.IdUsuario <= 0)
            {
                return;
            }

            string rutaRelativa = string.IsNullOrWhiteSpace(AvatarSeleccionadoRutaRelativa)
                ? sesion.AvatarRutaRelativa
                : AvatarSeleccionadoRutaRelativa;

            ObjetoAvatar avatar = AvatarAyudante.ObtenerAvatarPorRuta(rutaRelativa);
            string nombreActual = string.IsNullOrWhiteSpace(Nombre)
                ? sesion.Nombre
                : Nombre.Trim();
            string apellidoActual = string.IsNullOrWhiteSpace(Apellido)
                ? sesion.Apellido
                : Apellido.Trim();

            var dto = new DTOs.UsuarioDTO
            {
                UsuarioId = sesion.IdUsuario,
                JugadorId = sesion.JugadorId,
                NombreUsuario = sesion.NombreUsuario,
                Nombre = nombreActual,
                Apellido = apellidoActual,
                Correo = sesion.Correo,
                AvatarId = avatar?.Id ?? sesion.AvatarId,
                AvatarRutaRelativa = rutaRelativa,
                Instagram = ObtenerIdentificador("Instagram"),
                Facebook = ObtenerIdentificador("Facebook"),
                X = ObtenerIdentificador("X"),
                Discord = ObtenerIdentificador("Discord")
            };

            SesionUsuarioActual.Instancia.EstablecerUsuario(dto);
        }

        private void ActualizarSesion(DTOs.UsuarioDTO perfil)
        {
            if (perfil == null)
            {
                return;
            }

            SesionUsuarioActual.Instancia.EstablecerUsuario(perfil);
        }

        private async Task CargarCatalogoAvataresAsync()
        {
            try
            {
                IReadOnlyList<ObjetoAvatar> avatares = await _avatarServicio.ObtenerCatalogoAsync()
                    .ConfigureAwait(true);

                if (avatares != null && avatares.Count > 0)
                {
                    AvatarAyudante.ActualizarCatalogo(avatares);
                }
            }
            catch (ExcepcionServicio ex)
            {
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoServidorInformacionAvatar);
            }
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
