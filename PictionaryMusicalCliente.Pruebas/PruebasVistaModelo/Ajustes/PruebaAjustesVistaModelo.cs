using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.VistaModelo.Ajustes;
using System;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Ajustes
{
    [TestClass]
    public class PruebaAjustesVistaModelo
    {
        private MusicaManejador _musicaManejadorReal;
        private AjustesVistaModelo _viewModel;

        [TestInitialize]
        public void Inicializar()
        {
            _musicaManejadorReal = new MusicaManejador();
            _musicaManejadorReal.Volumen = 0.5;
            _viewModel = new AjustesVistaModelo(_musicaManejadorReal);
        }

        [TestCleanup]
        public void Limpiar()
        {
            _viewModel = null;
        }

        #region Pruebas de Constructor

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_ServicioMusicaNulo_LanzaExcepcion()
        {
            new AjustesVistaModelo(null);
        }

        [TestMethod]
        public void Prueba_Constructor_InicializacionCorrecta_ComandosNoNulos()
        {
            Assert.IsNotNull(_viewModel.ConfirmarComando);
            Assert.IsNotNull(_viewModel.CerrarSesionComando);
        }

        #endregion

        #region Pruebas de Propiedad Volumen

        [TestMethod]
        public void Prueba_Volumen_Obtener_LeeDelServicio()
        {
            double volumenEsperado = 0.8;
            _musicaManejadorReal.Volumen = volumenEsperado;

            double volumenActual = _viewModel.Volumen;

            Assert.AreEqual(volumenEsperado, volumenActual, 0.0001);
        }

        [TestMethod]
        public void Prueba_Volumen_Establecer_ActualizaServicio()
        {
            _viewModel.Volumen = 0.3;
            Assert.AreEqual(0.3, _musicaManejadorReal.Volumen, 0.0001);
        }

        [TestMethod]
        public void Prueba_Volumen_CambioValor_DisparaNotificacion()
        {
            bool notificacionRecibida = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AjustesVistaModelo.Volumen))
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
                if (e.PropertyName == nameof(AjustesVistaModelo.Volumen))
                    notificacionRecibida = true;
            };

            _viewModel.Volumen = 0.5;

            Assert.IsFalse(notificacionRecibida, "No debe notificar si el valor es idéntico.");
        }

        #endregion

        #region Pruebas de Comandos (Cobertura Total de Invoke)

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
        public void Prueba_CerrarSesionComando_ConAccion_InvocaMostrarDialogo()
        {
            bool accionInvocada = false;
            _viewModel.MostrarDialogoCerrarSesion = () => accionInvocada = true;

            _viewModel.CerrarSesionComando.Execute(null);

            Assert.IsTrue(accionInvocada, "El comando CerrarSesion debe invocar la acción MostrarDialogoCerrarSesion.");
        }

        [TestMethod]
        public void Prueba_CerrarSesionComando_SinAccion_NoFalla()
        {
            _viewModel.MostrarDialogoCerrarSesion = null;

            try
            {
                _viewModel.CerrarSesionComando.Execute(null);
            }
            catch (Exception)
            {
                Assert.Fail("El comando falló al ejecutarse sin acción asignada.");
            }
        }

        #endregion
    }
}