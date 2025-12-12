using System;
using System.Threading.Tasks;
using log4net;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalCliente.Vista;
using PictionaryMusicalCliente.Utilidades.Abstracciones;

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
            ICodigoVerificacionServicio codigoVerificacionServicio,
            IAvisoServicio avisoServicio,
            ILocalizadorServicio localizadorServicio,
            ISonidoManejador sonidoManejador)
        {
            if (codigoVerificacionServicio == null)
                throw new ArgumentNullException(nameof(codigoVerificacionServicio));
            if (avisoServicio == null)
                throw new ArgumentNullException(nameof(avisoServicio));
            if (localizadorServicio == null)
                throw new ArgumentNullException(nameof(localizadorServicio));
            if (sonidoManejador == null)
                throw new ArgumentNullException(nameof(sonidoManejador));

            var finalizacion = new TaskCompletionSource<DTOs.ResultadoRegistroCuentaDTO>();
            var ventana = new VerificacionCodigo();

            var vistaModelo = new VerificacionCodigoVistaModelo(
                App.VentanaServicio,
                localizadorServicio,
                descripcion,
                tokenCodigo,
                codigoVerificacionServicio,
                avisoServicio,
                sonidoManejador);

            ConfigurarEventos(vistaModelo, ventana, finalizacion);
            ConfigurarCierreVentana(ventana, finalizacion);

            ventana.DataContext = vistaModelo;
            ventana.ShowDialog();

            return finalizacion.Task;
        }

        private void ConfigurarEventos(
            VerificacionCodigoVistaModelo vistaModelo,
            VerificacionCodigo ventana,
            TaskCompletionSource<DTOs.ResultadoRegistroCuentaDTO> finalizacion)
        {
            vistaModelo.VerificacionCompletada = resultado =>
            {
                _logger.Info("Verificacion de codigo completada exitosamente.");
                finalizacion.TrySetResult(resultado);
                ventana.Close();
            };

            vistaModelo.Cancelado = () =>
            {
                finalizacion.TrySetResult(null);
                ventana.Close();
            };
        }

        private void ConfigurarCierreVentana(
            VerificacionCodigo ventana,
            TaskCompletionSource<DTOs.ResultadoRegistroCuentaDTO> finalizacion)
        {
            ventana.Closed += (_, __) =>
            {
                if (!finalizacion.Task.IsCompleted)
                {
                    finalizacion.TrySetResult(null);
                }
            };
        }
    }
}