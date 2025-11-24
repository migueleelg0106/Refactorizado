using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictionaryMusicalServidor.Pruebas
{
    [TestClass]
    public class PruebaCodigoVerificacionGenerador
    {
        [TestMethod]
        public void Prueba_GenerarCodigo_DeberiaGenerarCodigoConLongitudPredeterminada()
        {
            string codigo = CodigoVerificacionGenerador.GenerarCodigo();

            Assert.IsNotNull(codigo);
            Assert.AreEqual(6, codigo.Length);
            Assert.IsTrue(codigo.All(char.IsDigit));
        }

        [TestMethod]
        public void Prueba_GenerarCodigo_DeberiaGenerarCodigoConLongitudPersonalizada()
        {
            string codigo = CodigoVerificacionGenerador.GenerarCodigo(8);

            Assert.IsNotNull(codigo);
            Assert.AreEqual(8, codigo.Length);
            Assert.IsTrue(codigo.All(char.IsDigit));
        }

        [TestMethod]
        public void Prueba_GenerarCodigo_DeberiaGenerarCodigoConLongitudMinima()
        {
            string codigo = CodigoVerificacionGenerador.GenerarCodigo(1);

            Assert.IsNotNull(codigo);
            Assert.AreEqual(1, codigo.Length);
            Assert.IsTrue(codigo.All(char.IsDigit));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Prueba_GenerarCodigo_DeberiaLanzarExcepcionConLongitudCero()
        {
            CodigoVerificacionGenerador.GenerarCodigo(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Prueba_GenerarCodigo_DeberiaLanzarExcepcionConLongitudNegativa()
        {
            CodigoVerificacionGenerador.GenerarCodigo(-1);
        }

        [TestMethod]
        public void Prueba_GenerarCodigo_DeberiaGenerarCodigosSinCerosIniciales()
        {
            for (int i = 0; i < 10; i++)
            {
                string codigo = CodigoVerificacionGenerador.GenerarCodigo(6);
                Assert.IsFalse(codigo.StartsWith("0"), "El código no debería comenzar con cero");
            }
        }

        [TestMethod]
        public void Prueba_GenerarCodigo_DeberiaGenerarCodigosDiferentes()
        {
            const int TamanoMuestra = 100;
            var codigos = new HashSet<string>();
            
            for (int i = 0; i < TamanoMuestra; i++)
            {
                string codigo = CodigoVerificacionGenerador.GenerarCodigo();
                codigos.Add(codigo);
            }

            Assert.IsTrue(codigos.Count > 90, "Debería generar códigos mayormente únicos");
        }
    }
}
