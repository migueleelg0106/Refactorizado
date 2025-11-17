using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalCliente.Utilidades; 
using PictionaryMusicalCliente.VistaModelo.Ajustes;
using System;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Ajustes
{
    [TestClass]
    public class PruebaAjustesPartidaVistaModelo
    {
        private CancionManejador _cancionManejadorReal; 
        private AjustesPartidaVistaModelo _viewModel;

        [TestInitialize]
        public void Inicializar()
        {
            _cancionManejadorReal = new CancionManejador();
            _cancionManejadorReal.Volumen = 0.5; 
            _viewModel = new AjustesPartidaVistaModelo(_cancionManejadorReal);
        }

        [TestCleanup]
        public void Limpiar()
        {
            _viewModel = null;
            _cancionManejadorReal = null;
        }

        #region Pruebas de Constructor

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_ServicioCancionNulo_LanzaExcepcion()
        {
            new AjustesPartidaVistaModelo(null);
        }

        [TestMethod]
        public void Prueba_Constructor_InicializacionCorrecta_ComandosNoNulos()
        {
            Assert.IsNotNull(_viewModel.ConfirmarComando, "ConfirmarComando no debe ser nulo.");
            Assert.IsNotNull(_viewModel.SalirPartidaComando, "SalirPartidaComando no debe ser nulo.");
        }

        #endregion

        #region Pruebas de Propiedad Volumen

        [TestMethod]
        public void Prueba_Volumen_Obtener_LeeDelServicio()
        {
            double volumenEsperado = 0.8;
            _cancionManejadorReal.Volumen = volumenEsperado;

            double volumenActual = _viewModel.Volumen;

            Assert.AreEqual(volumenEsperado, volumenActual, 0.0001, "El ViewModel debe leer el volumen del servicio.");
        }

        [TestMethod]
        public void Prueba_Volumen_Establecer_ActualizaServicio()
        {
            _viewModel.Volumen = 0.3;

            Assert.AreEqual(0.3, _cancionManejadorReal.Volumen, 0.0001, "El ViewModel debe actualizar el volumen en el servicio.");
        }

        [TestMethod]
        public void Prueba_Volumen_CambioValor_DisparaNotificacion()
        {
            bool notificacionRecibida = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AjustesPartidaVistaModelo.Volumen))
                    notificacionRecibida = true;
            };

            _viewModel.Volumen = 0.9;

            Assert.IsTrue(notificacionRecibida, "Debe notificar PropertyChanged al cambiar el volumen.");
        }

        [TestMethod]
        public void Prueba_Volumen_MismoValor_NoDisparaNotificacion()
        {
            _viewModel.Volumen = 0.5;
            bool notificacionRecibida = false;

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AjustesPartidaVistaModelo.Volumen))
                    notificacionRecibida = true;
            };

            _viewModel.Volumen = 0.5;

            Assert.IsFalse(notificacionRecibida, "No debe notificar si el valor es idéntico.");
        }

        #endregion

        #region Pruebas de Comandos

        [TestMethod]
        public void Prueba_ConfirmarComando_ConAccion_InvocaOcultarVentana()
        {
            bool accionInvocada = false;
            _viewModel.OcultarVentana = () => accionInvocada = true;

            _viewModel.ConfirmarComando.Execute(null);

            Assert.IsTrue(accionInvocada, "El comando Confirmar debe invocar la acción OcultarVentana.");
        }

        [TestMethod]
        public void Prueba_ConfirmarComando_SinAccion_NoFalla()
        {
            _viewModel.OcultarVentana = null;

            try
            {
                _viewModel.ConfirmarComando.Execute(null);
            }
            catch (Exception)
            {
                Assert.Fail("El comando falló al ejecutarse sin acción asignada.");
            }
        }

        [TestMethod]
        public void Prueba_SalirPartidaComando_ConAccion_InvocaMostrarDialogoSalirPartida()
        {
            bool accionInvocada = false;
            _viewModel.MostrarDialogoSalirPartida = () => accionInvocada = true;

            _viewModel.SalirPartidaComando.Execute(null);

            Assert.IsTrue(accionInvocada, "El comando SalirPartida debe invocar la acción MostrarDialogoSalirPartida.");
        }

        [TestMethod]
        public void Prueba_SalirPartidaComando_SinAccion_NoFalla()
        {
            _viewModel.MostrarDialogoSalirPartida = null;

            try
            {
                _viewModel.SalirPartidaComando.Execute(null);
            }
            catch (Exception)
            {
                Assert.Fail("El comando falló al ejecutarse sin acción asignada.");
            }
        }

        #endregion
    }
}