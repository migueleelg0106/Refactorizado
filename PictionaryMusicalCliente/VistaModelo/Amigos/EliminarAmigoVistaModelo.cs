using System;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    public class EliminarAmigoVistaModelo : BaseVistaModelo
    {
        public EliminarAmigoVistaModelo(string nombreAmigo)
        {
            MensajeConfirmacion = string.IsNullOrWhiteSpace(nombreAmigo)
                ? Lang.eliminarAmigoTextoConfirmacion
                : string.Concat(Lang.eliminarAmigoTextoConfirmacion, nombreAmigo, "?");

            AceptarCommand = new ComandoDelegado(_ => Cerrar?.Invoke(true));
            CancelarCommand = new ComandoDelegado(_ => Cerrar?.Invoke(false));
        }

        public string MensajeConfirmacion { get; }

        public ICommand AceptarCommand { get; }

        public ICommand CancelarCommand { get; }

        public Action<bool?> Cerrar { get; set; }
    }
}
