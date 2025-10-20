using System;

namespace PictionaryMusicalCliente.Servicios
{
    public class AmistadEliminadaEventArgs : EventArgs
    {
        public AmistadEliminadaEventArgs(string jugador, string amigo)
        {
            Jugador = jugador;
            Amigo = amigo;
        }

        public string Jugador { get; }

        public string Amigo { get; }
    }
}
