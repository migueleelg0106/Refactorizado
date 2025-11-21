using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using log4net;

namespace PictionaryMusicalCliente.VistaModelo.Perfil
{
    /// <summary>
    /// Vista Modelo para el dialogo de seleccion de avatares predefinidos.
    /// </summary>
    public class SeleccionAvatarVistaModelo : BaseVistaModelo
    {
        private static readonly ILog Log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ObjetoAvatar _avatarSeleccionado;

        /// <summary>
        /// Inicializa el ViewModel con la lista de avatares disponibles.
        /// </summary>
        /// <param name="avatares">Lista de objetos avatar cargados.</param>
        public SeleccionAvatarVistaModelo(IEnumerable<ObjetoAvatar> avatares)
        {
            if (avatares == null)
            {
                throw new ArgumentNullException(nameof(avatares));
            }

            Avatares = new ObservableCollection<ObjetoAvatar>(avatares);
            ConfirmarSeleccionComando = new ComandoDelegado(_ =>
            {
                SonidoManejador.ReproducirClick();
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
                Log.Warn("Intento de confirmar selección sin avatar elegido.");
                SonidoManejador.ReproducirError();
                AvisoAyudante.Mostrar(Lang.errorTextoSeleccionAvatarValido);
                return;
            }

            Log.InfoFormat("Avatar seleccionado: ID {0}",
                AvatarSeleccionado.Id);
            SeleccionConfirmada?.Invoke(AvatarSeleccionado);
            CerrarAccion?.Invoke();
        }
    }
}