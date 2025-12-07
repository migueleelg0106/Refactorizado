using System;

namespace PictionaryMusicalServidor.Datos.Excepciones
{
    /// <summary>
    /// Excepcion lanzada cuando no hay canciones disponibles para las condiciones solicitadas.
    /// </summary>
    public class CancionNoDisponibleExcepcion : Exception
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="CancionNoDisponibleExcepcion"/>.
        /// </summary>
        public CancionNoDisponibleExcepcion()
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="CancionNoDisponibleExcepcion"/> 
        /// con un mensaje especifico.
        /// </summary>
        /// <param name="mensaje">Mensaje descriptivo del error.</param>
        public CancionNoDisponibleExcepcion(string mensaje)
            : base(mensaje)
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="CancionNoDisponibleExcepcion"/> 
        /// con un mensaje y una excepcion interna.
        /// </summary>
        /// <param name="mensaje">Mensaje descriptivo del error.</param>
        /// <param name="excepcionInterna">Excepcion que origino el error actual.</param>
        public CancionNoDisponibleExcepcion(string mensaje, Exception excepcionInterna)
            : base(mensaje, excepcionInterna)
        {
        }
    }
}
