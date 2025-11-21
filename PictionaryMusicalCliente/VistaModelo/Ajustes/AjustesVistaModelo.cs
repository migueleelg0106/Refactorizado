using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.Comandos;
using System;
using System.Windows.Input;

namespace PictionaryMusicalCliente.VistaModelo.Ajustes
{
    /// <summary>
    /// ViewModel para la ventana de configuracion global de la aplicacion.
    /// </summary>
    public class AjustesVistaModelo : BaseVistaModelo
    {
        private readonly MusicaManejador _musicaManejador;

        /// <summary>
        /// Accion para cerrar la ventana de ajustes.
        /// </summary>
        public Action OcultarVentana { get; set; }

        /// <summary>
        /// Accion para abrir el dialogo de cierre de sesion.
        /// </summary>
        public Action MostrarDialogoCerrarSesion { get; set; }

        /// <summary>
        /// Inicializa el ViewModel con el manejador de musica global.
        /// </summary>
        /// <param name="servicioMusica">Servicio de control de audio.</param>
        public AjustesVistaModelo(MusicaManejador servicioMusica)
        {
            _musicaManejador = servicioMusica ??
                throw new ArgumentNullException(nameof(servicioMusica));

            ConfirmarComando = new ComandoDelegado(_ => EjecutarConfirmar());
            CerrarSesionComando = new ComandoDelegado(_ => EjecutarCerrarSesion());
        }

        /// <summary>
        /// Obtiene o establece el volumen global de la musica.
        /// </summary>
        public double Volumen
        {
            get => _musicaManejador.Volumen;
            set
            {
                if (Math.Abs(_musicaManejador.Volumen - value) > 0.0001)
                {
                    _musicaManejador.Volumen = value;
                    NotificarCambio(nameof(Volumen));
                }
            }
        }

        /// <summary>
        /// Indica si los efectos de sonido están silenciados en la aplicación.
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
        /// Comando para guardar los cambios y cerrar.
        /// </summary>
        public ICommand ConfirmarComando { get; }

        /// <summary>
        /// Comando para solicitar el cierre de sesion.
        /// </summary>
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