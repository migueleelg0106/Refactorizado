using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.InicioSesion
{
    public class CreacionCuentaVistaModelo : BaseVistaModelo
    {
        private readonly ICodigoVerificacionServicio _codigoVerificacionServicio;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly ISeleccionarAvatarServicio _seleccionarAvatarServicio;
        private readonly IVerificacionCodigoDialogoServicio _verificarCodigoDialogoServicio;

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
            ICodigoVerificacionServicio codigoVerificacionServicio,
            ICuentaServicio cuentaServicio,
            ISeleccionarAvatarServicio seleccionarAvatarServicio,
            IVerificacionCodigoDialogoServicio verificarCodigoDialogoServicio)
        {
            _codigoVerificacionServicio = codigoVerificacionServicio ?? throw new ArgumentNullException(nameof(codigoVerificacionServicio));
            _cuentaServicio = cuentaServicio ?? throw new ArgumentNullException(nameof(cuentaServicio));
            _seleccionarAvatarServicio = seleccionarAvatarServicio ?? throw new ArgumentNullException(nameof(seleccionarAvatarServicio));
            _verificarCodigoDialogoServicio = verificarCodigoDialogoServicio ?? throw new ArgumentNullException(nameof(verificarCodigoDialogoServicio));

            CrearCuentaComando = new ComandoAsincrono(async _ =>
            {
                ManejadorSonido.ReproducirClick();
                await CrearCuentaAsync();
            }, _ => !EstaProcesando);

            CancelarComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                Cancelar();
            });

            SeleccionarAvatarComando = new ComandoAsincrono(async _ =>
            {
                ManejadorSonido.ReproducirClick();
                await SeleccionarAvatarAsync();
            });

            EstablecerAvatarPredeterminado();
        }

        public string Usuario { get => _usuario; set => EstablecerPropiedad(ref _usuario, value); }
        public string Nombre { get => _nombre; set => EstablecerPropiedad(ref _nombre, value); }
        public string Apellido { get => _apellido; set => EstablecerPropiedad(ref _apellido, value); }
        public string Correo { get => _correo; set => EstablecerPropiedad(ref _correo, value); }
        public string Contrasena { get => _contrasena; set => EstablecerPropiedad(ref _contrasena, value); }
        public ImageSource AvatarSeleccionadoImagen { get => _avatarSeleccionadoImagen; private set => EstablecerPropiedad(ref _avatarSeleccionadoImagen, value); }
        public int AvatarSeleccionadoId { get => _avatarSeleccionadoId; private set => EstablecerPropiedad(ref _avatarSeleccionadoId, value); }
        public bool MostrarErrorUsuario { get => _mostrarErrorUsuario; private set => EstablecerPropiedad(ref _mostrarErrorUsuario, value); }
        public bool MostrarErrorCorreo { get => _mostrarErrorCorreo; private set => EstablecerPropiedad(ref _mostrarErrorCorreo, value); }
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
        public IComandoAsincrono CrearCuentaComando { get; }
        public ICommand CancelarComando { get; }
        public IComandoAsincrono SeleccionarAvatarComando { get; }
        public Action CerrarAccion { get; set; }
        public Action<IList<string>> MostrarCamposInvalidos { get; set; }
        public Action<string> MostrarMensaje { get; set; }

        private async Task CrearCuentaAsync()
        {
            EstaProcesando = true;
            try
            {
                var (esValido, solicitud) = ValidarEntradasYMostrarErrores();
                if (!esValido) 
                {
                    ManejadorSonido.ReproducirError();
                    return;
                }
                await EjecutarFlujoDeRegistroAsync(solicitud).ConfigureAwait(true);
            }
            catch (ServicioExcepcion ex) 
            {
                ManejadorSonido.ReproducirError();
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
            string mensajeMostrar = camposInvalidos.Count > 1 ? Lang.errorTextoCamposInvalidosGenerico : primerMensajeError; 
            MostrarMensaje?.Invoke(mensajeMostrar ?? Lang.errorTextoCamposInvalidosGenerico);
            return (false, null);
        }

        private async Task EjecutarFlujoDeRegistroAsync(DTOs.NuevaCuentaDTO solicitud)
        {
            var (codigoEnviado, resultadoSolicitud, errorDuplicado) = await SolicitarCodigoRegistroYValidarRespuesta(solicitud).ConfigureAwait(true);
            if (!codigoEnviado) 
            {
                ManejadorSonido.ReproducirError();
                if (errorDuplicado) 
                {
                    MostrarErroresCamposDuplicados();
                }
                return;
            }
            
            var (verificacionExitosa, _) = await MostrarDialogoVerificacionYValidarRespuesta(resultadoSolicitud).ConfigureAwait(true);
            if (!verificacionExitosa) 
            {
                ManejadorSonido.ReproducirError();
                return;
            }

            var (registroExitoso, _) = await RegistrarCuentaYValidarRespuesta(solicitud).ConfigureAwait(true);
            if (registroExitoso) 
            {
                ManejadorSonido.ReproducirExito();
                MostrarMensaje?.Invoke(Lang.crearCuentaTextoExitosoMensaje); 
                CerrarAccion?.Invoke(); 
            }
            else 
            {
                ManejadorSonido.ReproducirError();
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

        private (DTOs.NuevaCuentaDTO Solicitud, List<string> CamposInvalidos, string PrimerMensajeError) LimpiarYValidarEntradas()
        {
            Usuario = Usuario?.Trim(); 
            Nombre = Nombre?.Trim(); 
            Apellido = Apellido?.Trim(); 
            Correo = Correo?.Trim(); 
            Contrasena = Contrasena?.Trim(); 

            var camposInvalidos = new List<string>();
            string primerMensajeError = null;

            ValidarCampo(ValidacionEntrada.ValidarUsuario(Usuario), nameof(Usuario), camposInvalidos, ref primerMensajeError);
            ValidarCampo(ValidacionEntrada.ValidarNombre(Nombre), nameof(Nombre), camposInvalidos, ref primerMensajeError);
            ValidarCampo(ValidacionEntrada.ValidarApellido(Apellido), nameof(Apellido), camposInvalidos, ref primerMensajeError);
            ValidarCampo(ValidacionEntrada.ValidarCorreo(Correo), nameof(Correo), camposInvalidos, ref primerMensajeError);
            ValidarCampo(ValidacionEntrada.ValidarContrasena(Contrasena), nameof(Contrasena), camposInvalidos, ref primerMensajeError);

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
                AvatarId = AvatarSeleccionadoId
            };
            return (solicitud, camposInvalidos, primerMensajeError);
        }

        private static void ValidarCampo(DTOs.ResultadoOperacionDTO resultado, string nombreCampo, List<string> invalidos, ref string primerError)
        {
            if (resultado?.OperacionExitosa != true)
            {
                invalidos.Add(nombreCampo);
                primerError ??= resultado?.Mensaje;
            }
        }

        private async Task<(bool CodigoEnviado, DTOs.ResultadoSolicitudCodigoDTO Resultado, bool ErrorDuplicado)>
            SolicitarCodigoRegistroYValidarRespuesta(DTOs.NuevaCuentaDTO solicitud)
        {
            DTOs.ResultadoSolicitudCodigoDTO resultado = await _codigoVerificacionServicio
                .SolicitarCodigoRegistroAsync(solicitud).ConfigureAwait(true);

            if (resultado == null)
            {
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
                MostrarMensaje?.Invoke(resultado.Mensaje ?? Lang.errorTextoRegistrarCuentaMasTarde);
                return (false, resultado, false);
            }

            return (true, resultado, false);
        }

        private async Task<(bool VerificacionExitosa, DTOs.ResultadoRegistroCuentaDTO Resultado)>
            MostrarDialogoVerificacionYValidarRespuesta(DTOs.ResultadoSolicitudCodigoDTO resultadoSolicitud)
        {
            DTOs.ResultadoRegistroCuentaDTO resultadoVerificacion = await _verificarCodigoDialogoServicio
                .MostrarDialogoAsync(
                    Lang.cambiarContrasenaTextoCodigoVerificacion,
                    resultadoSolicitud.TokenCodigo,
                    _codigoVerificacionServicio).ConfigureAwait(true);

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
                    MostrarMensaje?.Invoke(resultadoRegistro.Mensaje ?? Lang.errorTextoRegistrarCuentaMasTarde);
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
            var avatares = CatalogoAvataresLocales.ObtenerAvatares();
            if (avatares != null && avatares.Count > 0)
            {
                var avatar = avatares[0];
                AvatarSeleccionadoId = avatar.Id;
                AvatarSeleccionadoImagen = avatar.Imagen;
            }
        }
    }
}