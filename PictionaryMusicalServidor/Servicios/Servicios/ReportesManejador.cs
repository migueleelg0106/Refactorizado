using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.ServiceModel;
using Datos.Modelo;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Servicio encargado de registrar reportes de jugadores.
    /// Valida la informacion recibida y evita duplicados por jugador reportante y reportado.
    /// </summary>
    public class ReportesManejador : IReportesManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ReportesManejador));

        private readonly IContextoFactoria _contextoFactory;
        private readonly IValidadorNombreUsuario _validadorUsuario;

        public ReportesManejador() : this(new ContextoFactoria(), new ValidadorNombreUsuario())
        {
        }

        public ReportesManejador(
            IContextoFactoria contextoFactory,
            IValidadorNombreUsuario validadorUsuario)
        {
            _contextoFactory = contextoFactory ??
                throw new ArgumentNullException(nameof(contextoFactory));
            _validadorUsuario = validadorUsuario ??
                throw new ArgumentNullException(nameof(validadorUsuario));
        }

        /// <summary>
        /// Registra un reporte de un jugador hacia otro.
        /// Valida la informacion, verifica duplicados y almacena el reporte.
        /// </summary>
        /// <param name="reporte">Datos del reporte enviado por el cliente.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ResultadoOperacionDTO ReportarJugador(ReporteJugadorDTO reporte)
        {
            try
            {
                ValidarSolicitud(reporte);
                string motivoNormalizado = EntradaComunValidador.NormalizarTexto(reporte.Motivo);

                using (var contexto = _contextoFactory.CrearContexto())
                {
                    var idsUsuarios = ObtenerIdentificadoresUsuarios(contexto, reporte);
                    var reporteRepositorio = new ReporteRepositorio(contexto);

                    if (idsUsuarios.IdReportante == idsUsuarios.IdReportado)
                    {
                        return CrearResultadoFallo(
                            MensajesError.Cliente.ReporteMismoUsuario);
                    }

                    if (reporteRepositorio.ExisteReporte(
                        idsUsuarios.IdReportante,
                        idsUsuarios.IdReportado))
                    {
                        return CrearResultadoFallo(
                            MensajesError.Cliente.ReporteDuplicado);
                    }

                    var nuevoReporte = new Reporte
                    {
                        idReportante = idsUsuarios.IdReportante,
                        idReportado = idsUsuarios.IdReportado,
                        Motivo = motivoNormalizado,
                        Fecha_Reporte = DateTime.Now
                    };

                    reporteRepositorio.CrearReporte(nuevoReporte);

                    return new ResultadoOperacionDTO
                    {
                        OperacionExitosa = true,
                        Mensaje = MensajesError.Cliente.ReporteRegistrado
                    };
                }
            }
            catch (FaultException excepcion)
            {
                _logger.Warn("Validacion fallida al registrar reporte.", excepcion);
                return CrearResultadoFallo(excepcion.Message);
            }
            catch (ArgumentException excepcion)
            {
                _logger.Warn("Datos invalidos al registrar reporte.", excepcion);
                return CrearResultadoFallo(excepcion.Message);
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Warn("Operacion invalida al registrar reporte.", excepcion);
                return CrearResultadoFallo(excepcion.Message);
            }
            catch (KeyNotFoundException excepcion)
            {
                _logger.Warn("No se encontraron usuarios al registrar reporte.", excepcion);
                return CrearResultadoFallo(MensajesError.Cliente.UsuariosNoEncontrados);
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error de base de datos al registrar reporte.", excepcion);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorCrearReporte);
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error de datos al registrar reporte.", excepcion);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorCrearReporte);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error de datos al registrar reporte.", excepcion);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorCrearReporte);
            }
        }

        private void ValidarSolicitud(ReporteJugadorDTO reporte)
        {
            if (reporte == null)
            {
                throw new FaultException(MensajesError.Cliente.DatosInvalidos);
            }

            _validadorUsuario.Validar(
                reporte.NombreUsuarioReportante,
                "usuario reportante");
            _validadorUsuario.Validar(
                reporte.NombreUsuarioReportado,
                "usuario reportado");

            string motivo = EntradaComunValidador.NormalizarTexto(reporte.Motivo);
            if (motivo == null)
            {
                throw new FaultException(MensajesError.Cliente.ReporteMotivoObligatorio);
            }

            if (!EntradaComunValidador.EsLongitudValidaReporte(motivo))
            {
                throw new FaultException(MensajesError.Cliente.ReporteMotivoLongitud);
            }
        }

        private (int IdReportante, int IdReportado) ObtenerIdentificadoresUsuarios(
           BaseDatosPruebaEntities contexto,
           ReporteJugadorDTO reporte)
        {
            try
            {
                var usuarioRepositorio = new UsuarioRepositorio(contexto);
                var reportante = usuarioRepositorio.ObtenerPorNombreUsuario(
                    EntradaComunValidador.NormalizarTexto(reporte.NombreUsuarioReportante));
                var reportado = usuarioRepositorio.ObtenerPorNombreUsuario(
                    EntradaComunValidador.NormalizarTexto(reporte.NombreUsuarioReportado));

                if (reportante == null || reportado == null)
                {
                    throw new FaultException(
                        MensajesError.Cliente.UsuariosEspecificadosNoExisten);
                }

                return (reportante.idUsuario, reportado.idUsuario);
            }
            catch (KeyNotFoundException excepcion)
            {
                _logger.Warn("Intento de reporte con usuarios no registrados.", excepcion);
                throw new FaultException(MensajesError.Cliente.UsuariosEspecificadosNoExisten);
            }
            catch (Exception excepcion)
            {
                _logger.Warn("Intento de reporte con usuarios no registrados.", excepcion);
                throw new FaultException(MensajesError.Cliente.UsuariosEspecificadosNoExisten);
            }
        }

        private ResultadoOperacionDTO CrearResultadoFallo(string mensaje)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = mensaje
            };
        }
    }
}
