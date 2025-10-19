using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ListaAmigosDTO
    {
        public ListaAmigosDTO()
        {
            Amigos = new List<string>();
        }

        [DataMember]
        public bool OperacionExitosa { get; set; }

        [DataMember]
        public string Mensaje { get; set; }

        [DataMember]
        public string Jugador { get; set; }

        [DataMember]
        public List<string> Amigos { get; set; }
    }
}
