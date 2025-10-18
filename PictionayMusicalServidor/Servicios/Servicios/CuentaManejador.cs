using Servicios.Contratos;
using System;
using System.Linq;
using log4net;
using Servicios.Contratos.DTOs;
using Datos.Modelo;
using Datos.Utilidades;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Servicios.Servicios
{
    public class CuentaManejador : ICuentaManejador
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CuentaManejador));

        public ResultadoRegistroCuentaDTO RegistrarCuenta(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                throw new ArgumentNullException(nameof(nuevaCuenta));
            }

            if (!CodigoVerificacionServicio.EstaVerificacionConfirmada(nuevaCuenta))
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = "La cuenta no ha sido verificada."
                };
            }

            try
            {
                using (var contexto = CrearContexto())
                using (var transaccion = contexto.Database.BeginTransaction())
                {
                    bool usuarioRegistrado = contexto.Usuario.Any(u => u.Nombre_Usuario == nuevaCuenta.Usuario);
                    bool correoRegistrado = contexto.Jugador.Any(j => j.Correo == nuevaCuenta.Correo);

                    if (usuarioRegistrado || correoRegistrado)
                    {
                        return new ResultadoRegistroCuentaDTO
                        {
                            RegistroExitoso = false,
                            UsuarioYaRegistrado = usuarioRegistrado,
                            CorreoYaRegistrado = correoRegistrado,
                            Mensaje = "Usuario o correo ya registrados."
                        };
                    }

                    if (!contexto.Avatar.Any(a => a.idAvatar == nuevaCuenta.AvatarId))
                    {
                        return new ResultadoRegistroCuentaDTO
                        {
                            RegistroExitoso = false,
                            Mensaje = "Avatar no válido."
                        };
                    }

                    var clasificacion = new Clasificacion
                    {
                        Puntos_Ganados = 0,
                        Rondas_Ganadas = 0
                    };
                    contexto.Clasificacion.Add(clasificacion);
                    contexto.SaveChanges();

                    var jugador = new Jugador
                    {
                        Nombre = nuevaCuenta.Nombre,
                        Apellido = nuevaCuenta.Apellido,
                        Correo = nuevaCuenta.Correo,
                        Avatar_idAvatar = nuevaCuenta.AvatarId,
                        Clasificacion_idClasificacion = clasificacion.idClasificacion
                    };
                    contexto.Jugador.Add(jugador);
                    contexto.SaveChanges();

                    var usuario = new Usuario
                    {
                        Nombre_Usuario = nuevaCuenta.Usuario,
                        Contrasena = BCryptNet.HashPassword(nuevaCuenta.Contrasena),
                        Jugador_idJugador = jugador.idJugador
                    };
                    contexto.Usuario.Add(usuario);
                    contexto.SaveChanges();

                    transaccion.Commit();

                    CodigoVerificacionServicio.LimpiarVerificacion(nuevaCuenta);

                    return new ResultadoRegistroCuentaDTO
                    {
                        RegistroExitoso = true
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error al registrar la cuenta", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = ex.Message
                };
            }
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
            return CodigoVerificacionServicio.SolicitarCodigo(nuevaCuenta);
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenviarCodigoVerificacionDTO solicitud)
        {
            return CodigoVerificacionServicio.ReenviarCodigo(solicitud);
        }

        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmarCodigoDTO confirmacion)
        {
            return CodigoVerificacionServicio.ConfirmarCodigo(confirmacion);
        }
    }
}
