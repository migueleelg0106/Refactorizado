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
                new PropertyMetadata(false, OnSoloNumerosChanged));

        /// <summary>
        /// Obtiene el valor de la propiedad SoloNumeros del objeto especificado.
        /// </summary>
        public static bool GetSoloNumeros(DependencyObject objeto)
        {
            return objeto is not null && (bool)objeto.GetValue(SoloNumerosProperty);
        }

        /// <summary>
        /// Establece el valor de la propiedad SoloNumeros en el objeto especificado.
        /// </summary>
        public static void SetSoloNumeros(DependencyObject objeto, bool valor)
        {
            objeto?.SetValue(SoloNumerosProperty, valor);
        }

        private static void OnSoloNumerosChanged(
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
                DataObject.AddPastingHandler(cuadroTexto, ManejarPegado);
            }
            else
            {
                cuadroTexto.PreviewTextInput -= ValidarEntrada;
                DataObject.RemovePastingHandler(cuadroTexto, ManejarPegado);
            }
        }

        private static void ValidarEntrada(object remitente, 
            TextCompositionEventArgs argumentosEvento)
        {
            argumentosEvento.Handled = !EsTextoNumerico(argumentosEvento.Text);
        }

        private static void ManejarPegado(object remitente, 
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