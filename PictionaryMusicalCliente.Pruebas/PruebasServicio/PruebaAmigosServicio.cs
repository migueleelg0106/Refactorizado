using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasServicio
{
    /// <summary>
    /// Pruebas para AmigosServicio.
    /// Nota: Estas pruebas se enfocan en la lógica interna que puede probarse sin un servidor WCF,
    /// como validaciones, callbacks y gestión de estado.
    /// </summary>
    [TestClass]
    public class PruebaAmigosServicio
    {
        private AmigosServicio _servicio;

        [TestCleanup]
        public void Limpiar()
        {
            _servicio?.Dispose();
            _servicio = null;
        }

        #region Pruebas de Validación de Parámetros

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SuscribirAsync_NombreUsuarioVacio_LanzaExcepcion()
        {
            _servicio = new AmigosServicio();
            await _servicio.SuscribirAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SuscribirAsync_NombreUsuarioNulo_LanzaExcepcion()
        {
            _servicio = new AmigosServicio();
            await _servicio.SuscribirAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SuscribirAsync_NombreUsuarioEspacios_LanzaExcepcion()
        {
            _servicio = new AmigosServicio();
            await _servicio.SuscribirAsync("   ");
        }

        [TestMethod]
        public async Task CancelarSuscripcionAsync_NombreUsuarioVacio_NoLanzaExcepcion()
        {
            _servicio = new AmigosServicio();
            // No debería lanzar excepción
            await _servicio.CancelarSuscripcionAsync(string.Empty);
        }

        [TestMethod]
        public async Task CancelarSuscripcionAsync_NombreUsuarioNulo_NoLanzaExcepcion()
        {
            _servicio = new AmigosServicio();
            // No debería lanzar excepción
            await _servicio.CancelarSuscripcionAsync(null);
        }

        #endregion

        #region Pruebas de Propiedades y Estado Inicial

        [TestMethod]
        public void SolicitudesPendientes_IniciaVacio()
        {
            _servicio = new AmigosServicio();
            var solicitudes = _servicio.SolicitudesPendientes;

            Assert.IsNotNull(solicitudes);
            Assert.AreEqual(0, solicitudes.Count);
        }

        [TestMethod]
        public void SolicitudesPendientes_DevuelveColeccionDeSoloLectura()
        {
            _servicio = new AmigosServicio();
            var solicitudes = _servicio.SolicitudesPendientes;

            Assert.IsInstanceOfType(solicitudes, typeof(IReadOnlyCollection<DTOs.SolicitudAmistadDTO>));
        }

        #endregion

        #region Pruebas de Callbacks (Lógica sin servidor)

        [TestMethod]
        public void NotificarSolicitudActualizada_SolicitudNula_NoDisparaEvento()
        {
            _servicio = new AmigosServicio();
            bool eventoDisparado = false;
            _servicio.SolicitudesActualizadas += (s, e) => eventoDisparado = true;

            _servicio.NotificarSolicitudActualizada(null);

            Assert.IsFalse(eventoDisparado);
        }

        [TestMethod]
        public void NotificarSolicitudActualizada_EmisorVacio_NoDisparaEvento()
        {
            _servicio = new AmigosServicio();
            bool eventoDisparado = false;
            _servicio.SolicitudesActualizadas += (s, e) => eventoDisparado = true;

            var solicitud = new DTOs.SolicitudAmistadDTO
            {
                UsuarioEmisor = "",
                UsuarioReceptor = "Usuario1",
                SolicitudAceptada = false
            };

            _servicio.NotificarSolicitudActualizada(solicitud);

            Assert.IsFalse(eventoDisparado);
        }

        [TestMethod]
        public void NotificarSolicitudActualizada_ReceptorVacio_NoDisparaEvento()
        {
            _servicio = new AmigosServicio();
            bool eventoDisparado = false;
            _servicio.SolicitudesActualizadas += (s, e) => eventoDisparado = true;

            var solicitud = new DTOs.SolicitudAmistadDTO
            {
                UsuarioEmisor = "Usuario1",
                UsuarioReceptor = "",
                SolicitudAceptada = false
            };

            _servicio.NotificarSolicitudActualizada(solicitud);

            Assert.IsFalse(eventoDisparado);
        }

        [TestMethod]
        public void NotificarAmistadEliminada_SolicitudNula_NoDisparaEvento()
        {
            _servicio = new AmigosServicio();
            bool eventoDisparado = false;
            _servicio.SolicitudesActualizadas += (s, e) => eventoDisparado = true;

            _servicio.NotificarAmistadEliminada(null);

            Assert.IsFalse(eventoDisparado);
        }

        #endregion

        #region Pruebas de Dispose

        [TestMethod]
        public void Dispose_NoLanzaExcepcion()
        {
            _servicio = new AmigosServicio();
            _servicio.Dispose();
            
            // Verificar que todavía se puede acceder a propiedades
            var solicitudes = _servicio.SolicitudesPendientes;
            Assert.IsNotNull(solicitudes);
        }

        [TestMethod]
        public void Dispose_MultiplesLlamadas_NoGeneraError()
        {
            _servicio = new AmigosServicio();
            _servicio.Dispose();
            _servicio.Dispose();
            // No debería lanzar excepción
        }

        #endregion
    }
}
