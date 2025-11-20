using System.Windows.Input;

namespace PictionaryMusicalCliente.Modelo
{
    /// <summary>
    /// Modelo de presentacion para un jugador dentro de una lista en la interfaz grafica.
    /// </summary>
    public class JugadorElemento
    {
        /// <summary>
        /// Nombre visible del jugador (Gamertag).
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Determina si el boton de expulsar debe ser visible para este elemento.
        /// Generalmente verdadero solo si el usuario local es el anfitrion.
        /// </summary>
        public bool MostrarBotonExpulsar { get; set; }

        /// <summary>
        /// Comando a ejecutar cuando se presiona la accion de expulsar.
        /// </summary>
        public ICommand ExpulsarComando { get; set; }
    }
}