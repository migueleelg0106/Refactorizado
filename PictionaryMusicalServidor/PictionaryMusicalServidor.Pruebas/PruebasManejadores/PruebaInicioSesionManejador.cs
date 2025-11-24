using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios;
using System;

namespace PictionaryMusicalServidor.Pruebas.PruebasManejadores
{
    /// <summary>
    /// Pruebas unitarias para InicioSesionManejador.
    /// Nota: Estas pruebas se enfocan en validación de entrada y manejo de errores.
    /// Las pruebas completas de autenticación con base de datos requieren un contexto de prueba separado.
    /// </summary>
    [TestClass]
    public class PruebaInicioSesionManejador
    {
        private InicioSesionManejador _manejador;

        [TestInitialize]
        public void Inicializar()
        {
            _manejador = new InicioSesionManejador();
        }

        #region IniciarSesion - Validación de Entrada

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_IniciarSesion_CredencialesNulas_DeberiaLanzarExcepcion()
        {
            _manejador.IniciarSesion(null);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_IdentificadorVacio_DeberiaRetornarError()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "",
                Contrasena = "Pass123!"
            };

            var resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_IdentificadorNulo_DeberiaRetornarError()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = null,
                Contrasena = "Pass123!"
            };

            var resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_ContrasenaNula_DeberiaRetornarError()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario",
                Contrasena = null
            };

            var resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_ContrasenaVacia_DeberiaRetornarError()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario",
                Contrasena = ""
            };

            var resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_IdentificadorSoloEspacios_DeberiaRetornarError()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "   ",
                Contrasena = "Pass123!"
            };

            var resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_ContrasenaSoloEspacios_DeberiaRetornarError()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario",
                Contrasena = "   "
            };

            var resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_IdentificadorMuyLargo_DeberiaRetornarError()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = new string('a', 51),
                Contrasena = "Pass123!"
            };

            var resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        #endregion
    }
}
