using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PictionaryMusicalCliente.Convertidores
{
    /// <summary>
    /// Convierte valores booleanos a <see cref="Visibility"/> permitiendo inversión y selección
    /// del estado cuando el valor es falso.
    /// </summary>
    public class ConvertidorBooleanoVisibilidad : IValueConverter
    {
        /// <summary>
        /// Indica si el resultado debe invertirse.
        /// </summary>
        public bool Invertir { get; set; }

        /// <summary>
        /// Indica si se debe devolver <see cref="Visibility.Collapsed"/> cuando el valor es falso.
        /// Si es <c>false</c> se devolverá <see cref="Visibility.Hidden"/>.
        /// </summary>
        public bool ColapsarCuandoEsFalso { get; set; } = true;

        /// <inheritdoc />
        public object Convert(
            object value, 
            Type targetType, 
            object parameter, 
            CultureInfo culture)
        {
            bool valorBooleano = false;

            if (value is bool boolValor)
            {
                valorBooleano = boolValor;
            }

            if (Invertir)
            {
                valorBooleano = !valorBooleano;
            }

            if (valorBooleano)
            {
                return Visibility.Visible;
            }

            return ColapsarCuandoEsFalso ? Visibility.Collapsed : Visibility.Hidden;
        }

        /// <inheritdoc />
        public object ConvertBack(
            object value, 
            Type targetType, 
            object parameter, 
            CultureInfo culture)
        {
            if (value is Visibility visibilidad)
            {
                bool resultado = visibilidad == Visibility.Visible;

                return Invertir ? !resultado : resultado;
            }

            return Binding.DoNothing;
        }
    }
}