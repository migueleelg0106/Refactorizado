using Servicios.Contratos;
using System;
using log4net;
using Servicios.Contratos.DTOs;
using Datos.DAL.Implementaciones;
using Datos.Modelo;
using Datos.Utilidades;
using BCryptNet = BCrypt.Net.BCrypt;

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
                    var usuarioRepositorio = new UsuarioRepositorio(contexto);
                    var jugadorRepositorio = new JugadorRepositorio(contexto);
                    var clasificacionRepositorio = new ClasificacionRepositorio(contexto);
                    var avatarRepositorio = new AvatarRepositorio(contexto);

                    bool usuarioRegistrado = usuarioRepositorio.ExisteNombreUsuario(nuevaCuenta.Usuario);
                    bool correoRegistrado = jugadorRepositorio.ExisteCorreo(nuevaCuenta.Correo);

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

                    if (string.IsNullOrWhiteSpace(nuevaCuenta.AvatarRutaRelativa))
                    {
                        return new ResultadoRegistroCuentaDTO
                        {
                            RegistroExitoso = false,
                            Mensaje = "Avatar no válido."
                        };
                    }

                    Avatar avatar = avatarRepositorio.ObtenerAvatarPorRuta(nuevaCuenta.AvatarRutaRelativa);

                    if (avatar == null)
                    {
                        return new ResultadoRegistroCuentaDTO
                        {
                            RegistroExitoso = false,
                            Mensaje = "Avatar no válido."
                        };
                    }

                    var clasificacion = clasificacionRepositorio.CrearClasificacionInicial();

                    var jugador = jugadorRepositorio.CrearJugador(new Jugador
                    {
                        Nombre = nuevaCuenta.Nombre,
                        Apellido = nuevaCuenta.Apellido,
                        Correo = nuevaCuenta.Correo,
                        Avatar_idAvatar = avatar.idAvatar,
                        Clasificacion_idClasificacion = clasificacion.idClasificacion
                    });

                    usuarioRepositorio.CrearUsuario(new Usuario
                    {
                        Nombre_Usuario = nuevaCuenta.Usuario,
                        Contrasena = BCryptNet.HashPassword(nuevaCuenta.Contrasena),
                        Jugador_idJugador = jugador.idJugador
                    });

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
                _logger.Error("Error al registrar la cuenta", ex);
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

        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenvioCodigoVerificacionDTO solicitud)
        {
            return CodigoVerificacionServicio.ReenviarCodigo(solicitud);
        }

        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmacionCodigoDTO confirmacion)
        {
            return CodigoVerificacionServicio.ConfirmarCodigo(confirmacion);
        }
    }
}