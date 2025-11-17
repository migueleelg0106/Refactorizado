using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.VistaModelo.Sesion;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using System.Windows;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Sesion
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
            bool navegacionInvocada = false;

            SesionUsuarioActual.EstablecerUsuario(new UsuarioDTO
            {
                UsuarioId = 1,
                NombreUsuario = "UsuarioPrueba"
            });

            var vistaModelo = new TerminacionSesionVistaModelo();

            vistaModelo.OcultarDialogo = () => dialogoOculto = true;
            vistaModelo.EjecutarCierreSesionYNavegacion = () => navegacionInvocada = true;

            vistaModelo.AceptarComando.Execute(null);

            Assert.IsNull(UsuarioAutenticado.Instancia.NombreUsuario, "La sesión debe ser nula después de Aceptar.");

            Assert.IsTrue(dialogoOculto, "OcultarDialogo debe ser invocado.");
            Assert.IsTrue(navegacionInvocada, "EjecutarCierreSesionYNavegacion debe ser invocado para manejar la transición de la UI.");
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
