using System;
using System.ServiceModel;
using System.Threading.Tasks;
using log4net;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante
{
    /// <summary>
    /// Helper para manejar el ciclo de vida de los clientes WCF de forma segura.
    /// </summary>
    public static class WcfClienteAyudante
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(WcfClienteAyudante));

        /// <summary>
        /// Ejecuta una operacion asincrona en un cliente WCF, asegurando el cierre correcto 
        /// del canal.
        /// </summary>
        public static async Task<TResult> UsarAsincronoAsync<TClient, TResult>(
            TClient cliente,
            Func<TClient, Task<TResult>> operacion)
            where TClient : class, ICommunicationObject
        {
            if (cliente == null)
            {
                throw new ArgumentNullException(nameof(cliente));
            }

            if (operacion == null)
            {
                throw new ArgumentNullException(nameof(operacion));
            }

            try
            {
                TResult resultado = await operacion(cliente).ConfigureAwait(false);
                Cerrar(cliente);
                return resultado;
            }
            catch (CommunicationException)
            {
                Abortar(cliente);
                throw;
            }
            catch (TimeoutException)
            {
                Abortar(cliente);
                throw;
            }
            catch (InvalidOperationException)
            {
                Abortar(cliente);
                throw;
            }
            catch (Exception)
            {
                Abortar(cliente);
                throw;
            }
        }

        private static void Cerrar(ICommunicationObject cliente)
        {
            if (cliente == null)
            {
                return;
            }

            try
            {
                if (cliente.State == CommunicationState.Opened)
                {
                    cliente.Close();
                }
                else
                {
                    cliente.Abort();
                }
            }
            catch (CommunicationException ex)
            {
                _logger.Warn("Excepción al cerrar cliente WCF.", ex);
                cliente.Abort();
            }
            catch (TimeoutException ex)
            {
                _logger.Warn("Timeout al cerrar cliente WCF.", ex);
                cliente.Abort();
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Operación inválida al cerrar cliente WCF.", ex);
                cliente.Abort();
            }
        }

        private static void Abortar(ICommunicationObject cliente)
        {
            if (cliente == null)
            {
                return;
            }

            try
            {
                cliente.Abort();
            }
            catch (Exception ex)
            {
                // Ignorado de manera intencional: No se puede hacer nada para manejar
                // una excepcion al abortar.
                _logger.Error("Error crítico al abortar cliente WCF.", ex);
            }
        }
    }
}