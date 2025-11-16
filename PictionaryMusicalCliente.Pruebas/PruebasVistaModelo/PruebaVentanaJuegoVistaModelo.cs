using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo
{
    [TestClass]
    public class PruebaVentanaJuegoVistaModelo
    {
        private Mock<ISalasServicio> _mockSalasServicio;
        private Mock<IInvitacionesServicio> _mockInvitacionesServicio;
        private Mock<IListaAmigosServicio> _mockListaAmigosServicio;
        private Mock<IPerfilServicio> _mockPerfilServicio;
        private SalaDTO _salaDummy;
        private VentanaJuegoVistaModelo _viewModel;

        private const string UsuarioTest = "UsuarioPrueba";

        [TestInitialize]
        public void Inicializar()
        {
            _mockSalasServicio = new Mock<ISalasServicio>();
            _mockInvitacionesServicio = new Mock<IInvitacionesServicio>();
            _mockListaAmigosServicio = new Mock<IListaAmigosServicio>();
            _mockPerfilServicio = new Mock<IPerfilServicio>();

            _salaDummy = new SalaDTO
            {
                Codigo = "123456",
                Creador = "Creador",
                Jugadores = new[] { "Creador", "OtroJugador" }
            };

            _viewModel = new VentanaJuegoVistaModelo(
                _salaDummy,
                _mockSalasServicio.Object,
                _mockInvitacionesServicio.Object,
                _mockListaAmigosServicio.Object,
                _mockPerfilServicio.Object,
                nombreJugador: UsuarioTest,
                esInvitado: false
            );

            _viewModel.MostrarMensaje = (_) => { };
            _viewModel.ManejarExpulsion = (_) => { };
            _viewModel.MostrarConfirmacion = (_) => true; 
        }

        [TestCleanup]
        public void Limpiar()
        {
            if (_viewModel is IDisposable disposableVm)
            {
                disposableVm.Dispose();
            }
            _viewModel = null;
        }

        #region Constructor y Validaciones Nulas

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_SalaNula_LanzaExcepcion()
        {
            new VentanaJuegoVistaModelo(null, _mockSalasServicio.Object, _mockInvitacionesServicio.Object, _mockListaAmigosServicio.Object, _mockPerfilServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_SalasServicioNulo_LanzaExcepcion()
        {
            new VentanaJuegoVistaModelo(_salaDummy, null, _mockInvitacionesServicio.Object, _mockListaAmigosServicio.Object, _mockPerfilServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_InvitacionesServicioNulo_LanzaExcepcion()
        {
            new VentanaJuegoVistaModelo(_salaDummy, _mockSalasServicio.Object, null, _mockListaAmigosServicio.Object, _mockPerfilServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_ListaAmigosServicioNulo_LanzaExcepcion()
        {
            new VentanaJuegoVistaModelo(_salaDummy, _mockSalasServicio.Object, _mockInvitacionesServicio.Object, null, _mockPerfilServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_PerfilServicioNulo_LanzaExcepcion()
        {
            new VentanaJuegoVistaModelo(_salaDummy, _mockSalasServicio.Object, _mockInvitacionesServicio.Object, _mockListaAmigosServicio.Object, null);
        }

        #endregion

        #region Estado Inicial y Propiedades

        [TestMethod]
        public void Prueba_Constructor_InicializacionCorrecta_EstableceValoresPorDefecto()
        {
            Assert.AreEqual("123456", _viewModel.CodigoSala);
            Assert.IsTrue(_viewModel.BotonIniciarPartidaHabilitado);
            Assert.AreEqual(6, _viewModel.Grosor);
            Assert.IsTrue(_viewModel.PuedeInvitarPorCorreo);
            Assert.IsNotNull(_viewModel.Jugadores);
            Assert.AreEqual(2, _viewModel.Jugadores.Count);
            Assert.IsNotNull(_viewModel.InvitarCorreoComando);
            Assert.IsNotNull(_viewModel.InvitarAmigosComando);
        }

        [TestMethod]
        public void Prueba_Constructor_EsInvitadoTrue_DeshabilitaInvitaciones()
        {
            var vmInvitado = new VentanaJuegoVistaModelo(
                _salaDummy,
                _mockSalasServicio.Object,
                _mockInvitacionesServicio.Object,
                _mockListaAmigosServicio.Object,
                _mockPerfilServicio.Object,
                nombreJugador: UsuarioTest,
                esInvitado: true
            );

            Assert.IsTrue(vmInvitado.EsInvitado);
            Assert.IsFalse(vmInvitado.PuedeInvitarPorCorreo);
            Assert.IsFalse(vmInvitado.PuedeInvitarAmigos);
            if (vmInvitado is IDisposable d) d.Dispose();
        }

        [TestMethod]
        public void Prueba_PropiedadesVisuales_AsignacionYLectura_FuncionaCorrectamente()
        {
            _viewModel.VisibilidadOverlayDibujante = Visibility.Visible;
            Assert.AreEqual(Visibility.Visible, _viewModel.VisibilidadOverlayDibujante);

            _viewModel.VisibilidadOverlayAdivinador = Visibility.Visible;
            Assert.AreEqual(Visibility.Visible, _viewModel.VisibilidadOverlayAdivinador);

            _viewModel.VisibilidadInfoCancion = Visibility.Collapsed;
            Assert.AreEqual(Visibility.Collapsed, _viewModel.VisibilidadInfoCancion);

            _viewModel.VisibilidadPalabraAdivinar = Visibility.Hidden;
            Assert.AreEqual(Visibility.Hidden, _viewModel.VisibilidadPalabraAdivinar);

            _viewModel.VisibilidadCuadriculaDibujo = Visibility.Visible;
            Assert.AreEqual(Visibility.Visible, _viewModel.VisibilidadCuadriculaDibujo);

            _viewModel.TextoContador = "15";
            Assert.AreEqual("15", _viewModel.TextoContador);

            _viewModel.ColorContador = Brushes.Red;
            Assert.AreEqual(Brushes.Red, _viewModel.ColorContador);

            _viewModel.TextoArtista = "Test Artista";
            Assert.AreEqual("Test Artista", _viewModel.TextoArtista);

            _viewModel.TextoGenero = "Rock";
            Assert.AreEqual("Rock", _viewModel.TextoGenero);

            _viewModel.TextoBotonIniciarPartida = "Iniciar";
            Assert.AreEqual("Iniciar", _viewModel.TextoBotonIniciarPartida);

            _viewModel.PalabraAdivinar = "Test";
            Assert.AreEqual("Test", _viewModel.PalabraAdivinar);
        }

        #endregion

        #region Lógica de Herramientas de Dibujo

        [TestMethod]
        public void Prueba_HerramientasDibujo_CambioLapizBorrador_ActualizaEstados()
        {
            _viewModel.EsHerramientaBorrador = true;
            Assert.IsTrue(_viewModel.EsHerramientaBorrador);
            Assert.IsFalse(_viewModel.EsHerramientaLapiz);

            _viewModel.EsHerramientaLapiz = true;
            Assert.IsTrue(_viewModel.EsHerramientaLapiz);
            Assert.IsFalse(_viewModel.EsHerramientaBorrador);
        }

        [TestMethod]
        public void Prueba_EsHerramientaLapiz_AlEstablecerTrue_InvocaNotificacion()
        {
            bool notificado = false;
            _viewModel.NotificarCambioHerramienta = (v) => notificado = true;
            _viewModel.EsHerramientaBorrador = true;

            _viewModel.EsHerramientaLapiz = true;
            Assert.IsTrue(notificado, "Debe notificar cambio de herramienta");
        }

        [TestMethod]
        public void Prueba_EsHerramientaBorrador_AlEstablecerTrue_InvocaNotificacion()
        {
            bool notificado = false;
            _viewModel.NotificarCambioHerramienta = (v) => notificado = true;
            _viewModel.EsHerramientaLapiz = true;

            _viewModel.EsHerramientaBorrador = true;
            Assert.IsTrue(notificado, "Debe notificar cambio de herramienta");
        }

        [TestMethod]
        public void Prueba_CambiarGrosor_ValorValido_ActualizaPropiedad()
        {
            _viewModel.CambiarGrosorComando.Execute("15.5");
            Assert.AreEqual(15.5, _viewModel.Grosor);
        }

        [TestMethod]
        public void Prueba_CambiarGrosor_ValorNulo_NoHaceNada()
        {
            double original = _viewModel.Grosor;
            _viewModel.CambiarGrosorComando.Execute(null);
            Assert.AreEqual(original, _viewModel.Grosor);
        }

        [TestMethod]
        public void Prueba_CambiarGrosor_SiEsLapiz_AplicaEstilo()
        {
            bool estiloAplicado = false;
            _viewModel.AplicarEstiloLapiz = () => estiloAplicado = true;
            _viewModel.EsHerramientaLapiz = true;

            _viewModel.CambiarGrosorComando.Execute("10");
            Assert.IsTrue(estiloAplicado);
        }

        [TestMethod]
        public void Prueba_CambiarGrosor_SiEsBorrador_ActualizaFormaGoma()
        {
            bool gomaActualizada = false;
            _viewModel.ActualizarFormaGoma = () => gomaActualizada = true;
            _viewModel.EsHerramientaBorrador = true;

            _viewModel.CambiarGrosorComando.Execute("20");
            Assert.IsTrue(gomaActualizada);
        }

        [TestMethod]
        public void Prueba_CambiarColor_ValorValido_ActualizaColorYSeleccionaLapiz()
        {
            _viewModel.EsHerramientaBorrador = true;
            _viewModel.CambiarColorComando.Execute("Blue");

            Assert.AreEqual(Colors.Blue, _viewModel.Color);
            Assert.IsTrue(_viewModel.EsHerramientaLapiz);
        }

        [TestMethod]
        public void Prueba_LimpiarDibujo_InvocaAccion()
        {
            bool limpiado = false;
            _viewModel.LimpiarTrazos = () => limpiado = true;
            _viewModel.LimpiarDibujoComando.Execute(null);
            Assert.IsTrue(limpiado);
        }

        [TestMethod]
        public void Prueba_SeleccionarHerramientas_Comandos()
        {
            _viewModel.SeleccionarBorradorComando.Execute(null);
            Assert.IsTrue(_viewModel.EsHerramientaBorrador);

            _viewModel.SeleccionarLapizComando.Execute(null);
            Assert.IsTrue(_viewModel.EsHerramientaLapiz);
        }

        #endregion

        #region Lógica de Partida (Inicio y Overlays)

        [TestMethod]
        public void Prueba_AbrirAjustes_InvocaAccion()
        {
            bool ajustesAbiertos = false;
            _viewModel.AbrirAjustesPartida = (m) => ajustesAbiertos = true;
            _viewModel.AbrirAjustesComando.Execute(null);
            Assert.IsTrue(ajustesAbiertos);
        }

        [TestMethod]
        public void Prueba_IniciarPartida_EstadoInicial_ConfiguraInterfaz()
        {
            bool estiloAplicado = false;
            bool gomaActualizada = false;
            _viewModel.AplicarEstiloLapiz = () => estiloAplicado = true;
            _viewModel.ActualizarFormaGoma = () => gomaActualizada = true;

            _viewModel.IniciarPartidaComando.Execute(null);

            Assert.IsTrue(_viewModel.JuegoIniciado);
            Assert.AreEqual(Visibility.Visible, _viewModel.VisibilidadCuadriculaDibujo);
            Assert.IsTrue(_viewModel.EsHerramientaLapiz);
            Assert.IsFalse(_viewModel.BotonIniciarPartidaHabilitado);
            Assert.AreEqual(Lang.partidaTextoPartidaEnCurso, _viewModel.TextoBotonIniciarPartida);
            Assert.IsTrue(estiloAplicado);
            Assert.IsTrue(gomaActualizada);
        }

        [TestMethod]
        public void Prueba_IniciarPartida_SiYaIniciado_NoHaceNada()
        {
            _viewModel.IniciarPartidaComando.Execute(null);
            bool estiloAplicado = false;
            _viewModel.AplicarEstiloLapiz = () => estiloAplicado = true;

            _viewModel.IniciarPartidaComando.Execute(null);

            Assert.IsFalse(estiloAplicado);
        }

        [TestMethod]
        public void Prueba_MostrarOverlays_Comandos_CambianVisibilidad()
        {
            _viewModel.MostrarOverlayDibujanteComando.Execute(null);
            Assert.AreEqual(Visibility.Visible, _viewModel.VisibilidadOverlayDibujante);
            Assert.AreEqual(Visibility.Collapsed, _viewModel.VisibilidadOverlayAdivinador);

            _viewModel.MostrarOverlayAdivinadorComando.Execute(null);
            Assert.AreEqual(Visibility.Visible, _viewModel.VisibilidadOverlayAdivinador);
            Assert.AreEqual(Visibility.Collapsed, _viewModel.VisibilidadOverlayDibujante);

            _viewModel.CerrarOverlayComando.Execute(null);
            Assert.AreEqual(Visibility.Collapsed, _viewModel.VisibilidadOverlayDibujante);
            Assert.AreEqual(Visibility.Collapsed, _viewModel.VisibilidadOverlayAdivinador);
        }

        #endregion

        #region Pruebas de Timers (Reflection)

        [TestMethod]
        public void Prueba_OverlayTimer_Tick_DetieneTimerEIniciaTemporizador()
        {
            MethodInfo metodoTick = typeof(VentanaJuegoVistaModelo).GetMethod("OverlayTimer_Tick", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(metodoTick, "Método OverlayTimer_Tick no encontrado");

            metodoTick.Invoke(_viewModel, new object[] { null, EventArgs.Empty });

            Assert.AreEqual("30", _viewModel.TextoContador);
            Assert.AreEqual(Visibility.Visible, _viewModel.VisibilidadPalabraAdivinar);
            Assert.AreEqual(Visibility.Visible, _viewModel.VisibilidadInfoCancion);
        }

        [TestMethod]
        public void Prueba_Temporizador_Tick_DisminuyeContador()
        {
            MethodInfo iniciarTemp = typeof(VentanaJuegoVistaModelo).GetMethod("IniciarTemporizador", BindingFlags.NonPublic | BindingFlags.Instance);
            iniciarTemp.Invoke(_viewModel, null);

            MethodInfo tickTemp = typeof(VentanaJuegoVistaModelo).GetMethod("Temporizador_Tick", BindingFlags.NonPublic | BindingFlags.Instance);
            tickTemp.Invoke(_viewModel, new object[] { null, EventArgs.Empty });

            Assert.AreEqual("29", _viewModel.TextoContador);
        }

        [TestMethod]
        public void Prueba_Temporizador_Tick_FinDelTiempo_MuestraMensaje()
        {
            string mensaje = null;
            _viewModel.MostrarMensaje = (m) => mensaje = m;

            FieldInfo campoContador = typeof(VentanaJuegoVistaModelo).GetField("_contador", BindingFlags.NonPublic | BindingFlags.Instance);
            campoContador.SetValue(_viewModel, 1);

            MethodInfo tickTemp = typeof(VentanaJuegoVistaModelo).GetMethod("Temporizador_Tick", BindingFlags.NonPublic | BindingFlags.Instance);
            tickTemp.Invoke(_viewModel, new object[] { null, EventArgs.Empty });

            Assert.AreEqual("0", _viewModel.TextoContador);
            Assert.AreEqual("¡Tiempo terminado!", mensaje);
            Assert.AreEqual(Visibility.Collapsed, _viewModel.VisibilidadPalabraAdivinar);
        }

        #endregion

        #region Invitaciones Correo (Validaciones y Excepciones)

        [TestMethod]
        public void Prueba_InvitarCorreo_CorreoNulo_MuestraError()
        {
            _viewModel.CorreoInvitacion = null;
            string msj = null;
            _viewModel.MostrarMensaje = (m) => msj = m;

            _viewModel.InvitarCorreoComando.Execute(null);
            Assert.AreEqual(Lang.errorTextoCorreoInvalido, msj);
        }

        [TestMethod]
        public void Prueba_InvitarCorreo_CorreoInvalido_MuestraMensajeError()
        {
            _viewModel.CorreoInvitacion = "correo-sin-arroba";
            string mensajeMostrado = null;
            _viewModel.MostrarMensaje = (msj) => mensajeMostrado = msj;

            _viewModel.InvitarCorreoComando.Execute(null);
            Assert.IsNotNull(mensajeMostrado);
            _mockInvitacionesServicio.Verify(s => s.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_InvitarCorreo_ServicioExcepcion_CapturaError()
        {
            _viewModel.CorreoInvitacion = "test@correo.com";
            string mensajeMostrado = null;
            _viewModel.MostrarMensaje = (msj) => mensajeMostrado = msj;

            _mockInvitacionesServicio
                .Setup(s => s.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "Error WCF", null));

            _viewModel.InvitarCorreoComando.Execute(null);
            await Task.Delay(100);

            Assert.AreEqual("Error WCF", mensajeMostrado);
        }

        [TestMethod]
        public async Task Prueba_InvitarCorreo_ArgumentException_CapturaError()
        {
            _viewModel.CorreoInvitacion = "test@correo.com";
            string mensajeMostrado = null;
            _viewModel.MostrarMensaje = (msj) => mensajeMostrado = msj;

            _mockInvitacionesServicio
                .Setup(s => s.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Argumento malo"));

            _viewModel.InvitarCorreoComando.Execute(null);
            await Task.Delay(100);

            Assert.AreEqual("Argumento malo", mensajeMostrado);
        }

        [TestMethod]
        public async Task Prueba_InvitarCorreo_ResultadoFallido_MuestraMensajeServidor()
        {
            _viewModel.CorreoInvitacion = "test@correo.com";
            string mensajeMostrado = null;
            _viewModel.MostrarMensaje = (msj) => mensajeMostrado = msj;

            _mockInvitacionesServicio
                .Setup(s => s.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ResultadoOperacionDTO { OperacionExitosa = false, Mensaje = "ErrorLogico" });

            _viewModel.InvitarCorreoComando.Execute(null);
            await Task.Delay(100);

            Assert.IsNotNull(mensajeMostrado);
            Assert.AreNotEqual(Lang.invitarCorreoTextoEnviado, mensajeMostrado);
        }

        [TestMethod]
        public async Task Prueba_InvitarCorreo_Exito_LimpiaCampo()
        {
            _viewModel.CorreoInvitacion = "valido@correo.com";
            string mensajeMostrado = null;
            _viewModel.MostrarMensaje = (msj) => mensajeMostrado = msj;

            _mockInvitacionesServicio
                .Setup(s => s.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ResultadoOperacionDTO { OperacionExitosa = true });

            _viewModel.InvitarCorreoComando.Execute(null);
            await Task.Delay(100);

            Assert.AreEqual(Lang.invitarCorreoTextoEnviado, mensajeMostrado);
            Assert.AreEqual(string.Empty, _viewModel.CorreoInvitacion);
        }

        #endregion

        #region Invitaciones Amigos

        [TestMethod]
        public async Task Prueba_InvitarAmigos_ServiciosNulos_MuestraError()
        {
            var vmMalo = new VentanaJuegoVistaModelo(
                _salaDummy,
                _mockSalasServicio.Object,
                _mockInvitacionesServicio.Object, 
                _mockListaAmigosServicio.Object,
                _mockPerfilServicio.Object,
                nombreJugador: UsuarioTest
            );

            string msj = null;
            vmMalo.MostrarMensaje = (m) => msj = m;

            var flags = BindingFlags.NonPublic | BindingFlags.Instance;

            typeof(VentanaJuegoVistaModelo).GetField("_invitacionesServicio", flags)?.SetValue(vmMalo, null);
            typeof(VentanaJuegoVistaModelo).GetField("_listaAmigosServicio", flags)?.SetValue(vmMalo, null);
            typeof(VentanaJuegoVistaModelo).GetField("_perfilServicio", flags)?.SetValue(vmMalo, null);

            vmMalo.InvitarAmigosComando.Execute(null);
            await Task.Delay(50);

            Assert.AreEqual(Lang.errorTextoErrorProcesarSolicitud, msj);

            if (vmMalo is IDisposable d) d.Dispose();
        }

        [TestMethod]
        public async Task Prueba_InvitarAmigos_NombreUsuarioVacio_MuestraError()
        {
            var vmSinUser = new VentanaJuegoVistaModelo(_salaDummy, _mockSalasServicio.Object, _mockInvitacionesServicio.Object, _mockListaAmigosServicio.Object, _mockPerfilServicio.Object, nombreJugador: "");
            string msj = null;
            vmSinUser.MostrarMensaje = (m) => msj = m;

            vmSinUser.InvitarAmigosComando.Execute(null);
            await Task.Delay(50);

            Assert.AreEqual(Lang.errorTextoErrorProcesarSolicitud, msj);
            if (vmSinUser is IDisposable d) d.Dispose();
        }

        [TestMethod]
        public async Task Prueba_InvitarAmigos_ListaVacia_MuestraMensaje()
        {
            _mockListaAmigosServicio.Setup(s => s.ObtenerAmigosAsync(It.IsAny<string>())).ReturnsAsync(new List<AmigoDTO>());
            string msj = null;
            _viewModel.MostrarMensaje = (m) => msj = m;

            _viewModel.InvitarAmigosComando.Execute(null);
            await Task.Delay(50);

            Assert.AreEqual(Lang.invitarAmigosTextoSinAmigos, msj);
        }

        [TestMethod]
        public async Task Prueba_InvitarAmigos_Exito_AbreVentanaYAgregaInvitado()
        {
            int idAmigo = 10;
            var amigosMock = new List<AmigoDTO> { new AmigoDTO { NombreUsuario = "Amigo1", UsuarioId = idAmigo } };

            _mockListaAmigosServicio
                .Setup(s => s.ObtenerAmigosAsync(UsuarioTest))
                .ReturnsAsync(amigosMock);

            _mockInvitacionesServicio
                .Setup(s => s.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ResultadoOperacionDTO { OperacionExitosa = true });

            _mockPerfilServicio
                .Setup(s => s.ObtenerPerfilAsync(idAmigo))
                .ReturnsAsync(new UsuarioDTO { Correo = "amigo@prueba.com" });

            bool ventanaAbierta = false;
            InvitarAmigosVistaModelo vmAmigosCapturado = null;

            _viewModel.MostrarInvitarAmigos = (vm) =>
            {
                ventanaAbierta = true;
                vmAmigosCapturado = vm;
                return Task.CompletedTask;
            };

            _viewModel.InvitarAmigosComando.Execute(null);
            await Task.Delay(100);

            Assert.IsTrue(ventanaAbierta);
            Assert.IsNotNull(vmAmigosCapturado);

            var itemAmigo = vmAmigosCapturado.Amigos.First();

            itemAmigo.InvitarCommand.Execute(null);

            await Task.Delay(200);

            _mockInvitacionesServicio.Verify(
                s => s.EnviarInvitacionAsync(It.IsAny<string>(), "amigo@prueba.com"), 
                Times.AtLeastOnce,
                "El servicio no fue llamado. Posible causa: El perfil era nulo o el comando falló.");
        }

        #endregion

        #region Eventos de Sala y Actualización de Jugadores

        [TestMethod]
        public void Prueba_JugadorSeUnio_NombreVacio_NoHaceNada()
        {
            int countAntes = _viewModel.Jugadores.Count;
            _mockSalasServicio.Raise(m => m.JugadorSeUnio += null, null, "");
            Assert.AreEqual(countAntes, _viewModel.Jugadores.Count);
        }

        [TestMethod]
        public void Prueba_JugadorSeUnio_JugadorExistente_NoDuplica()
        {
            string jugador = "Creador";
            int countAntes = _viewModel.Jugadores.Count;
            _mockSalasServicio.Raise(m => m.JugadorSeUnio += null, null, jugador);
            Assert.AreEqual(countAntes, _viewModel.Jugadores.Count);
        }

        [TestMethod]
        public void Prueba_JugadorSeUnio_NuevoJugador_Agrega()
        {
            string nuevo = "Nuevo";
            _mockSalasServicio.Raise(m => m.JugadorSeUnio += null, null, nuevo);
            Assert.IsTrue(_viewModel.Jugadores.Any(j => j.Nombre == nuevo));
        }

        [TestMethod]
        public void Prueba_JugadorSalio_NombreVacio_NoHaceNada()
        {
            int countAntes = _viewModel.Jugadores.Count;
            _mockSalasServicio.Raise(m => m.JugadorSalio += null, null, "");
            Assert.AreEqual(countAntes, _viewModel.Jugadores.Count);
        }

        [TestMethod]
        public void Prueba_JugadorSalio_JugadorExistente_Elimina()
        {
            string salir = "OtroJugador";
            _mockSalasServicio.Raise(m => m.JugadorSalio += null, null, salir);
            Assert.IsFalse(_viewModel.Jugadores.Any(j => j.Nombre == salir));
        }

        [TestMethod]
        public void Prueba_ActualizarJugadores_ListaNula_NoFalla()
        {
            MethodInfo metodo = typeof(VentanaJuegoVistaModelo).GetMethod("ActualizarJugadores", BindingFlags.NonPublic | BindingFlags.Instance);
            metodo.Invoke(_viewModel, new object[] { null });

            Assert.AreEqual(0, _viewModel.Jugadores.Count);
        }

        #endregion

        #region Expulsión de Jugadores

        [TestMethod]
        public void Prueba_JugadorExpulsado_EsUsuarioActual_NavegaFuera()
        {
            bool navegacionInvocada = false;
            _viewModel.ManejarExpulsion = (destino) => navegacionInvocada = true;
            _mockSalasServicio.Raise(m => m.JugadorExpulsado += null, null, UsuarioTest);
            Assert.IsTrue(navegacionInvocada);
        }

        [TestMethod]
        public void Prueba_JugadorExpulsado_EsOtro_EliminaDeLista()
        {
            string otro = "OtroJugador";
            _mockSalasServicio.Raise(m => m.JugadorExpulsado += null, null, otro);
            Assert.IsFalse(_viewModel.Jugadores.Any(j => j.Nombre == otro));
        }

        [TestMethod]
        public async Task Prueba_EjecutarExpulsarJugador_ConfirmacionRechazada_NoExpulsa()
        {
            _viewModel.MostrarConfirmacion = (_) => false;

            _mockSalasServicio.Raise(m => m.JugadorSeUnio += null, null, "Victima");
            var jugadorElemento = _viewModel.Jugadores.First(j => j.Nombre == "Victima");

            jugadorElemento.ExpulsarComando.Execute(null);
            await Task.Delay(50);

            _mockSalasServicio.Verify(s => s.ExpulsarJugadorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Prueba_EjecutarExpulsarJugador_Exito_LlamaServicio()
        {
            _viewModel.MostrarConfirmacion = (_) => true;
            _mockSalasServicio.Raise(m => m.JugadorSeUnio += null, null, "Victima");
            var jugadorElemento = _viewModel.Jugadores.First(j => j.Nombre == "Victima");

            jugadorElemento.ExpulsarComando.Execute(null);
            await Task.Delay(50);

            _mockSalasServicio.Verify(s => s.ExpulsarJugadorAsync("123456", UsuarioTest, "Victima"), Times.Once);
        }

        [TestMethod]
        public async Task Prueba_EjecutarExpulsarJugador_Excepcion_CapturaError()
        {
            _viewModel.MostrarConfirmacion = (_) => true;
            _mockSalasServicio.Raise(m => m.JugadorSeUnio += null, null, "Victima");
            var jugadorElemento = _viewModel.Jugadores.First(j => j.Nombre == "Victima");

            _mockSalasServicio.Setup(s => s.ExpulsarJugadorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "Falla", null));

            string msj = null;
            _viewModel.MostrarMensaje = (m) => msj = m;

            jugadorElemento.ExpulsarComando.Execute(null);
            await Task.Delay(50);

            Assert.AreEqual("Falla", msj);
        }

        #endregion

        #region IComandoNotificable (Mock Manual)

        public class MockComandoNotificable : ICommand, IComandoNotificable
        {
            public event EventHandler CanExecuteChanged;
            public bool Notificado { get; private set; }

            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) { }

            public void NotificarPuedeEjecutar()
            {
                Notificado = true;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [TestMethod]
        public void Prueba_NotificarComando_SiEsNotificable_LlamaMetodo()
        {
            var mock = new MockComandoNotificable();

            MethodInfo metodo = typeof(VentanaJuegoVistaModelo).GetMethod("NotificarComando", BindingFlags.NonPublic | BindingFlags.Static);
            metodo.Invoke(null, new object[] { mock });

            Assert.IsTrue(mock.Notificado);
        }

        #endregion

        #region Finalización

        [TestMethod]
        public async Task Prueba_FinalizarAsync_LlamaAbandonarYDispose()
        {
            await _viewModel.FinalizarAsync();
            _mockSalasServicio.Verify(s => s.AbandonarSalaAsync("123456", UsuarioTest), Times.Once);
        }

        [TestMethod]
        public async Task Prueba_FinalizarAsync_SiFallaAbandonar_NoRompe()
        {
            _mockSalasServicio.Setup(s => s.AbandonarSalaAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Error de red"));

            await _viewModel.FinalizarAsync();
        }

        #endregion
    }
}