using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios;
using System;

namespace PictionaryMusicalServidor.Pruebas.PruebasManejadores
{
    /// <summary>
    /// Pruebas unitarias para PerfilManejador.
    /// Nota: Estas pruebas se enfocan en validación de entrada y manejo de errores.
    /// Las pruebas completas con base de datos requieren un contexto de prueba separado.
    /// </summary>
    [TestClass]
    public class PruebaPerfilManejador
    {
        private PerfilManejador _manejador;

        [TestInitialize]
        public void Inicializar()
        {
            _manejador = new PerfilManejador();
        }

        #region ObtenerPerfil

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Prueba_ObtenerPerfil_UsuarioIdCero_DeberiaLanzarExcepcion()
        {
            _manejador.ObtenerPerfil(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Prueba_ObtenerPerfil_UsuarioIdNegativo_DeberiaLanzarExcepcion()
        {
            _manejador.ObtenerPerfil(-1);
        }

        #endregion

        #region ActualizarPerfil

        [TestMethod]
        public void Prueba_ActualizarPerfil_PerfilNulo_DeberiaRetornarError()
        {
            var resultado = _manejador.ActualizarPerfil(null);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ActualizarPerfil_UsuarioIdCero_DeberiaRetornarError()
        {
            var perfil = new ActualizacionPerfilDTO
            {
                UsuarioId = 0,
                Nombre = "Nombre",
                Apellido = "Apellido",
                Avatar = "avatar.png"
            };

            var resultado = _manejador.ActualizarPerfil(perfil);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ActualizarPerfil_UsuarioIdNegativo_DeberiaRetornarError()
        {
            var perfil = new ActualizacionPerfilDTO
            {
                UsuarioId = -1,
                Nombre = "Nombre",
                Apellido = "Apellido",
                Avatar = "avatar.png"
            };

            var resultado = _manejador.ActualizarPerfil(perfil);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ActualizarPerfil_NombreVacio_DeberiaRetornarError()
        {
            var perfil = new ActualizacionPerfilDTO
            {
                UsuarioId = 1,
                Nombre = "",
                Apellido = "Apellido",
                Avatar = "avatar.png"
            };

            var resultado = _manejador.ActualizarPerfil(perfil);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ActualizarPerfil_ApellidoVacio_DeberiaRetornarError()
        {
            var perfil = new ActualizacionPerfilDTO
            {
                UsuarioId = 1,
                Nombre = "Nombre",
                Apellido = "",
                Avatar = "avatar.png"
            };

            var resultado = _manejador.ActualizarPerfil(perfil);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ActualizarPerfil_NombreMuyLargo_DeberiaRetornarError()
        {
            var perfil = new ActualizacionPerfilDTO
            {
                UsuarioId = 1,
                Nombre = new string('a', 51),
                Apellido = "Apellido",
                Avatar = "avatar.png"
            };

            var resultado = _manejador.ActualizarPerfil(perfil);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ActualizarPerfil_ApellidoMuyLargo_DeberiaRetornarError()
        {
            var perfil = new ActualizacionPerfilDTO
            {
                UsuarioId = 1,
                Nombre = "Nombre",
                Apellido = new string('a', 51),
                Avatar = "avatar.png"
            };

            var resultado = _manejador.ActualizarPerfil(perfil);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        #endregion
    }
}
