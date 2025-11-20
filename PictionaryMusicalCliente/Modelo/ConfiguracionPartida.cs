namespace PictionaryMusicalCliente.Modelo
{
    /// <summary>
    /// Almacena los parametros configurables para la creacion de una nueva partida.
    /// </summary>
    public class ConfiguracionPartida
    {
        /// <summary>
        /// Cantidad total de rondas que durara la partida.
        /// </summary>
        public int NumeroRondas { get; set; }

        /// <summary>
        /// Limite de tiempo en segundos para adivinar o dibujar en cada turno.
        /// </summary>
        public int TiempoPorRondaSegundos { get; set; }

        /// <summary>
        /// Codigo del idioma o genero de las canciones a utilizar.
        /// </summary>
        public string IdiomaCanciones { get; set; }

        /// <summary>
        /// Nivel de dificultad seleccionado (ej. Facil, Normal, Dificil).
        /// </summary>
        public string Dificultad { get; set; }
    }
}