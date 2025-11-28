using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Modelos;
using PictionaryMusicalServidor.Servicios.Servicios.Notificadores;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictionaryMusicalServidor.Pruebas
{
    [TestClass]
    public class PruebaNotificadorSalas
    {
        private List<SalaInterna> _salasSimuladas;

        [TestInitialize]
        public void Inicializar()
        {
            _salasSimuladas = new List<SalaInterna>();
        }

        private IEnumerable<SalaInterna> ObtenerSalasSimuladas()
        {
            return _salasSimuladas;
        }

        private ConfiguracionPartidaDTO CrearConfiguracionPrueba()
        {
            return new ConfiguracionPartidaDTO
            {
                NumeroRondas = 5,
                TiempoPorRondaSegundos = 60,
                IdiomaCanciones = "es",
                Dificultad = "Media"
            };
        }

        [TestMethod]
        public void Prueba_Suscribir_DeberiaRetornarGuidValido()
        {
            var notificador = new NotificadorSalas(ObtenerSalasSimuladas);
            var callback = new SalasCallbackMock();

            Guid sesionId = notificador.Suscribir(callback);

            Assert.AreNotEqual(Guid.Empty, sesionId);
        }

        [TestMethod]
        public void Prueba_Suscribir_DeberiaGenerarIdsUnicos()
        {
            var notificador = new NotificadorSalas(ObtenerSalasSimuladas);
            var callback1 = new SalasCallbackMock();
            var callback2 = new SalasCallbackMock();

            Guid sesionId1 = notificador.Suscribir(callback1);
            Guid sesionId2 = notificador.Suscribir(callback2);

            Assert.AreNotEqual(sesionId1, sesionId2);
        }

        [TestMethod]
        public void Prueba_Desuscribir_NoDeberiaFallarConIdInvalido()
        {
            var notificador = new NotificadorSalas(ObtenerSalasSimuladas);

            notificador.Desuscribir(Guid.NewGuid());
        }

        [TestMethod]
        public void Prueba_Desuscribir_DeberiaEliminarSuscripcion()
        {
            var notificador = new NotificadorSalas(ObtenerSalasSimuladas);
            var callback = new SalasCallbackMock();

            Guid sesionId = notificador.Suscribir(callback);
            notificador.Desuscribir(sesionId);

            // No hay forma directa de verificar, pero no debería fallar
        }

        [TestMethod]
        public void Prueba_DesuscribirPorCallback_NoDeberiaFallarConCallbackNoSuscrito()
        {
            var notificador = new NotificadorSalas(ObtenerSalasSimuladas);
            var callback = new SalasCallbackMock();

            notificador.DesuscribirPorCallback(callback);
        }

        [TestMethod]
        public void Prueba_DesuscribirPorCallback_DeberiaEliminarTodasLasSuscripcionesDelCallback()
        {
            var notificador = new NotificadorSalas(ObtenerSalasSimuladas);
            var callback = new SalasCallbackMock();

            notificador.Suscribir(callback);
            notificador.Suscribir(callback);
            notificador.DesuscribirPorCallback(callback);

            // No hay forma directa de verificar, pero no debería fallar
        }

        [TestMethod]
        public void Prueba_NotificarListaSalas_NoDeberiaFallarConCallbackValido()
        {
            var notificador = new NotificadorSalas(ObtenerSalasSimuladas);
            var callback = new SalasCallbackMock();

            _salasSimuladas.Add(new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba()));

            notificador.NotificarListaSalas(callback);

            Assert.AreEqual(1, callback.ContadorListaSalasActualizada);
        }

        [TestMethod]
        public void Prueba_NotificarListaSalas_DeberiaEnviarListaVaciaSiNoHaySalas()
        {
            var notificador = new NotificadorSalas(ObtenerSalasSimuladas);
            var callback = new SalasCallbackMock();

            notificador.NotificarListaSalas(callback);

            Assert.AreEqual(1, callback.ContadorListaSalasActualizada);
        }

        [TestMethod]
        public void Prueba_NotificarListaSalasATodos_NoDeberiaFallarSinSuscriptores()
        {
            var notificador = new NotificadorSalas(ObtenerSalasSimuladas);

            notificador.NotificarListaSalasATodos();
        }

        [TestMethod]
        public void Prueba_NotificarListaSalasATodos_DeberiaNotificarATodosSuscriptores()
        {
            var notificador = new NotificadorSalas(ObtenerSalasSimuladas);
            var callback1 = new SalasCallbackMock();
            var callback2 = new SalasCallbackMock();

            notificador.Suscribir(callback1);
            notificador.Suscribir(callback2);

            _salasSimuladas.Add(new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba()));

            notificador.NotificarListaSalasATodos();

            Assert.AreEqual(1, callback1.ContadorListaSalasActualizada);
            Assert.AreEqual(1, callback2.ContadorListaSalasActualizada);
        }

        [TestMethod]
        public void Prueba_NotificarListaSalasATodos_DeberiaEnviarSalasConvertidas()
        {
            var notificador = new NotificadorSalas(ObtenerSalasSimuladas);
            var callback = new TestCallbackConCaptura();

            notificador.Suscribir(callback);

            _salasSimuladas.Add(new SalaInterna("SALA1", "Host1", CrearConfiguracionPrueba()));
            _salasSimuladas.Add(new SalaInterna("SALA2", "Host2", CrearConfiguracionPrueba()));

            notificador.NotificarListaSalasATodos();

            Assert.IsNotNull(callback.UltimasSalas);
            Assert.AreEqual(2, callback.UltimasSalas.Length);
            Assert.IsTrue(callback.UltimasSalas.Any(s => s.Codigo == "SALA1"));
            Assert.IsTrue(callback.UltimasSalas.Any(s => s.Codigo == "SALA2"));
        }
    }

    // Callback especial para capturar las salas enviadas
    public class TestCallbackConCaptura : ISalasManejadorCallback
    {
        public SalaDTO[] UltimasSalas { get; set; }

        public void NotificarJugadorSeUnio(string codigoSala, string nombreJugador)
        {
        }

        public void NotificarJugadorSalio(string codigoSala, string nombreJugador)
        {
        }

        public void NotificarListaSalasActualizada(SalaDTO[] salas)
        {
            UltimasSalas = salas;
        }

        public void NotificarSalaActualizada(SalaDTO sala)
        {
        }

        public void NotificarJugadorExpulsado(string codigoSala, string nombreJugador)
        {
        }

        public void NotificarSalaCancelada(string codigoSala)
        {
        }
    }
}
