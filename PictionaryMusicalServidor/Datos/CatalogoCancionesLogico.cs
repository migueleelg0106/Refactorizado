using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using log4net;
using PictionaryMusicalServidor.Datos.Entidades;
using PictionaryMusicalServidor.Datos.Excepciones;

namespace PictionaryMusicalServidor.Datos
{
    /// <summary>
    /// Proporciona el acceso al catalogo interno de canciones disponibles para las partidas.
    /// </summary>
    public static class CatalogoCancionesLogico
    {
        private const string MensajeCancionesNoDisponibles = "No hay canciones disponibles para los criterios solicitados.";
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CatalogoCancionesLogico));
        private static readonly Dictionary<int, Cancion> _canciones = new Dictionary<int, Cancion>
        {
            { 1, CrearCancion(1, "Gasolina", "Daddy Yankee", "Reggaeton", "Español") },
            { 2, CrearCancion(2, "Bocanada", "Gustavo Cerati", "Rock Alternativo", "Español") },
            { 3, CrearCancion(3, "La Nave Del Olvido", "José José", "Balada", "Español") },
            { 4, CrearCancion(4, "Tiburón", "Proyecto Uno", "Merengue", "Español") },
            { 5, CrearCancion(5, "Pupilas De Gato", "Luis Miguel", "Balada Pop", "Español") },
            { 6, CrearCancion(6, "Black Or White", "Michael Jackson", "Pop", "Ingles") },
            { 7, CrearCancion(7, "Don't Stop The Music", "Rihanna", "Pop", "Ingles") },
            { 8, CrearCancion(8, "Man In The Mirror", "Michael Jackson", "Pop", "Ingles") },
            { 9, CrearCancion(9, "Earth Song", "Michael Jackson", "Pop", "Ingles") },
            { 10, CrearCancion(10, "Redbone", "Childish Gambino", "Funk", "Ingles") }
        };

        private static readonly object _randomLock = new object();
        private static readonly Random _random = new Random();

        /// <summary>
        /// Obtiene una cancion aleatoria segun el idioma solicitado y excluyendo los identificadores proporcionados.
        /// </summary>
        /// <param name="idioma">Idioma de la cancion ("Español" o "Ingles").</param>
        /// <param name="idsExcluidos">Coleccion de identificadores que no deben considerarse.</param>
        /// <returns>Una instancia de <see cref="Cancion"/> que cumple los criterios.</returns>
        /// <exception cref="ArgumentException">Se produce cuando el idioma es nulo o vacio.</exception>
        /// <exception cref="CancionNoDisponibleException">Se produce cuando no hay canciones disponibles.</exception>
        public static Cancion ObtenerCancionAleatoria(string idioma, HashSet<int> idsExcluidos)
        {
            if (string.IsNullOrWhiteSpace(idioma))
            {
                var exception = new ArgumentException("El idioma no puede ser nulo o vacio.", nameof(idioma));
                _logger.Error("Se recibio un idioma invalido al solicitar cancion.", exception);
                throw exception;
            }

            var idiomaNormalizado = idioma.Trim();
            var idsRechazados = idsExcluidos ?? new HashSet<int>();

            try
            {
                var disponibles = _canciones.Values
                    .Where(cancion => string.Equals(cancion.Idioma, idiomaNormalizado, StringComparison.OrdinalIgnoreCase)
                        && !idsRechazados.Contains(cancion.Id))
                    .ToList();

                if (!disponibles.Any())
                {
                    var exception = new CancionNoDisponibleException(MensajeCancionesNoDisponibles);
                    _logger.Error("No se encontraron canciones disponibles para los criterios proporcionados.", exception);
                    throw exception;
                }

                lock (_randomLock)
                {
                    var indice = _random.Next(disponibles.Count);
                    return disponibles[indice];
                }
            }
            catch (CancionNoDisponibleException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al obtener una cancion aleatoria.", ex);
                throw;
            }
        }

        /// <summary>
        /// Valida si el intento del usuario coincide con el nombre de la cancion indicada.
        /// </summary>
        /// <param name="idCancion">Identificador de la cancion a evaluar.</param>
        /// <param name="intentoUsuario">Texto proporcionado por el usuario.</param>
        /// <returns>True si el intento coincide con el nombre normalizado de la cancion, false en caso contrario.</returns>
        public static bool ValidarRespuesta(int idCancion, string intentoUsuario)
        {
            if (!_canciones.ContainsKey(idCancion))
            {
                _logger.ErrorFormat("No se encontro la cancion con id {0} en el catalogo.", idCancion);
                return false;
            }

            var intentoNormalizado = NormalizarTexto(intentoUsuario);

            if (string.IsNullOrWhiteSpace(intentoNormalizado))
            {
                _logger.Warn("El intento de respuesta es nulo o vacio despues de normalizar.");
                return false;
            }

            return string.Equals(intentoNormalizado, _canciones[idCancion].NombreNormalizado, StringComparison.Ordinal);
        }

        private static Cancion CrearCancion(int id, string nombre, string artista, string genero, string idioma)
        {
            var nombreNormalizado = NormalizarTexto(nombre);

            return new Cancion
            {
                Id = id,
                Nombre = nombre,
                NombreNormalizado = nombreNormalizado,
                Artista = artista,
                Genero = genero,
                Idioma = idioma
            };
        }

        private static string NormalizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return string.Empty;
            }

            var textoFormateado = texto.Trim().ToLowerInvariant();
            var textoDescompuesto = textoFormateado.Normalize(NormalizationForm.FormD);
            var constructor = new StringBuilder();

            foreach (var caracter in textoDescompuesto)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);
                if (categoria == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(caracter) || char.IsWhiteSpace(caracter))
                {
                    constructor.Append(caracter);
                }
            }

            var textoSinAcentos = constructor.ToString().Normalize(NormalizationForm.FormC);
            var partes = textoSinAcentos.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", partes);
        }
    }
}
