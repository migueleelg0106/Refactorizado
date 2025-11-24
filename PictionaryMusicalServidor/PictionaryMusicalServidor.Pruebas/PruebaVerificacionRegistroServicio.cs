using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Servicios;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using System;

namespace PictionaryMusicalServidor.Pruebas
{
    /// <summary>
    /// Pruebas unitarias para VerificacionRegistroServicio.
    /// Valida validación de correos, contraseñas y datos de registro.
    /// </summary>
    [TestClass]
    public class PruebaVerificacionRegistroServicio
    {
        #region Pruebas de Validación de Email

        [TestMethod]
        public void Prueba_ValidarCorreo_EmailSimpleValido_DeberiaRetornarTrue()
        {
            // Arrange
            string email = "usuario@dominio.com";

            // Act
            bool resultado = EntradaComunValidador.EsCorreoValido(email);

            // Assert
            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarCorreo_EmailComplejoConPuntoYGuionValido_DeberiaRetornarTrue()
        {
            // Arrange
            string email = "nombre.apellido-123@sub.dominio.com";

            // Act
            bool resultado = EntradaComunValidador.EsCorreoValido(email);

            // Assert
            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarCorreo_EmailConMasYDominioCompuesto_DeberiaRetornarTrue()
        {
            // Arrange
            string email = "usuario+tag@dominio.co.uk";

            // Act
            bool resultado = EntradaComunValidador.EsCorreoValido(email);

            // Assert
            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarCorreo_EmailSinArroba_DeberiaRetornarFalse()
        {
            // Arrange
            string email = "usuario.dominio.com";

            // Act
            bool resultado = EntradaComunValidador.EsCorreoValido(email);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarCorreo_EmailSinDominio_DeberiaRetornarFalse()
        {
            // Arrange
            string email = "usuario@";

            // Act
            bool resultado = EntradaComunValidador.EsCorreoValido(email);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarCorreo_EmailSinNombreUsuario_DeberiaRetornarFalse()
        {
            // Arrange
            string email = "@dominio.com";

            // Act
            bool resultado = EntradaComunValidador.EsCorreoValido(email);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarCorreo_EmailConEspacios_DeberiaRetornarFalse()
        {
            // Arrange
            string email = "usuario @dominio.com";

            // Act
            bool resultado = EntradaComunValidador.EsCorreoValido(email);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarCorreo_EmailNulo_DeberiaRetornarFalse()
        {
            // Arrange
            string email = null;

            // Act
            bool resultado = EntradaComunValidador.EsCorreoValido(email);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarCorreo_EmailVacio_DeberiaRetornarFalse()
        {
            // Arrange
            string email = "";

            // Act
            bool resultado = EntradaComunValidador.EsCorreoValido(email);

            // Assert
            Assert.IsFalse(resultado);
        }

        #endregion

        #region Pruebas de Validación de Contraseña

        [TestMethod]
        public void Prueba_ValidarContrasena_ContrasenaValida_DeberiaRetornarTrue()
        {
            // Arrange
            string contrasena = "Password1!";

            // Act
            bool resultado = EntradaComunValidador.EsContrasenaValida(contrasena);

            // Assert
            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarContrasena_ContrasenaValidaMaximaLongitud_DeberiaRetornarTrue()
        {
            // Arrange
            string contrasena = "Pass1234567890!"; // 15 caracteres

            // Act
            bool resultado = EntradaComunValidador.EsContrasenaValida(contrasena);

            // Assert
            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarContrasena_ContrasenaMuyCorta_DeberiaRetornarFalse()
        {
            // Arrange
            string contrasena = "Pass1!"; // Menos de 8 caracteres

            // Act
            bool resultado = EntradaComunValidador.EsContrasenaValida(contrasena);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarContrasena_ContrasenaMuyLarga_DeberiaRetornarFalse()
        {
            // Arrange
            string contrasena = "Password12345678!"; // Más de 15 caracteres

            // Act
            bool resultado = EntradaComunValidador.EsContrasenaValida(contrasena);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarContrasena_ContrasenaSinMayuscula_DeberiaRetornarFalse()
        {
            // Arrange
            string contrasena = "password1!";

            // Act
            bool resultado = EntradaComunValidador.EsContrasenaValida(contrasena);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarContrasena_ContrasenaSinNumero_DeberiaRetornarFalse()
        {
            // Arrange
            string contrasena = "Password!";

            // Act
            bool resultado = EntradaComunValidador.EsContrasenaValida(contrasena);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarContrasena_ContrasenaSinCaracterEspecial_DeberiaRetornarFalse()
        {
            // Arrange
            string contrasena = "Password1";

            // Act
            bool resultado = EntradaComunValidador.EsContrasenaValida(contrasena);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarContrasena_ContrasenaNula_DeberiaRetornarFalse()
        {
            // Arrange
            string contrasena = null;

            // Act
            bool resultado = EntradaComunValidador.EsContrasenaValida(contrasena);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarContrasena_ContrasenaVacia_DeberiaRetornarFalse()
        {
            // Arrange
            string contrasena = "";

            // Act
            bool resultado = EntradaComunValidador.EsContrasenaValida(contrasena);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarContrasena_ContrasenaSoloEspacios_DeberiaRetornarFalse()
        {
            // Arrange
            string contrasena = "   ";

            // Act
            bool resultado = EntradaComunValidador.EsContrasenaValida(contrasena);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarContrasena_ContrasenaDebil_DeberiaRetornarFalse()
        {
            // Arrange - Solo tiene letras minúsculas y es corta
            string contrasena = "pass";

            // Act
            bool resultado = EntradaComunValidador.EsContrasenaValida(contrasena);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarContrasena_ContrasenaConCaracteresEspecialesVariados_DeberiaRetornarTrue()
        {
            // Arrange
            string contrasena = "Pass@1234";

            // Act
            bool resultado = EntradaComunValidador.EsContrasenaValida(contrasena);

            // Assert
            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarContrasena_ContrasenaConGuion_DeberiaRetornarTrue()
        {
            // Arrange
            string contrasena = "Pass-word1";

            // Act
            bool resultado = EntradaComunValidador.EsContrasenaValida(contrasena);

            // Assert
            Assert.IsTrue(resultado);
        }

        #endregion

        #region Pruebas de Validación de Nueva Cuenta

        [TestMethod]
        public void Prueba_ValidarNuevaCuenta_CuentaNula_DeberiaRetornarOperacionFallida()
        {
            // Arrange
            NuevaCuentaDTO cuenta = null;

            // Act
            var resultado = EntradaComunValidador.ValidarNuevaCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ValidarNuevaCuenta_UsuarioNulo_DeberiaRetornarOperacionFallida()
        {
            // Arrange
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = null,
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "juan@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = EntradaComunValidador.ValidarNuevaCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ValidarNuevaCuenta_NombreNulo_DeberiaRetornarOperacionFallida()
        {
            // Arrange
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "juanperez",
                Nombre = null,
                Apellido = "Perez",
                Correo = "juan@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = EntradaComunValidador.ValidarNuevaCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ValidarNuevaCuenta_ApellidoNulo_DeberiaRetornarOperacionFallida()
        {
            // Arrange
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "juanperez",
                Nombre = "Juan",
                Apellido = null,
                Correo = "juan@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = EntradaComunValidador.ValidarNuevaCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ValidarNuevaCuenta_CorreoInvalido_DeberiaRetornarOperacionFallida()
        {
            // Arrange
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "juanperez",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "juan.example.com", // Sin @
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = EntradaComunValidador.ValidarNuevaCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ValidarNuevaCuenta_ContrasenaInvalida_DeberiaRetornarOperacionFallida()
        {
            // Arrange
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "juanperez",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "juan@example.com",
                Contrasena = "pass", // Demasiado corta
                AvatarId = 1
            };

            // Act
            var resultado = EntradaComunValidador.ValidarNuevaCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ValidarNuevaCuenta_CuentaValida_DeberiaRetornarOperacionExitosa()
        {
            // Arrange
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "juanperez",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "juan@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = EntradaComunValidador.ValidarNuevaCuenta(cuenta);

            // Assert
            Assert.IsTrue(resultado.OperacionExitosa);
            Assert.IsNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ValidarNuevaCuenta_CamposConEspaciosExtra_DebeNormalizar()
        {
            // Arrange
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "  juanperez  ",
                Nombre = "  Juan  ",
                Apellido = "  Perez  ",
                Correo = "  juan@example.com  ",
                Contrasena = "  Password1!  ",
                AvatarId = 1
            };

            // Act
            var resultado = EntradaComunValidador.ValidarNuevaCuenta(cuenta);

            // Assert
            Assert.IsTrue(resultado.OperacionExitosa);
            Assert.AreEqual("juanperez", cuenta.Usuario);
            Assert.AreEqual("Juan", cuenta.Nombre);
            Assert.AreEqual("Perez", cuenta.Apellido);
            Assert.AreEqual("juan@example.com", cuenta.Correo);
            Assert.AreEqual("Password1!", cuenta.Contrasena);
        }

        #endregion

        #region Pruebas de Validación de Token

        [TestMethod]
        public void Prueba_ValidarToken_TokenValido_DeberiaRetornarTrue()
        {
            // Arrange
            string token = "a1b2c3d4e5f6789012345678901234ab";

            // Act
            bool resultado = EntradaComunValidador.EsTokenValido(token);

            // Assert
            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarToken_TokenInvalidoLongitudIncorrecta_DeberiaRetornarFalse()
        {
            // Arrange
            string token = "a1b2c3d4";

            // Act
            bool resultado = EntradaComunValidador.EsTokenValido(token);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarToken_TokenConCaracteresInvalidos_DeberiaRetornarFalse()
        {
            // Arrange
            string token = "g1h2i3j4k5l6m7n8o9p0q1r2s3t4u5v6"; // Contiene caracteres no hexadecimales

            // Act
            bool resultado = EntradaComunValidador.EsTokenValido(token);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarToken_TokenNulo_DeberiaRetornarFalse()
        {
            // Arrange
            string token = null;

            // Act
            bool resultado = EntradaComunValidador.EsTokenValido(token);

            // Assert
            Assert.IsFalse(resultado);
        }

        #endregion

        #region Pruebas de Validación de Código de Verificación

        [TestMethod]
        public void Prueba_ValidarCodigoVerificacion_CodigoValido_DeberiaRetornarTrue()
        {
            // Arrange
            string codigo = "123456";

            // Act
            bool resultado = EntradaComunValidador.EsCodigoVerificacionValido(codigo);

            // Assert
            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarCodigoVerificacion_CodigoMuyCorto_DeberiaRetornarFalse()
        {
            // Arrange
            string codigo = "12345";

            // Act
            bool resultado = EntradaComunValidador.EsCodigoVerificacionValido(codigo);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarCodigoVerificacion_CodigoMuyLargo_DeberiaRetornarFalse()
        {
            // Arrange
            string codigo = "1234567";

            // Act
            bool resultado = EntradaComunValidador.EsCodigoVerificacionValido(codigo);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarCodigoVerificacion_CodigoConLetras_DeberiaRetornarFalse()
        {
            // Arrange
            string codigo = "12A456";

            // Act
            bool resultado = EntradaComunValidador.EsCodigoVerificacionValido(codigo);

            // Assert
            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_ValidarCodigoVerificacion_CodigoNulo_DeberiaRetornarFalse()
        {
            // Arrange
            string codigo = null;

            // Act
            bool resultado = EntradaComunValidador.EsCodigoVerificacionValido(codigo);

            // Assert
            Assert.IsFalse(resultado);
        }

        #endregion
    }
}
