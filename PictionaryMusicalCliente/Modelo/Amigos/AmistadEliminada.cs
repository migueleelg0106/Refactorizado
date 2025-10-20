using System;

namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class AmistadEliminada
    {
        public AmistadEliminada(string usuarioA, string usuarioB)
        {
            UsuarioA = usuarioA ?? throw new ArgumentNullException(nameof(usuarioA));
            UsuarioB = usuarioB ?? throw new ArgumentNullException(nameof(usuarioB));
        }

        public string UsuarioA { get; }

        public string UsuarioB { get; }

        public bool InvolucraUsuario(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return false;
            }

            return string.Equals(nombreUsuario, UsuarioA, StringComparison.OrdinalIgnoreCase)
                || string.Equals(nombreUsuario, UsuarioB, StringComparison.OrdinalIgnoreCase);
        }

        public string ObtenerOtroUsuario(string usuarioActual)
        {
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                return UsuarioA;
            }

            return string.Equals(usuarioActual, UsuarioA, StringComparison.OrdinalIgnoreCase)
                ? UsuarioB
                : UsuarioA;
        }
    }
}
