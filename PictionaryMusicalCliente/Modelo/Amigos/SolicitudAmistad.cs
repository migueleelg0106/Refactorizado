using System;

namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class SolicitudAmistad
    {
        public SolicitudAmistad(string usuarioEmisor, string usuarioReceptor, bool estaAceptada)
        {
            UsuarioEmisor = usuarioEmisor ?? throw new ArgumentNullException(nameof(usuarioEmisor));
            UsuarioReceptor = usuarioReceptor ?? throw new ArgumentNullException(nameof(usuarioReceptor));
            EstaAceptada = estaAceptada;
        }

        public string UsuarioEmisor { get; }

        public string UsuarioReceptor { get; }

        public bool EstaAceptada { get; }

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
