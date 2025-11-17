using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Pruebas.Utilidades;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.VistaModelo;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using System.Windows;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo
{
    [TestClass]
    public class PruebaTerminacionSesionVistaModelo
    {
        [TestInitialize]
        public void Inicializar()
        {
            UsuarioAutenticado.Instancia.Limpiar();
        }

        [TestCleanup]
        public void Limpiar()
        {
            UsuarioAutenticado.Instancia.Limpiar();
        }

        [TestMethod]
        public void Prueba_Constructor_ComandosInicializados()
        {
            var vistaModelo = new TerminacionSesionVistaModelo();

            Assert.IsNotNull(vistaModelo.AceptarComando);
            Assert.IsNotNull(vistaModelo.CancelarComando);
            Assert.IsNotNull(vistaModelo.CrearVentanaInicioSesion);
        }

        [TestMethod]
        public void Prueba_CancelarComando_InvocaOcultarDialogo()
        {
            var vistaModelo = new TerminacionSesionVistaModelo();
            bool dialogoOculto = false;
            vistaModelo.OcultarDialogo = () => dialogoOculto = true;

            vistaModelo.CancelarComando.Execute(null);

            Assert.IsTrue(dialogoOculto);
        }

        [TestMethod]
        public void Prueba_AceptarComando_CierraSesionYMuestraInicio()
        {
            bool dialogoOculto = false;

            StaTestHelper.Ejecutar(() =>
            {
                var app = new Application();
                var ventanaExistente = new VentanaPrueba();
                ventanaExistente.Show();

                var vistaModelo = new TerminacionSesionVistaModelo();
                vistaModelo.OcultarDialogo = () => dialogoOculto = true;
                vistaModelo.CrearVentanaInicioSesion = () => new VentanaPrueba();

                SesionUsuarioActual.EstablecerUsuario(new UsuarioDTO
                {
                    UsuarioId = 1,
                    NombreUsuario = "UsuarioPrueba"
                });

                vistaModelo.AceptarComando.Execute(null);

                Assert.IsNull(UsuarioAutenticado.Instancia.NombreUsuario);
                Assert.IsInstanceOfType(Application.Current.MainWindow, typeof(VentanaPrueba));
                Assert.IsTrue(ventanaExistente.FueCerrada);

                (Application.Current.MainWindow as Window)?.Close();
                app.Shutdown();
            });

            Assert.IsTrue(dialogoOculto);
        }

        private class VentanaPrueba : Window
        {
            public bool FueCerrada { get; private set; }

            public VentanaPrueba()
            {
                ShowInTaskbar = false;
                WindowStyle = WindowStyle.None;
                Width = 100;
                Height = 100;
            }

            protected override void OnClosed(System.EventArgs e)
            {
                FueCerrada = true;
                base.OnClosed(e);
            }
        }
    }
}
