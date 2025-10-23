using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class CrearCuentaVistaModelo : BaseVistaModelo
    {
        private readonly ICodigoVerificacionServicio _codigoVerificacionService;
        private readonly ICuentaServicio _cuentaService;
        private readonly ISeleccionarAvatarServicio _seleccionarAvatarService;
        private readonly IVerificarCodigoDialogoServicio _verificarCodigoDialogService;
        private readonly IAvatarServicio _avatarService;

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
            ICodigoVerificacionServicio codigoVerificacionService,
            ICuentaServicio cuentaService,
            ISeleccionarAvatarServicio seleccionarAvatarService,
            IVerificarCodigoDialogoServicio verificarCodigoDialogService,
            IAvatarServicio avatarService)
        {
            _codigoVerificacionService = codigoVerificacionService ?? throw new ArgumentNullException(nameof(codigoVerificacionService));
            _cuentaService = cuentaService ?? throw new ArgumentNullException(nameof(cuentaService));
            _seleccionarAvatarService = seleccionarAvatarService ?? throw new ArgumentNullException(nameof(seleccionarAvatarService));
            _verificarCodigoDialogService = verificarCodigoDialogService ?? throw new ArgumentNullException(nameof(verificarCodigoDialogService));
            _avatarService = avatarService ?? throw new ArgumentNullException(nameof(avatarService));

            CrearCuentaCommand = new ComandoAsincrono(_ => CrearCuentaAsync(), _ => !EstaProcesando);
            CancelarCommand = new ComandoDelegado(Cancelar);
            SeleccionarAvatarCommand = new ComandoAsincrono(_ => SeleccionarAvatarAsync());

            EstablecerAvatarPredeterminado();
            _ = CargarCatalogoAvataresAsync();
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

            DTOs.ResultadoOperacionDTO resultadoUsuario = ValidacionEntrada.ValidarUsuario(Usuario);
            if (resultadoUsuario?.OperacionExitosa != true)
            {
                camposInvalidos.Add(nameof(Usuario));
                if (mensajeError == null)
                {
                    mensajeError = resultadoUsuario?.Mensaje;
                }
            }

            DTOs.ResultadoOperacionDTO resultadoNombre = ValidacionEntrada.ValidarNombre(Nombre);
            if (resultadoNombre?.OperacionExitosa != true)
            {
                camposInvalidos.Add(nameof(Nombre));
                if (mensajeError == null)
                {
                    mensajeError = resultadoNombre?.Mensaje;
                }
            }

            DTOs.ResultadoOperacionDTO resultadoApellido = ValidacionEntrada.ValidarApellido(Apellido);
            if (resultadoApellido?.OperacionExitosa != true)
            {
                camposInvalidos.Add(nameof(Apellido));
                if (mensajeError == null)
                {
                    mensajeError = resultadoApellido?.Mensaje;
                }
            }

            DTOs.ResultadoOperacionDTO resultadoCorreo = ValidacionEntrada.ValidarCorreo(Correo);
            if (resultadoCorreo?.OperacionExitosa != true)
            {
                camposInvalidos.Add(nameof(Correo));
                if (mensajeError == null)
                {
                    mensajeError = resultadoCorreo?.Mensaje;
                }
            }

            DTOs.ResultadoOperacionDTO resultadoContrasena = ValidacionEntrada.ValidarContrasena(Contrasena);
            if (resultadoContrasena?.OperacionExitosa != true)
            {
                camposInvalidos.Add(nameof(Contrasena));
                if (mensajeError == null)
                {
                    mensajeError = resultadoContrasena?.Mensaje;
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

            var solicitud = new DTOs.NuevaCuentaDTO
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

                DTOs.ResultadoSolicitudCodigoDTO resultadoSolicitud = await _codigoVerificacionService
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

                DTOs.ResultadoRegistroCuentaDTO resultadoVerificacion = await _verificarCodigoDialogService
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

                DTOs.ResultadoRegistroCuentaDTO resultadoRegistro = await _cuentaService
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
            catch (ExcepcionServicio ex)
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
            AvatarSeleccionadoImagen = AvatarAyudante.ObtenerImagen(avatar);
        }

        private void EstablecerAvatarPredeterminado()
        {
            ObjetoAvatar avatar = AvatarAyudante.ObtenerAvatarPredeterminado();
            if (avatar != null)
            {
                AvatarSeleccionadoRutaRelativa = avatar.RutaRelativa;
                AvatarSeleccionadoImagen = AvatarAyudante.ObtenerImagen(avatar);
            }
        }

        private async Task CargarCatalogoAvataresAsync()
        {
            try
            {
                IReadOnlyList<ObjetoAvatar> avatares = await _avatarService.ObtenerCatalogoAsync()
                    .ConfigureAwait(true);

                if (avatares != null && avatares.Count > 0)
                {
                    AvatarAyudante.ActualizarCatalogo(avatares);
                    EstablecerAvatarPredeterminado();
                }
            }
            catch (ExcepcionServicio)
            {
                // Se mantiene el catálogo local como respaldo
            }
        }
    }
}
