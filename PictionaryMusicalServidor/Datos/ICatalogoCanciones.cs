using System.Collections.Generic;
using PictionaryMusicalServidor.Datos.Entidades;

namespace PictionaryMusicalServidor.Datos
{
    /// <summary>
    /// Interfaz para el servicio de gestion del catalogo de canciones.
    /// Define las operaciones disponibles para obtener y validar canciones.
    /// </summary>
    public interface ICatalogoCanciones
    {
        /// <summary>
        /// Obtiene una cancion aleatoria segun el idioma y las exclusiones.
        /// </summary>
        Cancion ObtenerCancionAleatoria(string idioma, HashSet<int> idsExcluidos);

        /// <summary>
        /// Obtiene una cancion especifica por su identificador.
        /// </summary>
        Cancion ObtenerCancionPorId(int idCancion);

        /// <summary>
        /// Valida si el intento del usuario coincide con la cancion.
        /// </summary>
        bool ValidarRespuesta(int idCancion, string intentoUsuario);
    }
}
