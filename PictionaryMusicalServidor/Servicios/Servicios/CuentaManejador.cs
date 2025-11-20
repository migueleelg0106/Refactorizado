using PictionaryMusicalServidor.Servicios.Contratos;
using System;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Datos.Modelo;
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
    /// Valida los datos de entrada, verifica que el correo no este duplicado y registra nuevas cuentas.
    /// </summary>
    public class CuentaManejador : ICuentaManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CuentaManejador));

        /// <summary>
        /// Registra una nueva cuenta en el sistema.
        /// Valida que el codigo de verificacion este confirmado, que el usuario y correo no esten duplicados,
        /// crea la clasificacion inicial, registra el jugador y el usuario, y encripta la contrasena.
        /// </summary>
        /// <param name="nuevaCuenta">Datos de la nueva cuenta a registrar.</param>
        /// <returns>Resultado del registro de la cuenta.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si nuevaCuenta es null.</exception>
        /// <exception cref="DbEntityValidationException">Se captura cuando hay errores de validacion de entidades.</exception>
        /// <exception cref="DbUpdateException">Se captura cuando hay errores al actualizar la base de datos.</exception>
        /// <exception cref="EntityException">Se captura cuando hay errores de conexion con la base de datos.</exception>
        /// <exception cref="DataException">Se captura cuando hay errores relacionados con los datos.</exception>
        /// <exception cref="InvalidOperationException">Se captura cuando hay operaciones invalidas.</exception>
        public ResultadoRegistroCuentaDTO RegistrarCuenta(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                throw new ArgumentNullException(nameof(nuevaCuenta));
            }

            ResultadoOperacionDTO validacionDatos = EntradaComunValidador.ValidarNuevaCuenta(nuevaCuenta);
            if (!validacionDatos.OperacionExitosa)
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = validacionDatos.Mensaje
                };
            }

            try
            {
                using (var contexto = ContextoFactory.CrearContexto())
                {
                    ResultadoRegistroCuentaDTO validacion = ValidarPrecondicionesRegistro(contexto, nuevaCuenta);
                    if (!validacion.RegistroExitoso)
                    {
                        return validacion;
                    }

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
                        usuarioRepositorio.CrearUsuario(new Usuario
                        {
                            Nombre_Usuario = nuevaCuenta.Usuario,
                            Contrasena = BCryptNet.HashPassword(nuevaCuenta.Contrasena),
                            Jugador_idJugador = jugador.idJugador
                        });

                        transaccion.Commit();

                        ServicioVerificacionRegistro.LimpiarVerificacion(nuevaCuenta);

                        return new ResultadoRegistroCuentaDTO
                        {
                            RegistroExitoso = true
                        };
                    }
                }
            }
            catch (DbEntityValidationException ex)
            {
                _logger.Error(MensajesError.Log.RegistroCuentaValidacionEntidad, ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorRegistrarCuenta
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.Error(MensajesError.Log.RegistroCuentaActualizacionBD, ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorRegistrarCuenta
                };
            }
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.RegistroCuentaErrorBD, ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorRegistrarCuenta
                };
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.RegistroCuentaErrorDatos, ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorRegistrarCuenta
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.RegistroCuentaOperacionInvalida, ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorRegistrarCuenta
                };
            }
        }

        private ResultadoRegistroCuentaDTO ValidarPrecondicionesRegistro(BaseDatosPruebaEntities1 contexto, NuevaCuentaDTO nuevaCuenta)
        {
            if (!ServicioVerificacionRegistro.EstaVerificacionConfirmada(nuevaCuenta))
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.CuentaNoVerificada
                };
            }

            bool usuarioRegistrado = contexto.Usuario.Any(u => u.Nombre_Usuario == nuevaCuenta.Usuario);
            bool correoRegistrado = contexto.Jugador.Any(j => j.Correo == nuevaCuenta.Correo);

            if (usuarioRegistrado || correoRegistrado)
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    UsuarioRegistrado = usuarioRegistrado,
                    CorreoRegistrado = correoRegistrado,
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

        /// <summary>
        /// Solicita el envio de un codigo de verificacion para registrar una nueva cuenta.
        /// Delega la operacion al servicio de verificacion de registro.
        /// </summary>
        /// <param name="nuevaCuenta">Datos de la nueva cuenta a registrar.</param>
        /// <returns>Resultado de la solicitud del codigo de verificacion.</returns>
        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            return ServicioVerificacionRegistro.SolicitarCodigo(nuevaCuenta);
        }

        /// <summary>
        /// Reenvia un codigo de verificacion previamente solicitado.
        /// Delega la operacion al servicio de verificacion de registro.
        /// </summary>
        /// <param name="solicitud">Solicitud con el token del codigo a reenviar.</param>
        /// <returns>Resultado del reenvio del codigo de verificacion.</returns>
        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenvioCodigoVerificacionDTO solicitud)
        {
            return ServicioVerificacionRegistro.ReenviarCodigo(solicitud);
        }

        /// <summary>
        /// Confirma un codigo de verificacion ingresado por el usuario.
        /// Delega la operacion al servicio de verificacion de registro.
        /// </summary>
        /// <param name="confirmacion">Datos para confirmar el codigo de verificacion.</param>
        /// <returns>Resultado de la confirmacion del codigo.</returns>
        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmacionCodigoDTO confirmacion)
        {
            return ServicioVerificacionRegistro.ConfirmarCodigo(confirmacion);
        }
    }
}