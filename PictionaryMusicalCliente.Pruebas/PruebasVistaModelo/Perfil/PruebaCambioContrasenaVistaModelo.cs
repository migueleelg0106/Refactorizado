using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Perfil
{
    [TestClass]
    public class PruebaCambioContrasenaVistaModelo
    {
        private Mock<ICambioContrasenaServicio> _mockServicio;
        private CambioContrasenaVistaModelo _viewModel;
        private const string TokenPrueba = "TOKEN123";

        [TestInitialize]
        public void Inicializar()
        {
            _mockServicio = new Mock<ICambioContrasenaServicio>();
            _viewModel = new CambioContrasenaVistaModelo(TokenPrueba, _mockServicio.Object);

            AvisoAyudante.DefinirMostrarAviso((m) => { });
            _viewModel.MostrarCamposInvalidos = (_) => { };
        }

        [TestCleanup]
        public void Limpiar()
        {
            _viewModel = null;
        }

        #region 1. Constructor y Validaciones Nulas

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_TokenNulo_LanzaExcepcion()
        {
            new CambioContrasenaVistaModelo(null, _mockServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_ServicioNulo_LanzaExcepcion()
        {
            new CambioContrasenaVistaModelo(TokenPrueba, null);
        }

        #endregion

        #region 2. Propiedades y Comandos (Estado)

        [TestMethod]
        public void Prueba_Propiedades_GetSet_Funcionan()
        {
            _viewModel.NuevaContrasena = "Pass1";
            Assert.AreEqual("Pass1", _viewModel.NuevaContrasena);

            _viewModel.ConfirmacionContrasena = "Pass2";
            Assert.AreEqual("Pass2", _viewModel.ConfirmacionContrasena);
        }

        [TestMethod]
        public void Prueba_EstaProcesando_AfectaComando()
        {
            typeof(CambioContrasenaVistaModelo).GetProperty("EstaProcesando").SetValue(_viewModel, true);
            Assert.IsFalse(_viewModel.ConfirmarComando.CanExecute(null));

            typeof(CambioContrasenaVistaModelo).GetProperty("EstaProcesando").SetValue(_viewModel, false);
            Assert.IsTrue(_viewModel.ConfirmarComando.CanExecute(null));
        }

        #endregion

        #region 3. Validaciones de Entrada

        [TestMethod]
        public async Task Prueba_Confirmar_CamposVacios_MuestraError()
        {
            _viewModel.NuevaContrasena = "";
            _viewModel.ConfirmacionContrasena = "";

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _viewModel.ConfirmarComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoConfirmacionContrasenaRequerida, mensaje);
            _mockServicio.Verify(s => s.ActualizarContrasenaAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_Confirmar_ContrasenaInvalida_MuestraErrorFormato()
        {
            _viewModel.NuevaContrasena = "123"; 
            _viewModel.ConfirmacionContrasena = "123";

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _viewModel.ConfirmarComando.EjecutarAsync(null);

            Assert.IsNotNull(mensaje);
            _mockServicio.Verify(s => s.ActualizarContrasenaAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_Confirmar_NoCoinciden_MuestraError()
        {
            _viewModel.NuevaContrasena = "Password123!";
            _viewModel.ConfirmacionContrasena = "OtraCosa!!!";

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);
            List<string> invalidos = null;
            _viewModel.MostrarCamposInvalidos = (l) => invalidos = l.ToList();

            await _viewModel.ConfirmarComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoContrasenasNoCoinciden, mensaje);
            Assert.IsNotNull(invalidos);
            Assert.IsTrue(invalidos.Contains("NuevaContrasena"));
            Assert.IsTrue(invalidos.Contains("ConfirmacionContrasena"));
        }

        #endregion

        #region 4. Flujo de Cambio de Contraseña

        [TestMethod]
        public async Task Prueba_Confirmar_Exito_InvocaAccion()
        {
            _viewModel.NuevaContrasena = "Password123!";
            _viewModel.ConfirmacionContrasena = "Password123!";

            _mockServicio.Setup(s => s.ActualizarContrasenaAsync(TokenPrueba, "Password123!"))
                .ReturnsAsync(new DTOs.ResultadoOperacionDTO { OperacionExitosa = true });

            DTOs.ResultadoOperacionDTO resultadoRecibido = null;
            _viewModel.CambioContrasenaCompletado = (r) => resultadoRecibido = r;

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _viewModel.ConfirmarComando.EjecutarAsync(null);

            Assert.IsNotNull(resultadoRecibido);
            Assert.IsTrue(resultadoRecibido.OperacionExitosa);
            Assert.AreEqual(Lang.avisoTextoContrasenaActualizada, mensaje);
        }

        [TestMethod]
        public async Task Prueba_Confirmar_FalloLogico_MuestraError()
        {
            _viewModel.NuevaContrasena = "Password123!";
            _viewModel.ConfirmacionContrasena = "Password123!";

            _mockServicio.Setup(s => s.ActualizarContrasenaAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new DTOs.ResultadoOperacionDTO { OperacionExitosa = false, Mensaje = "TokenExpirado" });

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);
            bool completado = false;
            _viewModel.CambioContrasenaCompletado = (_) => completado = true;

            await _viewModel.ConfirmarComando.EjecutarAsync(null);

            Assert.IsFalse(completado);
            Assert.AreEqual("TokenExpirado", mensaje);
        }

        [TestMethod]
        public async Task Prueba_Confirmar_RespuestaNula_MuestraError()
        {
            _viewModel.NuevaContrasena = "Password123!";
            _viewModel.ConfirmacionContrasena = "Password123!";

            _mockServicio.Setup(s => s.ActualizarContrasenaAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((DTOs.ResultadoOperacionDTO)null);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _viewModel.ConfirmarComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoActualizarContrasena, mensaje);
        }

        [TestMethod]
        public async Task Prueba_Confirmar_Excepcion_MuestraError()
        {
            _viewModel.NuevaContrasena = "Password123!";
            _viewModel.ConfirmacionContrasena = "Password123!";

            _mockServicio.Setup(s => s.ActualizarContrasenaAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorRed", null));

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _viewModel.ConfirmarComando.EjecutarAsync(null);

            Assert.AreEqual("ErrorRed", mensaje);
            Assert.IsFalse(_viewModel.EstaProcesando);
        }

        #endregion

        #region 5. Cancelación

        [TestMethod]
        public void Prueba_CancelarComando_InvocaAccion()
        {
            bool cancelado = false;
            _viewModel.Cancelado = () => cancelado = true;

            _viewModel.CancelarComando.Execute(null);

            Assert.IsTrue(cancelado);
        }

        #endregion
    }
}