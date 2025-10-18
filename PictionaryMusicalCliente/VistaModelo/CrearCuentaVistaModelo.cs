using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class CrearCuentaVistaModelo : BaseVistaModelo
    {
        private readonly ICodigoVerificacionService _codigoVerificacionService;
        private readonly ICuentaService _cuentaService;
        private readonly ISeleccionarAvatarService _seleccionarAvatarService;
        private readonly IVerificarCodigoDialogService _verificarCodigoDialogService;

        private string _usuario;
        private string _nombre;
        private string _apellido;
        private string _correo;
        private string _contrasena;
        private ImageSource _avatarSeleccionadoImagen;
        private string _avatarSeleccionadoRutaRelativa;
        private bool _mostrarErrorUsuario;
        private bool _mostrarErrorCorreo;
        private bool _estaProcesando;

        public CrearCuentaVistaModelo(
            ICodigoVerificacionService codigoVerificacionService,
            ICuentaService cuentaService,
            ISeleccionarAvatarService seleccionarAvatarService,
            IVerificarCodigoDialogService verificarCodigoDialogService)
        {
            _codigoVerificacionService = codigoVerificacionService ?? throw new ArgumentNullException(nameof(codigoVerificacionService));
            _cuentaService = cuentaService ?? throw new ArgumentNullException(nameof(cuentaService));
            _seleccionarAvatarService = seleccionarAvatarService ?? throw new ArgumentNullException(nameof(seleccionarAvatarService));
            _verificarCodigoDialogService = verificarCodigoDialogService ?? throw new ArgumentNullException(nameof(verificarCodigoDialogService));

            CrearCuentaCommand = new ComandoAsincrono(_ => CrearCuentaAsync(), _ => !EstaProcesando);
            CancelarCommand = new ComandoDelegado(Cancelar);
            SeleccionarAvatarCommand = new ComandoAsincrono(_ => SeleccionarAvatarAsync());

            EstablecerAvatarPredeterminado();
        }

        public string Usuario
        {
            get => _usuario;
            set => EstablecerPropiedad(ref _usuario, value);
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

        public string Correo
        {
            get => _correo;
            set => EstablecerPropiedad(ref _correo, value);
        }

        public string Contrasena
        {
            get => _contrasena;
            set => EstablecerPropiedad(ref _contrasena, value);
        }

        public ImageSource AvatarSeleccionadoImagen
        {
            get => _avatarSeleccionadoImagen;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoImagen, value);
        }

        public string AvatarSeleccionadoRutaRelativa
        {
            get => _avatarSeleccionadoRutaRelativa;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoRutaRelativa, value);
        }

        public bool MostrarErrorUsuario
        {
            get => _mostrarErrorUsuario;
            private set => EstablecerPropiedad(ref _mostrarErrorUsuario, value);
        }

        public bool MostrarErrorCorreo
        {
            get => _mostrarErrorCorreo;
            private set => EstablecerPropiedad(ref _mostrarErrorCorreo, value);
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ((IComandoNotificable)CrearCuentaCommand).NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono CrearCuentaCommand { get; }

        public ICommand CancelarCommand { get; }

        public IComandoAsincrono SeleccionarAvatarCommand { get; }

        public Action CerrarAccion { get; set; }

        public Action<IList<string>> MostrarCamposInvalidos { get; set; }

        public Action<string> MostrarMensaje { get; set; }

        private async Task CrearCuentaAsync()
        {
            MostrarErrorUsuario = false;
            MostrarErrorCorreo = false;

            Usuario = Usuario?.Trim();
            Nombre = Nombre?.Trim();
            Apellido = Apellido?.Trim();
            Correo = Correo?.Trim();
            Contrasena = Contrasena?.Trim();

            MostrarCamposInvalidos?.Invoke(Array.Empty<string>());

            IList<string> camposInvalidos = new List<string>();

            string mensajeError = null;

            ResultadoOperacion resultadoUsuario = ValidacionEntradaHelper.ValidarUsuario(Usuario);
            if (!resultadoUsuario.Exito)
            {
                camposInvalidos.Add(nameof(Usuario));
                if (mensajeError == null)
                {
                    mensajeError = resultadoUsuario.Mensaje;
                }
            }

            ResultadoOperacion resultadoNombre = ValidacionEntradaHelper.ValidarNombre(Nombre);
            if (!resultadoNombre.Exito)
            {
                camposInvalidos.Add(nameof(Nombre));
                if (mensajeError == null)
                {
                    mensajeError = resultadoNombre.Mensaje;
                }
            }

            ResultadoOperacion resultadoApellido = ValidacionEntradaHelper.ValidarApellido(Apellido);
            if (!resultadoApellido.Exito)
            {
                camposInvalidos.Add(nameof(Apellido));
                if (mensajeError == null)
                {
                    mensajeError = resultadoApellido.Mensaje;
                }
            }

            ResultadoOperacion resultadoCorreo = ValidacionEntradaHelper.ValidarCorreo(Correo);
            if (!resultadoCorreo.Exito)
            {
                camposInvalidos.Add(nameof(Correo));
                if (mensajeError == null)
                {
                    mensajeError = resultadoCorreo.Mensaje;
                }
            }

            ResultadoOperacion resultadoContrasena = ValidacionEntradaHelper.ValidarContrasena(Contrasena);
            if (!resultadoContrasena.Exito)
            {
                camposInvalidos.Add(nameof(Contrasena));
                if (mensajeError == null)
                {
                    mensajeError = resultadoContrasena.Mensaje;
                }
            }

            if (string.IsNullOrWhiteSpace(AvatarSeleccionadoRutaRelativa))
            {
                camposInvalidos.Add("Avatar");
                if (mensajeError == null)
                {
                    mensajeError = Lang.errorTextoSeleccionAvatarValido;
                }
            }

            if (camposInvalidos.Count > 1)
            {
                mensajeError = Lang.errorTextoCamposInvalidosGenerico;
            }

            if (camposInvalidos.Count > 0)
            {
                MostrarCamposInvalidos?.Invoke(camposInvalidos);
                MostrarMensaje?.Invoke(mensajeError ?? Lang.errorTextoCamposInvalidosGenerico);
                return;
            }

            var solicitud = new SolicitudRegistroCuenta
            {
                Usuario = Usuario,
                Nombre = Nombre,
                Apellido = Apellido,
                Correo = Correo,
                Contrasena = Contrasena,
                AvatarRutaRelativa = AvatarSeleccionadoRutaRelativa
            };

            try
            {
                EstaProcesando = true;

                ResultadoSolicitudCodigo resultadoSolicitud = await _codigoVerificacionService
                    .SolicitarCodigoRegistroAsync(solicitud).ConfigureAwait(true);

                if (resultadoSolicitud == null)
                {
                    MostrarMensaje?.Invoke(Lang.errorTextoRegistrarCuentaMasTarde);
                    return;
                }

                if (resultadoSolicitud.UsuarioYaRegistrado)
                {
                    MostrarErrorUsuario = true;
                }

                if (resultadoSolicitud.CorreoYaRegistrado)
                {
                    MostrarErrorCorreo = true;
                }

                bool tieneDuplicados = resultadoSolicitud.UsuarioYaRegistrado || resultadoSolicitud.CorreoYaRegistrado;

                if (tieneDuplicados)
                {
                    var duplicados = new List<string>();
                    if (resultadoSolicitud.UsuarioYaRegistrado)
                    {
                        duplicados.Add(nameof(Usuario));
                    }

                    if (resultadoSolicitud.CorreoYaRegistrado)
                    {
                        duplicados.Add(nameof(Correo));
                    }

                    if (duplicados.Count > 0)
                    {
                        MostrarCamposInvalidos?.Invoke(duplicados);
                    }

                    return;
                }

                if (!resultadoSolicitud.CodigoEnviado)
                {
                    MostrarMensaje?.Invoke(string.IsNullOrWhiteSpace(resultadoSolicitud.Mensaje)
                        ? Lang.errorTextoRegistrarCuentaMasTarde
                        : resultadoSolicitud.Mensaje);
                    return;
                }

                ResultadoRegistroCuenta resultadoVerificacion = await _verificarCodigoDialogService
                    .MostrarDialogoAsync(
                        Lang.cambiarContrasenaTextoCodigoVerificacion,
                        resultadoSolicitud.TokenCodigo,
                        _codigoVerificacionService).ConfigureAwait(true);

                if (resultadoVerificacion == null || !resultadoVerificacion.RegistroExitoso)
                {
                    if (!string.IsNullOrWhiteSpace(resultadoVerificacion?.Mensaje))
                    {
                        MostrarMensaje?.Invoke(resultadoVerificacion.Mensaje);
                    }

                    return;
                }

                ResultadoRegistroCuenta resultadoRegistro = await _cuentaService
                    .RegistrarCuentaAsync(solicitud).ConfigureAwait(true);

                if (resultadoRegistro == null)
                {
                    MostrarMensaje?.Invoke(Lang.errorTextoRegistrarCuentaMasTarde);
                    return;
                }

                if (!resultadoRegistro.RegistroExitoso)
                {
                    MostrarErrorUsuario = resultadoRegistro.UsuarioYaRegistrado;
                    MostrarErrorCorreo = resultadoRegistro.CorreoYaRegistrado;

                    if (resultadoRegistro.UsuarioYaRegistrado || resultadoRegistro.CorreoYaRegistrado)
                    {
                        var duplicados = new List<string>();
                        if (resultadoRegistro.UsuarioYaRegistrado)
                        {
                            duplicados.Add(nameof(Usuario));
                        }

                        if (resultadoRegistro.CorreoYaRegistrado)
                        {
                            duplicados.Add(nameof(Correo));
                        }

                        if (duplicados.Count > 0)
                        {
                            MostrarCamposInvalidos?.Invoke(duplicados);
                        }

                        return;
                    }

                    MostrarMensaje?.Invoke(string.IsNullOrWhiteSpace(resultadoRegistro.Mensaje)
                        ? Lang.errorTextoRegistrarCuentaMasTarde
                        : resultadoRegistro.Mensaje);
                    return;
                }

                MostrarMensaje?.Invoke(Lang.crearCuentaTextoExitosoMensaje);
                CerrarAccion?.Invoke();
            }
            catch (ServicioException ex)
            {
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoRegistrarCuentaMasTarde);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private void Cancelar()
        {
            CerrarAccion?.Invoke();
        }

        private async Task SeleccionarAvatarAsync()
        {
            ObjetoAvatar avatar = await _seleccionarAvatarService
                .SeleccionarAvatarAsync(AvatarSeleccionadoRutaRelativa).ConfigureAwait(true);

            if (avatar == null)
            {
                return;
            }

            AvatarSeleccionadoRutaRelativa = avatar.RutaRelativa;
            AvatarSeleccionadoImagen = avatar.Imagen;
        }

        private void EstablecerAvatarPredeterminado()
        {
            ObjetoAvatar avatar = AvatarHelper.ObtenerAvatarPredeterminado();
            if (avatar != null)
            {
                AvatarSeleccionadoRutaRelativa = avatar.RutaRelativa;
                AvatarSeleccionadoImagen = avatar.Imagen;
            }
        }
    }
}
