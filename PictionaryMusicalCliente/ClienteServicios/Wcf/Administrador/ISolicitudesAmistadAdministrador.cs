using System.Collections.Generic;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Administrador
{
    /// <summary>
    /// Define las operaciones para el manejo de las solicitudes de amistad en memoria.
    /// </summary>
    public interface ISolicitudesAmistadAdministrador
    {
        /// <summary>
        /// Devuelve una copia de las solicitudes almacenadas actualmente.
        /// </summary>
        /// <returns>
        /// Colección de solo lectura con las solicitudes existentes o vacía si no hay elementos.
        /// </returns>
        IReadOnlyCollection<DTOs.SolicitudAmistadDTO> ObtenerSolicitudes();

        /// <summary>
        /// Actualiza una solicitud según su estado, eliminándola si fue aceptada o
        /// guardándola cuando está pendiente.
        /// </summary>
        /// <param name="solicitud">La solicitud recibida desde el servicio.</param>
        /// <param name="usuarioActual">
        /// Usuario activo que recibe o tiene la solicitud asociada.
        /// </param>
        /// <returns>
        /// <see langword="true"/> si la solicitud fue modificada o eliminada en la lista;
        /// <see langword="false"/> en caso contrario.
        /// </returns>
        bool ActualizarSolicitud(DTOs.SolicitudAmistadDTO solicitud, string usuarioActual);

        /// <summary>
        /// Elimina una amistad cuando se recibe una notificación de eliminación.
        /// </summary>
        /// <param name="solicitud">Solicitud que contiene los datos de los usuarios.</param>
        /// <param name="usuarioActual">
        /// Usuario al que se debe eliminar la relación de amistad.
        /// </param>
        /// <returns>
        /// <see langword="true"/> si se encontró y eliminó la relación; de lo contrario,
        /// <see langword="false"/>.
        /// </returns>
        bool EliminaAmistadParaUsuario(DTOs.SolicitudAmistadDTO solicitud, string usuarioActual);

        /// <summary>
        /// Limpia todas las solicitudes almacenadas.
        /// </summary>
        void LimpiarSolicitudes();
    }
}