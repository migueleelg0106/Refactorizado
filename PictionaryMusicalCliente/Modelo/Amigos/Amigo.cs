using System;

namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class Amigo
    {
        public Amigo(int idUsuario, string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));
            }

            IdUsuario = idUsuario;
            NombreUsuario = nombreUsuario;
        }

        public int IdUsuario { get; }

        public string NombreUsuario { get; }
    }
}
