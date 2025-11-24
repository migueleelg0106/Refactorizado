using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Servicios.Servicios;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using System;
using System.Collections.Generic;

namespace PictionaryMusicalServidor.Pruebas
{
    [TestClass]
    public class PruebaAmistadServicio
    {
        private Mock<IAmigoRepositorio> _mockAmigoRepositorio;

        [TestInitialize]
        public void Inicializar()
        {
            _mockAmigoRepositorio = new Mock<IAmigoRepositorio>();
        }

        #region Pruebas de ObtenerSolicitudesPendientesDTO

        [TestMethod]
        public void Prueba_ObtenerSolicitudesPendientesDTO_SinSolicitudes_RetornaListaVacia()
        {
            // Arrange
            int usuarioId = 1;
            _mockAmigoRepositorio.Setup(r => r.ObtenerSolicitudesPendientes(usuarioId))
                .Returns(new List<Amigo>());

            // Act
            var resultado = AmistadServicio.ObtenerSolicitudesPendientesDTOInterno(usuarioId, _mockAmigoRepositorio.Object);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(0, resultado.Count);
        }

        [TestMethod]
        public void Prueba_ObtenerSolicitudesPendientesDTO_SolicitudesNulas_RetornaListaVacia()
        {
            // Arrange
            int usuarioId = 1;
            _mockAmigoRepositorio.Setup(r => r.ObtenerSolicitudesPendientes(usuarioId))
                .Returns((IList<Amigo>)null);

            // Act
            var resultado = AmistadServicio.ObtenerSolicitudesPendientesDTOInterno(usuarioId, _mockAmigoRepositorio.Object);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(0, resultado.Count);
        }

        [TestMethod]
        public void Prueba_ObtenerSolicitudesPendientesDTO_ConSolicitudesValidas_RetornaListaDTOs()
        {
            // Arrange
            int usuarioId = 2;
            var solicitudes = new List<Amigo>
            {
                new Amigo
                {
                    UsuarioEmisor = 1,
                    UsuarioReceptor = 2,
                    Estado = false,
                    Usuario = new Usuario { idUsuario = 1, Nombre_Usuario = "Emisor1" },
                    Usuario1 = new Usuario { idUsuario = 2, Nombre_Usuario = "Receptor2" }
                }
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerSolicitudesPendientes(usuarioId))
                .Returns(solicitudes);

            // Act
            var resultado = AmistadServicio.ObtenerSolicitudesPendientesDTOInterno(usuarioId, _mockAmigoRepositorio.Object);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(1, resultado.Count);
            Assert.AreEqual("Emisor1", resultado[0].UsuarioEmisor);
            Assert.AreEqual("Receptor2", resultado[0].UsuarioReceptor);
            Assert.IsFalse(resultado[0].SolicitudAceptada);
        }

        [TestMethod]
        public void Prueba_ObtenerSolicitudesPendientesDTO_FiltraSolicitudesConUsuarioNulo()
        {
            // Arrange
            int usuarioId = 2;
            var solicitudes = new List<Amigo>
            {
                new Amigo
                {
                    UsuarioEmisor = 1,
                    UsuarioReceptor = 2,
                    Estado = false,
                    Usuario = null,
                    Usuario1 = new Usuario { idUsuario = 2, Nombre_Usuario = "Receptor2" }
                },
                new Amigo
                {
                    UsuarioEmisor = 3,
                    UsuarioReceptor = 2,
                    Estado = false,
                    Usuario = new Usuario { idUsuario = 3, Nombre_Usuario = "Emisor3" },
                    Usuario1 = new Usuario { idUsuario = 2, Nombre_Usuario = "Receptor2" }
                }
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerSolicitudesPendientes(usuarioId))
                .Returns(solicitudes);

            // Act
            var resultado = AmistadServicio.ObtenerSolicitudesPendientesDTOInterno(usuarioId, _mockAmigoRepositorio.Object);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(1, resultado.Count);
            Assert.AreEqual("Emisor3", resultado[0].UsuarioEmisor);
        }

        [TestMethod]
        public void Prueba_ObtenerSolicitudesPendientesDTO_FiltraSolicitudesNoReceptor()
        {
            // Arrange
            int usuarioId = 2;
            var solicitudes = new List<Amigo>
            {
                new Amigo
                {
                    UsuarioEmisor = 2,
                    UsuarioReceptor = 3,
                    Estado = false,
                    Usuario = new Usuario { idUsuario = 2, Nombre_Usuario = "Emisor2" },
                    Usuario1 = new Usuario { idUsuario = 3, Nombre_Usuario = "Receptor3" }
                }
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerSolicitudesPendientes(usuarioId))
                .Returns(solicitudes);

            // Act
            var resultado = AmistadServicio.ObtenerSolicitudesPendientesDTOInterno(usuarioId, _mockAmigoRepositorio.Object);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(0, resultado.Count);
        }

        [TestMethod]
        public void Prueba_ObtenerSolicitudesPendientesDTO_FiltraSolicitudesConNombreVacio()
        {
            // Arrange
            int usuarioId = 2;
            var solicitudes = new List<Amigo>
            {
                new Amigo
                {
                    UsuarioEmisor = 1,
                    UsuarioReceptor = 2,
                    Estado = false,
                    Usuario = new Usuario { idUsuario = 1, Nombre_Usuario = "" },
                    Usuario1 = new Usuario { idUsuario = 2, Nombre_Usuario = "Receptor2" }
                }
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerSolicitudesPendientes(usuarioId))
                .Returns(solicitudes);

            // Act
            var resultado = AmistadServicio.ObtenerSolicitudesPendientesDTOInterno(usuarioId, _mockAmigoRepositorio.Object);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(0, resultado.Count);
        }

        #endregion

        #region Pruebas de CrearSolicitud

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_CrearSolicitud_MismoUsuario_LanzaExcepcion()
        {
            // Arrange
            int usuarioId = 1;

            // Act
            AmistadServicio.CrearSolicitudInterno(usuarioId, usuarioId, _mockAmigoRepositorio.Object);

            // Assert - ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_CrearSolicitud_RelacionExistente_LanzaExcepcion()
        {
            // Arrange
            int usuarioEmisorId = 1;
            int usuarioReceptorId = 2;

            _mockAmigoRepositorio.Setup(r => r.ExisteRelacion(usuarioEmisorId, usuarioReceptorId))
                .Returns(true);

            // Act
            AmistadServicio.CrearSolicitudInterno(usuarioEmisorId, usuarioReceptorId, _mockAmigoRepositorio.Object);

            // Assert - ExpectedException
        }

        [TestMethod]
        public void Prueba_CrearSolicitud_Exitosa_CreaYRegistra()
        {
            // Arrange
            int usuarioEmisorId = 1;
            int usuarioReceptorId = 2;

            _mockAmigoRepositorio.Setup(r => r.ExisteRelacion(usuarioEmisorId, usuarioReceptorId))
                .Returns(false);

            _mockAmigoRepositorio.Setup(r => r.CrearSolicitud(usuarioEmisorId, usuarioReceptorId))
                .Returns(new Amigo
                {
                    UsuarioEmisor = usuarioEmisorId,
                    UsuarioReceptor = usuarioReceptorId,
                    Estado = false
                });

            // Act
            AmistadServicio.CrearSolicitudInterno(usuarioEmisorId, usuarioReceptorId, _mockAmigoRepositorio.Object);

            // Assert
            _mockAmigoRepositorio.Verify(r => r.ExisteRelacion(usuarioEmisorId, usuarioReceptorId), Times.Once);
            _mockAmigoRepositorio.Verify(r => r.CrearSolicitud(usuarioEmisorId, usuarioReceptorId), Times.Once);
        }

        #endregion

        #region Pruebas de AceptarSolicitud

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_AceptarSolicitud_SolicitudNoExiste_LanzaExcepcion()
        {
            // Arrange
            int usuarioEmisorId = 1;
            int usuarioReceptorId = 2;

            _mockAmigoRepositorio.Setup(r => r.ObtenerRelacion(usuarioEmisorId, usuarioReceptorId))
                .Returns((Amigo)null);

            // Act
            AmistadServicio.AceptarSolicitudInterno(usuarioEmisorId, usuarioReceptorId, _mockAmigoRepositorio.Object);

            // Assert - ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_AceptarSolicitud_ReceptorIncorrecto_LanzaExcepcion()
        {
            // Arrange
            int usuarioEmisorId = 1;
            int usuarioReceptorId = 2;
            int receptorReal = 3;

            var relacion = new Amigo
            {
                UsuarioEmisor = usuarioEmisorId,
                UsuarioReceptor = receptorReal,
                Estado = false
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerRelacion(usuarioEmisorId, usuarioReceptorId))
                .Returns(relacion);

            // Act
            AmistadServicio.AceptarSolicitudInterno(usuarioEmisorId, usuarioReceptorId, _mockAmigoRepositorio.Object);

            // Assert - ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_AceptarSolicitud_YaAceptada_LanzaExcepcion()
        {
            // Arrange
            int usuarioEmisorId = 1;
            int usuarioReceptorId = 2;

            var relacion = new Amigo
            {
                UsuarioEmisor = usuarioEmisorId,
                UsuarioReceptor = usuarioReceptorId,
                Estado = true
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerRelacion(usuarioEmisorId, usuarioReceptorId))
                .Returns(relacion);

            // Act
            AmistadServicio.AceptarSolicitudInterno(usuarioEmisorId, usuarioReceptorId, _mockAmigoRepositorio.Object);

            // Assert - ExpectedException
        }

        [TestMethod]
        public void Prueba_AceptarSolicitud_Exitosa_ActualizaEstado()
        {
            // Arrange
            int usuarioEmisorId = 1;
            int usuarioReceptorId = 2;

            var relacion = new Amigo
            {
                UsuarioEmisor = usuarioEmisorId,
                UsuarioReceptor = usuarioReceptorId,
                Estado = false
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerRelacion(usuarioEmisorId, usuarioReceptorId))
                .Returns(relacion);

            // Act
            AmistadServicio.AceptarSolicitudInterno(usuarioEmisorId, usuarioReceptorId, _mockAmigoRepositorio.Object);

            // Assert
            _mockAmigoRepositorio.Verify(r => r.ObtenerRelacion(usuarioEmisorId, usuarioReceptorId), Times.Once);
            _mockAmigoRepositorio.Verify(r => r.ActualizarEstado(relacion, true), Times.Once);
        }

        #endregion

        #region Pruebas de EliminarAmistad

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_EliminarAmistad_MismoUsuario_LanzaExcepcion()
        {
            // Arrange
            int usuarioId = 1;

            // Act
            AmistadServicio.EliminarAmistadInterno(usuarioId, usuarioId, _mockAmigoRepositorio.Object);

            // Assert - ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_EliminarAmistad_RelacionNoExiste_LanzaExcepcion()
        {
            // Arrange
            int usuarioAId = 1;
            int usuarioBId = 2;

            _mockAmigoRepositorio.Setup(r => r.ObtenerRelacion(usuarioAId, usuarioBId))
                .Returns((Amigo)null);

            // Act
            AmistadServicio.EliminarAmistadInterno(usuarioAId, usuarioBId, _mockAmigoRepositorio.Object);

            // Assert - ExpectedException
        }

        [TestMethod]
        public void Prueba_EliminarAmistad_Exitosa_EliminaYRetornaRelacion()
        {
            // Arrange
            int usuarioAId = 1;
            int usuarioBId = 2;

            var relacion = new Amigo
            {
                UsuarioEmisor = usuarioAId,
                UsuarioReceptor = usuarioBId,
                Estado = true
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerRelacion(usuarioAId, usuarioBId))
                .Returns(relacion);

            // Act
            var resultado = AmistadServicio.EliminarAmistadInterno(usuarioAId, usuarioBId, _mockAmigoRepositorio.Object);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(usuarioAId, resultado.UsuarioEmisor);
            Assert.AreEqual(usuarioBId, resultado.UsuarioReceptor);
            _mockAmigoRepositorio.Verify(r => r.ObtenerRelacion(usuarioAId, usuarioBId), Times.Once);
            _mockAmigoRepositorio.Verify(r => r.EliminarRelacion(relacion), Times.Once);
        }

        #endregion

        #region Pruebas de ObtenerAmigosDTO

        [TestMethod]
        public void Prueba_ObtenerAmigosDTO_SinAmigos_RetornaListaVacia()
        {
            // Arrange
            int usuarioId = 1;
            _mockAmigoRepositorio.Setup(r => r.ObtenerAmigos(usuarioId))
                .Returns((IList<Usuario>)null);

            // Act
            var resultado = AmistadServicio.ObtenerAmigosDTOInterno(usuarioId, _mockAmigoRepositorio.Object);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(0, resultado.Count);
        }

        [TestMethod]
        public void Prueba_ObtenerAmigosDTO_ConAmigos_RetornaListaDTOs()
        {
            // Arrange
            int usuarioId = 1;
            var amigos = new List<Usuario>
            {
                new Usuario { idUsuario = 2, Nombre_Usuario = "Amigo1" },
                new Usuario { idUsuario = 3, Nombre_Usuario = "Amigo2" }
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerAmigos(usuarioId))
                .Returns(amigos);

            // Act
            var resultado = AmistadServicio.ObtenerAmigosDTOInterno(usuarioId, _mockAmigoRepositorio.Object);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(2, resultado.Count);
            Assert.AreEqual(2, resultado[0].UsuarioId);
            Assert.AreEqual("Amigo1", resultado[0].NombreUsuario);
            Assert.AreEqual(3, resultado[1].UsuarioId);
            Assert.AreEqual("Amigo2", resultado[1].NombreUsuario);
        }

        [TestMethod]
        public void Prueba_ObtenerAmigosDTO_FiltraAmigosNulos()
        {
            // Arrange
            int usuarioId = 1;
            var amigos = new List<Usuario>
            {
                new Usuario { idUsuario = 2, Nombre_Usuario = "Amigo1" },
                null,
                new Usuario { idUsuario = 3, Nombre_Usuario = "Amigo2" }
            };

            _mockAmigoRepositorio.Setup(r => r.ObtenerAmigos(usuarioId))
                .Returns(amigos);

            // Act
            var resultado = AmistadServicio.ObtenerAmigosDTOInterno(usuarioId, _mockAmigoRepositorio.Object);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(2, resultado.Count);
            Assert.AreEqual("Amigo1", resultado[0].NombreUsuario);
            Assert.AreEqual("Amigo2", resultado[1].NombreUsuario);
        }

        [TestMethod]
        public void Prueba_ObtenerAmigosDTO_ListaVacia_RetornaListaVacia()
        {
            // Arrange
            int usuarioId = 1;
            _mockAmigoRepositorio.Setup(r => r.ObtenerAmigos(usuarioId))
                .Returns(new List<Usuario>());

            // Act
            var resultado = AmistadServicio.ObtenerAmigosDTOInterno(usuarioId, _mockAmigoRepositorio.Object);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(0, resultado.Count);
        }

        #endregion
    }
}
