using PictionaryMusicalServidor.Servicios.Contratos;
using System;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using Datos.Modelo;
using BCryptNet = BCrypt.Net.BCrypt;
using System.Linq;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de gestion de cuentas de usuario.
    /// Maneja el proceso completo de registro incluyendo validacion, verificacion y creacion.
    /// </summary>
    public class CuentaManejador : ICuentaManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CuentaManejador));

        private readonly IContextoFactoria _contextoFactory;
        private readonly IVerificacionRegistroServicio _verificacionServicio;

        public CuentaManejador() : this(
            new ContextoFactoria(),
            new VerificacionRegistroServicio(
                new ContextoFactoria(),
                new NotificacionCodigosServicio(new CorreoCodigoVerificacionNotificador())))
        {
        }

        public CuentaManejador(
            IContextoFactoria contextoFactory,
            IVerificacionRegistroServicio verificacionServicio)
        {
            _contextoFactory = contextoFactory ??
                throw new ArgumentNullException(nameof(contextoFactory));

            _verificacionServicio = verificacionServicio ??
                throw new ArgumentNullException(nameof(verificacionServicio));
        }

        /// <summary>
        /// Registra una nueva cuenta de usuario en el sistema.
        /// Valida datos, verifica duplicados, crea entidades en transaccion y limpia verificacion.
        /// </summary>
        public ResultadoRegistroCuentaDTO RegistrarCuenta(NuevaCuentaDTO nuevaCuenta)
        {
            var validacionInicial = ValidarDatosEntrada(nuevaCuenta);
            if (!validacionInicial.OperacionExitosa)
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = validacionInicial.Mensaje
                };
            }

            try
            {
                using (var contexto = _contextoFactory.CrearContexto())
                {
                    var validacionNegocio = VerificarReglasDeNegocio(contexto, nuevaCuenta);
                    if (!validacionNegocio.RegistroExitoso)
                    {
                        return validacionNegocio;
                    }

                    EjecutarTransaccionRegistro(contexto, nuevaCuenta);

                    _verificacionServicio.LimpiarVerificacion(nuevaCuenta);

                    return new ResultadoRegistroCuentaDTO
                    {
                        RegistroExitoso = true
                    };
                }
            }
            catch (DbEntityValidationException ex)
            {
                _logger.Error("Validacion de entidad fallida durante el registro.", ex);
                return CrearFalloRegistro(MensajesError.Cliente.ErrorRegistrarCuenta);
            }
            catch (DbUpdateException ex)
            {
                _logger.Error("Error de actualizacion de BD durante el registro.", ex);
                return CrearFalloRegistro(MensajesError.Cliente.ErrorRegistrarCuenta);
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos durante el registro.", ex);
                return CrearFalloRegistro(MensajesError.Cliente.ErrorRegistrarCuenta);
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos durante el registro.", ex);
                return CrearFalloRegistro(MensajesError.Cliente.ErrorRegistrarCuenta);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operacion invalida durante el registro.", ex);
                return CrearFalloRegistro(MensajesError.Cliente.ErrorRegistrarCuenta);
            }
        }

        /// <summary>
        /// Solicita un codigo de verificacion para registrar una nueva cuenta.
        /// </summary>
        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            return _verificacionServicio.SolicitarCodigo(nuevaCuenta);
        }

        /// <summary>
        /// Reenvia el codigo de verificacion previamente solicitado.
        /// </summary>
        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(
            ReenvioCodigoVerificacionDTO solicitud)
        {
            return _verificacionServicio.ReenviarCodigo(solicitud);
        }

        /// <summary>
        /// Confirma el codigo de verificacion ingresado por el usuario.
        /// </summary>
        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(
            ConfirmacionCodigoDTO confirmacion)
        {
            return _verificacionServicio.ConfirmarCodigo(confirmacion);
        }

        private ResultadoOperacionDTO ValidarDatosEntrada(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                throw new ArgumentNullException(nameof(nuevaCuenta));
            }

            return EntradaComunValidador.ValidarNuevaCuenta(nuevaCuenta);
        }

        private ResultadoRegistroCuentaDTO VerificarReglasDeNegocio(
            BaseDatosPruebaEntities contexto,
            NuevaCuentaDTO nuevaCuenta)
        {
            if (!VerificarEstadoValidacion(nuevaCuenta))
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.CuentaNoVerificada
                };
            }

            if (VerificarDuplicados(contexto, nuevaCuenta))
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    UsuarioRegistrado = true,
                    CorreoRegistrado = true,
                    Mensaje = null
                };
            }

            if (nuevaCuenta.AvatarId <= 0)
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.AvatarInvalido
                };
            }

            return new ResultadoRegistroCuentaDTO { RegistroExitoso = true };
        }

        private bool VerificarEstadoValidacion(NuevaCuentaDTO nuevaCuenta)
        {
            if (!_verificacionServicio.EstaVerificacionConfirmada(nuevaCuenta))
            {
                _logger.Warn("Intento de registro sin verificacion confirmada.");
                return false;
            }
            return true;
        }

        private bool VerificarDuplicados(
            BaseDatosPruebaEntities contexto,
            NuevaCuentaDTO nuevaCuenta)
        {
            bool usuarioRegistrado = contexto.Usuario.Any(
                u => u.Nombre_Usuario == nuevaCuenta.Usuario);

            bool correoRegistrado = contexto.Jugador.Any(
                j => j.Correo == nuevaCuenta.Correo);

            if (usuarioRegistrado || correoRegistrado)
            {
                _logger.Warn("Registro duplicado detectado (usuario o correo existente).");
                return true;
            }

            return false;
        }

        private void EjecutarTransaccionRegistro(
            BaseDatosPruebaEntities contexto,
            NuevaCuentaDTO nuevaCuenta)
        {
            using (var transaccion = contexto.Database.BeginTransaction())
            {
                var clasificacionRepositorio = new ClasificacionRepositorio(contexto);
                var clasificacion = clasificacionRepositorio.CrearClasificacionInicial();

                var jugadorRepositorio = new JugadorRepositorio(contexto);
                var jugador = jugadorRepositorio.CrearJugador(new Jugador
                {
                    Nombre = nuevaCuenta.Nombre,
                    Apellido = nuevaCuenta.Apellido,
                    Correo = nuevaCuenta.Correo,
                    Id_Avatar = nuevaCuenta.AvatarId,
                    Clasificacion_idClasificacion = clasificacion.idClasificacion
                });

                var usuarioRepositorio = new UsuarioRepositorio(contexto);
                var usuarioCreado = usuarioRepositorio.CrearUsuario(new Usuario
                {
                    Nombre_Usuario = nuevaCuenta.Usuario,
                    Contrasena = BCryptNet.HashPassword(nuevaCuenta.Contrasena),
                    Jugador_idJugador = jugador.idJugador
                });

                transaccion.Commit();

                _logger.InfoFormat(
                    "Cuenta creada exitosamente con usuario id {0} y jugador id {1}.",
                    usuarioCreado.idUsuario,
                    jugador.idJugador);
            }
        }

        private static ResultadoRegistroCuentaDTO CrearFalloRegistro(string mensaje)
        {
            return new ResultadoRegistroCuentaDTO
            {
                RegistroExitoso = false,
                Mensaje = mensaje
            };
        }
    }
}