using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.ClienteServicios;
using System;
using System.Windows.Input;

namespace PictionaryMusicalCliente.VistaModelo.Ajustes
{
    /// <summary>
    /// Gestiona la logica de interaccion para la ventana de ajustes durante una partida.
    /// </summary>
    public class AjustesPartidaVistaModelo : BaseVistaModelo
    {
        private readonly CancionManejador _cancionManejador;

        /// <summary>
        /// Accion para cerrar la ventana actual.
        /// </summary>
        public Action OcultarVentana { get; set; }

        /// <summary>
        /// Accion para mostrar el dialogo de confirmacion de salida.
        /// </summary>
        public Action MostrarDialogoSalirPartida { get; set; }

        /// <summary>
        /// Inicializa el ViewModel con el servicio de control de audio del juego.
        /// </summary>
        /// <param name="servicioCancion">Servicio que gestiona la reproduccion de pistas.</param>
        public AjustesPartidaVistaModelo(CancionManejador servicioCancion)
        {
            _cancionManejador = servicioCancion ??
                throw new ArgumentNullException(nameof(servicioCancion));

            ConfirmarComando = new ComandoDelegado(_ => EjecutarConfirmar());
            SalirPartidaComando = new ComandoDelegado(_ => EjecutarSalirPartida());
        }

        /// <summary>
        /// Obtiene o establece el volumen de la musica de fondo.
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

        /// <summary>
        /// Indica si los efectos de sonido deben silenciarse durante la partida.
        /// </summary>
        public bool SonidosSilenciados
        {
            get => SonidoManejador.Silenciado;
            set
            {
                if (SonidoManejador.Silenciado != value)
                {
                    SonidoManejador.Silenciado = value;
                    NotificarCambio(nameof(SonidosSilenciados));
                }
            }
        }

        /// <summary>
        /// Comando para aplicar los cambios y cerrar la ventana.
        /// </summary>
        public ICommand ConfirmarComando { get; }

        /// <summary>
        /// Comando para iniciar el flujo de abandono de la partida.
        /// </summary>
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