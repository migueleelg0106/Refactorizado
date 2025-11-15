using System;
using System.Globalization;
using System.Windows.Data;

namespace PictionaryMusicalCliente.Convertidores
{
    /// <summary>
    /// Invierte valores booleanos permitiendo utilizarlos en bindings sin crear
    /// propiedades adicionales en la vista modelo.
    /// </summary>
    public class ConvertidorBooleanoInverso : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValor)
            {
                return !boolValor;
            }

            return value;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
