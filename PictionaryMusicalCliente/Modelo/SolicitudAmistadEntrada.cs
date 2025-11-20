using System;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Modelo
{
    /// <summary>
    /// Envuelve un DTO de solicitud de amistad con logica adicional para la interfaz de usuario.
    /// </summary>
    public class SolicitudAmistadEntrada(
        DTOs.SolicitudAmistadDTO solicitud,
        string nombreUsuario,
        bool puedeAceptar)
    {
        /// <summary>
        /// El objeto de transferencia de datos original recibido del servidor.
        /// </summary>
        public DTOs.SolicitudAmistadDTO Solicitud { get; } = solicitud ??
            throw new ArgumentNullException(nameof(solicitud));

        /// <summary>
        /// Nombre del usuario relacionado con la solicitud (Emisor o Receptor segun el contexto).
        /// </summary>
        public string NombreUsuario { get; } = nombreUsuario;

        /// <summary>
        /// Indica si el usuario actual tiene permiso para aceptar esta solicitud.
        /// </summary>
        public bool PuedeAceptar { get; } = puedeAceptar;
    }
}