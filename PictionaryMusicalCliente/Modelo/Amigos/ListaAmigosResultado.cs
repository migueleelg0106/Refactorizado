using System;
using System.Collections.Generic;

namespace PictionaryMusicalCliente.Modelo.Amigos
{
    public class ListaAmigosResultado
    {
        public ListaAmigosResultado(ResultadoOperacion resultado, IReadOnlyList<string> amigos, string jugador)
        {
            Resultado = resultado ?? throw new ArgumentNullException(nameof(resultado));
            Amigos = amigos;
            Jugador = jugador;
        }

        public ResultadoOperacion Resultado { get; }

        public IReadOnlyList<string> Amigos { get; }

        public string Jugador { get; }
    }

    public class ListaAmigosActualizadaEventArgs : EventArgs
    {
        public ListaAmigosActualizadaEventArgs(string jugador, IReadOnlyList<string> amigos)
        {
            Jugador = jugador;
            Amigos = amigos;
        }

        public string Jugador { get; }

        public IReadOnlyList<string> Amigos { get; }
    }
}
