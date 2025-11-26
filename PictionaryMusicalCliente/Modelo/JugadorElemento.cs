using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PictionaryMusicalCliente.Modelo
{
    /// <summary>
    /// Modelo de presentacion para un jugador dentro de una lista en la interfaz grafica.
    /// </summary>
    public class JugadorElemento : INotifyPropertyChanged
    {
        private string _nombre;
        private bool _mostrarBotonExpulsar;
        private ICommand _expulsarComando;
        private int _puntos;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Nombre visible del jugador (Gamertag).
        /// </summary>
        public string Nombre
        {
            get => _nombre;
            set => EstablecerPropiedad(ref _nombre, value);
        }

        /// <summary>
        /// Determina si el boton de expulsar debe ser visible para este elemento.
        /// Generalmente verdadero solo si el usuario local es el anfitrion.
        /// </summary>
        public bool MostrarBotonExpulsar
        {
            get => _mostrarBotonExpulsar;
            set => EstablecerPropiedad(ref _mostrarBotonExpulsar, value);
        }

        /// <summary>
        /// Comando a ejecutar cuando se presiona la accion de expulsar.
        /// </summary>
        public ICommand ExpulsarComando
        {
            get => _expulsarComando;
            set => EstablecerPropiedad(ref _expulsarComando, value);
        }

        /// <summary>
        /// Puntaje acumulado del jugador durante la partida.
        /// </summary>
        public int Puntos
        {
            get => _puntos;
            set => EstablecerPropiedad(ref _puntos, value);
        }

        private bool EstablecerPropiedad<T>(ref T campo, T valor, [CallerMemberName] string nombrePropiedad = null)
        {
            if (EqualityComparer<T>.Default.Equals(campo, valor))
            {
                return false;
            }

            campo = valor;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
            return true;
        }
    }
}
