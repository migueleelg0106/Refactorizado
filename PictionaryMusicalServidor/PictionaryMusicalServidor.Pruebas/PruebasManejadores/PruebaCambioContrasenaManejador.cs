using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios;

namespace PictionaryMusicalServidor.Pruebas.PruebasManejadores
{
    /// <summary>
    /// Pruebas unitarias para CambioContrasenaManejador.
    /// Nota: Estas pruebas se enfocan en validación de entrada y manejo de errores.
    /// Las pruebas completas con base de datos requieren un contexto de prueba separado.
    /// </summary>
    [TestClass]
    public class PruebaCambioContrasenaManejador
    {
        private CambioContrasenaManejador _manejador;

        [TestInitialize]
        public void Inicializar()
        {
            _manejador = new CambioContrasenaManejador();
        }

        #region SolicitarCodigoRecuperacion

        [TestMethod]
        public void Prueba_SolicitarCodigoRecuperacion_SolicitudNula_DeberiaRetornarError()
        {
            var resultado = _manejador.SolicitarCodigoRecuperacion(null);

            Assert.IsFalse(resultado.CodigoEnviado);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_SolicitarCodigoRecuperacion_IdentificadorVacio_DeberiaRetornarError()
        {
            var solicitud = new SolicitudRecuperarCuentaDTO
            {
                Identificador = ""
            };

            var resultado = _manejador.SolicitarCodigoRecuperacion(solicitud);

            Assert.IsFalse(resultado.CodigoEnviado);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_SolicitarCodigoRecuperacion_IdentificadorNulo_DeberiaRetornarError()
        {
            var solicitud = new SolicitudRecuperarCuentaDTO
            {
                Identificador = null
            };

            var resultado = _manejador.SolicitarCodigoRecuperacion(solicitud);

            Assert.IsFalse(resultado.CodigoEnviado);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_SolicitarCodigoRecuperacion_IdentificadorSoloEspacios_DeberiaRetornarError()
        {
            var solicitud = new SolicitudRecuperarCuentaDTO
            {
                Identificador = "   "
            };

            var resultado = _manejador.SolicitarCodigoRecuperacion(solicitud);

            Assert.IsFalse(resultado.CodigoEnviado);
            Assert.IsNotNull(resultado.Mensaje);
        }

        #endregion

        #region ReenviarCodigoRecuperacion

        [TestMethod]
        public void Prueba_ReenviarCodigoRecuperacion_SolicitudNula_DeberiaRetornarError()
        {
            var resultado = _manejador.ReenviarCodigoRecuperacion(null);

            Assert.IsFalse(resultado.CodigoEnviado);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ReenviarCodigoRecuperacion_TokenVacio_DeberiaRetornarError()
        {
            var solicitud = new ReenvioCodigoDTO
            {
                TokenCodigo = ""
            };

            var resultado = _manejador.ReenviarCodigoRecuperacion(solicitud);

            Assert.IsFalse(resultado.CodigoEnviado);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ReenviarCodigoRecuperacion_TokenInvalido_DeberiaRetornarError()
        {
            var solicitud = new ReenvioCodigoDTO
            {
                TokenCodigo = "tokenInvalido123"
            };

            var resultado = _manejador.ReenviarCodigoRecuperacion(solicitud);

            Assert.IsFalse(resultado.CodigoEnviado);
            Assert.IsNotNull(resultado.Mensaje);
        }

        #endregion

        #region ConfirmarCodigoRecuperacion

        [TestMethod]
        public void Prueba_ConfirmarCodigoRecuperacion_ConfirmacionNula_DeberiaRetornarError()
        {
            var resultado = _manejador.ConfirmarCodigoRecuperacion(null);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ConfirmarCodigoRecuperacion_TokenVacio_DeberiaRetornarError()
        {
            var confirmacion = new ConfirmacionCodigoDTO
            {
                TokenCodigo = "",
                Codigo = "123456"
            };

            var resultado = _manejador.ConfirmarCodigoRecuperacion(confirmacion);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ConfirmarCodigoRecuperacion_CodigoVacio_DeberiaRetornarError()
        {
            var confirmacion = new ConfirmacionCodigoDTO
            {
                TokenCodigo = "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4",
                Codigo = ""
            };

            var resultado = _manejador.ConfirmarCodigoRecuperacion(confirmacion);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ConfirmarCodigoRecuperacion_CodigoFormatoInvalido_DeberiaRetornarError()
        {
            var confirmacion = new ConfirmacionCodigoDTO
            {
                TokenCodigo = "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4",
                Codigo = "ABC"
            };

            var resultado = _manejador.ConfirmarCodigoRecuperacion(confirmacion);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        #endregion

        #region ActualizarContrasena

        [TestMethod]
        public void Prueba_ActualizarContrasena_ActualizacionNula_DeberiaRetornarError()
        {
            var resultado = _manejador.ActualizarContrasena(null);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ActualizarContrasena_TokenVacio_DeberiaRetornarError()
        {
            var actualizacion = new ActualizacionContrasenaDTO
            {
                TokenCodigo = "",
                NuevaContrasena = "NewPass123!"
            };

            var resultado = _manejador.ActualizarContrasena(actualizacion);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ActualizarContrasena_ContrasenaVacia_DeberiaRetornarError()
        {
            var actualizacion = new ActualizacionContrasenaDTO
            {
                TokenCodigo = "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4",
                NuevaContrasena = ""
            };

            var resultado = _manejador.ActualizarContrasena(actualizacion);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ActualizarContrasena_ContrasenaDebil_DeberiaRetornarError()
        {
            var actualizacion = new ActualizacionContrasenaDTO
            {
                TokenCodigo = "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4",
                NuevaContrasena = "debil"
            };

            var resultado = _manejador.ActualizarContrasena(actualizacion);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ActualizarContrasena_ContrasenaMuyLarga_DeberiaRetornarError()
        {
            // Máximo es 15 caracteres, esta tiene 20
            var actualizacion = new ActualizacionContrasenaDTO
            {
                TokenCodigo = "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4",
                NuevaContrasena = "Password123!TooLongX"
            };

            var resultado = _manejador.ActualizarContrasena(actualizacion);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        #endregion
    }
}
