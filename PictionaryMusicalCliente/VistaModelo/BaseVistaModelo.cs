using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading.Tasks;

namespace PictionaryMusicalCliente.VistaModelo
{
    /// <summary>
    /// Clase base para todos los ViewModels. Implementa notificacion de cambios
    /// y manejo centralizado de excepciones asincronas.
    /// </summary>
    public abstract class BaseVistaModelo : INotifyPropertyChanged
    {
        protected readonly IVentanaServicio _ventana;
        private readonly ILocalizadorServicio _localizador;

        /// <summary>
        /// Evento para notificar cambios en las propiedades a la interfaz de usuario.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Constructor para inyeccion de dependencias.
        /// </summary>
        protected BaseVistaModelo(IVentanaServicio ventana, ILocalizadorServicio localizador)
        {
            _ventana = ventana ?? throw new ArgumentNullException(nameof(ventana));
            _localizador = localizador ?? throw new ArgumentNullException(nameof(localizador));
        }

        /// <summary>
        /// Ejecuta una operacion asincrona capturando excepciones comunes de WCF.
        /// </summary>
        /// <param name=\"operacion\">La tarea asincrona a ejecutar.</param>
        /// <param name=\"accionError\">Accion opcional a ejecutar si ocurre un error.</param>
        protected async Task EjecutarOperacionAsync(Func<Task> operacion, Action<Exception> 
            accionError = null)
        {
            try
            {
                await operacion();
            }
            catch (TimeoutException ex)
            {
                ManejarError(ex, "El servidor tardó demasiado en responder.", accionError);
            }
            catch (EndpointNotFoundException ex)
            {
                ManejarError(ex, "No se pudo conectar con el servidor. Verifique su conexión.",
                    accionError);
            }
            catch (CommunicationException ex)
            {
                ManejarError(ex, "Error de comunicación con el servicio.", accionError);
            }
            catch (ServicioExcepcion ex)
            {
                ManejarError(ex, ex.Message, accionError);
            }
            catch (Exception ex)
            {
                ManejarError(ex, "Ocurrió un error inesperado.", accionError);
            }
        }

        private void ManejarError(Exception ex, string mensajePorDefecto, Action<Exception> 
            accionError)
        {
            if (accionError != null)
            {
                accionError(ex);
            }
            else
            {
                string mensajeFinal = _localizador != null
                    ? _localizador.Localizar(null, mensajePorDefecto)
                    : mensajePorDefecto;

                _ventana?.MostrarError(mensajeFinal);
            }
        }

        /// <summary>
        /// Asigna un nuevo valor a una propiedad y notifica el cambio si es diferente.
        /// </summary>
        protected bool EstablecerPropiedad<T>(ref T campo, T valor, [CallerMemberName] 
            string nombrePropiedad = null)
        {
            if (Equals(campo, valor))
            {
                return false;
            }

            campo = valor;
            NotificarCambio(nombrePropiedad);
            return true;
        }

        /// <summary>
        /// Dispara el evento PropertyChanged.
        /// </summary>
        protected void NotificarCambio([CallerMemberName] string nombrePropiedad = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }
    }
}