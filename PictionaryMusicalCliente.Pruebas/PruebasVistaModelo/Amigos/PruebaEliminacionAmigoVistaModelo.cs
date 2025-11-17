using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using System.Windows;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Amigos
{
    [TestClass]
    public class PruebaEliminacionAmigoVistaModelo
    {
        private EliminacionAmigoVistaModelo _viewModel;

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null) new Application();
            Application.ResourceAssembly = typeof(EliminacionAmigoVistaModelo).Assembly;
        }

        [TestMethod]
        public void Prueba_Constructor_NombreValido_FormateaMensajeCorrectamente()
        {
            string nombreAmigo = "Juan";
            string mensajeEsperado = string.Concat(Lang.eliminarAmigoTextoConfirmacion, nombreAmigo, "?");

            _viewModel = new EliminacionAmigoVistaModelo(nombreAmigo);

            Assert.AreEqual(mensajeEsperado, _viewModel.MensajeConfirmacion);
            Assert.IsNotNull(_viewModel.AceptarComando);
            Assert.IsNotNull(_viewModel.CancelarComando);
        }

        [TestMethod]
        public void Prueba_Constructor_NombreNulo_UsaMensajePorDefecto()
        {
            _viewModel = new EliminacionAmigoVistaModelo(null);

            Assert.AreEqual(Lang.eliminarAmigoTextoConfirmacion, _viewModel.MensajeConfirmacion);
        }

        [TestMethod]
        public void Prueba_Constructor_NombreVacio_UsaMensajePorDefecto()
        {
            _viewModel = new EliminacionAmigoVistaModelo("");

            Assert.AreEqual(Lang.eliminarAmigoTextoConfirmacion, _viewModel.MensajeConfirmacion);
        }

        [TestMethod]
        public void Prueba_Constructor_NombreEspacios_UsaMensajePorDefecto()
        {
            _viewModel = new EliminacionAmigoVistaModelo("   ");

            Assert.AreEqual(Lang.eliminarAmigoTextoConfirmacion, _viewModel.MensajeConfirmacion);
        }

        [TestMethod]
        public void Prueba_AceptarComando_InvocaCerrarConTrue()
        {
            _viewModel = new EliminacionAmigoVistaModelo("Amigo");
            bool? resultadoCierre = null;
            _viewModel.Cerrar = (r) => resultadoCierre = r;

            _viewModel.AceptarComando.Execute(null);

            Assert.AreEqual(true, resultadoCierre);
        }

        [TestMethod]
        public void Prueba_CancelarComando_InvocaCerrarConFalse()
        {
            _viewModel = new EliminacionAmigoVistaModelo("Amigo");
            bool? resultadoCierre = null;
            _viewModel.Cerrar = (r) => resultadoCierre = r;

            _viewModel.CancelarComando.Execute(null);

            Assert.AreEqual(false, resultadoCierre);
        }

        [TestMethod]
        public void Prueba_AceptarComando_SinAccionCerrar_NoFalla()
        {
            _viewModel = new EliminacionAmigoVistaModelo("Amigo");
            _viewModel.Cerrar = null;

            try
            {
                _viewModel.AceptarComando.Execute(null);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void Prueba_CancelarComando_SinAccionCerrar_NoFalla()
        {
            _viewModel = new EliminacionAmigoVistaModelo("Amigo");
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