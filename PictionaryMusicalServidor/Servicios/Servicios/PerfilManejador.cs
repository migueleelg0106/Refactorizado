using Datos.Modelo;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de gestion de perfiles de usuario.
    /// Maneja consulta y actualizacion de datos de perfil incluyendo informacion personal y
    /// redes sociales.
    /// </summary>
    public class PerfilManejador : IPerfilManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PerfilManejador));
        private readonly IContextoFactoria _contextoFactory;

        public PerfilManejador() : this(new ContextoFactoria())
        {
        }

        public PerfilManejador(IContextoFactoria contextoFactory)
        {
            _contextoFactory = contextoFactory ??
                throw new ArgumentNullException(nameof(contextoFactory));
        }

        /// <summary>
        /// Obtiene el perfil completo de un usuario incluyendo datos de jugador y redes sociales.
        /// Valida que el usuario exista y tenga un jugador asociado.
        /// </summary>
        public UsuarioDTO ObtenerPerfil(int idUsuario)
        {
            try
            {
                ValidarIdUsuario(idUsuario);

                using (var contexto = _contextoFactory.CrearContexto())
                {
                    var usuario = ObtenerUsuarioConRelaciones(contexto, idUsuario);
                    return ConstruirPerfilDTO(usuario);
                }
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Operacion invalida al obtener perfil.", ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Operacion invalida al obtener perfil.", ex);
                throw new FaultException(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                _logger.Error("Error de actualizacion al obtener perfil.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorObtenerPerfil);
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al obtener perfil.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorObtenerPerfil);
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al obtener perfil.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorObtenerPerfil);
            }
        }

        /// <summary>
        /// Actualiza el perfil de un usuario con nuevos datos personales y de redes sociales.
        /// Valida los datos de entrada, verifica que el usuario exista y actualiza jugador.
        /// </summary>
        public ResultadoOperacionDTO ActualizarPerfil(ActualizacionPerfilDTO solicitud)
        {
            try
            {
                var validacion = EntradaComunValidador.ValidarActualizacionPerfil(solicitud);
                if (!validacion.OperacionExitosa)
                {
                    return validacion;
                }

                EjecutarActualizacionEnBD(solicitud);

                _logger.Info("Perfil actualizado exitosamente.");

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = true,
                    Mensaje = MensajesError.Cliente.PerfilActualizadoExito
                };
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Operacion invalida al actualizar perfil.", ex);
                return CrearResultadoFallo(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Operacion invalida al actualizar perfil.", ex);
                return CrearResultadoFallo(ex.Message);
            }
            catch (DbEntityValidationException ex)
            {
                _logger.Error("Validacion de entidad fallida al actualizar perfil.", ex);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.Error("Error de concurrencia al actualizar perfil.", ex);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
            catch (DbUpdateException ex)
            {
                _logger.Error("Error de actualizacion de BD al actualizar perfil.", ex);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al actualizar perfil.", ex);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al actualizar perfil.", ex);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
        }

        private void ValidarIdUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                throw new ArgumentException(MensajesError.Cliente.DatosInvalidos);
            }
        }

        private Usuario ObtenerUsuarioConRelaciones(
            BaseDatosPruebaEntities contexto,
            int idUsuario)
        {
            IUsuarioRepositorio repositorio = new UsuarioRepositorio(contexto);
            var usuario = repositorio.ObtenerPorIdConRedesSociales(idUsuario);

            if (usuario == null)
            {
                throw new InvalidOperationException(
                    MensajesError.Cliente.UsuarioNoEncontrado);
            }

            if (usuario.Jugador == null)
            {
                throw new InvalidOperationException(
                    MensajesError.Cliente.JugadorNoAsociado);
            }

            return usuario;
        }

        private UsuarioDTO ConstruirPerfilDTO(Usuario usuario)
        {
            var jugador = usuario.Jugador;
            var redSocial = jugador.RedSocial.FirstOrDefault();

            return new UsuarioDTO
            {
                UsuarioId = usuario.idUsuario,
                JugadorId = jugador.idJugador,
                NombreUsuario = usuario.Nombre_Usuario,
                Nombre = jugador.Nombre,
                Apellido = jugador.Apellido,
                Correo = jugador.Correo,
                AvatarId = jugador.Id_Avatar ?? 0,
                Instagram = redSocial?.Instagram,
                Facebook = redSocial?.facebook,
                X = redSocial?.x,
                Discord = redSocial?.discord
            };
        }

        private void EjecutarActualizacionEnBD(ActualizacionPerfilDTO solicitud)
        {
            using (var contexto = _contextoFactory.CrearContexto())
            {
                var usuario = ObtenerUsuarioConRelaciones(contexto, solicitud.UsuarioId);
                var jugador = usuario.Jugador;

                ActualizarDatosPersonales(jugador, solicitud);
                ProcesarActualizacionRedesSociales(contexto, jugador, solicitud);

                contexto.SaveChanges();
            }
        }

        private void ActualizarDatosPersonales(
            Jugador jugador,
            ActualizacionPerfilDTO solicitud)
        {
            jugador.Nombre = solicitud.Nombre;
            jugador.Apellido = solicitud.Apellido;
            jugador.Id_Avatar = solicitud.AvatarId;
        }

        private void ProcesarActualizacionRedesSociales(
            BaseDatosPruebaEntities contexto,
            Jugador jugador,
            ActualizacionPerfilDTO solicitud)
        {
            var redSocial = jugador.RedSocial.FirstOrDefault();
            bool esNueva = false;

            if (redSocial == null)
            {
                redSocial = CrearRedSocialVacia(jugador.idJugador);
                esNueva = true;
            }

            AsignarValoresRedSocial(redSocial, solicitud);

            if (esNueva)
            {
                contexto.RedSocial.Add(redSocial);
                jugador.RedSocial.Add(redSocial);
            }
        }

        private RedSocial CrearRedSocialVacia(int jugadorId)
        {
            return new RedSocial
            {
                Jugador_idJugador = jugadorId
            };
        }

        private void AsignarValoresRedSocial(
            RedSocial redSocial,
            ActualizacionPerfilDTO solicitud)
        {
            redSocial.Instagram = solicitud.Instagram;
            redSocial.facebook = solicitud.Facebook;
            redSocial.x = solicitud.X;
            redSocial.discord = solicitud.Discord;
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