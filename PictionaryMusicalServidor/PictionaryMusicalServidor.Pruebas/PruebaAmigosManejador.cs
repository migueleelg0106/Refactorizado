using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Servicios;
using System;
using System.ServiceModel;

namespace PictionaryMusicalServidor.Pruebas
{
    /// <summary>
    /// Pruebas para AmigosManejador.
    /// Nota: AmigosManejador es un servicio WCF con estado singleton que usa dependencias estaticas
    /// y acceso directo a base de datos. Estas pruebas validan la logica de validacion de entrada.
    /// Para pruebas de integracion completas, se requiere configurar una base de datos de prueba.
    /// </summary>
    [TestClass]
    public class PruebaAmigosManejador
    {
        private AmigosManejador _manejador;

        [TestInitialize]
        public void Inicializar()
        {
            _manejador = new AmigosManejador();
        }

        [TestCleanup]
        public void Limpiar()
        {
            _manejador = null;
        }

        #region Suscribir - Validaciones de Entrada

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Suscribir_NombreUsuarioNulo_LanzaExcepcion()
        {
            // Act & Assert - Debe lanzar FaultException
            _manejador.Suscribir(null);
        }

        [TestMethod]
        public void Prueba_Suscribir_NombreUsuarioNulo_MensajeError()
        {
            try
            {
                // Act
                _manejador.Suscribir(null);
                Assert.Fail("Deberia lanzar FaultException");
            }
            catch (FaultException ex)
            {
                // Assert - Verifica que hay un mensaje de error
                Assert.IsNotNull(ex.Message);
                Assert.IsTrue(ex.Message.Length > 0, "El mensaje de error no debe estar vacio");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Suscribir_NombreUsuarioVacio_LanzaExcepcion()
        {
            // Act & Assert - Debe lanzar FaultException
            _manejador.Suscribir("");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Suscribir_NombreUsuarioEspacios_LanzaExcepcion()
        {
            // Act & Assert - Debe lanzar FaultException
            _manejador.Suscribir("   ");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Suscribir_NombreUsuarioTab_LanzaExcepcion()
        {
            // Act & Assert - Debe lanzar FaultException
            _manejador.Suscribir("\t");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_Suscribir_NombreUsuarioSaltosLinea_LanzaExcepcion()
        {
            // Act & Assert - Debe lanzar FaultException
            _manejador.Suscribir("\n\r");
        }

        // Casos de integracion a probar con base de datos:
        // - Prueba_Suscribir_UsuarioNoExiste_LanzaFaultException
        // - Prueba_Suscribir_UsuarioValido_SuscribeExitosamente
        // - Prueba_Suscribir_UsuarioYaSuscrito_ActualizaSuscripcion
        // - Prueba_Suscribir_NormalizaNombreUsuario_SuscribeConNombreNormalizado
        // - Prueba_Suscribir_NotificaSolicitudesPendientes_AlSuscribir
        // - Prueba_Suscribir_ErrorBaseDatos_LanzaFaultException
        // - Prueba_Suscribir_ErrorEntityException_LanzaFaultException
        // - Prueba_Suscribir_ErrorDataException_LanzaFaultException

        #endregion

        #region CancelarSuscripcion - Validaciones de Entrada

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_CancelarSuscripcion_NombreUsuarioNulo_LanzaExcepcion()
        {
            // Act & Assert - Debe lanzar FaultException
            _manejador.CancelarSuscripcion(null);
        }

        [TestMethod]
        public void Prueba_CancelarSuscripcion_NombreUsuarioNulo_MensajeError()
        {
            try
            {
                // Act
                _manejador.CancelarSuscripcion(null);
                Assert.Fail("Deberia lanzar FaultException");
            }
            catch (FaultException ex)
            {
                // Assert - Verifica que hay un mensaje de error
                Assert.IsNotNull(ex.Message);
                Assert.IsTrue(ex.Message.Length > 0, "El mensaje de error no debe estar vacio");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_CancelarSuscripcion_NombreUsuarioVacio_LanzaExcepcion()
        {
            // Act & Assert - Debe lanzar FaultException
            _manejador.CancelarSuscripcion("");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_CancelarSuscripcion_NombreUsuarioEspacios_LanzaExcepcion()
        {
            // Act & Assert - Debe lanzar FaultException
            _manejador.CancelarSuscripcion("   ");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_CancelarSuscripcion_NombreUsuarioTab_LanzaExcepcion()
        {
            // Act & Assert - Debe lanzar FaultException
            _manejador.CancelarSuscripcion("\t\t");
        }

        // Casos de integracion a probar con base de datos:
        // - Prueba_CancelarSuscripcion_UsuarioNoSuscrito_NoLanzaExcepcion
        // - Prueba_CancelarSuscripcion_UsuarioSuscrito_CancelaSuscripcion
        // - Prueba_CancelarSuscripcion_UsuarioSuscrito_NoRecibeNotificaciones

        #endregion

        #region EnviarSolicitudAmistad - Casos de integracion

        // Casos de integracion a probar con base de datos:
        // - Prueba_EnviarSolicitudAmistad_NombreEmisorNulo_LanzaFaultException
        // - Prueba_EnviarSolicitudAmistad_NombreEmisorVacio_LanzaFaultException
        // - Prueba_EnviarSolicitudAmistad_NombreReceptorNulo_LanzaFaultException
        // - Prueba_EnviarSolicitudAmistad_NombreReceptorVacio_LanzaFaultException
        // - Prueba_EnviarSolicitudAmistad_EmisorNoExiste_LanzaFaultException
        // - Prueba_EnviarSolicitudAmistad_ReceptorNoExiste_LanzaFaultException
        // - Prueba_EnviarSolicitudAmistad_RelacionExistente_LanzaFaultException
        // - Prueba_EnviarSolicitudAmistad_MismoUsuario_LanzaFaultException
        // - Prueba_EnviarSolicitudAmistad_Exitosa_CreaSolicitud
        // - Prueba_EnviarSolicitudAmistad_Exitosa_NotificaReceptor
        // - Prueba_EnviarSolicitudAmistad_Exitosa_UsaNombresNormalizados
        // - Prueba_EnviarSolicitudAmistad_ErrorInvalidOperation_LanzaFaultException
        // - Prueba_EnviarSolicitudAmistad_ErrorArgument_LanzaFaultException
        // - Prueba_EnviarSolicitudAmistad_ErrorDataException_LanzaFaultException
        // - Prueba_EnviarSolicitudAmistad_ErrorGenerico_LanzaFaultException

        #endregion

        #region ResponderSolicitudAmistad - Casos de integracion

        // Casos de integracion a probar con base de datos:
        // - Prueba_ResponderSolicitudAmistad_NombreEmisorNulo_LanzaFaultException
        // - Prueba_ResponderSolicitudAmistad_NombreEmisorVacio_LanzaFaultException
        // - Prueba_ResponderSolicitudAmistad_NombreReceptorNulo_LanzaFaultException
        // - Prueba_ResponderSolicitudAmistad_NombreReceptorVacio_LanzaFaultException
        // - Prueba_ResponderSolicitudAmistad_EmisorNoExiste_LanzaFaultException
        // - Prueba_ResponderSolicitudAmistad_ReceptorNoExiste_LanzaFaultException
        // - Prueba_ResponderSolicitudAmistad_SolicitudNoExiste_LanzaFaultException
        // - Prueba_ResponderSolicitudAmistad_UsuarioNoEsReceptor_LanzaFaultException
        // - Prueba_ResponderSolicitudAmistad_SolicitudYaAceptada_LanzaFaultException
        // - Prueba_ResponderSolicitudAmistad_Exitosa_AceptaSolicitud
        // - Prueba_ResponderSolicitudAmistad_Exitosa_NotificaEmisor
        // - Prueba_ResponderSolicitudAmistad_Exitosa_NotificaReceptor
        // - Prueba_ResponderSolicitudAmistad_Exitosa_ActualizaListaAmigosEmisor
        // - Prueba_ResponderSolicitudAmistad_Exitosa_ActualizaListaAmigosReceptor
        // - Prueba_ResponderSolicitudAmistad_Exitosa_UsaNombresNormalizados
        // - Prueba_ResponderSolicitudAmistad_ErrorInvalidOperation_LanzaFaultException
        // - Prueba_ResponderSolicitudAmistad_ErrorArgument_LanzaFaultException
        // - Prueba_ResponderSolicitudAmistad_ErrorDataException_LanzaFaultException
        // - Prueba_ResponderSolicitudAmistad_ErrorGenerico_LanzaFaultException

        #endregion

        #region EliminarAmigo - Casos de integracion

        // Casos de integracion a probar con base de datos:
        // - Prueba_EliminarAmigo_NombreUsuarioANulo_LanzaFaultException
        // - Prueba_EliminarAmigo_NombreUsuarioAVacio_LanzaFaultException
        // - Prueba_EliminarAmigo_NombreUsuarioBNulo_LanzaFaultException
        // - Prueba_EliminarAmigo_NombreUsuarioBVacio_LanzaFaultException
        // - Prueba_EliminarAmigo_UsuarioANoExiste_LanzaFaultException
        // - Prueba_EliminarAmigo_UsuarioBNoExiste_LanzaFaultException
        // - Prueba_EliminarAmigo_MismoUsuario_LanzaFaultException
        // - Prueba_EliminarAmigo_RelacionNoExiste_LanzaFaultException
        // - Prueba_EliminarAmigo_Exitosa_EliminaRelacion
        // - Prueba_EliminarAmigo_Exitosa_NotificaUsuarioA
        // - Prueba_EliminarAmigo_Exitosa_NotificaUsuarioB
        // - Prueba_EliminarAmigo_Exitosa_ActualizaListaAmigosUsuarioA
        // - Prueba_EliminarAmigo_Exitosa_ActualizaListaAmigosUsuarioB
        // - Prueba_EliminarAmigo_Exitosa_UsaNombresNormalizados
        // - Prueba_EliminarAmigo_Exitosa_IdentificaEmisorCorrectamente
        // - Prueba_EliminarAmigo_ErrorInvalidOperation_LanzaFaultException
        // - Prueba_EliminarAmigo_ErrorArgument_LanzaFaultException
        // - Prueba_EliminarAmigo_ErrorDataException_LanzaFaultException
        // - Prueba_EliminarAmigo_ErrorGenerico_LanzaFaultException

        #endregion

        #region Pruebas de Constructor

        [TestMethod]
        public void Prueba_Constructor_CreaInstancia()
        {
            // Arrange & Act
            var manejador = new AmigosManejador();

            // Assert
            Assert.IsNotNull(manejador);
        }

        #endregion
    }
}
