using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta la informacion de una invitacion a una sala.
    /// </summary>
    [DataContract]
    public class InvitacionSalaDTO
    {
        /// <summary>
        /// Codigo de la sala a la que se invita.
        /// </summary>
        [DataMember]
        public string CodigoSala { get; set; }

        /// <summary>
        /// Correo electronico del usuario invitado.
        /// </summary>
        [DataMember]
        public string Correo { get; set; }
    }
}
