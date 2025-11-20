using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Perfil
{
    [TestClass]
    public class PruebaPerfilVistaModelo
    {
        private Mock<IPerfilServicio> _mockPerfilServicio;
        private Mock<ISeleccionarAvatarServicio> _mockSeleccionarAvatar;
        private Mock<ICambioContrasenaServicio> _mockCambioContrasena;
        private Mock<IRecuperacionCuentaServicio> _mockRecuperacionCuenta;
        private PerfilVistaModelo _vistaModelo;

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null) new Application();
            Application.ResourceAssembly = typeof(PerfilVistaModelo).Assembly;

            _mockPerfilServicio = new Mock<IPerfilServicio>();
            _mockSeleccionarAvatar = new Mock<ISeleccionarAvatarServicio>();
            _mockCambioContrasena = new Mock<ICambioContrasenaServicio>();
            _mockRecuperacionCuenta = new Mock<IRecuperacionCuentaServicio>();

            SesionUsuarioActual.EstablecerUsuario(new DTOs.UsuarioDTO
            {
                UsuarioId = 1,
                NombreUsuario = "TestUser",
                Correo = "test@correo.com",
                Nombre = "Original",
                Apellido = "Original"
            });

            AvisoAyudante.DefinirMostrarAviso((msj) => { });

            _vistaModelo = new PerfilVistaModelo(
                _mockPerfilServicio.Object,
                _mockSeleccionarAvatar.Object,
                _mockCambioContrasena.Object,
                _mockRecuperacionCuenta.Object
            );

            _vistaModelo.MostrarCamposInvalidos = (_) => { };
            _vistaModelo.CerrarAccion = () => { };
        }

        [TestCleanup]
        public void Limpiar()
        {
            try
            {
                SesionUsuarioActual.EstablecerUsuario(new DTOs.UsuarioDTO { UsuarioId = 0 });
            }
            catch { }
            _vistaModelo = null;
        }

        #region 1. Constructor y Validaciones Iniciales

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
        public void Prueba_Constructor_InicializaRedesSocialesCorrectamente()
        {
            Assert.IsNotNull(_vistaModelo.RedesSociales);
            Assert.AreEqual(4, _vistaModelo.RedesSociales.Count);
            Assert.IsTrue(_vistaModelo.RedesSociales.Any(r => r.Nombre == "Instagram"));
            Assert.IsTrue(_vistaModelo.RedesSociales.Any(r => r.Nombre == "Facebook"));
            Assert.IsTrue(_vistaModelo.RedesSociales.Any(r => r.Nombre == "X"));
            Assert.IsTrue(_vistaModelo.RedesSociales.Any(r => r.Nombre == "Discord"));
        }

        #endregion

        #region 2. Carga de Perfil (CargarPerfilAsync)

        [TestMethod]
        public async Task Prueba_CargarPerfilAsync_SesionInvalida_CierraVentana()
        {
            SesionUsuarioActual.EstablecerUsuario(new DTOs.UsuarioDTO { UsuarioId = 0 });
            bool cerrado = false;
            _vistaModelo.CerrarAccion = () => cerrado = true;

            await _vistaModelo.CargarPerfilAsync();

            Assert.IsTrue(cerrado);
            _mockPerfilServicio.Verify(s => s.ObtenerPerfilAsync(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_CargarPerfilAsync_Exito_MapeaDatos()
        {
            var perfilDto = new DTOs.UsuarioDTO
            {
                UsuarioId = 1,
                NombreUsuario = "UserDB",
                Correo = "db@correo.com",
                Nombre = "NombreDB",
                Apellido = "ApellidoDB",
                AvatarId = 1,
                Instagram = "instaDB",
                Facebook = "faceDB"
            };
            _mockPerfilServicio.Setup(s => s.ObtenerPerfilAsync(1)).ReturnsAsync(perfilDto);

            await _vistaModelo.CargarPerfilAsync();

            Assert.AreEqual("NombreDB", _vistaModelo.Nombre);
            Assert.AreEqual("ApellidoDB", _vistaModelo.Apellido);
            Assert.AreEqual("instaDB", _vistaModelo.RedesSociales.First(r => r.Nombre == "Instagram").Identificador);
            Assert.AreEqual("faceDB", _vistaModelo.RedesSociales.First(r => r.Nombre == "Facebook").Identificador);
            Assert.IsNull(_vistaModelo.RedesSociales.First(r => r.Nombre == "X").Identificador);
        }

        [TestMethod]
        public async Task Prueba_CargarPerfilAsync_PerfilNulo_MuestraAviso()
        {
            _mockPerfilServicio.Setup(s => s.ObtenerPerfilAsync(1)).ReturnsAsync((DTOs.UsuarioDTO)null);
            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.CargarPerfilAsync();

            Assert.AreEqual(Lang.errorTextoServidorObtenerPerfil, mensaje);
        }

        [TestMethod]
        public async Task Prueba_CargarPerfilAsync_Excepcion_MuestraMensajeError()
        {
            _mockPerfilServicio.Setup(s => s.ObtenerPerfilAsync(1))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorConexion", null));

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.CargarPerfilAsync();

            Assert.AreEqual("ErrorConexion", mensaje);
            Assert.IsFalse(_vistaModelo.EstaProcesando); 
        }

        #endregion

        #region 3. Selección de Avatar

        [TestMethod]
        public async Task Prueba_SeleccionarAvatar_Exito_ActualizaPropiedades()
        {
            var avatarMock = new ObjetoAvatar(5, "NuevoAvatar", null);
            _mockSeleccionarAvatar.Setup(s => s.SeleccionarAvatarAsync(It.IsAny<int>())).ReturnsAsync(avatarMock);

            await _vistaModelo.SeleccionarAvatarComando.EjecutarAsync(null);

            Assert.AreEqual(5, _vistaModelo.AvatarSeleccionadoId);
            Assert.AreEqual("NuevoAvatar", _vistaModelo.AvatarSeleccionadoNombre);
        }

        [TestMethod]
        public async Task Prueba_SeleccionarAvatar_Cancelado_NoCambiaNada()
        {
            _mockSeleccionarAvatar.Setup(s => s.SeleccionarAvatarAsync(It.IsAny<int>())).ReturnsAsync((ObjetoAvatar)null);
            int idOriginal = _vistaModelo.AvatarSeleccionadoId;

            await _vistaModelo.SeleccionarAvatarComando.EjecutarAsync(null);

            Assert.AreEqual(idOriginal, _vistaModelo.AvatarSeleccionadoId);
        }

        #endregion

        #region 4. Guardar Cambios (Validaciones y Lógica)

        [TestMethod]
        public async Task Prueba_GuardarCambios_CamposInvalidos_MuestraErrores()
        {
            _vistaModelo.Nombre = "";
            _vistaModelo.Apellido = "";
            SetAvatarId(0); 

            List<string> invalidos = null;
            _vistaModelo.MostrarCamposInvalidos = (l) => invalidos = l.ToList();

            await _vistaModelo.GuardarCambiosComando.EjecutarAsync(null);

            Assert.IsTrue(invalidos.Contains("Nombre"));
            Assert.IsTrue(invalidos.Contains("Apellido"));
            Assert.IsTrue(invalidos.Contains("Avatar"));
            _mockPerfilServicio.Verify(s => s.ActualizarPerfilAsync(It.IsAny<DTOs.ActualizacionPerfilDTO>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_GuardarCambios_RedSocialLarga_MuestraError()
        {
            SetCamposValidos();
            var red = _vistaModelo.RedesSociales.First();
            red.Identificador = new string('a', 51); 

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.GuardarCambiosComando.EjecutarAsync(null);

            Assert.IsNotNull(mensaje);
            Assert.IsTrue(red.TieneError);
        }

        [TestMethod]
        public async Task Prueba_GuardarCambios_Exito_LlamaServicioYActualizaSesion()
        {
            SetCamposValidos();
            _vistaModelo.Nombre = "NuevoNombre";
            var red = _vistaModelo.RedesSociales.First(r => r.Nombre == "X");
            red.Identificador = "x_handle";

            _mockPerfilServicio
                .Setup(s => s.ActualizarPerfilAsync(It.IsAny<DTOs.ActualizacionPerfilDTO>()))
                .ReturnsAsync(new DTOs.ResultadoOperacionDTO { OperacionExitosa = true });

            await _vistaModelo.GuardarCambiosComando.EjecutarAsync(null);

            _mockPerfilServicio.Verify(s => s.ActualizarPerfilAsync(It.Is<DTOs.ActualizacionPerfilDTO>(d =>
                d.Nombre == "NuevoNombre" &&
                d.X == "x_handle" &&
                d.Instagram == null
            )), Times.Once);

            Assert.AreEqual("NuevoNombre", SesionUsuarioActual.Usuario.Nombre);
            Assert.AreEqual("x_handle", SesionUsuarioActual.Usuario.X);
        }

        [TestMethod]
        public async Task Prueba_GuardarCambios_RespuestaServidorNula_MuestraError()
        {
            SetCamposValidos();
            _mockPerfilServicio
                .Setup(s => s.ActualizarPerfilAsync(It.IsAny<DTOs.ActualizacionPerfilDTO>()))
                .ReturnsAsync((DTOs.ResultadoOperacionDTO)null);

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.GuardarCambiosComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoServidorActualizarPerfil, mensaje);
        }

        [TestMethod]
        public async Task Prueba_GuardarCambios_OperacionFallida_MuestraMensajeServidor()
        {
            SetCamposValidos();

            _mockPerfilServicio
                .Setup(s => s.ActualizarPerfilAsync(It.IsAny<DTOs.ActualizacionPerfilDTO>()))
                .ReturnsAsync(new DTOs.ResultadoOperacionDTO { OperacionExitosa = false, Mensaje = "ErrorValidacion" });

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.GuardarCambiosComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoActualizarPerfil, mensaje);
        }

        #endregion

        #region 5. Cambio de Contraseña

        [TestMethod]
        public async Task Prueba_CambiarContrasena_CorreoVacio_MuestraError()
        {
            typeof(PerfilVistaModelo).GetProperty("Correo").SetValue(_vistaModelo, "");

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.CambiarContrasenaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoIniciarCambioContrasena, mensaje);
            _mockRecuperacionCuenta.Verify(s => s.RecuperarCuentaAsync(It.IsAny<string>(), It.IsAny<ICambioContrasenaServicio>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_CambiarContrasena_Exito_LlamaServicio()
        {
            typeof(PerfilVistaModelo).GetProperty("Correo").SetValue(_vistaModelo, "test@correo.com");

            _mockRecuperacionCuenta
                .Setup(s => s.RecuperarCuentaAsync("test@correo.com", It.IsAny<ICambioContrasenaServicio>()))
                .ReturnsAsync(new DTOs.ResultadoOperacionDTO { OperacionExitosa = true });

            await _vistaModelo.CambiarContrasenaComando.EjecutarAsync(null);

            _mockRecuperacionCuenta.Verify(s => s.RecuperarCuentaAsync("test@correo.com", It.IsAny<ICambioContrasenaServicio>()), Times.Once);
        }

        [TestMethod]
        public async Task Prueba_CambiarContrasena_Fallo_MuestraMensaje()
        {
            typeof(PerfilVistaModelo).GetProperty("Correo").SetValue(_vistaModelo, "test@correo.com");
            _mockRecuperacionCuenta
                .Setup(s => s.RecuperarCuentaAsync(It.IsAny<string>(), It.IsAny<ICambioContrasenaServicio>()))
                .ReturnsAsync(new DTOs.ResultadoOperacionDTO { OperacionExitosa = false, Mensaje = "TokenInvalido" });

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.CambiarContrasenaComando.EjecutarAsync(null);

            Assert.AreEqual("TokenInvalido", mensaje);
        }

        [TestMethod]
        public async Task Prueba_CambiarContrasena_Excepcion_MuestraError()
        {
            typeof(PerfilVistaModelo).GetProperty("Correo").SetValue(_vistaModelo, "test@correo.com");
            _mockRecuperacionCuenta
                .Setup(s => s.RecuperarCuentaAsync(It.IsAny<string>(), It.IsAny<ICambioContrasenaServicio>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "FalloRed", null));

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.CambiarContrasenaComando.EjecutarAsync(null);

            Assert.AreEqual("FalloRed", mensaje);
            Assert.IsFalse(_vistaModelo.EstaCambiandoContrasena); 
        }

        #endregion

        #region 6. Propiedades y Comandos (Cobertura de Setters y Notificaciones)

        [TestMethod]
        public void Prueba_EstaProcesando_Setter_NotificaComandos()
        {
            PropertyInfo prop = typeof(PerfilVistaModelo).GetProperty("EstaProcesando");
            prop.SetValue(_vistaModelo, true);

            Assert.IsFalse(_vistaModelo.GuardarCambiosComando.CanExecute(null));
            Assert.IsFalse(_vistaModelo.SeleccionarAvatarComando.CanExecute(null));
        }

        [TestMethod]
        public void Prueba_EstaCambiandoContrasena_Setter_NotificaComando()
        {
            PropertyInfo prop = typeof(PerfilVistaModelo).GetProperty("EstaCambiandoContrasena");
            prop.SetValue(_vistaModelo, true);

            Assert.IsFalse(_vistaModelo.CambiarContrasenaComando.CanExecute(null));
        }

        [TestMethod]
        public void Prueba_CerrarComando_EjecutaAccion()
        {
            bool cerrado = false;
            _vistaModelo.CerrarAccion = () => cerrado = true;
            _vistaModelo.CerrarComando.Execute(null);
            Assert.IsTrue(cerrado);
        }

        #endregion

        #region 7. Lógica Auxiliar y Clases Anidadas

        [TestMethod]
        public void Prueba_RedSocialItem_Propiedades_FuncionanCorrectamente()
        {
            var item = new PerfilVistaModelo.RedSocialItemVistaModelo("TestRed", null);

            item.Identificador = "MiUser";
            item.TieneError = true;

            Assert.AreEqual("MiUser", item.Identificador);
            Assert.IsTrue(item.TieneError);
            Assert.AreEqual("TestRed", item.Nombre);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_RedSocialItem_NombreNulo_LanzaExcepcion()
        {
            new PerfilVistaModelo.RedSocialItemVistaModelo(null, null);
        }

        #endregion

        private void SetCamposValidos()
        {
            _vistaModelo.Nombre = "Valido";
            _vistaModelo.Apellido = "Valido";
            SetAvatarId(1);
        }

        private void SetAvatarId(int id)
        {
            typeof(PerfilVistaModelo).GetProperty("AvatarSeleccionadoId")?.SetValue(_vistaModelo, id);
        }
    }
}