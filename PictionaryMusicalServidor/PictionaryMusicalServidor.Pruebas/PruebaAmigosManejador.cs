using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Servicios;
using System;
using System.ServiceModel;

namespace PictionaryMusicalServidor.Pruebas
{
    /// <summary>
    /// Pruebas para AmigosManejador
    /// 
    /// NOTA: AmigosManejador es un servicio WCF con estado singleton y callbacks.
    /// Estas pruebas se centran en validar las entradas y el comportamiento observable
    /// sin necesidad de una base de datos o contexto WCF completo.
    /// 
    /// Las pruebas validan:
    /// 1. Validación de parámetros de entrada
    /// 2. Manejo de errores y excepciones esperadas
    /// 3. Comportamiento con valores nulos o vacíos
    /// 
    /// Para pruebas más completas de la lógica de negocio que interactúa con la base de datos,
    /// ver PruebaAmistadServicio, que prueba la capa de servicio donde se implementa
    /// la lógica principal.
    /// </summary>
    [TestClass]
    public class PruebaAmigosManejador
    {
        #region Pruebas de Validación de Entrada - Suscribir

        [TestMethod]
        public void Prueba_Suscribir_NombreUsuarioNulo_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.Suscribir(null);
            });

            Assert.IsTrue(exception.Message.Contains("obligatorio") || exception.Message.Contains("required"),
                "El mensaje de error debe indicar que el nombre de usuario es obligatorio");
        }

        [TestMethod]
        public void Prueba_Suscribir_NombreUsuarioVacio_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.Suscribir("");
            });

            Assert.IsTrue(exception.Message.Contains("obligatorio") || exception.Message.Contains("required"),
                "El mensaje de error debe indicar que el nombre de usuario es obligatorio");
        }

        [TestMethod]
        public void Prueba_Suscribir_NombreUsuarioSoloEspacios_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.Suscribir("   ");
            });

            Assert.IsTrue(exception.Message.Contains("obligatorio") || exception.Message.Contains("required"),
                "El mensaje de error debe indicar que el nombre de usuario es obligatorio");
        }

        #endregion

        #region Pruebas de Validación de Entrada - CancelarSuscripcion

        [TestMethod]
        public void Prueba_CancelarSuscripcion_NombreUsuarioNulo_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.CancelarSuscripcion(null);
            });

            Assert.IsTrue(exception.Message.Contains("obligatorio") || exception.Message.Contains("required"),
                "El mensaje de error debe indicar que el nombre de usuario es obligatorio");
        }

        [TestMethod]
        public void Prueba_CancelarSuscripcion_NombreUsuarioVacio_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.CancelarSuscripcion("");
            });

            Assert.IsTrue(exception.Message.Contains("obligatorio") || exception.Message.Contains("required"),
                "El mensaje de error debe indicar que el nombre de usuario es obligatorio");
        }

        [TestMethod]
        public void Prueba_CancelarSuscripcion_NombreUsuarioSoloEspacios_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.CancelarSuscripcion("   ");
            });

            Assert.IsTrue(exception.Message.Contains("obligatorio") || exception.Message.Contains("required"),
                "El mensaje de error debe indicar que el nombre de usuario es obligatorio");
        }

        [TestMethod]
        public void Prueba_CancelarSuscripcion_UsuarioNoSuscrito_NoLanzaExcepcion()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert - No debe lanzar excepción
            manejador.CancelarSuscripcion("UsuarioNoSuscrito");
        }

        #endregion

        #region Pruebas de Validación de Entrada - EnviarSolicitudAmistad

        [TestMethod]
        public void Prueba_EnviarSolicitudAmistad_NombreEmisorNulo_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.EnviarSolicitudAmistad(null, "UsuarioReceptor");
            });

            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void Prueba_EnviarSolicitudAmistad_NombreEmisorVacio_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.EnviarSolicitudAmistad("", "UsuarioReceptor");
            });

            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void Prueba_EnviarSolicitudAmistad_NombreReceptorNulo_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.EnviarSolicitudAmistad("UsuarioEmisor", null);
            });

            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void Prueba_EnviarSolicitudAmistad_NombreReceptorVacio_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.EnviarSolicitudAmistad("UsuarioEmisor", "");
            });

            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void Prueba_EnviarSolicitudAmistad_AmbosNombresVacios_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.EnviarSolicitudAmistad("", "");
            });

            Assert.IsNotNull(exception);
        }

        #endregion

        #region Pruebas de Validación de Entrada - ResponderSolicitudAmistad

        [TestMethod]
        public void Prueba_ResponderSolicitudAmistad_NombreEmisorNulo_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.ResponderSolicitudAmistad(null, "UsuarioReceptor");
            });

            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void Prueba_ResponderSolicitudAmistad_NombreEmisorVacio_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.ResponderSolicitudAmistad("", "UsuarioReceptor");
            });

            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void Prueba_ResponderSolicitudAmistad_NombreReceptorNulo_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.ResponderSolicitudAmistad("UsuarioEmisor", null);
            });

            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void Prueba_ResponderSolicitudAmistad_NombreReceptorVacio_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.ResponderSolicitudAmistad("UsuarioEmisor", "");
            });

            Assert.IsNotNull(exception);
        }

        #endregion

        #region Pruebas de Validación de Entrada - EliminarAmigo

        [TestMethod]
        public void Prueba_EliminarAmigo_NombreUsuarioANulo_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.EliminarAmigo(null, "UsuarioB");
            });

            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void Prueba_EliminarAmigo_NombreUsuarioAVacio_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.EliminarAmigo("", "UsuarioB");
            });

            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void Prueba_EliminarAmigo_NombreUsuarioBNulo_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.EliminarAmigo("UsuarioA", null);
            });

            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void Prueba_EliminarAmigo_NombreUsuarioBVacio_LanzaFaultException()
        {
            // Arrange
            var manejador = new AmigosManejador();

            // Act & Assert
            var exception = Assert.ThrowsException<FaultException>(() =>
            {
                manejador.EliminarAmigo("UsuarioA", "");
            });

            Assert.IsNotNull(exception);
        }

        #endregion

        #region Notas para Pruebas de Integración

        /// <summary>
        /// NOTAS PARA PRUEBAS DE INTEGRACIÓN FUTURAS:
        /// 
        /// VALIDACIÓN DE ENTRADA: Ya se están probando las validaciones de entrada
        /// para todos los métodos (Suscribir, CancelarSuscripcion, EnviarSolicitudAmistad,
        /// ResponderSolicitudAmistad, EliminarAmigo) sin necesidad de base de datos.
        /// 
        /// Las siguientes funcionalidades requieren pruebas de integración
        /// con una base de datos de prueba y un contexto WCF configurado:
        /// 
        /// 1. EnviarSolicitudAmistad (escenarios completos):
        ///    - Validar que usuarios existen en BD
        ///    - Crear solicitud en BD
        ///    - Notificar al receptor vía callback
        ///    - Normalizar nombres de usuario
        /// 
        /// 2. ResponderSolicitudAmistad (escenarios completos):
        ///    - Validar existencia de solicitud en BD
        ///    - Actualizar estado de solicitud
        ///    - Notificar a ambos usuarios
        ///    - Actualizar listas de amigos
        /// 
        /// 3. EliminarAmigo (escenarios completos):
        ///    - Validar existencia de relación en BD
        ///    - Eliminar relación de BD
        ///    - Notificar a ambos usuarios
        ///    - Actualizar listas de amigos
        /// 
        /// 4. Suscribir (casos complejos):
        ///    - Validar usuario existe en BD
        ///    - Registrar callback
        ///    - Notificar solicitudes pendientes
        ///    - Manejar reconexiones
        /// 
        /// RECOMENDACIÓN: Crear un proyecto de pruebas de integración separado que:
        /// - Configure una base de datos de prueba (SQLite, SQL Server LocalDB, o contenedor Docker)
        /// - Use un host WCF de prueba para simular callbacks
        /// - Ejecute escenarios completos de flujo de usuario
        /// - Limpie datos entre pruebas para aislamiento
        /// 
        /// ALTERNATIVA: Refactorizar AmigosManejador para soportar inyección de dependencias:
        /// - Crear IUsuarioRepositorio y IAmigoRepositorio como dependencias
        /// - Crear IManejadorCallback como dependencia
        /// - Crear INotificadorAmigos como dependencia
        /// - Inyectar estas dependencias en el constructor o propiedades
        /// - Esto permitiría pruebas unitarias completas con mocks
        /// 
        /// La lógica de negocio principal está bien testeada en AmistadServicio.
        /// Las pruebas de AmigosManejador deben enfocarse en:
        /// - Orquestación correcta de llamadas a servicios
        /// - Manejo correcto de callbacks WCF
        /// - Manejo de errores de comunicación
        /// - Validación de permisos y autorización
        /// </summary>
        [TestMethod]
        public void NotasPruebasIntegracion()
        {
            // Este método existe solo para documentar las necesidades de pruebas de integración
            Assert.IsTrue(true, "Ver comentarios de documentación sobre pruebas de integración");
        }

        #endregion

        #region Pruebas de Reglas de Negocio (Delegadas a AmistadServicio)

        /// <summary>
        /// Las siguientes reglas de negocio están implementadas en AmistadServicio
        /// y se prueban exhaustivamente en PruebaAmistadServicio:
        /// 
        /// - No se puede enviar solicitud a uno mismo
        /// - No se puede crear solicitud duplicada
        /// - No se puede aceptar solicitud inexistente
        /// - No se puede aceptar solicitud que no corresponde al receptor
        /// - No se puede aceptar solicitud ya aceptada
        /// - No se puede eliminar amistad consigo mismo
        /// - No se puede eliminar amistad inexistente
        /// - Filtrado correcto de solicitudes pendientes
        /// - Filtrado correcto de lista de amigos
        /// - Manejo de valores nulos y vacíos
        /// 
        /// AmigosManejador delega estas validaciones a AmistadServicio,
        /// por lo que no es necesario duplicar estas pruebas aquí.
        /// </summary>
        [TestMethod]
        public void NotasReglasNegocio()
        {
            // Este método existe solo para documentar que las reglas de negocio
            // se prueban en PruebaAmistadServicio
            Assert.IsTrue(true, "Las reglas de negocio se prueban en PruebaAmistadServicio");
        }

        #endregion
    }
}
