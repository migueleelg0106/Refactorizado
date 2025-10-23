using System;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using ICodigoVerificacionCli = PictionaryMusicalCliente.ClienteServicios.Abstracciones.ICodigoVerificacionServicio;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Dialogos
{
    public class VerificarCodigoDialogoServicio : IVerificarCodigoDialogoServicio
    {
        public Task<DTOs.ResultadoRegistroCuentaDTO> MostrarDialogoAsync(
            string descripcion,
            string tokenCodigo,
            ICodigoVerificacionCli codigoVerificacionService)
        {
            if (codigoVerificacionService == null)
                throw new ArgumentNullException(nameof(codigoVerificacionService));

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
