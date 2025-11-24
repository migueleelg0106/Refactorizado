using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Notificadores;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace PictionaryMusicalServidor.Pruebas
{
    [TestClass]
    public class PruebaAmigosManejador
    {
        private AmigosManejador _manejador;
        private FakeUsuarioRepositorio _usuarioRepositorio;
        private FakeAmigoRepositorio _amigoRepositorio;
        private ManejadorCallback<IAmigosManejadorCallback> _manejadorCallback;
        private CallbackAmigosPrueba _callback;

        [TestInitialize]
        public void Inicializar()
        {
            _usuarioRepositorio = new FakeUsuarioRepositorio();
            _amigoRepositorio = new FakeAmigoRepositorio();
            _manejadorCallback = new ManejadorCallback<IAmigosManejadorCallback>(StringComparer.OrdinalIgnoreCase);
            _callback = new CallbackAmigosPrueba();

            AmistadServicio.CrearContexto = () => new FakeContexto();
            AmistadServicio.CrearAmigoRepositorio = _ => _amigoRepositorio;

            _manejador = new AmigosManejador(
                () => new FakeContexto(),
                _ => _usuarioRepositorio,
                _ => _amigoRepositorio,
                _manejadorCallback,
                new NotificadorAmigos(_manejadorCallback),
                () => _callback);
        }

        [TestCleanup]
        public void Limpiar()
        {
            AmistadServicio.RestablecerDependencias();
        }

        [TestMethod]
        public void Suscribir_DeberiaLanzarCuandoNombreInvalido()
        {
            var excepcion = Assert.ThrowsException<FaultException>(() => _manejador.Suscribir(" "));

            Assert.AreEqual(MensajesError.Cliente.NombreUsuarioObligatorioSuscripcion, excepcion.Message);
        }

        [TestMethod]
        public void Suscribir_DeberiaLanzarCuandoUsuarioNoExiste()
        {
            var excepcion = Assert.ThrowsException<FaultException>(() => _manejador.Suscribir("nadie"));

            Assert.AreEqual(MensajesError.Cliente.UsuarioNoEncontrado, excepcion.Message);
        }

        [TestMethod]
        public void Suscribir_DeberiaRegistrarCallbackYNotificarPendientes()
        {
            _usuarioRepositorio.UsuariosPorNombre["Carlos"] = new Usuario { idUsuario = 1, Nombre_Usuario = "Carlos" };
            _amigoRepositorio.SolicitudesPendientes.Add(new Amigo
            {
                UsuarioEmisor = 2,
                UsuarioReceptor = 1,
                Estado = false,
                Usuario = new Usuario { Nombre_Usuario = "Emisor" },
                Usuario1 = new Usuario { Nombre_Usuario = "Carlos" }
            });

            _manejador.Suscribir("Carlos");

            Assert.IsTrue(_manejadorCallback.TryGetCallback("Carlos", out var registrado));
            Assert.AreSame(_callback, registrado);
            Assert.AreEqual(1, _callback.SolicitudesRecibidas.Count);
            Assert.AreEqual("Emisor", _callback.SolicitudesRecibidas[0].UsuarioEmisor);
        }

        [TestMethod]
        public void CancelarSuscripcion_DeberiaLanzarCuandoNombreVacio()
        {
            var excepcion = Assert.ThrowsException<FaultException>(() => _manejador.CancelarSuscripcion(null));

            Assert.AreEqual(MensajesError.Cliente.NombreUsuarioObligatorioCancelar, excepcion.Message);
        }

        [TestMethod]
        public void EnviarSolicitudAmistad_DeberiaLanzarSiEmisorNoExiste()
        {
            _usuarioRepositorio.UsuariosPorNombre["Receptor"] = new Usuario { idUsuario = 2, Nombre_Usuario = "Receptor" };

            var excepcion = Assert.ThrowsException<FaultException>(() => _manejador.EnviarSolicitudAmistad("Emisor", "Receptor"));

            Assert.AreEqual(MensajesError.Cliente.JugadorNoAsociado, excepcion.Message);
        }

        [TestMethod]
        public void EnviarSolicitudAmistad_DeberiaNotificarReceptor()
        {
            _usuarioRepositorio.UsuariosPorNombre["Emisor"] = new Usuario { idUsuario = 3, Nombre_Usuario = "Emisor" };
            _usuarioRepositorio.UsuariosPorNombre["Receptor"] = new Usuario { idUsuario = 4, Nombre_Usuario = "Receptor" };

            _manejador.EnviarSolicitudAmistad("Emisor", "Receptor");

            Assert.AreEqual(1, _callback.SolicitudesRecibidas.Count);
            Assert.IsFalse(_callback.SolicitudesRecibidas[0].SolicitudAceptada);
            CollectionAssert.Contains(_amigoRepositorio.SolicitudesCreadas, (3, 4));
        }

        [TestMethod]
        public void ResponderSolicitudAmistad_DeberiaLanzarCuandoUsuarioNoExiste()
        {
            _usuarioRepositorio.UsuariosPorNombre["Emisor"] = new Usuario { idUsuario = 5, Nombre_Usuario = "Emisor" };

            var excepcion = Assert.ThrowsException<FaultException>(() => _manejador.ResponderSolicitudAmistad("Emisor", "Receptor"));

            Assert.AreEqual(MensajesError.Cliente.UsuariosEspecificadosNoExisten, excepcion.Message);
        }

        [TestMethod]
        public void EliminarAmigo_DeberiaLanzarCuandoDatosInvalidos()
        {
            var excepcion = Assert.ThrowsException<FaultException>(() => _manejador.EliminarAmigo(" ", "Otro"));

            Assert.AreEqual(MensajesError.Cliente.NombreUsuarioObligatorio, excepcion.Message);
        }
    }

    internal class FakeUsuarioRepositorio : IUsuarioRepositorio
    {
        public Dictionary<string, Usuario> UsuariosPorNombre { get; } = new(StringComparer.OrdinalIgnoreCase);

        public bool ExisteNombreUsuario(string nombreUsuario)
        {
            return UsuariosPorNombre.ContainsKey(nombreUsuario);
        }

        public Usuario CrearUsuario(Usuario usuario)
        {
            UsuariosPorNombre[usuario.Nombre_Usuario] = usuario;
            return usuario;
        }

        public Usuario ObtenerPorNombreUsuario(string nombreUsuario)
        {
            UsuariosPorNombre.TryGetValue(nombreUsuario, out var usuario);
            return usuario;
        }
    }

    internal class FakeAmigoRepositorio : IAmigoRepositorio
    {
        public List<(int, int)> SolicitudesCreadas { get; } = new();
        public List<Amigo> SolicitudesPendientes { get; } = new();

        public void ActualizarEstado(Amigo relacion, bool estado)
        {
        }

        public Amigo CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId)
        {
            SolicitudesCreadas.Add((usuarioEmisorId, usuarioReceptorId));
            return new Amigo { UsuarioEmisor = usuarioEmisorId, UsuarioReceptor = usuarioReceptorId, Estado = false };
        }

        public void EliminarRelacion(Amigo relacion)
        {
        }

        public bool ExisteRelacion(int usuarioAId, int usuarioBId)
        {
            return false;
        }

        public IList<Usuario> ObtenerAmigos(int usuarioId)
        {
            return new List<Usuario>();
        }

        public Amigo ObtenerRelacion(int usuarioAId, int usuarioBId)
        {
            return new Amigo { UsuarioEmisor = usuarioAId, UsuarioReceptor = usuarioBId };
        }

        public IList<Amigo> ObtenerSolicitudesPendientes(int usuarioId)
        {
            return SolicitudesPendientes;
        }
    }

    internal class CallbackAmigosPrueba : IAmigosManejadorCallback
    {
        public List<SolicitudAmistadDTO> SolicitudesRecibidas { get; } = new();

        public void NotificarAmistadEliminada(SolicitudAmistadDTO solicitud)
        {
            SolicitudesRecibidas.Add(solicitud);
        }

        public void NotificarSolicitudActualizada(SolicitudAmistadDTO solicitud)
        {
            SolicitudesRecibidas.Add(solicitud);
        }
    }
}
