using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    [DataContract]
    public class ResultadoRegistroCuentaDTO
    {
        [DataMember]
        public bool RegistroExitoso { get; set; }

        [DataMember]
        public bool UsuarioRegistrado { get; set; }

        [DataMember]
        public bool CorreoRegistrado { get; set; }

        [DataMember]
        public string Mensaje { get; set; }
    }
}