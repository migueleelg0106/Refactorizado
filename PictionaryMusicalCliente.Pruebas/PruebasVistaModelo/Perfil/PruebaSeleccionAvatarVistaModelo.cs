using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Perfil
{
    [TestClass]
    public class PruebaSeleccionAvatarVistaModelo
    {
        private SeleccionAvatarVistaModelo _vistaModelo;
        private List<ObjetoAvatar> _listaAvatares;

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null) new Application();
            Application.ResourceAssembly = typeof(SeleccionAvatarVistaModelo).Assembly;

            AvisoAyudante.DefinirMostrarAviso((_) => { });

            _listaAvatares = new List<ObjetoAvatar>
            {
                new ObjetoAvatar(1, "Avatar1", null),
                new ObjetoAvatar(2, "Avatar2", null)
            };

            _vistaModelo = new SeleccionAvatarVistaModelo(_listaAvatares);
        }

        [TestCleanup]
        public void Limpiar()
        {
            _vistaModelo = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ListaNula_LanzaExcepcion()
        {
            new SeleccionAvatarVistaModelo(null);
        }

        [TestMethod]
        public void Constructor_ListaValida_InicializaColeccion()
        {
            Assert.IsNotNull(_vistaModelo.Avatares);
            Assert.AreEqual(2, _vistaModelo.Avatares.Count);
            Assert.IsNotNull(_vistaModelo.ConfirmarSeleccionComando);
        }

        [TestMethod]
        public void Propiedad_AvatarSeleccionado_NotificaCambio()
        {
            bool notificado = false;
            _vistaModelo.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SeleccionAvatarVistaModelo.AvatarSeleccionado))
                {
                    notificado = true;
                }
            };

            _vistaModelo.AvatarSeleccionado = _listaAvatares[0];

            Assert.IsTrue(notificado);
            Assert.AreEqual(_listaAvatares[0], _vistaModelo.AvatarSeleccionado);
        }

        [TestMethod]
        public void ConfirmarSeleccion_SinAvatarSeleccionado_MuestraErrorYNoCierra()
        {
            _vistaModelo.AvatarSeleccionado = null;
            string mensajeMostrado = null;
            bool cerroVentana = false;
            bool seleccionConfirmada = false;

            AvisoAyudante.DefinirMostrarAviso((m) => mensajeMostrado = m);
            _vistaModelo.CerrarAccion = () => cerroVentana = true;
            _vistaModelo.SeleccionConfirmada = (_) => seleccionConfirmada = true;

            _vistaModelo.ConfirmarSeleccionComando.Execute(null);

            Assert.AreEqual(Lang.errorTextoSeleccionAvatarValido, mensajeMostrado);
            Assert.IsFalse(cerroVentana);
            Assert.IsFalse(seleccionConfirmada);
        }

        [TestMethod]
        public void ConfirmarSeleccion_ConAvatarSeleccionado_InvocaAcciones()
        {
            var avatar = _listaAvatares[1];
            _vistaModelo.AvatarSeleccionado = avatar;

            bool cerroVentana = false;
            ObjetoAvatar avatarConfirmado = null;

            _vistaModelo.CerrarAccion = () => cerroVentana = true;
            _vistaModelo.SeleccionConfirmada = (a) => avatarConfirmado = a;

            _vistaModelo.ConfirmarSeleccionComando.Execute(null);

            Assert.IsTrue(cerroVentana);
            Assert.AreEqual(avatar, avatarConfirmado);
        }
    }
}