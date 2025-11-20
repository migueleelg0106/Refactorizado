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
    /// Maneja el proceso completo de registro incluyendo validacion, verificacion de codigo y creacion de entidades.
    /// Verifica que el correo y usuario no esten duplicados antes de crear la cuenta.
    /// </summary>
    public class CuentaManejador : ICuentaManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CuentaManejador));

        /// <summary>
        /// Registra una nueva cuenta de usuario en el sistema.
        /// Valida datos de entrada, verifica que el usuario y correo no esten registrados,
        /// crea entidades de clasificacion, jugador y usuario en una transaccion, y limpia la verificacion.
        /// </summary>
        /// <param name="nuevaCuenta">Datos completos de la cuenta a registrar.</param>
        /// <returns>Resultado del registro indicando exito o conflictos.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si nuevaCuenta es null.</exception>
        /// <exception cref="DbEntityValidationException">Se lanza si hay errores de validacion en entidades de base de datos.</exception>
        /// <exception cref="DbUpdateException">Se lanza si hay errores al actualizar la base de datos.</exception>
        /// <exception cref="EntityException">Se lanza si hay errores de conexion con la base de datos.</exception>
        /// <exception cref="DataException">Se lanza si hay errores de datos durante el registro.</exception>
        /// <exception cref="InvalidOperationException">Se lanza si hay operaciones invalidas durante el registro.</exception>
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

                        _logger.Info($"Nueva cuenta registrada exitosamente. Usuario: {nuevaCuenta.Usuario}, Correo: {nuevaCuenta.Correo}");

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
                _logger.Warn($"Intento de registro sin verificación confirmada. Correo: {nuevaCuenta.Correo}");
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
                _logger.Warn($"Intento de registro duplicado. Usuario existe: {usuarioRegistrado}, Correo existe: {correoRegistrado}");
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
        /// Solicita un codigo de verificacion para registrar una nueva cuenta.
        /// Delega en ServicioVerificacionRegistro para generar y enviar el codigo.
        /// </summary>
        /// <param name="nuevaCuenta">Datos de la nueva cuenta a verificar.</param>
        /// <returns>Resultado de la solicitud del codigo de verificacion.</returns>
        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            return ServicioVerificacionRegistro.SolicitarCodigo(nuevaCuenta);
        }

        /// <summary>
        /// Reenvia el codigo de verificacion previamente solicitado.
        /// Delega en ServicioVerificacionRegistro para reenviar el codigo.
        /// </summary>
        /// <param name="solicitud">Datos para el reenvio del codigo.</param>
        /// <returns>Resultado del reenvio del codigo de verificacion.</returns>
        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenvioCodigoVerificacionDTO solicitud)
        {
            return ServicioVerificacionRegistro.ReenviarCodigo(solicitud);
        }

        /// <summary>
        /// Confirma el codigo de verificacion ingresado por el usuario.
        /// Delega en ServicioVerificacionRegistro para validar el codigo.
        /// </summary>
        /// <param name="confirmacion">Datos de confirmacion del codigo.</param>
        /// <returns>Resultado de la confirmacion del codigo.</returns>
        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmacionCodigoDTO confirmacion)
        {
            return ServicioVerificacionRegistro.ConfirmarCodigo(confirmacion);
        }
    }
}