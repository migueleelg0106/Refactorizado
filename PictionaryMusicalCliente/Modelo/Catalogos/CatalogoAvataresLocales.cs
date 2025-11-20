using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace PictionaryMusicalCliente.Modelo.Catalogos
{
    /// <summary>
    /// Gestiona la coleccion de avatares predefinidos disponibles en la aplicacion.
    /// </summary>
    public static class CatalogoAvataresLocales
    {
        private const string CarpetaRecursos = "Recursos";

        private static readonly IReadOnlyList<ObjetoAvatar> _listaAvatares = new List<ObjetoAvatar>
        {
            Crear(1, "AC/DC", "ACDC.jpg"),
            Crear(2, "Aleks Syntek", "Aleks_Syntek.jpg"),
            Crear(3, "Anuel AA", "Anuel_AA.jpg"),
            Crear(4, "Bad Bunny", "Bad_Bunny.jpg"),
            Crear(5, "Britney Spears", "Britney_Spears.jpg"),
            Crear(6, "Bruno Mars", "Bruno_Mars.jpg"),
            Crear(7, "Drake", "Drake.jpg"),
            Crear(8, "Guns N' Roses", "Guns_N_Roses.jpg"),
            Crear(9, "J Balvin", "J_Balvin.jpg"),
            Crear(10, "José José", "Jose_Jose.jpg"),
            Crear(11, "Kanye West", "Kanye_West.jpg"),
            Crear(12, "Luis Miguel", "Luis_Miguel.jpg"),
            Crear(13, "Mariah Carey", "Mariah_Carey.jpg"),
            Crear(14, "Taylor Swift", "Taylor_Swift.jpg"),
            Crear(15, "The Weeknd", "The_Weeknd.jpg"),
            Crear(16, "Travis Scott", "Travis_Scott.jpg")
        };

        private static readonly Dictionary<int, ObjetoAvatar> _diccionarioAvatares =
            _listaAvatares.ToDictionary(a => a.Id);

        /// <summary>
        /// Obtiene la lista completa de avatares cargados en memoria.
        /// </summary>
        /// <returns>Lista de lectura de objetos avatar.</returns>
        public static IReadOnlyList<ObjetoAvatar> ObtenerAvatares()
        {
            return _listaAvatares;
        }

        /// <summary>
        /// Busca un avatar especifico por su identificador unico.
        /// </summary>
        /// <param name="id">Identificador del avatar.</param>
        /// <returns>El objeto avatar si existe, o null.</returns>
        public static ObjetoAvatar ObtenerPorId(int id)
        {
            return _diccionarioAvatares.TryGetValue(id, out var avatar) ? avatar : null;
        }

        private static ObjetoAvatar Crear(int id, string nombre, string archivo)
        {
            var uri = new Uri(
                $"pack://application:,,,/{CarpetaRecursos}/{archivo}",
                UriKind.Absolute);
            var imagen = new BitmapImage(uri);

            return new ObjetoAvatar(id, nombre, imagen);
        }
    }
}