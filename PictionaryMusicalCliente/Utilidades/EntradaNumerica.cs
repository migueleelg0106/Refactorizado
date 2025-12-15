using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PictionaryMusicalCliente.Utilidades
{
    /// <summary>
    /// Comportamiento adjunto para restringir la entrada de TextBoxes a solo numeros.
    /// </summary>
    public static class EntradaNumerica
    {
        /// <summary>
        /// Identifica la propiedad adjunta SoloNumeros.
        /// </summary>
        public static readonly DependencyProperty SoloNumerosProperty =
            DependencyProperty.RegisterAttached(
                "SoloNumeros",
                typeof(bool),
                typeof(EntradaNumerica),
                new PropertyMetadata(false, AlCambiarSoloNumeros));

        /// <summary>
        /// Obtiene el valor de la propiedad SoloNumeros del objeto especificado.
        /// </summary>
        /// <param name="objeto">El objeto de dependencia del cual se obtiene el valor.</param>
        /// <returns>True si solo se permiten numeros, False en caso contrario.</returns>
        public static bool GetSoloNumeros(DependencyObject objeto)
        {
            return objeto is not null && (bool)objeto.GetValue(SoloNumerosProperty);
        }

        /// <summary>
        /// Establece el valor de la propiedad SoloNumeros en el objeto especificado.
        /// </summary>
        /// <param name="objeto">El objeto de dependencia al cual se le asigna el valor.</param>
        /// <param name="valor">True para restringir a solo numeros.</param>
        public static void SetSoloNumeros(DependencyObject objeto, bool valor)
        {
            objeto?.SetValue(SoloNumerosProperty, valor);
        }

        private static void AlCambiarSoloNumeros(
            DependencyObject dependencia,
            DependencyPropertyChangedEventArgs argumentosEvento)
        {
            if (dependencia is not TextBox cuadroTexto)
            {
                return;
            }

            if (argumentosEvento.NewValue is bool habilitar && habilitar)
            {
                cuadroTexto.PreviewTextInput += ValidarEntrada;
                DataObject.AddPastingHandler(cuadroTexto, ManejarPegadoTexto);
            }
            else
            {
                cuadroTexto.PreviewTextInput -= ValidarEntrada;
                DataObject.RemovePastingHandler(cuadroTexto, ManejarPegadoTexto);
            }
        }

        private static void ValidarEntrada(object remitente, 
            TextCompositionEventArgs argumentosEvento)
        {
            argumentosEvento.Handled = !EsTextoNumerico(argumentosEvento.Text);
        }

        private static void ManejarPegadoTexto(object remitente, 
            DataObjectPastingEventArgs argumentosEvento)
        {
            if (!argumentosEvento.DataObject.GetDataPresent(DataFormats.Text))
            {
                argumentosEvento.CancelCommand();
                return;
            }

            string textoPegado = argumentosEvento.DataObject.GetData(DataFormats.Text) 
                as string ?? string.Empty;

            if (!EsTextoNumerico(textoPegado))
            {
                argumentosEvento.CancelCommand();
            }
        }

        private static bool EsTextoNumerico(string texto)
        {
            return texto.All(char.IsDigit);
        }
    }
}