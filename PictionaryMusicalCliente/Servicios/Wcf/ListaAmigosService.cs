using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using PictionaryMusicalCliente.Servicios;
using ListaAmigosSrv = PictionaryMusicalCliente.PictionaryServidorServicioListaAmigos;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class ListaAmigosService : IListaAmigosService, ListaAmigosSrv.IListaAmigosManejadorCallback
    {
        private const string ListaAmigosEndpoint = "NetTcpBinding_IListaAmigosManejador";

        private readonly object _syncRoot = new object();
        private readonly SynchronizationContext _synchronizationContext;
        private ListaAmigosSrv.ListaAmigosManejadorClient _cliente;

        public ListaAmigosService()
        {
            _synchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public event EventHandler<ListaAmigosResultado> ListaActualizada;

        public async Task<ListaAmigosResultado> ObtenerListaAmigosAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));
            }

            ListaAmigosSrv.ListaAmigosManejadorClient cliente = ObtenerCliente();

            try
            {
                ListaAmigosSrv.ListaAmigosDTO resultado = await cliente
                    .ObtenerListaAmigosAsync(nombreUsuario)
                    .ConfigureAwait(false);

                ListaAmigosResultado conversion = ConvertirResultado(resultado);

                if (conversion == null)
                {
                    return ListaAmigosResultado.Fallo(Lang.amigosTextoListaNoDisponible);
                }

                if (!conversion.Exito && string.IsNullOrWhiteSpace(conversion.Mensaje))
                {
                    return ListaAmigosResultado.Fallo(Lang.amigosTextoListaNoDisponible, conversion.Amigos);
                }

                return conversion;
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.amigosTextoListaNoDisponible);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                ReiniciarCliente();
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.amigosTextoListaNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                ReiniciarCliente();
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.amigosTextoListaNoDisponible, ex);
            }
            catch (CommunicationException ex)
            {
                ReiniciarCliente();
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.amigosTextoListaNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                ReiniciarCliente();
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.amigosTextoListaNoDisponible, ex);
            }
        }

        public void ListaAmigosActualizada(ListaAmigosSrv.ListaAmigosDTO lista)
        {
            ListaAmigosResultado resultado = ConvertirResultado(lista);

            if (resultado == null)
            {
                resultado = ListaAmigosResultado.Fallo(Lang.amigosTextoListaNoDisponible);
            }
            else if (!resultado.Exito && string.IsNullOrWhiteSpace(resultado.Mensaje))
            {
                resultado = ListaAmigosResultado.Fallo(Lang.amigosTextoListaNoDisponible, resultado.Amigos);
            }

            NotificarListaActualizada(resultado);
        }

        public void Dispose()
        {
            CerrarCliente();
        }

        private ListaAmigosSrv.ListaAmigosManejadorClient ObtenerCliente()
        {
            lock (_syncRoot)
            {
                if (_cliente != null && _cliente.State != CommunicationState.Faulted)
                {
                    return _cliente;
                }

                CerrarCliente();

                var contexto = new InstanceContext(this);
                _cliente = new ListaAmigosSrv.ListaAmigosManejadorClient(contexto, ListaAmigosEndpoint);
                return _cliente;
            }
        }

        private void ReiniciarCliente()
        {
            CerrarCliente();
        }

        private void CerrarCliente()
        {
            lock (_syncRoot)
            {
                if (_cliente == null)
                {
                    return;
                }

                try
                {
                    if (_cliente.State == CommunicationState.Opened)
                    {
                        _cliente.Close();
                    }
                    else
                    {
                        _cliente.Abort();
                    }
                }
                catch
                {
                    _cliente.Abort();
                }
                finally
                {
                    _cliente = null;
                }
            }
        }

        private void NotificarListaActualizada(ListaAmigosResultado resultado)
        {
            if (resultado == null)
            {
                return;
            }

            void Publicar(object state) => ListaActualizada?.Invoke(this, resultado);

            _synchronizationContext.Post(Publicar, null);
        }

        private static ListaAmigosResultado ConvertirResultado(ListaAmigosSrv.ListaAmigosDTO dto)
        {
            if (dto == null)
            {
                return null;
            }

            return dto.OperacionExitosa
                ? ListaAmigosResultado.Exitoso(dto.Amigos, dto.Mensaje)
                : ListaAmigosResultado.Fallo(dto.Mensaje, dto.Amigos);
        }
    }
}
