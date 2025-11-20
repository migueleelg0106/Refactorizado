using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows; 
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Perfil
{
    [TestClass]
    public class PruebaVerificacionCodigoVistaModelo
    {
        private Mock<ICodigoVerificacionServicio> _mockServicio;
        private VerificacionCodigoVistaModelo _vistaModelo;
        private const string TokenPrueba = "TOKEN_TEST";

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null) new Application();

            _mockServicio = new Mock<ICodigoVerificacionServicio>();

            AvisoAyudante.DefinirMostrarAviso((m) => { });

            _vistaModelo = new VerificacionCodigoVistaModelo(
                "Descripcion prueba",
                TokenPrueba,
                _mockServicio.Object
            );
        }

        [TestCleanup]
        public void Limpiar()
        {
            DetenerTimer("_temporizadorReenvio");
            DetenerTimer("_temporizadorExpiracion");
            _vistaModelo = null;
        }

        private void DetenerTimer(string nombreCampo)
        {
            try
            {
                var campo = typeof(VerificacionCodigoVistaModelo).GetField(nombreCampo, BindingFlags.NonPublic | BindingFlags.Instance);
                if (campo?.GetValue(_vistaModelo) is System.Windows.Threading.DispatcherTimer timer && timer.IsEnabled)
                {
                    timer.Stop();
                }
            }
            catch { }
        }

        #region 1. Constructor y Validaciones

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_DescripcionNula_LanzaExcepcion()
        {
            new VerificacionCodigoVistaModelo(null, TokenPrueba, _mockServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_TokenNulo_LanzaExcepcion()
        {
            new VerificacionCodigoVistaModelo("Desc", null, _mockServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_ServicioNulo_LanzaExcepcion()
        {
            new VerificacionCodigoVistaModelo("Desc", TokenPrueba, null);
        }

        [TestMethod]
        public void Prueba_Constructor_InicializacionCorrecta()
        {
            Assert.AreEqual("Descripcion prueba", _vistaModelo.Descripcion);
            Assert.IsFalse(_vistaModelo.PuedeReenviar); 
            Assert.IsNotNull(_vistaModelo.TextoBotonReenviar);
            Assert.IsNotNull(_vistaModelo.VerificarCodigoComando);
            Assert.IsNotNull(_vistaModelo.ReenviarCodigoComando);
            Assert.IsNotNull(_vistaModelo.CancelarComando);
        }

        #endregion

        #region 2. Propiedades

        [TestMethod]
        public void Prueba_CodigoVerificacion_Setter_GuardaValor()
        {
            _vistaModelo.CodigoVerificacion = "123456";
            Assert.AreEqual("123456", _vistaModelo.CodigoVerificacion);
        }

        [TestMethod]
        public void Prueba_EstaVerificando_Setter_NotificaComando()
        {
            typeof(VerificacionCodigoVistaModelo).GetProperty("EstaVerificando").SetValue(_vistaModelo, true);
            Assert.IsFalse(_vistaModelo.ReenviarCodigoComando.CanExecute(null)); 

            typeof(VerificacionCodigoVistaModelo).GetProperty("EstaVerificando").SetValue(_vistaModelo, false);
        }

        #endregion

        #region 3. Verificar Código (VerificarCodigoAsync)

        [TestMethod]
        public async Task Prueba_Verificar_CodigoVacio_MuestraError()
        {
            _vistaModelo.CodigoVerificacion = "";
            bool invalidoMarcado = false;
            _vistaModelo.MarcarCodigoInvalido = (v) => invalidoMarcado = v;
            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.VerificarCodigoComando.EjecutarAsync(null);

            Assert.IsTrue(invalidoMarcado);
            Assert.AreEqual(Lang.errorTextoCodigoVerificacionRequerido, mensaje);
            _mockServicio.Verify(s => s.ConfirmarCodigoRegistroAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_Verificar_Exito_CompletaProceso()
        {
            _vistaModelo.CodigoVerificacion = "123456";
            var resultadoExito = new DTOs.ResultadoRegistroCuentaDTO { RegistroExitoso = true };

            _mockServicio.Setup(s => s.ConfirmarCodigoRegistroAsync(TokenPrueba, "123456"))
                .ReturnsAsync(resultadoExito);

            DTOs.ResultadoRegistroCuentaDTO resultadoRecibido = null;
            _vistaModelo.VerificacionCompletada = (r) => resultadoRecibido = r;
            bool invalidoMarcado = true; 
            _vistaModelo.MarcarCodigoInvalido = (v) => invalidoMarcado = v;

            await _vistaModelo.VerificarCodigoComando.EjecutarAsync(null);

            Assert.IsNotNull(resultadoRecibido);
            Assert.IsTrue(resultadoRecibido.RegistroExitoso);
            Assert.IsFalse(invalidoMarcado);
        }

        [TestMethod]
        public async Task Prueba_Verificar_CodigoIncorrecto_MuestraError()
        {
            _vistaModelo.CodigoVerificacion = "MAL";
            var resultadoFallo = new DTOs.ResultadoRegistroCuentaDTO { RegistroExitoso = false, Mensaje = "Invalido" };

            _mockServicio.Setup(s => s.ConfirmarCodigoRegistroAsync(TokenPrueba, "MAL"))
                .ReturnsAsync(resultadoFallo);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);
            bool invalidoMarcado = false;
            _vistaModelo.MarcarCodigoInvalido = (v) => invalidoMarcado = v;

            await _vistaModelo.VerificarCodigoComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoCodigoIncorrecto, mensaje);
            Assert.IsTrue(invalidoMarcado);
        }

        [TestMethod]
        public async Task Prueba_Verificar_CodigoExpirado_FinalizaFlujo()
        {
            _vistaModelo.CodigoVerificacion = "OLD";
            var resultadoFallo = new DTOs.ResultadoRegistroCuentaDTO
            {
                RegistroExitoso = false,
                Mensaje = Lang.avisoTextoCodigoExpirado
            };

            _mockServicio.Setup(s => s.ConfirmarCodigoRegistroAsync(TokenPrueba, "OLD"))
                .ReturnsAsync(resultadoFallo);

            bool completado = false;
            _vistaModelo.VerificacionCompletada = (_) => completado = true;

            await _vistaModelo.VerificarCodigoComando.EjecutarAsync(null);

            Assert.IsTrue(completado, "Debe invocar VerificacionCompletada por expiración.");
        }

        [TestMethod]
        public async Task Prueba_Verificar_RespuestaNula_MuestraError()
        {
            _vistaModelo.CodigoVerificacion = "ANY";
            _mockServicio.Setup(s => s.ConfirmarCodigoRegistroAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((DTOs.ResultadoRegistroCuentaDTO)null);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.VerificarCodigoComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoVerificarCodigo, mensaje);
        }

        [TestMethod]
        public async Task Prueba_Verificar_Excepcion_MuestraError()
        {
            _vistaModelo.CodigoVerificacion = "ANY";
            _mockServicio.Setup(s => s.ConfirmarCodigoRegistroAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorRed", null));

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.VerificarCodigoComando.EjecutarAsync(null);

            Assert.AreEqual("ErrorRed", mensaje);
        }

        #endregion

        #region 4. Reenviar Código (ReenviarCodigoAsync)

        [TestMethod]
        public async Task Prueba_Reenviar_SiNoPuede_NoHaceNada()
        {
            typeof(VerificacionCodigoVistaModelo).GetProperty("PuedeReenviar").SetValue(_vistaModelo, false);

            await _vistaModelo.ReenviarCodigoComando.EjecutarAsync(null);

            _mockServicio.Verify(s => s.ReenviarCodigoRegistroAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_Reenviar_Exito_ReiniciaTimers()
        {
            typeof(VerificacionCodigoVistaModelo).GetProperty("PuedeReenviar").SetValue(_vistaModelo, true);

            var resultado = new DTOs.ResultadoSolicitudCodigoDTO { CodigoEnviado = true, TokenCodigo = "NEW_TOKEN" };
            _mockServicio.Setup(s => s.ReenviarCodigoRegistroAsync(TokenPrueba)).ReturnsAsync(resultado);

            await _vistaModelo.ReenviarCodigoComando.EjecutarAsync(null);

            _mockServicio.Verify(s => s.ReenviarCodigoRegistroAsync(TokenPrueba), Times.Once);

            // Verificamos que se reinició el bloqueo (timer reiniciado)
            Assert.IsFalse(_vistaModelo.PuedeReenviar, "Al reenviar, debe bloquearse de nuevo el botón");
        }

        [TestMethod]
        public async Task Prueba_Reenviar_Fallo_MuestraError()
        {
            typeof(VerificacionCodigoVistaModelo).GetProperty("PuedeReenviar").SetValue(_vistaModelo, true);

            var resultado = new DTOs.ResultadoSolicitudCodigoDTO { CodigoEnviado = false, Mensaje = "FalloEnvio" };
            _mockServicio.Setup(s => s.ReenviarCodigoRegistroAsync(TokenPrueba)).ReturnsAsync(resultado);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.ReenviarCodigoComando.EjecutarAsync(null);

            Assert.AreEqual("FalloEnvio", mensaje);
        }

        [TestMethod]
        public async Task Prueba_Reenviar_Excepcion_MuestraError()
        {
            typeof(VerificacionCodigoVistaModelo).GetProperty("PuedeReenviar").SetValue(_vistaModelo, true);
            _mockServicio.Setup(s => s.ReenviarCodigoRegistroAsync(TokenPrueba))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "Error", null));

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.ReenviarCodigoComando.EjecutarAsync(null);

            Assert.AreEqual("Error", mensaje);
        }

        #endregion

        #region 5. Timers y Cancelación (Reflection)

        [TestMethod]
        public void Prueba_TemporizadorReenvio_Tick_DecrementaYHabilita()
        {
            SetCampoPrivado("_segundosRestantes", 0);

            InvocarMetodoPrivado("TemporizadorReenvioTick", new object[] { null, EventArgs.Empty });

            Assert.IsTrue(_vistaModelo.PuedeReenviar);
            Assert.AreEqual(Lang.cambiarContrasenaTextoReenviarCodigo, _vistaModelo.TextoBotonReenviar);
        }

        [TestMethod]
        public void Prueba_TemporizadorReenvio_Tick_DecrementaNormal()
        {
            SetCampoPrivado("_segundosRestantes", 30);

            InvocarMetodoPrivado("TemporizadorReenvioTick", new object[] { null, EventArgs.Empty });

            int segundosActuales = (int)GetCampoPrivado("_segundosRestantes");
            Assert.AreEqual(29, segundosActuales);
            Assert.IsFalse(_vistaModelo.PuedeReenviar);
        }

        [TestMethod]
        public void Prueba_TemporizadorExpiracion_Tick_Cancela()
        {
            bool cancelado = false;
            _vistaModelo.Cancelado = () => cancelado = true;
            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            InvocarMetodoPrivado("TemporizadorExpiracionTick", new object[] { null, EventArgs.Empty });

            Assert.IsTrue(cancelado);
            Assert.AreEqual(Lang.avisoTextoCodigoExpirado, mensaje);
        }

        [TestMethod]
        public void Prueba_CancelarComando_DetieneTimersYNotifica()
        {
            bool cancelado = false;
            _vistaModelo.Cancelado = () => cancelado = true;

            _vistaModelo.CancelarComando.Execute(null);

            Assert.IsTrue(cancelado);
        }

        #endregion

        private void SetCampoPrivado(string nombre, object valor)
        {
            typeof(VerificacionCodigoVistaModelo).GetField(nombre, BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(_vistaModelo, valor);
        }

        private object GetCampoPrivado(string nombre)
        {
            return typeof(VerificacionCodigoVistaModelo).GetField(nombre, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(_vistaModelo);
        }

        private void InvocarMetodoPrivado(string nombre, object[] parametros)
        {
            typeof(VerificacionCodigoVistaModelo).GetMethod(nombre, BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(_vistaModelo, parametros);
        }
    }
}