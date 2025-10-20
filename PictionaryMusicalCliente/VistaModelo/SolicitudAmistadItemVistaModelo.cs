using System;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class SolicitudAmistadItemVistaModelo : BaseVistaModelo
    {
        private bool _estaProcesando;

        public SolicitudAmistadItemVistaModelo(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));
            }

            NombreUsuario = nombreUsuario;
        }

        public string NombreUsuario { get; }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            set => EstablecerPropiedad(ref _estaProcesando, value);
        }
    }
}
