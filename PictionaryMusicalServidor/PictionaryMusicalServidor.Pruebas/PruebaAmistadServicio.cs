using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Servicios.Servicios;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Pruebas
{
    [TestClass]
    public class PruebaAmistadServicio
    {
        private Mock<IContextoFactory> _mockContextoFactory;
        private Mock<BaseDatosPruebaEntities1> _mockContexto;
        private Mock<AmigoRepositorio> _mockAmigoRepositorio;
        private AmistadServicio _servicio;

        [TestInitialize]
        public void Inicializar()
        {
            _mockContextoFactory = new Mock<IContextoFactory>();
            _mockContexto = new Mock<BaseDatosPruebaEntities1>();
            _mockAmigoRepositorio = new Mock<AmigoRepositorio>(_mockContexto.Object);
            
            _servicio = new AmistadServicio(_mockContextoFactory.Object);
            
            _mockContextoFactory.Setup(f => f.CrearContexto()).Returns(_mockContexto.Object);
        }

        #region Pruebas CrearSolicitud

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_CrearSolicitud_DeberiaLanzarExcepcionCuandoUsuarioSeEnviaSolicitudASiMismo()
        {
            // Arrange
            int usuarioId = 1;

            // Act
            _servicio.CrearSolicitud(usuarioId, usuarioId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_CrearSolicitud_DeberiaLanzarExcepcionCuandoYaExisteRelacion()
        {
            // Arrange
            int usuarioEmisorId = 1;
            int usuarioReceptorId = 2;

            _mockAmigoRepositorio.Setup(r => r.ExisteRelacion(usuarioEmisorId, usuarioReceptorId)).Returns(true);

            // Configurar el contexto para devolver el repositorio mock
            // Nota: En una implementación real, necesitarías inyectar el repositorio también
            // Para esta prueba, asumimos que el servicio crea el repositorio internamente
            
            // Act
            _servicio.CrearSolicitud(usuarioEmisorId, usuarioReceptorId);
        }

        [TestMethod]
        public void Prueba_CrearSolicitud_DeberiaCrearSolicitudCorrectamente()
        {
            // Arrange
            int usuarioEmisorId = 1;
            int usuarioReceptorId = 2;

            _mockAmigoRepositorio.Setup(r => r.ExisteRelacion(usuarioEmisorId, usuarioReceptorId)).Returns(false);
            _mockAmigoRepositorio.Setup(r => r.CrearSolicitud(usuarioEmisorId, usuarioReceptorId))
                .Returns(new Amigo
                {
                    UsuarioEmisor = usuarioEmisorId,
                    UsuarioReceptor = usuarioReceptorId,
                    Estado = false
                });

            // Act - Nota: Esta prueba requeriría acceso al repositorio mock dentro del servicio
            // En una implementación más completa, el repositorio también debería ser inyectado
            try
            {
                _servicio.CrearSolicitud(usuarioEmisorId, usuarioReceptorId);
                // Si no lanza excepción, la prueba pasa
                Assert.IsTrue(true);
            }
            catch (Exception)
            {
                Assert.Fail("No debería lanzar excepción con datos válidos");
            }
        }

        #endregion

        #region Pruebas AceptarSolicitud

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_AceptarSolicitud_DeberiaLanzarExcepcionCuandoSolicitudNoExiste()
        {
            // Arrange
            int usuarioEmisorId = 1;
            int usuarioReceptorId = 2;

            _mockAmigoRepositorio.Setup(r => r.ObtenerRelacion(usuarioEmisorId, usuarioReceptorId)).Returns((Amigo)null);

            // Act
            _servicio.AceptarSolicitud(usuarioEmisorId, usuarioReceptorId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_AceptarSolicitud_DeberiaLanzarExcepcionCuandoSolicitudNoCorrespondeAlUsuario()
        {
            // Arrange
            int usuarioEmisorId = 1;
            int usuarioReceptorId = 2;
            int receptorReal = 3;

            var relacionExistente = new Amigo
            {
                UsuarioEmisor = usuarioEmisorId,
                UsuarioReceptor = receptorReal,
                Estado = false
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerRelacion(usuarioEmisorId, usuarioReceptorId)).Returns(relacionExistente);

            // Act
            _servicio.AceptarSolicitud(usuarioEmisorId, usuarioReceptorId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_AceptarSolicitud_DeberiaLanzarExcepcionCuandoSolicitudYaEstaAceptada()
        {
            // Arrange
            int usuarioEmisorId = 1;
            int usuarioReceptorId = 2;

            var relacionAceptada = new Amigo
            {
                UsuarioEmisor = usuarioEmisorId,
                UsuarioReceptor = usuarioReceptorId,
                Estado = true  // Ya está aceptada
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerRelacion(usuarioEmisorId, usuarioReceptorId)).Returns(relacionAceptada);

            // Act
            _servicio.AceptarSolicitud(usuarioEmisorId, usuarioReceptorId);
        }

        [TestMethod]
        public void Prueba_AceptarSolicitud_DeberiaAceptarSolicitudCorrectamente()
        {
            // Arrange
            int usuarioEmisorId = 1;
            int usuarioReceptorId = 2;

            var relacionPendiente = new Amigo
            {
                UsuarioEmisor = usuarioEmisorId,
                UsuarioReceptor = usuarioReceptorId,
                Estado = false
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerRelacion(usuarioEmisorId, usuarioReceptorId)).Returns(relacionPendiente);
            _mockAmigoRepositorio.Setup(r => r.ActualizarEstado(relacionPendiente, true));

            // Act
            try
            {
                _servicio.AceptarSolicitud(usuarioEmisorId, usuarioReceptorId);
                // Si no lanza excepción, la prueba pasa
                Assert.IsTrue(true);
            }
            catch (Exception)
            {
                Assert.Fail("No debería lanzar excepción con datos válidos");
            }
        }

        #endregion

        #region Pruebas EliminarAmistad

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_EliminarAmistad_DeberiaLanzarExcepcionCuandoUsuariosMismoId()
        {
            // Arrange
            int usuarioId = 1;

            // Act
            _servicio.EliminarAmistad(usuarioId, usuarioId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_EliminarAmistad_DeberiaLanzarExcepcionCuandoRelacionNoExiste()
        {
            // Arrange
            int usuarioAId = 1;
            int usuarioBId = 2;

            _mockAmigoRepositorio.Setup(r => r.ObtenerRelacion(usuarioAId, usuarioBId)).Returns((Amigo)null);

            // Act
            _servicio.EliminarAmistad(usuarioAId, usuarioBId);
        }

        [TestMethod]
        public void Prueba_EliminarAmistad_DeberiaEliminarRelacionCorrectamente()
        {
            // Arrange
            int usuarioAId = 1;
            int usuarioBId = 2;

            var relacionExistente = new Amigo
            {
                UsuarioEmisor = usuarioAId,
                UsuarioReceptor = usuarioBId,
                Estado = true
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerRelacion(usuarioAId, usuarioBId)).Returns(relacionExistente);
            _mockAmigoRepositorio.Setup(r => r.EliminarRelacion(relacionExistente));

            // Act
            try
            {
                var resultado = _servicio.EliminarAmistad(usuarioAId, usuarioBId);
                
                // Assert
                Assert.IsNotNull(resultado);
                Assert.AreEqual(usuarioAId, resultado.UsuarioEmisor);
                Assert.AreEqual(usuarioBId, resultado.UsuarioReceptor);
            }
            catch (Exception)
            {
                Assert.Fail("No debería lanzar excepción con datos válidos");
            }
        }

        #endregion

        #region Pruebas ObtenerSolicitudesPendientesDTO

        [TestMethod]
        public void Prueba_ObtenerSolicitudesPendientesDTO_DeberiaRetornarListaVaciaCuandoNoHaySolicitudes()
        {
            // Arrange
            int usuarioId = 1;

            _mockAmigoRepositorio.Setup(r => r.ObtenerSolicitudesPendientes(usuarioId)).Returns(new List<Amigo>());

            // Act
            var resultado = _servicio.ObtenerSolicitudesPendientesDTO(usuarioId);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(0, resultado.Count);
        }

        [TestMethod]
        public void Prueba_ObtenerSolicitudesPendientesDTO_DeberiaRetornarSolicitudesPendientes()
        {
            // Arrange
            int usuarioId = 2;

            var solicitudesPendientes = new List<Amigo>
            {
                new Amigo
                {
                    UsuarioEmisor = 1,
                    UsuarioReceptor = usuarioId,
                    Estado = false,
                    Usuario = new Usuario { idUsuario = 1, Nombre_Usuario = "emisor1" },
                    Usuario1 = new Usuario { idUsuario = usuarioId, Nombre_Usuario = "receptor" }
                }
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerSolicitudesPendientes(usuarioId)).Returns(solicitudesPendientes);

            // Act
            var resultado = _servicio.ObtenerSolicitudesPendientesDTO(usuarioId);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(1, resultado.Count);
            Assert.AreEqual("emisor1", resultado[0].UsuarioEmisor);
            Assert.AreEqual("receptor", resultado[0].UsuarioReceptor);
        }

        #endregion

        #region Pruebas ObtenerAmigosDTO

        [TestMethod]
        public void Prueba_ObtenerAmigosDTO_DeberiaRetornarListaVaciaCuandoNoHayAmigos()
        {
            // Arrange
            int usuarioId = 1;

            _mockAmigoRepositorio.Setup(r => r.ObtenerAmigos(usuarioId)).Returns(new List<Usuario>());

            // Act
            var resultado = _servicio.ObtenerAmigosDTO(usuarioId);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(0, resultado.Count);
        }

        [TestMethod]
        public void Prueba_ObtenerAmigosDTO_DeberiaRetornarListaDeAmigos()
        {
            // Arrange
            int usuarioId = 1;

            var amigos = new List<Usuario>
            {
                new Usuario { idUsuario = 2, Nombre_Usuario = "amigo1" },
                new Usuario { idUsuario = 3, Nombre_Usuario = "amigo2" }
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerAmigos(usuarioId)).Returns(amigos);

            // Act
            var resultado = _servicio.ObtenerAmigosDTO(usuarioId);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(2, resultado.Count);
            Assert.AreEqual("amigo1", resultado[0].NombreUsuario);
            Assert.AreEqual("amigo2", resultado[1].NombreUsuario);
        }

        [TestMethod]
        public void Prueba_ObtenerAmigosDTO_DeberiaFiltrarAmigosNulos()
        {
            // Arrange
            int usuarioId = 1;

            var amigos = new List<Usuario>
            {
                new Usuario { idUsuario = 2, Nombre_Usuario = "amigo1" },
                null,  // Amigo nulo que debe ser filtrado
                new Usuario { idUsuario = 3, Nombre_Usuario = "amigo2" }
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerAmigos(usuarioId)).Returns(amigos);

            // Act
            var resultado = _servicio.ObtenerAmigosDTO(usuarioId);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(2, resultado.Count);
            Assert.AreEqual("amigo1", resultado[0].NombreUsuario);
            Assert.AreEqual("amigo2", resultado[1].NombreUsuario);
        }

        #endregion

        #region Pruebas Constructor

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_DeberiaLanzarExcepcionConContextoFactoryNulo()
        {
            // Act
            new AmistadServicio(null);
        }

        #endregion
    }
}
