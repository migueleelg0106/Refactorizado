using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.VistaModelo.VentanaPrincipal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasVistaModelo.VentanaJuego
{
    [TestClass]
    public class PruebaClasificacionVistaModelo
    {
        private Mock<IClasificacionServicio> _mockServicio;
        private ClasificacionVistaModelo _vistaModelo;

        [TestInitialize]
        public void Inicializar()
        {
            _mockServicio = new Mock<IClasificacionServicio>();
            _vistaModelo = new ClasificacionVistaModelo(_mockServicio.Object);

            AvisoAyudante.DefinirMostrarAviso((msj) => { });
        }

        [TestCleanup]
        public void Limpiar()
        {
            _vistaModelo = null;
        }

        #region 1. Constructor y Validaciones

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Prueba_Constructor_ServicioNulo_LanzaExcepcion()
        {
            new ClasificacionVistaModelo(null);
        }

        [TestMethod]
        public void Prueba_Constructor_InicializacionCorrecta()
        {
            Assert.IsNotNull(_vistaModelo.Clasificacion);
            Assert.AreEqual(0, _vistaModelo.Clasificacion.Count);
            Assert.IsFalse(_vistaModelo.HayResultados);
            Assert.IsFalse(_vistaModelo.EstaCargando);
            Assert.IsNotNull(_vistaModelo.OrdenarPorPuntosComando);
            Assert.IsNotNull(_vistaModelo.OrdenarPorRondasComando);
            Assert.IsNotNull(_vistaModelo.CerrarComando);
        }

        #endregion

        #region 2. Carga de Datos

        [TestMethod]
        public async Task Prueba_CargarClasificacionAsync_Exito_LlenaLista()
        {
            var datosMock = new List<DTOs.ClasificacionUsuarioDTO>
            {
                new DTOs.ClasificacionUsuarioDTO { Usuario = "Jugador1", Puntos = 100 },
                new DTOs.ClasificacionUsuarioDTO { Usuario = "Jugador2", Puntos = 200 }
            };

            _mockServicio.Setup(s => s.ObtenerTopJugadoresAsync()).ReturnsAsync(datosMock);

            await _vistaModelo.CargarClasificacionAsync();

            Assert.AreEqual(2, _vistaModelo.Clasificacion.Count);
            Assert.IsTrue(_vistaModelo.HayResultados);
            Assert.IsFalse(_vistaModelo.EstaCargando);
            Assert.AreEqual("Jugador1", _vistaModelo.Clasificacion[0].Usuario);
        }

        [TestMethod]
        public async Task Prueba_CargarClasificacionAsync_ListaVacia_NoHayResultados()
        {
            _mockServicio.Setup(s => s.ObtenerTopJugadoresAsync()).ReturnsAsync(new List<DTOs.ClasificacionUsuarioDTO>());

            await _vistaModelo.CargarClasificacionAsync();

            Assert.AreEqual(0, _vistaModelo.Clasificacion.Count);
            Assert.IsFalse(_vistaModelo.HayResultados);
        }

        [TestMethod]
        public async Task Prueba_CargarClasificacionAsync_Excepcion_MuestraMensaje()
        {
            _mockServicio.Setup(s => s.ObtenerTopJugadoresAsync())
                .ThrowsAsync(new ServicioExcepcion(TipoErrorServicio.FallaServicio, "ErrorServidor", null));

            string mensaje = null;
            AvisoAyudante.DefinirMostrarAviso(m => mensaje = m);

            await _vistaModelo.CargarClasificacionAsync();

            Assert.AreEqual("ErrorServidor", mensaje);
            Assert.IsFalse(_vistaModelo.EstaCargando, "Debe limpiar estado de carga en finally");
        }

        #endregion

        #region 3. Ordenamiento

        [TestMethod]
        public async Task Prueba_OrdenarPorPuntos_OrdenDescendente()
        {
            var datos = new List<DTOs.ClasificacionUsuarioDTO>
            {
                new DTOs.ClasificacionUsuarioDTO { Usuario = "Bajo", Puntos = 10, RondasGanadas = 5 },
                new DTOs.ClasificacionUsuarioDTO { Usuario = "Alto", Puntos = 100, RondasGanadas = 2 },
                new DTOs.ClasificacionUsuarioDTO { Usuario = "Medio", Puntos = 50, RondasGanadas = 8 }
            };
            _mockServicio.Setup(s => s.ObtenerTopJugadoresAsync()).ReturnsAsync(datos);
            await _vistaModelo.CargarClasificacionAsync();

            _vistaModelo.OrdenarPorPuntosComando.Execute(null);

            Assert.AreEqual("Alto", _vistaModelo.Clasificacion[0].Usuario);  
            Assert.AreEqual("Medio", _vistaModelo.Clasificacion[1].Usuario); 
            Assert.AreEqual("Bajo", _vistaModelo.Clasificacion[2].Usuario);  
        }

        [TestMethod]
        public async Task Prueba_OrdenarPorRondas_OrdenDescendente()
        {
            var datos = new List<DTOs.ClasificacionUsuarioDTO>
            {
                new DTOs.ClasificacionUsuarioDTO { Usuario = "Pocas", Puntos = 100, RondasGanadas = 2 },
                new DTOs.ClasificacionUsuarioDTO { Usuario = "Muchas", Puntos = 10, RondasGanadas = 20 }
            };
            _mockServicio.Setup(s => s.ObtenerTopJugadoresAsync()).ReturnsAsync(datos);
            await _vistaModelo.CargarClasificacionAsync();

            _vistaModelo.OrdenarPorRondasComando.Execute(null);

            Assert.AreEqual("Muchas", _vistaModelo.Clasificacion[0].Usuario); 
            Assert.AreEqual("Pocas", _vistaModelo.Clasificacion[1].Usuario);  
        }

        [TestMethod]
        public async Task Prueba_Ordenar_CriteriosDesempate()
        {
            var datos = new List<DTOs.ClasificacionUsuarioDTO>
            {
                new DTOs.ClasificacionUsuarioDTO { Usuario = "B_MenosRondas", Puntos = 100, RondasGanadas = 1 },
                new DTOs.ClasificacionUsuarioDTO { Usuario = "A_MasRondas", Puntos = 100, RondasGanadas = 5 },
                new DTOs.ClasificacionUsuarioDTO { Usuario = "Z_MismasRondas", Puntos = 100, RondasGanadas = 5 }
            };
            _mockServicio.Setup(s => s.ObtenerTopJugadoresAsync()).ReturnsAsync(datos);
            await _vistaModelo.CargarClasificacionAsync();

            _vistaModelo.OrdenarPorPuntosComando.Execute(null);

            Assert.AreEqual("A_MasRondas", _vistaModelo.Clasificacion[0].Usuario);
            Assert.AreEqual("Z_MismasRondas", _vistaModelo.Clasificacion[1].Usuario);
            Assert.AreEqual("B_MenosRondas", _vistaModelo.Clasificacion[2].Usuario);
        }

        [TestMethod]
        public void Prueba_Ordenar_SinDatos_NoEjecuta()
        {
            _vistaModelo.OrdenarPorPuntosComando.Execute(null);

            Assert.AreEqual(0, _vistaModelo.Clasificacion.Count);
        }

        [TestMethod]
        public void Prueba_PuedeOrdenar_EstadoCargando_DevuelveFalso()
        {
            typeof(ClasificacionVistaModelo).GetProperty("EstaCargando").SetValue(_vistaModelo, true);

            Assert.IsFalse(_vistaModelo.OrdenarPorPuntosComando.CanExecute(null));
        }

        #endregion

        #region 4. Navegación

        [TestMethod]
        public void Prueba_CerrarComando_InvocaAccion()
        {
            bool cerrado = false;
            _vistaModelo.CerrarAccion = () => cerrado = true;

            _vistaModelo.CerrarComando.Execute(null);

            Assert.IsTrue(cerrado);
        }

        #endregion
    }
}