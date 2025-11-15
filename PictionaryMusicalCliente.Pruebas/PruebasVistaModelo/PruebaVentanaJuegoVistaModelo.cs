using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using SalaDTO = Servicios.Contratos.DTOs.SalaDTO;
using AmigoDTO = Servicios.Contratos.DTOs.AmigoDTO;
using ResultadoOperacionDTO = Servicios.Contratos.DTOs.ResultadoOperacionDTO;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo
{
    [TestClass]
    public class PruebaVentanaJuegoVistaModelo
    {
        private Mock<ISalasServicio> _mockSalasServicio;
        private Mock<IInvitacionesServicio> _mockInvitacionesServicio;
        private Mock<IListaAmigosServicio> _mockListaAmigosServicio;
        private Mock<IPerfilServicio> _mockPerfilServicio;
        private SalaDTO _salaDummy;
        private VentanaJuegoVistaModelo _viewModel;

        private const string UsuarioTest = "UsuarioPrueba";

        [TestInitialize]
        public void Inicializar()
        {
            _mockSalasServicio = new Mock<ISalasServicio>();
            _mockInvitacionesServicio = new Mock<IInvitacionesServicio>();
            _mockListaAmigosServicio = new Mock<IListaAmigosServicio>();
            _mockPerfilServicio = new Mock<IPerfilServicio>();

            _salaDummy = new SalaDTO
            {
                Codigo = "123456",
                Creador = "Creador",
                Jugadores = new[] { "Creador", "OtroJugador" }
            };

            _viewModel = new VentanaJuegoVistaModelo(
                _salaDummy,
                _mockSalasServicio.Object,
                _mockInvitacionesServicio.Object,
                _mockListaAmigosServicio.Object,
                _mockPerfilServicio.Object,
                nombreJugador: UsuarioTest,
                esInvitado: false
            );
        }

        [TestMethod]
        public void Prueba_Constructor_InicializacionCorrecta_EstableceValoresPorDefecto()
        {
            Assert.AreEqual("123456", _viewModel.CodigoSala);
            Assert.IsTrue(_viewModel.BotonIniciarPartidaHabilitado);
            Assert.AreEqual(6, _viewModel.Grosor); 
            Assert.IsTrue(_viewModel.PuedeInvitarPorCorreo);
            Assert.IsNotNull(_viewModel.Jugadores);
            Assert.AreEqual(2, _viewModel.Jugadores.Count); 
        }

        [TestMethod]
        public void Prueba_HerramientasDibujo_CambioLapizBorrador_ActualizaEstados()
        {
            _viewModel.EsHerramientaBorrador = true;

            Assert.IsTrue(_viewModel.EsHerramientaBorrador);
            Assert.IsFalse(_viewModel.EsHerramientaLapiz);

            _viewModel.EsHerramientaLapiz = true;

            Assert.IsTrue(_viewModel.EsHerramientaLapiz);
            Assert.IsFalse(_viewModel.EsHerramientaBorrador);
        }

        [TestMethod]
        public void Prueba_CambiarGrosor_ValorValido_ActualizaPropiedad()
        {
            _viewModel.CambiarGrosorComando.Execute("15.5");

            Assert.AreEqual(15.5, _viewModel.Grosor);
        }

        [TestMethod]
        public void Prueba_CambiarColor_ValorValido_ActualizaColorYSeleccionaLapiz()
        {
            _viewModel.EsHerramientaBorrador = true; 

            _viewModel.CambiarColorComando.Execute("Blue");

            Assert.AreEqual(Colors.Blue, _viewModel.Color);
            Assert.IsTrue(_viewModel.EsHerramientaLapiz, "Al cambiar color debe activarse el lápiz");
        }

        [TestMethod]
        public void Prueba_InvitarCorreo_CorreoInvalido_MuestraMensajeError()
        {
            _viewModel.CorreoInvitacion = "correo-sin-arroba";
            string mensajeMostrado = null;
            _viewModel.MostrarMensaje = (msj) => mensajeMostrado = msj;

            _viewModel.InvitarCorreoComando.Execute(null);

            _mockInvitacionesServicio.Verify(s => s.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.IsNotNull(mensajeMostrado);
        }

        [TestMethod]
        public async Task Prueba_InvitarCorreo_ServicioFalla_CapturaExcepcion()
        {
            _viewModel.CorreoInvitacion = "test@correo.com";
            string mensajeMostrado = null;
            _viewModel.MostrarMensaje = (msj) => mensajeMostrado = msj;

            _mockInvitacionesServicio
                .Setup(s => s.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "Error simulado WCF", null));

            _viewModel.InvitarCorreoComando.Execute(null);

            await Task.Delay(100);

            Assert.AreEqual("Error simulado WCF", mensajeMostrado);
        }

        [TestMethod]
        public async Task Prueba_InvitarCorreo_Exito_LimpiaCampoYMuestraExito()
        {
            _viewModel.CorreoInvitacion = "valido@correo.com";
            string mensajeMostrado = null;
            _viewModel.MostrarMensaje = (msj) => mensajeMostrado = msj;

            _mockInvitacionesServicio
                .Setup(s => s.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ResultadoOperacionDTO { OperacionExitosa = true });

            _viewModel.InvitarCorreoComando.Execute(null);
            await Task.Delay(100); 

            Assert.AreEqual(Lang.invitarCorreoTextoEnviado, mensajeMostrado);
            Assert.AreEqual(string.Empty, _viewModel.CorreoInvitacion);
        }

        [TestMethod]
        public void Prueba_JugadorSeUnio_NuevoJugador_SeAgregaALaLista()
        {
            string nuevoJugador = "JugadorNuevo";

            _mockSalasServicio.Raise(m => m.JugadorSeUnio += null, null, nuevoJugador);

            Assert.IsTrue(_viewModel.Jugadores.Any(j => j.Nombre == nuevoJugador));
        }

        [TestMethod]
        public void Prueba_JugadorExpulsado_EsUsuarioActual_NavegaFuera()
        {
            bool navegacionInvocada = false;
            _viewModel.ManejarExpulsion = (destino) => navegacionInvocada = true;

            _mockSalasServicio.Raise(m => m.JugadorExpulsado += null, null, UsuarioTest);

            Assert.IsTrue(navegacionInvocada, "Debería haber invocado la navegación de expulsión.");
        }

        [TestMethod]
        public void Prueba_JugadorExpulsado_EsOtroJugador_SeEliminaDeLista()
        {
            string otroJugador = "OtroJugador"; 
            Assert.IsTrue(_viewModel.Jugadores.Any(j => j.Nombre == otroJugador));

            _mockSalasServicio.Raise(m => m.JugadorExpulsado += null, null, otroJugador);

            Assert.IsFalse(_viewModel.Jugadores.Any(j => j.Nombre == otroJugador), "El jugador debería haber sido eliminado.");
        }

        [TestMethod]
        public async Task Prueba_FinalizarAsync_LlamaAbandonarYDispose()
        {
            await _viewModel.FinalizarAsync();

            _mockSalasServicio.Verify(s => s.AbandonarSalaAsync("123456", UsuarioTest), Times.Once);
        }

        [TestMethod]
        public async Task Prueba_InvitarAmigos_ListaConAmigos_AbreVentana()
        {
            var amigosMock = new List<AmigoDTO>
            {
                new AmigoDTO { NombreUsuario = "Amigo1" }
            };

            _mockListaAmigosServicio
                .Setup(s => s.ObtenerAmigosAsync(UsuarioTest))
                .ReturnsAsync(amigosMock);

            bool ventanaAbierta = false;
            _viewModel.MostrarInvitarAmigos = (vm) =>
            {
                ventanaAbierta = true;
                return Task.CompletedTask;
            };

            _viewModel.InvitarAmigosComando.Execute(null);
            await Task.Delay(100);

            Assert.IsTrue(ventanaAbierta);
        }
    }
}