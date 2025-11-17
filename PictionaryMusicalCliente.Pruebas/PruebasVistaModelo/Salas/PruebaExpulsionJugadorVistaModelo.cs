using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.Salas;
using System.Windows;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Salas
{
    [TestClass]
    public class PruebaExpulsionJugadorVistaModelo
    {
        private ExpulsionJugadorVistaModelo _viewModel;

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null) new Application();
            Application.ResourceAssembly = typeof(ExpulsionJugadorVistaModelo).Assembly;
        }

        [TestCleanup]
        public void Limpiar()
        {
            _viewModel = null;
        }

        [TestMethod]
        public void Constructor_MensajeValido_AsignaMensaje()
        {
            string mensajeEsperado = "Mensaje de prueba";
            _viewModel = new ExpulsionJugadorVistaModelo(mensajeEsperado);

            Assert.AreEqual(mensajeEsperado, _viewModel.MensajeConfirmacion);
            Assert.IsNotNull(_viewModel.ConfirmarComando);
            Assert.IsNotNull(_viewModel.CancelarComando);
        }

        [TestMethod]
        public void Constructor_MensajeNulo_AsignaMensajePorDefecto()
        {
            _viewModel = new ExpulsionJugadorVistaModelo(null);

            Assert.AreEqual(Lang.expulsarTextoConfirmacion, _viewModel.MensajeConfirmacion);
        }

        [TestMethod]
        public void Constructor_MensajeVacio_AsignaMensajePorDefecto()
        {
            _viewModel = new ExpulsionJugadorVistaModelo(string.Empty);

            Assert.AreEqual(Lang.expulsarTextoConfirmacion, _viewModel.MensajeConfirmacion);
        }

        [TestMethod]
        public void Constructor_MensajeEspaciosEnBlanco_AsignaMensajePorDefecto()
        {
            _viewModel = new ExpulsionJugadorVistaModelo("   ");

            Assert.AreEqual(Lang.expulsarTextoConfirmacion, _viewModel.MensajeConfirmacion);
        }

        [TestMethod]
        public void ConfirmarComando_Ejecutar_InvocaCerrarConTrue()
        {
            _viewModel = new ExpulsionJugadorVistaModelo("Mensaje");
            bool? resultado = null;
            _viewModel.Cerrar = (valor) => resultado = valor;

            _viewModel.ConfirmarComando.Execute(null);

            Assert.IsTrue(resultado.HasValue);
            Assert.IsTrue(resultado.Value);
        }

        [TestMethod]
        public void CancelarComando_Ejecutar_InvocaCerrarConFalse()
        {
            _viewModel = new ExpulsionJugadorVistaModelo("Mensaje");
            bool? resultado = null;
            _viewModel.Cerrar = (valor) => resultado = valor;

            _viewModel.CancelarComando.Execute(null);

            Assert.IsTrue(resultado.HasValue);
            Assert.IsFalse(resultado.Value);
        }

        [TestMethod]
        public void ConfirmarComando_SinAccionCerrar_NoLanzaExcepcion()
        {
            _viewModel = new ExpulsionJugadorVistaModelo("Mensaje");
            _viewModel.Cerrar = null;

            try
            {
                _viewModel.ConfirmarComando.Execute(null);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void CancelarComando_SinAccionCerrar_NoLanzaExcepcion()
        {
            _viewModel = new ExpulsionJugadorVistaModelo("Mensaje");
            _viewModel.Cerrar = null;

            try
            {
                _viewModel.CancelarComando.Execute(null);
            }
            catch
            {
                Assert.Fail();
            }
        }
    }
}