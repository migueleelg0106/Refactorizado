using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalCliente.Utilidades.Abstracciones;

namespace PictionaryMusicalCliente.VistaModelo.InicioSesion
{
    /// <summary>
    /// Gestiona el proceso de registro de una nueva cuenta, incluyendo validacion de datos
    /// y seleccion de avatar.
    /// </summary>
    public class CreacionCuentaVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICodigoVerificacionServicio _codigoVerificacionServicio;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly ISeleccionarAvatarServicio _seleccionarAvatarServicio;
        private readonly IVerificacionCodigoDialogoServicio _verificarCodigoDialogoServicio;
        private readonly ILocalizacionServicio _localizacionServicio;
        private readonly ISonidoManejador _sonidoManejador;
        private readonly IValidadorEntrada _validadorEntrada;
        private readonly ICatalogoAvatares _catalogoAvatares;
        private readonly IAvisoServicio _avisoServicio;
        private readonly ILocalizadorServicio _localizador;

        private string _usuario;
        private string _nombre;
        private string _apellido;
        private string _correo;
        private string _contrasena;
        private ImageSource _avatarSeleccionadoImagen;
        private int _avatarSeleccionadoId;
        private bool _mostrarErrorUsuario;
        private bool _mostrarErrorCorreo;
        private bool _estaProcesando;

        /// <summary>
        /// Inicializa el ViewModel e inyecta los servicios necesarios para el registro.
        /// </summary>
        /// <param name="codigoVerificacionServicio">Servicio para envio de codigos.</param>
        /// <param name="cuentaServicio">Servicio para creacion de cuentas.</param>
        /// <param name="seleccionarAvatarServicio">Servicio para manejo de avatares.</param>
        /// <param name="verificarCodigoDialogoServicio">Servicio para UI de verificacion.</param>
        public CreacionCuentaVistaModelo(
            ICodigoVerificacionServicio codigoVerificacionServicio,
            ICuentaServicio cuentaServicio,
            ISeleccionarAvatarServicio seleccionarAvatarServicio,
            IVerificacionCodigoDialogoServicio verificarCodigoDialogoServicio,
            ISonidoManejador sonidoManejador,
            IValidadorEntrada validadorEntrada,
            ICatalogoAvatares catalogoAvatares,
            IAvisoServicio avisoServicio,
            ILocalizadorServicio localizador,
            ILocalizacionServicio localizacionServicio = null
            )
        {
            _codigoVerificacionServicio = codigoVerificacionServicio ??
                throw new ArgumentNullException(nameof(codigoVerificacionServicio));
            _cuentaServicio = cuentaServicio ??
                throw new ArgumentNullException(nameof(cuentaServicio));
            _seleccionarAvatarServicio = seleccionarAvatarServicio ??
                throw new ArgumentNullException(nameof(seleccionarAvatarServicio));
            _verificarCodigoDialogoServicio = verificarCodigoDialogoServicio ??
                throw new ArgumentNullException(nameof(verificarCodigoDialogoServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            _catalogoAvatares = catalogoAvatares ??
                throw new ArgumentNullException(nameof(catalogoAvatares));
            _validadorEntrada = validadorEntrada ??
                throw new ArgumentNullException(nameof(validadorEntrada));
            _localizacionServicio = localizacionServicio ?? _localizacionServicio;
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));
            _localizador = localizador ??
                throw new ArgumentNullException(nameof(localizador));

            CrearCuentaComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await CrearCuentaAsync();
            }, _ => !EstaProcesando);

            CancelarComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                Cancelar();
            });

            SeleccionarAvatarComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await SeleccionarAvatarAsync();
            });

            EstablecerAvatarPredeterminado();
        }

        /// <summary>
        /// Nombre de usuario unico para la cuenta.
        /// </summary>
        public string Usuario
        {
            get => _usuario;
            set => EstablecerPropiedad(ref _usuario, value);
        }

        /// <summary>
        /// Nombre real del usuario.
        /// </summary>
        public string Nombre
        {
            get => _nombre;
            set => EstablecerPropiedad(ref _nombre, value);
        }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        public string Apellido
        {
            get => _apellido;
            set => EstablecerPropiedad(ref _apellido, value);
        }

        /// <summary>
        /// Correo electronico para contacto y validacion.
        /// </summary>
        public string Correo
        {
            get => _correo;
            set => EstablecerPropiedad(ref _correo, value);
        }

        /// <summary>
        /// Contrasena segura para la cuenta.
        /// </summary>
        public string Contrasena
        {
            get => _contrasena;
            set => EstablecerPropiedad(ref _contrasena, value);
        }

        /// <summary>
        /// Imagen del avatar seleccionado para mostrar en la vista.
        /// </summary>
        public ImageSource AvatarSeleccionadoImagen
        {
            get => _avatarSeleccionadoImagen;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoImagen, value);
        }

        /// <summary>
        /// Identificador del avatar seleccionado.
        /// </summary>
        public int AvatarSeleccionadoId
        {
            get => _avatarSeleccionadoId;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoId, value);
        }

        /// <summary>
        /// Indica si debe mostrarse un error visual en el campo de usuario.
        /// </summary>
        public bool MostrarErrorUsuario
        {
            get => _mostrarErrorUsuario;
            private set => EstablecerPropiedad(ref _mostrarErrorUsuario, value);
        }

        /// <summary>
        /// Indica si debe mostrarse un error visual en el campo de correo.
        /// </summary>
        public bool MostrarErrorCorreo
        {
            get => _mostrarErrorCorreo;
            private set => EstablecerPropiedad(ref _mostrarErrorCorreo, value);
        }

        /// <summary>
        /// Bloquea los comandos mientras se realiza una operacion de red.
        /// </summary>
        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ((IComandoNotificable)CrearCuentaComando).NotificarPuedeEjecutar();
                }
            }
        }

        /// <summary>
        /// Comando que inicia el flujo de creacion de cuenta.
        /// </summary>
        public IComandoAsincrono CrearCuentaComando { get; }

        /// <summary>
        /// Comando para cancelar el registro.
        /// </summary>
        public ICommand CancelarComando { get; }

        /// <summary>
        /// Comando para abrir el dialogo de seleccion de avatar.
        /// </summary>
        public IComandoAsincrono SeleccionarAvatarComando { get; }

        /// <summary>
        /// Accion para cerrar la ventana tras un registro exitoso.
        /// </summary>
        public Action CerrarAccion { get; set; }

        /// <summary>
        /// Accion para indicar visualmente que campos fallaron la validacion.
        /// </summary>
        public Action<IList<string>> MostrarCamposInvalidos { get; set; }

        /// <summary>
        /// Accion para mostrar mensajes informativos o de error al usuario.
        /// </summary>
        public Action<string> MostrarMensaje { get; set; }

        private async Task CrearCuentaAsync()
        {
            EstaProcesando = true;
            try
            {
                var (esValido, solicitud) = ValidarEntradasYMostrarErrores();
                if (!esValido)
                {
                    _sonidoManejador.ReproducirError();
					_logger.Warn(
                        "Intento de creación de cuenta fallido por validación de campos.");
                    return;
                }

                _logger.InfoFormat("Iniciando flujo de registro para usuario: {0}",
                    solicitud.Usuario);
                await EjecutarFlujoDeRegistroAsync(solicitud).ConfigureAwait(true);
            }
            catch (ServicioExcepcion ex)
            {
                _logger.Error("Error de servicio durante la creación de cuenta.", ex);
                _sonidoManejador.ReproducirError();
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoRegistrarCuentaMasTarde);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private (bool EsValido, DTOs.NuevaCuentaDTO Solicitud) ValidarEntradasYMostrarErrores()
        {
            MostrarErrorUsuario = false;
            MostrarErrorCorreo = false;
            MostrarCamposInvalidos?.Invoke(Array.Empty<string>());

            var (solicitud, camposInvalidos, primerMensajeError) = LimpiarYValidarEntradas();

            if (camposInvalidos.Count == 0)
            {
                return (true, solicitud);
            }
            MostrarCamposInvalidos?.Invoke(camposInvalidos);
            string mensajeMostrar = camposInvalidos.Count > 1
                ? Lang.errorTextoCamposInvalidosGenerico
                : primerMensajeError;
            MostrarMensaje?.Invoke(mensajeMostrar ?? Lang.errorTextoCamposInvalidosGenerico);
            return (false, null);
        }

        private async Task EjecutarFlujoDeRegistroAsync(DTOs.NuevaCuentaDTO solicitud)
        {
            var (codigoEnviado, resultadoSolicitud, errorDuplicado) =
                await SolicitarCodigoRegistroYValidarRespuesta(solicitud).ConfigureAwait(true);

            if (!codigoEnviado)
            {
                _sonidoManejador.ReproducirError();
                if (errorDuplicado)
                {
                    _logger.Info("Intento de registro con usuario o correo ya existente.");
                    MostrarErroresCamposDuplicados();
                }
                return;
            }

            var (verificacionExitosa, _) = await MostrarDialogoVerificacionYValidarRespuesta(
                resultadoSolicitud).ConfigureAwait(true);

            if (!verificacionExitosa)
            {
                _logger.Info("Verificación de código fallida o cancelada por el usuario.");
                _sonidoManejador.ReproducirError();
                return;
            }

            var (registroExitoso, _) = await RegistrarCuentaYValidarRespuesta(solicitud)
                .ConfigureAwait(true);

            if (registroExitoso)
            {
                _logger.InfoFormat("Cuenta creada exitosamente para usuario: {0}",
                    solicitud.Usuario);
                _sonidoManejador.ReproducirExito();
                MostrarMensaje?.Invoke(Lang.crearCuentaTextoExitosoMensaje);
                CerrarAccion?.Invoke();
            }
            else
            {
                _logger.Warn("Fallo en el paso final de registro de cuenta.");
                _sonidoManejador.ReproducirError();
                MostrarErroresCamposDuplicados();
            }
        }

        private void MostrarErroresCamposDuplicados()
        {
            var camposDuplicados = new List<string>();
            if (MostrarErrorUsuario) camposDuplicados.Add(nameof(Usuario));
            if (MostrarErrorCorreo) camposDuplicados.Add(nameof(Correo));
            MostrarCamposInvalidos?.Invoke(camposDuplicados);
        }

        private (DTOs.NuevaCuentaDTO Solicitud, List<string> CamposInvalidos,
            string PrimerMensajeError) LimpiarYValidarEntradas()
        {
            Usuario = Usuario?.Trim();
            Nombre = Nombre?.Trim();
            Apellido = Apellido?.Trim();
            Correo = Correo?.Trim();
            Contrasena = Contrasena?.Trim();

            var camposInvalidos = new List<string>();
            string primerMensajeError = null;

            ValidarCampo(_validadorEntrada.ValidarUsuario(Usuario), nameof(Usuario),
                camposInvalidos, ref primerMensajeError);
            ValidarCampo(_validadorEntrada.ValidarNombre(Nombre), nameof(Nombre),
                camposInvalidos, ref primerMensajeError);
            ValidarCampo(_validadorEntrada.ValidarApellido(Apellido), nameof(Apellido),
                camposInvalidos, ref primerMensajeError);
            ValidarCampo(_validadorEntrada.ValidarCorreo(Correo), nameof(Correo),
                camposInvalidos, ref primerMensajeError);
            ValidarCampo(_validadorEntrada.ValidarContrasena(Contrasena), nameof(Contrasena),
                camposInvalidos, ref primerMensajeError);

            if (AvatarSeleccionadoId <= 0)
            {
                camposInvalidos.Add("Avatar");
                primerMensajeError ??= Lang.errorTextoSeleccionAvatarValido;
            }

            if (camposInvalidos.Count > 0)
            {
                return (null, camposInvalidos, primerMensajeError);
            }

            var solicitud = new DTOs.NuevaCuentaDTO
            {
                Usuario = Usuario,
                Nombre = Nombre,
                Apellido = Apellido,
                Correo = Correo,
                Contrasena = Contrasena,
                AvatarId = AvatarSeleccionadoId,
                Idioma = _localizacionServicio.CulturaActual?.Name
                    ?? CultureInfo.CurrentUICulture?.Name
            };
            return (solicitud, camposInvalidos, primerMensajeError);
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

        private async Task<(bool CodigoEnviado, DTOs.ResultadoSolicitudCodigoDTO Resultado,
            bool ErrorDuplicado)>
            SolicitarCodigoRegistroYValidarRespuesta(DTOs.NuevaCuentaDTO solicitud)
        {
            DTOs.ResultadoSolicitudCodigoDTO resultado = await _codigoVerificacionServicio
                .SolicitarCodigoRegistroAsync(solicitud).ConfigureAwait(true);

            if (resultado == null)
            {
                _logger.Error("El servicio de código de verificación retornó null.");
                MostrarMensaje?.Invoke(Lang.errorTextoRegistrarCuentaMasTarde);
                return (false, null, false);
            }

            MostrarErrorUsuario = resultado.UsuarioRegistrado;
            MostrarErrorCorreo = resultado.CorreoRegistrado;

            if (resultado.UsuarioRegistrado || resultado.CorreoRegistrado)
            {
                return (false, resultado, true);
            }

            if (!resultado.CodigoEnviado)
            {
                _logger.WarnFormat("No se pudo enviar el código. Mensaje: {0}",
                    resultado.Mensaje);
                MostrarMensaje?.Invoke(resultado.Mensaje ??
                    Lang.errorTextoRegistrarCuentaMasTarde);
                return (false, resultado, false);
            }

            return (true, resultado, false);
        }

        private async Task<(bool VerificacionExitosa, DTOs.ResultadoRegistroCuentaDTO Resultado)>
            MostrarDialogoVerificacionYValidarRespuesta(
            DTOs.ResultadoSolicitudCodigoDTO resultadoSolicitud)
        {
            DTOs.ResultadoRegistroCuentaDTO resultadoVerificacion =
                await _verificarCodigoDialogoServicio.MostrarDialogoAsync(
                    Lang.cambiarContrasenaTextoCodigoVerificacion,
                    resultadoSolicitud.TokenCodigo,
                    _codigoVerificacionServicio, _avisoServicio,
                    _localizador, _sonidoManejador).ConfigureAwait(true);

            if (resultadoVerificacion == null || !resultadoVerificacion.RegistroExitoso)
            {
                if (!string.IsNullOrWhiteSpace(resultadoVerificacion?.Mensaje))
                {
                    MostrarMensaje?.Invoke(resultadoVerificacion.Mensaje);
                }
                return (false, resultadoVerificacion);
            }
            return (true, resultadoVerificacion);
        }

        private async Task<(bool RegistroExitoso, DTOs.ResultadoRegistroCuentaDTO Resultado)>
            RegistrarCuentaYValidarRespuesta(DTOs.NuevaCuentaDTO solicitud)
        {
            DTOs.ResultadoRegistroCuentaDTO resultadoRegistro = await _cuentaServicio
                .RegistrarCuentaAsync(solicitud).ConfigureAwait(true);

            if (resultadoRegistro == null)
            {
                _logger.Error("El servicio de registro de cuenta retornó null.");
                MostrarMensaje?.Invoke(Lang.errorTextoRegistrarCuentaMasTarde);
                return (false, null);
            }

            if (!resultadoRegistro.RegistroExitoso)
            {
                MostrarErrorUsuario = resultadoRegistro.UsuarioRegistrado;
                MostrarErrorCorreo = resultadoRegistro.CorreoRegistrado;

                if (resultadoRegistro.UsuarioRegistrado || resultadoRegistro.CorreoRegistrado)
                {
                    return (false, resultadoRegistro);
                }
                else
                {
                    _logger.WarnFormat("Error al registrar cuenta: {0}",
                        resultadoRegistro.Mensaje);
                    MostrarMensaje?.Invoke(resultadoRegistro.Mensaje ??
                        Lang.errorTextoRegistrarCuentaMasTarde);
                    return (false, resultadoRegistro);
                }
            }
            return (true, resultadoRegistro);
        }

        private void Cancelar()
        {
            CerrarAccion?.Invoke();
        }

        private async Task SeleccionarAvatarAsync()
        {
            ObjetoAvatar avatar = await _seleccionarAvatarServicio
                .SeleccionarAvatarAsync(AvatarSeleccionadoId).ConfigureAwait(true);

            if (avatar == null) return;

            AvatarSeleccionadoId = avatar.Id;
            AvatarSeleccionadoImagen = avatar.Imagen;
        }

        private void EstablecerAvatarPredeterminado()
        {
            var avatares = _catalogoAvatares.ObtenerAvatares();
            if (avatares != null && avatares.Count > 0)
            {
                var avatar = avatares[0];
                AvatarSeleccionadoId = avatar.Id;
                AvatarSeleccionadoImagen = avatar.Imagen;
            }
        }
    }
}