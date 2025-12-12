using System;

namespace PictionaryMusicalCliente.Utilidades.Abstracciones
{
    /// <summary>
    /// Contrato para gestionar la apertura, cierre y mensajeria de ventanas
    /// de forma desacoplada.
    /// </summary>
    public interface IVentanaServicio
    {
        /// <summary>
        /// Muestra una ventana no modal asociada a un ViewModel.
        /// </summary>
        void MostrarVentana(object vistaModelo);

        /// <summary>
        /// Muestra una ventana en modo dialogo.
        /// </summary>
        bool? MostrarVentanaDialogo(object vistaModelo);

        /// <summary>
        /// Cierra la ventana activa asociada al ViewModel proporcionado.
        /// </summary>
        void CerrarVentana(object vistaModelo);

        /// <summary>
        /// Muestra un mensaje emergente al usuario.
        /// </summary>
        void MostrarMensaje(string titulo, string mensaje);

        /// <summary>
        /// Muestra un mensaje de error comun.
        /// </summary>
        void MostrarError(string mensaje);
    }
}