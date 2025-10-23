using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class SeleccionarAvatarVistaModelo : BaseVistaModelo
    {
        private ObjetoAvatar _avatarSeleccionado;

        public SeleccionarAvatarVistaModelo(IEnumerable<ObjetoAvatar> avatares)
        {
            if (avatares == null)
            {
                throw new ArgumentNullException(nameof(avatares));
            }

            Avatares = new ObservableCollection<ObjetoAvatar>(avatares);
            ConfirmarSeleccionCommand = new ComandoDelegado(ConfirmarSeleccion);
        }

        public ObservableCollection<ObjetoAvatar> Avatares { get; }

        public ObjetoAvatar AvatarSeleccionado
        {
            get => _avatarSeleccionado;
            set => EstablecerPropiedad(ref _avatarSeleccionado, value);
        }

        public ICommand ConfirmarSeleccionCommand { get; }

        public Action<ObjetoAvatar> SeleccionConfirmada { get; set; }

        public Action CerrarAccion { get; set; }

        private void ConfirmarSeleccion()
        {
            if (AvatarSeleccionado == null)
            {
                AvisoAyudante.Mostrar(Lang.errorTextoSeleccionAvatarValido);
                return;
            }

            SeleccionConfirmada?.Invoke(AvatarSeleccionado);
            CerrarAccion?.Invoke();
        }
    }
}
