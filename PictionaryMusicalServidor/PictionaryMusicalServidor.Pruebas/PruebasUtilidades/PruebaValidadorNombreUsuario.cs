using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System.ServiceModel;

namespace PictionaryMusicalServidor.Pruebas.PruebasUtilidades
{
    [TestClass]
    public class PruebaValidadorNombreUsuario
    {
        [TestMethod]
        public void Prueba_Validar_NombreUsuarioValido_NoDeberiaLanzarExcepcion()
        {
            ValidadorNombreUsuario.Validar("UsuarioValido", "nombreUsuario");
            // Si no lanza excepción, la prueba pasa
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Validar_NombreUsuarioNulo_DeberiaLanzarExcepcion()
        {
            ValidadorNombreUsuario.Validar(null, "nombreUsuario");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Validar_NombreUsuarioVacio_DeberiaLanzarExcepcion()
        {
            ValidadorNombreUsuario.Validar("", "nombreUsuario");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Validar_NombreUsuarioSoloEspacios_DeberiaLanzarExcepcion()
        {
            ValidadorNombreUsuario.Validar("   ", "nombreUsuario");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Validar_NombreUsuarioMuyLargo_DeberiaLanzarExcepcion()
        {
            string nombreLargo = new string('a', 51); // Más de 50 caracteres
            ValidadorNombreUsuario.Validar(nombreLargo, "nombreUsuario");
        }

        [TestMethod]
        public void Prueba_Validar_NombreUsuarioConEspacios_DeberiaValidarTrimmeado()
        {
            ValidadorNombreUsuario.Validar("  Usuario  ", "nombreUsuario");
            // Debería validar correctamente después de hacer trim
        }

        [TestMethod]
        public void Prueba_ObtenerNombreNormalizado_ConNombreBaseDatos_DeberiaRetornarBaseDatos()
        {
            string resultado = ValidadorNombreUsuario.ObtenerNombreNormalizado("NombreBD", "NombreAlterno");

            Assert.AreEqual("NombreBD", resultado);
        }

        [TestMethod]
        public void Prueba_ObtenerNombreNormalizado_SinNombreBaseDatos_DeberiaRetornarAlterno()
        {
            string resultado = ValidadorNombreUsuario.ObtenerNombreNormalizado(null, "NombreAlterno");

            Assert.AreEqual("NombreAlterno", resultado);
        }

        [TestMethod]
        public void Prueba_ObtenerNombreNormalizado_NombreBaseDatosVacio_DeberiaRetornarAlterno()
        {
            string resultado = ValidadorNombreUsuario.ObtenerNombreNormalizado("", "NombreAlterno");

            Assert.AreEqual("NombreAlterno", resultado);
        }

        [TestMethod]
        public void Prueba_ObtenerNombreNormalizado_NombreBaseDatosSoloEspacios_DeberiaRetornarAlterno()
        {
            string resultado = ValidadorNombreUsuario.ObtenerNombreNormalizado("   ", "NombreAlterno");

            Assert.AreEqual("NombreAlterno", resultado);
        }

        [TestMethod]
        public void Prueba_ObtenerNombreNormalizado_NombreBaseDatosConEspacios_DeberiaRetornarTrimmeado()
        {
            string resultado = ValidadorNombreUsuario.ObtenerNombreNormalizado("  NombreBD  ", "NombreAlterno");

            Assert.AreEqual("NombreBD", resultado);
        }

        [TestMethod]
        public void Prueba_ObtenerNombreNormalizado_AmbosNulos_DeberiaRetornarNull()
        {
            string resultado = ValidadorNombreUsuario.ObtenerNombreNormalizado(null, null);

            Assert.IsNull(resultado);
        }
    }
}
