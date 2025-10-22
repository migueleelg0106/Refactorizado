using System;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;

namespace PictionaryMusicalCliente.Servicios.Dialogos
{
    public class VerificarCodigoDialogService : IVerificarCodigoDialogService
    {
        public Task<DTOs.ResultadoRegistroCuentaDTO> MostrarDialogoAsync(
            string descripcion,
            string tokenCodigo,
            ICodigoVerificacionService codigoVerificacionService)
        {
            if (codigoVerificacionService == null)
            {
                throw new ArgumentNullException(nameof(codigoVerificacionService));
            }

            var ventana = new VerificarCodigo();
            var vistaModelo = new VerificarCodigoVistaModelo(descripcion, tokenCodigo, codigoVerificacionService);
            var finalizacion = new TaskCompletionSource<DTOs.ResultadoRegistroCuentaDTO>();

            vistaModelo.VerificacionCompletada = resultado =>
            {
                finalizacion.TrySetResult(resultado);
                ventana.Close();
            };

            vistaModelo.Cancelado = () =>
            {
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
