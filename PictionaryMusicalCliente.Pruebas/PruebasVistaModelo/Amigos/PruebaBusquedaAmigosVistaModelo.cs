using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Amigos
{
    [TestClass]
    public class PruebaBusquedaAmigoVistaModelo
    {
        private Mock<IAmigosServicio> _mockAmigosServicio;
        private BusquedaAmigoVistaModelo _viewModel;
        private const string UsuarioTest = "UsuarioPrueba";

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null) new Application();
            Application.ResourceAssembly = typeof(BusquedaAmigoVistaModelo).Assembly;

            _mockAmigosServicio = new Mock<IAmigosServicio>();

            SesionUsuarioActual.EstablecerUsuario(new DTOs.UsuarioDTO { NombreUsuario = UsuarioTest, UsuarioId = 1 });

            AvisoAyudante.DefinirMostrarAviso((msj) => { });

            _viewModel = new BusquedaAmigoVistaModelo(_mockAmigosServicio.Object);
        }

        [TestCleanup]
        public void Limpiar()
        {
            try { SesionUsuarioActual.EstablecerUsuario(new DTOs.UsuarioDTO { UsuarioId = 0 }); } catch { }
            _viewModel = null;
        }

        #region 1. Constructor y Validaciones

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_ServicioNulo_LanzaExcepcion()
        {
            new BusquedaAmigoVistaModelo(null);
        }

        [TestMethod]
        public void Prueba_Constructor_InicializacionCorrecta()
        {
            Assert.IsNotNull(_viewModel.EnviarSolicitudComando);
            Assert.IsNotNull(_viewModel.CancelarComando);
            Assert.IsNull(_viewModel.NombreUsuarioBusqueda);
            Assert.IsFalse(_viewModel.EstaProcesando);
        }

        [TestMethod]
        public void Prueba_UsuarioActual_LeeDeSesion()
        {
            var campo = typeof(BusquedaAmigoVistaModelo).GetField("_usuarioActual", BindingFlags.NonPublic | BindingFlags.Instance);
            var valor = campo.GetValue(_viewModel) as string;

            Assert.AreEqual(UsuarioTest, valor);
        }

        #endregion

        #region 2. Propiedades y Comandos (Estado)

        [TestMethod]
        public void Prueba_NombreUsuarioBusqueda_Setter_NotificaComando()
        {
            _viewModel.NombreUsuarioBusqueda = "Amigo";
            Assert.IsTrue(_viewModel.EnviarSolicitudComando.CanExecute(null));

            _viewModel.NombreUsuarioBusqueda = "";
            Assert.IsFalse(_viewModel.EnviarSolicitudComando.CanExecute(null));
        }

        [TestMethod]
        public void Prueba_EstaProcesando_Setter_NotificaComando()
        {
            _viewModel.NombreUsuarioBusqueda = "Amigo"; 

            typeof(BusquedaAmigoVistaModelo).GetProperty("EstaProcesando").SetValue(_viewModel, true);
            Assert.IsFalse(_viewModel.EnviarSolicitudComando.CanExecute(null));

            typeof(BusquedaAmigoVistaModelo).GetProperty("EstaProcesando").SetValue(_viewModel, false);
            Assert.IsTrue(_viewModel.EnviarSolicitudComando.CanExecute(null));
        }

        #endregion

        #region 3. Enviar Solicitud (Lógica Principal)

        [TestMethod]
        public async Task Prueba_EnviarSolicitud_NombreVacio_MuestraError()
        {
            _viewModel.NombreUsuarioBusqueda = "   "; 
            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            var metodo = typeof(BusquedaAmigoVistaModelo).GetMethod("EnviarSolicitudAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)metodo.Invoke(_viewModel, null);

            Assert.AreEqual(Lang.buscarAmigoTextoIngreseUsuario, mensaje);
            _mockAmigosServicio.Verify(s => s.EnviarSolicitudAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_EnviarSolicitud_UsuarioSesionVacio_MuestraError()
        {
            typeof(BusquedaAmigoVistaModelo).GetField("_usuarioActual", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_viewModel, "");
            _viewModel.NombreUsuarioBusqueda = "Amigo";

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _viewModel.EnviarSolicitudComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoErrorProcesarSolicitud, mensaje);
        }

        [TestMethod]
        public async Task Prueba_EnviarSolicitud_Exito_LlamaServicioYNotifica()
        {
            _viewModel.NombreUsuarioBusqueda = "AmigoFuturo";

            bool notificado = false;
            _viewModel.SolicitudEnviada = () => notificado = true;
            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            _mockAmigosServicio.Setup(s => s.EnviarSolicitudAsync(UsuarioTest, "AmigoFuturo"))
                .Returns(Task.CompletedTask);

            await _viewModel.EnviarSolicitudComando.EjecutarAsync(null);

            _mockAmigosServicio.Verify(s => s.EnviarSolicitudAsync(UsuarioTest, "AmigoFuturo"), Times.Once);
            Assert.IsTrue(notificado, "Debe invocar SolicitudEnviada");
            Assert.AreEqual(Lang.amigosTextoSolicitudEnviada, mensaje);
        }

        [TestMethod]
        public async Task Prueba_EnviarSolicitud_ExcepcionServicio_MuestraError()
        {
            _viewModel.NombreUsuarioBusqueda = "AmigoError";

            _mockAmigosServicio.Setup(s => s.EnviarSolicitudAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorWCF", null));

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _viewModel.EnviarSolicitudComando.EjecutarAsync(null);

            Assert.AreEqual("ErrorWCF", mensaje);
            Assert.IsFalse(_viewModel.EstaProcesando); 
        }

        #endregion

        #region 4. Cancelación

        [TestMethod]
        public void Prueba_CancelarComando_InvocaAccion()
        {
            bool cancelado = false;
            _viewModel.Cancelado = () => cancelado = true;

            _viewModel.CancelarComando.Execute(null);

            Assert.IsTrue(cancelado);
        }

        #endregion
    }
}