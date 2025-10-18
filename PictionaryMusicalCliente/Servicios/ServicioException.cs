using System;

namespace PictionaryMusicalCliente.Servicios
{
    public enum TipoErrorServicio
    {
        Ninguno,
        FallaServicio,
        Comunicacion,
        TiempoAgotado,
        OperacionInvalida,
        Desconocido
    }

    public class ServicioException : Exception
    {
        public ServicioException(TipoErrorServicio tipo, string mensaje, Exception causa = null)
            : base(string.IsNullOrWhiteSpace(mensaje) ? null : mensaje, causa)
        {
            Tipo = tipo;
        }

        public TipoErrorServicio Tipo { get; }
    }
}
