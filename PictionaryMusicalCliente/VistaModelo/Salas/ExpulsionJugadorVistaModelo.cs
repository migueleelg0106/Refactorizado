using System;
using System.Windows.Input;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente.VistaModelo.Salas
{
    public class ExpulsionJugadorVistaModelo : BaseVistaModelo
    {
        public ExpulsionJugadorVistaModelo(string mensajeConfirmacion)
        {
            MensajeConfirmacion = string.IsNullOrWhiteSpace(mensajeConfirmacion)
                ? Lang.expulsarTextoConfirmacion
                : mensajeConfirmacion;

            ConfirmarComando = new ComandoDelegado(_ =>
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

        public ICommand ConfirmarComando { get; }

        public ICommand CancelarComando { get; }

        public Action<bool?> Cerrar { get; set; }
    }
}
