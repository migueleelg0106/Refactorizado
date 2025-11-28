using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using Datos.Modelo;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using log4net;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Servicio interno para la gestion de logica de negocio relacionada con amistades.
    /// Proporciona metodos para crear, aceptar, eliminar y consultar relaciones de amistad entre usuarios.
    /// </summary>
    public class AmistadServicio : IAmistadServicio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AmistadServicio));
        private readonly IContextoFactory _contextoFactory;

        /// <summary>
        /// Constructor que inyecta la factoría de contextos.
        /// </summary>
        /// <param name="contextoFactory">Factoría para crear contextos de base de datos.</param>
        public AmistadServicio(IContextoFactory contextoFactory)
        {
            _contextoFactory = contextoFactory ?? throw new ArgumentNullException(nameof(contextoFactory));
        }

        /// <summary>
        /// Obtiene las solicitudes de amistad pendientes para un usuario especifico.
        /// Filtra solo las solicitudes donde el usuario es el receptor.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario receptor.</param>
        /// <returns>Lista de solicitudes de amistad pendientes como DTOs.</returns>
        public List<SolicitudAmistadDTO> ObtenerSolicitudesPendientesDTO(int usuarioId)
        {
            using (var contexto = _contextoFactory.CrearContexto())
            {
                var amigoRepositorio = new AmigoRepositorio(contexto);
                var solicitudesPendientes = amigoRepositorio.ObtenerSolicitudesPendientes(usuarioId);

                if (solicitudesPendientes == null || solicitudesPendientes.Count == 0)
                {
                    return new List<SolicitudAmistadDTO>();
                }

                var resultadoDTOs = new List<SolicitudAmistadDTO>();
                foreach (var solicitud in solicitudesPendientes)
                {
                    if (solicitud.UsuarioReceptor != usuarioId)
                    {
                        continue;
                    }

                    string emisor = solicitud.Usuario?.Nombre_Usuario;
                    string receptor = solicitud.Usuario1?.Nombre_Usuario;

                    if (string.IsNullOrWhiteSpace(emisor) || string.IsNullOrWhiteSpace(receptor))
                    {
                        continue;
                    }

                    resultadoDTOs.Add(new SolicitudAmistadDTO
                    {
                        UsuarioEmisor = emisor,
                        UsuarioReceptor = receptor,
                        SolicitudAceptada = solicitud.Estado
                    });
                }
                return resultadoDTOs;
            }
        }

        /// <summary>
        /// Crea una nueva solicitud de amistad entre dos usuarios.
        /// Valida que los usuarios sean diferentes y que no exista ya una relacion.
        /// </summary>
        /// <param name="usuarioEmisorId">Identificador del usuario que envia la solicitud.</param>
        /// <param name="usuarioReceptorId">Identificador del usuario que recibe la solicitud.</param>
        /// <exception cref="InvalidOperationException">Se lanza si los usuarios son el mismo o ya existe una relacion.</exception>
        public void CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId)
        {
            if (usuarioEmisorId == usuarioReceptorId)
            {
                _logger.WarnFormat("Intento de auto-solicitud de amistad por usuario ID: {0}", usuarioEmisorId);
                throw new InvalidOperationException(MensajesError.Cliente.SolicitudAmistadMismoUsuario);
            }

            using (var contexto = _contextoFactory.CrearContexto())
            {
                var amigoRepositorio = new AmigoRepositorio(contexto);
                if (amigoRepositorio.ExisteRelacion(usuarioEmisorId, usuarioReceptorId))
                {
                    _logger.WarnFormat("Intento de crear solicitud de amistad existente entre {0} y {1}", usuarioEmisorId, usuarioReceptorId);
                    throw new InvalidOperationException(MensajesError.Cliente.RelacionAmistadExistente);
                }

                amigoRepositorio.CrearSolicitud(usuarioEmisorId, usuarioReceptorId);
                _logger.InfoFormat("Solicitud de amistad creada. Emisor ID: {0}, Receptor ID: {1}", usuarioEmisorId, usuarioReceptorId);
            }
        }

        /// <summary>
        /// Acepta una solicitud de amistad pendiente entre dos usuarios.
        /// Actualiza el estado de la relacion a aceptada en la base de datos.
        /// </summary>
        /// <param name="usuarioEmisorId">Identificador del usuario que envio la solicitud.</param>
        /// <param name="usuarioReceptorId">Identificador del usuario que acepta la solicitud.</param>
        /// <exception cref="InvalidOperationException">Se lanza si no existe la solicitud o ya fue aceptada.</exception>
        public void AceptarSolicitud(int usuarioEmisorId, int usuarioReceptorId)
        {
            using (var contexto = _contextoFactory.CrearContexto())
            {
                var amigoRepositorio = new AmigoRepositorio(contexto);
                var relacion = amigoRepositorio.ObtenerRelacion(usuarioEmisorId, usuarioReceptorId);

                if (relacion == null)
                {
                    _logger.WarnFormat("Intento de aceptar solicitud inexistente entre {0} y {1}", usuarioEmisorId, usuarioReceptorId);
                    throw new InvalidOperationException(MensajesError.Cliente.SolicitudAmistadNoExiste);
                }

                if (relacion.UsuarioReceptor != usuarioReceptorId)
                {
                    _logger.WarnFormat("Usuario ID: {0} intentó aceptar una solicitud que no le corresponde (Receptor real: {1})", usuarioReceptorId, relacion.UsuarioReceptor);
                    throw new InvalidOperationException(MensajesError.Cliente.ErrorAceptarSolicitud);
                }

                if (relacion.Estado)
                {
                    _logger.WarnFormat("Intento de aceptar una solicitud ya aceptada entre {0} y {1}", usuarioEmisorId, usuarioReceptorId);
                    throw new InvalidOperationException(MensajesError.Cliente.SolicitudAmistadYaAceptada);
                }

                amigoRepositorio.ActualizarEstado(relacion, true);
                _logger.InfoFormat("Solicitud de amistad aceptada entre Emisor ID: {0} y Receptor ID: {1}", usuarioEmisorId, usuarioReceptorId);
            }
        }

        /// <summary>
        /// Elimina la relacion de amistad entre dos usuarios.
        /// Valida que los usuarios no sean el mismo y que exista la relacion antes de eliminarla.
        /// </summary>
        /// <param name="usuarioAId">Identificador del primer usuario en la relacion.</param>
        /// <param name="usuarioBId">Identificador del segundo usuario en la relacion.</param>
        /// <returns>La relacion de amistad que fue eliminada.</returns>
        /// <exception cref="InvalidOperationException">Se lanza si los usuarios son el mismo o la relacion no existe.</exception>
        public Amigo EliminarAmistad(int usuarioAId, int usuarioBId)
        {
            if (usuarioAId == usuarioBId)
            {
                throw new InvalidOperationException(MensajesError.Cliente.ErrorEliminarAmistad);
            }

            using (var contexto = _contextoFactory.CrearContexto())
            {
                var amigoRepositorio = new AmigoRepositorio(contexto);
                var relacion = amigoRepositorio.ObtenerRelacion(usuarioAId, usuarioBId);

                if (relacion == null)
                {
                    _logger.WarnFormat("Intento de eliminar relación inexistente entre {0} y {1}", usuarioAId, usuarioBId);
                    throw new InvalidOperationException(MensajesError.Cliente.RelacionAmistadNoExiste);
                }

                amigoRepositorio.EliminarRelacion(relacion);
                _logger.InfoFormat("Relación de amistad eliminada entre ID: {0} e ID: {1}", usuarioAId, usuarioBId);
                return relacion;
            }
        }

        /// <summary>
        /// Obtiene la lista de amigos de un usuario como objetos DTO.
        /// Filtra amigos nulos y retorna una lista vacia si no hay amigos.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario cuyos amigos se desean obtener.</param>
        /// <returns>Lista de amigos como DTOs, o lista vacia si no hay amigos.</returns>
        public List<AmigoDTO> ObtenerAmigosDTO(int usuarioId)
        {
            using (var contexto = _contextoFactory.CrearContexto())
            {
                var amigoRepositorio = new AmigoRepositorio(contexto);
                IList<Usuario> amigos = amigoRepositorio.ObtenerAmigos(usuarioId);

                if (amigos == null)
                {
                    return new List<AmigoDTO>();
                }

                var resultado = new List<AmigoDTO>(amigos.Count);
                foreach (var amigo in amigos)
                {
                    if (amigo == null)
                    {
                        continue;
                    }

                    resultado.Add(new AmigoDTO
                    {
                        UsuarioId = amigo.idUsuario,
                        NombreUsuario = amigo.Nombre_Usuario
                    });
                }

                return resultado;
            }
        }

    }

}