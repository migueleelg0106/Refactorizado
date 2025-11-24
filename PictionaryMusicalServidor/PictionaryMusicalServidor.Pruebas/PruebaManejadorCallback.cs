using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;

namespace PictionaryMusicalServidor.Pruebas
{
    // Interface de prueba para el callback
    public interface ICallbackPrueba
    {
        void Notificar(string mensaje);
    }

    // Implementación mock del callback
    public class CallbackPrueba : ICallbackPrueba
    {
        public int VecesLlamado { get; private set; }
        public string UltimoMensaje { get; private set; }

        public void Notificar(string mensaje)
        {
            VecesLlamado++;
            UltimoMensaje = mensaje;
        }
    }

    [TestClass]
    public class PruebaManejadorCallback
    {
        [TestMethod]
        public void Prueba_Suscribir_DeberiaAgregarCallback()
        {
            var manejador = new ManejadorCallback<ICallbackPrueba>();
            var callback = new CallbackPrueba();

            manejador.Suscribir("usuario1", callback);

            bool existe = manejador.TryGetCallback("usuario1", out var callbackRecuperado);
            Assert.IsTrue(existe);
            Assert.AreSame(callback, callbackRecuperado);
        }

        [TestMethod]
        public void Prueba_Suscribir_NoDeberiaAgregarCallbackConNombreNulo()
        {
            var manejador = new ManejadorCallback<ICallbackPrueba>();
            var callback = new CallbackPrueba();

            manejador.Suscribir(null, callback);

            bool existe = manejador.TryGetCallback(null, out _);
            Assert.IsFalse(existe);
        }

        [TestMethod]
        public void Prueba_Suscribir_NoDeberiaAgregarCallbackConNombreVacio()
        {
            var manejador = new ManejadorCallback<ICallbackPrueba>();
            var callback = new CallbackPrueba();

            manejador.Suscribir("", callback);

            bool existe = manejador.TryGetCallback("", out _);
            Assert.IsFalse(existe);
        }

        [TestMethod]
        public void Prueba_Suscribir_NoDeberiaAgregarCallbackNulo()
        {
            var manejador = new ManejadorCallback<ICallbackPrueba>();

            manejador.Suscribir("usuario1", null);

            bool existe = manejador.TryGetCallback("usuario1", out _);
            Assert.IsFalse(existe);
        }

        [TestMethod]
        public void Prueba_Suscribir_DeberiaReemplazarCallbackExistente()
        {
            var manejador = new ManejadorCallback<ICallbackPrueba>();
            var callback1 = new CallbackPrueba();
            var callback2 = new CallbackPrueba();

            manejador.Suscribir("usuario1", callback1);
            manejador.Suscribir("usuario1", callback2);

            bool existe = manejador.TryGetCallback("usuario1", out var callbackRecuperado);
            Assert.IsTrue(existe);
            Assert.AreSame(callback2, callbackRecuperado);
        }

        [TestMethod]
        public void Prueba_Desuscribir_DeberiaEliminarCallback()
        {
            var manejador = new ManejadorCallback<ICallbackPrueba>();
            var callback = new CallbackPrueba();

            manejador.Suscribir("usuario1", callback);
            manejador.Desuscribir("usuario1");

            bool existe = manejador.TryGetCallback("usuario1", out _);
            Assert.IsFalse(existe);
        }

        [TestMethod]
        public void Prueba_Desuscribir_NoDeberiaFallarConUsuarioNoExistente()
        {
            var manejador = new ManejadorCallback<ICallbackPrueba>();

            manejador.Desuscribir("usuarioInexistente");
        }

        [TestMethod]
        public void Prueba_Desuscribir_NoDeberiaFallarConNombreNulo()
        {
            var manejador = new ManejadorCallback<ICallbackPrueba>();

            manejador.Desuscribir(null);
        }

        [TestMethod]
        public void Prueba_TryGetCallback_DeberiaRetornarFalseSiNoExiste()
        {
            var manejador = new ManejadorCallback<ICallbackPrueba>();

            bool existe = manejador.TryGetCallback("usuarioInexistente", out var callback);

            Assert.IsFalse(existe);
            Assert.IsNull(callback);
        }

        [TestMethod]
        public void Prueba_TryGetCallback_DeberiaSerCaseInsensitive()
        {
            var manejador = new ManejadorCallback<ICallbackPrueba>();
            var callback = new CallbackPrueba();

            manejador.Suscribir("Usuario1", callback);

            bool existe = manejador.TryGetCallback("usuario1", out var callbackRecuperado);
            Assert.IsTrue(existe);
            Assert.AreSame(callback, callbackRecuperado);
        }

        [TestMethod]
        public void Prueba_Notificar_DeberiaEjecutarAccionSiCallbackExiste()
        {
            var manejador = new ManejadorCallback<ICallbackPrueba>();
            var callback = new CallbackPrueba();

            manejador.Suscribir("usuario1", callback);
            manejador.Notificar("usuario1", cb => cb.Notificar("Mensaje de prueba"));

            Assert.AreEqual(1, callback.VecesLlamado);
            Assert.AreEqual("Mensaje de prueba", callback.UltimoMensaje);
        }

        [TestMethod]
        public void Prueba_Notificar_NoDeberiaFallarSiCallbackNoExiste()
        {
            var manejador = new ManejadorCallback<ICallbackPrueba>();

            manejador.Notificar("usuarioInexistente", cb => cb.Notificar("Mensaje"));
        }

        [TestMethod]
        public void Prueba_Constructor_ConComparadorPersonalizado()
        {
            var manejador = new ManejadorCallback<ICallbackPrueba>(StringComparer.Ordinal);
            var callback = new CallbackPrueba();

            manejador.Suscribir("Usuario1", callback);

            // Con comparador Ordinal (case-sensitive), "usuario1" no debería encontrarse
            bool existe = manejador.TryGetCallback("usuario1", out _);
            Assert.IsFalse(existe);

            // Pero "Usuario1" sí debería encontrarse
            existe = manejador.TryGetCallback("Usuario1", out _);
            Assert.IsTrue(existe);
        }
    }
}
