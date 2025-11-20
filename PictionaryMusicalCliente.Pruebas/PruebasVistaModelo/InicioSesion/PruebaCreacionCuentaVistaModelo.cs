using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.InicioSesion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows; 
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.InicioSesion
{
    [TestClass]
    public class PruebaCreacionCuentaVistaModelo
    {
        private Mock<ICodigoVerificacionServicio> _mockCodigoService;
        private Mock<ICuentaServicio> _mockCuentaService;
        private Mock<ISeleccionarAvatarServicio> _mockAvatarService;
        private Mock<IVerificacionCodigoDialogoServicio> _mockDialogoService;
        private CreacionCuentaVistaModelo _vistaModelo;

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null) new Application();
            Application.ResourceAssembly = typeof(CreacionCuentaVistaModelo).Assembly;

            _mockCodigoService = new Mock<ICodigoVerificacionServicio>();
            _mockCuentaService = new Mock<ICuentaServicio>();
            _mockAvatarService = new Mock<ISeleccionarAvatarServicio>();
            _mockDialogoService = new Mock<IVerificacionCodigoDialogoServicio>();

            _vistaModelo = new CreacionCuentaVistaModelo(
                _mockCodigoService.Object,
                _mockCuentaService.Object,
                _mockAvatarService.Object,
                _mockDialogoService.Object
            );

            _vistaModelo.MostrarMensaje = (_) => { };
            _vistaModelo.MostrarCamposInvalidos = (_) => { };
            _vistaModelo.CerrarAccion = () => { };
            AvisoAyudante.DefinirMostrarAviso((_) => { });
        }

        [TestCleanup]
        public void Limpiar()
        {
            _vistaModelo = null;
        }

        #region 1. Constructor y Validaciones de Dependencias

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_CodigoServicioNulo_LanzaExcepcion()
        {
            new CreacionCuentaVistaModelo(null, _mockCuentaService.Object, _mockAvatarService.Object, _mockDialogoService.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_CuentaServicioNulo_LanzaExcepcion()
        {
            new CreacionCuentaVistaModelo(_mockCodigoService.Object, null, _mockAvatarService.Object, _mockDialogoService.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_AvatarServicioNulo_LanzaExcepcion()
        {
            new CreacionCuentaVistaModelo(_mockCodigoService.Object, _mockCuentaService.Object, null, _mockDialogoService.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_DialogoServicioNulo_LanzaExcepcion()
        {
            new CreacionCuentaVistaModelo(_mockCodigoService.Object, _mockCuentaService.Object, _mockAvatarService.Object, null);
        }

        [TestMethod]
        public void Prueba_Constructor_InicializaAvatarPredeterminado()
        {
            if (CatalogoAvataresLocales.ObtenerAvatares().Count > 0)
            {
                Assert.IsTrue(_vistaModelo.AvatarSeleccionadoId > 0);
                Assert.IsNotNull(_vistaModelo.AvatarSeleccionadoImagen);
            }
        }

        #endregion

        #region 2. Selección de Avatar

        [TestMethod]
        public async Task Prueba_SeleccionarAvatar_Exito_ActualizaPropiedades()
        {
            var avatarMock = new ObjetoAvatar(99, "AvatarTest", null);
            _mockAvatarService.Setup(s => s.SeleccionarAvatarAsync(It.IsAny<int>())).ReturnsAsync(avatarMock);

            await _vistaModelo.SeleccionarAvatarComando.EjecutarAsync(null);

            Assert.AreEqual(99, _vistaModelo.AvatarSeleccionadoId);
        }

        [TestMethod]
        public async Task Prueba_SeleccionarAvatar_Cancelado_NoCambia()
        {
            _mockAvatarService.Setup(s => s.SeleccionarAvatarAsync(It.IsAny<int>())).ReturnsAsync((ObjetoAvatar)null);
            int idOriginal = _vistaModelo.AvatarSeleccionadoId;

            await _vistaModelo.SeleccionarAvatarComando.EjecutarAsync(null);

            Assert.AreEqual(idOriginal, _vistaModelo.AvatarSeleccionadoId);
        }

        #endregion

        #region 3. Crear Cuenta - Validaciones Locales

        [TestMethod]
        public async Task Prueba_CrearCuenta_CamposVacios_MuestraErrores()
        {
            List<string> invalidos = null;
            _vistaModelo.MostrarCamposInvalidos = (l) => invalidos = l.ToList();

            await _vistaModelo.CrearCuentaComando.EjecutarAsync(null);

            Assert.IsNotNull(invalidos);
            Assert.IsTrue(invalidos.Contains("Usuario"));
            Assert.IsTrue(invalidos.Contains("Contrasena"));
            Assert.IsTrue(invalidos.Contains("Correo"));
            _mockCodigoService.Verify(s => s.SolicitarCodigoRegistroAsync(It.IsAny<DTOs.NuevaCuentaDTO>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_CrearCuenta_AvatarInvalido_MuestraError()
        {
            CargarDatosValidos();
            typeof(CreacionCuentaVistaModelo).GetProperty("AvatarSeleccionadoId").SetValue(_vistaModelo, 0);

            List<string> invalidos = null;
            _vistaModelo.MostrarCamposInvalidos = (l) => invalidos = l.ToList();

            await _vistaModelo.CrearCuentaComando.EjecutarAsync(null);

            Assert.IsTrue(invalidos.Contains("Avatar"));
        }

        [TestMethod]
        public async Task Prueba_CrearCuenta_UnSoloCampoInvalido_MuestraMensajeEspecifico()
        {
            CargarDatosValidos();
            _vistaModelo.Usuario = ""; 

            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;
            List<string> invalidos = null;
            _vistaModelo.MostrarCamposInvalidos = (l) => invalidos = l.ToList();

            await _vistaModelo.CrearCuentaComando.EjecutarAsync(null);

            Assert.AreEqual(1, invalidos.Count);
            Assert.AreNotEqual(Lang.errorTextoCamposInvalidosGenerico, mensaje);
        }

        #endregion

        #region 4. Crear Cuenta - Fase 1: Solicitar Código (Servicio)

        [TestMethod]
        public async Task Prueba_CrearCuenta_SolicitarCodigo_ServicioRetornaNulo_MuestraError()
        {
            CargarDatosValidos();
            _mockCodigoService.Setup(s => s.SolicitarCodigoRegistroAsync(It.IsAny<DTOs.NuevaCuentaDTO>()))
                .ReturnsAsync((DTOs.ResultadoSolicitudCodigoDTO)null);

            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            await _vistaModelo.CrearCuentaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoRegistrarCuentaMasTarde, mensaje);
        }

        [TestMethod]
        public async Task Prueba_CrearCuenta_SolicitarCodigo_UsuarioDuplicado_MuestraErrorCampos()
        {
            CargarDatosValidos();
            var resultadoDuplicado = new DTOs.ResultadoSolicitudCodigoDTO
            {
                UsuarioRegistrado = true,
                CorreoRegistrado = false
            };
            _mockCodigoService.Setup(s => s.SolicitarCodigoRegistroAsync(It.IsAny<DTOs.NuevaCuentaDTO>()))
                .ReturnsAsync(resultadoDuplicado);

            List<string> invalidos = null;
            _vistaModelo.MostrarCamposInvalidos = (l) => invalidos = l.ToList();

            await _vistaModelo.CrearCuentaComando.EjecutarAsync(null);

            Assert.IsTrue(_vistaModelo.MostrarErrorUsuario);
            Assert.IsTrue(invalidos.Contains("Usuario"));
            _mockDialogoService.Verify(s => s.MostrarDialogoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ICodigoVerificacionServicio>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_CrearCuenta_SolicitarCodigo_FalloEnvio_MuestraMensaje()
        {
            CargarDatosValidos();
            var resultadoFallo = new DTOs.ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = false,
                Mensaje = "Error SMTP"
            };
            _mockCodigoService.Setup(s => s.SolicitarCodigoRegistroAsync(It.IsAny<DTOs.NuevaCuentaDTO>()))
                .ReturnsAsync(resultadoFallo);

            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            await _vistaModelo.CrearCuentaComando.EjecutarAsync(null);

            Assert.AreEqual("Error SMTP", mensaje);
        }

        [TestMethod]
        public async Task Prueba_CrearCuenta_ExcepcionServicio_MuestraError()
        {
            CargarDatosValidos();
            _mockCodigoService.Setup(s => s.SolicitarCodigoRegistroAsync(It.IsAny<DTOs.NuevaCuentaDTO>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "FalloRed", null));

            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            await _vistaModelo.CrearCuentaComando.EjecutarAsync(null);

            Assert.AreEqual("FalloRed", mensaje);
            Assert.IsFalse(_vistaModelo.EstaProcesando);
        }

        #endregion

        #region 5. Crear Cuenta - Fase 2: Diálogo de Verificación

        [TestMethod]
        public async Task Prueba_CrearCuenta_DialogoVerificacion_FalloONulo_DetieneFlujo()
        {
            CargarDatosValidos();
            var resultadoCodigo = new DTOs.ResultadoSolicitudCodigoDTO { CodigoEnviado = true, TokenCodigo = "TOKEN" };
            _mockCodigoService.Setup(s => s.SolicitarCodigoRegistroAsync(It.IsAny<DTOs.NuevaCuentaDTO>()))
                .ReturnsAsync(resultadoCodigo);

            _mockDialogoService.Setup(s => s.MostrarDialogoAsync(It.IsAny<string>(), "TOKEN", _mockCodigoService.Object))
                .ReturnsAsync((DTOs.ResultadoRegistroCuentaDTO)null);

            await _vistaModelo.CrearCuentaComando.EjecutarAsync(null);

            _mockCuentaService.Verify(s => s.RegistrarCuentaAsync(It.IsAny<DTOs.NuevaCuentaDTO>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_CrearCuenta_DialogoVerificacion_MuestraMensajeSiFalla()
        {
            CargarDatosValidos();
            var resultadoCodigo = new DTOs.ResultadoSolicitudCodigoDTO { CodigoEnviado = true };
            _mockCodigoService.Setup(s => s.SolicitarCodigoRegistroAsync(It.IsAny<DTOs.NuevaCuentaDTO>())).ReturnsAsync(resultadoCodigo);

            var resultadoVerificacion = new DTOs.ResultadoRegistroCuentaDTO { RegistroExitoso = false, Mensaje = "CodigoIncorrecto" };
            _mockDialogoService.Setup(s => s.MostrarDialogoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ICodigoVerificacionServicio>()))
                .ReturnsAsync(resultadoVerificacion);

            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            await _vistaModelo.CrearCuentaComando.EjecutarAsync(null);

            Assert.AreEqual("CodigoIncorrecto", mensaje);
        }

        #endregion

        #region 6. Crear Cuenta - Fase 3: Registro Final

        [TestMethod]
        public async Task Prueba_CrearCuenta_RegistroFinal_Nulo_MuestraError()
        {
            ConfigurarHastaRegistro();
            _mockCuentaService.Setup(s => s.RegistrarCuentaAsync(It.IsAny<DTOs.NuevaCuentaDTO>()))
                .ReturnsAsync((DTOs.ResultadoRegistroCuentaDTO)null);

            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            await _vistaModelo.CrearCuentaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoRegistrarCuentaMasTarde, mensaje);
        }

        [TestMethod]
        public async Task Prueba_CrearCuenta_RegistroFinal_DuplicadoTardio_MuestraErrores()
        {
            ConfigurarHastaRegistro();
            var resultadoFallo = new DTOs.ResultadoRegistroCuentaDTO
            {
                RegistroExitoso = false,
                UsuarioRegistrado = true
            };
            _mockCuentaService.Setup(s => s.RegistrarCuentaAsync(It.IsAny<DTOs.NuevaCuentaDTO>())).ReturnsAsync(resultadoFallo);

            List<string> invalidos = null;
            _vistaModelo.MostrarCamposInvalidos = (l) => invalidos = l.ToList();

            await _vistaModelo.CrearCuentaComando.EjecutarAsync(null);

            Assert.IsTrue(invalidos.Contains("Usuario"));
        }

        [TestMethod]
        public async Task Prueba_CrearCuenta_RegistroFinal_ErrorGenerico_MuestraMensaje()
        {
            ConfigurarHastaRegistro();
            var resultadoFallo = new DTOs.ResultadoRegistroCuentaDTO
            {
                RegistroExitoso = false,
                Mensaje = "ErrorBD"
            };
            _mockCuentaService.Setup(s => s.RegistrarCuentaAsync(It.IsAny<DTOs.NuevaCuentaDTO>())).ReturnsAsync(resultadoFallo);

            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            await _vistaModelo.CrearCuentaComando.EjecutarAsync(null);

            Assert.AreEqual("ErrorBD", mensaje);
        }

        [TestMethod]
        public async Task Prueba_CrearCuenta_Exito_FlujoCompleto()
        {
            CargarDatosValidos();

            _mockCodigoService.Setup(s => s.SolicitarCodigoRegistroAsync(It.IsAny<DTOs.NuevaCuentaDTO>()))
                .ReturnsAsync(new DTOs.ResultadoSolicitudCodigoDTO { CodigoEnviado = true, TokenCodigo = "TOKEN" });

            _mockDialogoService.Setup(s => s.MostrarDialogoAsync(It.IsAny<string>(), "TOKEN", _mockCodigoService.Object))
                .ReturnsAsync(new DTOs.ResultadoRegistroCuentaDTO { RegistroExitoso = true }); 

            _mockCuentaService.Setup(s => s.RegistrarCuentaAsync(It.IsAny<DTOs.NuevaCuentaDTO>()))
                .ReturnsAsync(new DTOs.ResultadoRegistroCuentaDTO { RegistroExitoso = true });

            bool cerrado = false;
            _vistaModelo.CerrarAccion = () => cerrado = true;
            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            await _vistaModelo.CrearCuentaComando.EjecutarAsync(null);

            Assert.IsTrue(cerrado, "La ventana debió cerrarse tras el registro exitoso.");
            Assert.AreEqual(Lang.crearCuentaTextoExitosoMensaje, mensaje);
        }

        #endregion

        #region 7. Comandos y Propiedades

        [TestMethod]
        public void Prueba_CancelarComando_CierraVentana()
        {
            bool cerrado = false;
            _vistaModelo.CerrarAccion = () => cerrado = true;

            _vistaModelo.CancelarComando.Execute(null);

            Assert.IsTrue(cerrado);
        }

        [TestMethod]
        public void Prueba_EstaProcesando_Setter_NotificaComando()
        {
            typeof(CreacionCuentaVistaModelo).GetProperty("EstaProcesando").SetValue(_vistaModelo, true);

            Assert.IsFalse(_vistaModelo.CrearCuentaComando.CanExecute(null));

            typeof(CreacionCuentaVistaModelo).GetProperty("EstaProcesando").SetValue(_vistaModelo, false);
            Assert.IsTrue(_vistaModelo.CrearCuentaComando.CanExecute(null));
        }

        #endregion

        private void CargarDatosValidos()
        {
            _vistaModelo.Usuario = "User123";
            _vistaModelo.Nombre = "Name";
            _vistaModelo.Apellido = "Last";
            _vistaModelo.Correo = "a@a.com";
            _vistaModelo.Contrasena = "Pass123!";
        }

        private void ConfigurarHastaRegistro()
        {
            CargarDatosValidos();
            _mockCodigoService.Setup(s => s.SolicitarCodigoRegistroAsync(It.IsAny<DTOs.NuevaCuentaDTO>()))
               .ReturnsAsync(new DTOs.ResultadoSolicitudCodigoDTO { CodigoEnviado = true });

            _mockDialogoService.Setup(s => s.MostrarDialogoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ICodigoVerificacionServicio>()))
               .ReturnsAsync(new DTOs.ResultadoRegistroCuentaDTO { RegistroExitoso = true });
        }
    }
}