using Datos.Modelo;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using BCryptNet = BCrypt.Net.BCrypt;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de autenticacion de usuarios.
    /// Valida credenciales comparando identificador y contrasena con hash BCrypt.
    /// </summary>
    public class InicioSesionManejador : IInicioSesionManejador
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(InicioSesionManejador));

        private const int LimiteReportesParaBaneo = 3;

        private readonly IContextoFactoria _contextoFactory;

        public InicioSesionManejador() : this(new ContextoFactoria())
        {
        }

        public InicioSesionManejador(IContextoFactoria contextoFactory)
        {
            _contextoFactory = contextoFactory ??
                throw new ArgumentNullException(nameof(contextoFactory));
        }

        /// <summary>
        /// Inicia sesion de un usuario validando sus credenciales.
        /// </summary>
        /// <param name="credenciales">Credenciales de inicio de sesion del usuario.</param>
        /// <returns>Resultado del inicio de sesion con datos del usuario si es exitoso.</returns>
        public ResultadoInicioSesionDTO IniciarSesion(
            CredencialesInicioSesionDTO credenciales)
        {
            if (credenciales == null)
            {
                throw new ArgumentNullException(nameof(credenciales));
            }

            if (!SonCredencialesValidas(credenciales))
            {
                _logger.Warn("Intento de inicio de sesion con formato de datos invalido.");
                return CrearRespuestaDatosInvalidos();
            }

            try
            {
                using (var contexto = _contextoFactory.CrearContexto())
                {
                    return ProcesarAutenticacion(contexto, credenciales);
                }
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error de base de datos durante el inicio de sesion.", excepcion);
                return CrearErrorGenerico();
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error de datos durante el inicio de sesion.", excepcion);
                return CrearErrorGenerico();
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Error("Operacion invalida durante el inicio de sesion.", excepcion);
                return CrearErrorGenerico();
            }
            catch (Exception excepcion)
            {
                _logger.Error("Operacion invalida durante el inicio de sesion.", excepcion);
                return CrearErrorGenerico();
            }
        }

        private bool SonCredencialesValidas(CredencialesInicioSesionDTO credenciales)
        {
            string identificador = EntradaComunValidador.NormalizarTexto(
                credenciales.Identificador);

            string contrasena = credenciales.Contrasena?.Trim();

            if (!EntradaComunValidador.EsLongitudValida(identificador))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                return false;
            }

            return true;
        }

        private ResultadoInicioSesionDTO CrearRespuestaDatosInvalidos()
        {
            return new ResultadoInicioSesionDTO
            {
                CuentaEncontrada = true,
                Mensaje = MensajesError.Cliente.CredencialesInvalidas
            };
        }

        private ResultadoInicioSesionDTO ProcesarAutenticacion(
            BaseDatosPruebaEntities contexto,
            CredencialesInicioSesionDTO credenciales)
        {
            string identificador = EntradaComunValidador.NormalizarTexto(
                credenciales.Identificador);

            Usuario usuario;
            try
            {
                usuario = ObtenerUsuarioPorCredencial(contexto, identificador);
            }
            catch
            {
                _logger.Warn("Inicio de sesion fallido. Usuario no encontrado.");
                return new ResultadoInicioSesionDTO
                {
                    CuentaEncontrada = false,
                    Mensaje = MensajesError.Cliente.CredencialesIncorrectas
                };
            }

            if (!VerificarContrasena(credenciales.Contrasena, usuario.Contrasena))
            {
                _logger.Warn("Inicio de sesion fallido. Contrasena incorrecta.");

                return new ResultadoInicioSesionDTO
                {
                    ContrasenaIncorrecta = true,
                    Mensaje = MensajesError.Cliente.CredencialesIncorrectas
                };
            }

            if (UsuarioAlcanzoLimiteReportes(contexto, usuario.idUsuario))
            {
                _logger.WarnFormat(
                    "Usuario con id {0} bloqueado por superar limite de reportes.",
                    usuario.idUsuario);

                return new ResultadoInicioSesionDTO
                {
                    InicioSesionExitoso = false,
                    CuentaEncontrada = true,
                    Mensaje = MensajesError.Cliente.UsuarioBaneadoPorReportes
                };
            }

            _logger.InfoFormat(
                "Inicio de sesion exitoso para usuario con id {0}.",
                usuario.idUsuario);

            return new ResultadoInicioSesionDTO
            {
                InicioSesionExitoso = true,
                Usuario = MapearUsuario(usuario)
            };
        }

        private bool VerificarContrasena(string contrasenaEntrada, string hashAlmacenado)
        {
            if (string.IsNullOrWhiteSpace(contrasenaEntrada) ||
                string.IsNullOrWhiteSpace(hashAlmacenado))
            {
                return false;
            }

            return BCryptNet.Verify(contrasenaEntrada.Trim(), hashAlmacenado);
        }

        private bool UsuarioAlcanzoLimiteReportes(BaseDatosPruebaEntities contexto, int usuarioId)
        {
            IReporteRepositorio reporteRepositorio = new ReporteRepositorio(contexto);
            int totalReportes = reporteRepositorio.ContarReportesRecibidos(usuarioId);

            return totalReportes >= LimiteReportesParaBaneo;
        }

        private static ResultadoInicioSesionDTO CrearErrorGenerico()
        {
            return new ResultadoInicioSesionDTO
            {
                InicioSesionExitoso = false,
                Mensaje = MensajesError.Cliente.ErrorInicioSesion
            };
        }

        private Usuario ObtenerUsuarioPorCredencial(
            BaseDatosPruebaEntities contexto,
            string identificador)
        {
            try
            {
                return BuscarPorNombreUsuario(contexto, identificador);
            }
            catch (KeyNotFoundException excepcion)
            {
                _logger.Warn(
                    "Usuario no encontrado por nombre, se intentara buscar por correo.",
                    excepcion);
                return BuscarPorCorreoElectronico(contexto, identificador);
            }
            catch (Exception excepcion)
            {
                _logger.Error(
                    "Error inesperado al buscar usuario por nombre, intentando por correo.",
                    excepcion);
                return BuscarPorCorreoElectronico(contexto, identificador);
            }
        }

        private Usuario BuscarPorNombreUsuario(
            BaseDatosPruebaEntities contexto,
            string nombreUsuario)
        {
            IUsuarioRepositorio repositorio = new UsuarioRepositorio(contexto);
            return repositorio.ObtenerPorNombreUsuario(nombreUsuario);
        }

        private Usuario BuscarPorCorreoElectronico(
            BaseDatosPruebaEntities contexto,
            string correo)
        {
            IUsuarioRepositorio repositorio = new UsuarioRepositorio(contexto);
            var usuario = repositorio.ObtenerPorCorreo(correo);

            if (usuario == null)
            {
                throw new KeyNotFoundException("Usuario no encontrado por correo.");
            }
            return usuario;
        }

        private static UsuarioDTO MapearUsuario(Usuario usuario)
        {
            Jugador jugador = usuario.Jugador;

            return new UsuarioDTO
            {
                UsuarioId = usuario.idUsuario,
                JugadorId = jugador?.idJugador ?? 0,
                NombreUsuario = usuario.Nombre_Usuario,
                Nombre = jugador?.Nombre,
                Apellido = jugador?.Apellido,
                Correo = jugador?.Correo,
                AvatarId = jugador?.Id_Avatar ?? 0,
            };
        }
    }
}
