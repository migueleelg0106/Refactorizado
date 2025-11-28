using System;
using log4net;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Datos.Utilidades;
using PictionaryMusicalServidor.Servicios.Contratos;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Factoría para la creación de contextos de base de datos.
    /// Centraliza la lógica de creación de instancias de contexto.
    /// </summary>
    public class ContextoFactory : IContextoFactory
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ContextoFactory));

        /// <summary>
        /// Crea una nueva instancia del contexto de base de datos.
        /// </summary>
        /// <returns>Instancia del contexto de base de datos configurada.</returns>
        /// <exception cref="InvalidOperationException">
        /// Se lanza cuando las variables de entorno requeridas (BD_USUARIO, BD_CONTRASENA) no están configuradas.
        /// </exception>
        public BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return new BaseDatosPruebaEntities1(conexion);
        }
    }
}