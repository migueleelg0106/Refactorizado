using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{

    [DataContract]
    public class SalaDTO
    {
        [DataMember]
        public string Codigo { get; set; }

        [DataMember]
        public string Creador { get; set; }

        [DataMember]
        public ConfiguracionPartidaDTO Configuracion { get; set; }

        [DataMember]
        public IList<string> Jugadores { get; set; }
    }
}
