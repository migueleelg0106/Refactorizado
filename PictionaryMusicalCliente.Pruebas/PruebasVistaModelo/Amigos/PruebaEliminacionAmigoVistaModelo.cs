using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using System.Windows;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Amigos
{
    [TestClass]
    public class PruebaEliminacionAmigoVistaModelo
    {
        private EliminacionAmigoVistaModelo _vistaModelo;

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

            _vistaModelo = new EliminacionAmigoVistaModelo(nombreAmigo);

            Assert.AreEqual(mensajeEsperado, _vistaModelo.MensajeConfirmacion);
            Assert.IsNotNull(_vistaModelo.AceptarComando);
            Assert.IsNotNull(_vistaModelo.CancelarComando);
        }

        [TestMethod]
        public void Prueba_Constructor_NombreNulo_UsaMensajePorDefecto()
        {
            _vistaModelo = new EliminacionAmigoVistaModelo(null);

            Assert.AreEqual(Lang.eliminarAmigoTextoConfirmacion, _vistaModelo.MensajeConfirmacion);
        }

        [TestMethod]
        public void Prueba_Constructor_NombreVacio_UsaMensajePorDefecto()
        {
            _vistaModelo = new EliminacionAmigoVistaModelo("");

            Assert.AreEqual(Lang.eliminarAmigoTextoConfirmacion, _vistaModelo.MensajeConfirmacion);
        }

        [TestMethod]
        public void Prueba_Constructor_NombreEspacios_UsaMensajePorDefecto()
        {
            _vistaModelo = new EliminacionAmigoVistaModelo("   ");

            Assert.AreEqual(Lang.eliminarAmigoTextoConfirmacion, _vistaModelo.MensajeConfirmacion);
        }

        [TestMethod]
        public void Prueba_AceptarComando_InvocaCerrarConTrue()
        {
            _vistaModelo = new EliminacionAmigoVistaModelo("Amigo");
            bool? resultadoCierre = null;
            _vistaModelo.Cerrar = (r) => resultadoCierre = r;

            _vistaModelo.AceptarComando.Execute(null);

            Assert.AreEqual(true, resultadoCierre);
        }

        [TestMethod]
        public void Prueba_CancelarComando_InvocaCerrarConFalse()
        {
            _vistaModelo = new EliminacionAmigoVistaModelo("Amigo");
            bool? resultadoCierre = null;
            _vistaModelo.Cerrar = (r) => resultadoCierre = r;

            _vistaModelo.CancelarComando.Execute(null);

            Assert.AreEqual(false, resultadoCierre);
        }

        [TestMethod]
        public void Prueba_AceptarComando_SinAccionCerrar_NoFalla()
        {
            _vistaModelo = new EliminacionAmigoVistaModelo("Amigo");
            _vistaModelo.Cerrar = null;

            try
            {
                _vistaModelo.AceptarComando.Execute(null);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void Prueba_CancelarComando_SinAccionCerrar_NoFalla()
        {
            _vistaModelo = new EliminacionAmigoVistaModelo("Amigo");
            _vistaModelo.Cerrar = null;

            try
            {
                _vistaModelo.CancelarComando.Execute(null);
            }
            catch
            {
                Assert.Fail();
            }
        }
    }
}