namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class SolicitudAmistadNotificacion
    {
        public string Remitente { get; set; }

        public string Receptor { get; set; }
    }

    public class RespuestaSolicitudAmistadNotificacion
    {
        public string Remitente { get; set; }

        public string Receptor { get; set; }

        public bool Aceptada { get; set; }
    }

    public class AmistadEliminadaNotificacion
    {
        public string Jugador { get; set; }

        public string Amigo { get; set; }
    }
}