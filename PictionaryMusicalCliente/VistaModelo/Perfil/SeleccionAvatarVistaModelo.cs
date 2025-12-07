using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using log4net;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades.Abstracciones;

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

        /// <summary>
        /// Inicializa el ViewModel con la lista de avatares disponibles.
        /// </summary>
        /// <param name="avatares">Lista de objetos avatar cargados.</param>
        public SeleccionAvatarVistaModelo(IEnumerable<ObjetoAvatar> avatares,
            IAvisoServicio avisoServicio,
            ISonidoManejador sonidoManejador)
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

        /// <summary>
        /// Coleccion de avatares para mostrar en la grilla de seleccion.
        /// </summary>
        public ObservableCollection<ObjetoAvatar> Avatares { get; }

        /// <summary>
        /// El avatar actualmente seleccionado por el usuario.
        /// </summary>
        public ObjetoAvatar AvatarSeleccionado
        {
            get => _avatarSeleccionado;
            set => EstablecerPropiedad(ref _avatarSeleccionado, value);
        }

        /// <summary>
        /// Comando para confirmar la eleccion y cerrar el dialogo.
        /// </summary>
        public ICommand ConfirmarSeleccionComando { get; }

        /// <summary>
        /// Accion que se ejecuta al confirmar, pasando el avatar seleccionado.
        /// </summary>
        public Action<ObjetoAvatar> SeleccionConfirmada { get; set; }

        /// <summary>
        /// Accion para cerrar la ventana.
        /// </summary>
        public Action CerrarAccion { get; set; }

        private void ConfirmarSeleccion()
        {
            if (AvatarSeleccionado == null)
            {
				_logger.Warn("Intento de confirmar selección sin avatar elegido.");
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(Lang.errorTextoSeleccionAvatarValido);
                return;
            }

            _logger.InfoFormat("Avatar seleccionado: ID {0}",
                AvatarSeleccionado.Id);
            SeleccionConfirmada?.Invoke(AvatarSeleccionado);
            CerrarAccion?.Invoke();
        }
    }
}