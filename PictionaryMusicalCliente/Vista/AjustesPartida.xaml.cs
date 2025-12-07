using PictionaryMusicalCliente.Utilidades.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Ajustes;
using System;
using System.Windows;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana para modificar configuraciones especificas durante una partida en curso.
    /// </summary>
    public partial class AjustesPartida : Window
    {
        private readonly AjustesPartidaVistaModelo _vistaModelo;
        private readonly ISonidoManejador _sonidoManejador;
        private readonly ICancionManejador _cancionManejador;

        /// <summary>
        /// Accion a ejecutar si el usuario decide salir de la partida.
        /// </summary>
        public Action SalirDePartidaConfirmado { get; set; }

        /// <summary>
        /// Constructor por defecto, solo para uso del diseñador/XAML. 
        /// La aplicación debe usar el constructor que recibe dependencias.
        /// </summary>
        public AjustesPartida()
        {
        }

        /// <summary>
        /// Inicializa la ventana de ajustes de partida.
        /// </summary>
        public AjustesPartida(
            ISonidoManejador sonidoManejador,
            ICancionManejador cancionManejador)
        {
            InitializeComponent();

            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            _cancionManejador = cancionManejador ??
                throw new ArgumentNullException(nameof(cancionManejador));

            _vistaModelo = new AjustesPartidaVistaModelo(_cancionManejador, _sonidoManejador);

            _vistaModelo.OcultarVentana = () => Close();

            _vistaModelo.MostrarDialogoSalirPartida = () =>
            {
                var dialogo = new ConfirmacionSalirPartida(_sonidoManejador)
                {
                    Owner = this
                };

                if (dialogo.ShowDialog() == true)
                {
                    Close();
                    SalirDePartidaConfirmado?.Invoke();
                }
            };

            DataContext = _vistaModelo;
        }
    }
}