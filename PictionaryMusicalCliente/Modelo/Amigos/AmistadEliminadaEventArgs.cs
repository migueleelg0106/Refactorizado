using System;

namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class AmistadEliminadaEventArgs : EventArgs
    {
        public AmistadEliminadaEventArgs(AmistadEliminada amistad)
        {
            Amistad = amistad ?? throw new ArgumentNullException(nameof(amistad));
        }

        public AmistadEliminada Amistad { get; }
    }
}
