using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Amigos
{
    [TestClass]
    public class PruebaSolicitudesVistaModelo
    {
        private Mock<IAmigosServicio> _mockAmigosServicio;
        private SolicitudesVistaModelo _viewModel;
        private const string UsuarioTest = "UsuarioPrueba";

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null) new Application();
            Application.ResourceAssembly = typeof(SolicitudesVistaModelo).Assembly;

            _mockAmigosServicio = new Mock<IAmigosServicio>();
            SesionUsuarioActual.EstablecerUsuario(new DTOs.UsuarioDTO { NombreUsuario = UsuarioTest, UsuarioId = 1 });
            AvisoAyudante.DefinirMostrarAviso((_) => { });
        }

        [TestCleanup]
        public void Limpiar()
        {
            _viewModel?.Dispose();
            _viewModel = null;
            try { SesionUsuarioActual.EstablecerUsuario(new DTOs.UsuarioDTO { UsuarioId = 0 }); } catch { }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ServicioNulo_LanzaExcepcion()
        {
            new SolicitudesVistaModelo(null);
        }

        [TestMethod]
        public void Constructor_InicializaCorrectamente()
        {
            _mockAmigosServicio.Setup(s => s.SolicitudesPendientes).Returns(new List<DTOs.SolicitudAmistadDTO>());
            _viewModel = new SolicitudesVistaModelo(_mockAmigosServicio.Object);

            Assert.IsNotNull(_viewModel.Solicitudes);
            Assert.IsNotNull(_viewModel.AceptarSolicitudComando);
            Assert.IsNotNull(_viewModel.RechazarSolicitudComando);
            Assert.IsNotNull(_viewModel.CerrarComando);
        }

        [TestMethod]
        public void ActualizarSolicitudes_FiltraCorrectamente()
        {
            var lista = new List<DTOs.SolicitudAmistadDTO>
            {
                new DTOs.SolicitudAmistadDTO { UsuarioReceptor = UsuarioTest, UsuarioEmisor = "Amigo1", SolicitudAceptada = false },
                new DTOs.SolicitudAmistadDTO { UsuarioReceptor = "Otro", UsuarioEmisor = "Amigo2", SolicitudAceptada = false },
                new DTOs.SolicitudAmistadDTO { UsuarioReceptor = UsuarioTest, UsuarioEmisor = "Amigo3", SolicitudAceptada = true },
                null,
                new DTOs.SolicitudAmistadDTO { UsuarioReceptor = UsuarioTest, UsuarioEmisor = " ", SolicitudAceptada = false }
            };

            _mockAmigosServicio.Setup(s => s.SolicitudesPendientes).Returns(lista);
            _viewModel = new SolicitudesVistaModelo(_mockAmigosServicio.Object);

            Assert.AreEqual(1, _viewModel.Solicitudes.Count);
            Assert.AreEqual("Amigo1", _viewModel.Solicitudes[0].NombreUsuario);
        }

        [TestMethod]
        public void EventoSolicitudesActualizadas_ActualizaLista()
        {
            _viewModel = new SolicitudesVistaModelo(_mockAmigosServicio.Object);
            var nuevas = new List<DTOs.SolicitudAmistadDTO>
            {
                new DTOs.SolicitudAmistadDTO { UsuarioReceptor = UsuarioTest, UsuarioEmisor = "NuevoAmigo", SolicitudAceptada = false }
            };

            _mockAmigosServicio.Raise(m => m.SolicitudesActualizadas += null, null, nuevas);

            Assert.AreEqual(1, _viewModel.Solicitudes.Count);
            Assert.AreEqual("NuevoAmigo", _viewModel.Solicitudes[0].NombreUsuario);
        }

        [TestMethod]
        public void SolicitudesActualizadas_ListaNula_LimpiaColeccion()
        {
            _viewModel = new SolicitudesVistaModelo(_mockAmigosServicio.Object);
            _viewModel.Solicitudes.Add(new SolicitudAmistadEntrada(new DTOs.SolicitudAmistadDTO(), "Test", true));

            _mockAmigosServicio.Raise(m => m.SolicitudesActualizadas += null, null, (List<DTOs.SolicitudAmistadDTO>)null);

            Assert.AreEqual(0, _viewModel.Solicitudes.Count);
        }

        [TestMethod]
        public void Dispose_DesuscribeEvento()
        {
            _viewModel = new SolicitudesVistaModelo(_mockAmigosServicio.Object);

            _viewModel.Dispose();

            var nuevasSolicitudes = new List<DTOs.SolicitudAmistadDTO>
            {
                new DTOs.SolicitudAmistadDTO { UsuarioEmisor = "Test", UsuarioReceptor = UsuarioTest }
            };

            _mockAmigosServicio.Raise(m => m.SolicitudesActualizadas += null, null, nuevasSolicitudes);

            Assert.AreEqual(0, _viewModel.Solicitudes.Count, "El evento no debió procesarse después del Dispose");
        }

        [TestMethod]
        public async Task AceptarSolicitud_Exito_LlamaServicio()
        {
            _viewModel = new SolicitudesVistaModelo(_mockAmigosServicio.Object);
            var dto = new DTOs.SolicitudAmistadDTO { UsuarioEmisor = "Amigo", UsuarioReceptor = UsuarioTest };
            var entrada = new SolicitudAmistadEntrada(dto, "Amigo", true);

            await _viewModel.AceptarSolicitudComando.EjecutarAsync(entrada);

            _mockAmigosServicio.Verify(s => s.ResponderSolicitudAsync("Amigo", UsuarioTest), Times.Once);
        }

        [TestMethod]
        public async Task AceptarSolicitud_Excepcion_MuestraError()
        {
            _viewModel = new SolicitudesVistaModelo(_mockAmigosServicio.Object);
            var entrada = new SolicitudAmistadEntrada(new DTOs.SolicitudAmistadDTO(), "A", true);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            _mockAmigosServicio.Setup(s => s.ResponderSolicitudAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorRed", null));

            await _viewModel.AceptarSolicitudComando.EjecutarAsync(entrada);

            Assert.AreEqual("ErrorRed", mensaje);
            Assert.IsTrue(_viewModel.AceptarSolicitudComando.CanExecute(entrada)); 
        }

        [TestMethod]
        public async Task AceptarSolicitud_EntradaNula_NoHaceNada()
        {
            _viewModel = new SolicitudesVistaModelo(_mockAmigosServicio.Object);
            await _viewModel.AceptarSolicitudComando.EjecutarAsync(null);
            _mockAmigosServicio.Verify(s => s.ResponderSolicitudAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task RechazarSolicitud_Exito_LlamaServicio()
        {
            _viewModel = new SolicitudesVistaModelo(_mockAmigosServicio.Object);
            var dto = new DTOs.SolicitudAmistadDTO { UsuarioEmisor = "Amigo", UsuarioReceptor = UsuarioTest };
            var entrada = new SolicitudAmistadEntrada(dto, "Amigo", true);

            await _viewModel.RechazarSolicitudComando.EjecutarAsync(entrada);

            _mockAmigosServicio.Verify(s => s.EliminarAmigoAsync("Amigo", UsuarioTest), Times.Once);
        }

        [TestMethod]
        public async Task RechazarSolicitud_Excepcion_MuestraError()
        {
            _viewModel = new SolicitudesVistaModelo(_mockAmigosServicio.Object);
            var entrada = new SolicitudAmistadEntrada(new DTOs.SolicitudAmistadDTO(), "A", true);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            _mockAmigosServicio.Setup(s => s.EliminarAmigoAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "Fallo", null));

            await _viewModel.RechazarSolicitudComando.EjecutarAsync(entrada);

            Assert.AreEqual("Fallo", mensaje);
        }

        [TestMethod]
        public async Task RechazarSolicitud_EntradaNula_NoHaceNada()
        {
            _viewModel = new SolicitudesVistaModelo(_mockAmigosServicio.Object);
            await _viewModel.RechazarSolicitudComando.EjecutarAsync(null);
            _mockAmigosServicio.Verify(s => s.EliminarAmigoAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void CerrarComando_InvocaAccion()
        {
            _viewModel = new SolicitudesVistaModelo(_mockAmigosServicio.Object);
            bool cerrado = false;
            _viewModel.Cerrar = () => cerrado = true;

            _viewModel.CerrarComando.Execute(null);

            Assert.IsTrue(cerrado);
        }

        [TestMethod]
        public void PuedeAceptar_SoloSiEsReceptor()
        {
            _viewModel = new SolicitudesVistaModelo(_mockAmigosServicio.Object);
            var entradaReceptor = new SolicitudAmistadEntrada(new DTOs.SolicitudAmistadDTO(), "A", true);
            var entradaEmisor = new SolicitudAmistadEntrada(new DTOs.SolicitudAmistadDTO(), "B", false);

            Assert.IsTrue(_viewModel.AceptarSolicitudComando.CanExecute(entradaReceptor));
            Assert.IsFalse(_viewModel.AceptarSolicitudComando.CanExecute(entradaEmisor));
        }

        [TestMethod]
        public void EstaProcesando_BloqueaComandos()
        {
            _viewModel = new SolicitudesVistaModelo(_mockAmigosServicio.Object);
            var entrada = new SolicitudAmistadEntrada(new DTOs.SolicitudAmistadDTO(), "A", true);

            typeof(SolicitudesVistaModelo).GetProperty("EstaProcesando", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_viewModel, true);

            Assert.IsFalse(_viewModel.AceptarSolicitudComando.CanExecute(entrada));
            Assert.IsFalse(_viewModel.RechazarSolicitudComando.CanExecute(entrada));
        }
    }
}