using System;

namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class RespuestaSolicitudAmistadNotificacion
    {
        public RespuestaSolicitudAmistadNotificacion(string remitente, string receptor, bool aceptada)
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
            Aceptada = aceptada;
        }

        public string Remitente { get; }

        public string Receptor { get; }

        public bool Aceptada { get; }
    }
}
