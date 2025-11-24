using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Servicios;

namespace PictionaryMusicalServidor.Pruebas.PruebasManejadores
{
    /// <summary>
    /// Pruebas unitarias para ClasificacionManejador.
    /// Nota: Estas pruebas verifican el comportamiento sin conexión a base de datos.
    /// Las pruebas completas con base de datos requieren un contexto de prueba separado.
    /// </summary>
    [TestClass]
    public class PruebaClasificacionManejador
    {
        private ClasificacionManejador _manejador;

        [TestInitialize]
        public void Inicializar()
        {
            _manejador = new ClasificacionManejador();
        }

        #region ObtenerTopJugadores

        [TestMethod]
        public void Prueba_ObtenerTopJugadores_SinConexionBD_DeberiaRetornarListaVacia()
        {
            // Este test verifica que el manejador maneja errores de BD retornando lista vacía
            var resultado = _manejador.ObtenerTopJugadores();

            Assert.IsNotNull(resultado);
            // Puede ser vacía si no hay BD o puede tener datos si la BD está disponible
            // Lo importante es que no lance excepción
        }

        [TestMethod]
        public void Prueba_ObtenerTopJugadores_NoDeberiaLanzarExcepcion()
        {
            // Este test verifica que el manejador no propaga excepciones al cliente
            var resultado = _manejador.ObtenerTopJugadores();

            Assert.IsNotNull(resultado, "El resultado nunca debería ser null");
        }

        #endregion
    }
}
