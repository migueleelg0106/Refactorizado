using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.Amigos
{
    [TestClass]
    public class PruebaInvitarAmigosVistaModelo
    {
        private Mock<IInvitacionesServicio> _mockInvitaciones;
        private Mock<IPerfilServicio> _mockPerfil;
        private InvitarAmigosVistaModelo _vistaModelo;
        private List<DTOs.AmigoDTO> _listaAmigos;
        private const string CodigoSala = "SALA123";

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null) new Application();

            _mockInvitaciones = new Mock<IInvitacionesServicio>();
            _mockPerfil = new Mock<IPerfilServicio>();

            _listaAmigos = new List<DTOs.AmigoDTO>
            {
                new DTOs.AmigoDTO { UsuarioId = 1, NombreUsuario = "Amigo1" },
                new DTOs.AmigoDTO { UsuarioId = 2, NombreUsuario = "Amigo2" }
            };

            AvisoAyudante.DefinirMostrarAviso((_) => { });

            _vistaModelo = new InvitarAmigosVistaModelo(
                _listaAmigos,
                _mockInvitaciones.Object,
                _mockPerfil.Object,
                CodigoSala,
                (id) => false,
                (id) => { },
                (msg) => { }
            );
        }

        [TestCleanup]
        public void Limpiar()
        {
            _vistaModelo = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_InvitacionesNulo_LanzaExcepcion()
        {
            new InvitarAmigosVistaModelo(_listaAmigos, null, _mockPerfil.Object, CodigoSala, null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_PerfilNulo_LanzaExcepcion()
        {
            new InvitarAmigosVistaModelo(_listaAmigos, _mockInvitaciones.Object, null, CodigoSala, null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_CodigoSalaVacio_LanzaExcepcion()
        {
            new InvitarAmigosVistaModelo(_listaAmigos, _mockInvitaciones.Object, _mockPerfil.Object, "", null, null, null);
        }

        [TestMethod]
        public void Constructor_ListaAmigosNula_InicializaVacia()
        {
            var vm = new InvitarAmigosVistaModelo(null, _mockInvitaciones.Object, _mockPerfil.Object, CodigoSala, null, null, null);
            Assert.IsNotNull(vm.Amigos);
            Assert.AreEqual(0, vm.Amigos.Count);
        }

        [TestMethod]
        public void Constructor_FiltraIdsInvalidosYDuplicados()
        {
            var listaSucia = new List<DTOs.AmigoDTO>
            {
                new DTOs.AmigoDTO { UsuarioId = 1, NombreUsuario = "A" },
                new DTOs.AmigoDTO { UsuarioId = 0, NombreUsuario = "B" },
                new DTOs.AmigoDTO { UsuarioId = 1, NombreUsuario = "A Duplicado" },
                new DTOs.AmigoDTO { UsuarioId = 3, NombreUsuario = "C" },
                null
            };

            var vm = new InvitarAmigosVistaModelo(listaSucia, _mockInvitaciones.Object, _mockPerfil.Object, CodigoSala, null, null, null);

            Assert.AreEqual(2, vm.Amigos.Count);
            Assert.IsTrue(vm.Amigos.Any(a => a.UsuarioId == 1));
            Assert.IsTrue(vm.Amigos.Any(a => a.UsuarioId == 3));
        }

        [TestMethod]
        public void Constructor_MarcaYaInvitados()
        {
            var vm = new InvitarAmigosVistaModelo(
                _listaAmigos,
                _mockInvitaciones.Object,
                _mockPerfil.Object,
                CodigoSala,
                (id) => id == 2,
                null,
                null);

            var item1 = vm.Amigos.First(a => a.UsuarioId == 1);
            var item2 = vm.Amigos.First(a => a.UsuarioId == 2);

            Assert.IsFalse(item1.InvitacionEnviada);
            Assert.IsTrue(item2.InvitacionEnviada);
        }

        [TestMethod]
        public async Task InvitarAsync_ItemNulo_NoHaceNada()
        {
            var metodo = typeof(InvitarAmigosVistaModelo).GetMethod("InvitarAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)metodo.Invoke(_vistaModelo, new object[] { null });

            _mockPerfil.Verify(p => p.ObtenerPerfilAsync(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task InvitarAsync_YaInvitado_MuestraMensajeYRetorna()
        {
            string mensaje = null;
            var vm = new InvitarAmigosVistaModelo(_listaAmigos, _mockInvitaciones.Object, _mockPerfil.Object, CodigoSala, null, null, m => mensaje = m);

            var item = vm.Amigos.First();

            var prop = item.GetType().GetProperty("InvitacionEnviada");
            prop.SetValue(item, true);

            item.InvitarComando.Execute(null);
            await Task.Delay(50);

            Assert.AreEqual(Lang.invitarAmigosTextoYaInvitado, mensaje);
            _mockPerfil.Verify(p => p.ObtenerPerfilAsync(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task InvitarAsync_IdInvalido_MuestraMensaje()
        {
            string mensaje = null;
            var vm = new InvitarAmigosVistaModelo(_listaAmigos, _mockInvitaciones.Object, _mockPerfil.Object, CodigoSala, null, null, m => mensaje = m);

            var item = vm.Amigos.First();

            var fieldInfo = typeof(AmigoInvitacionItemVistaModelo)
                .GetField("<UsuarioId>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(fieldInfo, "No se encontró el campo de respaldo de UsuarioId");
            fieldInfo.SetValue(item, 0); 

            item.InvitarComando.Execute(null);
            await Task.Delay(50);

            Assert.AreEqual(Lang.errorTextoErrorProcesarSolicitud, mensaje);
            _mockPerfil.Verify(p => p.ObtenerPerfilAsync(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task InvitarAsync_PerfilNulo_MuestraMensaje()
        {
            string mensaje = null;
            var vm = new InvitarAmigosVistaModelo(_listaAmigos, _mockInvitaciones.Object, _mockPerfil.Object, CodigoSala, null, null, m => mensaje = m);
            var item = vm.Amigos.First();

            _mockPerfil.Setup(p => p.ObtenerPerfilAsync(item.UsuarioId)).ReturnsAsync((DTOs.UsuarioDTO)null);

            item.InvitarComando.Execute(null);
            await Task.Delay(50);

            Assert.AreEqual(Lang.invitarAmigosTextoCorreoNoDisponible, mensaje);
            Assert.IsFalse(item.EstaProcesando);
        }

        [TestMethod]
        public async Task InvitarAsync_CorreoVacio_MuestraMensaje()
        {
            string mensaje = null;
            var vm = new InvitarAmigosVistaModelo(_listaAmigos, _mockInvitaciones.Object, _mockPerfil.Object, CodigoSala, null, null, m => mensaje = m);
            var item = vm.Amigos.First();

            _mockPerfil.Setup(p => p.ObtenerPerfilAsync(item.UsuarioId)).ReturnsAsync(new DTOs.UsuarioDTO { Correo = "" });

            item.InvitarComando.Execute(null);
            await Task.Delay(50);

            Assert.AreEqual(Lang.invitarAmigosTextoCorreoNoDisponible, mensaje);
        }

        [TestMethod]
        public async Task InvitarAsync_Exito_InvocaAccionesYActualizaEstado()
        {
            string mensaje = null;
            int idRegistrado = 0;

            var vm = new InvitarAmigosVistaModelo(
                _listaAmigos,
                _mockInvitaciones.Object,
                _mockPerfil.Object,
                CodigoSala,
                null,
                id => idRegistrado = id,
                m => mensaje = m);

            var item = vm.Amigos.First();
            string correo = "test@test.com";

            _mockPerfil.Setup(p => p.ObtenerPerfilAsync(item.UsuarioId)).ReturnsAsync(new DTOs.UsuarioDTO { Correo = correo });

            _mockInvitaciones.Setup(i => i.EnviarInvitacionAsync(CodigoSala, correo))
                .ReturnsAsync(new DTOs.ResultadoOperacionDTO { OperacionExitosa = true });

            item.InvitarComando.Execute(null);
            await Task.Delay(100);

            Assert.IsTrue(item.InvitacionEnviada);
            Assert.AreEqual(item.UsuarioId, idRegistrado);
            Assert.AreEqual(Lang.invitarCorreoTextoEnviado, mensaje);
            Assert.IsFalse(item.EstaProcesando);
        }

        [TestMethod]
        public async Task InvitarAsync_FalloOperacion_MuestraMensajeServidor()
        {
            string mensaje = null;
            var vm = new InvitarAmigosVistaModelo(_listaAmigos, _mockInvitaciones.Object, _mockPerfil.Object, CodigoSala, null, null, m => mensaje = m);
            var item = vm.Amigos.First();

            _mockPerfil.Setup(p => p.ObtenerPerfilAsync(item.UsuarioId)).ReturnsAsync(new DTOs.UsuarioDTO { Correo = "a@b.c" });

            _mockInvitaciones.Setup(i => i.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new DTOs.ResultadoOperacionDTO { OperacionExitosa = false, Mensaje = "FalloLogico" });

            item.InvitarComando.Execute(null);
            await Task.Delay(100);

            Assert.IsFalse(item.InvitacionEnviada);

            Assert.AreEqual(Lang.errorTextoEnviarCorreo, mensaje);
        }

        [TestMethod]
        public async Task InvitarAsync_ServicioExcepcion_MuestraError()
        {
            string mensaje = null;
            var vm = new InvitarAmigosVistaModelo(_listaAmigos, _mockInvitaciones.Object, _mockPerfil.Object, CodigoSala, null, null, m => mensaje = m);
            var item = vm.Amigos.First();

            _mockPerfil.Setup(p => p.ObtenerPerfilAsync(item.UsuarioId)).ReturnsAsync(new DTOs.UsuarioDTO { Correo = "a@b.c" });

            _mockInvitaciones.Setup(i => i.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorRed", null));

            item.InvitarComando.Execute(null);
            await Task.Delay(100);

            Assert.AreEqual("ErrorRed", mensaje);
            Assert.IsFalse(item.EstaProcesando);
        }

        [TestMethod]
        public async Task InvitarAsync_ArgumentException_MuestraError()
        {
            string mensaje = null;
            var vm = new InvitarAmigosVistaModelo(_listaAmigos, _mockInvitaciones.Object, _mockPerfil.Object, CodigoSala, null, null, m => mensaje = m);
            var item = vm.Amigos.First();

            _mockPerfil.Setup(p => p.ObtenerPerfilAsync(item.UsuarioId)).ReturnsAsync(new DTOs.UsuarioDTO { Correo = "a@b.c" });

            _mockInvitaciones.Setup(i => i.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("ArgInvalido"));

            item.InvitarComando.Execute(null);
            await Task.Delay(100);

            Assert.AreEqual("ArgInvalido", mensaje);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Item_Constructor_AmigoNulo_LanzaExcepcion()
        {
            new AmigoInvitacionItemVistaModelo(null, _vistaModelo, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Item_Constructor_PadreNulo_LanzaExcepcion()
        {
            new AmigoInvitacionItemVistaModelo(new DTOs.AmigoDTO(), null, false);
        }

        [TestMethod]
        public void Item_TextoBoton_CambiaCorrectamente()
        {
            var item = new AmigoInvitacionItemVistaModelo(new DTOs.AmigoDTO(), _vistaModelo, false);
            Assert.AreEqual(Lang.globalTextoInvitar, item.TextoBoton);

            var prop = item.GetType().GetProperty("InvitacionEnviada");
            prop.SetValue(item, true);

            Assert.AreEqual(Lang.invitarAmigosTextoInvitado, item.TextoBoton);
        }

        [TestMethod]
        public void Item_EstaProcesando_NotificaComando()
        {
            var item = new AmigoInvitacionItemVistaModelo(new DTOs.AmigoDTO(), _vistaModelo, false);
            Assert.IsTrue(item.InvitarComando.CanExecute(null));

            item.EstaProcesando = true;
            Assert.IsFalse(item.InvitarComando.CanExecute(null));
        }
    }
}