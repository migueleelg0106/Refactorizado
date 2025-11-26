using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using PictionaryMusicalCliente.VistaModelo.Salas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.VentanaJuego
{
    [TestClass]
    public class PruebaVentanaJuegoVistaModelo
    {
        private const string CodigoSalaTest = "CODIGO123";
        private const string UsuarioHost = "HostPrueba";

        private Mock<ISalasServicio> _mockSalasServicio;
        private Mock<IInvitacionesServicio> _mockInvitacionesServicio;
        private Mock<IListaAmigosServicio> _mockListaAmigosServicio;
        private Mock<IPerfilServicio> _mockPerfilServicio;

        private SalaVistaModelo _vistaModelo;
        private DTOs.SalaDTO _sala;
        private List<string> _mensajesMostrados;
        private SalaVistaModelo.DestinoNavegacion? _destinoNavegacion;
        private bool _cerrarAplicacionGlobal;

        [TestInitialize]
        public void Inicializar()
        {
            if (Application.Current == null)
            {
                new Application();
            }

            Application.ResourceAssembly = typeof(SalaVistaModelo).Assembly;

            _sala = new DTOs.SalaDTO
            {
                Codigo = CodigoSalaTest,
                Creador = UsuarioHost,
                Jugadores = new List<string> { UsuarioHost }
            };

            _mockSalasServicio = new Mock<ISalasServicio>();
            _mockInvitacionesServicio = new Mock<IInvitacionesServicio>();
            _mockListaAmigosServicio = new Mock<IListaAmigosServicio>();
            _mockPerfilServicio = new Mock<IPerfilServicio>();

            _mensajesMostrados = new List<string>();
            _destinoNavegacion = null;
            _cerrarAplicacionGlobal = false;

            _vistaModelo = new SalaVistaModelo(
                _sala,
                _mockSalasServicio.Object,
                _mockInvitacionesServicio.Object,
                _mockListaAmigosServicio.Object,
                _mockPerfilServicio.Object,
                UsuarioHost,
                false);

            _vistaModelo.MostrarMensaje = m => _mensajesMostrados.Add(m);
            _vistaModelo.ManejarNavegacion = d => _destinoNavegacion = d;
            _vistaModelo.ChequearCierreAplicacionGlobal = () => _cerrarAplicacionGlobal;
        }

        [TestCleanup]
        public void Limpiar()
        {
            _vistaModelo = null;
            _sala = null;
            _mensajesMostrados = null;
        }

        #region Metodos de ayuda

        private void EstablecerCampoPrivado<T>(string nombreCampo, T valor)
        {
            var campo = typeof(SalaVistaModelo).GetField(
                nombreCampo,
                BindingFlags.Instance | BindingFlags.NonPublic);

            campo.SetValue(_vistaModelo, valor);
        }

        private T ObtenerCampoPrivado<T>(string nombreCampo)
        {
            var campo = typeof(SalaVistaModelo).GetField(
                nombreCampo,
                BindingFlags.Instance | BindingFlags.NonPublic);

            return (T)campo.GetValue(_vistaModelo);
        }

        private void InvocarMetodoPrivado(string nombreMetodo, params object[] parametros)
        {
            var metodo = typeof(SalaVistaModelo).GetMethod(
                nombreMetodo,
                BindingFlags.Instance | BindingFlags.NonPublic);

            metodo.Invoke(_vistaModelo, parametros);
        }

        private async Task InvocarMetodoPrivadoAsync(string nombreMetodo, params object[] parametros)
        {
            var metodo = typeof(SalaVistaModelo).GetMethod(
                nombreMetodo,
                BindingFlags.Instance | BindingFlags.NonPublic);

            var resultado = metodo.Invoke(_vistaModelo, parametros);

            if (resultado is Task tarea)
            {
                await tarea.ConfigureAwait(false);
            }
        }

        #endregion

        #region 1. Constructores y estado inicial

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_SalaNula_LanzaExcepcion()
        {
            new SalaVistaModelo(
                null,
                _mockSalasServicio.Object,
                _mockInvitacionesServicio.Object,
                _mockListaAmigosServicio.Object,
                _mockPerfilServicio.Object,
                UsuarioHost,
                false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_SalasServicioNulo_LanzaExcepcion()
        {
            new SalaVistaModelo(
                _sala,
                null,
                _mockInvitacionesServicio.Object,
                _mockListaAmigosServicio.Object,
                _mockPerfilServicio.Object,
                UsuarioHost,
                false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_InvitacionesServicioNulo_LanzaExcepcion()
        {
            new SalaVistaModelo(
                _sala,
                _mockSalasServicio.Object,
                null,
                _mockListaAmigosServicio.Object,
                _mockPerfilServicio.Object,
                UsuarioHost,
                false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_ListaAmigosServicioNulo_LanzaExcepcion()
        {
            new SalaVistaModelo(
                _sala,
                _mockSalasServicio.Object,
                _mockInvitacionesServicio.Object,
                null,
                _mockPerfilServicio.Object,
                UsuarioHost,
                false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_PerfilServicioNulo_LanzaExcepcion()
        {
            new SalaVistaModelo(
                _sala,
                _mockSalasServicio.Object,
                _mockInvitacionesServicio.Object,
                _mockListaAmigosServicio.Object,
                null,
                UsuarioHost,
                false);
        }

        [TestMethod]
        public void Prueba_Constructor_InicializacionCorrecta()
        {
            Assert.IsFalse(_vistaModelo.JuegoIniciado);
            Assert.AreEqual(6, _vistaModelo.Grosor, 0.0001);
            Assert.AreEqual(Colors.Black, _vistaModelo.Color);
            Assert.AreEqual("30", _vistaModelo.TextoContador);
            Assert.AreEqual(Brushes.Black, _vistaModelo.ColorContador);
            Assert.IsTrue(_vistaModelo.EsHerramientaLapiz);
            Assert.IsFalse(_vistaModelo.EsHerramientaBorrador);

            Assert.AreEqual(Visibility.Collapsed, _vistaModelo.VisibilidadCuadriculaDibujo);
            Assert.AreEqual(Visibility.Collapsed, _vistaModelo.VisibilidadOverlayDibujante);
            Assert.AreEqual(Visibility.Collapsed, _vistaModelo.VisibilidadOverlayAdivinador);
            Assert.AreEqual(Visibility.Collapsed, _vistaModelo.VisibilidadPalabraAdivinar);
            Assert.AreEqual(Visibility.Collapsed, _vistaModelo.VisibilidadInfoCancion);

            Assert.AreEqual(Lang.partidaAdminTextoIniciarPartida, _vistaModelo.TextoBotonIniciarPartida);
            Assert.IsTrue(_vistaModelo.BotonIniciarPartidaHabilitado);

            Assert.AreEqual(CodigoSalaTest, _vistaModelo.CodigoSala);
            Assert.IsNotNull(_vistaModelo.Jugadores);
            Assert.AreEqual(1, _vistaModelo.Jugadores.Count);
            Assert.AreEqual(UsuarioHost, _vistaModelo.Jugadores[0].Nombre);

            Assert.IsNotNull(_vistaModelo.InvitarCorreoComando);
            Assert.IsNotNull(_vistaModelo.InvitarAmigosComando);
            Assert.IsNotNull(_vistaModelo.AbrirAjustesComando);
            Assert.IsNotNull(_vistaModelo.IniciarPartidaComando);
            Assert.IsNotNull(_vistaModelo.SeleccionarLapizComando);
            Assert.IsNotNull(_vistaModelo.SeleccionarBorradorComando);
            Assert.IsNotNull(_vistaModelo.CambiarGrosorComando);
            Assert.IsNotNull(_vistaModelo.CambiarColorComando);
            Assert.IsNotNull(_vistaModelo.LimpiarDibujoComando);
            Assert.IsNotNull(_vistaModelo.CerrarVentanaComando);
        }

        #endregion

        #region 2. Herramientas de dibujo y comandos basicos

        [TestMethod]
        public void Prueba_SeleccionarLapiz_Comando_ActivaLapizYDesactivaBorrador()
        {
            EstablecerCampoPrivado("_esHerramientaLapiz", false);
            EstablecerCampoPrivado("_esHerramientaBorrador", true);

            bool notificacionRecibida = false;
            bool? ultimoValor = null;

            _vistaModelo.NotificarCambioHerramienta = valor =>
            {
                notificacionRecibida = true;
                ultimoValor = valor;
            };

            _vistaModelo.SeleccionarLapizComando.Execute(null);

            Assert.IsTrue(_vistaModelo.EsHerramientaLapiz);
            Assert.IsFalse(_vistaModelo.EsHerramientaBorrador);
            Assert.IsTrue(notificacionRecibida);
            Assert.IsTrue(ultimoValor.HasValue && ultimoValor.Value);
        }

        [TestMethod]
        public void Prueba_SeleccionarBorrador_Comando_ActivaBorradorYDesactivaLapiz()
        {
            EstablecerCampoPrivado("_esHerramientaLapiz", true);
            EstablecerCampoPrivado("_esHerramientaBorrador", false);

            bool notificacionRecibida = false;
            bool? ultimoValor = null;

            _vistaModelo.NotificarCambioHerramienta = valor =>
            {
                notificacionRecibida = true;
                ultimoValor = valor;
            };

            _vistaModelo.SeleccionarBorradorComando.Execute(null);

            Assert.IsFalse(_vistaModelo.EsHerramientaLapiz);
            Assert.IsTrue(_vistaModelo.EsHerramientaBorrador);
            Assert.IsTrue(notificacionRecibida);
            Assert.IsFalse(ultimoValor ?? true);
        }

        [TestMethod]
        public void Prueba_CambiarGrosor_ConLapiz_AplicaEstiloLapiz()
        {
            bool aplicarLapizInvocado = false;
            bool actualizarGomaInvocado = false;

            _vistaModelo.AplicarEstiloLapiz = () => aplicarLapizInvocado = true;
            _vistaModelo.ActualizarFormaGoma = () => actualizarGomaInvocado = true;

            _vistaModelo.EsHerramientaLapiz = true;
            _vistaModelo.EsHerramientaBorrador = false;

            _vistaModelo.CambiarGrosorComando.Execute("8.5");

            Assert.AreEqual(8.5, _vistaModelo.Grosor, 0.0001);
            Assert.IsTrue(aplicarLapizInvocado);
            Assert.IsFalse(actualizarGomaInvocado);
        }

        [TestMethod]
        public void Prueba_CambiarGrosor_ConBorrador_ActualizaFormaGoma()
        {
            bool aplicarLapizInvocado = false;
            bool actualizarGomaInvocado = false;

            _vistaModelo.AplicarEstiloLapiz = () => aplicarLapizInvocado = true;
            _vistaModelo.ActualizarFormaGoma = () => actualizarGomaInvocado = true;

            _vistaModelo.EsHerramientaLapiz = false;
            _vistaModelo.EsHerramientaBorrador = true;

            _vistaModelo.CambiarGrosorComando.Execute("3");

            Assert.AreEqual(3, _vistaModelo.Grosor, 0.0001);
            Assert.IsFalse(aplicarLapizInvocado);
            Assert.IsTrue(actualizarGomaInvocado);
        }

        [TestMethod]
        public void Prueba_CambiarColor_Comando_ActualizaColorYActivaLapiz()
        {
            bool aplicarLapizInvocado = false;
            _vistaModelo.AplicarEstiloLapiz = () => aplicarLapizInvocado = true;

            _vistaModelo.CambiarColorComando.Execute("Red");

            Assert.AreEqual(Colors.Red, _vistaModelo.Color);
            Assert.IsTrue(_vistaModelo.EsHerramientaLapiz);
            Assert.IsTrue(aplicarLapizInvocado);
        }

        [TestMethod]
        public void Prueba_LimpiarDibujo_Comando_LlamaAccion()
        {
            bool limpiarInvocado = false;
            _vistaModelo.LimpiarTrazos = () => limpiarInvocado = true;

            _vistaModelo.LimpiarDibujoComando.Execute(null);

            Assert.IsTrue(limpiarInvocado);
        }

        [TestMethod]
        public void Prueba_AbrirAjustes_Comando_InvocaAccionConManejadorCancion()
        {
            CancionManejador manejadorRecibido = null;

            _vistaModelo.AbrirAjustesPartida = m => manejadorRecibido = m;

            _vistaModelo.AbrirAjustesComando.Execute(null);

            Assert.IsNotNull(manejadorRecibido);
        }

        [TestMethod]
        public void Prueba_IniciarPartida_Comando_ModificaEstadoYNoRepite()
        {
            int llamadasLapiz = 0;
            int llamadasGoma = 0;

            _vistaModelo.AplicarEstiloLapiz = () => llamadasLapiz++;
            _vistaModelo.ActualizarFormaGoma = () => llamadasGoma++;

            _vistaModelo.IniciarPartidaComando.Execute(null);

            Assert.IsTrue(_vistaModelo.JuegoIniciado);
            Assert.AreEqual(Visibility.Visible, _vistaModelo.VisibilidadCuadriculaDibujo);
            Assert.AreEqual(Lang.partidaTextoPartidaEnCurso, _vistaModelo.TextoBotonIniciarPartida);
            Assert.IsFalse(_vistaModelo.BotonIniciarPartidaHabilitado);
            Assert.AreEqual(1, llamadasLapiz);
            Assert.AreEqual(1, llamadasGoma);

            _vistaModelo.IniciarPartidaComando.Execute(null);

            Assert.AreEqual(1, llamadasLapiz);
            Assert.AreEqual(1, llamadasGoma);
        }

        #endregion

        #region 3. Invitacion por correo

        [TestMethod]
        public async Task Prueba_InvitarCorreo_CorreoVacio_MuestraErrorYNoLlamaServicio()
        {
            _vistaModelo.CorreoInvitacion = "   ";
            _mensajesMostrados.Clear();

            var comando = (IComandoAsincrono)_vistaModelo.InvitarCorreoComando;

            await comando.EjecutarAsync(null);

            Assert.AreEqual(1, _mensajesMostrados.Count);
            Assert.AreEqual(Lang.errorTextoCorreoInvalido, _mensajesMostrados[0]);

            _mockInvitacionesServicio.Verify(
                s => s.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Prueba_InvitarCorreo_CorreoInvalido_NoLlamaServicio()
        {
            _vistaModelo.CorreoInvitacion = "correoInvalido";
            _mensajesMostrados.Clear();

            var comando = (IComandoAsincrono)_vistaModelo.InvitarCorreoComando;

            await comando.EjecutarAsync(null);

            Assert.AreEqual(1, _mensajesMostrados.Count);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_mensajesMostrados[0]));

            _mockInvitacionesServicio.Verify(
                s => s.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Prueba_InvitarCorreo_Exito_LimpiaCorreoYNotifica()
        {
            string correo = "correo@prueba.com";
            _vistaModelo.CorreoInvitacion = correo;
            _mensajesMostrados.Clear();

            var resultado = new DTOs.ResultadoOperacionDTO
            {
                OperacionExitosa = true,
                Mensaje = "OK"
            };

            _mockInvitacionesServicio.Setup(
                    s => s.EnviarInvitacionAsync(CodigoSalaTest, correo))
                .ReturnsAsync(resultado);

            var comando = (IComandoAsincrono)_vistaModelo.InvitarCorreoComando;

            await comando.EjecutarAsync(null);

            _mockInvitacionesServicio.Verify(
                s => s.EnviarInvitacionAsync(CodigoSalaTest, correo),
                Times.Once);

            Assert.AreEqual(string.Empty, _vistaModelo.CorreoInvitacion);
            Assert.AreEqual(1, _mensajesMostrados.Count);
            Assert.AreEqual(Lang.invitarCorreoTextoEnviado, _mensajesMostrados[0]);
        }

        [TestMethod]
        public async Task Prueba_InvitarCorreo_ErrorServicio_MuestraMensajeLocalizado()
        {
            string correo = "correo@prueba.com";
            _vistaModelo.CorreoInvitacion = correo;
            _mensajesMostrados.Clear();

            var resultado = new DTOs.ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = "COD_ERROR"
            };

            _mockInvitacionesServicio.Setup(
                    s => s.EnviarInvitacionAsync(CodigoSalaTest, correo))
                .ReturnsAsync(resultado);

            var comando = (IComandoAsincrono)_vistaModelo.InvitarCorreoComando;

            await comando.EjecutarAsync(null);

            _mockInvitacionesServicio.Verify(
                s => s.EnviarInvitacionAsync(CodigoSalaTest, correo),
                Times.Once);

            Assert.AreEqual(1, _mensajesMostrados.Count);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_mensajesMostrados[0]));
        }

        [TestMethod]
        public async Task Prueba_InvitarCorreo_ExcepcionServicio_MuestraMensajeExcepcion()
        {
            string correo = "correo@prueba.com";
            _vistaModelo.CorreoInvitacion = correo;
            _mensajesMostrados.Clear();

            _mockInvitacionesServicio.Setup(
                    s => s.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorWCF", null));

            var comando = (IComandoAsincrono)_vistaModelo.InvitarCorreoComando;

            await comando.EjecutarAsync(null);

            Assert.AreEqual("ErrorWCF", _mensajesMostrados.Single());
        }

        [TestMethod]
        public async Task Prueba_InvitarCorreo_ArgumentException_MuestraMensajeExcepcion()
        {
            string correo = "correo@prueba.com";
            _vistaModelo.CorreoInvitacion = correo;
            _mensajesMostrados.Clear();

            _mockInvitacionesServicio.Setup(
                    s => s.EnviarInvitacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("ErrorParametro"));

            var comando = (IComandoAsincrono)_vistaModelo.InvitarCorreoComando;

            await comando.EjecutarAsync(null);

            Assert.AreEqual("ErrorParametro", _mensajesMostrados.Single());
        }

        #endregion

        #region 4. Invitacion de amigos

        [TestMethod]
        public async Task Prueba_InvitarAmigos_UsuarioSesionVacio_MuestraError()
        {
            EstablecerCampoPrivado("_nombreUsuarioSesion", string.Empty);
            _mensajesMostrados.Clear();

            await _vistaModelo.InvitarAmigosComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoErrorProcesarSolicitud, _mensajesMostrados.Single());
            _mockListaAmigosServicio.Verify(
                s => s.ObtenerAmigosAsync(It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Prueba_InvitarAmigos_SinAmigos_MuestraMensajeSinAmigos()
        {
            var amigos = new List<DTOs.AmigoDTO>().AsReadOnly();

            _mockListaAmigosServicio.Setup(
                    s => s.ObtenerAmigosAsync(UsuarioHost))
                .ReturnsAsync(amigos);

            _mensajesMostrados.Clear();

            await _vistaModelo.InvitarAmigosComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.invitarAmigosTextoSinAmigos, _mensajesMostrados.Single());
        }

        [TestMethod]
        public async Task Prueba_InvitarAmigos_ExcepcionServicio_MuestraError()
        {
            _mockListaAmigosServicio.Setup(
                    s => s.ObtenerAmigosAsync(It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorLista", null));

            _mensajesMostrados.Clear();

            await _vistaModelo.InvitarAmigosComando.EjecutarAsync(null);

            Assert.AreEqual("ErrorLista", _mensajesMostrados.Single());
        }

        [TestMethod]
        public async Task Prueba_InvitarAmigos_ArgumentException_MuestraError()
        {
            _mockListaAmigosServicio.Setup(
                    s => s.ObtenerAmigosAsync(It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("ErrorArgumento"));

            _mensajesMostrados.Clear();

            await _vistaModelo.InvitarAmigosComando.EjecutarAsync(null);

            Assert.AreEqual("ErrorArgumento", _mensajesMostrados.Single());
        }

        [TestMethod]
        public async Task Prueba_InvitarAmigos_ConAmigos_MuestraVentanaInvitacion()
        {
            var amigos = new List<DTOs.AmigoDTO>
            {
                new DTOs.AmigoDTO()
            }.AsReadOnly();

            _mockListaAmigosServicio.Setup(
                    s => s.ObtenerAmigosAsync(UsuarioHost))
                .ReturnsAsync(amigos);

            InvitarAmigosVistaModelo vistaModeloRecibida = null;

            _vistaModelo.MostrarInvitarAmigos = vm =>
            {
                vistaModeloRecibida = vm;
                return Task.CompletedTask;
            };

            _mensajesMostrados.Clear();

            await _vistaModelo.InvitarAmigosComando.EjecutarAsync(null);

            Assert.IsNotNull(vistaModeloRecibida);
        }

        #endregion

        #region 5. Overlays y temporizador

        [TestMethod]
        public void Prueba_MostrarOverlayDibujante_Comando_ActivaOverlayCorrecto()
        {

            Assert.AreEqual(Visibility.Visible, _vistaModelo.VisibilidadOverlayDibujante);
            Assert.AreEqual(Visibility.Collapsed, _vistaModelo.VisibilidadOverlayAdivinador);
        }

        [TestMethod]
        public void Prueba_MostrarOverlayAdivinador_Comando_ActivaOverlayCorrecto()
        {

            Assert.AreEqual(Visibility.Visible, _vistaModelo.VisibilidadOverlayAdivinador);
            Assert.AreEqual(Visibility.Collapsed, _vistaModelo.VisibilidadOverlayDibujante);
        }

        [TestMethod]
        public void Prueba_OverlayTimer_Tick_IniciaTemporizadorYOcultaOverlays()
        {

            InvocarMetodoPrivado("OverlayTimer_Tick", this, EventArgs.Empty);

            Assert.AreEqual(Visibility.Collapsed, _vistaModelo.VisibilidadOverlayDibujante);
            Assert.AreEqual(Visibility.Collapsed, _vistaModelo.VisibilidadOverlayAdivinador);

            Assert.AreEqual("30", _vistaModelo.TextoContador);
            Assert.AreEqual(Brushes.Black, _vistaModelo.ColorContador);
            Assert.AreEqual(Visibility.Visible, _vistaModelo.VisibilidadPalabraAdivinar);
            Assert.AreEqual(Visibility.Visible, _vistaModelo.VisibilidadInfoCancion);

            Assert.AreEqual("Gasolina", _vistaModelo.PalabraAdivinar);
            Assert.AreEqual("Artista: Daddy Yankee", _vistaModelo.TextoArtista);
            Assert.AreEqual("Género: Reggaeton", _vistaModelo.TextoGenero);
        }

        #endregion

        #region 6. Eventos de sala y manejo de jugadores

        [TestMethod]
        public void Prueba_JugadorSeUnio_AgregaJugadorNuevo()
        {
            int inicial = _vistaModelo.Jugadores.Count;

            InvocarMetodoPrivado("SalasServicio_JugadorSeUnio", this, "JugadorNuevo");

            Assert.AreEqual(inicial + 1, _vistaModelo.Jugadores.Count);
            Assert.IsNotNull(_vistaModelo.Jugadores.FirstOrDefault(j => j.Nombre == "JugadorNuevo"));
        }

        [TestMethod]
        public void Prueba_JugadorSeUnio_NombreVacio_NoHaceNada()
        {
            int inicial = _vistaModelo.Jugadores.Count;

            InvocarMetodoPrivado("SalasServicio_JugadorSeUnio", this, "  ");

            Assert.AreEqual(inicial, _vistaModelo.Jugadores.Count);
        }

        [TestMethod]
        public void Prueba_JugadorSeUnio_Repetido_NoDuplica()
        {
            InvocarMetodoPrivado("SalasServicio_JugadorSeUnio", this, "JugadorExtra");
            int despuesPrimero = _vistaModelo.Jugadores.Count;

            InvocarMetodoPrivado("SalasServicio_JugadorSeUnio", this, "JugadorExtra");

            Assert.AreEqual(despuesPrimero, _vistaModelo.Jugadores.Count);
        }

        [TestMethod]
        public void Prueba_JugadorSeUnio_NoSuperaMaximoJugadores()
        {
            var lista = new List<string> { "J1", "J2", "J3", "J4" };
            InvocarMetodoPrivado("ActualizarJugadores", lista);

            int inicial = _vistaModelo.Jugadores.Count;

            InvocarMetodoPrivado("SalasServicio_JugadorSeUnio", this, "J5");

            Assert.AreEqual(inicial, _vistaModelo.Jugadores.Count);
        }

        [TestMethod]
        public void Prueba_JugadorSalio_EliminaJugadorExistente()
        {
            InvocarMetodoPrivado("SalasServicio_JugadorSeUnio", this, "JugadorSalida");
            Assert.IsNotNull(_vistaModelo.Jugadores.FirstOrDefault(j => j.Nombre == "JugadorSalida"));

            InvocarMetodoPrivado("SalasServicio_JugadorSalio", this, "JugadorSalida");

            Assert.IsNull(_vistaModelo.Jugadores.FirstOrDefault(j => j.Nombre == "JugadorSalida"));
        }

        [TestMethod]
        public void Prueba_JugadorExpulsado_UsuarioActualNoInvitado_NavegaVentanaPrincipal()
        {
            _mensajesMostrados.Clear();
            _destinoNavegacion = null;

            InvocarMetodoPrivado("SalasServicio_JugadorExpulsado", this, UsuarioHost);

            Assert.AreEqual(
                SalaVistaModelo.DestinoNavegacion.VentanaPrincipal,
                _destinoNavegacion);

            Assert.AreEqual(Lang.expulsarJugadorTextoFuisteExpulsado, _mensajesMostrados.Single());
            Assert.IsTrue(_vistaModelo.DebeEjecutarAccionAlCerrar());
        }

        [TestMethod]
        public void Prueba_JugadorExpulsado_UsuarioActualInvitado_NavegaInicioSesionYMarcaCierre()
        {
            var viewModelInvitado = new SalaVistaModelo(
                _sala,
                _mockSalasServicio.Object,
                _mockInvitacionesServicio.Object,
                _mockListaAmigosServicio.Object,
                _mockPerfilServicio.Object,
                UsuarioHost,
                true);

            SalaVistaModelo.DestinoNavegacion? destino = null;
            string mensaje = null;

            viewModelInvitado.ManejarNavegacion = d => destino = d;
            viewModelInvitado.MostrarMensaje = m => mensaje = m;

            var metodo = typeof(SalaVistaModelo).GetMethod(
                "SalasServicio_JugadorExpulsado",
                BindingFlags.Instance | BindingFlags.NonPublic);

            metodo.Invoke(viewModelInvitado, new object[] { this, UsuarioHost });

            Assert.AreEqual(
                SalaVistaModelo.DestinoNavegacion.InicioSesion,
                destino);

            Assert.AreEqual(Lang.expulsarJugadorTextoFuisteExpulsado, mensaje);
            Assert.IsFalse(viewModelInvitado.DebeEjecutarAccionAlCerrar());
        }

        [TestMethod]
        public void Prueba_JugadorExpulsado_OtroUsuario_SeEliminaDeLista()
        {
            InvocarMetodoPrivado("SalasServicio_JugadorSeUnio", this, "JugadorExpulsado");
            Assert.IsNotNull(_vistaModelo.Jugadores.FirstOrDefault(j => j.Nombre == "JugadorExpulsado"));

            InvocarMetodoPrivado("SalasServicio_JugadorExpulsado", this, "JugadorExpulsado");

            Assert.IsNull(_vistaModelo.Jugadores.FirstOrDefault(j => j.Nombre == "JugadorExpulsado"));
        }

        [TestMethod]
        public void Prueba_SalaActualizada_CodigoDistinto_NoModificaJugadores()
        {
            var jugadoresOriginales = _vistaModelo.Jugadores.ToList();

            var salaNueva = new DTOs.SalaDTO
            {
                Codigo = "OTRA",
                Jugadores = new List<string> { "X", "Y" }
            };

            InvocarMetodoPrivado("SalasServicio_SalaActualizada", this, salaNueva);

            CollectionAssert.AreEqual(
                jugadoresOriginales.Select(j => j.Nombre).ToList(),
                _vistaModelo.Jugadores.Select(j => j.Nombre).ToList());
        }

        [TestMethod]
        public void Prueba_SalaActualizada_CodigoCoincide_ActualizaJugadores()
        {
            var salaNueva = new DTOs.SalaDTO
            {
                Codigo = CodigoSalaTest,
                Creador = UsuarioHost,
                Jugadores = new List<string> { UsuarioHost, "Otro1", "Otro2" }
            };

            InvocarMetodoPrivado("SalasServicio_SalaActualizada", this, salaNueva);

            var nombres = _vistaModelo.Jugadores.Select(j => j.Nombre).ToList();

            Assert.AreEqual(3, nombres.Count);
            CollectionAssert.Contains(nombres, UsuarioHost);
            CollectionAssert.Contains(nombres, "Otro1");
            CollectionAssert.Contains(nombres, "Otro2");
        }

        [TestMethod]
        public void Prueba_AgregarJugador_ComoHost_MuestraBotonExpulsarParaOtros()
        {
            var lista = new List<string> { UsuarioHost, "JugadorExpulsable" };
            InvocarMetodoPrivado("ActualizarJugadores", lista);

            var jugadorHost = _vistaModelo.Jugadores.First(j => j.Nombre == UsuarioHost);
            var jugadorOtro = _vistaModelo.Jugadores.First(j => j.Nombre == "JugadorExpulsable");

            Assert.IsFalse(jugadorHost.MostrarBotonExpulsar);
            Assert.IsTrue(jugadorOtro.MostrarBotonExpulsar);
            Assert.IsNotNull(jugadorOtro.ExpulsarComando);
        }

        #endregion

        #region 7. Expulsar jugador (EjecutarExpulsarJugadorAsync)

        [TestMethod]
        public async Task Prueba_ExpulsarJugador_SinConfirmacion_NoLlamaServicio()
        {
            var lista = new List<string> { UsuarioHost, "Expulsado" };
            InvocarMetodoPrivado("ActualizarJugadores", lista);

            var jugador = _vistaModelo.Jugadores.First(j => j.Nombre == "Expulsado");

            _vistaModelo.MostrarConfirmacion = null;

            await ((IComandoAsincrono)jugador.ExpulsarComando).EjecutarAsync(null);

            _mockSalasServicio.Verify(
                s => s.ExpulsarJugadorAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Prueba_ExpulsarJugador_UsuarioCancela_NoLlamaServicio()
        {
            var lista = new List<string> { UsuarioHost, "Expulsado" };
            InvocarMetodoPrivado("ActualizarJugadores", lista);

            var jugador = _vistaModelo.Jugadores.First(j => j.Nombre == "Expulsado");

            _vistaModelo.MostrarConfirmacion = _ => false;

            await ((IComandoAsincrono)jugador.ExpulsarComando).EjecutarAsync(null);

            _mockSalasServicio.Verify(
                s => s.ExpulsarJugadorAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Prueba_ExpulsarJugador_Confirmado_Exito_LlamaServicioYNotifica()
        {
            var lista = new List<string> { UsuarioHost, "Expulsado" };
            InvocarMetodoPrivado("ActualizarJugadores", lista);

            var jugador = _vistaModelo.Jugadores.First(j => j.Nombre == "Expulsado");

            string mensajeConfirmacion = null;
            _vistaModelo.MostrarConfirmacion = m =>
            {
                mensajeConfirmacion = m;
                return true;
            };

            _mockSalasServicio.Setup(
                    s => s.ExpulsarJugadorAsync(CodigoSalaTest, UsuarioHost, "Expulsado"))
                .Returns(Task.CompletedTask);

            _mensajesMostrados.Clear();

            await ((IComandoAsincrono)jugador.ExpulsarComando).EjecutarAsync(null);

            _mockSalasServicio.Verify(
                s => s.ExpulsarJugadorAsync(CodigoSalaTest, UsuarioHost, "Expulsado"),
                Times.Once);

            Assert.IsFalse(string.IsNullOrWhiteSpace(mensajeConfirmacion));
            Assert.AreEqual(Lang.expulsarJugadorTextoExito, _mensajesMostrados.Single());
        }

        [TestMethod]
        public async Task Prueba_ExpulsarJugador_ExcepcionServicio_MuestraError()
        {
            var lista = new List<string> { UsuarioHost, "Expulsado" };
            InvocarMetodoPrivado("ActualizarJugadores", lista);

            var jugador = _vistaModelo.Jugadores.First(j => j.Nombre == "Expulsado");

            _vistaModelo.MostrarConfirmacion = _ => true;

            _mockSalasServicio.Setup(
                    s => s.ExpulsarJugadorAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorExpulsar", null));

            _mensajesMostrados.Clear();

            await ((IComandoAsincrono)jugador.ExpulsarComando).EjecutarAsync(null);

            Assert.AreEqual("ErrorExpulsar", _mensajesMostrados.Single());
        }

        [TestMethod]
        public async Task Prueba_ExpulsarJugador_ArgumentException_MuestraError()
        {
            var lista = new List<string> { UsuarioHost, "Expulsado" };
            InvocarMetodoPrivado("ActualizarJugadores", lista);

            var jugador = _vistaModelo.Jugadores.First(j => j.Nombre == "Expulsado");

            _vistaModelo.MostrarConfirmacion = _ => true;

            _mockSalasServicio.Setup(
                    s => s.ExpulsarJugadorAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("ErrorArgumentoExpulsar"));

            _mensajesMostrados.Clear();

            await ((IComandoAsincrono)jugador.ExpulsarComando).EjecutarAsync(null);

            Assert.AreEqual("ErrorArgumentoExpulsar", _mensajesMostrados.Single());
        }

        #endregion

        #region 8. Finalizacion y cierre de ventana

        [TestMethod]
        public async Task Prueba_FinalizarAsync_ConSalaYUsuario_LlamaAbandonarSala()
        {
            _mockSalasServicio.Setup(
                    s => s.AbandonarSalaAsync(CodigoSalaTest, UsuarioHost))
                .Returns(Task.CompletedTask);

            await _vistaModelo.FinalizarAsync();

            _mockSalasServicio.Verify(
                s => s.AbandonarSalaAsync(CodigoSalaTest, UsuarioHost),
                Times.Once);
        }

        [TestMethod]
        public async Task Prueba_FinalizarAsync_SinUsuario_NoLlamaAbandonarSala()
        {
            EstablecerCampoPrivado("_nombreUsuarioSesion", string.Empty);

            await _vistaModelo.FinalizarAsync();

            _mockSalasServicio.Verify(
                s => s.AbandonarSalaAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public void Prueba_DebeEjecutarAccionAlCerrar_TruePorDefecto()
        {
            Assert.IsTrue(_vistaModelo.DebeEjecutarAccionAlCerrar());
        }

        [TestMethod]
        public void Prueba_NotificarCierreAplicacionCompleta_MarcaComoNoEjecutable()
        {
            _vistaModelo.NotificarCierreAplicacionCompleta();

            Assert.IsFalse(_vistaModelo.DebeEjecutarAccionAlCerrar());
        }

        [TestMethod]
        public void Prueba_CerrarVentana_CuandoDebeCerrarAplicacion_MarcaCierre()
        {
            _cerrarAplicacionGlobal = true;

            _vistaModelo.CerrarVentanaComando.Execute(null);

            Assert.IsFalse(_vistaModelo.DebeEjecutarAccionAlCerrar());
        }

        [TestMethod]
        public void Prueba_CerrarVentana_CuandoNoDebeCerrarAplicacion_NoMarcaCierre()
        {
            _cerrarAplicacionGlobal = false;

            _vistaModelo.CerrarVentanaComando.Execute(null);

            Assert.IsTrue(_vistaModelo.DebeEjecutarAccionAlCerrar());
        }

        #endregion
    }
}
