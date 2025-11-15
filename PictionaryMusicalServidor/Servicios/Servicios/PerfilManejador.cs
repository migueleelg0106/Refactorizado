using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Datos.Utilidades;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using log4net;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    public class PerfilManejador : IPerfilManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PerfilManejador));

        public UsuarioDTO ObtenerPerfil(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                throw new FaultException(MensajesError.Cliente.DatosInvalidos);
            }

            try
            {
                using (BaseDatosPruebaEntities1 contexto = CrearContexto())
                {
                    Usuario usuario = contexto.Usuario
                        .Include(u => u.Jugador.RedSocial)
                        .FirstOrDefault(u => u.idUsuario == idUsuario);

                    if (usuario == null)
                    {
                        throw new FaultException(MensajesError.Cliente.UsuarioNoEncontrado);
                    }

                    Jugador jugador = usuario.Jugador;

                    if (jugador == null)
                    {
                        throw new FaultException(MensajesError.Cliente.JugadorNoAsociado);
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
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.PerfilObtenerOperacionInvalida, ex);
                throw new FaultException(MensajesError.Cliente.ErrorObtenerPerfil);
            }
        }

        public ResultadoOperacionDTO ActualizarPerfil(ActualizacionPerfilDTO solicitud)
        {

            ResultadoOperacionDTO validacion = PerfilValidador.ValidarActualizacion(solicitud);
            if (!validacion.OperacionExitosa)
            {
                return validacion;
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
                        return CrearResultadoFallo(MensajesError.Cliente.UsuarioNoEncontrado);
                    }

                    Jugador jugador = usuario.Jugador;

                    if (jugador == null)
                    {
                        return CrearResultadoFallo(MensajesError.Cliente.JugadorNoAsociado);
                    }

                    jugador.Nombre = solicitud.Nombre.Trim();
                    jugador.Apellido = solicitud.Apellido.Trim();
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

                    redSocial.Instagram = NormalizarRedSocial(solicitud.Instagram);
                    redSocial.facebook = NormalizarRedSocial(solicitud.Facebook);
                    redSocial.x = NormalizarRedSocial(solicitud.X);
                    redSocial.discord = NormalizarRedSocial(solicitud.Discord);

                    contexto.SaveChanges();

                    return new ResultadoOperacionDTO
                    {
                        OperacionExitosa = true,
                        Mensaje = MensajesError.Cliente.PerfilActualizadoExito
                    };
                }
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
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.PerfilActualizarOperacionInvalida, ex);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
        }

        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
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
    }
}