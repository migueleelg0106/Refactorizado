using System;

namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class RespuestaSolicitudAmistadEventArgs : EventArgs
    {
        public RespuestaSolicitudAmistadEventArgs(RespuestaSolicitudAmistad respuesta)
        {
            Respuesta = respuesta ?? throw new ArgumentNullException(nameof(respuesta));
        }

        public RespuestaSolicitudAmistad Respuesta { get; }
    }
}
