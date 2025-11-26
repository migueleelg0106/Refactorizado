using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.Salas;
using System.Windows;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Salas
{
    [TestClass]
    public class PruebaChatVistaModelo
    {
        private ChatVistaModelo _vistaModelo;

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null) new Application();
            Application.ResourceAssembly = typeof(ChatVistaModelo).Assembly;
            _vistaModelo = new ChatVistaModelo();
        }

        [TestCleanup]
        public void Limpiar()
        {
            _vistaModelo = null;
        }

        [TestMethod]
        public void Constructor_EstadoInicial_PropiedadesCorrectas()
        {
            Assert.IsTrue(_vistaModelo.PuedeEscribir);
            Assert.IsFalse(_vistaModelo.EsPartidaIniciada);
            Assert.IsFalse(_vistaModelo.EsDibujante);
            Assert.AreEqual(string.Empty, _vistaModelo.NombreCancionCorrecta);
            Assert.AreEqual(0, _vistaModelo.TiempoRestante);
        }

        [TestMethod]
        public void EnviarMensaje_MensajeVacio_NoEnviaNada()
        {
            bool mensajeEnviado = false;
            _vistaModelo.EnviarMensajeAlServidor = _ => mensajeEnviado = true;

            _vistaModelo.EnviarMensaje(string.Empty);

            Assert.IsFalse(mensajeEnviado);
        }

        [TestMethod]
        public void EnviarMensaje_MensajeNulo_NoEnviaNada()
        {
            bool mensajeEnviado = false;
            _vistaModelo.EnviarMensajeAlServidor = _ => mensajeEnviado = true;

            _vistaModelo.EnviarMensaje(null);

            Assert.IsFalse(mensajeEnviado);
        }

        [TestMethod]
        public void EnviarMensaje_MensajeEspaciosEnBlanco_NoEnviaNada()
        {
            bool mensajeEnviado = false;
            _vistaModelo.EnviarMensajeAlServidor = _ => mensajeEnviado = true;

            _vistaModelo.EnviarMensaje("   ");

            Assert.IsFalse(mensajeEnviado);
        }

        [TestMethod]
        public void EnviarMensaje_PartidaNoIniciada_EnviaMensajeDirectamente()
        {
            string mensajeEnviado = null;
            _vistaModelo.EsPartidaIniciada = false;
            _vistaModelo.EnviarMensajeAlServidor = mensaje => mensajeEnviado = mensaje;

            _vistaModelo.EnviarMensaje("Hola mundo");

            Assert.AreEqual("Hola mundo", mensajeEnviado);
        }

        [TestMethod]
        public void EnviarMensaje_EsDibujante_NoEnviaMensaje()
        {
            bool mensajeEnviado = false;
            _vistaModelo.EsPartidaIniciada = true;
            _vistaModelo.EsDibujante = true;
            _vistaModelo.EnviarMensajeAlServidor = _ => mensajeEnviado = true;

            _vistaModelo.EnviarMensaje("Hola mundo");

            Assert.IsFalse(mensajeEnviado);
        }

        [TestMethod]
        public void EnviarMensaje_AdivinadorRespuestaIncorrecta_EnviaMensaje()
        {
            string mensajeEnviado = null;
            _vistaModelo.EsPartidaIniciada = true;
            _vistaModelo.EsDibujante = false;
            _vistaModelo.NombreCancionCorrecta = "Gasolina";
            _vistaModelo.EnviarMensajeAlServidor = mensaje => mensajeEnviado = mensaje;

            _vistaModelo.EnviarMensaje("Respuesta incorrecta");

            Assert.AreEqual("Respuesta incorrecta", mensajeEnviado);
        }

        //[TestMethod]
        //public void EnviarMensaje_AdivinadorRespuestaCorrecta_NoEnviaMensajeAlChat()
        //{
        //    bool mensajeEnviado = false;
        //    _vistaModelo.EsPartidaIniciada = true;
        //    _vistaModelo.EsDibujante = false;
        //    _vistaModelo.NombreCancionCorrecta = "Gasolina";
        //    _vistaModelo.TiempoRestante = 30;
        //    _vistaModelo.ObtenerNombreJugadorActual = () => "TestPlayer";
        //    _vistaModelo.RegistrarAciertoEnServidor = (_, __, ___) => { };
        //    _vistaModelo.EnviarMensajeAlServidor = _ => mensajeEnviado = true;

        //    _vistaModelo.EnviarMensaje("Gasolina");

        //    Assert.IsFalse(mensajeEnviado);
        //}

        //[TestMethod]
        //public void EnviarMensaje_AdivinadorRespuestaCorrecta_IgnoraMayusculasMinusculas()
        //{
        //    bool mensajeEnviado = false;
        //    _vistaModelo.EsPartidaIniciada = true;
        //    _vistaModelo.EsDibujante = false;
        //    _vistaModelo.NombreCancionCorrecta = "Gasolina";
        //    _vistaModelo.TiempoRestante = 30;
        //    _vistaModelo.ObtenerNombreJugadorActual = () => "TestPlayer";
        //    _vistaModelo.RegistrarAciertoEnServidor = (_, __, ___) => { };
        //    _vistaModelo.EnviarMensajeAlServidor = _ => mensajeEnviado = true;

        //    _vistaModelo.EnviarMensaje("GASOLINA");

        //    Assert.IsFalse(mensajeEnviado);
        //}

        //[TestMethod]
        //public void EnviarMensaje_AdivinadorRespuestaCorrecta_CalculaPuntosCorrectamente()
        //{
        //    int puntosAdivinadorRecibidos = 0;
        //    int puntosDibujanteRecibidos = 0;
        //    _vistaModelo.EsPartidaIniciada = true;
        //    _vistaModelo.EsDibujante = false;
        //    _vistaModelo.NombreCancionCorrecta = "Gasolina";
        //    _vistaModelo.TiempoRestante = 50;
        //    _vistaModelo.ObtenerNombreJugadorActual = () => "TestPlayer";
        //    _vistaModelo.RegistrarAciertoEnServidor = (nombre, puntosAdiv, puntosDib) =>
        //    {
        //        puntosAdivinadorRecibidos = puntosAdiv;
        //        puntosDibujanteRecibidos = puntosDib;
        //    };

        //    _vistaModelo.EnviarMensaje("Gasolina");

        //    Assert.AreEqual(50, puntosAdivinadorRecibidos);
        //    Assert.AreEqual(10, puntosDibujanteRecibidos);
        //}

        //[TestMethod]
        //public void EnviarMensaje_AdivinadorRespuestaCorrecta_RegistraAcierto()
        //{
        //    bool aciertoRegistrado = false;
        //    string nombreJugadorRegistrado = null;
        //    _vistaModelo.EsPartidaIniciada = true;
        //    _vistaModelo.EsDibujante = false;
        //    _vistaModelo.NombreCancionCorrecta = "Gasolina";
        //    _vistaModelo.TiempoRestante = 30;
        //    _vistaModelo.ObtenerNombreJugadorActual = () => "TestPlayer";
        //    _vistaModelo.RegistrarAciertoEnServidor = (nombre, _, __) =>
        //    {
        //        aciertoRegistrado = true;
        //        nombreJugadorRegistrado = nombre;
        //    };

        //    _vistaModelo.EnviarMensaje("Gasolina");

        //    Assert.IsTrue(aciertoRegistrado);
        //    Assert.AreEqual("TestPlayer", nombreJugadorRegistrado);
        //}

        [TestMethod]
        public void PuedeEscribir_CambioPropiedadNotificado()
        {
            bool propiedadCambiada = false;
            _vistaModelo.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ChatVistaModelo.PuedeEscribir))
                {
                    propiedadCambiada = true;
                }
            };

            _vistaModelo.PuedeEscribir = false;

            Assert.IsTrue(propiedadCambiada);
        }

        [TestMethod]
        public void EsPartidaIniciada_CambioPropiedadNotificado()
        {
            bool propiedadCambiada = false;
            _vistaModelo.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ChatVistaModelo.EsPartidaIniciada))
                {
                    propiedadCambiada = true;
                }
            };

            _vistaModelo.EsPartidaIniciada = true;

            Assert.IsTrue(propiedadCambiada);
        }

        [TestMethod]
        public void EsDibujante_CambioPropiedadNotificado()
        {
            bool propiedadCambiada = false;
            _vistaModelo.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ChatVistaModelo.EsDibujante))
                {
                    propiedadCambiada = true;
                }
            };

            _vistaModelo.EsDibujante = true;

            Assert.IsTrue(propiedadCambiada);
        }

        [TestMethod]
        public void NombreCancionCorrecta_CambioPropiedadNotificado()
        {
            bool propiedadCambiada = false;
            _vistaModelo.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ChatVistaModelo.NombreCancionCorrecta))
                {
                    propiedadCambiada = true;
                }
            };

            _vistaModelo.NombreCancionCorrecta = "NuevaCancion";

            Assert.IsTrue(propiedadCambiada);
        }

        [TestMethod]
        public void TiempoRestante_CambioPropiedadNotificado()
        {
            bool propiedadCambiada = false;
            _vistaModelo.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ChatVistaModelo.TiempoRestante))
                {
                    propiedadCambiada = true;
                }
            };

            _vistaModelo.TiempoRestante = 45;

            Assert.IsTrue(propiedadCambiada);
        }

        [TestMethod]
        public void EnviarMensaje_SinNombreCancionCorrecta_EnviaMensaje()
        {
            string mensajeEnviado = null;
            _vistaModelo.EsPartidaIniciada = true;
            _vistaModelo.EsDibujante = false;
            _vistaModelo.NombreCancionCorrecta = string.Empty;
            _vistaModelo.EnviarMensajeAlServidor = mensaje => mensajeEnviado = mensaje;

            _vistaModelo.EnviarMensaje("Cualquier mensaje");

            Assert.AreEqual("Cualquier mensaje", mensajeEnviado);
        }

        //[TestMethod]
        //public void EnviarMensaje_AdivinadorRespuestaConEspacios_ComparaCorrectamente()
        //{
        //    bool mensajeEnviado = false;
        //    _vistaModelo.EsPartidaIniciada = true;
        //    _vistaModelo.EsDibujante = false;
        //    _vistaModelo.NombreCancionCorrecta = "Gasolina";
        //    _vistaModelo.TiempoRestante = 30;
        //    _vistaModelo.ObtenerNombreJugadorActual = () => "TestPlayer";
        //    _vistaModelo.RegistrarAciertoEnServidor = (_, __, ___) => { };
        //    _vistaModelo.EnviarMensajeAlServidor = _ => mensajeEnviado = true;

        //    _vistaModelo.EnviarMensaje("  Gasolina  ");

        //    Assert.IsFalse(mensajeEnviado);
        //}
    }
}
