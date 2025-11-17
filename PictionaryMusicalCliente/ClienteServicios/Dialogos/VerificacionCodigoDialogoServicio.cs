using System;
using System.Threading.Tasks;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using ICodigoVerificacionCli = PictionaryMusicalCliente.ClienteServicios.Abstracciones.ICodigoVerificacionServicio;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Dialogos
{
    public class VerificacionCodigoDialogoServicio : IVerificacionCodigoDialogoServicio
    {
        public Task<DTOs.ResultadoRegistroCuentaDTO> MostrarDialogoAsync(
            string descripcion,
            string tokenCodigo,
            ICodigoVerificacionCli codigoVerificacionServicio)
        {
            if (codigoVerificacionServicio == null)
                throw new ArgumentNullException(nameof(codigoVerificacionServicio));

            var ventana = new VerificacionCodigo();
            var vistaModelo = new VerificacionCodigoVistaModelo(descripcion, tokenCodigo, codigoVerificacionServicio);
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
