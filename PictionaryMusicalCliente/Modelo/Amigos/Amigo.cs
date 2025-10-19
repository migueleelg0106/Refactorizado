using System.Linq;

namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class Amigo
    {
        public int JugadorId { get; set; }

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string Correo { get; set; }

        public int AvatarId { get; set; }

        public string AvatarRutaRelativa { get; set; }

        public string NombreParaMostrar
        {
            get
            {
                string nombreCompleto = string.Join(" ",
                    new[] { Nombre, Apellido }.Where(campo => !string.IsNullOrWhiteSpace(campo)));

                if (!string.IsNullOrWhiteSpace(nombreCompleto))
                {
                    return nombreCompleto;
                }

                if (!string.IsNullOrWhiteSpace(Correo))
                {
                    return Correo;
                }

                return JugadorId > 0
                    ? $"Jugador {JugadorId}"
                    : string.Empty;
            }
        }
    }
}
