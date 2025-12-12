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
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.InicioSesion
{
    public class CreacionCuentaVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICodigoVerificacionServicio _codigoVerificacionServicio;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly ISeleccionarAvatarServicio _seleccionarAvatarServicio;
        private readonly IVerificacionCodigoDialogoServicio 
            _verificarCodigoDialogoServicio;
        private readonly ILocalizacionServicio _localizacionServicio;
        private readonly ISonidoManejador _sonidoManejador;
        private readonly IValidadorEntrada _validadorEntrada;
        private readonly ICatalogoAvatares _catalogoAvatares;
        private readonly IAvisoServicio _avisoServicio;

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

        public CreacionCuentaVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            ICodigoVerificacionServicio codigoVerificacionServicio,
            ICuentaServicio cuentaServicio,
            ISeleccionarAvatarServicio seleccionarAvatarServicio,
            IVerificacionCodigoDialogoServicio verificarCodigoDialogoServicio,
            ISonidoManejador sonidoManejador,
            IValidadorEntrada validadorEntrada,
            ICatalogoAvatares catalogoAvatares,
            IAvisoServicio avisoServicio,
            ILocalizacionServicio localizacionServicio = null)
            : base(ventana, localizador)
        {
            _codigoVerificacionServicio = codigoVerificacionServicio ??
                throw new ArgumentNullException(nameof(codigoVerificacionServicio));
            _cuentaServicio = cuentaServicio ??
                throw new ArgumentNullException(nameof(cuentaServicio));
            _seleccionarAvatarServicio = seleccionarAvatarServicio ??
                throw new ArgumentNullException(nameof(seleccionarAvatarServicio));
            _verificarCodigoDialogoServicio = verificarCodigoDialogoServicio ??
                throw new ArgumentNullException(
                    nameof(verificarCodigoDialogoServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            _catalogoAvatares = catalogoAvatares ??
                throw new ArgumentNullException(nameof(catalogoAvatares));
            _validadorEntrada = validadorEntrada ??
                throw new ArgumentNullException(nameof(validadorEntrada));
            _localizacionServicio = localizacionServicio;
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));

            CrearCuentaComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await CrearCuentaAsync();
            }, _ => !EstaProcesando);

            CancelarComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                _ventana.CerrarVentana(this);
            });

            SeleccionarAvatarComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await SeleccionarAvatarAsync();
            });

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

        public int AvatarSeleccionadoId
        {
            get => _avatarSeleccionadoId;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoId, value);
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
                    ((IComandoNotificable)CrearCuentaComando)
                        .NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono CrearCuentaComando { get; }

        public ICommand CancelarComando { get; }

        public IComandoAsincrono SeleccionarAvatarComando { get; }

        public Action<IList<string>> MostrarCamposInvalidos { get; set; }

        public Action<string> MostrarMensaje { get; set; }

        public bool RegistroExitoso { get; private set; }

        private async Task CrearCuentaAsync()
        {
            var (esValido, solicitud) = ValidarEntradasYMostrarErrores();
            
            if (!esValido)
            {
                _sonidoManejador.ReproducirError();
                _logger.Warn(
                    "Intento de creacion de cuenta fallido por validacion.");
                return;
            }

            EstaProcesando = true;

            await EjecutarOperacionAsync(async () =>
            {
                _logger.InfoFormat("Iniciando flujo de registro para usuario: {0}",
                    solicitud.Usuario);
                
                await EjecutarFlujoDeRegistroAsync(solicitud);
            },
            ex =>
            {
                _logger.Error("Error de servicio durante la creacion de cuenta.", ex);
                _sonidoManejador.ReproducirError();
                MostrarMensaje?.Invoke(ex.Message ?? 
                    Lang.errorTextoRegistrarCuentaMasTarde);
                EstaProcesando = false;
            });

            EstaProcesando = false;
        }

        private (bool EsValido, DTOs.NuevaCuentaDTO Solicitud) 
            ValidarEntradasYMostrarErrores()
        {
            LimpiarErroresVisuales();

            var (solicitud, camposInvalidos, primerMensajeError) = 
                LimpiarYValidarEntradas();

            if (camposInvalidos.Count == 0)
            {
                return (true, solicitud);
            }

            MostrarErroresValidacion(camposInvalidos, primerMensajeError);
            return (false, null);
        }

        private void LimpiarErroresVisuales()
        {
            MostrarErrorUsuario = false;
            MostrarErrorCorreo = false;
            MostrarCamposInvalidos?.Invoke(Array.Empty<string>());
        }

        private void MostrarErroresValidacion(
            List<string> camposInvalidos,
            string primerMensajeError)
        {
            MostrarCamposInvalidos?.Invoke(camposInvalidos);
            
            string mensajeMostrar = camposInvalidos.Count > 1
                ? Lang.errorTextoCamposInvalidosGenerico
                : primerMensajeError;
            
            MostrarMensaje?.Invoke(mensajeMostrar ?? 
                Lang.errorTextoCamposInvalidosGenerico);
        }

        private async Task EjecutarFlujoDeRegistroAsync(DTOs.NuevaCuentaDTO solicitud)
        {
            if (!await SolicitarYValidarCodigoRegistroAsync(solicitud))
            {
                return;
            }

            if (!await VerificarCodigoConUsuarioAsync())
            {
                return;
            }

            await CompletarRegistroCuentaAsync(solicitud);
        }

        private async Task<bool> SolicitarYValidarCodigoRegistroAsync(
            DTOs.NuevaCuentaDTO solicitud)
        {
            var (codigoEnviado, resultadoSolicitud, errorDuplicado) =
                await SolicitarCodigoRegistroAsync(solicitud);

            if (!codigoEnviado)
            {
                _sonidoManejador.ReproducirError();
                
                if (errorDuplicado)
                {
                    _logger.Info(
                        "Intento de registro con usuario o correo ya existente.");
                    MostrarErroresCamposDuplicados();
                }
                
                return false;
            }

            return true;
        }

        private async Task<bool> VerificarCodigoConUsuarioAsync()
        {
            var (verificacionExitosa, _) = 
                await MostrarDialogoVerificacionAsync();

            if (!verificacionExitosa)
            {
                _logger.Info(
                    "Verificacion de codigo fallida o cancelada por el usuario.");
                _sonidoManejador.ReproducirError();
                return false;
            }

            return true;
        }

        private async Task CompletarRegistroCuentaAsync(
            DTOs.NuevaCuentaDTO solicitud)
        {
            var (registroExitoso, _) = await RegistrarCuentaAsync(solicitud);

            if (registroExitoso)
            {
                FinalizarRegistroExitoso(solicitud.Usuario);
            }
            else
            {
                ManejarErrorRegistro();
            }
        }

        private void FinalizarRegistroExitoso(string usuario)
        {
            _logger.InfoFormat("Cuenta creada exitosamente para usuario: {0}",
                usuario);
            _sonidoManejador.ReproducirNotificacion();
            MostrarMensaje?.Invoke(Lang.crearCuentaTextoExitosoMensaje);
            RegistroExitoso = true;
            _ventana.CerrarVentana(this);
        }

        private void ManejarErrorRegistro()
        {
            _logger.Warn("Fallo en el paso final de registro de cuenta.");
            _sonidoManejador.ReproducirError();
            MostrarErroresCamposDuplicados();
        }

        private void MostrarErroresCamposDuplicados()
        {
            var camposDuplicados = new List<string>();
            
            if (MostrarErrorUsuario)
            {
                camposDuplicados.Add(nameof(Usuario));
            }
            
            if (MostrarErrorCorreo)
            {
                camposDuplicados.Add(nameof(Correo));
            }
            
            MostrarCamposInvalidos?.Invoke(camposDuplicados);
        }

        private (DTOs.NuevaCuentaDTO Solicitud, List<string> CamposInvalidos,
            string PrimerMensajeError) LimpiarYValidarEntradas()
        {
            LimpiarCamposTexto();
            
            var camposInvalidos = new List<string>();
            string primerMensajeError = null;

            ValidarTodosCampos(camposInvalidos, ref primerMensajeError);

            if (camposInvalidos.Count > 0)
            {
                return (null, camposInvalidos, primerMensajeError);
            }

            DTOs.NuevaCuentaDTO solicitud = CrearSolicitudNuevaCuenta();
            return (solicitud, camposInvalidos, primerMensajeError);
        }

        private void LimpiarCamposTexto()
        {
            Usuario = Usuario?.Trim();
            Nombre = Nombre?.Trim();
            Apellido = Apellido?.Trim();
            Correo = Correo?.Trim();
            Contrasena = Contrasena?.Trim();
        }

        private void ValidarTodosCampos(
            List<string> camposInvalidos,
            ref string primerMensajeError)
        {
            ValidarCampo(_validadorEntrada.ValidarUsuario(Usuario),
                nameof(Usuario), camposInvalidos, ref primerMensajeError);
            ValidarCampo(_validadorEntrada.ValidarNombre(Nombre),
                nameof(Nombre), camposInvalidos, ref primerMensajeError);
            ValidarCampo(_validadorEntrada.ValidarApellido(Apellido),
                nameof(Apellido), camposInvalidos, ref primerMensajeError);
            ValidarCampo(_validadorEntrada.ValidarCorreo(Correo),
                nameof(Correo), camposInvalidos, ref primerMensajeError);
            ValidarCampo(_validadorEntrada.ValidarContrasena(Contrasena),
                nameof(Contrasena), camposInvalidos, ref primerMensajeError);

            if (AvatarSeleccionadoId <= 0)
            {
                camposInvalidos.Add("Avatar");
                primerMensajeError ??= Lang.errorTextoSeleccionAvatarValido;
            }
        }

        private DTOs.NuevaCuentaDTO CrearSolicitudNuevaCuenta()
        {
            return new DTOs.NuevaCuentaDTO
            {
                Usuario = Usuario,
                Nombre = Nombre,
                Apellido = Apellido,
                Correo = Correo,
                Contrasena = Contrasena,
                AvatarId = AvatarSeleccionadoId,
                Idioma = _localizacionServicio?.CulturaActual?.Name
                    ?? CultureInfo.CurrentUICulture?.Name
            };
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

        private DTOs.ResultadoSolicitudCodigoDTO _resultadoSolicitudCodigo;

        private async Task<(bool CodigoEnviado, 
            DTOs.ResultadoSolicitudCodigoDTO Resultado, bool ErrorDuplicado)>
            SolicitarCodigoRegistroAsync(DTOs.NuevaCuentaDTO solicitud)
        {
            DTOs.ResultadoSolicitudCodigoDTO resultado = 
                await _codigoVerificacionServicio
                    .SolicitarCodigoRegistroAsync(solicitud)
                    .ConfigureAwait(true);

            if (!ValidarResultadoSolicitudCodigo(resultado))
            {
                return (false, null, false);
            }

            _resultadoSolicitudCodigo = resultado;

            ActualizarErroresDuplicados(resultado);

            if (TieneCamposDuplicados(resultado))
            {
                return (false, resultado, true);
            }

            if (!resultado.CodigoEnviado)
            {
                MostrarErrorEnvioCodigo(resultado);
                return (false, resultado, false);
            }

            return (true, resultado, false);
        }

        private bool ValidarResultadoSolicitudCodigo(
            DTOs.ResultadoSolicitudCodigoDTO resultado)
        {
            if (resultado == null)
            {
                _logger.Error(
                    "El servicio de codigo de verificacion retorno null.");
                MostrarMensaje?.Invoke(Lang.errorTextoRegistrarCuentaMasTarde);
                return false;
            }

            return true;
        }

        private void ActualizarErroresDuplicados(
            DTOs.ResultadoSolicitudCodigoDTO resultado)
        {
            MostrarErrorUsuario = resultado.UsuarioRegistrado;
            MostrarErrorCorreo = resultado.CorreoRegistrado;
        }

        private bool TieneCamposDuplicados(
            DTOs.ResultadoSolicitudCodigoDTO resultado)
        {
            return resultado.UsuarioRegistrado || resultado.CorreoRegistrado;
        }

        private void MostrarErrorEnvioCodigo(
            DTOs.ResultadoSolicitudCodigoDTO resultado)
        {
            _logger.WarnFormat("No se pudo enviar el codigo. Mensaje: {0}",
                resultado.Mensaje);
            MostrarMensaje?.Invoke(resultado.Mensaje ??
                Lang.errorTextoRegistrarCuentaMasTarde);
        }

        private async Task<(bool VerificacionExitosa, 
            DTOs.ResultadoRegistroCuentaDTO Resultado)>
            MostrarDialogoVerificacionAsync()
        {
            DTOs.ResultadoRegistroCuentaDTO resultadoVerificacion =
                await _verificarCodigoDialogoServicio.MostrarDialogoAsync(
                    Lang.cambiarContrasenaTextoCodigoVerificacion,
                    _resultadoSolicitudCodigo.TokenCodigo,
                    _codigoVerificacionServicio,
                    _avisoServicio,
                    _localizador,
                    _sonidoManejador)
                .ConfigureAwait(true);

            if (!ValidarResultadoVerificacion(resultadoVerificacion))
            {
                MostrarMensajeVerificacionFallida(resultadoVerificacion);
                return (false, resultadoVerificacion);
            }

            return (true, resultadoVerificacion);
        }

        private bool ValidarResultadoVerificacion(
            DTOs.ResultadoRegistroCuentaDTO resultadoVerificacion)
        {
            return resultadoVerificacion != null && 
                   resultadoVerificacion.RegistroExitoso;
        }

        private void MostrarMensajeVerificacionFallida(
            DTOs.ResultadoRegistroCuentaDTO resultadoVerificacion)
        {
            if (!string.IsNullOrWhiteSpace(resultadoVerificacion?.Mensaje))
            {
                MostrarMensaje?.Invoke(resultadoVerificacion.Mensaje);
            }
        }

        private async Task<(bool RegistroExitoso, 
            DTOs.ResultadoRegistroCuentaDTO Resultado)>
            RegistrarCuentaAsync(DTOs.NuevaCuentaDTO solicitud)
        {
            DTOs.ResultadoRegistroCuentaDTO resultadoRegistro = 
                await _cuentaServicio
                    .RegistrarCuentaAsync(solicitud)
                    .ConfigureAwait(true);

            if (!ValidarResultadoRegistro(resultadoRegistro))
            {
                return (false, null);
            }

            if (!resultadoRegistro.RegistroExitoso)
            {
                return ProcesarRegistroFallido(resultadoRegistro);
            }

            return (true, resultadoRegistro);
        }

        private bool ValidarResultadoRegistro(
            DTOs.ResultadoRegistroCuentaDTO resultadoRegistro)
        {
            if (resultadoRegistro == null)
            {
                _logger.Error("El servicio de registro de cuenta retorno null.");
                MostrarMensaje?.Invoke(Lang.errorTextoRegistrarCuentaMasTarde);
                return false;
            }

            return true;
        }

        private (bool RegistroExitoso, DTOs.ResultadoRegistroCuentaDTO Resultado)
            ProcesarRegistroFallido(DTOs.ResultadoRegistroCuentaDTO resultadoRegistro)
        {
            MostrarErrorUsuario = resultadoRegistro.UsuarioRegistrado;
            MostrarErrorCorreo = resultadoRegistro.CorreoRegistrado;

            if (resultadoRegistro.UsuarioRegistrado || 
                resultadoRegistro.CorreoRegistrado)
            {
                return (false, resultadoRegistro);
            }

            _logger.WarnFormat("Error al registrar cuenta: {0}",
                resultadoRegistro.Mensaje);
            MostrarMensaje?.Invoke(resultadoRegistro.Mensaje ??
                Lang.errorTextoRegistrarCuentaMasTarde);
            return (false, resultadoRegistro);
        }

        private async Task SeleccionarAvatarAsync()
        {
            ObjetoAvatar avatar = await _seleccionarAvatarServicio
                .SeleccionarAvatarAsync(AvatarSeleccionadoId)
                .ConfigureAwait(true);

            if (avatar == null)
            {
                return;
            }

            ActualizarAvatarSeleccionado(avatar);
        }

        private void ActualizarAvatarSeleccionado(ObjetoAvatar avatar)
        {
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