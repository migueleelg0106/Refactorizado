using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System.Text.RegularExpressions;

namespace PictionaryMusicalServidor.Pruebas.PruebasUtilidades
{
    [TestClass]
    public class PruebaTokenGenerador
    {
        [TestMethod]
        public void Prueba_GenerarToken_DeberiaRetornarTokenValido()
        {
            string token = TokenGenerador.GenerarToken();

            Assert.IsNotNull(token);
            Assert.AreEqual(32, token.Length);
        }

        [TestMethod]
        public void Prueba_GenerarToken_DeberiaRetornarTokenHexadecimal()
        {
            string token = TokenGenerador.GenerarToken();

            Regex hexRegex = new Regex(@"^[a-fA-F0-9]{32}$");
            Assert.IsTrue(hexRegex.IsMatch(token), "El token debe contener solo caracteres hexadecimales");
        }

        [TestMethod]
        public void Prueba_GenerarToken_DeberiaGenerarTokensUnicos()
        {
            string token1 = TokenGenerador.GenerarToken();
            string token2 = TokenGenerador.GenerarToken();

            Assert.AreNotEqual(token1, token2, "Los tokens generados deben ser únicos");
        }

        [TestMethod]
        public void Prueba_GenerarToken_MultiplesLlamadas_DeberianGenerarTokensDiferentes()
        {
            var tokens = new System.Collections.Generic.HashSet<string>();
            
            for (int i = 0; i < 100; i++)
            {
                string token = TokenGenerador.GenerarToken();
                Assert.IsTrue(tokens.Add(token), $"Se encontró un token duplicado: {token}");
            }

            Assert.AreEqual(100, tokens.Count);
        }
    }
}
