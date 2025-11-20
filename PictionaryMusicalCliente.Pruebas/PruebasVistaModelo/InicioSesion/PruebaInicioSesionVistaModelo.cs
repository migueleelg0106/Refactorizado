using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.VistaModelo.InicioSesion;
using PictionaryMusicalCliente.VistaModelo.Salas;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows; 
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.InicioSesion
{
    [TestClass]
    public class PruebaInicioSesionVistaModelo
    {
        private Mock<IInicioSesionServicio> _mockInicioSesion;
        private Mock<ICambioContrasenaServicio> _mockCambioContrasena;
        private Mock<IRecuperacionCuentaServicio> _mockRecuperacion;
        private Mock<ILocalizacionServicio> _mockLocalizacion;
        private Mock<ISalasServicio> _mockSalasServicio;
        private InicioSesionVistaModelo _vistaModelo;

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null) new Application();

            _mockInicioSesion = new Mock<IInicioSesionServicio>();
            _mockCambioContrasena = new Mock<ICambioContrasenaServicio>();
            _mockRecuperacion = new Mock<IRecuperacionCuentaServicio>();
            _mockLocalizacion = new Mock<ILocalizacionServicio>();
            _mockSalasServicio = new Mock<ISalasServicio>();

            _mockLocalizacion.Setup(l => l.CulturaActual).Returns(new CultureInfo("es-MX"));

            _vistaModelo = new InicioSesionVistaModelo(
                _mockInicioSesion.Object,
                _mockCambioContrasena.Object,
                _mockRecuperacion.Object,
                _mockLocalizacion.Object,
                () => _mockSalasServicio.Object 
            );

            AvisoAyudante.DefinirMostrarAviso((msj) => { });
            _vistaModelo.MostrarCamposInvalidos = (_) => { };
            _vistaModelo.CerrarAccion = () => { };
        }

        [TestCleanup]
        public void Limpiar()
        {
            try { SesionUsuarioActual.EstablecerUsuario(new DTOs.UsuarioDTO { UsuarioId = 0 }); } catch { }
            _vistaModelo = null;
        }

        #region 1. Constructor y Validaciones

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_InicioSesionNulo_LanzaExcepcion()
        {
            new InicioSesionVistaModelo(null, _mockCambioContrasena.Object, _mockRecuperacion.Object, _mockLocalizacion.Object, () => _mockSalasServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_CambioContrasenaNulo_LanzaExcepcion()
        {
            new InicioSesionVistaModelo(_mockInicioSesion.Object, null, _mockRecuperacion.Object, _mockLocalizacion.Object, () => _mockSalasServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_RecuperacionNulo_LanzaExcepcion()
        {
            new InicioSesionVistaModelo(_mockInicioSesion.Object, _mockCambioContrasena.Object, null, _mockLocalizacion.Object, () => _mockSalasServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_LocalizacionNulo_LanzaExcepcion()
        {
            new InicioSesionVistaModelo(_mockInicioSesion.Object, _mockCambioContrasena.Object, _mockRecuperacion.Object, null, () => _mockSalasServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_SalasFactoryNulo_LanzaExcepcion()
        {
            new InicioSesionVistaModelo(_mockInicioSesion.Object, _mockCambioContrasena.Object, _mockRecuperacion.Object, _mockLocalizacion.Object, null);
        }

        [TestMethod]
        public void Prueba_Constructor_InicializaIdiomas()
        {
            Assert.IsNotNull(_vistaModelo.IdiomasDisponibles);
            Assert.AreEqual(2, _vistaModelo.IdiomasDisponibles.Count);
            Assert.AreEqual("es-MX", _vistaModelo.IdiomaSeleccionado.Codigo);
        }

        #endregion

        #region 2. Inicio de Sesión (IniciarSesionAsync)

        [TestMethod]
        public async Task Prueba_IniciarSesion_CamposVacios_MuestraErrores()
        {
            _vistaModelo.Identificador = "";
            _vistaModelo.EstablecerContrasena("");

            List<string> invalidos = null;
            _vistaModelo.MostrarCamposInvalidos = (l) => invalidos = l.ToList();

            await _vistaModelo.IniciarSesionComando.EjecutarAsync(null);

            Assert.IsNotNull(invalidos);
            Assert.IsTrue(invalidos.Contains("Identificador"));
            Assert.IsTrue(invalidos.Contains("Contrasena"));
            _mockInicioSesion.Verify(s => s.IniciarSesionAsync(It.IsAny<DTOs.CredencialesInicioSesionDTO>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_IniciarSesion_Exito_ActualizaSesionYNavega()
        {
            _vistaModelo.Identificador = "User";
            _vistaModelo.EstablecerContrasena("Pass");

            var resultadoExito = new DTOs.ResultadoInicioSesionDTO
            {
                InicioSesionExitoso = true,
                Usuario = new DTOs.UsuarioDTO { NombreUsuario = "User", UsuarioId = 1 }
            };

            _mockInicioSesion
                .Setup(s => s.IniciarSesionAsync(It.IsAny<DTOs.CredencialesInicioSesionDTO>()))
                .ReturnsAsync(resultadoExito);

            bool cerrado = false;
            _vistaModelo.CerrarAccion = () => cerrado = true;
            DTOs.ResultadoInicioSesionDTO resultadoRecibido = null;
            _vistaModelo.InicioSesionCompletado = (r) => resultadoRecibido = r;

            await _vistaModelo.IniciarSesionComando.EjecutarAsync(null);

            Assert.IsTrue(cerrado);
            Assert.IsNotNull(resultadoRecibido);
            Assert.AreEqual("User", SesionUsuarioActual.Usuario.NombreUsuario);
        }

        [TestMethod]
        public async Task Prueba_IniciarSesion_CredencialesIncorrectas_MuestraError()
        {
            _vistaModelo.Identificador = "User";
            _vistaModelo.EstablecerContrasena("WrongPass");

            var resultadoFallo = new DTOs.ResultadoInicioSesionDTO
            {
                InicioSesionExitoso = false,
                Mensaje = "CredencialesInvalidas"
            };

            _mockInicioSesion.Setup(s => s.IniciarSesionAsync(It.IsAny<DTOs.CredencialesInicioSesionDTO>())).ReturnsAsync(resultadoFallo);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.IniciarSesionComando.EjecutarAsync(null);

            Assert.AreEqual("CredencialesInvalidas", mensaje);
        }

        [TestMethod]
        public async Task Prueba_IniciarSesion_RespuestaNula_MuestraErrorServidor()
        {
            _vistaModelo.Identificador = "User";
            _vistaModelo.EstablecerContrasena("Pass");
            _mockInicioSesion.Setup(s => s.IniciarSesionAsync(It.IsAny<DTOs.CredencialesInicioSesionDTO>())).ReturnsAsync((DTOs.ResultadoInicioSesionDTO)null);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.IniciarSesionComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoServidorInicioSesion, mensaje);
        }

        [TestMethod]
        public async Task Prueba_IniciarSesion_Excepcion_MuestraMensaje()
        {
            _vistaModelo.Identificador = "User";
            _vistaModelo.EstablecerContrasena("Pass");
            _mockInicioSesion.Setup(s => s.IniciarSesionAsync(It.IsAny<DTOs.CredencialesInicioSesionDTO>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorRed", null));

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.IniciarSesionComando.EjecutarAsync(null);

            Assert.AreEqual("ErrorRed", mensaje);
            Assert.IsFalse(_vistaModelo.EstaProcesando);
        }

        #endregion

        #region 3. Recuperar Cuenta

        [TestMethod]
        public async Task Prueba_RecuperarCuenta_IdentificadorVacio_MuestraError()
        {
            _vistaModelo.Identificador = ""; 
            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.RecuperarCuentaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoIdentificadorRecuperacionRequerido, mensaje);
            _mockRecuperacion.Verify(s => s.RecuperarCuentaAsync(It.IsAny<string>(), It.IsAny<ICambioContrasenaServicio>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_RecuperarCuenta_Exito_LlamaServicio()
        {
            _vistaModelo.Identificador = "UserRecover";
            _mockRecuperacion.Setup(s => s.RecuperarCuentaAsync("UserRecover", It.IsAny<ICambioContrasenaServicio>()))
                .ReturnsAsync(new DTOs.ResultadoOperacionDTO { OperacionExitosa = true });

            await _vistaModelo.RecuperarCuentaComando.EjecutarAsync(null);

            _mockRecuperacion.Verify(s => s.RecuperarCuentaAsync("UserRecover", It.IsAny<ICambioContrasenaServicio>()), Times.Once);
        }

        [TestMethod]
        public async Task Prueba_RecuperarCuenta_Fallo_MuestraMensaje()
        {
            _vistaModelo.Identificador = "User";
            _mockRecuperacion.Setup(s => s.RecuperarCuentaAsync(It.IsAny<string>(), It.IsAny<ICambioContrasenaServicio>()))
                .ReturnsAsync(new DTOs.ResultadoOperacionDTO { OperacionExitosa = false, Mensaje = "NoEncontrado" });

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.RecuperarCuentaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoCuentaNoRegistrada, mensaje);
        }

        [TestMethod]
        public async Task Prueba_RecuperarCuenta_Excepcion_MuestraError()
        {
            _vistaModelo.Identificador = "User";
            _mockRecuperacion.Setup(s => s.RecuperarCuentaAsync(It.IsAny<string>(), It.IsAny<ICambioContrasenaServicio>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorWCF", null));

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.RecuperarCuentaComando.EjecutarAsync(null);

            Assert.AreEqual("ErrorWCF", mensaje);
        }

        #endregion

        #region 4. Modo Invitado (IniciarSesionInvitadoAsync)

        [TestMethod]
        public async Task Prueba_Invitado_FactoryFalla_MuestraError()
        {
            var vmFactoryNull = new InicioSesionVistaModelo(
                _mockInicioSesion.Object, _mockCambioContrasena.Object, _mockRecuperacion.Object, _mockLocalizacion.Object,
                () => null);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await vmFactoryNull.IniciarSesionInvitadoComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoNoEncuentraPartida, mensaje);
            if (vmFactoryNull is IDisposable d) d.Dispose();
        }

        [TestMethod]
        public async Task Prueba_Invitado_Exito_NavegaAJuego()
        {
            bool ventanaJuegoAbierta = false;
            _vistaModelo.AbrirVentanaJuegoInvitado = (s, serv, n) => ventanaJuegoAbierta = true;

            _vistaModelo.MostrarIngresoInvitado = (vm) =>
            {
                typeof(IngresoPartidaInvitadoVistaModelo)
                    .GetProperty("SeUnioSala")
                    ?.SetValue(vm, true);
                vm.SalaUnida?.Invoke(new DTOs.SalaDTO(), "Invitado1");
            };

            await _vistaModelo.IniciarSesionInvitadoComando.EjecutarAsync(null);

            Assert.IsTrue(ventanaJuegoAbierta);
            _mockSalasServicio.Verify(s => s.Dispose(), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_Invitado_Cancelado_HaceDispose()
        {
            _vistaModelo.MostrarIngresoInvitado = (vm) =>
            {
                typeof(IngresoPartidaInvitadoVistaModelo)
                    .GetProperty("SeUnioSala")
                    ?.SetValue(vm, false);
            };

            await _vistaModelo.IniciarSesionInvitadoComando.EjecutarAsync(null);

            _mockSalasServicio.Verify(s => s.Dispose(), Times.Once);
        }

        [TestMethod]
        public async Task Prueba_Invitado_SinAccionMostrar_MuestraErrorYDispose()
        {
            _vistaModelo.MostrarIngresoInvitado = null; 
            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.IniciarSesionInvitadoComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoNoEncuentraPartida, mensaje);
            _mockSalasServicio.Verify(s => s.Dispose(), Times.Once);
        }

        [TestMethod]
        public async Task Prueba_Invitado_Excepcion_MuestraErrorYDispose()
        {
            var vmExplosivo = new InicioSesionVistaModelo(
                 _mockInicioSesion.Object, _mockCambioContrasena.Object, _mockRecuperacion.Object, _mockLocalizacion.Object,
                 () => throw new Exception("Boom"));

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await vmExplosivo.IniciarSesionInvitadoComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoNoEncuentraPartida, mensaje);
            if (vmExplosivo is IDisposable d) d.Dispose();
        }

        #endregion

        #region 5. Idiomas y Comandos Extra

        [TestMethod]
        public void Prueba_IdiomaActualizado_Evento_ActualizaLista()
        {
            _mockLocalizacion.Setup(l => l.CulturaActual).Returns(new CultureInfo("en-US"));
            MethodInfo metodo = typeof(InicioSesionVistaModelo).GetMethod("LocalizacionServicioEnIdiomaActualizado", BindingFlags.NonPublic | BindingFlags.Instance);
            metodo.Invoke(_vistaModelo, new object[] { null, EventArgs.Empty });

            Assert.AreEqual("en-US", _vistaModelo.IdiomaSeleccionado.Codigo);
        }

        [TestMethod]
        public void Prueba_IdiomaSeleccionado_Setter_CambiaIdiomaServicio()
        {
            var nuevoIdioma = _vistaModelo.IdiomasDisponibles.Last();
            _vistaModelo.IdiomaSeleccionado = nuevoIdioma;

            _mockLocalizacion.Verify(l => l.EstablecerIdioma(nuevoIdioma.Codigo), Times.Once);
        }

        [TestMethod]
        public void Prueba_AbrirCrearCuenta_InvocaAccion()
        {
            bool abierto = false;
            _vistaModelo.AbrirCrearCuenta = () => abierto = true;

            _vistaModelo.AbrirCrearCuentaComando.Execute(null);

            Assert.IsTrue(abierto);
        }

        [TestMethod]
        public void Prueba_EstaProcesando_Setter_NotificaComandos()
        {
            typeof(InicioSesionVistaModelo).GetProperty("EstaProcesando").SetValue(_vistaModelo, true);
            Assert.IsFalse(_vistaModelo.IniciarSesionComando.CanExecute(null));

            typeof(InicioSesionVistaModelo).GetProperty("EstaProcesando").SetValue(_vistaModelo, false);
            Assert.IsTrue(_vistaModelo.IniciarSesionComando.CanExecute(null));
        }

        #endregion
    }
}