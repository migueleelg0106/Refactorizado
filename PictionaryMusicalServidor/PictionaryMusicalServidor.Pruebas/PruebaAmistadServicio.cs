using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Servicios.Servicios;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using System;
using System.Collections.Generic;

namespace PictionaryMusicalServidor.Pruebas
{
    [TestClass]
    public class PruebaAmistadServicio
    {
        private FakeAmigoRepositorio _amigoRepositorio;

        [TestInitialize]
        public void Inicializar()
        {
            _amigoRepositorio = new FakeAmigoRepositorio();
            // Las factorías configurables en AmistadServicio permiten aislar las pruebas
            // del contexto y los repositorios reales, sin tocar la implementación de
            // producción. Si se restablecen, el servicio vuelve a su comportamiento
            // original.
            AmistadServicio.CrearContexto = () => new FakeContexto();
            AmistadServicio.CrearAmigoRepositorio = _ => _amigoRepositorio;
        }

        [TestCleanup]
        public void Limpiar()
        {
            AmistadServicio.RestablecerDependencias();
        }

        [TestMethod]
        public void CrearSolicitud_DeberiaLanzarSiUsuariosIguales()
        {
            var excepcion = Assert.ThrowsException<InvalidOperationException>(() => AmistadServicio.CrearSolicitud(5, 5));

            Assert.AreEqual(MensajesError.Cliente.SolicitudAmistadMismoUsuario, excepcion.Message);
        }

        [TestMethod]
        public void CrearSolicitud_DeberiaLanzarSiRelacionExiste()
        {
            _amigoRepositorio.RelacionesExistentes.Add((1, 2));

            var excepcion = Assert.ThrowsException<InvalidOperationException>(() => AmistadServicio.CrearSolicitud(1, 2));

            Assert.AreEqual(MensajesError.Cliente.RelacionAmistadExistente, excepcion.Message);
        }

        [TestMethod]
        public void CrearSolicitud_DeberiaCrearRelacion()
        {
            AmistadServicio.CrearSolicitud(3, 4);

            Assert.AreEqual(1, _amigoRepositorio.SolicitudesCreadas.Count);
            Assert.AreEqual((3, 4), _amigoRepositorio.SolicitudesCreadas[0]);
        }

        [TestMethod]
        public void AceptarSolicitud_DeberiaLanzarSiNoExiste()
        {
            var excepcion = Assert.ThrowsException<InvalidOperationException>(() => AmistadServicio.AceptarSolicitud(1, 2));

            Assert.AreEqual(MensajesError.Cliente.SolicitudAmistadNoExiste, excepcion.Message);
        }

        [TestMethod]
        public void AceptarSolicitud_DeberiaLanzarSiReceptorIncorrecto()
        {
            _amigoRepositorio.RelacionesPorPar[(1, 2)] = new Amigo { UsuarioEmisor = 1, UsuarioReceptor = 3, Estado = false };

            var excepcion = Assert.ThrowsException<InvalidOperationException>(() => AmistadServicio.AceptarSolicitud(1, 2));

            Assert.AreEqual(MensajesError.Cliente.ErrorAceptarSolicitud, excepcion.Message);
        }

        [TestMethod]
        public void AceptarSolicitud_DeberiaLanzarSiYaAceptada()
        {
            _amigoRepositorio.RelacionesPorPar[(1, 2)] = new Amigo { UsuarioEmisor = 1, UsuarioReceptor = 2, Estado = true };

            var excepcion = Assert.ThrowsException<InvalidOperationException>(() => AmistadServicio.AceptarSolicitud(1, 2));

            Assert.AreEqual(MensajesError.Cliente.SolicitudAmistadYaAceptada, excepcion.Message);
        }

        [TestMethod]
        public void AceptarSolicitud_DeberiaActualizarEstado()
        {
            var relacion = new Amigo { UsuarioEmisor = 1, UsuarioReceptor = 2, Estado = false };
            _amigoRepositorio.RelacionesPorPar[(1, 2)] = relacion;

            AmistadServicio.AceptarSolicitud(1, 2);

            Assert.IsTrue(relacion.Estado);
            CollectionAssert.Contains(_amigoRepositorio.RelacionesActualizadas, relacion);
        }

        [TestMethod]
        public void EliminarAmistad_DeberiaLanzarSiUsuariosIguales()
        {
            var excepcion = Assert.ThrowsException<InvalidOperationException>(() => AmistadServicio.EliminarAmistad(2, 2));

            Assert.AreEqual(MensajesError.Cliente.ErrorEliminarAmistad, excepcion.Message);
        }

        [TestMethod]
        public void EliminarAmistad_DeberiaLanzarSiRelacionNoExiste()
        {
            var excepcion = Assert.ThrowsException<InvalidOperationException>(() => AmistadServicio.EliminarAmistad(2, 3));

            Assert.AreEqual(MensajesError.Cliente.RelacionAmistadNoExiste, excepcion.Message);
        }

        [TestMethod]
        public void EliminarAmistad_DeberiaRetornarRelacionEliminada()
        {
            var relacion = new Amigo { UsuarioEmisor = 4, UsuarioReceptor = 5 };
            _amigoRepositorio.RelacionesPorPar[(4, 5)] = relacion;

            var resultado = AmistadServicio.EliminarAmistad(4, 5);

            Assert.AreSame(relacion, resultado);
            CollectionAssert.Contains(_amigoRepositorio.RelacionesEliminadas, relacion);
        }

        [TestMethod]
        public void ObtenerSolicitudesPendientesDTO_DeberiaFiltrarNulasYSoloReceptor()
        {
            _amigoRepositorio.SolicitudesPendientes.Add(new Amigo { UsuarioEmisor = 1, UsuarioReceptor = 2, Estado = false, Usuario = new Usuario { Nombre_Usuario = "A" }, Usuario1 = new Usuario { Nombre_Usuario = "B" } });
            _amigoRepositorio.SolicitudesPendientes.Add(new Amigo { UsuarioEmisor = 3, UsuarioReceptor = 2, Estado = true, Usuario = new Usuario { Nombre_Usuario = null }, Usuario1 = new Usuario { Nombre_Usuario = "C" } });
            _amigoRepositorio.SolicitudesPendientes.Add(new Amigo { UsuarioEmisor = 2, UsuarioReceptor = 1, Estado = false, Usuario = new Usuario { Nombre_Usuario = "D" }, Usuario1 = new Usuario { Nombre_Usuario = "E" } });

            var resultado = AmistadServicio.ObtenerSolicitudesPendientesDTO(2);

            Assert.AreEqual(1, resultado.Count);
            Assert.AreEqual("A", resultado[0].UsuarioEmisor);
            Assert.AreEqual("B", resultado[0].UsuarioReceptor);
            Assert.IsFalse(resultado[0].SolicitudAceptada);
        }

        [TestMethod]
        public void ObtenerAmigosDTO_DeberiaOmitirUsuariosNulos()
        {
            _amigoRepositorio.Amigos.Add(new Usuario { idUsuario = 1, Nombre_Usuario = "Juan" });
            _amigoRepositorio.Amigos.Add(null);

            var amigos = AmistadServicio.ObtenerAmigosDTO(7);

            Assert.AreEqual(1, amigos.Count);
            Assert.AreEqual(1, amigos[0].UsuarioId);
            Assert.AreEqual("Juan", amigos[0].NombreUsuario);
        }
    }

    internal class FakeContexto : BaseDatosPruebaEntities1
    {
        public FakeContexto() : base()
        {
        }
    }

    internal class FakeAmigoRepositorio : IAmigoRepositorio
    {
        public List<(int Emisor, int Receptor)> SolicitudesCreadas { get; } = new();
        public Dictionary<(int, int), Amigo> RelacionesPorPar { get; } = new();
        public List<Amigo> RelacionesActualizadas { get; } = new();
        public List<Amigo> RelacionesEliminadas { get; } = new();
        public List<Amigo> SolicitudesPendientes { get; } = new();
        public List<Usuario> Amigos { get; } = new();
        public List<(int, int)> RelacionesExistentes { get; } = new();

        public void ActualizarEstado(Amigo relacion, bool estado)
        {
            relacion.Estado = estado;
            RelacionesActualizadas.Add(relacion);
        }

        public Amigo CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId)
        {
            SolicitudesCreadas.Add((usuarioEmisorId, usuarioReceptorId));
            var solicitud = new Amigo { UsuarioEmisor = usuarioEmisorId, UsuarioReceptor = usuarioReceptorId, Estado = false };
            RelacionesPorPar[(usuarioEmisorId, usuarioReceptorId)] = solicitud;
            return solicitud;
        }

        public void EliminarRelacion(Amigo relacion)
        {
            RelacionesEliminadas.Add(relacion);
        }

        public bool ExisteRelacion(int usuarioAId, int usuarioBId)
        {
            return RelacionesExistentes.Contains((usuarioAId, usuarioBId));
        }

        public IList<Usuario> ObtenerAmigos(int usuarioId)
        {
            return Amigos;
        }

        public Amigo ObtenerRelacion(int usuarioAId, int usuarioBId)
        {
            RelacionesPorPar.TryGetValue((usuarioAId, usuarioBId), out var relacion);
            return relacion;
        }

        public IList<Amigo> ObtenerSolicitudesPendientes(int usuarioId)
        {
            return SolicitudesPendientes;
        }
    }
}
