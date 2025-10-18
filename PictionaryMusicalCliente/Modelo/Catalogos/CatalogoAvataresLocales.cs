using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.Modelo.Catalogos
{
    public static class CatalogoAvataresLocales
    {
        private const string CarpetaRecursos = "Recursos";

        private static readonly IReadOnlyList<ObjetoAvatar> Avatares = new List<ObjetoAvatar>
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

        public static IReadOnlyList<ObjetoAvatar> ObtenerAvatares()
        {
            return Avatares;
        }

        private static ObjetoAvatar Crear(int id, string nombre, string archivo)
        {
            string rutaRelativa = $"{CarpetaRecursos}/{archivo}";
            var uri = new Uri($"pack://application:,,,/{rutaRelativa}", UriKind.Absolute);
            var imagen = new BitmapImage(uri);
            return new ObjetoAvatar(id, nombre, imagen, rutaRelativa, null);
        }
    }
}
