using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Servicios.Servicios;
using System;
using System.Collections.Generic;
using System.Data;

namespace PictionaryMusicalServidor.Pruebas
{
    /// <summary>
    /// Pruebas para AmistadServicio.
    /// Nota: AmistadServicio es una clase estática que usa ContextoFactory.CrearContexto()
    /// directamente. Estas pruebas validan la lógica de negocio que no depende de base de datos.
    /// Para pruebas de integración completas, se requiere configurar una base de datos de prueba.
    /// </summary>
    [TestClass]
    public class PruebaAmistadServicio
    {
        #region CrearSolicitud - Validaciones de Negocio

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_CrearSolicitud_MismoUsuario_LanzaExcepcion()
        {
            // Arrange
            int usuarioId = 1;

            // Act & Assert - Debe lanzar InvalidOperationException
            AmistadServicio.CrearSolicitud(usuarioId, usuarioId);
        }

        [TestMethod]
        public void Prueba_CrearSolicitud_MismoUsuario_MensajeError()
        {
            // Arrange
            int usuarioId = 1;

            try
            {
                // Act
                AmistadServicio.CrearSolicitud(usuarioId, usuarioId);
                Assert.Fail("Debería lanzar InvalidOperationException");
            }
            catch (InvalidOperationException ex)
            {
                // Assert - Verifica que el mensaje de error es apropiado
                Assert.IsNotNull(ex.Message);
                Assert.IsTrue(ex.Message.Length > 0, "El mensaje de error no debe estar vacío");
            }
        }

        // Casos de integracion a probar con base de datos:
        // - Prueba_CrearSolicitud_UsuariosValidos_CreaSolicitudPendiente
        // - Prueba_CrearSolicitud_RelacionExistente_LanzaInvalidOperationException
        // - Prueba_CrearSolicitud_ErrorBaseDatos_PropagaExcepcion

        #endregion

        #region AceptarSolicitud - Casos de integracion

        // Casos de integracion a probar con base de datos:
        // - Prueba_AceptarSolicitud_SolicitudNoExiste_LanzaInvalidOperationException
        // - Prueba_AceptarSolicitud_UsuarioNoEsReceptor_LanzaInvalidOperationException
        // - Prueba_AceptarSolicitud_SolicitudYaAceptada_LanzaInvalidOperationException
        // - Prueba_AceptarSolicitud_SolicitudValida_ActualizaEstadoATrue
        // - Prueba_AceptarSolicitud_ErrorBaseDatos_PropagaExcepcion

        #endregion

        #region EliminarAmistad - Validaciones de Negocio

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Prueba_EliminarAmistad_MismoUsuario_LanzaExcepcion()
        {
            // Arrange
            int usuarioId = 1;

            // Act & Assert - Debe lanzar InvalidOperationException
            AmistadServicio.EliminarAmistad(usuarioId, usuarioId);
        }

        [TestMethod]
        public void Prueba_EliminarAmistad_MismoUsuario_MensajeError()
        {
            // Arrange
            int usuarioId = 5;

            try
            {
                // Act
                AmistadServicio.EliminarAmistad(usuarioId, usuarioId);
                Assert.Fail("Debería lanzar InvalidOperationException");
            }
            catch (InvalidOperationException ex)
            {
                // Assert - Verifica que el mensaje de error es apropiado
                Assert.IsNotNull(ex.Message);
                Assert.IsTrue(ex.Message.Length > 0, "El mensaje de error no debe estar vacío");
            }
        }

        // Casos de integracion a probar con base de datos:
        // - Prueba_EliminarAmistad_RelacionNoExiste_LanzaInvalidOperationException
        // - Prueba_EliminarAmistad_RelacionPendiente_EliminaRelacion
        // - Prueba_EliminarAmistad_RelacionAceptada_EliminaRelacion
        // - Prueba_EliminarAmistad_Exitosa_RetornaRelacionEliminada
        // - Prueba_EliminarAmistad_ErrorBaseDatos_PropagaExcepcion

        #endregion

        #region ObtenerSolicitudesPendientesDTO - Casos de integracion

        // Casos de integracion a probar con base de datos:
        // - Prueba_ObtenerSolicitudesPendientesDTO_SinSolicitudes_RetornaListaVacia
        // - Prueba_ObtenerSolicitudesPendientesDTO_ConSolicitudesPendientes_RetornaListaFiltrada
        // - Prueba_ObtenerSolicitudesPendientesDTO_SoloSolicitudesRecibidas_NoIncluyeEnviadas
        // - Prueba_ObtenerSolicitudesPendientesDTO_SoloSolicitudesPendientes_NoIncluyeAceptadas
        // - Prueba_ObtenerSolicitudesPendientesDTO_UsuarioSinDatos_OmiteRegistrosInvalidos
        // - Prueba_ObtenerSolicitudesPendientesDTO_NombreUsuarioNulo_OmiteRegistro
        // - Prueba_ObtenerSolicitudesPendientesDTO_NombreUsuarioVacio_OmiteRegistro
        // - Prueba_ObtenerSolicitudesPendientesDTO_ListaNula_RetornaListaVacia
        // - Prueba_ObtenerSolicitudesPendientesDTO_ErrorBaseDatos_PropagaExcepcion

        #endregion

        #region ObtenerAmigosDTO - Casos de integracion

        // Casos de integracion a probar con base de datos:
        // - Prueba_ObtenerAmigosDTO_SinAmigos_RetornaListaVacia
        // - Prueba_ObtenerAmigosDTO_ConAmigosAceptados_RetornaListaCompleta
        // - Prueba_ObtenerAmigosDTO_SoloRelacionesAceptadas_NoIncluyePendientes
        // - Prueba_ObtenerAmigosDTO_AmigoNulo_OmiteRegistro
        // - Prueba_ObtenerAmigosDTO_ListaNula_RetornaListaVacia
        // - Prueba_ObtenerAmigosDTO_ErrorBaseDatos_PropagaExcepcion

        #endregion

        #region Pruebas de Wrapper para Mocking

        [TestMethod]
        public void Prueba_AmistadServicioWrapper_CrearSolicitud_DelegaAClaseEstatica()
        {
            // Arrange
            var wrapper = new AmistadServicioWrapper();
            int usuarioId = 1;

            try
            {
                // Act - Debe lanzar excepcion igual que la clase estática
                wrapper.CrearSolicitud(usuarioId, usuarioId);
                Assert.Fail("Debería lanzar InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
                // Assert - Comportamiento esperado
            }
        }

        [TestMethod]
        public void Prueba_AmistadServicioWrapper_EliminarAmistad_DelegaAClaseEstatica()
        {
            // Arrange
            var wrapper = new AmistadServicioWrapper();
            int usuarioId = 1;

            try
            {
                // Act - Debe lanzar excepcion igual que la clase estática
                wrapper.EliminarAmistad(usuarioId, usuarioId);
                Assert.Fail("Debería lanzar InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
                // Assert - Comportamiento esperado
            }
        }

        #endregion
    }
}
