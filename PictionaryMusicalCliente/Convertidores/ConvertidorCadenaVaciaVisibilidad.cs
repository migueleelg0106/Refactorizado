using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PictionaryMusicalCliente.Convertidores
{
    /// <summary>
    /// Convierte una cadena a <see cref="Visibility"/> mostrando el elemento únicamente
    /// cuando existe contenido en la cadena.
    /// </summary>
    public class ConvertidorCadenaVaciaVisibilidad : IValueConverter
    {
        /// <summary>
        /// Indica si se debe invertir el resultado de la conversión.
        /// </summary>
        public bool Invertir { get; set; }

        /// <summary>
        /// Indica si el elemento debe colapsarse cuando el resultado sea falso.
        /// </summary>
        public bool ColapsarCuandoEsFalso { get; set; } = true;

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool tieneContenido = !string.IsNullOrWhiteSpace(value as string);

            if (Invertir)
            {
                tieneContenido = !tieneContenido;
            }

            if (tieneContenido)
            {
                return Visibility.Visible;
            }

            return ColapsarCuandoEsFalso ? Visibility.Collapsed : Visibility.Hidden;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
