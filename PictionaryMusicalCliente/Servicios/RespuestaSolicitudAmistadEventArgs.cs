using System;

namespace PictionaryMusicalCliente.Servicios
{
    public class RespuestaSolicitudAmistadEventArgs : EventArgs
    {
        public RespuestaSolicitudAmistadEventArgs(string remitente, string receptor, bool aceptada)
        {
            Remitente = remitente;
            Receptor = receptor;
            Aceptada = aceptada;
        }

        public string Remitente { get; }

        public string Receptor { get; }

        public bool Aceptada { get; }
    }
}
