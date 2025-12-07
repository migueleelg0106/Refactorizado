using System.ServiceModel;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para procesar excepciones de servicios WCF y convertirlas en mensajes
    /// amigables.
    /// </summary>
    public interface IManejadorErrorServicio
    {
        /// <summary>
        /// Obtiene un mensaje localizado y amigable a partir de una excepción FaultException.
        /// </summary>
        /// <param name="excepcion">La excepción capturada del servicio WCF.</param>
        /// <param name="mensajePredeterminado">Mensaje a retornar si no se puede extraer
        /// información específica de la excepción.</param>
        /// <returns>El mensaje final localizado listo para mostrar al usuario.</returns>
        string ObtenerMensaje(FaultException excepcion, string mensajePredeterminado);
    }
}