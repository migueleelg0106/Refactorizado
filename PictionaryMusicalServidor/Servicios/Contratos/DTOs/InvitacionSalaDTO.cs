using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para una invitacion a sala de juego.
    /// Contiene el codigo de sala y el correo del destinatario de la invitacion.
    /// </summary>
    [DataContract]
    public class InvitacionSalaDTO
    {
        /// <summary>
        /// Codigo identificador de la sala a la que se invita.
        /// </summary>
        [DataMember]
        public string CodigoSala { get; set; }

        /// <summary>
        /// Correo electronico del usuario invitado.
        /// </summary>
        [DataMember]
        public string Correo { get; set; }

        /// <summary>
        /// Codigo de idioma para personalizar el correo de invitacion (opcional).
        /// </summary>
        [DataMember]
        public string Idioma { get; set; }
    }
}
