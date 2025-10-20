using System;

namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class RespuestaSolicitudAmistad
    {
        public RespuestaSolicitudAmistad(string usuarioEmisor, string usuarioReceptor, bool solicitudAceptada)
        {
            UsuarioEmisor = usuarioEmisor ?? throw new ArgumentNullException(nameof(usuarioEmisor));
            UsuarioReceptor = usuarioReceptor ?? throw new ArgumentNullException(nameof(usuarioReceptor));
            SolicitudAceptada = solicitudAceptada;
        }

        public string UsuarioEmisor { get; }

        public string UsuarioReceptor { get; }

        public bool SolicitudAceptada { get; }

        public bool InvolucraUsuario(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return false;
            }

            return string.Equals(nombreUsuario, UsuarioEmisor, StringComparison.OrdinalIgnoreCase)
                || string.Equals(nombreUsuario, UsuarioReceptor, StringComparison.OrdinalIgnoreCase);
        }

        public string ObtenerOtroUsuario(string usuarioActual)
        {
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                return UsuarioEmisor;
            }

            return string.Equals(usuarioActual, UsuarioEmisor, StringComparison.OrdinalIgnoreCase)
                ? UsuarioReceptor
                : UsuarioEmisor;
        }
    }
}
