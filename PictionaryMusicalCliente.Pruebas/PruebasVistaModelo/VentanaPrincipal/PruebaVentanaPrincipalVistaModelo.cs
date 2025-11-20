using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.VentanaPrincipal;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.VentanaPrincipal
{
    [TestClass]
    public class PruebaVentanaPrincipalVistaModelo
    {
        private Mock<ILocalizacionServicio> _mockLocalizacion;
        private Mock<IListaAmigosServicio> _mockListaAmigos;
        private Mock<IAmigosServicio> _mockAmigosServicio;
        private Mock<ISalasServicio> _mockSalasServicio;
        private VentanaPrincipalVistaModelo _vistaModelo;

        private const string UsuarioTest = "UsuarioPrueba";

        [TestInitialize]
        public void Inicializar()
        {
            _mockLocalizacion = new Mock<ILocalizacionServicio>();
            _mockListaAmigos = new Mock<IListaAmigosServicio>();
            _mockAmigosServicio = new Mock<IAmigosServicio>();
            _mockSalasServicio = new Mock<ISalasServicio>();

            _mockLocalizacion.Setup(l => l.CulturaActual).Returns(new CultureInfo("es-MX"));
            _mockListaAmigos.Setup(l => l.ListaActual).Returns(new List<AmigoDTO>());

            _vistaModelo = new VentanaPrincipalVistaModelo(
                _mockLocalizacion.Object,
                _mockListaAmigos.Object,
                _mockAmigosServicio.Object,
                _mockSalasServicio.Object
            );

            _vistaModelo.MostrarMensaje = (_) => { };
            _vistaModelo.ConfirmarEliminarAmigo = (_) => true;
        }

        [TestCleanup]
        public void Limpiar()
        {
            _vistaModelo = null;
        }

        #region Constructor y Validaciones

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_LocalizacionNula_LanzaExcepcion()
        {
            new VentanaPrincipalVistaModelo(
                null,
                _mockListaAmigos.Object,
                _mockAmigosServicio.Object,
                _mockSalasServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_ListaAmigosNula_LanzaExcepcion()
        {
            new VentanaPrincipalVistaModelo(
                _mockLocalizacion.Object,
                null,
                _mockAmigosServicio.Object,
                _mockSalasServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_AmigosServicioNulo_LanzaExcepcion()
        {
            new VentanaPrincipalVistaModelo(
                _mockLocalizacion.Object,
                _mockListaAmigos.Object,
                null,
                _mockSalasServicio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_SalasServicioNulo_LanzaExcepcion()
        {
            new VentanaPrincipalVistaModelo(
                _mockLocalizacion.Object,
                _mockListaAmigos.Object,
                _mockAmigosServicio.Object,
                null);
        }

        [TestMethod]
        public void Prueba_Constructor_InicializaListasYOpcionesCorrectamente()
        {
            Assert.IsNotNull(_vistaModelo.Amigos);
            Assert.IsNotNull(_vistaModelo.NumeroRondasOpciones);
            Assert.IsNotNull(_vistaModelo.TiempoRondaOpciones);
            Assert.IsNotNull(_vistaModelo.IdiomasDisponibles);
            Assert.IsNotNull(_vistaModelo.DificultadesDisponibles);

            Assert.AreEqual(3, _vistaModelo.NumeroRondasOpciones.Count);
            Assert.AreEqual(3, _vistaModelo.TiempoRondaOpciones.Count);
            Assert.AreEqual(3, _vistaModelo.DificultadesDisponibles.Count);

            Assert.IsNotNull(_vistaModelo.NumeroRondasSeleccionada);
            Assert.IsNotNull(_vistaModelo.TiempoRondaSeleccionada);
            Assert.IsNotNull(_vistaModelo.DificultadSeleccionada);
            Assert.IsNotNull(_vistaModelo.IdiomaSeleccionado);
        }

        #endregion

        #region Propiedades y Notificaciones

        [TestMethod]
        public void Prueba_CodigoSala_Setter_NotificaCambio()
        {
            bool notificado = false;
            _vistaModelo.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_vistaModelo.CodigoSala))
                {
                    notificado = true;
                }
            };

            _vistaModelo.CodigoSala = "123456";

            Assert.IsTrue(notificado);
            Assert.AreEqual("123456", _vistaModelo.CodigoSala);
        }

        [TestMethod]
        public void Prueba_AmigoSeleccionado_Setter_GuardaValor()
        {
            var amigo = new AmigoDTO { NombreUsuario = "Juan" };
            _vistaModelo.AmigoSeleccionado = amigo;
            Assert.AreEqual(amigo, _vistaModelo.AmigoSeleccionado);
        }

        [TestMethod]
        public void Prueba_OpcionesJuego_Setters_ActualizanEstadoComando()
        {
            var nuevaOpcionRonda = _vistaModelo.NumeroRondasOpciones.Last();
            _vistaModelo.NumeroRondasSeleccionada = nuevaOpcionRonda;
            Assert.AreEqual(nuevaOpcionRonda, _vistaModelo.NumeroRondasSeleccionada);

            var nuevoTiempo = _vistaModelo.TiempoRondaOpciones.Last();
            _vistaModelo.TiempoRondaSeleccionada = nuevoTiempo;
            Assert.AreEqual(nuevoTiempo, _vistaModelo.TiempoRondaSeleccionada);

            var nuevaDificultad = _vistaModelo.DificultadesDisponibles.Last();
            _vistaModelo.DificultadSeleccionada = nuevaDificultad;
            Assert.AreEqual(nuevaDificultad, _vistaModelo.DificultadSeleccionada);
        }

        #endregion

        #region Comandos de Navegacion (Delegados)

        [TestMethod]
        public void Prueba_ComandosNavegacion_InvocanAccionesBasicas()
        {
            bool perfilAbierto = false;
            bool ajustesAbierto = false;
            bool comoJugarAbierto = false;
            bool clasificacionAbierto = false;
            bool buscarAmigoAbierto = false;

            _vistaModelo.AbrirPerfil = () => perfilAbierto = true;
            _vistaModelo.AbrirAjustes = () => ajustesAbierto = true;
            _vistaModelo.AbrirComoJugar = () => comoJugarAbierto = true;
            _vistaModelo.AbrirClasificacion = () => clasificacionAbierto = true;
            _vistaModelo.AbrirBuscarAmigo = () => buscarAmigoAbierto = true;

            _vistaModelo.AbrirPerfilComando.Execute(null);
            _vistaModelo.AbrirAjustesComando.Execute(null);
            _vistaModelo.AbrirComoJugarComando.Execute(null);
            _vistaModelo.AbrirClasificacionComando.Execute(null);
            _vistaModelo.AbrirBuscarAmigoComando.Execute(null);

            Assert.IsTrue(perfilAbierto);
            Assert.IsTrue(ajustesAbierto);
            Assert.IsTrue(comoJugarAbierto);
            Assert.IsTrue(clasificacionAbierto);
            Assert.IsTrue(buscarAmigoAbierto);
        }

        [TestMethod]
        public void Prueba_AbrirSolicitudes_SinSolicitudes_MuestraAvisoYNoAbreVentana()
        {
            bool solicitudesAbierto = false;
            string mensaje = null;

            _vistaModelo.AbrirSolicitudes = () => solicitudesAbierto = true;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            _vistaModelo.AbrirSolicitudesComando.Execute(null);

            Assert.IsFalse(solicitudesAbierto);
            Assert.AreEqual(Lang.amigosAvisoSinSolicitudesPendientes, mensaje);
        }

        #endregion

        #region Gestion de Amigos (Eventos y Actualizacion)

        [TestMethod]
        public void Prueba_ListaActualizada_Evento_AgregaAmigos()
        {
            var nuevosAmigos = new List<AmigoDTO>
                {
                    new AmigoDTO { NombreUsuario = "Amigo1" },
                    new AmigoDTO { NombreUsuario = "Amigo2" }
                };

            _mockListaAmigos.Raise(m => m.ListaActualizada += null, null, nuevosAmigos);

            Assert.AreEqual(2, _vistaModelo.Amigos.Count);
            Assert.IsTrue(_vistaModelo.Amigos.Any(a => a.NombreUsuario == "Amigo1"));
        }

        [TestMethod]
        public void Prueba_ListaActualizada_AmigoSeleccionadoDesaparece_LimpiaSeleccion()
        {
            var amigo = new AmigoDTO { NombreUsuario = "AmigoBorrado" };
            _vistaModelo.AmigoSeleccionado = amigo;

            _mockListaAmigos.Raise(m => m.ListaActualizada += null, null, new List<AmigoDTO>());

            Assert.IsNull(_vistaModelo.AmigoSeleccionado);
        }

        [TestMethod]
        public void Prueba_ListaActualizada_AmigoSeleccionadoPermanece_MantieneSeleccion()
        {
            var amigo = new AmigoDTO { NombreUsuario = "AmigoFiel" };
            _vistaModelo.AmigoSeleccionado = amigo;
            var listaNueva = new List<AmigoDTO>
                {
                    new AmigoDTO { NombreUsuario = "AmigoFiel" }
                };

            _mockListaAmigos.Raise(m => m.ListaActualizada += null, null, listaNueva);

            Assert.IsNotNull(_vistaModelo.AmigoSeleccionado);
            Assert.AreEqual("AmigoFiel", _vistaModelo.AmigoSeleccionado.NombreUsuario);
        }

        #endregion

        #region Comando Eliminar Amigo

        [TestMethod]
        public async Task Prueba_EliminarAmigo_ArgumentoNulo_NoHaceNada()
        {
            bool confirmarInvocado = false;
            string mensaje = null;

            _vistaModelo.ConfirmarEliminarAmigo = (_) =>
            {
                confirmarInvocado = true;
                return true;
            };
            _vistaModelo.MostrarMensaje = m => mensaje = m;

            await _vistaModelo.EliminarAmigoComando.EjecutarAsync(null);

            Assert.IsFalse(confirmarInvocado);
            _mockAmigosServicio.Verify(
                s => s.EliminarAmigoAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            Assert.IsNull(mensaje);
        }

        [TestMethod]
        public async Task Prueba_EliminarAmigo_ConfirmacionCancelada_NoLlamaServicio()
        {
            _vistaModelo.ConfirmarEliminarAmigo = (nombre) => false;
            var amigo = new AmigoDTO { NombreUsuario = "Amigo1" };

            await _vistaModelo.EliminarAmigoComando.EjecutarAsync(amigo);

            _mockAmigosServicio.Verify(
                s => s.EliminarAmigoAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Prueba_EliminarAmigo_UsuarioSesionVacio_MuestraError()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "");

            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;
            _vistaModelo.ConfirmarEliminarAmigo = (_) => true;

            await _vistaModelo.EliminarAmigoComando.EjecutarAsync(new AmigoDTO { NombreUsuario = "X" });

            Assert.AreEqual(Lang.errorTextoErrorProcesarSolicitud, mensaje);
        }

        [TestMethod]
        public async Task Prueba_EliminarAmigo_Exito_LlamaServicioYMensaje()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "Yo");

            _vistaModelo.ConfirmarEliminarAmigo = (_) => true;
            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;
            var amigo = new AmigoDTO { NombreUsuario = "AmigoX" };

            await _vistaModelo.EliminarAmigoComando.EjecutarAsync(amigo);

            _mockAmigosServicio.Verify(s => s.EliminarAmigoAsync("Yo", "AmigoX"), Times.Once);
            Assert.AreEqual(Lang.amigosTextoAmigoEliminado, mensaje);
        }

        [TestMethod]
        public async Task Prueba_EliminarAmigo_Excepcion_MuestraError()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "Yo");
            _vistaModelo.ConfirmarEliminarAmigo = (_) => true;

            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            _mockAmigosServicio
                .Setup(s => s.EliminarAmigoAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(
                    TipoErrorServicio.FallaServicio,
                    "ErrorWCF",
                    null));

            await _vistaModelo.EliminarAmigoComando.EjecutarAsync(
                new AmigoDTO { NombreUsuario = "A" });

            Assert.AreEqual("ErrorWCF", mensaje);
        }

        #endregion

        #region Comando Unirse a Sala

        [TestMethod]
        public async Task Prueba_UnirseSala_CodigoVacio_MuestraError()
        {
            _vistaModelo.CodigoSala = "   ";
            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.unirseSalaTextoVacio, mensaje);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_UsuarioSesionVacio_MuestraErrorProcesarSolicitud()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "");

            _vistaModelo.CodigoSala = "123456";
            string mensaje = null;
            _vistaModelo.MostrarMensaje = m => mensaje = m;

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoErrorProcesarSolicitud, mensaje);
            _mockSalasServicio.Verify(
                s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_Exito_InvocaAccion()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "Yo");
            _vistaModelo.CodigoSala = "123456";

            SalaDTO salaRetornada = new SalaDTO { Codigo = "123456" };
            _mockSalasServicio
                .Setup(s => s.UnirseSalaAsync("123456", "Yo"))
                .ReturnsAsync(salaRetornada);

            SalaDTO salaRecibida = null;
            _vistaModelo.UnirseSala = (s) => salaRecibida = s;

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(salaRetornada, salaRecibida);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_SalaLlena_MuestraMensajeEspecifico()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "Yo");
            _vistaModelo.CodigoSala = "123456";
            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            var ex = new ServicioExcepcion(
                TipoErrorServicio.OperacionInvalida,
                Lang.errorTextoSalaLlena,
                null);

            _mockSalasServicio
                .Setup(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(ex);

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoSalaLlena, mensaje);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_FallaServicio_MuestraMensajeDefecto()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "Yo");
            _vistaModelo.CodigoSala = "123456";
            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            var ex = new ServicioExcepcion(
                TipoErrorServicio.FallaServicio,
                "ErrorX",
                null);

            _mockSalasServicio
                .Setup(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(ex);

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoNoEncuentraPartida, mensaje);
        }

        [TestMethod]
        public async Task Prueba_UnirseSala_ErrorGeneral_MuestraMensajeDeExcepcion()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "Yo");
            _vistaModelo.CodigoSala = "123456";
            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            var ex = new ServicioExcepcion(
                TipoErrorServicio.OperacionInvalida,
                "ErrorGenerico",
                null);

            _mockSalasServicio
                .Setup(s => s.UnirseSalaAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(ex);

            await _vistaModelo.UnirseSalaComando.EjecutarAsync(null);

            Assert.AreEqual("ErrorGenerico", mensaje);
        }

        #endregion

        #region Comando Iniciar Juego (Crear Sala)

        [TestMethod]
        public async Task Prueba_IniciarJuego_OpcionesNulas_MuestraError()
        {
            _vistaModelo.NumeroRondasSeleccionada = null;
            _vistaModelo.TiempoRondaSeleccionada = null;

            string mensajeCapturado = null;
            _vistaModelo.MostrarMensaje = (m) => mensajeCapturado = m;

            MethodInfo metodo = typeof(VentanaPrincipalVistaModelo)
                .GetMethod("IniciarJuegoInternoAsync",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            var tarea = (Task)metodo.Invoke(_vistaModelo, null);
            await tarea;

            Assert.IsNotNull(
                mensajeCapturado,
                "El mensaje es nulo. Verifica que el metodo IniciarJuegoInternoAsync tenga logica para mostrar error.");
            Assert.AreEqual(Lang.errorTextoErrorProcesarSolicitud, mensajeCapturado);
        }

        [TestMethod]
        public async Task Prueba_IniciarJuego_UsuarioSesionVacio_MuestraError()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "");

            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            await _vistaModelo.IniciarJuegoComando.EjecutarAsync(null);

            Assert.AreEqual(Lang.errorTextoErrorProcesarSolicitud, mensaje);
            _mockSalasServicio.Verify(
                s => s.CrearSalaAsync(It.IsAny<string>(), It.IsAny<ConfiguracionPartidaDTO>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Prueba_IniciarJuego_Exito_CreaSalaEInvocaAccion()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "Yo");

            SalaDTO salaCreada = new SalaDTO { Codigo = "NUEVA" };
            _mockSalasServicio
                .Setup(s => s.CrearSalaAsync("Yo", It.IsAny<ConfiguracionPartidaDTO>()))
                .ReturnsAsync(salaCreada);

            SalaDTO salaRecibida = null;
            _vistaModelo.IniciarJuego = (s) => salaRecibida = s;

            await _vistaModelo.IniciarJuegoComando.EjecutarAsync(null);

            Assert.AreEqual(salaCreada, salaRecibida);
        }

        [TestMethod]
        public async Task Prueba_IniciarJuego_Excepcion_MuestraError()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "Yo");
            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            _mockSalasServicio
                .Setup(s => s.CrearSalaAsync(It.IsAny<string>(), It.IsAny<ConfiguracionPartidaDTO>()))
                .ThrowsAsync(new ServicioExcepcion(
                    TipoErrorServicio.FallaServicio,
                    "FalloCreacion",
                    null));

            await _vistaModelo.IniciarJuegoComando.EjecutarAsync(null);

            Assert.AreEqual("FalloCreacion", mensaje);
        }

        #endregion

        #region Idioma y Localizacion

        [TestMethod]
        public void Prueba_IdiomaActualizado_Evento_ActualizaListaIdiomas()
        {
            _mockLocalizacion.Setup(l => l.CulturaActual).Returns(new CultureInfo("en-US"));

            MethodInfo metodo = typeof(VentanaPrincipalVistaModelo)
                .GetMethod("LocalizacionServicioEnIdiomaActualizado",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            metodo.Invoke(_vistaModelo, new object[] { null, EventArgs.Empty });

            Assert.IsNotNull(_vistaModelo.IdiomaSeleccionado, "El idioma seleccionado es nulo");
            Assert.AreEqual("en-US", _vistaModelo.IdiomaSeleccionado.Codigo);
        }

        [TestMethod]
        public void Prueba_IdiomaSeleccionado_Cambio_ActualizaEstadoComando()
        {
            var nuevoIdioma = _vistaModelo.IdiomasDisponibles.Last();
            _vistaModelo.IdiomaSeleccionado = nuevoIdioma;
            Assert.AreEqual(nuevoIdioma, _vistaModelo.IdiomaSeleccionado);
        }

        #endregion

        #region Ciclo de Vida (Inicializar y Finalizar)

        [TestMethod]
        public async Task Prueba_InicializarAsync_UsuarioSesionVacio_NoSuscribeServicios()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "");

            await _vistaModelo.InicializarAsync();

            _mockListaAmigos.Verify(
                s => s.SuscribirAsync(It.IsAny<string>()),
                Times.Never);
            _mockAmigosServicio.Verify(
                s => s.SuscribirAsync(It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Prueba_InicializarAsync_SuscribeServicios()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "Yo");

            await _vistaModelo.InicializarAsync();

            _mockListaAmigos.Verify(s => s.SuscribirAsync("Yo"), Times.Once);
            _mockAmigosServicio.Verify(s => s.SuscribirAsync("Yo"), Times.Once);
        }

        [TestMethod]
        public async Task Prueba_InicializarAsync_SiYaSuscrito_NoHaceNada()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "Yo");

            await _vistaModelo.InicializarAsync();
            await _vistaModelo.InicializarAsync();

            _mockListaAmigos.Verify(s => s.SuscribirAsync("Yo"), Times.Once);
        }

        [TestMethod]
        public async Task Prueba_InicializarAsync_Excepcion_MuestraError()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "Yo");
            string mensaje = null;
            _vistaModelo.MostrarMensaje = (m) => mensaje = m;

            _mockListaAmigos
                .Setup(s => s.SuscribirAsync(It.IsAny<string>()))
                .ThrowsAsync(new ServicioExcepcion(
                    TipoErrorServicio.FallaServicio,
                    "ErrorSuscripcion",
                    null));

            await _vistaModelo.InicializarAsync();

            Assert.AreEqual("ErrorSuscripcion", mensaje);
        }

        [TestMethod]
        public async Task Prueba_FinalizarAsync_UsuarioSesionVacio_NoCancelaSuscripciones()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "");

            await _vistaModelo.FinalizarAsync();

            _mockListaAmigos.Verify(
                s => s.CancelarSuscripcionAsync(It.IsAny<string>()),
                Times.Never);
            _mockAmigosServicio.Verify(
                s => s.CancelarSuscripcionAsync(It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Prueba_FinalizarAsync_CancelaSuscripciones()
        {
            typeof(VentanaPrincipalVistaModelo)
                .GetField("_nombreUsuarioSesion", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_vistaModelo, "Yo");

            await _vistaModelo.FinalizarAsync();

            _mockListaAmigos.Verify(s => s.CancelarSuscripcionAsync("Yo"), Times.Once);
            _mockAmigosServicio.Verify(s => s.CancelarSuscripcionAsync("Yo"), Times.Once);
        }

        #endregion
    }
}
