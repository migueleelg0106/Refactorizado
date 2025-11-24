using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasServicio
{
    [TestClass]
    public class PruebaListaAmigosServicio
    {
        private Mock<PictionaryServidorServicioListaAmigos.ListaAmigosManejadorClient> _mockCliente;
        private TestableListaAmigosServicio _servicio;

        [TestInitialize]
        public void Inicializar()
        {
            _mockCliente = new Mock<PictionaryServidorServicioListaAmigos.ListaAmigosManejadorClient>();
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
            _servicio = new TestableListaAmigosServicio();
            await _servicio.SuscribirAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SuscribirAsync_NombreUsuarioNulo_LanzaExcepcion()
        {
            _servicio = new TestableListaAmigosServicio();
            await _servicio.SuscribirAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SuscribirAsync_NombreUsuarioEspacios_LanzaExcepcion()
        {
            _servicio = new TestableListaAmigosServicio();
            await _servicio.SuscribirAsync("   ");
        }

        [TestMethod]
        public async Task SuscribirAsync_PrimeraVez_CreaClienteYSuscribe()
        {
            _mockCliente.Setup(c => c.SuscribirAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);

            await _servicio.SuscribirAsync("Usuario1");

            _mockCliente.Verify(c => c.SuscribirAsync("Usuario1"), Times.Once);
        }

        [TestMethod]
        public async Task SuscribirAsync_MismoUsuarioDosVeces_NoSuscribeDeNuevo()
        {
            _mockCliente.Setup(c => c.SuscribirAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);

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

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);

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

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);

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

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);

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

        [TestMethod]
        public async Task SuscribirAsync_CommunicationException_LanzaServicioExcepcion()
        {
            _mockCliente.Setup(c => c.SuscribirAsync(It.IsAny<string>()))
                .ThrowsAsync(new CommunicationException("Error de comunicación"));
            _mockCliente.Setup(c => c.Abort());

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);

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

        #endregion

        #region Pruebas de Cancelación de Suscripción

        [TestMethod]
        public async Task CancelarSuscripcionAsync_NombreUsuarioVacio_NoHaceNada()
        {
            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);
            await _servicio.CancelarSuscripcionAsync(string.Empty);
            _mockCliente.Verify(c => c.CancelarSuscripcionAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task CancelarSuscripcionAsync_NombreUsuarioNulo_NoHaceNada()
        {
            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);
            await _servicio.CancelarSuscripcionAsync(null);
            _mockCliente.Verify(c => c.CancelarSuscripcionAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task CancelarSuscripcionAsync_SinSuscripcion_NoHaceNada()
        {
            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);
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

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);
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

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);
            await _servicio.SuscribirAsync("Usuario1");
            await _servicio.CancelarSuscripcionAsync("Usuario2");

            _mockCliente.Verify(c => c.CancelarSuscripcionAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region Pruebas de ObtenerAmigosAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ObtenerAmigosAsync_NombreUsuarioVacio_LanzaExcepcion()
        {
            _servicio = new TestableListaAmigosServicio();
            await _servicio.ObtenerAmigosAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ObtenerAmigosAsync_NombreUsuarioNulo_LanzaExcepcion()
        {
            _servicio = new TestableListaAmigosServicio();
            await _servicio.ObtenerAmigosAsync(null);
        }

        [TestMethod]
        public async Task ObtenerAmigosAsync_ConAmigos_DevuelveListaCorrectamente()
        {
            var amigos = new[]
            {
                new DTOs.AmigoDTO { UsuarioId = 1, NombreUsuario = "Amigo1" },
                new DTOs.AmigoDTO { UsuarioId = 2, NombreUsuario = "Amigo2" },
                new DTOs.AmigoDTO { UsuarioId = 3, NombreUsuario = "Amigo3" }
            };

            _mockCliente.Setup(c => c.ObtenerAmigosAsync(It.IsAny<string>()))
                .ReturnsAsync(amigos);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);
            var resultado = await _servicio.ObtenerAmigosAsync("Usuario1");

            Assert.AreEqual(3, resultado.Count);
            Assert.AreEqual("Amigo1", resultado[0].NombreUsuario);
            Assert.AreEqual("Amigo2", resultado[1].NombreUsuario);
            Assert.AreEqual("Amigo3", resultado[2].NombreUsuario);
        }

        [TestMethod]
        public async Task ObtenerAmigosAsync_SinAmigos_DevuelveListaVacia()
        {
            _mockCliente.Setup(c => c.ObtenerAmigosAsync(It.IsAny<string>()))
                .ReturnsAsync(new DTOs.AmigoDTO[0]);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);
            var resultado = await _servicio.ObtenerAmigosAsync("Usuario1");

            Assert.AreEqual(0, resultado.Count);
        }

        [TestMethod]
        public async Task ObtenerAmigosAsync_ConAmigosNulos_FiltrarCorrectamente()
        {
            var amigos = new[]
            {
                new DTOs.AmigoDTO { UsuarioId = 1, NombreUsuario = "Amigo1" },
                null,
                new DTOs.AmigoDTO { UsuarioId = 2, NombreUsuario = "Amigo2" },
                new DTOs.AmigoDTO { UsuarioId = 3, NombreUsuario = "" }
            };

            _mockCliente.Setup(c => c.ObtenerAmigosAsync(It.IsAny<string>()))
                .ReturnsAsync(amigos);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);
            var resultado = await _servicio.ObtenerAmigosAsync("Usuario1");

            Assert.AreEqual(2, resultado.Count);
            Assert.AreEqual("Amigo1", resultado[0].NombreUsuario);
            Assert.AreEqual("Amigo2", resultado[1].NombreUsuario);
        }

        [TestMethod]
        public async Task ObtenerAmigosAsync_FaultException_LanzaServicioExcepcion()
        {
            _mockCliente.Setup(c => c.ObtenerAmigosAsync(It.IsAny<string>()))
                .ThrowsAsync(new FaultException("Error"));
            _mockCliente.Setup(c => c.Abort());

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);

            try
            {
                await _servicio.ObtenerAmigosAsync("Usuario1");
                Assert.Fail("Debería lanzar ServicioExcepcion");
            }
            catch (ServicioExcepcion ex)
            {
                Assert.AreEqual(TipoErrorServicio.FallaServicio, ex.TipoError);
            }
        }

        [TestMethod]
        public async Task ObtenerAmigosAsync_TimeoutException_LanzaServicioExcepcion()
        {
            _mockCliente.Setup(c => c.ObtenerAmigosAsync(It.IsAny<string>()))
                .ThrowsAsync(new TimeoutException("Timeout"));
            _mockCliente.Setup(c => c.Abort());

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);

            try
            {
                await _servicio.ObtenerAmigosAsync("Usuario1");
                Assert.Fail("Debería lanzar ServicioExcepcion");
            }
            catch (ServicioExcepcion ex)
            {
                Assert.AreEqual(TipoErrorServicio.TiempoAgotado, ex.TipoError);
            }
        }

        #endregion

        #region Pruebas de Callbacks

        [TestMethod]
        public void NotificarListaAmigosActualizada_AmigosValidos_DisparaEvento()
        {
            _mockCliente.Setup(c => c.SuscribirAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);
            _servicio.SuscribirAsync("Usuario1").Wait();

            bool eventoDisparado = false;
            IReadOnlyList<DTOs.AmigoDTO> amigosRecibidos = null;
            _servicio.ListaActualizada += (s, e) =>
            {
                eventoDisparado = true;
                amigosRecibidos = e;
            };

            var amigos = new[]
            {
                new DTOs.AmigoDTO { UsuarioId = 1, NombreUsuario = "Amigo1" },
                new DTOs.AmigoDTO { UsuarioId = 2, NombreUsuario = "Amigo2" }
            };

            _servicio.NotificarListaAmigosActualizada(amigos);

            Assert.IsTrue(eventoDisparado);
            Assert.IsNotNull(amigosRecibidos);
            Assert.AreEqual(2, amigosRecibidos.Count);
        }

        [TestMethod]
        public void NotificarListaAmigosActualizada_AmigosNulos_DisparaEventoConListaVacia()
        {
            _mockCliente.Setup(c => c.SuscribirAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);
            _servicio.SuscribirAsync("Usuario1").Wait();

            bool eventoDisparado = false;
            IReadOnlyList<DTOs.AmigoDTO> amigosRecibidos = null;
            _servicio.ListaActualizada += (s, e) =>
            {
                eventoDisparado = true;
                amigosRecibidos = e;
            };

            _servicio.NotificarListaAmigosActualizada(null);

            Assert.IsTrue(eventoDisparado);
            Assert.IsNotNull(amigosRecibidos);
            Assert.AreEqual(0, amigosRecibidos.Count);
        }

        [TestMethod]
        public void ListaActual_IniciaSinAmigos()
        {
            _servicio = new TestableListaAmigosServicio();
            var lista = _servicio.ListaActual;

            Assert.IsNotNull(lista);
            Assert.AreEqual(0, lista.Count);
        }

        [TestMethod]
        public void ListaActual_DespuesDeCallback_DevuelveListaActualizada()
        {
            _servicio = new TestableListaAmigosServicio();

            var amigos = new[]
            {
                new DTOs.AmigoDTO { UsuarioId = 1, NombreUsuario = "Amigo1" },
                new DTOs.AmigoDTO { UsuarioId = 2, NombreUsuario = "Amigo2" }
            };

            _servicio.NotificarListaAmigosActualizada(amigos);

            var lista = _servicio.ListaActual;
            Assert.AreEqual(2, lista.Count);
            Assert.AreEqual("Amigo1", lista[0].NombreUsuario);
        }

        #endregion

        #region Pruebas de Dispose

        [TestMethod]
        public void Dispose_LiberaRecursos()
        {
            _mockCliente.Setup(c => c.State).Returns(CommunicationState.Opened);
            _mockCliente.Setup(c => c.Close());

            _servicio = new TestableListaAmigosServicio(_mockCliente.Object);
            _servicio.Dispose();

            // Verificar que no se puede usar después del dispose
            var lista = _servicio.ListaActual;
            Assert.IsNotNull(lista);
        }

        [TestMethod]
        public void Dispose_MultiplesLlamadas_NoGeneraError()
        {
            _servicio = new TestableListaAmigosServicio();
            _servicio.Dispose();
            _servicio.Dispose();
            // No debería lanzar excepción
        }

        #endregion
    }

    /// <summary>
    /// Clase testable que permite inyectar un mock del cliente WCF
    /// </summary>
    internal class TestableListaAmigosServicio : ListaAmigosServicio
    {
        private readonly PictionaryServidorServicioListaAmigos.ListaAmigosManejadorClient _clienteMock;

        public TestableListaAmigosServicio() : base()
        {
        }

        public TestableListaAmigosServicio(PictionaryServidorServicioListaAmigos.ListaAmigosManejadorClient clienteMock)
        {
            _clienteMock = clienteMock;
        }

        // Sobrescribe el método de creación para devolver el mock
        protected override PictionaryServidorServicioListaAmigos.ListaAmigosManejadorClient CrearCliente()
        {
            return _clienteMock ?? base.CrearCliente();
        }
    }
}
