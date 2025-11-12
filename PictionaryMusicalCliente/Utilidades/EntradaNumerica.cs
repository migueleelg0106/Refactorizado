using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PictionaryMusicalCliente.Utilidades
{
    public static class EntradaNumerica
    {
        public static readonly DependencyProperty SoloNumerosProperty = DependencyProperty.RegisterAttached(
            "SoloNumeros",
            typeof(bool),
            typeof(EntradaNumerica),
            new PropertyMetadata(false, OnSoloNumerosChanged));

        public static bool GetSoloNumeros(DependencyObject objeto)
        {
            return objeto is not null && (bool)objeto.GetValue(SoloNumerosProperty);
        }

        public static void SetSoloNumeros(DependencyObject objeto, bool valor)
        {
            objeto?.SetValue(SoloNumerosProperty, valor);
        }

        private static void OnSoloNumerosChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox cuadroTexto)
            {
                return;
            }

            if (e.NewValue is bool habilitar && habilitar)
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

        private static void ValidarEntrada(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !EsTextoNumerico(e.Text);
        }

        private static void ManejarPegado(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(DataFormats.Text))
            {
                e.CancelCommand();
                return;
            }

            string textoPegado = e.DataObject.GetData(DataFormats.Text) as string ?? string.Empty;

            if (!EsTextoNumerico(textoPegado))
            {
                e.CancelCommand();
            }
        }

        private static bool EsTextoNumerico(string texto)
        {
            return texto.All(char.IsDigit);
        }
    }
}

