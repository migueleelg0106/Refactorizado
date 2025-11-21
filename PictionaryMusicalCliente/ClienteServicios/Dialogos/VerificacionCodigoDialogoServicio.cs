using System;
using System.Threading.Tasks;
using log4net;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using ICodigoVerificacionCli = PictionaryMusicalCliente.ClienteServicios.
    Abstracciones.ICodigoVerificacionServicio;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Dialogos
{
    /// <summary>
    /// Servicio de dialogo para manejar la interfaz de verificacion de codigo.
    /// </summary>
    public class VerificacionCodigoDialogoServicio : IVerificacionCodigoDialogoServicio
    {
        private static readonly ILog _logger = 
            LogManager.GetLogger(typeof(VerificacionCodigoDialogoServicio));

        /// <summary>
        /// Muestra el dialogo de verificacion y retorna el resultado.
        /// </summary>
        public Task<DTOs.ResultadoRegistroCuentaDTO> MostrarDialogoAsync(
            string descripcion,
            string tokenCodigo,
            ICodigoVerificacionCli codigoVerificacionServicio)
        {
            if (codigoVerificacionServicio == null)
            {
                var ex = new ArgumentNullException(nameof(codigoVerificacionServicio));
                _logger.Error("Intento de abrir diálogo de verificación con servicio nulo.", ex);
                throw ex;
            }

            _logger.Info("Abriendo diálogo de verificación de código.");

            var ventana = new VerificacionCodigo();
            var vistaModelo = new VerificacionCodigoVistaModelo(
                descripcion,
                tokenCodigo,
                codigoVerificacionServicio);

            var finalizacion = new TaskCompletionSource<DTOs.ResultadoRegistroCuentaDTO>();

            vistaModelo.VerificacionCompletada = resultado =>
            {
                _logger.Info("Verificación de código completada exitosamente en el diálogo.");
                finalizacion.TrySetResult(resultado);
                ventana.Close();
            };

            vistaModelo.Cancelado = () =>
            {
                _logger.Info("Verificación de código cancelada por el usuario.");
                finalizacion.TrySetResult(null);
                ventana.Close();
            };

            ventana.ConfigurarVistaModelo(vistaModelo);

            ventana.Closed += (_, __) =>
            {
                if (!finalizacion.Task.IsCompleted)
                {
                    finalizacion.TrySetResult(null);
                }
            };

            ventana.ShowDialog();

            return finalizacion.Task;
        }
    }
}