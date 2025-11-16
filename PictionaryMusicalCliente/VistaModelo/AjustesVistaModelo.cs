using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.Comandos;
using System;
using System.Windows.Input;

namespace PictionaryMusicalCliente.VistaModelo
{
    public class AjustesVistaModelo : BaseVistaModelo
    {
        private readonly MusicaManejador _musicaManejador;

        public Action OcultarVentana { get; set; }
        public Action MostrarDialogoCerrarSesion { get; set; }

        public AjustesVistaModelo(MusicaManejador servicioMusica)
        {
            _musicaManejador = servicioMusica ?? throw new ArgumentNullException(nameof(servicioMusica));

            ConfirmarComando = new ComandoDelegado(_ => EjecutarConfirmar());
            CerrarSesionComando = new ComandoDelegado(_ => EjecutarCerrarSesion());
        }

        /// <summary>
        /// Propiedad para el Slider de Volumen.
        /// El get/set ahora actualiza el servicio directamente.
        /// </summary>
        public double Volumen
        {
            get => _musicaManejador.Volumen;
            set
            {
                if (_musicaManejador.Volumen != value)
                {
                    _musicaManejador.Volumen = value;
                    NotificarCambio(nameof(Volumen));
                }
            }
        }

        public ICommand ConfirmarComando { get; }
        public ICommand CerrarSesionComando { get; }

        private void EjecutarConfirmar()
        {
            OcultarVentana?.Invoke();
        }

        private void EjecutarCerrarSesion()
        {
            MostrarDialogoCerrarSesion?.Invoke();
        }
    }
}