using System;

namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class SolicitudAmistadEventArgs : EventArgs
    {
        public SolicitudAmistadEventArgs(SolicitudAmistad solicitud)
        {
            Solicitud = solicitud ?? throw new ArgumentNullException(nameof(solicitud));
        }

        public SolicitudAmistad Solicitud { get; }
    }
}
