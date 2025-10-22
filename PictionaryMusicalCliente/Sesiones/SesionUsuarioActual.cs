using System;
using Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Sesiones
{
    public sealed class SesionUsuarioActual
    {
        private static readonly Lazy<SesionUsuarioActual> InstanciaInterna =
            new Lazy<SesionUsuarioActual>(() => new SesionUsuarioActual());

        private SesionUsuarioActual()
        {
        }

        public static SesionUsuarioActual Instancia => InstanciaInterna.Value;

        public UsuarioDTO Usuario { get; private set; }

        public bool EstaAutenticado => Usuario != null;

        public void EstablecerUsuario(UsuarioDTO usuario)
        {
            Usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));
        }

        public void CerrarSesion()
        {
            Usuario = null;
        }
    }
}
