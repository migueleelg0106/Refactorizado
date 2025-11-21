using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Properties;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.VistaModelo.Ajustes;
using System;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Ajustes
{
    [TestClass]
    public class PruebaAjustesPartidaVistaModelo
    {
        private CancionManejador _cancionManejadorReal;
        private AjustesPartidaVistaModelo _vistaModelo;

        [TestInitialize]
        public void Inicializar()
        {
            Settings.Default.volumenCancion = 0.5;
            Settings.Default.efectosSilenciados = false;
            Settings.Default.Save();

            _cancionManejadorReal = new CancionManejador();
            _cancionManejadorReal.Volumen = 0.5;
            _vistaModelo = new AjustesPartidaVistaModelo(_cancionManejadorReal);
        }

        [TestCleanup]
        public void Limpiar()
        {
            _vistaModelo = null;
            _cancionManejadorReal = null;
            SonidoManejador.Silenciado = false;
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
            Assert.IsNotNull(_vistaModelo.ConfirmarComando, "ConfirmarComando no debe ser nulo.");
            Assert.IsNotNull(_vistaModelo.SalirPartidaComando, "SalirPartidaComando no debe ser nulo.");
        }

        #endregion

        #region Pruebas de Propiedad Volumen

        [TestMethod]
        public void Prueba_Volumen_Obtener_LeeDelServicio()
        {
            double volumenEsperado = 0.8;
            _cancionManejadorReal.Volumen = volumenEsperado;

            double volumenActual = _vistaModelo.Volumen;

            Assert.AreEqual(volumenEsperado, volumenActual, 0.0001, "El VistaModelo debe leer el volumen del servicio.");
        }

        [TestMethod]
        public void Prueba_Volumen_Establecer_ActualizaServicio()
        {
            _vistaModelo.Volumen = 0.3;

            Assert.AreEqual(0.3, _cancionManejadorReal.Volumen, 0.0001, "El VistaModelo debe actualizar el volumen en el servicio.");
        }

        [TestMethod]
        public void Prueba_Volumen_CambioValor_DisparaNotificacion()
        {
            bool notificacionRecibida = false;
            _vistaModelo.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AjustesPartidaVistaModelo.Volumen))
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
                if (e.PropertyName == nameof(AjustesPartidaVistaModelo.Volumen))
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
                if (e.PropertyName == nameof(AjustesPartidaVistaModelo.SonidosSilenciados))
                {
                    notificacionRecibida = true;
                }
            };

            _vistaModelo.SonidosSilenciados = true;

            Assert.IsTrue(notificacionRecibida);
        }

        #endregion

        #region Pruebas de Comandos

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
        public void Prueba_SalirPartidaComando_ConAccion_InvocaMostrarDialogoSalirPartida()
        {
            bool accionInvocada = false;
            _vistaModelo.MostrarDialogoSalirPartida = () => accionInvocada = true;

            _vistaModelo.SalirPartidaComando.Execute(null);

            Assert.IsTrue(accionInvocada, "El comando SalirPartida debe invocar la acción MostrarDialogoSalirPartida.");
        }

        [TestMethod]
        public void Prueba_SalirPartidaComando_SinAccion_NoFalla()
        {
            _vistaModelo.MostrarDialogoSalirPartida = null;

            try
            {
                _vistaModelo.SalirPartidaComando.Execute(null);
            }
            catch (Exception)
            {
                Assert.Fail("El comando falló al ejecutarse sin acción asignada.");
            }
        }

        #endregion
    }
}