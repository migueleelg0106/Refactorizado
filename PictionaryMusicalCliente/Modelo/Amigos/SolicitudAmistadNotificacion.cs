using System;

namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class SolicitudAmistadNotificacion
    {
        public SolicitudAmistadNotificacion(string remitente, string receptor)
        {
            if (string.IsNullOrWhiteSpace(remitente))
            {
                throw new ArgumentException("El remitente es obligatorio.", nameof(remitente));
            }

            if (string.IsNullOrWhiteSpace(receptor))
            {
                throw new ArgumentException("El receptor es obligatorio.", nameof(receptor));
            }

            Remitente = remitente.Trim();
            Receptor = receptor.Trim();
        }

        public string Remitente { get; }

        public string Receptor { get; }
    }
}
