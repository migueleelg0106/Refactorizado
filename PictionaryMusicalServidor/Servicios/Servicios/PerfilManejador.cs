using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using log4net;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de gestion de perfiles de usuario.
    /// Valida los identificadores, recupera la informacion del perfil y actualiza los datos del usuario.
    /// </summary>
    public class PerfilManejador : IPerfilManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PerfilManejador));

        /// <summary>
        /// Obtiene el perfil completo de un usuario incluyendo redes sociales.
        /// Valida que el ID sea mayor a 0 y que el usuario exista.
        /// </summary>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <returns>Datos del perfil del usuario.</returns>
        /// <exception cref="ArgumentException">Se lanza si el ID del usuario es invalido.</exception>
        /// <exception cref="InvalidOperationException">Se lanza si el usuario no existe.</exception>
        /// <exception cref="EntityException">Se captura cuando hay errores de conexion con la base de datos.</exception>
        /// <exception cref="DataException">Se captura cuando hay errores relacionados con los datos.</exception>
        public UsuarioDTO ObtenerPerfil(int idUsuario)
        {
            try
            {
                if (idUsuario <= 0)
                {
                    throw new ArgumentException(MensajesError.Cliente.DatosInvalidos);
                }

                using (BaseDatosPruebaEntities1 contexto = ContextoFactory.CrearContexto())
                {
                    Usuario usuario = contexto.Usuario
                        .Include(u => u.Jugador.RedSocial)
                        .FirstOrDefault(u => u.idUsuario == idUsuario);

                    if (usuario == null)
                    {
                        throw new InvalidOperationException(MensajesError.Cliente.UsuarioNoEncontrado);
                    }

                    Jugador jugador = usuario.Jugador;

                    if (jugador == null)
                    {
                        throw new InvalidOperationException(MensajesError.Cliente.JugadorNoAsociado);
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
                        AvatarId = jugador.Id_Avatar,
                        Instagram = redSocial?.Instagram,
                        Facebook = redSocial?.facebook,
                        X = redSocial?.x,
                        Discord = redSocial?.discord
                    };
                }
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.PerfilObtenerOperacionInvalida, ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.PerfilObtenerOperacionInvalida, ex);
                throw new FaultException(ex.Message);
            }
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.PerfilObtenerErrorBD, ex);
                throw new FaultException(MensajesError.Cliente.ErrorObtenerPerfil);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.PerfilObtenerErrorDatos, ex);
                throw new FaultException(MensajesError.Cliente.ErrorObtenerPerfil);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.PerfilObtenerOperacionInvalida, ex);
                throw new FaultException(MensajesError.Cliente.ErrorObtenerPerfil);
            }
        }

        /// <summary>
        /// Actualiza la informacion del perfil de un usuario.
        /// Valida los datos de entrada, verifica que el usuario exista y actualiza el jugador y redes sociales.
        /// </summary>
        /// <param name="solicitud">Datos actualizados del perfil.</param>
        /// <returns>Resultado de la actualizacion del perfil.</returns>
        /// <exception cref="DbEntityValidationException">Se captura cuando hay errores de validacion de entidades.</exception>
        /// <exception cref="DbUpdateException">Se captura cuando hay errores al actualizar la base de datos.</exception>
        /// <exception cref="EntityException">Se captura cuando hay errores de conexion con la base de datos.</exception>
        /// <exception cref="DataException">Se captura cuando hay errores relacionados con los datos.</exception>
        /// <exception cref="InvalidOperationException">Se captura cuando hay operaciones invalidas.</exception>
        public ResultadoOperacionDTO ActualizarPerfil(ActualizacionPerfilDTO solicitud)
        {
            try
            {
                ResultadoOperacionDTO validacion = EntradaComunValidador.ValidarActualizacionPerfil(solicitud);
                if (!validacion.OperacionExitosa)
                {
                    return validacion;
                }

                using (BaseDatosPruebaEntities1 contexto = ContextoFactory.CrearContexto())
                {
                    Usuario usuario = contexto.Usuario
                        .Include(u => u.Jugador.RedSocial)
                        .FirstOrDefault(u => u.idUsuario == solicitud.UsuarioId);


                    if (usuario == null)
                    {
                        throw new InvalidOperationException(MensajesError.Cliente.UsuarioNoEncontrado);
                    }

                    Jugador jugador = usuario.Jugador;

                    if (jugador == null)
                    {
                        throw new InvalidOperationException(MensajesError.Cliente.JugadorNoAsociado);
                    }

                    jugador.Nombre = solicitud.Nombre;
                    jugador.Apellido = solicitud.Apellido;
                    jugador.Id_Avatar = solicitud.AvatarId;

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

                    redSocial.Instagram = solicitud.Instagram;
                    redSocial.facebook = solicitud.Facebook;
                    redSocial.x = solicitud.X;
                    redSocial.discord = solicitud.Discord;

                    contexto.SaveChanges();

                    return new ResultadoOperacionDTO
                    {
                        OperacionExitosa = true,
                        Mensaje = MensajesError.Cliente.PerfilActualizadoExito
                    };
                }
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.PerfilActualizarOperacionInvalida, ex);
                return CrearResultadoFallo(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.PerfilActualizarOperacionInvalida, ex);
                return CrearResultadoFallo(ex.Message);
            }
            catch (DbEntityValidationException ex)
            {
                _logger.Error(MensajesError.Log.PerfilActualizarValidacionEntidad, ex);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
            catch (DbUpdateException ex)
            {
                _logger.Error(MensajesError.Log.PerfilActualizarActualizacionBD, ex);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.PerfilActualizarErrorBD, ex);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.PerfilActualizarErrorDatos, ex);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.PerfilActualizarOperacionInvalida, ex);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
        }

        private static ResultadoOperacionDTO CrearResultadoFallo(string mensaje)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = mensaje
            };
        }
    }
}