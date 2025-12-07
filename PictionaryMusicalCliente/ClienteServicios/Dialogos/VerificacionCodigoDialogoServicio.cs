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
        private ICodigoVerificacionServicio _codigoVerificacionServicio;
        private IAvisoServicio _avisoServicio;
        private ILocalizadorServicio _localizadorServicio;
        private ISonidoManejador _sonidoManejador;

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
            _codigoVerificacionServicio = codigoVerificacionServicio ??
                throw new ArgumentNullException(nameof(codigoVerificacionServicio));
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));
            _localizadorServicio = localizadorServicio ??
                throw new ArgumentNullException(nameof(localizadorServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));

            ValidarServicio(codigoVerificacionServicio);

            var finalizacion = new TaskCompletionSource<DTOs.ResultadoRegistroCuentaDTO>();
            var ventana = new VerificacionCodigo();

            var vistaModelo = CrearVistaModelo(
                descripcion,
                tokenCodigo,
                codigoVerificacionServicio);

            ConfigurarEventos(vistaModelo, ventana, finalizacion);
            ConfigurarCierreVentana(ventana, finalizacion);

            ventana.ConfigurarVistaModelo(vistaModelo);
            ventana.ShowDialog();

            return finalizacion.Task;
        }

        private void ValidarServicio(ICodigoVerificacionServicio servicio)
        {
            if (servicio == null)
            {
                var ex = new ArgumentNullException(nameof(servicio));
                _logger.Error("Intento de abrir dialogo de verificacion con servicio nulo.", ex);
                throw ex;
            }
        }

        private VerificacionCodigoVistaModelo CrearVistaModelo(
            string descripcion,
            string token,
            ICodigoVerificacionServicio servicio)
        {
            return new VerificacionCodigoVistaModelo(descripcion, token, 
                _codigoVerificacionServicio, _avisoServicio, 
                _localizadorServicio, _sonidoManejador);
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