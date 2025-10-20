using System;
using System.Collections.Generic;

namespace PictionaryMusicalCliente.Servicios
{
    public class ListaAmigosActualizadaEventArgs : EventArgs
    {
        public ListaAmigosActualizadaEventArgs(IReadOnlyList<string> amigos, string mensajeError = null)
        {
            Amigos = amigos ?? Array.Empty<string>();
            MensajeError = string.IsNullOrWhiteSpace(mensajeError) ? null : mensajeError;
        }

        public IReadOnlyList<string> Amigos { get; }

        public string MensajeError { get; }

        public bool TieneError => !string.IsNullOrWhiteSpace(MensajeError);
    }
}
