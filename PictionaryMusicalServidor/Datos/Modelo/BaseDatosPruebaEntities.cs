namespace Datos.Modelo
{
    /// <summary>
    /// Representa el contexto de la base de datos extendido para la aplicacion.
    /// Gestiona la conexion y el mapeo de entidades con la base de datos.
    /// </summary>
    public partial class BaseDatosPruebaEntities
    {
        /// <summary>
        /// Inicializa una nueva instancia del contexto de la base de datos utilizando una cadena 
        /// de conexion especifica.
        /// </summary>
        /// <param name="conexion">Cadena de conexion completa para la base de datos SQL Server.
        /// </param>
        public BaseDatosPruebaEntities(string conexion) : base(conexion)
        {
        }
    }
}
