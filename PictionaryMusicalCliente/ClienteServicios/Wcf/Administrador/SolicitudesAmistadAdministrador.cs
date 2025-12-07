using System;
using System.Collections.Generic;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Administrador
{
    /// <summary>
    /// Maneja las operaciones relacionadas con las solicitudes de amistad.
    /// </summary>
    public class SolicitudesAmistadAdministrador : ISolicitudesAmistadAdministrador
    {
        private readonly object _solicitudesBloqueo = new();
        private readonly List<DTOs.SolicitudAmistadDTO> _solicitudes = new();

        /// <summary>
        /// Devuelve una copia de las solicitudes almacenadas actualmente.
        /// </summary>
        /// <returns>
        /// Colección de solo lectura con las solicitudes existentes o vacía si no hay
        /// elementos.
        /// </returns>
        public IReadOnlyCollection<DTOs.SolicitudAmistadDTO> ObtenerSolicitudes()
        {
            lock (_solicitudesBloqueo)
            {
                return _solicitudes.Count == 0
                    ? Array.Empty<DTOs.SolicitudAmistadDTO>()
                    : _solicitudes.ToArray();
            }
        }

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
        public bool ActualizarSolicitud(
            DTOs.SolicitudAmistadDTO solicitud,
            string usuarioActual)
        {
            lock (_solicitudesBloqueo)
            {
                if (solicitud.SolicitudAceptada)
                {
                    return RemoverSolicitudAceptada(solicitud.UsuarioEmisor, usuarioActual);
                }

                return ProcesarSolicitudPendiente(solicitud, usuarioActual);
            }
        }

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
        public bool EliminaAmistadParaUsuario(
            DTOs.SolicitudAmistadDTO solicitud,
            string usuarioActual)
        {
            lock (_solicitudesBloqueo)
            {
                return RemoverSiCoincide(solicitud.UsuarioEmisor, usuarioActual);
            }
        }

        /// <summary>
        /// Limpia todas las solicitudes almacenadas.
        /// </summary>
        public void LimpiarSolicitudes()
        {
            lock (_solicitudesBloqueo)
            {
                _solicitudes.Clear();
            }
        }

        private bool RemoverSolicitudAceptada(string usuarioEmisor, string usuarioActual)
        {
            return RemoverSiCoincide(usuarioEmisor, usuarioActual);
        }

        private bool RemoverSiCoincide(string usuarioEmisor, string usuarioReceptor)
        {
            int indice = _solicitudes.FindIndex(s =>
                s.UsuarioEmisor == usuarioEmisor &&
                s.UsuarioReceptor == usuarioReceptor);

            if (indice < 0)
            {
                return false;
            }

            _solicitudes.RemoveAt(indice);
            return true;
        }

        private bool ProcesarSolicitudPendiente(
            DTOs.SolicitudAmistadDTO solicitud,
            string usuarioActual)
        {
            if (!string.Equals(
                solicitud.UsuarioReceptor,
                usuarioActual,
                StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            int indice = _solicitudes.FindIndex(s =>
                s.UsuarioEmisor == solicitud.UsuarioEmisor &&
                s.UsuarioReceptor == usuarioActual);

            if (indice >= 0)
            {
                _solicitudes[indice] = solicitud;
            }
            else
            {
                _solicitudes.Add(solicitud);
            }

            return true;
        }
    }
}