using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using log4net;
using System;
using System.Windows.Input;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    /// <summary>
    /// ViewModel para el cuadro de dialogo de confirmacion de eliminacion de amigo.
    /// </summary>
    public class EliminacionAmigoVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ISonidoManejador _sonidoManejador;
        private readonly string _nombreAmigo;

        public EliminacionAmigoVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            ISonidoManejador sonidoManejador,
            string nombreAmigo)
            : base(ventana, localizador)
        {
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            _nombreAmigo = nombreAmigo;
            
            MensajeConfirmacion = string.IsNullOrWhiteSpace(nombreAmigo)
                ? Lang.eliminarAmigoTextoConfirmacion
                : string.Concat(Lang.eliminarAmigoTextoConfirmacion, nombreAmigo, "?");
            
            AceptarComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                _logger.InfoFormat("Usuario confirmo la eliminacion del amigo: {0}",
                    _nombreAmigo);
                DialogResult = true;
                _ventana.CerrarVentana(this);
            });
            
            CancelarComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                _logger.InfoFormat("Usuario cancelo la eliminacion del amigo: {0}",
                    _nombreAmigo);
                DialogResult = false;
                _ventana.CerrarVentana(this);
            });
        }

        public string MensajeConfirmacion { get; }

        public ICommand AceptarComando { get; }

        public ICommand CancelarComando { get; }

        public bool? DialogResult { get; private set; }
    }
}