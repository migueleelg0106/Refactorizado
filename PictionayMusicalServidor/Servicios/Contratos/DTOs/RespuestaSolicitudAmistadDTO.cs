using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class RespuestaSolicitudAmistadDTO
    {
        [DataMember]
        public string UsuarioEmisor { get; set; }

        [DataMember]
        public string UsuarioReceptor { get; set; }

        [DataMember]
        public bool SolicitudAceptada { get; set; }
    }
}
