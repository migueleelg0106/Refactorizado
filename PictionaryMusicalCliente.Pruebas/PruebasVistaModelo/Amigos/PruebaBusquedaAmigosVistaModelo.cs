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
        private BusquedaAmigoVistaModelo _vistaModelo;
        private const string UsuarioTest = "UsuarioPrueba";

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null) new Application();
            Application.ResourceAssembly = typeof(BusquedaAmigoVistaModelo).Assembly;

            _mockAmigosServicio = new Mock<IAmigosServicio>();

            SesionUsuarioActual.EstablecerUsuario(new DTOs.UsuarioDTO { NombreUsuario = UsuarioTest, UsuarioId = 1 });

            AvisoAyudante.DefinirMostrarAviso((msj) => { });

            _vistaModelo = new BusquedaAmigoVistaModelo(_mockAmigosServicio.Object);
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
        public void Prueba_Constructor_ServicioNulo_LanzaExcepcion()
        {
            new BusquedaAmigoVistaModelo(null);
        }

        [TestMethod]
        public void Prueba_Constructor_InicializacionCorrecta()
        {
            Assert.IsNotNull(_vistaModelo.EnviarSolicitudComando);
            Assert.IsNotNull(_vistaModelo.CancelarComando);
            Assert.IsNull(_vistaModelo.NombreUsuarioBusqueda);
            Assert.IsFalse(_vistaModelo.EstaProcesando);
        }

        [TestMethod]
        public void Prueba_UsuarioActual_LeeDeSesion()
        {
            var campo = typeof(BusquedaAmigoVistaModelo).GetField("_usuarioActual", BindingFlags.NonPublic | BindingFlags.Instance);
            var valor = campo.GetValue(_vistaModelo) as string;

            Assert.AreEqual(UsuarioTest, valor);
        }

        #endregion

        #region 2. Propiedades y Comandos (Estado)

        [TestMethod]
        public void Prueba_NombreUsuarioBusqueda_Setter_NotificaComando()
        {
            _vistaModelo.NombreUsuarioBusqueda = "Amigo";
            Assert.IsTrue(_vistaModelo.EnviarSolicitudComando.CanExecute(null));

            _vistaModelo.NombreUsuarioBusqueda = "";
            Assert.IsFalse(_vistaModelo.EnviarSolicitudComando.CanExecute(null));
        }

        [TestMethod]
        public void Prueba_EstaProcesando_Setter_NotificaComando()
        {
            _vistaModelo.NombreUsuarioBusqueda = "Amigo"; 

            typeof(BusquedaAmigoVistaModelo).GetProperty("EstaProcesando").SetValue(_vistaModelo, true);
            Assert.IsFalse(_vistaModelo.EnviarSolicitudComando.CanExecute(null));

            typeof(BusquedaAmigoVistaModelo).GetProperty("EstaProcesando").SetValue(_vistaModelo, false);
            Assert.IsTrue(_vistaModelo.EnviarSolicitudComando.CanExecute(null));
        }

        #endregion

        #region 3. Enviar Solicitud (Lógica Principal)

        [TestMethod]
        public async Task Prueba_EnviarSolicitud_NombreVacio_MuestraError()
        {
            _vistaModelo.NombreUsuarioBusqueda = "   "; 
            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            var metodo = typeof(BusquedaAmigoVistaModelo).GetMethod("EnviarSolicitudAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)metodo.Invoke(_vistaModelo, null);

            Assert.AreEqual(Lang.buscarAmigoTextoIngreseUsuario, mensaje);
            _mockAmigosServicio.Verify(s => s.EnviarSolicitudAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_EnviarSolicitud_UsuarioSesionVacio_MuestraError()
        {
            typeof(BusquedaAmigoVistaModelo).GetField("_usuarioActual", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_vistaModelo, "");
            _vistaModelo.NombreUsuarioBusqueda = "Amigo";

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.EnviarSolicitudComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoErrorProcesarSolicitud, mensaje);
        }

        [TestMethod]
        public async Task Prueba_EnviarSolicitud_Exito_LlamaServicioYNotifica()
        {
            _vistaModelo.NombreUsuarioBusqueda = "AmigoFuturo";

            bool notificado = false;
            _vistaModelo.SolicitudEnviada = () => notificado = true;
            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            _mockAmigosServicio.Setup(s => s.EnviarSolicitudAsync(UsuarioTest, "AmigoFuturo"))
                .Returns(Task.CompletedTask);

            await _vistaModelo.EnviarSolicitudComando.EjecutarAsync(null);

            _mockAmigosServicio.Verify(s => s.EnviarSolicitudAsync(UsuarioTest, "AmigoFuturo"), Times.Once);
            Assert.IsTrue(notificado, "Debe invocar SolicitudEnviada");
            Assert.AreEqual(Lang.amigosTextoSolicitudEnviada, mensaje);
        }

        [TestMethod]
        public async Task Prueba_EnviarSolicitud_ExcepcionServicio_MuestraError()
        {
            _vistaModelo.NombreUsuarioBusqueda = "AmigoError";

            const string mensajeEsperado = "The entered user was not found, please enter another one.";
            _mockAmigosServicio.Setup(s => s.EnviarSolicitudAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensajeEsperado, null));

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.EnviarSolicitudComando.EjecutarAsync(null);

            Assert.AreEqual(mensajeEsperado, mensaje, "El mensaje mostrado debe ser el mensaje específico de la excepción");
            Assert.IsFalse(_vistaModelo.EstaProcesando); 
        }

        #endregion

        #region 4. Cancelación

        [TestMethod]
        public void Prueba_CancelarComando_InvocaAccion()
        {
            bool cancelado = false;
            _vistaModelo.Cancelado = () => cancelado = true;

            _vistaModelo.CancelarComando.Execute(null);

            Assert.IsTrue(cancelado);
        }

        #endregion
    }
}