using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PictionaryMusicalCliente.VistaModelo.Perfil
{
    public class SeleccionAvatarVistaModelo : BaseVistaModelo
    {
        private ObjetoAvatar _avatarSeleccionado;

        public SeleccionAvatarVistaModelo(IEnumerable<ObjetoAvatar> avatares)
        {
            if (avatares == null)
            {
                throw new ArgumentNullException(nameof(avatares));
            }

            Avatares = new ObservableCollection<ObjetoAvatar>(avatares);
            ConfirmarSeleccionComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
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

        public Action CerrarAccion { get; set; }

        private void ConfirmarSeleccion()
        {
            if (AvatarSeleccionado == null)
            {
                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(Lang.errorTextoSeleccionAvatarValido);
                return;
            }

            SeleccionConfirmada?.Invoke(AvatarSeleccionado);
            CerrarAccion?.Invoke();
        }
    }
}
