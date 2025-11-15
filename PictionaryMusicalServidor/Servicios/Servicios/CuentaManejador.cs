using Servicios.Contratos;
using System;
using log4net;
using Servicios.Contratos.DTOs;
using Datos.DAL.Implementaciones;
using Datos.Modelo;
using Datos.Utilidades;
using BCryptNet = BCrypt.Net.BCrypt;
using System.Linq;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using Servicios.Servicios.Constantes; 

namespace Servicios.Servicios
{
    public class CuentaManejador : ICuentaManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CuentaManejador));

        public ResultadoRegistroCuentaDTO RegistrarCuenta(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                throw new ArgumentNullException(nameof(nuevaCuenta));
            }

            try
            {
                using (var contexto = CrearContexto())
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
                _logger.Error("Validación de entidad fallida al registrar la cuenta", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.ErrorRegistrarCuenta
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.Error("Error de actualización de base de datos al registrar la cuenta", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.ErrorRegistrarCuenta
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al registrar la cuenta", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.ErrorRegistrarCuenta
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al registrar la cuenta", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.ErrorRegistrarCuenta
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operación inválida al registrar la cuenta", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.ErrorRegistrarCuenta
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
                    Mensaje = "La cuenta no ha sido verificada."
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
                    Mensaje = "Avatar no válido."
                };
            }

            return new ResultadoRegistroCuentaDTO { RegistroExitoso = true }; 
        }


        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }

        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            return ServicioVerificacionRegistro.SolicitarCodigo(nuevaCuenta);
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenvioCodigoVerificacionDTO solicitud)
        {
            return ServicioVerificacionRegistro.ReenviarCodigo(solicitud);
        }

        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmacionCodigoDTO confirmacion)
        {
            return ServicioVerificacionRegistro.ConfirmarCodigo(confirmacion);
        }
    }
}