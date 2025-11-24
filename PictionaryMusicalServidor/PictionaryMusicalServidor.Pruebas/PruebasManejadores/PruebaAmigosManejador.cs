using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Servicios;
using System.ServiceModel;

namespace PictionaryMusicalServidor.Pruebas.PruebasManejadores
{
    /// <summary>
    /// Pruebas unitarias para AmigosManejador.
    /// Nota: Estas pruebas se enfocan en validación de entrada y manejo de errores.
    /// Las pruebas completas con base de datos y callbacks requieren un contexto de prueba separado.
    /// </summary>
    [TestClass]
    public class PruebaAmigosManejador
    {
        private AmigosManejador _manejador;

        [TestInitialize]
        public void Inicializar()
        {
            _manejador = new AmigosManejador();
        }

        #region Suscribir

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Suscribir_NombreUsuarioNulo_DeberiaLanzarExcepcion()
        {
            _manejador.Suscribir(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Suscribir_NombreUsuarioVacio_DeberiaLanzarExcepcion()
        {
            _manejador.Suscribir("");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Suscribir_NombreUsuarioSoloEspacios_DeberiaLanzarExcepcion()
        {
            _manejador.Suscribir("   ");
        }

        #endregion

        #region CancelarSuscripcion

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_CancelarSuscripcion_NombreUsuarioNulo_DeberiaLanzarExcepcion()
        {
            _manejador.CancelarSuscripcion(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_CancelarSuscripcion_NombreUsuarioVacio_DeberiaLanzarExcepcion()
        {
            _manejador.CancelarSuscripcion("");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_CancelarSuscripcion_NombreUsuarioSoloEspacios_DeberiaLanzarExcepcion()
        {
            _manejador.CancelarSuscripcion("   ");
        }

        #endregion
    }
}
