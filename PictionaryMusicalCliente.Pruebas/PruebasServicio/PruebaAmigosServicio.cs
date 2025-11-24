using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasServicio
{
    [TestClass]
    public class PruebaAmigosServicio
    {
        private Mock<PictionaryServidorServicioAmigos.AmigosManejadorClient> _mockCliente;
        private TestableAmigosServicio _servicio;

        [TestInitialize]
        public void Inicializar()
        {
            _mockCliente = new Mock<PictionaryServidorServicioAmigos.AmigosManejadorClient>();
        }

        [TestCleanup]
        public void Limpiar()
        {
            _servicio?.Dispose();
            _servicio = null;
        }

        #region Pruebas de Suscripción

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SuscribirAsync_NombreUsuarioVacio_LanzaExcepcion()
        {
            _servicio = new TestableAmigosServicio();
            await _servicio.SuscribirAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SuscribirAsync_NombreUsuarioNulo_LanzaExcepcion()
        {
            _servicio = new TestableAmigosServicio();
            await _servicio.SuscribirAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SuscribirAsync_NombreUsuarioEspacios_LanzaExcepcion()
        {
            _servicio = new TestableAmigosServicio();
            await _servicio.SuscribirAsync("   ");
        }

        [TestMethod]
        public async Task SuscribirAsync_PrimeraVez_CreaClienteYSuscribe()
        {
            _mockCliente.Setup(c => c.SuscribirAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableAmigosServicio(_mockCliente.Object);

            await _servicio.SuscribirAsync("Usuario1");

            _mockCliente.Verify(c => c.SuscribirAsync("Usuario1"), Times.Once);
        }

        [TestMethod]
        public async Task SuscribirAsync_MismoUsuarioDosVeces_NoSuscribeDeNuevo()
        {
            _mockCliente.Setup(c => c.SuscribirAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableAmigosServicio(_mockCliente.Object);

            await _servicio.SuscribirAsync("Usuario1");
            await _servicio.SuscribirAsync("Usuario1");

            _mockCliente.Verify(c => c.SuscribirAsync("Usuario1"), Times.Once);
        }

        [TestMethod]
        public async Task SuscribirAsync_FaultException_LanzaServicioExcepcion()
        {
            _mockCliente.Setup(c => c.SuscribirAsync(It.IsAny<string>()))
                .ThrowsAsync(new FaultException("Error del servidor"));
            _mockCliente.Setup(c => c.Abort());

            _servicio = new TestableAmigosServicio(_mockCliente.Object);

            try
            {
                await _servicio.SuscribirAsync("Usuario1");
                Assert.Fail("Debería lanzar ServicioExcepcion");
            }
            catch (ServicioExcepcion ex)
            {
                Assert.AreEqual(TipoErrorServicio.FallaServicio, ex.TipoError);
                _mockCliente.Verify(c => c.Abort(), Times.Once);
            }
        }

        [TestMethod]
        public async Task SuscribirAsync_EndpointNotFoundException_LanzaServicioExcepcion()
        {
            _mockCliente.Setup(c => c.SuscribirAsync(It.IsAny<string>()))
                .ThrowsAsync(new EndpointNotFoundException("No se encontró el endpoint"));
            _mockCliente.Setup(c => c.Abort());

            _servicio = new TestableAmigosServicio(_mockCliente.Object);

            try
            {
                await _servicio.SuscribirAsync("Usuario1");
                Assert.Fail("Debería lanzar ServicioExcepcion");
            }
            catch (ServicioExcepcion ex)
            {
                Assert.AreEqual(TipoErrorServicio.Comunicacion, ex.TipoError);
                _mockCliente.Verify(c => c.Abort(), Times.Once);
            }
        }

        [TestMethod]
        public async Task SuscribirAsync_TimeoutException_LanzaServicioExcepcion()
        {
            _mockCliente.Setup(c => c.SuscribirAsync(It.IsAny<string>()))
                .ThrowsAsync(new TimeoutException("Tiempo agotado"));
            _mockCliente.Setup(c => c.Abort());

            _servicio = new TestableAmigosServicio(_mockCliente.Object);

            try
            {
                await _servicio.SuscribirAsync("Usuario1");
                Assert.Fail("Debería lanzar ServicioExcepcion");
            }
            catch (ServicioExcepcion ex)
            {
                Assert.AreEqual(TipoErrorServicio.TiempoAgotado, ex.TipoError);
                _mockCliente.Verify(c => c.Abort(), Times.Once);
            }
        }

        #endregion

        #region Pruebas de Cancelación de Suscripción

        [TestMethod]
        public async Task CancelarSuscripcionAsync_NombreUsuarioVacio_NoHaceNada()
        {
            _servicio = new TestableAmigosServicio(_mockCliente.Object);
            await _servicio.CancelarSuscripcionAsync(string.Empty);
            _mockCliente.Verify(c => c.CancelarSuscripcionAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task CancelarSuscripcionAsync_NombreUsuarioNulo_NoHaceNada()
        {
            _servicio = new TestableAmigosServicio(_mockCliente.Object);
            await _servicio.CancelarSuscripcionAsync(null);
            _mockCliente.Verify(c => c.CancelarSuscripcionAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task CancelarSuscripcionAsync_SinSuscripcion_NoHaceNada()
        {
            _servicio = new TestableAmigosServicio(_mockCliente.Object);
            await _servicio.CancelarSuscripcionAsync("Usuario1");
            _mockCliente.Verify(c => c.CancelarSuscripcionAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task CancelarSuscripcionAsync_ConSuscripcion_CancelaCorrectamente()
        {
            _mockCliente.Setup(c => c.SuscribirAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);
            _mockCliente.Setup(c => c.CancelarSuscripcionAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCliente.Setup(c => c.Close());

            _servicio = new TestableAmigosServicio(_mockCliente.Object);
            await _servicio.SuscribirAsync("Usuario1");
            await _servicio.CancelarSuscripcionAsync("Usuario1");

            _mockCliente.Verify(c => c.CancelarSuscripcionAsync("Usuario1"), Times.Once);
        }

        [TestMethod]
        public async Task CancelarSuscripcionAsync_UsuarioDiferente_NoHaceNada()
        {
            _mockCliente.Setup(c => c.SuscribirAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableAmigosServicio(_mockCliente.Object);
            await _servicio.SuscribirAsync("Usuario1");
            await _servicio.CancelarSuscripcionAsync("Usuario2");

            _mockCliente.Verify(c => c.CancelarSuscripcionAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region Pruebas de Operaciones de Amistad

        [TestMethod]
        public async Task EnviarSolicitudAsync_LlamaClienteCorrectamente()
        {
            _mockCliente.Setup(c => c.EnviarSolicitudAmistadAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableAmigosServicio(_mockCliente.Object);
            await _servicio.EnviarSolicitudAsync("Usuario1", "Usuario2");

            _mockCliente.Verify(c => c.EnviarSolicitudAmistadAsync("Usuario1", "Usuario2"), Times.Once);
        }

        [TestMethod]
        public async Task ResponderSolicitudAsync_LlamaClienteCorrectamente()
        {
            _mockCliente.Setup(c => c.ResponderSolicitudAmistadAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableAmigosServicio(_mockCliente.Object);
            await _servicio.ResponderSolicitudAsync("Usuario1", "Usuario2");

            _mockCliente.Verify(c => c.ResponderSolicitudAmistadAsync("Usuario1", "Usuario2"), Times.Once);
        }

        [TestMethod]
        public async Task EliminarAmigoAsync_LlamaClienteCorrectamente()
        {
            _mockCliente.Setup(c => c.EliminarAmigoAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableAmigosServicio(_mockCliente.Object);
            await _servicio.EliminarAmigoAsync("Usuario1", "Usuario2");

            _mockCliente.Verify(c => c.EliminarAmigoAsync("Usuario1", "Usuario2"), Times.Once);
        }

        [TestMethod]
        public async Task EnviarSolicitudAsync_FaultException_LanzaServicioExcepcion()
        {
            _mockCliente.Setup(c => c.EnviarSolicitudAmistadAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new FaultException("Error"));
            _mockCliente.Setup(c => c.Abort());

            _servicio = new TestableAmigosServicio(_mockCliente.Object);

            try
            {
                await _servicio.EnviarSolicitudAsync("Usuario1", "Usuario2");
                Assert.Fail("Debería lanzar ServicioExcepcion");
            }
            catch (ServicioExcepcion ex)
            {
                Assert.AreEqual(TipoErrorServicio.FallaServicio, ex.TipoError);
            }
        }

        #endregion

        #region Pruebas de Callbacks

        [TestMethod]
        public void NotificarSolicitudActualizada_SolicitudNula_NoHaceNada()
        {
            _servicio = new TestableAmigosServicio();
            bool eventoDisparado = false;
            _servicio.SolicitudesActualizadas += (s, e) => eventoDisparado = true;

            _servicio.NotificarSolicitudActualizada(null);

            Assert.IsFalse(eventoDisparado);
        }

        [TestMethod]
        public void NotificarSolicitudActualizada_SolicitudValida_DisparaEvento()
        {
            _mockCliente.Setup(c => c.SuscribirAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableAmigosServicio(_mockCliente.Object);
            _servicio.SuscribirAsync("Usuario1").Wait();

            bool eventoDisparado = false;
            IReadOnlyCollection<DTOs.SolicitudAmistadDTO> solicitudes = null;
            _servicio.SolicitudesActualizadas += (s, e) =>
            {
                eventoDisparado = true;
                solicitudes = e;
            };

            var solicitud = new DTOs.SolicitudAmistadDTO
            {
                UsuarioEmisor = "Usuario2",
                UsuarioReceptor = "Usuario1",
                SolicitudAceptada = false
            };

            _servicio.NotificarSolicitudActualizada(solicitud);

            Assert.IsTrue(eventoDisparado);
            Assert.IsNotNull(solicitudes);
        }

        [TestMethod]
        public void NotificarAmistadEliminada_SolicitudValida_DisparaEvento()
        {
            _mockCliente.Setup(c => c.SuscribirAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableAmigosServicio(_mockCliente.Object);
            _servicio.SuscribirAsync("Usuario1").Wait();

            var solicitud = new DTOs.SolicitudAmistadDTO
            {
                UsuarioEmisor = "Usuario2",
                UsuarioReceptor = "Usuario1",
                SolicitudAceptada = false
            };
            _servicio.NotificarSolicitudActualizada(solicitud);

            bool eventoDisparado = false;
            _servicio.SolicitudesActualizadas += (s, e) => eventoDisparado = true;

            _servicio.NotificarAmistadEliminada(solicitud);

            Assert.IsTrue(eventoDisparado);
        }

        [TestMethod]
        public void SolicitudesPendientes_IniciaSinSolicitudes()
        {
            _servicio = new TestableAmigosServicio();
            var solicitudes = _servicio.SolicitudesPendientes;

            Assert.IsNotNull(solicitudes);
            Assert.AreEqual(0, solicitudes.Count);
        }

        #endregion

        #region Pruebas de Dispose

        [TestMethod]
        public void Dispose_LiberaRecursos()
        {
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);
            _mockCliente.Setup(c => c.Close());

            _servicio = new TestableAmigosServicio(_mockCliente.Object);
            _servicio.Dispose();

            // Verificar que no se puede usar después del dispose
            var solicitudes = _servicio.SolicitudesPendientes;
            Assert.IsNotNull(solicitudes);
        }

        [TestMethod]
        public void Dispose_MultiplesLlamadas_NoGeneraError()
        {
            _servicio = new TestableAmigosServicio();
            _servicio.Dispose();
            _servicio.Dispose();
            // No debería lanzar excepción
        }

        #endregion
    }

    /// <summary>
    /// Clase testable que permite inyectar un mock del cliente WCF
    /// </summary>
    internal class TestableAmigosServicio : AmigosServicio
    {
        private readonly PictionaryServidorServicioAmigos.AmigosManejadorClient _clienteMock;

        public TestableAmigosServicio() : base()
        {
        }

        public TestableAmigosServicio(PictionaryServidorServicioAmigos.AmigosManejadorClient clienteMock)
        {
            _clienteMock = clienteMock;
        }

        // Sobrescribe el método de creación para devolver el mock
        protected override PictionaryServidorServicioAmigos.AmigosManejadorClient CrearCliente()
        {
            return _clienteMock ?? base.CrearCliente();
        }
    }
}
