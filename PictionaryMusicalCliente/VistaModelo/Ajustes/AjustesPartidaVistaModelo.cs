using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Comandos;
using System;
using System.Windows.Input;

namespace PictionaryMusicalCliente.VistaModelo.Ajustes
{
    public class AjustesPartidaVistaModelo : BaseVistaModelo
    {
        private readonly CancionManejador _cancionManejador;

        public Action OcultarVentana { get; set; }
        public Action MostrarDialogoSalirPartida { get; set; }

        public AjustesPartidaVistaModelo(CancionManejador servicioCancion)
        {
            _cancionManejador = servicioCancion ?? throw new ArgumentNullException(nameof(servicioCancion));

            ConfirmarComando = new ComandoDelegado(_ => EjecutarConfirmar());
            SalirPartidaComando = new ComandoDelegado(_ => EjecutarSalirPartida());
        }

        /// <summary>
        /// Propiedad enlazada al Slider. Lee/escribe directamente en el manejador.
        /// </summary>
        public double Volumen
        {
            get => _cancionManejador.Volumen;
            set
            {
                if (Math.Abs(_cancionManejador.Volumen - value) > 0.0001)
                {
                    _cancionManejador.Volumen = value;
                    NotificarCambio(nameof(Volumen));
                }
            }
        }

        public ICommand ConfirmarComando { get; }
        public ICommand SalirPartidaComando { get; }

        private void EjecutarConfirmar()
        {
            OcultarVentana?.Invoke();
        }

        private void EjecutarSalirPartida()
        {
            MostrarDialogoSalirPartida?.Invoke();
        }
    }
}