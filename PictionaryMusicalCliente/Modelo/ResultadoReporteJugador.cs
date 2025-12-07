namespace PictionaryMusicalCliente.Modelo
{
    /// <summary>
    /// Resultado capturado al solicitar los datos para reportar a un jugador.
    /// </summary>
    public class ResultadoReporteJugador
    {
        /// <summary>
        /// Indica si el usuario confirmó el envío del reporte.
        /// </summary>
        public bool Confirmado { get; set; }

        /// <summary>
        /// Motivo detallado proporcionado por el usuario.
        /// </summary>
        public string Motivo { get; set; }
    }
}