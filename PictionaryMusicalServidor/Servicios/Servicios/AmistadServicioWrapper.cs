using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using System.Collections.Generic;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Wrapper que implementa IAmistadServicio y delega las llamadas a la clase estatica AmistadServicio.
    /// Permite la inyeccion de dependencias mientras se mantiene la compatibilidad con el codigo existente.
    /// </summary>
    internal class AmistadServicioWrapper : IAmistadServicio
    {
        public List<SolicitudAmistadDTO> ObtenerSolicitudesPendientesDTO(int usuarioId)
        {
            return AmistadServicio.ObtenerSolicitudesPendientesDTO(usuarioId);
        }

        public void CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId)
        {
            AmistadServicio.CrearSolicitud(usuarioEmisorId, usuarioReceptorId);
        }

        public void AceptarSolicitud(int usuarioEmisorId, int usuarioReceptorId)
        {
            AmistadServicio.AceptarSolicitud(usuarioEmisorId, usuarioReceptorId);
        }

        public Amigo EliminarAmistad(int usuarioAId, int usuarioBId)
        {
            return AmistadServicio.EliminarAmistad(usuarioAId, usuarioBId);
        }

        public List<AmigoDTO> ObtenerAmigosDTO(int usuarioId)
        {
            return AmistadServicio.ObtenerAmigosDTO(usuarioId);
        }
    }
}
