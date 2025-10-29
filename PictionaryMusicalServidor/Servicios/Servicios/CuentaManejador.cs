using Servicios.Contratos;
using System;
using log4net;
using Servicios.Contratos.DTOs;
using Datos.DAL.Implementaciones;
using Datos.Modelo;
using Datos.Utilidades;
using BCryptNet = BCrypt.Net.BCrypt;
// Agregamos el using a los nuevos servicios
using Servicios.Servicios;
using System.Linq; // Necesario para la validación

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
                    // --- INICIO REFACTORIZACIÓN (Reducción de CC) ---
                    // 1. Validaciones extraídas a un método privado
                    ResultadoRegistroCuentaDTO validacion = ValidarPrecondicionesRegistro(contexto, nuevaCuenta);
                    if (!validacion.RegistroExitoso)
                    {
                        return validacion;
                    }
                    // --- FIN REFACTORIZACIÓN ---

                    using (var transaccion = contexto.Database.BeginTransaction())
                    {
                        var avatarRepositorio = new AvatarRepositorio(contexto);
                        Avatar avatar = avatarRepositorio.ObtenerAvatarPorRuta(nuevaCuenta.AvatarRutaRelativa);
                        // Esta validación se deja aquí porque requiere el repositorio dentro del contexto
                        if (avatar == null)
                        {
                            return new ResultadoRegistroCuentaDTO
                            {
                                RegistroExitoso = false,
                                Mensaje = "Avatar no válido."
                            };
                        }

                        var clasificacionRepositorio = new ClasificacionRepositorio(contexto);
                        var clasificacion = clasificacionRepositorio.CrearClasificacionInicial();

                        var jugadorRepositorio = new JugadorRepositorio(contexto);
                        var jugador = jugadorRepositorio.CrearJugador(new Jugador
                        {
                            Nombre = nuevaCuenta.Nombre,
                            Apellido = nuevaCuenta.Apellido,
                            Correo = nuevaCuenta.Correo,
                            Avatar_idAvatar = avatar.idAvatar,
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

                        // Limpiamos la verificación SÓLO si todo fue exitoso
                        ServicioVerificacionRegistro.LimpiarVerificacion(nuevaCuenta);

                        return new ResultadoRegistroCuentaDTO
                        {
                            RegistroExitoso = true
                        };
                    }
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

        /// <summary>
        /// NUEVO MÉTODO PRIVADO: Extrae validaciones para reducir CC en RegistrarCuenta.
        /// </summary>
        private ResultadoRegistroCuentaDTO ValidarPrecondicionesRegistro(BaseDatosPruebaEntities1 contexto, NuevaCuentaDTO nuevaCuenta)
        {
            // 1. Validar que el código fue confirmado
            if (!ServicioVerificacionRegistro.EstaVerificacionConfirmada(nuevaCuenta))
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = "La cuenta no ha sido verificada."
                };
            }

            // 2. Validar existencia de usuario y correo
            // (Optimizamos esto para hacer una sola consulta si es posible, pero lo mantenemos como estaba)
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

            // 3. Validar Avatar (Validación básica, la existencia se valida después)
            if (string.IsNullOrWhiteSpace(nuevaCuenta.AvatarRutaRelativa))
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = "Avatar no válido."
                };
            }

            return new ResultadoRegistroCuentaDTO { RegistroExitoso = true }; // Pasa todas las validaciones
        }


        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }

        // DELEGA A: ServicioVerificacionRegistro
        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            return ServicioVerificacionRegistro.SolicitarCodigo(nuevaCuenta);
        }

        // DELEGA A: ServicioVerificacionRegistro
        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenvioCodigoVerificacionDTO solicitud)
        {
            return ServicioVerificacionRegistro.ReenviarCodigo(solicitud);
        }

        // DELEGA A: ServicioVerificacionRegistro
        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmacionCodigoDTO confirmacion)
        {
            return ServicioVerificacionRegistro.ConfirmarCodigo(confirmacion);
        }
    }
}