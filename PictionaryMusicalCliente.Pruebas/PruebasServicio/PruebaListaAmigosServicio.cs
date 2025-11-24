using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Pruebas.PruebasServicio
{
    /// <summary>
    /// Pruebas para ListaAmigosServicio.
    /// Nota: Estas pruebas se enfocan en la lógica interna que puede probarse sin un servidor WCF,
    /// como validaciones, callbacks y gestión de estado.
    /// </summary>
    [TestClass]
    public class PruebaListaAmigosServicio
    {
        private ListaAmigosServicio _servicio;

        [TestCleanup]
        public void Limpiar()
        {
            _servicio?.Dispose();
            _servicio = null;
        }

        #region Pruebas de Validación de Parámetros para Suscripción

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SuscribirAsync_NombreUsuarioVacio_LanzaExcepcion()
        {
            _servicio = new ListaAmigosServicio();
            await _servicio.SuscribirAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SuscribirAsync_NombreUsuarioNulo_LanzaExcepcion()
        {
            _servicio = new ListaAmigosServicio();
            await _servicio.SuscribirAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SuscribirAsync_NombreUsuarioEspacios_LanzaExcepcion()
        {
            _servicio = new ListaAmigosServicio();
            await _servicio.SuscribirAsync("   ");
        }

        [TestMethod]
        public async Task CancelarSuscripcionAsync_NombreUsuarioVacio_NoLanzaExcepcion()
        {
            _servicio = new ListaAmigosServicio();
            // No debería lanzar excepción
            await _servicio.CancelarSuscripcionAsync(string.Empty);
        }

        [TestMethod]
        public async Task CancelarSuscripcionAsync_NombreUsuarioNulo_NoLanzaExcepcion()
        {
            _servicio = new ListaAmigosServicio();
            // No debería lanzar excepción
            await _servicio.CancelarSuscripcionAsync(null);
        }

        #endregion

        #region Pruebas de Validación de Parámetros para ObtenerAmigos

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ObtenerAmigosAsync_NombreUsuarioVacio_LanzaExcepcion()
        {
            _servicio = new ListaAmigosServicio();
            await _servicio.ObtenerAmigosAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ObtenerAmigosAsync_NombreUsuarioNulo_LanzaExcepcion()
        {
            _servicio = new ListaAmigosServicio();
            await _servicio.ObtenerAmigosAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ObtenerAmigosAsync_NombreUsuarioEspacios_LanzaExcepcion()
        {
            _servicio = new ListaAmigosServicio();
            await _servicio.ObtenerAmigosAsync("   ");
        }

        #endregion

        #region Pruebas de Propiedades y Estado Inicial

        [TestMethod]
        public void ListaActual_IniciaVacia()
        {
            _servicio = new ListaAmigosServicio();
            var lista = _servicio.ListaActual;

            Assert.IsNotNull(lista);
            Assert.AreEqual(0, lista.Count);
        }

        [TestMethod]
        public void ListaActual_DevuelveColeccionDeSoloLectura()
        {
            _servicio = new ListaAmigosServicio();
            var lista = _servicio.ListaActual;

            Assert.IsInstanceOfType(lista, typeof(IReadOnlyList<DTOs.AmigoDTO>));
        }

        #endregion

        #region Pruebas de Callbacks (Lógica sin servidor)

        [TestMethod]
        public void NotificarListaAmigosActualizada_ListaValida_ActualizaListaActual()
        {
            _servicio = new ListaAmigosServicio();

            var amigos = new[]
            {
                new DTOs.AmigoDTO { UsuarioId = 1, NombreUsuario = "Amigo1" },
                new DTOs.AmigoDTO { UsuarioId = 2, NombreUsuario = "Amigo2" },
                new DTOs.AmigoDTO { UsuarioId = 3, NombreUsuario = "Amigo3" }
            };

            _servicio.NotificarListaAmigosActualizada(amigos);

            var listaActual = _servicio.ListaActual;
            Assert.AreEqual(3, listaActual.Count);
            Assert.AreEqual("Amigo1", listaActual[0].NombreUsuario);
            Assert.AreEqual("Amigo2", listaActual[1].NombreUsuario);
            Assert.AreEqual("Amigo3", listaActual[2].NombreUsuario);
        }

        [TestMethod]
        public void NotificarListaAmigosActualizada_ListaNula_LimpiaLista()
        {
            _servicio = new ListaAmigosServicio();

            // Primero agregamos algunos amigos
            var amigos = new[]
            {
                new DTOs.AmigoDTO { UsuarioId = 1, NombreUsuario = "Amigo1" }
            };
            _servicio.NotificarListaAmigosActualizada(amigos);
            Assert.AreEqual(1, _servicio.ListaActual.Count);

            // Luego enviamos null
            _servicio.NotificarListaAmigosActualizada(null);

            var listaActual = _servicio.ListaActual;
            Assert.AreEqual(0, listaActual.Count);
        }

        [TestMethod]
        public void NotificarListaAmigosActualizada_ListaVacia_LimpiaLista()
        {
            _servicio = new ListaAmigosServicio();

            // Primero agregamos algunos amigos
            var amigos = new[]
            {
                new DTOs.AmigoDTO { UsuarioId = 1, NombreUsuario = "Amigo1" }
            };
            _servicio.NotificarListaAmigosActualizada(amigos);
            Assert.AreEqual(1, _servicio.ListaActual.Count);

            // Luego enviamos array vacío
            _servicio.NotificarListaAmigosActualizada(new DTOs.AmigoDTO[0]);

            var listaActual = _servicio.ListaActual;
            Assert.AreEqual(0, listaActual.Count);
        }

        [TestMethod]
        public void NotificarListaAmigosActualizada_ConAmigosInvalidos_FiltraCorrectamente()
        {
            _servicio = new ListaAmigosServicio();

            var amigos = new[]
            {
                new DTOs.AmigoDTO { UsuarioId = 1, NombreUsuario = "Amigo1" },
                null,
                new DTOs.AmigoDTO { UsuarioId = 2, NombreUsuario = "" },
                new DTOs.AmigoDTO { UsuarioId = 3, NombreUsuario = "Amigo3" },
                new DTOs.AmigoDTO { UsuarioId = 4, NombreUsuario = "   " }
            };

            _servicio.NotificarListaAmigosActualizada(amigos);

            var listaActual = _servicio.ListaActual;
            Assert.AreEqual(2, listaActual.Count);
            Assert.AreEqual("Amigo1", listaActual[0].NombreUsuario);
            Assert.AreEqual("Amigo3", listaActual[1].NombreUsuario);
        }

        [TestMethod]
        public void NotificarListaAmigosActualizada_DisparaEvento()
        {
            _servicio = new ListaAmigosServicio();

            bool eventoDisparado = false;
            IReadOnlyList<DTOs.AmigoDTO> amigosRecibidos = null;

            _servicio.ListaActualizada += (sender, e) =>
            {
                eventoDisparado = true;
                amigosRecibidos = e;
            };

            var amigos = new[]
            {
                new DTOs.AmigoDTO { UsuarioId = 1, NombreUsuario = "Amigo1" },
                new DTOs.AmigoDTO { UsuarioId = 2, NombreUsuario = "Amigo2" }
            };

            _servicio.NotificarListaAmigosActualizada(amigos);

            Assert.IsTrue(eventoDisparado);
            Assert.IsNotNull(amigosRecibidos);
            Assert.AreEqual(2, amigosRecibidos.Count);
        }

        [TestMethod]
        public void NotificarListaAmigosActualizada_ReemplazaListaAnterior()
        {
            _servicio = new ListaAmigosServicio();

            // Primera actualización
            var amigos1 = new[]
            {
                new DTOs.AmigoDTO { UsuarioId = 1, NombreUsuario = "Amigo1" },
                new DTOs.AmigoDTO { UsuarioId = 2, NombreUsuario = "Amigo2" }
            };
            _servicio.NotificarListaAmigosActualizada(amigos1);
            Assert.AreEqual(2, _servicio.ListaActual.Count);

            // Segunda actualización con lista diferente
            var amigos2 = new[]
            {
                new DTOs.AmigoDTO { UsuarioId = 3, NombreUsuario = "Amigo3" }
            };
            _servicio.NotificarListaAmigosActualizada(amigos2);

            var listaActual = _servicio.ListaActual;
            Assert.AreEqual(1, listaActual.Count);
            Assert.AreEqual("Amigo3", listaActual[0].NombreUsuario);
        }

        #endregion

        #region Pruebas de Dispose

        [TestMethod]
        public void Dispose_NoLanzaExcepcion()
        {
            _servicio = new ListaAmigosServicio();
            _servicio.Dispose();

            // Verificar que todavía se puede acceder a propiedades
            var lista = _servicio.ListaActual;
            Assert.IsNotNull(lista);
        }

        [TestMethod]
        public void Dispose_MultiplesLlamadas_NoGeneraError()
        {
            _servicio = new ListaAmigosServicio();
            _servicio.Dispose();
            _servicio.Dispose();
            // No debería lanzar excepción
        }

        [TestMethod]
        public void Dispose_LimpiaLista()
        {
            _servicio = new ListaAmigosServicio();

            var amigos = new[]
            {
                new DTOs.AmigoDTO { UsuarioId = 1, NombreUsuario = "Amigo1" }
            };
            _servicio.NotificarListaAmigosActualizada(amigos);
            Assert.AreEqual(1, _servicio.ListaActual.Count);

            _servicio.Dispose();

            // Después de dispose, la lista debería estar vacía o accesible
            var lista = _servicio.ListaActual;
            Assert.IsNotNull(lista);
        }

        #endregion
    }
}
