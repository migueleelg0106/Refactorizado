using System;
using System.Collections.Generic;
using System.Linq;

namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class ListaAmigosResultado
    {
        private ListaAmigosResultado(bool exito, string mensaje, IReadOnlyList<string> amigos)
        {
            Exito = exito;
            Mensaje = mensaje;
            Amigos = amigos ?? Array.Empty<string>();
        }

        public bool Exito { get; }

        public string Mensaje { get; }

        public IReadOnlyList<string> Amigos { get; }

        public static ListaAmigosResultado Exitoso(IEnumerable<string> amigos, string mensaje = null)
        {
            return new ListaAmigosResultado(true, mensaje, Normalizar(amigos));
        }

        public static ListaAmigosResultado Fallo(string mensaje, IEnumerable<string> amigos = null)
        {
            return new ListaAmigosResultado(false, mensaje, Normalizar(amigos));
        }

        private static IReadOnlyList<string> Normalizar(IEnumerable<string> amigos)
        {
            if (amigos == null)
            {
                return Array.Empty<string>();
            }

            List<string> lista = amigos
                .Where(amigo => !string.IsNullOrWhiteSpace(amigo))
                .Select(amigo => amigo.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return lista.Count > 0 ? lista.AsReadOnly() : Array.Empty<string>();
        }
    }
}
