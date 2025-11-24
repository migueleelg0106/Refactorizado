using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios;
using System;

namespace PictionaryMusicalServidor.Pruebas.PruebasManejadores
{
    /// <summary>
    /// Pruebas unitarias para CuentaManejador.
    /// Nota: Estas pruebas se enfocan en validación de entrada y manejo de errores.
    /// Las pruebas completas de registro con base de datos requieren un contexto de prueba separado.
    /// </summary>
    [TestClass]
    public class PruebaCuentaManejador
    {
        private CuentaManejador _manejador;

        [TestInitialize]
        public void Inicializar()
        {
            _manejador = new CuentaManejador();
        }

        #region RegistrarCuenta

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_RegistrarCuenta_CuentaNula_DeberiaLanzarExcepcion()
        {
            _manejador.RegistrarCuenta(null);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_UsuarioVacio_DeberiaRetornarError()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "",
                Nombre = "Nombre",
                Apellido = "Apellido",
                Correo = "test@example.com",
                Contrasena = "Pass123!"
            };

            var resultado = _manejador.RegistrarCuenta(cuenta);

            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_NombreVacio_DeberiaRetornarError()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "usuario",
                Nombre = "",
                Apellido = "Apellido",
                Correo = "test@example.com",
                Contrasena = "Pass123!"
            };

            var resultado = _manejador.RegistrarCuenta(cuenta);

            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_ApellidoVacio_DeberiaRetornarError()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "usuario",
                Nombre = "Nombre",
                Apellido = "",
                Correo = "test@example.com",
                Contrasena = "Pass123!"
            };

            var resultado = _manejador.RegistrarCuenta(cuenta);

            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_CorreoInvalido_DeberiaRetornarError()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "usuario",
                Nombre = "Nombre",
                Apellido = "Apellido",
                Correo = "correoInvalido",
                Contrasena = "Pass123!"
            };

            var resultado = _manejador.RegistrarCuenta(cuenta);

            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_ContrasenaDebil_DeberiaRetornarError()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "usuario",
                Nombre = "Nombre",
                Apellido = "Apellido",
                Correo = "test@example.com",
                Contrasena = "debil"
            };

            var resultado = _manejador.RegistrarCuenta(cuenta);

            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_UsuarioMuyLargo_DeberiaRetornarError()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = new string('a', 51),
                Nombre = "Nombre",
                Apellido = "Apellido",
                Correo = "test@example.com",
                Contrasena = "Pass123!"
            };

            var resultado = _manejador.RegistrarCuenta(cuenta);

            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_NombreMuyLargo_DeberiaRetornarError()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "usuario",
                Nombre = new string('a', 51),
                Apellido = "Apellido",
                Correo = "test@example.com",
                Contrasena = "Pass123!"
            };

            var resultado = _manejador.RegistrarCuenta(cuenta);

            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_ApellidoMuyLargo_DeberiaRetornarError()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "usuario",
                Nombre = "Nombre",
                Apellido = new string('a', 51),
                Correo = "test@example.com",
                Contrasena = "Pass123!"
            };

            var resultado = _manejador.RegistrarCuenta(cuenta);

            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_ContrasenaMuyLarga_DeberiaRetornarError()
        {
            // Máximo es 15 caracteres, esta tiene 20
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "usuario",
                Nombre = "Nombre",
                Apellido = "Apellido",
                Correo = "test@example.com",
                Contrasena = "Password123!TooLongX"
            };

            var resultado = _manejador.RegistrarCuenta(cuenta);

            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        #endregion
    }
}
