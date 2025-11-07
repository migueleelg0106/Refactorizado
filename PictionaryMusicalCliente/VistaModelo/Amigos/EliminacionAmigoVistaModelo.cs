using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using System;
using System.Windows.Input;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    public class EliminacionAmigoVistaModelo : BaseVistaModelo
    {
        public EliminacionAmigoVistaModelo(string nombreAmigo)
        {
            MensajeConfirmacion = string.IsNullOrWhiteSpace(nombreAmigo)
                ? Lang.eliminarAmigoTextoConfirmacion
                : string.Concat(Lang.eliminarAmigoTextoConfirmacion, nombreAmigo, "?");

            AceptarComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                Cerrar?.Invoke(true);
            });

            CancelarComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                Cerrar?.Invoke(false);
            });
        }

        public string MensajeConfirmacion { get; }

        public ICommand AceptarComando { get; }

        public ICommand CancelarComando { get; }

        public Action<bool?> Cerrar { get; set; }
    }
}
