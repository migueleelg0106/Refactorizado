using System;

namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class AmistadEliminadaNotificacion
    {
        public AmistadEliminadaNotificacion(string jugador, string amigo, bool operacionLocal)
        {
            if (string.IsNullOrWhiteSpace(jugador))
            {
                throw new ArgumentException("El jugador es obligatorio.", nameof(jugador));
            }

            if (string.IsNullOrWhiteSpace(amigo))
            {
                throw new ArgumentException("El amigo es obligatorio.", nameof(amigo));
            }

            Jugador = jugador.Trim();
            Amigo = amigo.Trim();
            OperacionLocal = operacionLocal;
        }

        public string Jugador { get; }

        public string Amigo { get; }

        public bool OperacionLocal { get; }
    }
}
