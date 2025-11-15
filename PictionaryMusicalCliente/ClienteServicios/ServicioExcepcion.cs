using System;

namespace PictionaryMusicalCliente.ClienteServicios
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

    public class ServicioExcepcion : Exception
    {
        public ServicioExcepcion(TipoErrorServicio tipo, string mensaje, Exception causa = null)
            : base(string.IsNullOrWhiteSpace(mensaje) ? null : mensaje, causa)
        {
            Tipo = tipo;
        }

        public TipoErrorServicio Tipo { get; }
    }
}
