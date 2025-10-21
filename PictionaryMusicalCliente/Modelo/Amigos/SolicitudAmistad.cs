using System;

namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class SolicitudAmistad
    {
        public SolicitudAmistad(string usuarioEmisor, string usuarioReceptor, bool solicitudAceptada)
        {
            if (string.IsNullOrWhiteSpace(usuarioEmisor))
            {
                throw new ArgumentException("El usuario emisor es obligatorio.", nameof(usuarioEmisor));
            }

            if (string.IsNullOrWhiteSpace(usuarioReceptor))
            {
                throw new ArgumentException("El usuario receptor es obligatorio.", nameof(usuarioReceptor));
            }

            UsuarioEmisor = usuarioEmisor;
            UsuarioReceptor = usuarioReceptor;
            SolicitudAceptada = solicitudAceptada;
        }

        public string UsuarioEmisor { get; }

        public string UsuarioReceptor { get; }

        public bool SolicitudAceptada { get; }

        public bool EstaPendiente => !SolicitudAceptada;

        public bool CoincideCon(string usuarioEmisor, string usuarioReceptor)
        {
            return string.Equals(UsuarioEmisor, usuarioEmisor, StringComparison.OrdinalIgnoreCase)
                && string.Equals(UsuarioReceptor, usuarioReceptor, StringComparison.OrdinalIgnoreCase);
        }

        public bool InvolucraUsuario(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return false;
            }

            return string.Equals(UsuarioEmisor, nombreUsuario, StringComparison.OrdinalIgnoreCase)
                || string.Equals(UsuarioReceptor, nombreUsuario, StringComparison.OrdinalIgnoreCase);
        }

        public string ObtenerOtroUsuario(string usuarioActual)
        {
            if (string.Equals(UsuarioEmisor, usuarioActual, StringComparison.OrdinalIgnoreCase))
            {
                return UsuarioReceptor;
            }

            if (string.Equals(UsuarioReceptor, usuarioActual, StringComparison.OrdinalIgnoreCase))
            {
                return UsuarioEmisor;
            }

            return UsuarioReceptor;
        }

        public bool UsuarioEsReceptor(string usuarioActual)
        {
            return string.Equals(UsuarioReceptor, usuarioActual, StringComparison.OrdinalIgnoreCase);
        }
    }
}
