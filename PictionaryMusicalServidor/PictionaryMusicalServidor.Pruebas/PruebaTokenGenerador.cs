using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictionaryMusicalServidor.Pruebas
{
    [TestClass]
    public class PruebaTokenGenerador
    {
        [TestMethod]
        public void Prueba_GenerarToken_DeberiaGenerarTokenNoVacio()
        {
            string token = TokenGenerador.GenerarToken();

            Assert.IsNotNull(token);
            Assert.IsFalse(string.IsNullOrWhiteSpace(token));
        }

        [TestMethod]
        public void Prueba_GenerarToken_DeberiaGenerarTokenConLongitudCorrecta()
        {
            string token = TokenGenerador.GenerarToken();

            Assert.AreEqual(32, token.Length);
        }

        [TestMethod]
        public void Prueba_GenerarToken_DeberiaGenerarTokenHexadecimal()
        {
            string token = TokenGenerador.GenerarToken();

            Assert.IsTrue(token.All(c => char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')),
                "El token debería contener solo caracteres hexadecimales");
        }

        [TestMethod]
        public void Prueba_GenerarToken_NoDeberiaContenerGuiones()
        {
            string token = TokenGenerador.GenerarToken();

            Assert.IsFalse(token.Contains("-"), "El token no debería contener guiones");
        }

        [TestMethod]
        public void Prueba_GenerarToken_DeberiaGenerarTokensUnicos()
        {
            var tokens = new HashSet<string>();

            for (int i = 0; i < 100; i++)
            {
                string token = TokenGenerador.GenerarToken();
                tokens.Add(token);
            }

            Assert.AreEqual(100, tokens.Count, "Todos los tokens deberían ser únicos");
        }

        [TestMethod]
        public void Prueba_GenerarToken_DeberiaSerValidoComoGuid()
        {
            string token = TokenGenerador.GenerarToken();

            // Verificar que se puede reconstruir como GUID
            bool esGuidValido = Guid.TryParseExact(token, "N", out Guid guid);
            Assert.IsTrue(esGuidValido, "El token debería ser un formato GUID válido");
            Assert.AreNotEqual(Guid.Empty, guid, "El token no debería ser un GUID vacío");
        }
    }
}
