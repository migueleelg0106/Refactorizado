using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PictionaryMusicalCliente.Utilidades
{
    public static class ControlVisualHelper
    {
        public static void RestablecerEstadoCampo(Control control)
        {
            if (control == null)
            {
                return;
            }

            control.ClearValue(Control.BorderBrushProperty);
            control.ClearValue(Control.BorderThicknessProperty);
        }

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
