using System;
using System.Data;
using System.Data.Entity.Core;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Servicios.Servicios;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;

namespace PictionaryMusicalServidor.Pruebas
{
    [TestClass]
    public class PruebaAmigosManejador
    {
        private Mock<IContextoFactory> _mockContextoFactory;
        private Mock<IAmistadServicio> _mockAmistadServicio;
        private Mock<BaseDatosPruebaEntities1> _mockContexto;
        private AmigosManejador _manejador;

        [TestInitialize]
        public void Inicializar()
        {
            _mockContextoFactory = new Mock<IContextoFactory>();
            _mockAmistadServicio = new Mock<IAmistadServicio>();
            _mockContexto = new Mock<BaseDatosPruebaEntities1>();
            
            _manejador = new AmigosManejador(_mockContextoFactory.Object, _mockAmistadServicio.Object);
        }

        #region Pruebas Suscribir

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Suscribir_DeberiaLanzarExcepcionConNombreUsuarioNulo()
        {
            // Act
            _manejador.Suscribir(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Suscribir_DeberiaLanzarExcepcionConNombreUsuarioVacio()
        {
            // Act
            _manejador.Suscribir("");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Suscribir_DeberiaLanzarExcepcionConNombreUsuarioSoloEspacios()
        {
            // Act
            _manejador.Suscribir("   ");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Suscribir_DeberiaLanzarExcepcionCuandoUsuarioNoExiste()
        {
            // Arrange
            var mockUsuarioRepositorio = new Mock<UsuarioRepositorio>(_mockContexto.Object);
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario(It.IsAny<string>())).Returns((Usuario)null);

            _mockContextoFactory.Setup(f => f.CrearContexto()).Returns(_mockContexto.Object);

            // Act
            _manejador.Suscribir("usuarioInexistente");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Suscribir_DeberiaLanzarExcepcionConErrorBaseDatos()
        {
            // Arrange
            _mockContextoFactory.Setup(f => f.CrearContexto()).Throws<EntityException>();

            // Act
            _manejador.Suscribir("usuario1");
        }

        #endregion

        #region Pruebas EnviarSolicitudAmistad

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_EnviarSolicitudAmistad_DeberiaLanzarExcepcionConNombreEmisorNulo()
        {
            // Act
            _manejador.EnviarSolicitudAmistad(null, "receptor");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_EnviarSolicitudAmistad_DeberiaLanzarExcepcionConNombreReceptorNulo()
        {
            // Act
            _manejador.EnviarSolicitudAmistad("emisor", null);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_EnviarSolicitudAmistad_DeberiaLanzarExcepcionCuandoEmisorNoExiste()
        {
            // Arrange
            var mockUsuarioRepositorio = new Mock<UsuarioRepositorio>(_mockContexto.Object);
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario("emisor")).Returns((Usuario)null);
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario("receptor")).Returns(new Usuario { idUsuario = 2, Nombre_Usuario = "receptor" });

            _mockContextoFactory.Setup(f => f.CrearContexto()).Returns(_mockContexto.Object);

            // Act
            _manejador.EnviarSolicitudAmistad("emisor", "receptor");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_EnviarSolicitudAmistad_DeberiaLanzarExcepcionCuandoReceptorNoExiste()
        {
            // Arrange
            var mockUsuarioRepositorio = new Mock<UsuarioRepositorio>(_mockContexto.Object);
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario("emisor")).Returns(new Usuario { idUsuario = 1, Nombre_Usuario = "emisor" });
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario("receptor")).Returns((Usuario)null);

            _mockContextoFactory.Setup(f => f.CrearContexto()).Returns(_mockContexto.Object);

            // Act
            _manejador.EnviarSolicitudAmistad("emisor", "receptor");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_EnviarSolicitudAmistad_DeberiaLanzarExcepcionConFalloBaseDatos()
        {
            // Arrange
            _mockContextoFactory.Setup(f => f.CrearContexto()).Throws<DataException>();

            // Act
            _manejador.EnviarSolicitudAmistad("emisor", "receptor");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_EnviarSolicitudAmistad_DeberiaLanzarExcepcionCuandoYaExisteRelacion()
        {
            // Arrange
            var mockUsuarioRepositorio = new Mock<UsuarioRepositorio>(_mockContexto.Object);
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario("emisor")).Returns(new Usuario { idUsuario = 1, Nombre_Usuario = "emisor" });
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario("receptor")).Returns(new Usuario { idUsuario = 2, Nombre_Usuario = "receptor" });

            _mockContextoFactory.Setup(f => f.CrearContexto()).Returns(_mockContexto.Object);
            _mockAmistadServicio.Setup(s => s.CrearSolicitud(1, 2)).Throws(new InvalidOperationException("Ya existe una relación de amistad"));

            // Act
            _manejador.EnviarSolicitudAmistad("emisor", "receptor");
        }

        #endregion

        #region Pruebas ResponderSolicitudAmistad

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_ResponderSolicitudAmistad_DeberiaLanzarExcepcionConNombreEmisorNulo()
        {
            // Act
            _manejador.ResponderSolicitudAmistad(null, "receptor");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_ResponderSolicitudAmistad_DeberiaLanzarExcepcionConNombreReceptorNulo()
        {
            // Act
            _manejador.ResponderSolicitudAmistad("emisor", null);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_ResponderSolicitudAmistad_DeberiaLanzarExcepcionCuandoUsuariosNoExisten()
        {
            // Arrange
            var mockUsuarioRepositorio = new Mock<UsuarioRepositorio>(_mockContexto.Object);
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario("emisor")).Returns((Usuario)null);
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario("receptor")).Returns((Usuario)null);

            _mockContextoFactory.Setup(f => f.CrearContexto()).Returns(_mockContexto.Object);

            // Act
            _manejador.ResponderSolicitudAmistad("emisor", "receptor");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_ResponderSolicitudAmistad_DeberiaLanzarExcepcionConFalloBaseDatos()
        {
            // Arrange
            _mockContextoFactory.Setup(f => f.CrearContexto()).Throws<DataException>();

            // Act
            _manejador.ResponderSolicitudAmistad("emisor", "receptor");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_ResponderSolicitudAmistad_DeberiaLanzarExcepcionCuandoSolicitudNoExiste()
        {
            // Arrange
            var mockUsuarioRepositorio = new Mock<UsuarioRepositorio>(_mockContexto.Object);
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario("emisor")).Returns(new Usuario { idUsuario = 1, Nombre_Usuario = "emisor" });
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario("receptor")).Returns(new Usuario { idUsuario = 2, Nombre_Usuario = "receptor" });

            _mockContextoFactory.Setup(f => f.CrearContexto()).Returns(_mockContexto.Object);
            _mockAmistadServicio.Setup(s => s.AceptarSolicitud(1, 2)).Throws(new InvalidOperationException("La solicitud no existe"));

            // Act
            _manejador.ResponderSolicitudAmistad("emisor", "receptor");
        }

        #endregion

        #region Pruebas EliminarAmigo

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_EliminarAmigo_DeberiaLanzarExcepcionConNombreUsuarioANulo()
        {
            // Act
            _manejador.EliminarAmigo(null, "usuarioB");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_EliminarAmigo_DeberiaLanzarExcepcionConNombreUsuarioBNulo()
        {
            // Act
            _manejador.EliminarAmigo("usuarioA", null);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_EliminarAmigo_DeberiaLanzarExcepcionCuandoUsuariosNoExisten()
        {
            // Arrange
            var mockUsuarioRepositorio = new Mock<UsuarioRepositorio>(_mockContexto.Object);
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario("usuarioA")).Returns((Usuario)null);
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario("usuarioB")).Returns((Usuario)null);

            _mockContextoFactory.Setup(f => f.CrearContexto()).Returns(_mockContexto.Object);

            // Act
            _manejador.EliminarAmigo("usuarioA", "usuarioB");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_EliminarAmigo_DeberiaLanzarExcepcionConFalloBaseDatos()
        {
            // Arrange
            _mockContextoFactory.Setup(f => f.CrearContexto()).Throws<DataException>();

            // Act
            _manejador.EliminarAmigo("usuarioA", "usuarioB");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_EliminarAmigo_DeberiaLanzarExcepcionCuandoRelacionNoExiste()
        {
            // Arrange
            var mockUsuarioRepositorio = new Mock<UsuarioRepositorio>(_mockContexto.Object);
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario("usuarioA")).Returns(new Usuario { idUsuario = 1, Nombre_Usuario = "usuarioA" });
            mockUsuarioRepositorio.Setup(r => r.ObtenerPorNombreUsuario("usuarioB")).Returns(new Usuario { idUsuario = 2, Nombre_Usuario = "usuarioB" });

            _mockContextoFactory.Setup(f => f.CrearContexto()).Returns(_mockContexto.Object);
            _mockAmistadServicio.Setup(s => s.EliminarAmistad(1, 2)).Throws(new InvalidOperationException("La relación no existe"));

            // Act
            _manejador.EliminarAmigo("usuarioA", "usuarioB");
        }

        #endregion

        #region Pruebas CancelarSuscripcion

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_CancelarSuscripcion_DeberiaLanzarExcepcionConNombreUsuarioNulo()
        {
            // Act
            _manejador.CancelarSuscripcion(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_CancelarSuscripcion_DeberiaLanzarExcepcionConNombreUsuarioVacio()
        {
            // Act
            _manejador.CancelarSuscripcion("");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_CancelarSuscripcion_DeberiaLanzarExcepcionConNombreUsuarioSoloEspacios()
        {
            // Act
            _manejador.CancelarSuscripcion("   ");
        }

        #endregion
    }
}
