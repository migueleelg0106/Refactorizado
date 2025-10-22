using System;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Modelo
{
    /// <summary>
    /// Representa al usuario autenticado actualmente en el cliente.
    /// </summary>
    public sealed class UsuarioAutenticado
    {
        private static readonly Lazy<UsuarioAutenticado> _instancia =
            new(() => new UsuarioAutenticado());

        public static UsuarioAutenticado Instancia => _instancia.Value;

        private UsuarioAutenticado() { }

        public int IdUsuario { get; private set; }
        public int JugadorId { get; private set; }
        public string NombreUsuario { get; private set; }
        public string Nombre { get; private set; }
        public string Apellido { get; private set; }
        public string Correo { get; private set; }
        public int AvatarId { get; private set; }
        public string AvatarRutaRelativa { get; private set; }
        public string Instagram { get; private set; }
        public string Facebook { get; private set; }
        public string X { get; private set; }
        public string Discord { get; private set; }

        public void CargarDesdeDTO(DTOs.UsuarioDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            IdUsuario = dto.IdUsuario;
            JugadorId = dto.JugadorId;
            NombreUsuario = dto.NombreUsuario;
            Nombre = dto.Nombre;
            Apellido = dto.Apellido;
            Correo = dto.Correo;
            AvatarId = dto.AvatarId;
            AvatarRutaRelativa = dto.AvatarRutaRelativa;
            Instagram = dto.Instagram;
            Facebook = dto.Facebook;
            X = dto.X;
            Discord = dto.Discord;
        }

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
