using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    [DataContract]
    public class ResultadoSolicitudRecuperacionDTO
    {
        [DataMember]
        public bool CuentaEncontrada { get; set; }

        [DataMember]
        public bool CodigoEnviado { get; set; }

        [DataMember]
        public string CorreoDestino { get; set; }

        [DataMember]
        public string Mensaje { get; set; }

        [DataMember]
        public string TokenCodigo { get; set; }
    }
}