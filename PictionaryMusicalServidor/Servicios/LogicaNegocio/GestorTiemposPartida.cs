using System;
using System.Timers;

namespace PictionaryMusicalServidor.Servicios.LogicaNegocio
{
    /// <summary>
    /// Gestiona los temporizadores de la partida y el control del tiempo restante para rondas y 
    /// transiciones.
    /// Implementa IDisposable para asegurar la liberacion correcta de los recursos del 
    /// temporizador.
    /// </summary>
    public class GestorTiemposPartida : IDisposable
    {
        private readonly Timer _timerRonda;
        private readonly Timer _timerTransicion;
        private DateTime _inicioRonda;
        private int _duracionRondaSegundos;

        /// <summary>
        /// Evento que se dispara cuando el tiempo de la ronda de juego ha finalizado.
        /// </summary>
        public event Action TiempoRondaAgotado;

        /// <summary>
        /// Evento que se dispara cuando el tiempo de espera (transicion) entre rondas ha 
        /// finalizado.
        /// </summary>
        public event Action TiempoTransicionAgotado;

        /// <summary>
        /// Inicializa una nueva instancia del gestor de tiempos.
        /// </summary>
        /// <param name="duracionRondaSegundos">Duracion en segundos para cada ronda de juego.
        /// </param>
        /// <param name="duracionTransicionSegundos">Duracion en segundos para las pausas entre 
        /// rondas.</param>
        public GestorTiemposPartida(int duracionRondaSegundos, int duracionTransicionSegundos)
        {
            _duracionRondaSegundos = duracionRondaSegundos;

            _timerRonda = new Timer 
            { 
                AutoReset = false 
            };

            _timerRonda.Elapsed += (s, e) =>
            {
                TiempoRondaAgotado?.Invoke();
            };

            _timerTransicion = new Timer
            {
                AutoReset = false,
                Interval = duracionTransicionSegundos * 1000
            };

            _timerTransicion.Elapsed += (s, e) =>
            {
                TiempoTransicionAgotado?.Invoke();
            };
        }

        /// <summary>
        /// Inicia el temporizador principal de la ronda y registra la marca de tiempo de inicio.
        /// </summary>
        public void IniciarRonda()
        {
            _timerTransicion.Stop();
            _timerRonda.Interval = _duracionRondaSegundos * 1000;
            _inicioRonda = DateTime.UtcNow;
            _timerRonda.Start();
        }

        /// <summary>
        /// Detiene la ronda actual e inicia el temporizador de transicion.
        /// </summary>
        public void IniciarTransicion()
        {
            DetenerRonda();
            _timerTransicion.Start();
        }

        /// <summary>
        /// Detiene inmediatamente todos los temporizadores activos.
        /// </summary>
        public void DetenerTodo()
        {
            DetenerRonda();
            _timerTransicion.Stop();
        }

        /// <summary>
        /// Calcula los puntos a otorgar basandose en el tiempo restante de la ronda.
        /// </summary>
        /// <returns>Cantidad de segundos restantes como puntos, o cero si el tiempo se agoto.
        /// </returns>
        public int CalcularPuntosPorTiempo()
        {
            if (!_timerRonda.Enabled)
            {
                return 0;
            }

            var transcurrido = (int)(DateTime.UtcNow - _inicioRonda).TotalSeconds;
            var restante = _duracionRondaSegundos - transcurrido;
            return Math.Max(0, restante);
        }

        /// <summary>
        /// Libera los recursos utilizados por los temporizadores.
        /// </summary>
        public void Dispose()
        {
            _timerRonda?.Dispose();
            _timerTransicion?.Dispose();
        }

        private void DetenerRonda()
        {
            _timerRonda.Stop();
            _inicioRonda = default;
        }
    }
}