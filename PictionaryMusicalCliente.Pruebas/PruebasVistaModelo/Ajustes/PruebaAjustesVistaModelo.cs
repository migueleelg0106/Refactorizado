using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.Properties;
using PictionaryMusicalCliente.VistaModelo.Ajustes;
using System;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Ajustes
{
    [TestClass]
    public class PruebaAjustesVistaModelo
    {
        private MusicaManejador _musicaManejadorReal;
        private AjustesVistaModelo _vistaModelo;

        [TestInitialize]
        public void Inicializar()
        {
            Settings.Default.volumenMusica = 0.5;
            Settings.Default.efectosSilenciados = false;
            Settings.Default.Save();

            _musicaManejadorReal = new MusicaManejador();
            _musicaManejadorReal.Volumen = 0.5;
            _vistaModelo = new AjustesVistaModelo(_musicaManejadorReal);
        }

        [TestCleanup]
        public void Limpiar()
        {
            _vistaModelo = null;
            SonidoManejador.Silenciado = false;
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
            Assert.IsNotNull(_vistaModelo.ConfirmarComando);
            Assert.IsNotNull(_vistaModelo.CerrarSesionComando);
        }

        #endregion

        #region Pruebas de Propiedad Volumen

        [TestMethod]
        public void Prueba_Volumen_Obtener_LeeDelServicio()
        {
            double volumenEsperado = 0.8;
            _musicaManejadorReal.Volumen = volumenEsperado;

            double volumenActual = _vistaModelo.Volumen;

            Assert.AreEqual(volumenEsperado, volumenActual, 0.0001);
        }

        [TestMethod]
        public void Prueba_Volumen_Establecer_ActualizaServicio()
        {
            _vistaModelo.Volumen = 0.3;
            Assert.AreEqual(0.3, _musicaManejadorReal.Volumen, 0.0001);
        }

        [TestMethod]
        public void Prueba_Volumen_CambioValor_DisparaNotificacion()
        {
            bool notificacionRecibida = false;
            _vistaModelo.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AjustesVistaModelo.Volumen))
                    notificacionRecibida = true;
            };

            _vistaModelo.Volumen = 0.9;

            Assert.IsTrue(notificacionRecibida, "Debe notificar PropertyChanged al cambiar el volumen.");
        }

        [TestMethod]
        public void Prueba_Volumen_MismoValor_NoDisparaNotificacion()
        {
            _vistaModelo.Volumen = 0.5;
            bool notificacionRecibida = false;

            _vistaModelo.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AjustesVistaModelo.Volumen))
                    notificacionRecibida = true;
            };

            _vistaModelo.Volumen = 0.5;

            Assert.IsFalse(notificacionRecibida, "No debe notificar si el valor es idéntico.");
        }

        #endregion

        #region Pruebas de Propiedad SonidosSilenciados

        [TestMethod]
        public void Prueba_SonidosSilenciados_Obtener_LeeDePreferencia()
        {
            SonidoManejador.Silenciado = true;

            bool estadoActual = _vistaModelo.SonidosSilenciados;

            Assert.IsTrue(estadoActual);
        }

        [TestMethod]
        public void Prueba_SonidosSilenciados_Establecer_ActualizaPreferencia()
        {
            _vistaModelo.SonidosSilenciados = true;

            Assert.IsTrue(SonidoManejador.Silenciado);
        }

        [TestMethod]
        public void Prueba_SonidosSilenciados_CambioValor_DisparaNotificacion()
        {
            bool notificacionRecibida = false;
            _vistaModelo.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AjustesVistaModelo.SonidosSilenciados))
                {
                    notificacionRecibida = true;
                }
            };

            _vistaModelo.SonidosSilenciados = true;

            Assert.IsTrue(notificacionRecibida);
        }

        #endregion

        #region Pruebas de Comandos (Cobertura Total de Invoke)

        [TestMethod]
        public void Prueba_ConfirmarComando_ConAccion_InvocaOcultarVentana()
        {
            bool accionInvocada = false;
            _vistaModelo.OcultarVentana = () => accionInvocada = true;

            _vistaModelo.ConfirmarComando.Execute(null);

            Assert.IsTrue(accionInvocada, "El comando Confirmar debe invocar la acción OcultarVentana.");
        }

        [TestMethod]
        public void Prueba_ConfirmarComando_SinAccion_NoFalla()
        {
            _vistaModelo.OcultarVentana = null;

            try
            {
                _vistaModelo.ConfirmarComando.Execute(null);
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
            _vistaModelo.MostrarDialogoCerrarSesion = () => accionInvocada = true;

            _vistaModelo.CerrarSesionComando.Execute(null);

            Assert.IsTrue(accionInvocada, "El comando CerrarSesion debe invocar la acción MostrarDialogoCerrarSesion.");
        }

        [TestMethod]
        public void Prueba_CerrarSesionComando_SinAccion_NoFalla()
        {
            _vistaModelo.MostrarDialogoCerrarSesion = null;

            try
            {
                _vistaModelo.CerrarSesionComando.Execute(null);
            }
            catch (Exception)
            {
                Assert.Fail("El comando falló al ejecutarse sin acción asignada.");
            }
        }

        #endregion
    }
}