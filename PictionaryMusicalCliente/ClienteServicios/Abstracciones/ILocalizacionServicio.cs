using System;
using System.Globalization;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface ILocalizacionServicio
    {
        event EventHandler IdiomaActualizado;

        CultureInfo CulturaActual { get; }

        void EstablecerIdioma(string codigoIdioma);

        void EstablecerCultura(CultureInfo cultura);
    }
}
