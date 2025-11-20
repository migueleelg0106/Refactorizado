using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.Salas;
using System.Windows;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Salas
{
    [TestClass]
    public class PruebaExpulsionJugadorVistaModelo
    {
        private ExpulsionJugadorVistaModelo _vistaModelo;

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null) new Application();
            Application.ResourceAssembly = typeof(ExpulsionJugadorVistaModelo).Assembly;
        }

        [TestCleanup]
        public void Limpiar()
        {
            _vistaModelo = null;
        }

        [TestMethod]
        public void Constructor_MensajeValido_AsignaMensaje()
        {
            string mensajeEsperado = "Mensaje de prueba";
            _vistaModelo = new ExpulsionJugadorVistaModelo(mensajeEsperado);

            Assert.AreEqual(mensajeEsperado, _vistaModelo.MensajeConfirmacion);
            Assert.IsNotNull(_vistaModelo.ConfirmarComando);
            Assert.IsNotNull(_vistaModelo.CancelarComando);
        }

        [TestMethod]
        public void Constructor_MensajeNulo_AsignaMensajePorDefecto()
        {
            _vistaModelo = new ExpulsionJugadorVistaModelo(null);

            Assert.AreEqual(Lang.expulsarTextoConfirmacion, _vistaModelo.MensajeConfirmacion);
        }

        [TestMethod]
        public void Constructor_MensajeVacio_AsignaMensajePorDefecto()
        {
            _vistaModelo = new ExpulsionJugadorVistaModelo(string.Empty);

            Assert.AreEqual(Lang.expulsarTextoConfirmacion, _vistaModelo.MensajeConfirmacion);
        }

        [TestMethod]
        public void Constructor_MensajeEspaciosEnBlanco_AsignaMensajePorDefecto()
        {
            _vistaModelo = new ExpulsionJugadorVistaModelo("   ");

            Assert.AreEqual(Lang.expulsarTextoConfirmacion, _vistaModelo.MensajeConfirmacion);
        }

        [TestMethod]
        public void ConfirmarComando_Ejecutar_InvocaCerrarConTrue()
        {
            _vistaModelo = new ExpulsionJugadorVistaModelo("Mensaje");
            bool? resultado = null;
            _vistaModelo.Cerrar = (valor) => resultado = valor;

            _vistaModelo.ConfirmarComando.Execute(null);

            Assert.IsTrue(resultado.HasValue);
            Assert.IsTrue(resultado.Value);
        }

        [TestMethod]
        public void CancelarComando_Ejecutar_InvocaCerrarConFalse()
        {
            _vistaModelo = new ExpulsionJugadorVistaModelo("Mensaje");
            bool? resultado = null;
            _vistaModelo.Cerrar = (valor) => resultado = valor;

            _vistaModelo.CancelarComando.Execute(null);

            Assert.IsTrue(resultado.HasValue);
            Assert.IsFalse(resultado.Value);
        }

        [TestMethod]
        public void ConfirmarComando_SinAccionCerrar_NoLanzaExcepcion()
        {
            _vistaModelo = new ExpulsionJugadorVistaModelo("Mensaje");
            _vistaModelo.Cerrar = null;

            try
            {
                _vistaModelo.ConfirmarComando.Execute(null);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void CancelarComando_SinAccionCerrar_NoLanzaExcepcion()
        {
            _vistaModelo = new ExpulsionJugadorVistaModelo("Mensaje");
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