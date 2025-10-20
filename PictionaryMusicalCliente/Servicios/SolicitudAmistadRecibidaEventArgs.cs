using System;

namespace PictionaryMusicalCliente.Servicios
{
    public class SolicitudAmistadRecibidaEventArgs : EventArgs
    {
        public SolicitudAmistadRecibidaEventArgs(string remitente, string receptor)
        {
            Remitente = remitente;
            Receptor = receptor;
        }

        public string Remitente { get; }

        public string Receptor { get; }
    }
}
