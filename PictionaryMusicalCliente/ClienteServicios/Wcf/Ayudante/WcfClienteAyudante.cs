using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace PictionaryMusicalCliente.Servicios.Wcf.Helpers
{
    public static class WcfClienteAyudante
    {
        public static async Task<TResult> UsarAsincrono<TClient, TResult>(
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
            catch
            {
                //Se propaga la excepción en dónde sea llamada
                Abortar(cliente);
                throw;
            }
        }

        public static async Task UsarAsincrono<TClient>(
            TClient cliente,
            Func<TClient, Task> operacion)
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
                await operacion(cliente).ConfigureAwait(false);
                Cerrar(cliente);
            }
            catch
            {
                //Se propaga la excepción en dónde sea llamada
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
            catch (CommunicationException)
            {
                cliente.Abort();
            }
            catch (TimeoutException)
            {
                cliente.Abort();
            }
            catch (InvalidOperationException)
            {
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
            catch
            {
                // Ignorado de manera intencional: No se puede hacer nada para manejar una excepción al abortar.
            }
        }
    }
}
