using Datos.Modelo;
using log4net;
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
        private readonly IContextoFactoria _contextoFactoria;
        private readonly IRepositorioFactoria _repositorioFactoria;

        /// <summary>
        /// Constructor por defecto para uso en WCF.
        /// </summary>
        public PerfilManejador() : this(new ContextoFactoria(), new RepositorioFactoria())
        {
        }

        /// <summary>
        /// Constructor con inyeccion de dependencias para pruebas unitarias.
        /// </summary>
        /// <param name="contextoFactoria">Factoria para crear contextos de base de datos.</param>
        /// <param name="repositorioFactoria">Factoria para crear repositorios.</param>
        public PerfilManejador(
            IContextoFactoria contextoFactoria,
            IRepositorioFactoria repositorioFactoria)
        {
            _contextoFactoria = contextoFactoria ??
                throw new ArgumentNullException(nameof(contextoFactoria));
            _repositorioFactoria = repositorioFactoria ??
                throw new ArgumentNullException(nameof(repositorioFactoria));
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

                using (var contexto = _contextoFactoria.CrearContexto())
                {
                    var usuario = ObtenerUsuarioConRelaciones(contexto, idUsuario);
                    return ConstruirPerfilDTO(usuario);
                }
            }
            catch (ArgumentException excepcion)
            {
                _logger.Warn("Operacion invalida al obtener perfil.", excepcion);
                throw new FaultException(excepcion.Message);
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Warn("Operacion invalida al obtener perfil.", excepcion);
                throw new FaultException(excepcion.Message);
            }
            catch (DbUpdateException excepcion)
            {
                _logger.Error("Error de actualizacion al obtener perfil.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorObtenerPerfil);
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error de base de datos al obtener perfil.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorObtenerPerfil);
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error de datos al obtener perfil.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorObtenerPerfil);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error de datos al obtener perfil.", excepcion);
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
            catch (ArgumentException excepcion)
            {
                _logger.Warn("Operacion invalida al actualizar perfil.", excepcion);
                return CrearResultadoFallo(excepcion.Message);
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Warn("Operacion invalida al actualizar perfil.", excepcion);
                return CrearResultadoFallo(excepcion.Message);
            }
            catch (DbEntityValidationException excepcion)
            {
                _logger.Error("Validacion de entidad fallida al actualizar perfil.", excepcion);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
            catch (DbUpdateConcurrencyException excepcion)
            {
                _logger.Error("Error de concurrencia al actualizar perfil.", excepcion);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
            catch (DbUpdateException excepcion)
            {
                _logger.Error("Error de actualizacion de BD al actualizar perfil.", excepcion);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error de base de datos al actualizar perfil.", excepcion);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error de datos al actualizar perfil.", excepcion);
                return CrearResultadoFallo(MensajesError.Cliente.ErrorActualizarPerfil);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error de datos al actualizar perfil.", excepcion);
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
            IUsuarioRepositorio repositorio = 
                _repositorioFactoria.CrearUsuarioRepositorio(contexto);
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
            using (var contexto = _contextoFactoria.CrearContexto())
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
