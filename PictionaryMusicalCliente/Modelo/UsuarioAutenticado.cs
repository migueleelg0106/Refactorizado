using System;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Modelo
{
    /// <summary>
    /// Representa al usuario autenticado actualmente en el cliente (Singleton).
    /// Mantiene el estado global de la sesion del usuario.
    /// </summary>
    public sealed class UsuarioAutenticado
    {
        private static readonly Lazy<UsuarioAutenticado> _instancia =
            new(() => new UsuarioAutenticado());

        /// <summary>
        /// Obtiene la instancia unica del usuario autenticado.
        /// </summary>
        public static UsuarioAutenticado Instancia => _instancia.Value;

        private UsuarioAutenticado() { }

        /// <summary>
        /// Identificador unico de la cuenta de usuario.
        /// </summary>
        public int IdUsuario { get; private set; }

        /// <summary>
        /// Identificador del perfil de jugador asociado.
        /// </summary>
        public int JugadorId { get; private set; }

        /// <summary>
        /// Nombre de usuario (nickname) unico en el sistema.
        /// </summary>
        public string NombreUsuario { get; private set; }

        /// <summary>
        /// Nombre real del usuario.
        /// </summary>
        public string Nombre { get; private set; }

        /// <summary>
        /// Apellido real del usuario.
        /// </summary>
        public string Apellido { get; private set; }

        /// <summary>
        /// Correo electronico registrado.
        /// </summary>
        public string Correo { get; private set; }

        /// <summary>
        /// Identificador del avatar seleccionado.
        /// </summary>
        public int AvatarId { get; private set; }

        /// <summary>
        /// Ruta relativa de la imagen del avatar (si aplica localmente).
        /// </summary>
        public string AvatarRutaRelativa { get; private set; }

        /// <summary>
        /// Usuario de Instagram.
        /// </summary>
        public string Instagram { get; private set; }

        /// <summary>
        /// Usuario de Facebook.
        /// </summary>
        public string Facebook { get; private set; }

        /// <summary>
        /// Usuario de X (Twitter).
        /// </summary>
        public string X { get; private set; }

        /// <summary>
        /// Usuario de Discord.
        /// </summary>
        public string Discord { get; private set; }

        /// <summary>
        /// Actualiza los datos de la sesion local con la informacion proveniente del servidor.
        /// </summary>
        /// <param name="dto">Objeto de transferencia de datos con la informacion del usuario.
        /// </param>
        public void CargarDesdeDTO(DTOs.UsuarioDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            IdUsuario = dto.UsuarioId;
            JugadorId = dto.JugadorId;
            NombreUsuario = dto.NombreUsuario;
            Nombre = dto.Nombre;
            Apellido = dto.Apellido;
            Correo = dto.Correo;
            AvatarId = dto.AvatarId;
            Instagram = dto.Instagram;
            Facebook = dto.Facebook;
            X = dto.X;
            Discord = dto.Discord;
        }

        /// <summary>
        /// Restablece los valores de la sesion (Cerrar sesion).
        /// </summary>
        public void Limpiar()
        {
            IdUsuario = 0;
            JugadorId = 0;
            NombreUsuario = null;
            Nombre = null;
            Apellido = null;
            Correo = null;
            AvatarId = 0;
            AvatarRutaRelativa = null;
            Instagram = null;
            Facebook = null;
            X = null;
            Discord = null;
        }
    }
}