using System;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel;
using Datos.DAL.Implementaciones;
using Datos.Modelo;
using Datos.Utilidades;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using log4net;

namespace Servicios.Servicios
{
    public class PerfilManejador : IPerfilManejador
    {
        private const int LongitudMaximaRedSocial = 50;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PerfilManejador));

        public UsuarioDTO ObtenerPerfil(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                throw new FaultException("Los datos proporcionados no son válidos para obtener el perfil.");
            }

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    Usuario usuario = contexto.Usuario
                        .Include(u => u.Jugador.Avatar)
                        .Include(u => u.Jugador.RedSocial)
                        .FirstOrDefault(u => u.idUsuario == idUsuario);

                    if (usuario == null)
                    {
                        throw new FaultException("No se encontró el usuario especificado.");
                    }

                    Jugador jugador = usuario.Jugador;

                    if (jugador == null)
                    {
                        throw new FaultException("No existe un jugador asociado al usuario especificado.");
                    }

                    RedSocial redSocial = jugador.RedSocial.FirstOrDefault();

                    return new UsuarioDTO
                    {
                        UsuarioId = usuario.idUsuario,
                        JugadorId = jugador.idJugador,
                        NombreUsuario = usuario.Nombre_Usuario,
                        Nombre = jugador.Nombre,
                        Apellido = jugador.Apellido,
                        Correo = jugador.Correo,
                        AvatarId = jugador.Avatar_idAvatar,
                        AvatarRutaRelativa = jugador.Avatar?.Avatar_Ruta,
                        Instagram = redSocial?.Instagram,
                        Facebook = redSocial?.facebook,
                        X = redSocial?.x,
                        Discord = redSocial?.discord
                    };
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error("Error al obtener el perfil del usuario", ex);
                throw new FaultException("Ocurrió un problema al consultar la información del perfil.");
            }
        }

        public ResultadoOperacionDTO ActualizarPerfil(ActualizacionPerfilDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new FaultException("La solicitud de actualización es obligatoria.");
            }

            if (solicitud.UsuarioId <= 0)
            {
                return CrearResultadoFallo("El identificador de usuario es inválido.");
            }

            string nombre = solicitud.Nombre?.Trim();
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 50)
            {
                return CrearResultadoFallo("El nombre es obligatorio y no debe exceder 50 caracteres.");
            }

            string apellido = solicitud.Apellido?.Trim();
            if (string.IsNullOrWhiteSpace(apellido) || apellido.Length > 50)
            {
                return CrearResultadoFallo("El apellido es obligatorio y no debe exceder 50 caracteres.");
            }

            string rutaAvatar = solicitud.AvatarRutaRelativa?.Trim();
            if (string.IsNullOrWhiteSpace(rutaAvatar))
            {
                return CrearResultadoFallo("Selecciona un avatar válido.");
            }

            ResultadoOperacionDTO validacionRedes = ValidarRedesSociales(solicitud);
            if (!validacionRedes.OperacionExitosa)
            {
                return validacionRedes;
            }

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    Usuario usuario = contexto.Usuario
                        .Include(u => u.Jugador.RedSocial)
                        .FirstOrDefault(u => u.idUsuario == solicitud.UsuarioId);

                    if (usuario == null)
                    {
                        return CrearResultadoFallo("No se encontró el usuario especificado.");
                    }

                    Jugador jugador = usuario.Jugador;

                    if (jugador == null)
                    {
                        return CrearResultadoFallo("No existe un jugador asociado al usuario especificado.");
                    }

                    var avatarRepositorio = new AvatarRepositorio(contexto);
                    Avatar avatar = avatarRepositorio.ObtenerAvatarPorRuta(rutaAvatar);
                    if (avatar == null)
                    {
                        return CrearResultadoFallo("El avatar seleccionado no existe.");
                    }

                    jugador.Nombre = nombre;
                    jugador.Apellido = apellido;
                    jugador.Avatar_idAvatar = avatar.idAvatar;

                    RedSocial redSocial = jugador.RedSocial.FirstOrDefault();
                    if (redSocial == null)
                    {
                        redSocial = new RedSocial
                        {
                            Jugador_idJugador = jugador.idJugador
                        };
                        contexto.RedSocial.Add(redSocial);
                        jugador.RedSocial.Add(redSocial);
                    }

                    redSocial.Instagram = NormalizarRedSocial(solicitud.Instagram);
                    redSocial.facebook = NormalizarRedSocial(solicitud.Facebook);
                    redSocial.x = NormalizarRedSocial(solicitud.X);
                    redSocial.discord = NormalizarRedSocial(solicitud.Discord);

                    contexto.SaveChanges();

                    return new ResultadoOperacionDTO
                    {
                        OperacionExitosa = true,
                        Mensaje = "Perfil actualizado correctamente."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error al actualizar el perfil", ex);
                return CrearResultadoFallo("No fue posible actualizar el perfil.");
            }
        }

        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }

        private static ResultadoOperacionDTO ValidarRedesSociales(ActualizacionPerfilDTO solicitud)
        {
            ResultadoOperacionDTO resultado = ValidarRedSocial("Instagram", solicitud.Instagram);
            if (!resultado.OperacionExitosa)
            {
                return resultado;
            }

            resultado = ValidarRedSocial("Facebook", solicitud.Facebook);
            if (!resultado.OperacionExitosa)
            {
                return resultado;
            }

            resultado = ValidarRedSocial("X", solicitud.X);
            if (!resultado.OperacionExitosa)
            {
                return resultado;
            }

            return ValidarRedSocial("Discord", solicitud.Discord);
        }

        private static ResultadoOperacionDTO ValidarRedSocial(string nombre, string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return ResultadoOperacionExitoso();
            }

            string normalizado = valor.Trim();
            if (normalizado.Length > LongitudMaximaRedSocial)
            {
                return CrearResultadoFallo(
                    $"El identificador de {nombre} no debe exceder {LongitudMaximaRedSocial} caracteres.");
            }

            return ResultadoOperacionExitoso();
        }

        private static string NormalizarRedSocial(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            string normalizado = valor.Trim();
            return normalizado.Length == 0 ? null : normalizado;
        }

        private static ResultadoOperacionDTO CrearResultadoFallo(string mensaje)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = mensaje
            };
        }

        private static ResultadoOperacionDTO ResultadoOperacionExitoso()
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = true
            };
        }
    }
}