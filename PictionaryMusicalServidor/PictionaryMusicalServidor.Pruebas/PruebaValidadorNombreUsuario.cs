using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System.ServiceModel;

namespace PictionaryMusicalServidor.Pruebas
{
    [TestClass]
    public class PruebaValidadorNombreUsuario
    {
        [TestMethod]
        public void Prueba_Validar_NoDeberiaLanzarExcepcionConNombreValido()
        {
            ValidadorNombreUsuario.Validar("UsuarioValido", "nombreUsuario");
        }

        [TestMethod]
        public void Prueba_Validar_NoDeberiaLanzarExcepcionConNombreConEspacios()
        {
            ValidadorNombreUsuario.Validar("  Usuario Valido  ", "nombreUsuario");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Validar_DeberiaLanzarExcepcionConNombreNulo()
        {
            ValidadorNombreUsuario.Validar(null, "nombreUsuario");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Validar_DeberiaLanzarExcepcionConNombreVacio()
        {
            ValidadorNombreUsuario.Validar("", "nombreUsuario");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Validar_DeberiaLanzarExcepcionConNombreSoloEspacios()
        {
            ValidadorNombreUsuario.Validar("   ", "nombreUsuario");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Validar_DeberiaLanzarExcepcionConNombreMuyLargo()
        {
            string nombreLargo = new string('a', 201);
            ValidadorNombreUsuario.Validar(nombreLargo, "nombreUsuario");
        }

        [TestMethod]
        public void Prueba_ObtenerNombreNormalizado_DeberiaRetornarNombreBaseDatos()
        {
            string resultado = ValidadorNombreUsuario.ObtenerNombreNormalizado("NombreBD", "NombreAlterno");

            Assert.AreEqual("NombreBD", resultado);
        }

        [TestMethod]
        public void Prueba_ObtenerNombreNormalizado_DeberiaRetornarNombreBaseDatosSinEspacios()
        {
            string resultado = ValidadorNombreUsuario.ObtenerNombreNormalizado("  NombreBD  ", "NombreAlterno");

            Assert.AreEqual("NombreBD", resultado);
        }

        [TestMethod]
        public void Prueba_ObtenerNombreNormalizado_DeberiaRetornarAlternoSiBaseDatosEsNulo()
        {
            string resultado = ValidadorNombreUsuario.ObtenerNombreNormalizado(null, "NombreAlterno");

            Assert.AreEqual("NombreAlterno", resultado);
        }

        [TestMethod]
        public void Prueba_ObtenerNombreNormalizado_DeberiaRetornarAlternoSiBaseDatosEsVacio()
        {
            string resultado = ValidadorNombreUsuario.ObtenerNombreNormalizado("", "NombreAlterno");

            Assert.AreEqual("NombreAlterno", resultado);
        }

        [TestMethod]
        public void Prueba_ObtenerNombreNormalizado_DeberiaRetornarAlternoSiBaseDatosEsSoloEspacios()
        {
            string resultado = ValidadorNombreUsuario.ObtenerNombreNormalizado("   ", "NombreAlterno");

            Assert.AreEqual("NombreAlterno", resultado);
        }

        [TestMethod]
        public void Prueba_ObtenerNombreNormalizado_DeberiaRetornarAlternoSinEspacios()
        {
            string resultado = ValidadorNombreUsuario.ObtenerNombreNormalizado(null, "  NombreAlterno  ");

            Assert.AreEqual("NombreAlterno", resultado);
        }

        [TestMethod]
        public void Prueba_ObtenerNombreNormalizado_DeberiaRetornarNuloSiAmbosNulos()
        {
            string resultado = ValidadorNombreUsuario.ObtenerNombreNormalizado(null, null);

            Assert.IsNull(resultado);
        }
    }
}
