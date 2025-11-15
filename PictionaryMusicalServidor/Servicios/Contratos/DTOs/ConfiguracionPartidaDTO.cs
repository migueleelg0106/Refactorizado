using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{

    [DataContract]
    public class ConfiguracionPartidaDTO
    {
        [DataMember]
        public int NumeroRondas { get; set; }

        [DataMember]
        public int TiempoPorRondaSegundos { get; set; }

        [DataMember]
        public string IdiomaCanciones { get; set; }

        [DataMember]
        public string Dificultad { get; set; }
    }
}
