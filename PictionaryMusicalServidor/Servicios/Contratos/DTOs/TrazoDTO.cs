using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Representa un trazo dibujado en el lienzo compartido durante la partida.
    /// </summary>
    [DataContract]
    public class TrazoDTO
    {
        /// <summary>
        /// Coordenadas X de los puntos que conforman el trazo.
        /// </summary>
        [DataMember]
        public double[] PuntosX { get; set; }

        /// <summary>
        /// Coordenadas Y de los puntos que conforman el trazo.
        /// </summary>
        [DataMember]
        public double[] PuntosY { get; set; }

        /// <summary>
        /// Color del trazo representado en formato hexadecimal.
        /// </summary>
        [DataMember]
        public string ColorHex { get; set; }

        /// <summary>
        /// Grosor del pincel utilizado para el trazo.
        /// </summary>
        [DataMember]
        public double Grosor { get; set; }

        /// <summary>
        /// Indica si el trazo corresponde a una accion de borrado.
        /// </summary>
        [DataMember]
        public bool EsBorrado { get; set; }

        /// <summary>
        /// Indica si se debe limpiar todo el lienzo.
        /// </summary>
        [DataMember]
        public bool EsLimpiarTodo { get; set; }
    }
}
