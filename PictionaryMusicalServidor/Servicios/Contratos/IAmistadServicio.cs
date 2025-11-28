using System.Collections.Generic;
using Datos.Modelo;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Interfaz para el servicio de gestion de logica de negocio de amistades.
    /// Define operaciones para crear, aceptar, eliminar y consultar relaciones de amistad.
    /// </summary>
    public interface IAmistadServicio
    {
        /// <summary>
        /// Obtiene las solicitudes de amistad pendientes para un usuario especifico.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario receptor.</param>
        /// <returns>Lista de solicitudes de amistad pendientes como DTOs.</returns>
        List<SolicitudAmistadDTO> ObtenerSolicitudesPendientesDTO(int usuarioId);

        /// <summary>
        /// Crea una nueva solicitud de amistad entre dos usuarios.
        /// </summary>
        /// <param name="usuarioEmisorId">Identificador del usuario que envia la solicitud.</param>
        /// <param name="usuarioReceptorId">Identificador del usuario que recibe la solicitud.</param>
        void CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId);

        /// <summary>
        /// Acepta una solicitud de amistad pendiente entre dos usuarios.
        /// </summary>
        /// <param name="usuarioEmisorId">Identificador del usuario que envio la solicitud.</param>
        /// <param name="usuarioReceptorId">Identificador del usuario que acepta la solicitud.</param>
        void AceptarSolicitud(int usuarioEmisorId, int usuarioReceptorId);

        /// <summary>
        /// Elimina la relacion de amistad entre dos usuarios.
        /// </summary>
        /// <param name="usuarioAId">Identificador del primer usuario en la relacion.</param>
        /// <param name="usuarioBId">Identificador del segundo usuario en la relacion.</param>
        /// <returns>La relacion de amistad que fue eliminada.</returns>
        Amigo EliminarAmistad(int usuarioAId, int usuarioBId);

        /// <summary>
        /// Obtiene la lista de amigos de un usuario como objetos DTO.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario cuyos amigos se desean obtener.</param>
        /// <returns>Lista de amigos como DTOs.</returns>
        List<AmigoDTO> ObtenerAmigosDTO(int usuarioId);
    }
}
