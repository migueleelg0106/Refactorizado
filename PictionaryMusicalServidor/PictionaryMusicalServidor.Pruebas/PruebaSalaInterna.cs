using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Modelos;
using System;
using System.ServiceModel;

namespace PictionaryMusicalServidor.Pruebas
{
    // Mock callback para pruebas
    public class SalasCallbackMock : ISalasManejadorCallback
    {
        public int ContadorJugadorSeUnio { get; set; }
        public int ContadorJugadorSalio { get; set; }
        public int ContadorListaSalasActualizada { get; set; }
        public int ContadorSalaActualizada { get; set; }
        public int ContadorJugadorExpulsado { get; set; }
        public int ContadorSalaCancelada { get; set; }
        public string UltimoCodigoSala { get; set; }
        public string UltimoNombreJugador { get; set; }

        public void NotificarJugadorSeUnio(string codigoSala, string nombreJugador)
        {
            ContadorJugadorSeUnio++;
            UltimoCodigoSala = codigoSala;
            UltimoNombreJugador = nombreJugador;
        }

        public void NotificarJugadorSalio(string codigoSala, string nombreJugador)
        {
            ContadorJugadorSalio++;
            UltimoCodigoSala = codigoSala;
            UltimoNombreJugador = nombreJugador;
        }

        public void NotificarListaSalasActualizada(SalaDTO[] salas)
        {
            ContadorListaSalasActualizada++;
        }

        public void NotificarSalaActualizada(SalaDTO sala)
        {
            ContadorSalaActualizada++;
        }

        public void NotificarJugadorExpulsado(string codigoSala, string nombreJugador)
        {
            ContadorJugadorExpulsado++;
            UltimoCodigoSala = codigoSala;
            UltimoNombreJugador = nombreJugador;
        }

        public void NotificarSalaCancelada(string codigoSala)
        {
            ContadorSalaCancelada++;
            UltimoCodigoSala = codigoSala;
        }
    }

    [TestClass]
    public class PruebaSalaInterna
    {
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
        public void Prueba_Constructor_DeberiaInicializarPropiedades()
        {
            var configuracion = CrearConfiguracionPrueba();
            var sala = new SalaInterna("ABCD", "Usuario1", configuracion);

            Assert.AreEqual("ABCD", sala.Codigo);
            Assert.AreEqual("Usuario1", sala.Creador);
            Assert.AreSame(configuracion, sala.Configuracion);
            Assert.IsNotNull(sala.Jugadores);
            Assert.AreEqual(0, sala.Jugadores.Count);
            Assert.IsFalse(sala.DebeEliminarse);
        }

        [TestMethod]
        public void Prueba_ConvertirADto_DeberiaConvertirCorrectamente()
        {
            var configuracion = CrearConfiguracionPrueba();
            var sala = new SalaInterna("ABCD", "Usuario1", configuracion);
            
            var dto = sala.ConvertirADto();

            Assert.AreEqual("ABCD", dto.Codigo);
            Assert.AreEqual("Usuario1", dto.Creador);
            Assert.AreSame(configuracion, dto.Configuracion);
            Assert.IsNotNull(dto.Jugadores);
            Assert.AreEqual(0, dto.Jugadores.Count);
        }

        [TestMethod]
        public void Prueba_AgregarJugador_DeberiaAgregarJugadorCorrectamente()
        {
            var sala = new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba());
            var callback = new SalasCallbackMock();

            var resultado = sala.AgregarJugador("Usuario2", callback, false);

            Assert.AreEqual(1, sala.Jugadores.Count);
            Assert.AreEqual("Usuario2", sala.Jugadores[0]);
            Assert.IsNotNull(resultado);
            Assert.AreEqual(1, resultado.Jugadores.Count);
        }

        [TestMethod]
        public void Prueba_AgregarJugador_DeberiaNotificarSiEsRequerido()
        {
            var sala = new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba());
            var callback1 = new SalasCallbackMock();
            var callback2 = new SalasCallbackMock();

            sala.AgregarJugador("Usuario1", callback1, false);
            sala.AgregarJugador("Usuario2", callback2, true);

            // Usuario1 debería ser notificado del nuevo jugador
            Assert.IsTrue(callback1.ContadorJugadorSeUnio > 0);
            Assert.IsTrue(callback1.ContadorSalaActualizada > 0);
        }

        [TestMethod]
        public void Prueba_AgregarJugador_DeberiaReconectarJugadorExistente()
        {
            var sala = new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba());
            var callback1 = new SalasCallbackMock();
            var callback2 = new SalasCallbackMock();

            sala.AgregarJugador("Usuario1", callback1, false);
            var resultado = sala.AgregarJugador("Usuario1", callback2, false);

            Assert.AreEqual(1, sala.Jugadores.Count);
            Assert.AreEqual("Usuario1", sala.Jugadores[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_AgregarJugador_DeberiaLanzarExcepcionSiSalaLlena()
        {
            // MaximoJugadores en SalaInterna es 4
            const int MaximoJugadores = 4;
            var sala = new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba());
            var callback = new SalasCallbackMock();

            for (int i = 1; i <= MaximoJugadores; i++)
            {
                sala.AgregarJugador($"Usuario{i}", callback, false);
            }
            
            // Intento agregar jugador adicional debería fallar
            sala.AgregarJugador($"Usuario{MaximoJugadores + 1}", callback, false);
        }

        [TestMethod]
        public void Prueba_RemoverJugador_DeberiaRemoverJugadorCorrectamente()
        {
            var sala = new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba());
            var callback = new SalasCallbackMock();

            sala.AgregarJugador("Usuario1", callback, false);
            sala.AgregarJugador("Usuario2", callback, false);
            sala.RemoverJugador("Usuario2");

            Assert.AreEqual(1, sala.Jugadores.Count);
            Assert.AreEqual("Usuario1", sala.Jugadores[0]);
        }

        [TestMethod]
        public void Prueba_RemoverJugador_DeberiaMarcarParaEliminarSiSaleCreador()
        {
            var sala = new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba());
            var callback = new SalasCallbackMock();

            sala.AgregarJugador("Usuario1", callback, false);
            sala.AgregarJugador("Usuario2", callback, false);
            sala.RemoverJugador("Usuario1");

            Assert.IsTrue(sala.DebeEliminarse);
        }

        [TestMethod]
        public void Prueba_RemoverJugador_DeberiaMarcarParaEliminarSiSalaVacia()
        {
            var sala = new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba());
            var callback = new SalasCallbackMock();

            sala.AgregarJugador("Usuario2", callback, false);
            sala.RemoverJugador("Usuario2");

            Assert.IsTrue(sala.DebeEliminarse);
        }

        [TestMethod]
        public void Prueba_RemoverJugador_NoDeberiaFallarConJugadorInexistente()
        {
            var sala = new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba());

            sala.RemoverJugador("UsuarioInexistente");

            Assert.IsFalse(sala.DebeEliminarse);
        }

        [TestMethod]
        public void Prueba_ExpulsarJugador_DeberiaExpulsarCorrectamente()
        {
            var sala = new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba());
            var callback1 = new SalasCallbackMock();
            var callback2 = new SalasCallbackMock();

            sala.AgregarJugador("Usuario1", callback1, false);
            sala.AgregarJugador("Usuario2", callback2, false);
            sala.ExpulsarJugador("Usuario1", "Usuario2");

            Assert.AreEqual(1, sala.Jugadores.Count);
            Assert.AreEqual("Usuario1", sala.Jugadores[0]);
            Assert.IsTrue(callback2.ContadorJugadorExpulsado > 0);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_ExpulsarJugador_DeberiaFallarSiNoEsCreador()
        {
            var sala = new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba());
            var callback = new SalasCallbackMock();

            sala.AgregarJugador("Usuario1", callback, false);
            sala.AgregarJugador("Usuario2", callback, false);
            sala.AgregarJugador("Usuario3", callback, false);
            
            // Usuario2 intenta expulsar a Usuario3, pero no es el creador
            sala.ExpulsarJugador("Usuario2", "Usuario3");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_ExpulsarJugador_DeberiaFallarSiIntentaExpulsarCreador()
        {
            var sala = new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba());
            var callback = new SalasCallbackMock();

            sala.AgregarJugador("Usuario1", callback, false);
            
            // El creador no puede expulsarse a sí mismo
            sala.ExpulsarJugador("Usuario1", "Usuario1");
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void Prueba_ExpulsarJugador_DeberiaFallarSiJugadorNoExiste()
        {
            var sala = new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba());
            var callback = new SalasCallbackMock();

            sala.AgregarJugador("Usuario1", callback, false);
            
            sala.ExpulsarJugador("Usuario1", "UsuarioInexistente");
        }

        [TestMethod]
        public void Prueba_AgregarJugador_DeberiaSerCaseInsensitive()
        {
            var sala = new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba());
            var callback = new SalasCallbackMock();

            sala.AgregarJugador("USUARIO1", callback, false);
            var resultado = sala.AgregarJugador("usuario1", callback, false);

            // Debería reconectar, no agregar nuevo jugador
            Assert.AreEqual(1, sala.Jugadores.Count);
        }

        [TestMethod]
        public void Prueba_ConvertirADto_DeberiaCrearCopiaIndependiente()
        {
            var sala = new SalaInterna("ABCD", "Usuario1", CrearConfiguracionPrueba());
            var callback = new SalasCallbackMock();

            sala.AgregarJugador("Usuario1", callback, false);
            var dto1 = sala.ConvertirADto();
            
            sala.AgregarJugador("Usuario2", callback, false);
            var dto2 = sala.ConvertirADto();

            // Los DTOs deben tener listas independientes
            Assert.AreEqual(1, dto1.Jugadores.Count);
            Assert.AreEqual(2, dto2.Jugadores.Count);
        }
    }
}
