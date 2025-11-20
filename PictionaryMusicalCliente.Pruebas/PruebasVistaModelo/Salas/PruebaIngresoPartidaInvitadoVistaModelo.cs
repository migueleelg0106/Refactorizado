using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.Salas;
using System;
using System.Threading.Tasks;
using System.Windows; 
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Salas
{
    [TestClass]
    public class PruebaIngresoPartidaInvitadoVistaModelo
    {
        private Mock<ILocalizacionServicio> _mockLocalizacion;
        private Mock<ISalasServicio> _mockSalasServicio;
        private IngresoPartidaInvitadoVistaModelo _vistaModelo;

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null) new Application();

            _mockLocalizacion = new Mock<ILocalizacionServicio>();
            _mockSalasServicio = new Mock<ISalasServicio>();

            _mockLocalizacion.Setup(l => l.CulturaActual).Returns(System.Globalization.CultureInfo.InvariantCulture);
            AvisoAyudante.DefinirMostrarAviso((_) => { });

            _vistaModelo = new IngresoPartidaInvitadoVistaModelo(
                _mockLocalizacion.Object,
                _mockSalasServicio.Object
            );
        }

        [TestCleanup]
        public void Limpiar()
        {
            _vistaModelo = null;
        }

        #region 1. Constructor y Validaciones

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_LocalizacionNula_LanzaExcepcion()
        {
            new IngresoPartidaInvitadoVistaModelo(null, _mockSalasServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_SalasServicioNulo_LanzaExcepcion()
        {
            new IngresoPartidaInvitadoVistaModelo(_mockLocalizacion.Object, null);
        }

        [TestMethod]
        public void Prueba_Constructor_InicializacionCorrecta()
        {
            Assert.IsNotNull(_vistaModelo.UnirseSalaComando);
            Assert.IsNotNull(_vistaModelo.CancelarComando);
            Assert.IsFalse(_vistaModelo.EstaProcesando);
            Assert.IsFalse(_vistaModelo.SeUnioSala);
        }

        #endregion

        #region 2. Validaciones de Entrada (UnirseSalaComoInvitadoAsync)

        [TestMethod]
        public async Task Prueba_UnirseSala_CodigoVacio_MuestraError()
        {
            _vistaModelo.CodigoSala = "   ";
            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.unirseSalaTextoVacio, mensaje);
            _mockSalasServicio.Verify(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_YaProcesando_NoHaceNada()
        {
            typeof(IngresoPartidaInvitadoVistaModelo).GetProperty("EstaProcesando").SetValue(_vistaModelo, true);
            _vistaModelo.CodigoSala = "123456";

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            _mockSalasServicio.Verify(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region 3. Flujos de Unión (IntentarUnirseSalaAsync)

        [TestMethod]
        public async Task Prueba_UnirseSala_Exito_NavegaYCierra()
        {
            _vistaModelo.CodigoSala = "123456";
            var sala = new DTOs.SalaDTO { Codigo = "123456", Jugadores = new[] { "Host" } };

            _mockSalasServicio
                .Setup(s => s.UnirseSalaAsync("123456", It.IsAny<string>())) 
                .ReturnsAsync(sala);

            bool cerrado = false;
            DTOs.SalaDTO salaUnida = null;
            _vistaModelo.CerrarVentana = () => cerrado = true;
            _vistaModelo.SalaUnida = (s, n) => salaUnida = s;

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.IsTrue(cerrado);
            Assert.IsTrue(_vistaModelo.SeUnioSala);
            Assert.AreEqual(sala, salaUnida);
            Assert.IsFalse(_vistaModelo.EstaProcesando);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_SalaNula_MuestraError()
        {
            _vistaModelo.CodigoSala = "123456";
            _mockSalasServicio.Setup(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((DTOs.SalaDTO)null);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoNoEncuentraPartida, mensaje);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_SalaLlena_AbandonaYMuestraError()
        {
            _vistaModelo.CodigoSala = "123456";
            var salaLlena = new DTOs.SalaDTO
            {
                Codigo = "123456",
                Jugadores = new[] { "1", "2", "3", "4", "Yo" } 
            };

            _mockSalasServicio.Setup(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(salaLlena);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            _mockSalasServicio.Verify(s => s.AbandonarSalaAsync("123456", It.IsAny<string>()), Times.Once);
            Assert.AreEqual(Lang.errorTextoSalaLlena, mensaje);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_NombreDuplicado_Reintenta()
        {
            _vistaModelo.CodigoSala = "123456";

            _mockSalasServicio
                .Setup(s => s.UnirseSalaAsync("123456", It.IsAny<string>()))
                .Returns((string c, string n) =>
                {
                    if (n.Contains("1") || n == "Invitado 1") 
                    {
                        return Task.FromResult(new DTOs.SalaDTO
                        {
                            Codigo = c,
                            Jugadores = new[] { "Host", n } 
                        });
                    }
                    return Task.FromResult(new DTOs.SalaDTO { Codigo = c, Jugadores = new[] { "Host", n } });
                });

            int llamadas = 0;
            _mockSalasServicio.Setup(s => s.UnirseSalaAsync("123456", It.IsAny<string>()))
                .Returns((string c, string n) =>
                {
                    llamadas++;
                    if (llamadas == 1)
                    {
                        return Task.FromResult(new DTOs.SalaDTO { Codigo = c, Jugadores = new[] { n, n } }); // Duplicado real
                    }
                    return Task.FromResult(new DTOs.SalaDTO { Codigo = c, Jugadores = new[] { n } });
                });

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.IsTrue(llamadas >= 2);
            _mockSalasServicio.Verify(s => s.AbandonarSalaAsync("123456", It.IsAny<string>()), Times.AtLeastOnce);
            Assert.IsTrue(_vistaModelo.SeUnioSala);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_AgotaIntentos_MuestraError()
        {
            _vistaModelo.CodigoSala = "123456";

            _mockSalasServicio
                .Setup(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string codigo, string nombreGenerado) =>
                {
                    return Task.FromResult(new DTOs.SalaDTO
                    {
                        Codigo = codigo,
                        Jugadores = new[] { "Host", nombreGenerado, nombreGenerado }
                    });
                });

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoNombresInvitadoAgotados, mensaje);
        }

        #endregion

        #region 4. Manejo de Excepciones de Servicio

        [TestMethod]
        public async Task Prueba_UnirseSala_ExcepcionSalaLlena_MuestraMensaje()
        {
            _vistaModelo.CodigoSala = "123456";
            var ex = new ServicioExcepcion(TipoErrorServicio.OperacionInvalida, Lang.errorTextoSalaLlena, null);
            _mockSalasServicio.Setup(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(ex);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoSalaLlena, mensaje);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_ExcepcionNoEncontrada_MuestraMensaje()
        {
            _vistaModelo.CodigoSala = "123456";
            var ex = new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorX", null); 
            _mockSalasServicio.Setup(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(ex);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoNoEncuentraPartida, mensaje);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_ExcepcionGenerica_MuestraMensajeOriginal()
        {
            _vistaModelo.CodigoSala = "123456";
            var ex = new ServicioExcepcion(TipoErrorServicio.Desconocido, "ErrorCustom", null);
            _mockSalasServicio.Setup(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(ex);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual("ErrorCustom", mensaje);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_ExcepcionSistema_MuestraErrorDefecto()
        {
            _vistaModelo.CodigoSala = "123456";
            _mockSalasServicio.Setup(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(
                    TipoErrorServicio.FallaServicio,
                    "Error simulado para forzar falla de servicio",
                    new Exception("Boom")));

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoNoEncuentraPartida, mensaje);
        }

        #endregion

        #region 5. Comandos y Lógica Auxiliar

        [TestMethod]
        public void Prueba_CancelarComando_CierraVentana()
        {
            bool cerrado = false;
            _vistaModelo.CerrarVentana = () => cerrado = true;

            _vistaModelo.CancelarComando.Execute(null);

            Assert.IsTrue(cerrado);
        }

        [TestMethod]
        public async Task Prueba_AbandonarSala_FallaSilenciosamente()
        {

            int llamadas = 0;
            _mockSalasServicio.Setup(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string c, string n) =>
                {
                    llamadas++;
                    if (llamadas == 1) return Task.FromResult(new DTOs.SalaDTO { Codigo = c, Jugadores = new[] { n, n } });
                    return Task.FromResult(new DTOs.SalaDTO { Codigo = c, Jugadores = new[] { n } });
                });

            _mockSalasServicio.Setup(s => s.AbandonarSalaAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Fallo red al abandonar"));

            _vistaModelo.CodigoSala = "CODE";
            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.IsTrue(_vistaModelo.SeUnioSala);
        }

        #endregion
    }
}