using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PictionaryMusicalCliente.Utilidades
{
    /// <summary>
    /// Provee metodos auxiliares para manipular la apariencia de controles visuales.
    /// </summary>
    public static class ControlVisual
    {
        /// <summary>
        /// Elimina los estilos de error (borde rojo) aplicados a un control.
        /// </summary>
        public static void RestablecerEstadoCampo(Control control)
        {
            if (control == null)
            {
                return;
            }

            control.ClearValue(Control.BorderBrushProperty);
            control.ClearValue(Control.BorderThicknessProperty);
        }

        /// <summary>
        /// Aplica un estilo visual para indicar que el campo tiene un error de validacion.
        /// </summary>
        public static void MarcarCampoInvalido(Control control)
        {
            if (control == null)
            {
                return;
            }

            control.BorderBrush = Brushes.Red;
            control.BorderThickness = new Thickness(2);
        }
    }
}