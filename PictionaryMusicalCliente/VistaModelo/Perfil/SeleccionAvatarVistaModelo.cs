using log4net;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PictionaryMusicalCliente.VistaModelo.Perfil
{
    /// <summary>
    /// Vista Modelo para el dialogo de seleccion de avatares predefinidos.
    /// </summary>
    public class SeleccionAvatarVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IAvisoServicio _avisoServicio;
        private readonly ISonidoManejador _sonidoManejador;
        private ObjetoAvatar _avatarSeleccionado;

        public SeleccionAvatarVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            IEnumerable<ObjetoAvatar> avatares,
            IAvisoServicio avisoServicio,
            ISonidoManejador sonidoManejador)
            : base(ventana, localizador)
        {
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            if (avatares == null)
            {
                throw new ArgumentNullException(nameof(avatares));
            }

            Avatares = new ObservableCollection<ObjetoAvatar>(avatares);
            ConfirmarSeleccionComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                ConfirmarSeleccion();
            });
        }

        public ObservableCollection<ObjetoAvatar> Avatares { get; }

        public ObjetoAvatar AvatarSeleccionado
        {
            get => _avatarSeleccionado;
            set => EstablecerPropiedad(ref _avatarSeleccionado, value);
        }

        public ICommand ConfirmarSeleccionComando { get; }

        public Action<ObjetoAvatar> SeleccionConfirmada { get; set; }

        private void ConfirmarSeleccion()
        {
            if (AvatarSeleccionado == null)
            {
				_logger.Warn("Intento de confirmar seleccion sin avatar elegido.");
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(Lang.errorTextoSeleccionAvatarValido);
                return;
            }

            _logger.InfoFormat("Avatar seleccionado: ID {0}",
                AvatarSeleccionado.Id);
            SeleccionConfirmada?.Invoke(AvatarSeleccionado);
        }
    }
}