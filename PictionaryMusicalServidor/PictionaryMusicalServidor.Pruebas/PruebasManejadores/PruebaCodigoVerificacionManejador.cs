using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios;

namespace PictionaryMusicalServidor.Pruebas.PruebasManejadores
{
    /// <summary>
    /// Pruebas unitarias para CodigoVerificacionManejador.
    /// Nota: Estas pruebas se enfocan en validación de entrada y manejo de errores.
    /// Las pruebas completas de integración con base de datos requieren un contexto de prueba separado.
    /// </summary>
    [TestClass]
    public class PruebaCodigoVerificacionManejador
    {
        private CodigoVerificacionManejador _manejador;

        [TestInitialize]
        public void Inicializar()
        {
            _manejador = new CodigoVerificacionManejador();
        }

        #region SolicitarCodigoVerificacion

        [TestMethod]
        public void Prueba_SolicitarCodigoVerificacion_CuentaNula_DeberiaRetornarError()
        {
            var resultado = _manejador.SolicitarCodigoVerificacion(null);

            Assert.IsFalse(resultado.CodigoEnviado);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_SolicitarCodigoVerificacion_UsuarioVacio_DeberiaRetornarError()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "",
                Nombre = "Nombre",
                Apellido = "Apellido",
                Correo = "test@example.com",
                Contrasena = "Pass123!"
            };

            var resultado = _manejador.SolicitarCodigoVerificacion(cuenta);

            Assert.IsFalse(resultado.CodigoEnviado);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_SolicitarCodigoVerificacion_CorreoInvalido_DeberiaRetornarError()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "usuario",
                Nombre = "Nombre",
                Apellido = "Apellido",
                Correo = "correoInvalido",
                Contrasena = "Pass123!"
            };

            var resultado = _manejador.SolicitarCodigoVerificacion(cuenta);

            Assert.IsFalse(resultado.CodigoEnviado);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_SolicitarCodigoVerificacion_ContrasenaDebil_DeberiaRetornarError()
        {
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "usuario",
                Nombre = "Nombre",
                Apellido = "Apellido",
                Correo = "test@example.com",
                Contrasena = "debil"
            };

            var resultado = _manejador.SolicitarCodigoVerificacion(cuenta);

            Assert.IsFalse(resultado.CodigoEnviado);
            Assert.IsNotNull(resultado.Mensaje);
        }

        #endregion

        #region ReenviarCodigoVerificacion

        [TestMethod]
        public void Prueba_ReenviarCodigoVerificacion_SolicitudNula_DeberiaRetornarError()
        {
            var resultado = _manejador.ReenviarCodigoVerificacion(null);

            Assert.IsFalse(resultado.CodigoEnviado);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ReenviarCodigoVerificacion_TokenVacio_DeberiaRetornarError()
        {
            var solicitud = new ReenvioCodigoVerificacionDTO
            {
                TokenCodigo = ""
            };

            var resultado = _manejador.ReenviarCodigoVerificacion(solicitud);

            Assert.IsFalse(resultado.CodigoEnviado);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ReenviarCodigoVerificacion_TokenInvalido_DeberiaRetornarError()
        {
            var solicitud = new ReenvioCodigoVerificacionDTO
            {
                TokenCodigo = "tokenInvalido123"
            };

            var resultado = _manejador.ReenviarCodigoVerificacion(solicitud);

            Assert.IsFalse(resultado.CodigoEnviado);
            Assert.IsNotNull(resultado.Mensaje);
        }

        #endregion

        #region ConfirmarCodigoVerificacion

        [TestMethod]
        public void Prueba_ConfirmarCodigoVerificacion_ConfirmacionNula_DeberiaRetornarError()
        {
            var resultado = _manejador.ConfirmarCodigoVerificacion(null);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ConfirmarCodigoVerificacion_TokenVacio_DeberiaRetornarError()
        {
            var confirmacion = new ConfirmacionCodigoDTO
            {
                TokenCodigo = "",
                Codigo = "123456"
            };

            var resultado = _manejador.ConfirmarCodigoVerificacion(confirmacion);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ConfirmarCodigoVerificacion_CodigoVacio_DeberiaRetornarError()
        {
            var confirmacion = new ConfirmacionCodigoDTO
            {
                TokenCodigo = "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4",
                Codigo = ""
            };

            var resultado = _manejador.ConfirmarCodigoVerificacion(confirmacion);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ConfirmarCodigoVerificacion_TokenInvalido_DeberiaRetornarError()
        {
            var confirmacion = new ConfirmacionCodigoDTO
            {
                TokenCodigo = "tokenInvalido",
                Codigo = "123456"
            };

            var resultado = _manejador.ConfirmarCodigoVerificacion(confirmacion);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_ConfirmarCodigoVerificacion_CodigoMuyCorto_DeberiaRetornarError()
        {
            var confirmacion = new ConfirmacionCodigoDTO
            {
                TokenCodigo = "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4",
                Codigo = "123"
            };

            var resultado = _manejador.ConfirmarCodigoVerificacion(confirmacion);

            Assert.IsFalse(resultado.OperacionExitosa);
            Assert.IsNotNull(resultado.Mensaje);
        }

        #endregion
    }
}
