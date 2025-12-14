using System;
using System.Collections.Generic;
using Datos.Modelo;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Servicio interno para la gestion de logica de negocio relacionada con amistades.
    /// Proporciona metodos para crear, aceptar, eliminar y consultar relaciones de amistad entre
    /// usuarios.
    /// </summary>
    public class AmistadServicio : IAmistadServicio
    {
        private readonly IContextoFactoria _contextoFactoria;
        private readonly IRepositorioFactoria _repositorioFactoria;

        /// <summary>
        /// Constructor por defecto para uso en WCF.
        /// </summary>
        public AmistadServicio() : this(new ContextoFactoria(), new RepositorioFactoria())
        {
        }

        /// <summary>
        /// Constructor con inyeccion de dependencias para pruebas unitarias.
        /// </summary>
        /// <param name="contextoFactoria">Factoria para crear contextos de base de datos.</param>
        /// <param name="repositorioFactoria">Factoria para crear repositorios.</param>
        public AmistadServicio(
            IContextoFactoria contextoFactoria,
            IRepositorioFactoria repositorioFactoria)
        {
            _contextoFactoria = contextoFactoria ?? 
                throw new ArgumentNullException(nameof(contextoFactoria));
            _repositorioFactoria = repositorioFactoria ??
                throw new ArgumentNullException(nameof(repositorioFactoria));
        }

        /// <summary>
        /// Obtiene las solicitudes de amistad pendientes para un usuario especifico.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario receptor.</param>
        /// <returns>Lista de solicitudes de amistad pendientes como DTOs.</returns>
        public List<SolicitudAmistadDTO> ObtenerSolicitudesPendientesDTO(int usuarioId)
        {
            using (var contexto = _contextoFactoria.CrearContexto())
            {
                var repositorioAmigos = _repositorioFactoria.CrearAmigoRepositorio(contexto);
                var solicitudes = repositorioAmigos.ObtenerSolicitudesPendientes(usuarioId);

                if (solicitudes == null || solicitudes.Count == 0)
                {
                    return new List<SolicitudAmistadDTO>();
                }

                return MapearSolicitudes(solicitudes, usuarioId);
            }
        }

        /// <summary>
        /// Crea una nueva solicitud de amistad entre dos usuarios.
        /// </summary>
        /// <param name="usuarioEmisorId">Identificador del usuario que envia la solicitud.
        /// </param>
        /// <param name="usuarioReceptorId">Identificador del usuario que recibe la solicitud.
        /// </param>
        /// <exception cref="InvalidOperationException">Se lanza si los usuarios son el mismo o 
        /// ya existe una relacion.</exception>
        public void CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId)
        {
            if (usuarioEmisorId == usuarioReceptorId)
            {
                throw new InvalidOperationException(
                    MensajesError.Cliente.SolicitudAmistadMismoUsuario);
            }

            using (var contexto = _contextoFactoria.CrearContexto())
            {
                var repositorioAmigos = _repositorioFactoria.CrearAmigoRepositorio(contexto);
                if (repositorioAmigos.ExisteRelacion(usuarioEmisorId, usuarioReceptorId))
                {
                    throw new InvalidOperationException(
                        MensajesError.Cliente.RelacionAmistadExistente);
                }

                repositorioAmigos.CrearSolicitud(usuarioEmisorId, usuarioReceptorId);
            }
        }

        /// <summary>
        /// Acepta una solicitud de amistad pendiente entre dos usuarios.
        /// </summary>
        /// <param name="usuarioEmisorId">Identificador del usuario que envio la solicitud.
        /// </param>
        /// <param name="usuarioReceptorId">Identificador del usuario que acepta la solicitud.
        /// </param>
        /// <exception cref="InvalidOperationException">Se lanza si no existe la solicitud o ya
        /// fue aceptada.</exception>
        public void AceptarSolicitud(int usuarioEmisorId, int usuarioReceptorId)
        {
            using (var contexto = _contextoFactoria.CrearContexto())
            {
                var repositorioAmigos = _repositorioFactoria.CrearAmigoRepositorio(contexto);
                var relacion = repositorioAmigos.ObtenerRelacion(usuarioEmisorId, usuarioReceptorId);

                ValidarSolicitudParaAceptar(relacion, usuarioReceptorId);
                repositorioAmigos.ActualizarEstado(relacion, true);
            }
        }

        /// <summary>
        /// Elimina la relacion de amistad entre dos usuarios.
        /// </summary>
        /// <param name="usuarioAId">Identificador del primer usuario en la relacion.</param>
        /// <param name="usuarioBId">Identificador del segundo usuario en la relacion.</param>
        /// <returns>La relacion de amistad que fue eliminada.</returns>
        /// <exception cref="InvalidOperationException">Se lanza si los usuarios son el mismo o la
        /// relacion no existe.</exception>
        public Amigo EliminarAmistad(int usuarioAId, int usuarioBId)
        {
            if (usuarioAId == usuarioBId)
            {
                throw new InvalidOperationException(MensajesError.Cliente.ErrorEliminarAmistad);
            }

            using (var contexto = _contextoFactoria.CrearContexto())
            {
                var repositorioAmigos = _repositorioFactoria.CrearAmigoRepositorio(contexto);
                var relacion = repositorioAmigos.ObtenerRelacion(usuarioAId, usuarioBId);

                if (relacion == null)
                {
                    throw new InvalidOperationException(
                        MensajesError.Cliente.RelacionAmistadNoExiste);
                }

                repositorioAmigos.EliminarRelacion(relacion);
                return relacion;
            }
        }

        /// <summary>
        /// Obtiene la lista de amigos de un usuario como objetos DTO.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario cuyos amigos se desean obtener.
        /// </param>
        /// <returns>Lista de amigos como DTOs, o lista vacia si no hay amigos.</returns>
        public List<AmigoDTO> ObtenerAmigosDTO(int usuarioId)
        {
            using (var contexto = _contextoFactoria.CrearContexto())
            {
                var repositorioAmigos = _repositorioFactoria.CrearAmigoRepositorio(contexto);
                var amigos = repositorioAmigos.ObtenerAmigos(usuarioId);

                if (amigos == null)
                {
                    return new List<AmigoDTO>();
                }

                var resultado = new List<AmigoDTO>();
                foreach (var amigo in amigos)
                {
                    if (amigo != null)
                    {
                        resultado.Add(new AmigoDTO
                        {
                            UsuarioId = amigo.idUsuario,
                            NombreUsuario = amigo.Nombre_Usuario
                        });
                    }
                }
                return resultado;
            }
        }

       private List<SolicitudAmistadDTO> MapearSolicitudes(IList<Amigo> solicitudes,
            int usuarioId)
        {
            var resultadoDTOs = new List<SolicitudAmistadDTO>();
            foreach (var solicitud in solicitudes)
            {
                if (solicitud.UsuarioReceptor != usuarioId)
                {
                    continue;
                }

                string emisor = solicitud.Usuario?.Nombre_Usuario;
                string receptor = solicitud.Usuario1?.Nombre_Usuario;

                if (!string.IsNullOrWhiteSpace(emisor) && !string.IsNullOrWhiteSpace(receptor))
                {
                    resultadoDTOs.Add(new SolicitudAmistadDTO
                    {
                        UsuarioEmisor = emisor,
                        UsuarioReceptor = receptor,
                        SolicitudAceptada = solicitud.Estado
                    });
                }
            }
            return resultadoDTOs;
        }

        private void ValidarSolicitudParaAceptar(Amigo relacion, int usuarioReceptorId)
        {
            if (relacion == null)
            {
                throw new InvalidOperationException(
                    MensajesError.Cliente.SolicitudAmistadNoExiste);
            }

            if (relacion.UsuarioReceptor != usuarioReceptorId)
            {
                throw new InvalidOperationException(MensajesError.Cliente.ErrorAceptarSolicitud);
            }

            if (relacion.Estado)
            {
                throw new InvalidOperationException(
                    MensajesError.Cliente.SolicitudAmistadYaAceptada);
            }
        }
    }
}