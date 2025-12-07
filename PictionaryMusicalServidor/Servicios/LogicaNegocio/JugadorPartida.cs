namespace PictionaryMusicalServidor.Servicios.LogicaNegocio
{
    /// <summary>
    /// Representa el estado de un jugador dentro de una partida en memoria.
    /// Contiene informacion sobre su puntaje, rol y estado de conexion.
    /// </summary>
    public class JugadorPartida
    {
        /// <summary>
        /// Nombre visible del usuario en la partida.
        /// </summary>
        public string NombreUsuario { get; set; }

        /// <summary>
        /// Identificador unico de la conexion del cliente.
        /// </summary>
        public string IdConexion { get; set; }

        /// <summary>
        /// Indica si el jugador es el anfitrion de la partida.
        /// </summary>
        public bool EsHost { get; set; }

        /// <summary>
        /// Indica si el jugador tiene el rol de dibujante en la ronda actual.
        /// </summary>
        public bool EsDibujante { get; set; }

        /// <summary>
        /// Indica si el jugador ya acerto la cancion en la ronda actual.
        /// </summary>
        public bool YaAdivino { get; set; }

        /// <summary>
        /// Puntaje acumulado del jugador en la partida.
        /// </summary>
        public int PuntajeTotal { get; set; }

        /// <summary>
        /// Crea una copia superficial de los datos basicos del jugador.
        /// </summary>
        /// <returns>Una nueva instancia de JugadorPartida.</returns>
        public JugadorPartida CopiarDatosBasicos()
        {
            return new JugadorPartida
            {
                NombreUsuario = NombreUsuario,
                IdConexion = IdConexion,
                EsHost = EsHost,
                EsDibujante = EsDibujante,
                YaAdivino = YaAdivino,
                PuntajeTotal = PuntajeTotal
            };
        }

        /// <summary>
        /// Devuelve una cadena que representa al objeto actual.
        /// </summary>
        /// <returns>Cadena con informacion del jugador.</returns>
        public override string ToString()
        {
            return string.Format(
                "Nombre: {0}, Conexion: {1}, Host: {2}, Dibujante: {3}, Puntaje: {4}",
                NombreUsuario,
                IdConexion,
                EsHost,
                EsDibujante,
                PuntajeTotal);
        }
    }
}