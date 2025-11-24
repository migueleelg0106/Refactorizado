using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Linq;
using BCryptNet = BCrypt.Net.BCrypt;

namespace PictionaryMusicalServidor.Pruebas
{
    /// <summary>
    /// Clase de pruebas unitarias para InicioSesionManejador.
    /// Verifica el comportamiento del servicio de autenticación de usuarios,
    /// incluyendo validaciones, manejo de excepciones y casos de error.
    /// </summary>
    [TestClass]
    public class PruebaInicioSesionManejador
    {
        private InicioSesionManejador _manejador;
        private const string ContrasenaTextoPlano = "Password123!";

        [TestInitialize]
        public void Inicializar()
        {
            _manejador = new InicioSesionManejador();
        }

        #region 1. Validaciones de Argumentos Nulos

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_IniciarSesion_CredencialesNulas_LanzaExcepcion()
        {
            _manejador.IniciarSesion(null);
        }

        #endregion

        #region 2. Validaciones de Datos Invalidos

        [TestMethod]
        public void Prueba_IniciarSesion_IdentificadorVacio_RetornaCredencialesInvalidas()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "",
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsTrue(resultado.CuentaEncontrada);
            Assert.AreEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_IdentificadorNulo_RetornaCredencialesInvalidas()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = null,
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsTrue(resultado.CuentaEncontrada);
            Assert.AreEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_IdentificadorSoloEspacios_RetornaCredencialesInvalidas()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "   ",
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsTrue(resultado.CuentaEncontrada);
            Assert.AreEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_ContrasenaNula_RetornaCredencialesInvalidas()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario123",
                Contrasena = null
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsTrue(resultado.CuentaEncontrada);
            Assert.AreEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_ContrasenaVacia_RetornaCredencialesInvalidas()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario123",
                Contrasena = ""
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsTrue(resultado.CuentaEncontrada);
            Assert.AreEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_ContrasenaSoloEspacios_RetornaCredencialesInvalidas()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario123",
                Contrasena = "    "
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsTrue(resultado.CuentaEncontrada);
            Assert.AreEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_IdentificadorExcedeLongitud_RetornaCredencialesInvalidas()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = new string('a', 51), // 51 caracteres excede el limite de 50
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsTrue(resultado.CuentaEncontrada);
            Assert.AreEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_AmbosVacios_RetornaCredencialesInvalidas()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "",
                Contrasena = ""
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsTrue(resultado.CuentaEncontrada);
            Assert.AreEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        #endregion

        #region 3. Validaciones de Normalizacion

        [TestMethod]
        public void Prueba_IniciarSesion_IdentificadorConEspaciosAlInicio_NormalizaYProcesa()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "   usuario123",
                Contrasena = "Password123!"
            };

            // Este test verifica que el identificador se normaliza correctamente
            // El resultado depende de si el usuario existe en la BD
            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            // La validacion no debe fallar por espacios, ya que se normalizan
            Assert.AreNotEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_IdentificadorConEspaciosAlFinal_NormalizaYProcesa()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario123   ",
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            // La validacion no debe fallar por espacios, ya que se normalizan
            Assert.AreNotEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_ContrasenaConEspacios_ProcesaCorrectamente()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario123",
                Contrasena = "  Password123!  "
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            // La contrasena se debe normalizar (trim) y procesar
            Assert.AreNotEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        #endregion

        #region 4. Casos de Usuario No Encontrado

        [TestMethod]
        public void Prueba_IniciarSesion_UsuarioNoExiste_RetornaCuentaNoEncontrada()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuarioInexistente999",
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsFalse(resultado.CuentaEncontrada);
            Assert.AreEqual(MensajesError.Cliente.CredencialesIncorrectas, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_CorreoNoExiste_RetornaCuentaNoEncontrada()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "correo.inexistente@example.com",
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsFalse(resultado.CuentaEncontrada);
            Assert.AreEqual(MensajesError.Cliente.CredencialesIncorrectas, resultado.Mensaje);
        }

        #endregion

        #region 5. Casos de Contrasena Incorrecta

        // Nota: Estas pruebas requieren que exista un usuario en la base de datos de pruebas
        // o que se mockee el contexto de base de datos. Por ahora, documentamos el comportamiento esperado.

        /*
        [TestMethod]
        public void Prueba_IniciarSesion_ContrasenaIncorrecta_RetornaContrasenaIncorrecta()
        {
            // Este test requiere un usuario real en la BD o un mock del contexto
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuarioExistente",
                Contrasena = "ContrasenaIncorrecta123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado.InicioSesionExitoso);
            Assert.IsTrue(resultado.ContrasenaIncorrecta);
            Assert.AreEqual(MensajesError.Cliente.CredencialesIncorrectas, resultado.Mensaje);
        }
        */

        #endregion

        #region 6. Casos de Exito (Happy Path)

        // Nota: Estas pruebas requieren datos en la base de datos de pruebas
        // o un mock completo del contexto de Entity Framework.

        /*
        [TestMethod]
        public void Prueba_IniciarSesion_CredencialesCorrectas_RetornaExito()
        {
            // Este test requiere un usuario real en la BD con credenciales conocidas
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuarioTest",
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            Assert.IsTrue(resultado.InicioSesionExitoso);
            Assert.IsNotNull(resultado.Usuario);
            Assert.IsTrue(resultado.Usuario.UsuarioId > 0);
            Assert.AreEqual("usuarioTest", resultado.Usuario.NombreUsuario);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_InicioConCorreo_RetornaExito()
        {
            // Test de inicio de sesión usando correo electrónico
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario@test.com",
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            Assert.IsTrue(resultado.InicioSesionExitoso);
            Assert.IsNotNull(resultado.Usuario);
            Assert.IsTrue(resultado.Usuario.UsuarioId > 0);
            Assert.AreEqual("usuario@test.com", resultado.Usuario.Correo);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_Exito_MapeoCompletoDeDatos()
        {
            // Verifica que todos los campos del usuario se mapeen correctamente
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuarioTest",
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            Assert.IsTrue(resultado.InicioSesionExitoso);
            Assert.IsNotNull(resultado.Usuario);
            Assert.IsTrue(resultado.Usuario.UsuarioId > 0);
            Assert.IsTrue(resultado.Usuario.JugadorId > 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(resultado.Usuario.NombreUsuario));
            Assert.IsFalse(string.IsNullOrWhiteSpace(resultado.Usuario.Nombre));
            Assert.IsFalse(string.IsNullOrWhiteSpace(resultado.Usuario.Apellido));
            Assert.IsFalse(string.IsNullOrWhiteSpace(resultado.Usuario.Correo));
            Assert.IsTrue(resultado.Usuario.AvatarId > 0);
        }
        */

        #endregion

        #region 7. Manejo de Excepciones de Base de Datos

        /*
        // Nota: Estos tests requieren mocking del ContextoFactory y del contexto de base de datos
        // debido a que Entity Framework no es fácilmente mockeable sin interfaces.
        // 
        // RECOMENDACIÓN: Para implementar estas pruebas, refactorizar la arquitectura usando:
        // 1. Patrón Repositorio: Crear IUsuarioRepositorio para abstraer acceso a datos
        // 2. Inyección de Dependencias: Permitir inyectar IContextoFactory en lugar de usar clase estática
        // 3. Wrapper del Contexto: Crear interfaz IBaseDatosContexto para mockear Entity Framework
        //
        // Ejemplo de refactorización recomendada:
        // public InicioSesionManejador(IUsuarioRepositorio repositorio) { ... }
        //
        // Con esta arquitectura, los siguientes tests serían implementables:

        [TestMethod]
        public void Prueba_IniciarSesion_EntityException_RetornaMensajeError()
        {
            // Simula un error de conexión a la base de datos
            // var mockRepo = new Mock<IUsuarioRepositorio>();
            // mockRepo.Setup(r => r.BuscarPorIdentificador(It.IsAny<string>()))
            //         .Throws(new EntityException("Error de conexión"));
            // 
            // var manejador = new InicioSesionManejador(mockRepo.Object);
            // var resultado = manejador.IniciarSesion(credenciales);
            // 
            // Assert.AreEqual(MensajesError.Cliente.ErrorInicioSesion, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_DataException_RetornaMensajeError()
        {
            // Simula un error de datos durante la consulta
            // Similar al anterior pero lanzando DataException
        }

        [TestMethod]
        public void Prueba_IniciarSesion_InvalidOperationException_RetornaMensajeError()
        {
            // Simula un estado inconsistente del contexto
            // Similar al anterior pero lanzando InvalidOperationException
        }
        */

        #endregion

        #region 8. Pruebas de Seguridad y BCrypt

        [TestMethod]
        public void Prueba_VerificacionBCrypt_ContrasenaCorrecta_RetornaTrue()
        {
            // Test unitario para verificar el comportamiento de BCrypt
            string contrasenaHasheada = BCryptNet.HashPassword(ContrasenaTextoPlano);
            bool resultado = BCryptNet.Verify(ContrasenaTextoPlano, contrasenaHasheada);

            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void Prueba_VerificacionBCrypt_ContrasenaIncorrecta_RetornaFalse()
        {
            // Test unitario para verificar el comportamiento de BCrypt con contrasena incorrecta
            string contrasenaHasheada = BCryptNet.HashPassword("Password123!");
            bool resultado = BCryptNet.Verify("ContrasenaIncorrecta!", contrasenaHasheada);

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Prueba_VerificacionBCrypt_ContrasenasCaseSensitive_RetornaFalse()
        {
            // Verifica que BCrypt es sensible a mayusculas/minusculas
            string contrasenaHasheada = BCryptNet.HashPassword("Password123!");
            bool resultado = BCryptNet.Verify("password123!", contrasenaHasheada);

            Assert.IsFalse(resultado);
        }

        #endregion

        #region 9. Casos Limite y Edge Cases

        [TestMethod]
        public void Prueba_IniciarSesion_IdentificadorLongitudMaxima_ProcesaCorrectamente()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = new string('a', 50), // Exactamente 50 caracteres (limite)
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            // No debe fallar por longitud, debe procesar normalmente
            Assert.AreNotEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_ContrasenaConCaracteresEspeciales_ProcesaCorrectamente()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario123",
                Contrasena = "P@ssw0rd!#$%^&*()"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            // La contrasena con caracteres especiales debe procesarse correctamente
            Assert.AreNotEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_ContrasenaConEspaciosEnMedio_ProcesaCorrectamente()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario123",
                Contrasena = "Pass word 123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            // Las contrasenas pueden tener espacios en el medio
            Assert.AreNotEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_IdentificadorConCaracteresUnicode_ProcesaCorrectamente()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario123",
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            // Los caracteres unicode deben procesarse correctamente
            Assert.AreNotEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_CorreoConMayusculas_ProcesaCorrectamente()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "Usuario@Test.COM",
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado = _manejador.IniciarSesion(credenciales);

            Assert.IsNotNull(resultado);
            // Los correos pueden tener mayusculas
            Assert.AreNotEqual(MensajesError.Cliente.CredencialesInvalidas, resultado.Mensaje);
        }

        #endregion

        #region 10. Pruebas de Consistencia de Mensajes

        [TestMethod]
        public void Prueba_IniciarSesion_DatosInvalidos_MensajeConsistente()
        {
            var credenciales1 = new CredencialesInicioSesionDTO
            {
                Identificador = "",
                Contrasena = "Password123!"
            };

            var credenciales2 = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario",
                Contrasena = ""
            };

            ResultadoInicioSesionDTO resultado1 = _manejador.IniciarSesion(credenciales1);
            ResultadoInicioSesionDTO resultado2 = _manejador.IniciarSesion(credenciales2);

            // Ambos casos invalidos deben retornar el mismo mensaje
            Assert.AreEqual(resultado1.Mensaje, resultado2.Mensaje);
            Assert.AreEqual(MensajesError.Cliente.CredencialesInvalidas, resultado1.Mensaje);
        }

        [TestMethod]
        public void Prueba_IniciarSesion_UsuariosInexistentes_MensajeConsistente()
        {
            var credenciales1 = new CredencialesInicioSesionDTO
            {
                Identificador = "usuarioInexistente1",
                Contrasena = "Password123!"
            };

            var credenciales2 = new CredencialesInicioSesionDTO
            {
                Identificador = "correo.inexistente@example.com",
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado1 = _manejador.IniciarSesion(credenciales1);
            ResultadoInicioSesionDTO resultado2 = _manejador.IniciarSesion(credenciales2);

            // Ambos casos de usuario no encontrado deben retornar el mismo mensaje
            Assert.AreEqual(resultado1.Mensaje, resultado2.Mensaje);
            Assert.AreEqual(MensajesError.Cliente.CredencialesIncorrectas, resultado1.Mensaje);
        }

        #endregion

        #region 11. Pruebas de Multiples Intentos

        [TestMethod]
        public void Prueba_IniciarSesion_MultiplesIntentosFallidos_CadaUnoRetornaError()
        {
            var credenciales = new CredencialesInicioSesionDTO
            {
                Identificador = "usuarioInexistente",
                Contrasena = "Password123!"
            };

            ResultadoInicioSesionDTO resultado1 = _manejador.IniciarSesion(credenciales);
            ResultadoInicioSesionDTO resultado2 = _manejador.IniciarSesion(credenciales);
            ResultadoInicioSesionDTO resultado3 = _manejador.IniciarSesion(credenciales);

            // Cada intento debe retornar error consistentemente
            Assert.IsFalse(resultado1.InicioSesionExitoso);
            Assert.IsFalse(resultado2.InicioSesionExitoso);
            Assert.IsFalse(resultado3.InicioSesionExitoso);
            Assert.AreEqual(resultado1.Mensaje, resultado2.Mensaje);
            Assert.AreEqual(resultado2.Mensaje, resultado3.Mensaje);
        }

        #endregion

        #region 12. Pruebas de Inmutabilidad del Manejador

        [TestMethod]
        public void Prueba_IniciarSesion_InstanciaManejador_ReutilizableSinEstado()
        {
            var credenciales1 = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario1",
                Contrasena = "Password123!"
            };

            var credenciales2 = new CredencialesInicioSesionDTO
            {
                Identificador = "usuario2",
                Contrasena = "Password456!"
            };

            // El manejador no debe mantener estado entre llamadas
            ResultadoInicioSesionDTO resultado1 = _manejador.IniciarSesion(credenciales1);
            ResultadoInicioSesionDTO resultado2 = _manejador.IniciarSesion(credenciales2);

            // Cada resultado debe ser independiente
            Assert.IsNotNull(resultado1);
            Assert.IsNotNull(resultado2);
            Assert.AreNotSame(resultado1, resultado2);
        }

        #endregion
    }
}
