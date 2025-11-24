using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;
using System.Linq;

namespace PictionaryMusicalServidor.Pruebas.PruebasUtilidades
{
    [TestClass]
    public class PruebaCodigoVerificacionGenerador
    {
        [TestMethod]
        public void Prueba_GenerarCodigo_LongitudPorDefecto_DeberiaRetornar6Digitos()
        {
            string codigo = CodigoVerificacionGenerador.GenerarCodigo();

            Assert.IsNotNull(codigo);
            Assert.AreEqual(6, codigo.Length);
        }

        [TestMethod]
        public void Prueba_GenerarCodigo_DeberiaContenerSoloNumeros()
        {
            string codigo = CodigoVerificacionGenerador.GenerarCodigo();

            Assert.IsTrue(codigo.All(char.IsDigit), "El código debe contener solo dígitos");
        }

        [TestMethod]
        public void Prueba_GenerarCodigo_ConLongitudEspecifica_DeberiaRetornarLongitudCorrecta()
        {
            string codigo = CodigoVerificacionGenerador.GenerarCodigo(8);

            Assert.AreEqual(8, codigo.Length);
            Assert.IsTrue(codigo.All(char.IsDigit));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Prueba_GenerarCodigo_LongitudCero_DeberiaLanzarExcepcion()
        {
            CodigoVerificacionGenerador.GenerarCodigo(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Prueba_GenerarCodigo_LongitudNegativa_DeberiaLanzarExcepcion()
        {
            CodigoVerificacionGenerador.GenerarCodigo(-1);
        }

        [TestMethod]
        public void Prueba_GenerarCodigo_NoDeberiaEmpezarConCero()
        {
            for (int i = 0; i < 50; i++)
            {
                string codigo = CodigoVerificacionGenerador.GenerarCodigo(6);
                Assert.AreNotEqual('0', codigo[0], "El código no debe comenzar con cero");
            }
        }

        [TestMethod]
        public void Prueba_GenerarCodigo_DeberiaGenerarCodigosDiferentes()
        {
            string codigo1 = CodigoVerificacionGenerador.GenerarCodigo();
            string codigo2 = CodigoVerificacionGenerador.GenerarCodigo();
            string codigo3 = CodigoVerificacionGenerador.GenerarCodigo();

            // Puede que por coincidencia sean iguales, pero es muy improbable que los tres sean iguales
            bool todosIguales = (codigo1 == codigo2) && (codigo2 == codigo3);
            Assert.IsFalse(todosIguales, "Los códigos generados deben ser diferentes (al menos en su mayoría)");
        }

        [TestMethod]
        public void Prueba_GenerarCodigo_DeberiaEstarEnRangoValido()
        {
            for (int i = 0; i < 20; i++)
            {
                string codigo = CodigoVerificacionGenerador.GenerarCodigo(6);
                int numero = int.Parse(codigo);

                Assert.IsTrue(numero >= 100000 && numero <= 999999, 
                    $"El código {codigo} debe estar entre 100000 y 999999");
            }
        }
    }
}
