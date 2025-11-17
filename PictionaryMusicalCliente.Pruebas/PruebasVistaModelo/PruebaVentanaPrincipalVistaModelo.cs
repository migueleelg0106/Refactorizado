using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo
{
    [TestClass]
    public class PruebaVentanaPrincipalVistaModelo
    {
        private Mock<ILocalizacionServicio> _mockLocalizacion;
        private Mock<IListaAmigosServicio> _mockListaAmigos;
        private Mock<IAmigosServicio> _mockAmigosServicio;
        private Mock<ISalasServicio> _mockSalasServicio;
        private Mock<MusicaManejador> _mockServicioMusica;

        private VentanaPrincipalVistaModelo _viewModel;
        private const string UsuarioTest = "UsuarioPrueba";

        [TestInitialize]
        public void Inicializar()
        {
            _mockLocalizacion = new Mock<ILocalizacionServicio>();
            _mockListaAmigos = new Mock<IListaAmigosServicio>();
            _mockAmigosServicio = new Mock<IAmigosServicio>();
            _mockSalasServicio = new Mock<ISalasServicio>();
            _mockServicioMusica = new Mock<MusicaManejador> { CallBase = true };

            _mockLocalizacion.Setup(l => l.CulturaActual).Returns(new CultureInfo("es-MX"));
            _mockListaAmigos.Setup(l => l.ListaActual).Returns(new List<AmigoDTO>());
            _mockAmigosServicio.Setup(s => s.SolicitudesPendientes).Returns(new List<SolicitudAmistadDTO>()); 

            var nombreUsuarioField = typeof(VentanaPrincipalVistaModelo).GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance);

            _viewModel = new VentanaPrincipalVistaModelo(
                _mockLocalizacion.Object,
                _mockListaAmigos.Object,
                _mockAmigosServicio.Object,
                _mockSalasServicio.Object,
                _mockServicioMusica.Object
            );

            nombreUsuarioField.SetValue(_viewModel, UsuarioTest);

            _viewModel.MostrarMensaje = (_) => { };
            _viewModel.ConfirmarEliminarAmigo = (_) => true;
            _viewModel.UnirseSala = (_) => { };
            _viewModel.IniciarJuego = (_) => { };
        }

        [TestCleanup]
        public void Limpiar()
        {
            if (_viewModel != null)
            {
                typeof(VentanaPrincipalVistaModelo).GetField("_abrioVentanaJuego", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(_viewModel, false);
                _viewModel.FinalizarAsync().Wait();
            }
            _viewModel = null;
        }

        #region Constructor y Validaciones

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_LocalizacionNula_LanzaExcepcion()
        {
            new VentanaPrincipalVistaModelo(null, _mockListaAmigos.Object, _mockAmigosServicio.Object, _mockSalasServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_ListaAmigosNula_LanzaExcepcion()
        {
            new VentanaPrincipalVistaModelo(_mockLocalizacion.Object, null, _mockAmigosServicio.Object, _mockSalasServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_AmigosServicioNulo_LanzaExcepcion()
        {
            new VentanaPrincipalVistaModelo(_mockLocalizacion.Object, _mockListaAmigos.Object, null, _mockSalasServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_SalasServicioNulo_LanzaExcepcion()
        {
            new VentanaPrincipalVistaModelo(_mockLocalizacion.Object, _mockListaAmigos.Object, _mockAmigosServicio.Object, null);
        }

        [TestMethod]
        public void Prueba_Constructor_InicializaListasYOpcionesCorrectamente()
        {
            Assert.IsNotNull(_viewModel.Amigos);
            Assert.IsNotNull(_viewModel.NumeroRondasOpciones);
            Assert.IsNotNull(_viewModel.TiempoRondaOpciones);
            Assert.IsNotNull(_viewModel.IdiomasDisponibles);
            Assert.IsNotNull(_viewModel.DificultadesDisponibles);

            Assert.AreEqual(3, _viewModel.NumeroRondasOpciones.Count);
            Assert.AreEqual(3, _viewModel.TiempoRondaOpciones.Count);
            Assert.AreEqual(3, _viewModel.DificultadesDisponibles.Count);

            Assert.IsNotNull(_viewModel.NumeroRondasSeleccionada);
            Assert.IsNotNull(_viewModel.TiempoRondaSeleccionada);
            Assert.IsNotNull(_viewModel.DificultadSeleccionada);
            Assert.IsNotNull(_viewModel.IdiomaSeleccionado);
        }

        #endregion

        #region Propiedades y Notificaciones

        [TestMethod]
        public void Prueba_CodigoSala_Setter_NotificaCambio()
        {
            bool notificado = false;
            _viewModel.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(_viewModel.CodigoSala)) notificado = true; };

            _viewModel.CodigoSala = "123456";

            Assert.IsTrue(notificado);
            Assert.AreEqual("123456", _viewModel.CodigoSala);
        }

        [TestMethod]
        public void Prueba_AmigoSeleccionado_Setter_GuardaValor()
        {
            var amigo = new AmigoDTO { NombreUsuario = "Juan" };
            _viewModel.AmigoSeleccionado = amigo;
            Assert.AreEqual(amigo, _viewModel.AmigoSeleccionado);
        }

        [TestMethod]
        public void Prueba_OpcionesJuego_Setters_ActualizanEstadoComando()
        {
            bool notificado = false;
            _viewModel.IniciarJuegoComando.CanExecuteChanged += (s, e) => notificado = true;

            var nuevaOpcionRonda = _viewModel.NumeroRondasOpciones.Last();
            _viewModel.NumeroRondasSeleccionada = nuevaOpcionRonda;

            Assert.IsTrue(notificado);
            Assert.AreEqual(nuevaOpcionRonda, _viewModel.NumeroRondasSeleccionada);
        }

        #endregion

        #region Comandos de Navegación (Delegados)

        [TestMethod]
        public void Prueba_ComandosNavegacion_InvocanAcciones()
        {
            bool perfilAbierto = false;
            bool ajustesAbierto = false;
            bool buscarAmigoAbierto = false;

            _viewModel.AbrirPerfil = () => perfilAbierto = true;
            _viewModel.AbrirAjustes = () => ajustesAbierto = true;
            _viewModel.AbrirBuscarAmigo = () => buscarAmigoAbierto = true;

            _viewModel.AbrirPerfilComando.Execute(null);
            _viewModel.AbrirAjustesComando.Execute(null);
            _viewModel.AbrirBuscarAmigoComando.Execute(null);

            Assert.IsTrue(perfilAbierto);
            Assert.IsTrue(ajustesAbierto);
            Assert.IsTrue(buscarAmigoAbierto);
        }

        [TestMethod]
        public void Prueba_AbrirSolicitudes_SinSolicitudes_MuestraMensajeError()
        {
            _mockAmigosServicio.Setup(s => s.SolicitudesPendientes).Returns(new List<SolicitudAmistadDTO>());

            string msj = null;
            _viewModel.MostrarMensaje = (m) => msj = m;

            _viewModel.AbrirSolicitudesComando.Execute(null);

            Assert.AreEqual(Lang.amigosAvisoSinSolicitudesPendientes, msj);
            _mockAmigosServicio.Verify(s => s.SolicitudesPendientes, Times.Once);
        }

        [TestMethod]
        public void Prueba_AbrirSolicitudes_ConSolicitudes_InvocaAbrirSolicitudes()
        {
            _mockAmigosServicio.Setup(s => s.SolicitudesPendientes).Returns(new List<SolicitudAmistadDTO> { new SolicitudAmistadDTO() });

            bool solicitudesAbierto = false;
            _viewModel.AbrirSolicitudes = () => solicitudesAbierto = true;

            _viewModel.AbrirSolicitudesComando.Execute(null);

            Assert.IsTrue(solicitudesAbierto);
            _mockAmigosServicio.Verify(s => s.SolicitudesPendientes, Times.Once);
        }

        #endregion

        #region Gestión de Amigos (Eventos y Actualización)

        [TestMethod]
        public void Prueba_ListaActualizada_Evento_AgregaAmigos()
        {
            _viewModel.Amigos.Clear();

            var nuevosAmigos = new List<AmigoDTO>
            {
                new AmigoDTO { NombreUsuario = "Amigo1" },
                new AmigoDTO { NombreUsuario = "Amigo2" }
            };

            _mockListaAmigos.Raise(m => m.ListaActualizada += null, null, nuevosAmigos);

            Assert.AreEqual(2, _viewModel.Amigos.Count);
            Assert.IsTrue(_viewModel.Amigos.Any(a => a.NombreUsuario == "Amigo1"));
        }

        [TestMethod]
        public void Prueba_ListaActualizada_AmigoSeleccionadoDesaparece_LimpiaSeleccion()
        {
            var amigo = new AmigoDTO { NombreUsuario = "AmigoBorrado" };
            _viewModel.AmigoSeleccionado = amigo;

            _mockListaAmigos.Raise(m => m.ListaActualizada += null, null, new List<AmigoDTO>());

            Assert.IsNull(_viewModel.AmigoSeleccionado);
        }

        [TestMethod]
        public void Prueba_ListaActualizada_AmigoSeleccionadoPermanece_MantieneSeleccion()
        {
            var amigo = new AmigoDTO { NombreUsuario = "AmigoFiel" };
            _viewModel.AmigoSeleccionado = amigo;
            var listaNueva = new List<AmigoDTO> { new AmigoDTO { NombreUsuario = "AmigoFiel" } };

            _mockListaAmigos.Raise(m => m.ListaActualizada += null, null, listaNueva);

            Assert.IsNotNull(_viewModel.AmigoSeleccionado);
            Assert.AreEqual("AmigoFiel", _viewModel.AmigoSeleccionado.NombreUsuario);
        }

        #endregion

        #region Comando Eliminar Amigo

        [TestMethod]
        public async Task Prueba_EliminarAmigo_ConfirmacionCancelada_NoLlamaServicio()
        {
            _viewModel.ConfirmarEliminarAmigo = (nombre) => false;
            var amigo = new AmigoDTO { NombreUsuario = "Amigo1" };

            await _viewModel.EliminarAmigoComando.EjecutarAsync(amigo);

            _mockAmigosServicio.Verify(s => s.EliminarAmigoAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_EliminarAmigo_UsuarioSesionVacio_MuestraError()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_viewModel, "");

            string mensaje = null;
            _viewModel.MostrarMensaje = (m) => mensaje = m;
            _viewModel.ConfirmarEliminarAmigo = (_) => true;

            await _viewModel.EliminarAmigoComando.EjecutarAsync(new AmigoDTO { NombreUsuario = "X" });

            Assert.AreEqual(Lang.errorTextoErrorProcesarSolicitud, mensaje);
            _mockAmigosServicio.Verify(s => s.EliminarAmigoAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_EliminarAmigo_Exito_LlamaServicioYMensaje()
        {
            _viewModel.ConfirmarEliminarAmigo = (_) => true;
            string mensaje = null;
            _viewModel.MostrarMensaje = (m) => mensaje = m;
            var amigo = new AmigoDTO { NombreUsuario = "AmigoX" };

            await _viewModel.EliminarAmigoComando.EjecutarAsync(amigo);

            _mockAmigosServicio.Verify(s => s.EliminarAmigoAsync(UsuarioTest, "AmigoX"), Times.Once);
            Assert.AreEqual(Lang.amigosTextoAmigoEliminado, mensaje);
        }

        [TestMethod]
        public async Task Prueba_EliminarAmigo_Excepcion_MuestraError()
        {
            _viewModel.ConfirmarEliminarAmigo = (_) => true;
            string mensaje = null;
            _viewModel.MostrarMensaje = (m) => mensaje = m;

            _mockAmigosServicio.Setup(s => s.EliminarAmigoAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorWCF", null));

            await _viewModel.EliminarAmigoComando.EjecutarAsync(new AmigoDTO { NombreUsuario = "A" });

            Assert.AreEqual("ErrorWCF", mensaje);
        }

        #endregion

        #region Comando Unirse a Sala

        [TestMethod]
        public async Task Prueba_UnirseSala_CodigoVacio_MuestraError()
        {
            _viewModel.CodigoSala = "   ";
            string mensaje = null;
            _viewModel.MostrarMensaje = (m) => mensaje = m;

            await _viewModel.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.globalTextoIngreseCodigoPartida, mensaje);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_Exito_InvocaAccionYEjecutaTransicion()
        {
            _viewModel.CodigoSala = "123456";

            SalaDTO salaRetornada = new SalaDTO { Codigo = "123456" };
            _mockSalasServicio.Setup(s => s.UnirseSalaAsync("123456", UsuarioTest)).ReturnsAsync(salaRetornada);

            SalaDTO salaRecibida = null;
            _viewModel.UnirseSala = (s) => salaRecibida = s;

            await _viewModel.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(salaRetornada, salaRecibida);
            _mockServicioMusica.Verify(m => m.Detener(), Times.Once);
            _mockServicioMusica.As<IDisposable>().Verify(m => m.Dispose(), Times.Once);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_SalaLlena_MuestraMensajeEspecifico()
        {
            _viewModel.CodigoSala = "123456";
            string mensaje = null;
            _viewModel.MostrarMensaje = (m) => mensaje = m;

            var ex = new ServicioExcepcion(TipoErrorServicio.OperacionInvalida, Lang.errorTextoSalaLlena, null);
            _mockSalasServicio.Setup(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(ex);

            await _viewModel.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoSalaLlena, mensaje);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_ErrorGeneral_MuestraMensajeDefecto()
        {
            _viewModel.CodigoSala = "123456";
            string mensaje = null;
            _viewModel.MostrarMensaje = (m) => mensaje = m;

            var ex = new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorX", null);
            _mockSalasServicio.Setup(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(ex);

            await _viewModel.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoNoEncuentraPartida, mensaje);
        }

        #endregion

        #region Comando Iniciar Juego (Crear Sala)

        [TestMethod]
        public async Task Prueba_IniciarJuego_OpcionesNulas_MuestraError()
        {
            _viewModel.NumeroRondasSeleccionada = null;
            _viewModel.TiempoRondaSeleccionada = null;

            string mensajeCapturado = null;
            _viewModel.MostrarMensaje = (m) => mensajeCapturado = m;

            await _viewModel.IniciarJuegoComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoErrorProcesarSolicitud, mensajeCapturado);
            _mockSalasServicio.Verify(s => s.CrearSalaAsync(It.IsAny<string>(), It.IsAny<ConfiguracionPartidaDTO>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_IniciarJuego_Exito_CreaSalaEInvocaAccion()
        {
            SalaDTO salaCreada = new SalaDTO { Codigo = "789123" };
            _mockSalasServicio.Setup(s => s.CrearSalaAsync(UsuarioTest, It.IsAny<ConfiguracionPartidaDTO>())).ReturnsAsync(salaCreada);

            SalaDTO salaRecibida = null;
            _viewModel.IniciarJuego = (s) => salaRecibida = s; 

            await _viewModel.IniciarJuegoComando.EjecutarAsync(null);

            Assert.AreEqual(salaCreada, salaRecibida);
            _mockServicioMusica.Verify(m => m.Detener(), Times.Once);
            _mockServicioMusica.As<IDisposable>().Verify(m => m.Dispose(), Times.Once);
        }

        [TestMethod]
        public async Task Prueba_IniciarJuego_Excepcion_MuestraError()
        {
            string mensaje = null;
            _viewModel.MostrarMensaje = (m) => mensaje = m;

            _mockSalasServicio.Setup(s => s.CrearSalaAsync(It.IsAny<string>(), It.IsAny<ConfiguracionPartidaDTO>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "FalloCreacion", null));

            await _viewModel.IniciarJuegoComando.EjecutarAsync(null);

            Assert.AreEqual("FalloCreacion", mensaje);
        }

        #endregion

        #region Ciclo de Vida (Finalizar y Dispose)

        [TestMethod]
        public async Task Prueba_FinalizarAsync_CancelaSuscripciones()
        {
            await _viewModel.FinalizarAsync();

            _mockListaAmigos.Verify(s => s.CancelarSuscripcionAsync(UsuarioTest), Times.Once);
            _mockAmigosServicio.Verify(s => s.CancelarSuscripcionAsync(UsuarioTest), Times.Once);
        }

        [TestMethod]
        public async Task Prueba_FinalizarAsync_DisponeServiciosSiNoHuboJuego()
        {
            await _viewModel.FinalizarAsync();

            _mockListaAmigos.As<IDisposable>().Verify(d => d.Dispose(), Times.Once);
            _mockAmigosServicio.As<IDisposable>().Verify(d => d.Dispose(), Times.Once);
            _mockSalasServicio.As<IDisposable>().Verify(d => d.Dispose(), Times.Once);
        }

        [TestMethod]
        public async Task Prueba_FinalizarAsync_NoDisponeSalasServicioSiHuboJuego()
        {
            var sala = new DTOs.SalaDTO { Codigo = "TEMP" };
            var metodo = typeof(VentanaPrincipalVistaModelo).GetMethod("EjecutarTransicionAJuego", BindingFlags.NonPublic | BindingFlags.Instance);
            metodo.Invoke(_viewModel, new object[] { sala });

            await _viewModel.FinalizarAsync();

            _mockListaAmigos.As<IDisposable>().Verify(d => d.Dispose(), Times.Once);
            _mockAmigosServicio.As<IDisposable>().Verify(d => d.Dispose(), Times.Once);

            _mockSalasServicio.As<IDisposable>().Verify(d => d.Dispose(), Times.Never);
        }

        #endregion
    }
}