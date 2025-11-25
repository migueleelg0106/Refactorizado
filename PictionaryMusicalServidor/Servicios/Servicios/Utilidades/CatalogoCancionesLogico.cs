using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Catalogo estatico de canciones para el juego.
    /// Proporciona metodos para obtener y validar canciones durante la partida.
    /// </summary>
    public static class CatalogoCancionesLogico
    {
        private static readonly Dictionary<int, DatosCancionDTO> _catalogoCanciones;

        static CatalogoCancionesLogico()
        {
            _catalogoCanciones = new Dictionary<int, DatosCancionDTO>
            {
                {
                    1, new DatosCancionDTO
                    {
                        IdCancion = 1,
                        Nombre = "Gasolina",
                        Artista = "Daddy Yankee",
                        Genero = "Reggaeton",
                        RutaArchivo = "Gasolina_Daddy_Yankee.mp3"
                    }
                },
                {
                    2, new DatosCancionDTO
                    {
                        IdCancion = 2,
                        Nombre = "Despacito",
                        Artista = "Luis Fonsi",
                        Genero = "Reggaeton",
                        RutaArchivo = "Despacito_Luis_Fonsi.mp3"
                    }
                },
                {
                    3, new DatosCancionDTO
                    {
                        IdCancion = 3,
                        Nombre = "La Bicicleta",
                        Artista = "Carlos Vives",
                        Genero = "Vallenato",
                        RutaArchivo = "La_Bicicleta_Carlos_Vives.mp3"
                    }
                },
                {
                    4, new DatosCancionDTO
                    {
                        IdCancion = 4,
                        Nombre = "Vivir Mi Vida",
                        Artista = "Marc Anthony",
                        Genero = "Salsa",
                        RutaArchivo = "Vivir_Mi_Vida_Marc_Anthony.mp3"
                    }
                },
                {
                    5, new DatosCancionDTO
                    {
                        IdCancion = 5,
                        Nombre = "Livin la Vida Loca",
                        Artista = "Ricky Martin",
                        Genero = "Pop Latino",
                        RutaArchivo = "Livin_la_Vida_Loca_Ricky_Martin.mp3"
                    }
                },
                {
                    6, new DatosCancionDTO
                    {
                        IdCancion = 6,
                        Nombre = "Bailando",
                        Artista = "Enrique Iglesias",
                        Genero = "Reggaeton",
                        RutaArchivo = "Bailando_Enrique_Iglesias.mp3"
                    }
                },
                {
                    7, new DatosCancionDTO
                    {
                        IdCancion = 7,
                        Nombre = "La Camisa Negra",
                        Artista = "Juanes",
                        Genero = "Rock Latino",
                        RutaArchivo = "La_Camisa_Negra_Juanes.mp3"
                    }
                },
                {
                    8, new DatosCancionDTO
                    {
                        IdCancion = 8,
                        Nombre = "Corazon Espinado",
                        Artista = "Mana",
                        Genero = "Rock Latino",
                        RutaArchivo = "Corazon_Espinado_Mana.mp3"
                    }
                },
                {
                    9, new DatosCancionDTO
                    {
                        IdCancion = 9,
                        Nombre = "Suavemente",
                        Artista = "Elvis Crespo",
                        Genero = "Merengue",
                        RutaArchivo = "Suavemente_Elvis_Crespo.mp3"
                    }
                },
                {
                    10, new DatosCancionDTO
                    {
                        IdCancion = 10,
                        Nombre = "El Baile del Gorila",
                        Artista = "Melody",
                        Genero = "Pop Latino",
                        RutaArchivo = "El_Baile_del_Gorila_Melody.mp3"
                    }
                }
            };
        }

        /// <summary>
        /// Obtiene una cancion por su identificador.
        /// </summary>
        /// <param name="idCancion">Identificador de la cancion.</param>
        /// <returns>Datos de la cancion o null si no existe.</returns>
        public static DatosCancionDTO ObtenerCancion(int idCancion)
        {
            if (_catalogoCanciones.TryGetValue(idCancion, out var cancion))
            {
                return cancion;
            }

            return null;
        }

        /// <summary>
        /// Obtiene todas las canciones del catalogo.
        /// </summary>
        /// <returns>Lista de todas las canciones disponibles.</returns>
        public static IEnumerable<DatosCancionDTO> ObtenerTodasLasCanciones()
        {
            return _catalogoCanciones.Values;
        }

        /// <summary>
        /// Valida si el intento de adivinanza coincide con el titulo de la cancion.
        /// Normaliza ambas cadenas para comparacion (minusculas, sin acentos, sin espacios extra).
        /// </summary>
        /// <param name="idCancion">Identificador de la cancion.</param>
        /// <param name="intento">Texto del intento de adivinanza.</param>
        /// <returns>True si el intento coincide con el titulo, false en caso contrario.</returns>
        public static bool ValidarTitulo(int idCancion, string intento)
        {
            if (string.IsNullOrWhiteSpace(intento))
            {
                return false;
            }

            if (!_catalogoCanciones.TryGetValue(idCancion, out var cancion))
            {
                return false;
            }

            string tituloNormalizado = NormalizarTexto(cancion.Nombre);
            string intentoNormalizado = NormalizarTexto(intento);

            return string.Equals(
                tituloNormalizado,
                intentoNormalizado,
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Normaliza un texto removiendo acentos, espacios extra y convirtiendo a minusculas.
        /// </summary>
        /// <param name="texto">Texto a normalizar.</param>
        /// <returns>Texto normalizado.</returns>
        private static string NormalizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return string.Empty;
            }

            string textoLimpio = texto.Trim().ToLowerInvariant();
            string sinAcentos = RemoverAcentos(textoLimpio);

            return sinAcentos;
        }

        /// <summary>
        /// Remueve los acentos de un texto.
        /// </summary>
        /// <param name="texto">Texto con posibles acentos.</param>
        /// <returns>Texto sin acentos.</returns>
        private static string RemoverAcentos(string texto)
        {
            if (string.IsNullOrEmpty(texto))
            {
                return texto;
            }

            string normalizado = texto.Normalize(NormalizationForm.FormD);
            var resultado = new StringBuilder();

            foreach (char caracter in normalizado)
            {
                UnicodeCategory categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);
                if (categoria != UnicodeCategory.NonSpacingMark)
                {
                    resultado.Append(caracter);
                }
            }

            return resultado.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
