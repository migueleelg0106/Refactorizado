using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    [DataContract]
    public class ClasificacionUsuarioDTO
    {
        [DataMember]
        public string Usuario { get; set; }

        [DataMember]
        public int Puntos { get; set; }

        [DataMember]
        public int RondasGanadas { get; set; }
    }
}