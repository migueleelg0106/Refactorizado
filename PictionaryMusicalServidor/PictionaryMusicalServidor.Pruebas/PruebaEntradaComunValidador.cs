using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PictionaryMusicalServidor.Pruebas
{
    [TestClass]
    public class PruebaEntradaComunValidador
    {
        [TestMethod]
        public void Prueba_NormalizarTexto_DeberiaRetornarCadenaVaciaParaNull()
        {
            string resultado = Servicios.Servicios.EntradaComunValidador.NormalizarTexto(null);

            Assert.AreEqual(string.Empty, resultado);
        }

        [TestMethod]
        public void Prueba_NormalizarTexto_DeberiaRetornarCadenaVaciaParaEspaciosEnBlanco()
        {
            string resultado = Servicios.Servicios.EntradaComunValidador.NormalizarTexto("   ");

            Assert.AreEqual(string.Empty, resultado);
        }

        [TestMethod]
        public void Prueba_NormalizarTexto_DeberiaEliminarEspaciosAlInicioYFinal()
        {
            string resultado = Servicios.Servicios.EntradaComunValidador.NormalizarTexto("  texto  ");

            Assert.AreEqual("texto", resultado);
        }

        [TestMethod]
        public void Prueba_EsTokenValido_DeberiaRetornarFalseParaCadenaVacia()
        {
            bool resultado = Servicios.Servicios.EntradaComunValidador.EsTokenValido("");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsTokenValido_DeberiaRetornarFalseParaNull()
        {
            bool resultado = Servicios.Servicios.EntradaComunValidador.EsTokenValido(null);

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsCodigoVerificacionValido_DeberiaRetornarFalseParaCadenaVacia()
        {
            bool resultado = Servicios.Servicios.EntradaComunValidador.EsCodigoVerificacionValido("");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsCodigoVerificacionValido_DeberiaRetornarTrueParaCodigoValido()
        {
            bool resultado = Servicios.Servicios.EntradaComunValidador.EsCodigoVerificacionValido("123456");

            Assert.IsTrue(resultado);
        }
    }
}
