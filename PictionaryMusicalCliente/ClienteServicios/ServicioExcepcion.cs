using System;

namespace PictionaryMusicalCliente.ClienteServicios
{
    /// <summary>
    /// Categoria del error ocurrido en el consumo del servicio.
    /// </summary>
    public enum TipoErrorServicio
    {
        /// <summary>Sin error.</summary>
        Ninguno,
        /// <summary>Error logico o validacion del lado del servidor.</summary>
        FallaServicio,
        /// <summary>Error de red o conexion.</summary>
        Comunicacion,
        /// <summary>El tiempo de espera de la solicitud expiro.</summary>
        TiempoAgotado,
        /// <summary>La operacion no es valida en el estado actual.</summary>
        OperacionInvalida,
        /// <summary>Error no clasificado.</summary>
        Desconocido
    }

    /// <summary>
    /// Excepcion personalizada para encapsular errores provenientes de servicios WCF.
    /// </summary>
    public class ServicioExcepcion : Exception
    {
        /// <summary>
        /// Inicializa una nueva instancia de la excepcion.
        /// </summary>
        /// <param name="tipo">La categoria del error.</param>
        /// <param name="mensaje">El mensaje descriptivo.</param>
        /// <param name="causa">La excepcion original (opcional).</param>
        public ServicioExcepcion(TipoErrorServicio tipo, string mensaje, Exception causa = null)
            : base(string.IsNullOrWhiteSpace(mensaje) ? null : mensaje, causa)
        {
            Tipo = tipo;
        }

        /// <summary>
        /// Obtiene el tipo de error categorizado.
        /// </summary>
        public TipoErrorServicio Tipo { get; }
    }
}