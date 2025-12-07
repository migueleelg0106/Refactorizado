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
    /// 
    /// Proporciona el acceso al catalogo interno de canciones disponibles para las partidas.
    /// </summary>
    public class CatalogoCanciones : ICatalogoCanciones
    {
        private const string MensajeCancionesNoDisponibles = 
            "No hay canciones disponibles para los criterios solicitados.";
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CatalogoCanciones));
        private static readonly Dictionary<int, Cancion> _canciones = new Dictionary<int, Cancion>
        {
            { 1, CrearCancion(1, "Gasolina", "Daddy Yankee", "Reggaeton", "Espanol") },
            { 2, CrearCancion(2, "Bocanada", "Gustavo Cerati", "Rock Alternativo", "Espanol") },
            { 3, CrearCancion(3, "La Nave Del Olvido", "José José", "Balada", "Espanol") },
            { 4, CrearCancion(4, "Tiburón", "Proyecto Uno", "Merengue House", "Espanol") },
            { 5, CrearCancion(5, "Pupilas De Gato", "Luis Miguel", "Pop Latino", "Espanol") },
            { 6, CrearCancion(6, "El Triste", "José José", "Balada", "Espanol") },
            { 7, CrearCancion(7, "El Reloj", "Luis Miguel", "Bolero", "Espanol") },
            { 8, CrearCancion(8, "La Camisa Negra", "Juanes", "Pop Rock", "Espanol") },
            { 9, CrearCancion(9, "Rosas", "La Oreja de Van Gogh", "Pop", "Espanol") },
            { 10, CrearCancion(10, "La Bicicleta", "Shakira", "Vallenato Pop", "Espanol") },
            { 11, CrearCancion(11, "El Taxi", "Pitbull", "Urbano", "Espanol") },
            { 12, CrearCancion
                (12, "La Puerta Negra", "Los Tigres del Norte", "Norteño", "Espanol") },
            { 13, CrearCancion(13, "Baraja de Oro", "Chalino Sánchez", "Corrido", "Espanol") },
            { 14, CrearCancion(14, "Los Luchadores", "La Sonora Santanera", "Cumbia", "Espanol") },
            { 15, CrearCancion(15, "El Oso Polar", "Nelson Kanzela", "Cumbia", "Espanol") },
            { 16, CrearCancion(16, "El Teléfono", "Wisin & Yandel", "Reggaeton", "Espanol") },
            { 17, CrearCancion(17, "La Planta", "Caos", "Pop Rock", "Espanol") },
            { 18, CrearCancion(18, "Lluvia", "Eddie Santiago", "Salsa", "Espanol") },
            { 19, CrearCancion(19, "Pose", "Daddy Yankee", "Reggaeton", "Espanol") },
            { 20, CrearCancion(20, "Cama y Mesa", "Roberto Carlos", "Balada", "Espanol") },

            { 21, CrearCancion(21, "Black Or White", "Michael Jackson", "Pop", "Ingles") },
            { 22, CrearCancion(22, "Don't Stop The Music", "Rihanna", "Dance Pop", "Ingles") },
            { 23, CrearCancion(23, "Man In The Mirror", "Michael Jackson", "Pop/R&B", "Ingles") },
            { 24, CrearCancion(24, "Earth Song", "Michael Jackson", "Pop", "Ingles") },
            { 25, CrearCancion(25, "Redbone", "Childish Gambino", "Funk", "Ingles") },
            { 26, CrearCancion(26, "The Chain", "Fleetwood Mac", "Rock", "Ingles") },
            { 27, CrearCancion(27, "Umbrella", "Rihanna", "R&B", "Ingles") },
            { 28, CrearCancion(28, "Yellow Submarine", "The Beatles", "Pop Rock", "Ingles") },
            { 29, CrearCancion(29, "Money", "Pink Floyd", "Rock Progresivo", "Ingles") },
            { 30, CrearCancion(30, "Diamonds", "Rihanna", "Pop", "Ingles") },
            { 31, CrearCancion(31, "Grenade", "Bruno Mars", "Pop", "Ingles") },
            { 32, CrearCancion(32, "Scarface", "Paul Engemann", "Synthpop", "Ingles") },
            { 33, CrearCancion(33, "Animals", "Martin Garrix", "EDM", "Ingles") },
            { 34, CrearCancion(34, "Hotel California", "Eagles", "Rock", "Ingles") },
            { 35, CrearCancion(35, "67", "Skrilla", "Hip Hop", "Ingles") },
            { 36, CrearCancion(36, "Blackbird", "The Beatles", "Folk", "Ingles") },
            { 37, CrearCancion(37, "Pony", "Ginuwine", "R&B", "Ingles") },
            { 38, CrearCancion(38, "Rocket Man", "Elton John", "Soft Rock", "Ingles") },
            { 39, CrearCancion(39, "Starman", "David Bowie", "Glam Rock", "Ingles") },
            { 40, CrearCancion(40, "Time In A Bottle", "Jim Croce", "Folk", "Ingles") }
        };

        private static readonly object _randomLock = new object();
        private static readonly Random _random = new Random();

        /// <summary>
        /// Obtiene una cancion aleatoria segun el idioma solicitado y excluyendo los 
        /// identificadores proporcionados.
        /// </summary>
        /// <param name="idioma">Idioma de la cancion ("Espanol" o "Ingles").</param>
        /// <param name="idsExcluidos">Coleccion de identificadores que no deben considerarse.
        /// </param>
        /// <returns>Una instancia de <see cref="Cancion"/> que cumple los criterios.</returns>
        /// <exception cref="ArgumentException">Se produce cuando el idioma es nulo o vacio.
        /// </exception>
        /// <exception cref="CancionNoDisponibleExcepcion">Se produce cuando no hay canciones 
        /// disponibles.</exception>

        public Cancion ObtenerCancionAleatoria(string idioma, HashSet<int> idsExcluidos)
        {
            if (string.IsNullOrWhiteSpace(idioma))
            {
                var ex = new ArgumentException("El idioma no puede ser nulo o vacio.", 
                    nameof(idioma));
                _logger.Error("Se recibio un idioma invalido al solicitar cancion.", ex);
                throw ex;
            }

            try
            {
                string idiomaInterno = MapearIdiomaInterno(idioma);
                string idiomaBusqueda = NormalizarTexto(idiomaInterno);
                var idsRechazados = idsExcluidos ?? new HashSet<int>();

                var candidatos = ObtenerCandidatos(idiomaBusqueda, idsRechazados);

                if (!candidatos.Any())
                {
                    RegistrarErrorFaltaCanciones(idioma, idiomaInterno, idiomaBusqueda);
                    throw new CancionNoDisponibleExcepcion(MensajeCancionesNoDisponibles);
                }

                return SeleccionarAleatorio(candidatos);
            }
            catch (CancionNoDisponibleExcepcion)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Error("Error inesperado al obtener una cancion aleatoria.", ex);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Error inesperado al obtener una cancion aleatoria.", ex);
                throw;
            }
        }

        /// <summary>
        /// Obtiene una cancion por su identificador.
        /// </summary>
        /// <param name="idCancion">Identificador de la cancion.</param>
        /// <returns>La cancion correspondiente.</returns>
        /// <exception cref="KeyNotFoundException">Se lanza si el identificador no existe en el 
        /// catalogo.</exception>
        public Cancion ObtenerCancionPorId(int idCancion)
        {
            if (_canciones.TryGetValue(idCancion, out var cancion))
            {
                return cancion;
            }

            _logger.WarnFormat(
                "No se encontro la cancion con id {0} en el catalogo.", 
                idCancion);
            
            throw new KeyNotFoundException(
                "La cancion solicitada no se encuentra en el catalogo interno.");
        }

        /// <summary>
        /// Valida si el intento del usuario coincide con el nombre de la cancion indicada.
        /// </summary>
        /// <param name="idCancion">Identificador de la cancion a evaluar.</param>
        /// <param name="intentoUsuario">Texto proporcionado por el usuario.</param>
        /// <returns>True si el intento coincide con el nombre normalizado de la cancion, false en
        /// caso contrario.</returns>
        public bool ValidarRespuesta(int idCancion, string intentoUsuario)
        {
            if (!_canciones.ContainsKey(idCancion))
            {
                _logger.ErrorFormat("No se encontro la cancion con id {0} en el catalogo.", 
                    idCancion);
                return false;
            }

            var intentoNormalizado = NormalizarTexto(intentoUsuario);

            if (string.IsNullOrWhiteSpace(intentoNormalizado))
            {
                return false;
            }

            return string.Equals(intentoNormalizado, _canciones[idCancion].NombreNormalizado, 
                StringComparison.Ordinal);
        }

        private static Cancion CrearCancion(int id, string nombre, string artista, string genero, 
            string idioma)
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

        private static string MapearIdiomaInterno(string idiomaEntrada)
        {
            if (idiomaEntrada.StartsWith("es", StringComparison.OrdinalIgnoreCase))
            {
                return "Espanol";
            }

            if (idiomaEntrada.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            {
                return "Ingles";
            }

            return idiomaEntrada;
        }

        private static List<Cancion> ObtenerCandidatos(string idiomaNormalizado, 
            HashSet<int> idsRechazados)
        {
            var candidatos = _canciones.Values
                .Where(cancion => !idsRechazados.Contains(cancion.Id));

            if (!string.Equals(idiomaNormalizado, "mixto", StringComparison.OrdinalIgnoreCase))
            {
                candidatos = candidatos.Where(cancion =>
                    string.Equals(NormalizarTexto(cancion.Idioma), idiomaNormalizado, 
                    StringComparison.OrdinalIgnoreCase));
            }

            return candidatos.ToList();
        }

        private static void RegistrarErrorFaltaCanciones(string idiomaOriginal, 
            string idiomaMapeado, string idiomaNorm)
        {
            _logger.WarnFormat(
                "Fallo al buscar cancion. Idioma Entrante: {0}, Mapeado: {1}, Normalizado: {2}.",
                idiomaOriginal, idiomaMapeado, idiomaNorm);
        }

        private static Cancion SeleccionarAleatorio(List<Cancion> candidatos)
        {
            lock (_randomLock)
            {
                var indice = _random.Next(candidatos.Count);
                return candidatos[indice];
            }
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
            var partes = textoSinAcentos.Split(new[] { ' ' }, 
                StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", partes);
        }
    }
}
