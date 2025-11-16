using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo
{
    [TestClass]
    public class PruebaPerfilVistaModelo
    {
        private Mock<IPerfilServicio> _mockPerfilServicio;
        private Mock<ISeleccionarAvatarServicio> _mockSeleccionarAvatar;
        private Mock<ICambioContrasenaServicio> _mockCambioContrasena;
        private Mock<IRecuperacionCuentaServicio> _mockRecuperacionCuenta;
        private PerfilVistaModelo _viewModel;

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null)
            {
                new Application();
            }

            Application.ResourceAssembly = typeof(PerfilVistaModelo).Assembly;

            _mockPerfilServicio = new Mock<IPerfilServicio>();
            _mockSeleccionarAvatar = new Mock<ISeleccionarAvatarServicio>();
            _mockCambioContrasena = new Mock<ICambioContrasenaServicio>();
            _mockRecuperacionCuenta = new Mock<IRecuperacionCuentaServicio>();

            // Configurar sesión por defecto
            try
            {
                SesionUsuarioActual.EstablecerUsuario(new DTOs.UsuarioDTO
                {
                    UsuarioId = 1,
                    NombreUsuario = "TestUser",
                    Correo = "test@correo.com"
                });
            }
            catch { /* Ignorar si ya estaba establecida */ }

            AvisoAyudante.DefinirMostrarAviso((msj) => { });

            _viewModel = new PerfilVistaModelo(
                _mockPerfilServicio.Object,
                _mockSeleccionarAvatar.Object,
                _mockCambioContrasena.Object,
                _mockRecuperacionCuenta.Object
            );
        }

        [TestCleanup]
        public void Limpiar()
        {
            try
            {
                SesionUsuarioActual.EstablecerUsuario(new DTOs.UsuarioDTO());
            }
            catch
            {
                // Si incluso el vacío falla, ignoramos el error de limpieza para no ocultar errores reales de prueba.
            }

            _viewModel = null;
        }

        #region Constructor y Validaciones

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_PerfilServicioNulo_LanzaExcepcion()
        {
            new PerfilVistaModelo(null, _mockSeleccionarAvatar.Object, _mockCambioContrasena.Object, _mockRecuperacionCuenta.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_SeleccionarAvatarNulo_LanzaExcepcion()
        {
            new PerfilVistaModelo(_mockPerfilServicio.Object, null, _mockCambioContrasena.Object, _mockRecuperacionCuenta.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_CambioContrasenaNulo_LanzaExcepcion()
        {
            new PerfilVistaModelo(_mockPerfilServicio.Object, _mockSeleccionarAvatar.Object, null, _mockRecuperacionCuenta.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_RecuperacionCuentaNulo_LanzaExcepcion()
        {
            new PerfilVistaModelo(_mockPerfilServicio.Object, _mockSeleccionarAvatar.Object, _mockCambioContrasena.Object, null);
        }

        [TestMethod]
        public void Prueba_Constructor_InicializaRedesSociales()
        {
            Assert.IsNotNull(_viewModel.RedesSociales);
            Assert.AreEqual(4, _viewModel.RedesSociales.Count); 
            Assert.IsTrue(_viewModel.RedesSociales.Any(r => r.Nombre == "Instagram"));
        }

        #endregion

        #region Carga de Perfil

        [TestMethod]
        public async Task Prueba_CargarPerfilAsync_SesionInvalida_MuestraError()
        {
            var usuarioInvalido = new DTOs.UsuarioDTO
            {
                UsuarioId = 0,
                NombreUsuario = "Dummy",
                Correo = "dummy@test.com",
                Nombre = "Dummy",
                Apellido = "Dummy"
            };

            SesionUsuarioActual.EstablecerUsuario(usuarioInvalido);

            bool cerrado = false;
            _viewModel.CerrarAccion = () => cerrado = true;

            await _viewModel.CargarPerfilAsync();

            Assert.IsTrue(cerrado, "La ventana debería cerrarse si la sesión es inválida.");
            _mockPerfilServicio.Verify(s => s.ObtenerPerfilAsync(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_CargarPerfilAsync_PerfilNulo_MuestraError()
        {
            _mockPerfilServicio.Setup(s => s.ObtenerPerfilAsync(1)).ReturnsAsync((DTOs.UsuarioDTO)null);

            await _viewModel.CargarPerfilAsync();

            Assert.IsNull(_viewModel.Nombre);
        }

        [TestMethod]
        public async Task Prueba_CargarPerfilAsync_Exito_MapeaDatos()
        {
            var perfilMock = new DTOs.UsuarioDTO
            {
                UsuarioId = 1,
                NombreUsuario = "User",
                Correo = "c@c.com",
                Nombre = "Juan",
                Apellido = "Perez",
                AvatarId = 1,
                Instagram = "instaUser",
                Facebook = "faceUser"
            };

            _mockPerfilServicio.Setup(s => s.ObtenerPerfilAsync(1)).ReturnsAsync(perfilMock);

            await _viewModel.CargarPerfilAsync();

            Assert.AreEqual("Juan", _viewModel.Nombre);
            Assert.AreEqual("Perez", _viewModel.Apellido);
            Assert.AreEqual("instaUser", _viewModel.RedesSociales.First(r => r.Nombre == "Instagram").Identificador);
            Assert.AreEqual("faceUser", _viewModel.RedesSociales.First(r => r.Nombre == "Facebook").Identificador);
            Assert.IsNull(_viewModel.RedesSociales.First(r => r.Nombre == "X").Identificador); 
        }

        [TestMethod]
        public async Task Prueba_CargarPerfilAsync_AvatarInexistente_UsaDefault()
        {
            var perfilMock = new DTOs.UsuarioDTO { UsuarioId = 1, AvatarId = 9999 }; 
            _mockPerfilServicio.Setup(s => s.ObtenerPerfilAsync(1)).ReturnsAsync(perfilMock);

            await _viewModel.CargarPerfilAsync();

            if (CatalogoAvataresLocales.ObtenerAvatares().Count > 0)
            {
                Assert.IsNotNull(_viewModel.AvatarSeleccionadoImagen);
            }
        }

        [TestMethod]
        public async Task Prueba_CargarPerfilAsync_Excepcion_MuestraError()
        {
            _mockPerfilServicio.Setup(s => s.ObtenerPerfilAsync(1)).ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "Error", null));

            await _viewModel.CargarPerfilAsync();

            Assert.IsFalse(_viewModel.EstaProcesando);
        }

        #endregion

        #region Selección de Avatar

        [TestMethod]
        public async Task Prueba_SeleccionarAvatarAsync_Cancelado_NoCambia()
        {
            _mockSeleccionarAvatar.Setup(s => s.SeleccionarAvatarAsync(It.IsAny<int>())).ReturnsAsync((ObjetoAvatar)null);

            int idInicial = _viewModel.AvatarSeleccionadoId;

            await _viewModel.SeleccionarAvatarComando.EjecutarAsync(null);

            Assert.AreEqual(idInicial, _viewModel.AvatarSeleccionadoId);
        }

        [TestMethod]
        public async Task Prueba_SeleccionarAvatarAsync_Exito_ActualizaPropiedades()
        {
            var nuevoAvatar = new ObjetoAvatar(5, "Nuevo", null);
            _mockSeleccionarAvatar.Setup(s => s.SeleccionarAvatarAsync(It.IsAny<int>())).ReturnsAsync(nuevoAvatar);

            await _viewModel.SeleccionarAvatarComando.EjecutarAsync(null);

            Assert.AreEqual(5, _viewModel.AvatarSeleccionadoId);
            Assert.AreEqual("Nuevo", _viewModel.AvatarSeleccionadoNombre);
        }

        #endregion

        #region Guardar Cambios (Validaciones)

        [TestMethod]
        public async Task Prueba_GuardarCambios_CamposVacios_MuestraErrores()
        {
            List<string> erroresReportados = null;
            _viewModel.MostrarCamposInvalidos = (l) => erroresReportados = l.ToList();

            await _viewModel.GuardarCambiosComando.EjecutarAsync(null);

            Assert.IsNotNull(erroresReportados);
            Assert.IsTrue(erroresReportados.Contains("Nombre"));
            Assert.IsTrue(erroresReportados.Contains("Apellido"));
            Assert.IsTrue(erroresReportados.Contains("Avatar")); 
            _mockPerfilServicio.Verify(s => s.ActualizarPerfilAsync(It.IsAny<DTOs.ActualizacionPerfilDTO>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_GuardarCambios_RedSocialLarga_MuestraError()
        {
            _viewModel.Nombre = "Valido";
            _viewModel.Apellido = "Valido";
            typeof(PerfilVistaModelo).GetProperty("AvatarSeleccionadoId")?.SetValue(_viewModel, 1);

            var red = _viewModel.RedesSociales.First();
            red.Identificador = new string('a', 51); 

            List<string> erroresReportados = null;
            _viewModel.MostrarCamposInvalidos = (l) => erroresReportados = l.ToList();

            await _viewModel.GuardarCambiosComando.EjecutarAsync(null);

            Assert.IsTrue(erroresReportados.Contains("RedesSociales"));
            Assert.IsTrue(red.TieneError);
            _mockPerfilServicio.Verify(s => s.ActualizarPerfilAsync(It.IsAny<DTOs.ActualizacionPerfilDTO>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_GuardarCambios_Exito_LlamaServicioYActualizaSesion()
        {
            _viewModel.Nombre = "Juan";
            _viewModel.Apellido = "Perez";
            typeof(PerfilVistaModelo).GetProperty("AvatarSeleccionadoId")?.SetValue(_viewModel, 1);

            var red = _viewModel.RedesSociales.First(r => r.Nombre == "Instagram");
            red.Identificador = "juan.perez";

            _mockPerfilServicio
                .Setup(s => s.ActualizarPerfilAsync(It.IsAny<DTOs.ActualizacionPerfilDTO>()))
                .ReturnsAsync(new DTOs.ResultadoOperacionDTO { OperacionExitosa = true });

            await _viewModel.GuardarCambiosComando.EjecutarAsync(null);

            _mockPerfilServicio.Verify(s => s.ActualizarPerfilAsync(It.Is<DTOs.ActualizacionPerfilDTO>(dto =>
                dto.Nombre == "Juan" &&
                dto.Instagram == "juan.perez" &&
                dto.Facebook == null 
            )), Times.Once);

            Assert.AreEqual("Juan", SesionUsuarioActual.Usuario.Nombre);
            Assert.AreEqual("juan.perez", SesionUsuarioActual.Usuario.Instagram);
        }

        [TestMethod]
        public async Task Prueba_GuardarCambios_ResultadoNulo_MuestraError()
        {
            ConfigurarCamposValidos();
            _mockPerfilServicio.Setup(s => s.ActualizarPerfilAsync(It.IsAny<DTOs.ActualizacionPerfilDTO>())).ReturnsAsync((DTOs.ResultadoOperacionDTO)null);

            await _viewModel.GuardarCambiosComando.EjecutarAsync(null);

            Assert.IsFalse(_viewModel.EstaProcesando);
        }

        [TestMethod]
        public async Task Prueba_GuardarCambios_Excepcion_MuestraError()
        {
            ConfigurarCamposValidos();
            _mockPerfilServicio.Setup(s => s.ActualizarPerfilAsync(It.IsAny<DTOs.ActualizacionPerfilDTO>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "Fallo", null));

            await _viewModel.GuardarCambiosComando.EjecutarAsync(null);

            Assert.IsFalse(_viewModel.EstaProcesando);
        }

        private void ConfigurarCamposValidos()
        {
            _viewModel.Nombre = "Valido";
            _viewModel.Apellido = "Valido";
            typeof(PerfilVistaModelo).GetProperty("AvatarSeleccionadoId")?.SetValue(_viewModel, 1);
        }

        #endregion

        #region Cambio de Contraseña

        [TestMethod]
        public async Task Prueba_CambiarContrasena_CorreoVacio_MuestraError()
        {
            typeof(PerfilVistaModelo).GetProperty("Correo")?.SetValue(_viewModel, "");

            await _viewModel.CambiarContrasenaComando.EjecutarAsync(null);

            _mockRecuperacionCuenta.Verify(s => s.RecuperarCuentaAsync(It.IsAny<string>(), It.IsAny<ICambioContrasenaServicio>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_CambiarContrasena_Exito_LlamaServicio()
        {
            typeof(PerfilVistaModelo).GetProperty("Correo")?.SetValue(_viewModel, "test@correo.com");

            _mockRecuperacionCuenta
                .Setup(s => s.RecuperarCuentaAsync("test@correo.com", It.IsAny<ICambioContrasenaServicio>()))
                .ReturnsAsync(new DTOs.ResultadoOperacionDTO { OperacionExitosa = true });

            await _viewModel.CambiarContrasenaComando.EjecutarAsync(null);

            _mockRecuperacionCuenta.Verify(s => s.RecuperarCuentaAsync("test@correo.com", It.IsAny<ICambioContrasenaServicio>()), Times.Once);
            Assert.IsFalse(_viewModel.EstaCambiandoContrasena);
        }
        [TestMethod]
        public async Task Prueba_CambiarContrasena_FalloLogico_MuestraMensaje()
        {
            _mockRecuperacionCuenta
                .Setup(s => s.RecuperarCuentaAsync(It.IsAny<string>(), It.IsAny<ICambioContrasenaServicio>()))
                .ReturnsAsync(new DTOs.ResultadoOperacionDTO { OperacionExitosa = false, Mensaje = "Error" });

            await _viewModel.CambiarContrasenaComando.EjecutarAsync(null);

            Assert.IsFalse(_viewModel.EstaCambiandoContrasena);
        }

        [TestMethod]
        public async Task Prueba_CambiarContrasena_Excepcion_MuestraError()
        {
            _mockRecuperacionCuenta
                .Setup(s => s.RecuperarCuentaAsync(It.IsAny<string>(), It.IsAny<ICambioContrasenaServicio>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "Fallo", null));

            await _viewModel.CambiarContrasenaComando.EjecutarAsync(null);

            Assert.IsFalse(_viewModel.EstaCambiandoContrasena);
        }

        #endregion

        #region Comandos y Cierre

        [TestMethod]
        public void Prueba_CerrarComando_InvocaAccion()
        {
            bool cerrado = false;
            _viewModel.CerrarAccion = () => cerrado = true;
            _viewModel.CerrarComando.Execute(null);
            Assert.IsTrue(cerrado);
        }

        [TestMethod]
        public void Prueba_RedSocialItem_Propiedades_Funcionan()
        {
            var item = new PerfilVistaModelo.RedSocialItemVistaModelo("Test", null);
            item.Identificador = "Valor";
            item.TieneError = true;

            Assert.AreEqual("Valor", item.Identificador);
            Assert.IsTrue(item.TieneError);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_RedSocialItem_NombreNulo_LanzaExcepcion()
        {
            new PerfilVistaModelo.RedSocialItemVistaModelo(null, null);
        }

        #endregion
    }
}