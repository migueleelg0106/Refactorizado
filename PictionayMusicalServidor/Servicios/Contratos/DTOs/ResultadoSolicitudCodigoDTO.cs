using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ResultadoSolicitudCodigoDTO
    {
        [DataMember]
        public bool CodigoEnviado { get; set; }

        [DataMember]
        public bool UsuarioRegistrado { get; set; }

        [DataMember]
        public bool CorreoRegistrado { get; set; }

        [DataMember]
        public string Mensaje { get; set; }

        [DataMember]
        public string TokenCodigo { get; set; }
    }
}