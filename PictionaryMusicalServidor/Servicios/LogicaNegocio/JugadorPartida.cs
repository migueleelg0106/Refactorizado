using System;

namespace PictionaryMusicalServidor.Servicios.LogicaNegocio
{
    /// <summary>
    /// Representa el estado de un jugador dentro de una partida en memoria.
    /// </summary>
    public class JugadorPartida
    {
        public string NombreUsuario { get; set; }

        public string IdConexion { get; set; }

        public bool EsHost { get; set; }

        public bool EsDibujante { get; set; }

        public bool YaAdivino { get; set; }

        public int PuntajeTotal { get; set; }

        public JugadorPartida CopiarDatosBasicos()
        {
            return new JugadorPartida
            {
                NombreUsuario = NombreUsuario,
                IdConexion = IdConexion,
                EsHost = EsHost,
                EsDibujante = EsDibujante,
                YaAdivino = YaAdivino,
                PuntajeTotal = PuntajeTotal
            };
        }

        public override string ToString()
        {
            return string.Format(
                "Nombre: {0}, Conexion: {1}, Host: {2}, Dibujante: {3}, Puntaje: {4}",
                NombreUsuario,
                IdConexion,
                EsHost,
                EsDibujante,
                PuntajeTotal);
        }
    }
}
