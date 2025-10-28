using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
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