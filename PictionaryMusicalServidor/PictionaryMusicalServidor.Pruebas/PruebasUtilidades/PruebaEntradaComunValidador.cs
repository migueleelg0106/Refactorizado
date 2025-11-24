using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios;

namespace PictionaryMusicalServidor.Pruebas.PruebasUtilidades
{
    [TestClass]
    public class PruebaEntradaComunValidador
    {
        #region NormalizarTexto

        [TestMethod]
        public void Prueba_NormalizarTexto_ConTextoValido_DeberiaRetornarTextoTrimmeado()
        {
            string resultado = EntradaComunValidador.NormalizarTexto("  Texto  ");

            Assert.AreEqual("Texto", resultado);
        }

        [TestMethod]
        public void Prueba_NormalizarTexto_ConTextoNulo_DeberiaRetornarNull()
        {
            string resultado = EntradaComunValidador.NormalizarTexto(null);

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public void Prueba_NormalizarTexto_ConTextoVacio_DeberiaRetornarNull()
        {
            string resultado = EntradaComunValidador.NormalizarTexto("");

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public void Prueba_NormalizarTexto_ConSoloEspacios_DeberiaRetornarNull()
        {
            string resultado = EntradaComunValidador.NormalizarTexto("   ");

            Assert.IsNull(resultado);
        }

        #endregion

        #region EsLongitudValida

        [TestMethod]
        public void Prueba_EsLongitudValida_TextoValido_DeberiaRetornarTrue()
        {
            bool resultado = EntradaComunValidador.EsLongitudValida("Usuario");

            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_EsLongitudValida_TextoLargo_DeberiaRetornarFalse()
        {
            string textoLargo = new string('a', 51);
            bool resultado = EntradaComunValidador.EsLongitudValida(textoLargo);

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsLongitudValida_TextoMaximo_DeberiaRetornarTrue()
        {
            string textoMaximo = new string('a', 50);
            bool resultado = EntradaComunValidador.EsLongitudValida(textoMaximo);

            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_EsLongitudValida_TextoVacio_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsLongitudValida("");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsLongitudValida_TextoNulo_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsLongitudValida(null);

            Assert.IsFalse(resultado);
        }

        #endregion

        #region EsCorreoValido

        [TestMethod]
        public void Prueba_EsCorreoValido_CorreoValido_DeberiaRetornarTrue()
        {
            bool resultado = EntradaComunValidador.EsCorreoValido("usuario@example.com");

            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_EsCorreoValido_CorreoSinArroba_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsCorreoValido("usuarioexample.com");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsCorreoValido_CorreoSinDominio_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsCorreoValido("usuario@");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsCorreoValido_CorreoSinPunto_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsCorreoValido("usuario@examplecom");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsCorreoValido_CorreoComplejo_DeberiaRetornarTrue()
        {
            bool resultado = EntradaComunValidador.EsCorreoValido("nombre.apellido+tag@dominio.co.uk");

            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_EsCorreoValido_CorreoVacio_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsCorreoValido("");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsCorreoValido_CorreoNulo_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsCorreoValido(null);

            Assert.IsFalse(resultado);
        }

        #endregion

        #region EsContrasenaValida

        [TestMethod]
        public void Prueba_EsContrasenaValida_ContrasenaValida_DeberiaRetornarTrue()
        {
            bool resultado = EntradaComunValidador.EsContrasenaValida("Pass123!");

            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_EsContrasenaValida_SinMayuscula_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsContrasenaValida("pass123!");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsContrasenaValida_SinNumero_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsContrasenaValida("Password!");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsContrasenaValida_SinCaracterEspecial_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsContrasenaValida("Password123");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsContrasenaValida_MuyCorta_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsContrasenaValida("Pa1!");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsContrasenaValida_MuyLarga_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsContrasenaValida("Password123!Extra");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsContrasenaValida_ContrasenaVacia_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsContrasenaValida("");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsContrasenaValida_ContrasenaNula_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsContrasenaValida(null);

            Assert.IsFalse(resultado);
        }

        #endregion

        #region EsTokenValido

        [TestMethod]
        public void Prueba_EsTokenValido_TokenValido_DeberiaRetornarTrue()
        {
            bool resultado = EntradaComunValidador.EsTokenValido("a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4");

            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_EsTokenValido_TokenMuyCorto_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsTokenValido("a1b2c3d4e5f6");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsTokenValido_TokenMuyLargo_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsTokenValido("a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4extra");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsTokenValido_TokenConCaracteresInvalidos_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsTokenValido("a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3g!");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsTokenValido_TokenNulo_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsTokenValido(null);

            Assert.IsFalse(resultado);
        }

        #endregion

        #region EsCodigoVerificacionValido

        [TestMethod]
        public void Prueba_EsCodigoVerificacionValido_CodigoValido_DeberiaRetornarTrue()
        {
            bool resultado = EntradaComunValidador.EsCodigoVerificacionValido("123456");

            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_EsCodigoVerificacionValido_CodigoMuyCorto_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsCodigoVerificacionValido("12345");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsCodigoVerificacionValido_CodigoMuyLargo_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsCodigoVerificacionValido("1234567");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsCodigoVerificacionValido_CodigoConLetras_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsCodigoVerificacionValido("12A456");

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_EsCodigoVerificacionValido_CodigoNulo_DeberiaRetornarFalse()
        {
            bool resultado = EntradaComunValidador.EsCodigoVerificacionValido(null);

            Assert.IsFalse(resultado);
        }

        #endregion

        #region ValidarNuevaCuenta

        [TestMethod]
        public void Prueba_ValidarNuevaCuenta_CuentaValida_DeberiaRetornarExito()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "usuario",
                Nombre = "Nombre",
                Apellido = "Apellido",
                Correo = "usuario@example.com",
                Contrasena = "Pass123!"
            };

            var resultado = EntradaComunValidador.ValidarNuevaCuenta(cuenta);

            Assert.IsTrue(resultado.OperacionExitosa);
        }

        [TestMethod]
        public void Prueba_ValidarNuevaCuenta_CuentaNula_DeberiaRetornarError()
        {
            var resultado = EntradaComunValidador.ValidarNuevaCuenta(null);

            Assert.IsFalse(resultado.OperacionExitosa);
        }

        [TestMethod]
        public void Prueba_ValidarNuevaCuenta_UsuarioInvalido_DeberiaRetornarError()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "",
                Nombre = "Nombre",
                Apellido = "Apellido",
                Correo = "usuario@example.com",
                Contrasena = "Pass123!"
            };

            var resultado = EntradaComunValidador.ValidarNuevaCuenta(cuenta);

            Assert.IsFalse(resultado.OperacionExitosa);
        }

        [TestMethod]
        public void Prueba_ValidarNuevaCuenta_CorreoInvalido_DeberiaRetornarError()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "usuario",
                Nombre = "Nombre",
                Apellido = "Apellido",
                Correo = "correoInvalido",
                Contrasena = "Pass123!"
            };

            var resultado = EntradaComunValidador.ValidarNuevaCuenta(cuenta);

            Assert.IsFalse(resultado.OperacionExitosa);
        }

        [TestMethod]
        public void Prueba_ValidarNuevaCuenta_ContrasenaInvalida_DeberiaRetornarError()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "usuario",
                Nombre = "Nombre",
                Apellido = "Apellido",
                Correo = "usuario@example.com",
                Contrasena = "debil"
            };

            var resultado = EntradaComunValidador.ValidarNuevaCuenta(cuenta);

            Assert.IsFalse(resultado.OperacionExitosa);
        }

        #endregion

        #region ValidarActualizacionPerfil

        [TestMethod]
        public void Prueba_ValidarActualizacionPerfil_PerfilValido_DeberiaRetornarExito()
        {
            var perfil = new ActualizacionPerfilDTO
            {
                UsuarioId = 1,
                Nombre = "Nombre",
                Apellido = "Apellido",
                Avatar = "avatar.png"
            };

            var resultado = EntradaComunValidador.ValidarActualizacionPerfil(perfil);

            Assert.IsTrue(resultado.OperacionExitosa);
        }

        [TestMethod]
        public void Prueba_ValidarActualizacionPerfil_PerfilNulo_DeberiaRetornarError()
        {
            var resultado = EntradaComunValidador.ValidarActualizacionPerfil(null);

            Assert.IsFalse(resultado.OperacionExitosa);
        }

        [TestMethod]
        public void Prueba_ValidarActualizacionPerfil_UsuarioIdInvalido_DeberiaRetornarError()
        {
            var perfil = new ActualizacionPerfilDTO
            {
                UsuarioId = 0,
                Nombre = "Nombre",
                Apellido = "Apellido",
                Avatar = "avatar.png"
            };

            var resultado = EntradaComunValidador.ValidarActualizacionPerfil(perfil);

            Assert.IsFalse(resultado.OperacionExitosa);
        }

        #endregion
    }
}
