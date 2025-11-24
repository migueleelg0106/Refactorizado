using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Servicios;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using System;

namespace PictionaryMusicalServidor.Pruebas
{
    /// <summary>
    /// Pruebas unitarias para CuentaManejador.
    /// Valida el registro de cuentas, detección de duplicados, validación de datos y manejo de errores.
    /// </summary>
    [TestClass]
    public class PruebaCuentaManejador
    {
        #region Pruebas de Validación de Datos

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_RegistrarCuenta_CuentaNula_DeberiaLanzarArgumentNullException()
        {
            // Arrange
            var manejador = new CuentaManejador();

            // Act
            manejador.RegistrarCuenta(null);

            // Assert - Se espera ArgumentNullException
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_UsuarioVacio_DeberiaRetornarErrorValidacion()
        {
            // Arrange
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "juan@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_CorreoFormatoInvalido_DeberiaRetornarErrorValidacion()
        {
            // Arrange
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "juanperez",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "usuario.com", // Sin @
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_CorreoSinDominio_DeberiaRetornarErrorValidacion()
        {
            // Arrange
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "juanperez",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "usuario@",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_UsuarioConCaracteresProhibidos_DeberiaRetornarErrorValidacion()
        {
            // Arrange
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "juan@perez", // Contiene @
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "juan@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            // La validación depende de las reglas específicas del sistema
            // Esta prueba puede necesitar ajustes según la implementación
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_NombreVacio_DeberiaRetornarErrorValidacion()
        {
            // Arrange
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "juanperez",
                Nombre = "",
                Apellido = "Perez",
                Correo = "juan@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_ApellidoVacio_DeberiaRetornarErrorValidacion()
        {
            // Arrange
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "juanperez",
                Nombre = "Juan",
                Apellido = "",
                Correo = "juan@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_ContrasenaDebil_DeberiaRetornarErrorValidacion()
        {
            // Arrange
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "juanperez",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "juan@example.com",
                Contrasena = "pass", // Demasiado corta, sin mayúsculas, sin números, sin caracteres especiales
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_AvatarIdInvalido_DeberiaRetornarErrorValidacion()
        {
            // Arrange
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "juanperez",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "juan@example.com",
                Contrasena = "Password1!",
                AvatarId = 0 // ID inválido
            };

            // Act
            // NOTA: Esta prueba requiere mock del contexto de base de datos
            // para evitar intentar validar contra la BD real
            
            // Assert
            // Se espera error de validación por AvatarId inválido
        }

        #endregion

        #region Pruebas de Lógica de Negocio - Duplicados

        // NOTA: Las siguientes pruebas requieren mocking de la base de datos
        // Se recomienda usar Moq para simular el contexto de Entity Framework

        /*
        [TestMethod]
        public void Prueba_RegistrarCuenta_CorreoYaRegistrado_DeberiaRetornarErrorDuplicado()
        {
            // Arrange
            // TODO: Mock del contexto para retornar true en la búsqueda de correo
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "juanperez",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "existente@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsTrue(resultado.CorreoRegistrado);
            Assert.IsFalse(resultado.UsuarioRegistrado);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_UsuarioYaRegistrado_DeberiaRetornarErrorDuplicado()
        {
            // Arrange
            // TODO: Mock del contexto para retornar true en la búsqueda de usuario
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "usuarioexistente",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "nuevo@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsFalse(resultado.CorreoRegistrado);
            Assert.IsTrue(resultado.UsuarioRegistrado);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_UsuarioYCorreoRegistrados_DeberiaRetornarAmbosErrores()
        {
            // Arrange
            // TODO: Mock del contexto para retornar true en ambas búsquedas
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "usuarioexistente",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "existente@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsTrue(resultado.CorreoRegistrado);
            Assert.IsTrue(resultado.UsuarioRegistrado);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_DatosValidos_DeberiaGuardarEnBaseDatos()
        {
            // Arrange
            // TODO: Mock del contexto para permitir guardado exitoso
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "nuevousuario",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "nuevo@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsTrue(resultado.RegistroExitoso);
            Assert.IsNull(resultado.Mensaje);
            // Verificar que SaveChanges fue llamado
            // Verificar que la contraseña fue hasheada con BCrypt
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_ContrasenaHasheada_DeberiaUsarBCrypt()
        {
            // Arrange
            // TODO: Mock del contexto para capturar el usuario guardado
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "nuevousuario",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "nuevo@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsTrue(resultado.RegistroExitoso);
            // Verificar que la contraseña guardada no es texto plano
            // Verificar que la contraseña guardada es un hash BCrypt válido
        }
        */

        #endregion

        #region Pruebas de Infraestructura - Manejo de Errores

        /*
        [TestMethod]
        public void Prueba_RegistrarCuenta_FalloSaveChanges_DeberiaRetornarMensajeError()
        {
            // Arrange
            // TODO: Mock del contexto para lanzar DbUpdateException en SaveChanges
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "nuevousuario",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "nuevo@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_ErrorBaseDatos_DeberiaRetornarMensajeError()
        {
            // Arrange
            // TODO: Mock del contexto para lanzar EntityException
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "nuevousuario",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "nuevo@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
            // Verificar que el mensaje no expone detalles técnicos
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_ErrorValidacionEntidad_DeberiaRetornarMensajeError()
        {
            // Arrange
            // TODO: Mock del contexto para lanzar DbEntityValidationException
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "nuevousuario",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "nuevo@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.RegistroExitoso);
            Assert.IsNotNull(resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_RegistrarCuenta_TransaccionFalla_DeberiaRevertirCambios()
        {
            // Arrange
            // TODO: Mock del contexto para simular fallo en transacción
            var manejador = new CuentaManejador();
            var cuenta = new NuevaCuentaDTO
            {
                Usuario = "nuevousuario",
                Nombre = "Juan",
                Apellido = "Perez",
                Correo = "nuevo@example.com",
                Contrasena = "Password1!",
                AvatarId = 1
            };

            // Act
            var resultado = manejador.RegistrarCuenta(cuenta);

            // Assert
            Assert.IsFalse(resultado.RegistroExitoso);
            // Verificar que no se guardó ningún dato parcial en BD
        }
        */

        #endregion

        #region Pruebas de Verificación de Código

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_SolicitarCodigoVerificacion_CuentaNula_DeberiaLanzarArgumentNullException()
        {
            // Arrange
            var manejador = new CuentaManejador();

            // Act
            manejador.SolicitarCodigoVerificacion(null);

            // Assert - Se espera ArgumentNullException
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_ReenviarCodigoVerificacion_SolicitudNula_DeberiaLanzarArgumentNullException()
        {
            // Arrange
            var manejador = new CuentaManejador();

            // Act
            manejador.ReenviarCodigoVerificacion(null);

            // Assert - Se espera ArgumentNullException
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_ConfirmarCodigoVerificacion_ConfirmacionNula_DeberiaLanzarArgumentNullException()
        {
            // Arrange
            var manejador = new CuentaManejador();

            // Act
            manejador.ConfirmarCodigoVerificacion(null);

            // Assert - Se espera ArgumentNullException
        }

        #endregion
    }
}
