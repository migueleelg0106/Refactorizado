using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace PictionaryMusicalServidor.Datos.Utilidades
{
    /// <summary>
    /// Clase utilitaria para construir la cadena de conexión a SQL Server
    /// usando variables de entorno en lugar de valores codificados.
    /// </summary>
    public static class Conexion
    {
        /// <summary>
        /// Obtiene la cadena de conexión para Entity Framework usando variables de entorno.
        /// </summary>
        /// <remarks>
        /// Las siguientes variables de entorno deben estar configuradas:
        /// <list type="bullet">
        ///   <item><description>BD_SERVIDOR: Nombre del servidor o instancia de SQL Server (por ejemplo: localhost o EQUIPO\SQLEXPRESS). Por defecto: localhost</description></item>
        ///   <item><description>BD_USUARIO: Usuario de la base de datos SQL</description></item>
        ///   <item><description>BD_CONTRASENA: Contraseña del usuario de la base de datos SQL</description></item>
        /// </list>
        /// </remarks>
        /// <returns>Cadena de conexión de Entity Framework configurada para la base de datos.</returns>
        public static string ObtenerConexion()
        {
            var constructorSql = new SqlConnectionStringBuilder
            {
                DataSource = Environment.GetEnvironmentVariable("BD_SERVIDOR") ?? "localhost",
                InitialCatalog = "BaseDatosPrueba",
                UserID = Environment.GetEnvironmentVariable("BD_USUARIO"),
                Password = Environment.GetEnvironmentVariable("BD_CONTRASENA"),
                MultipleActiveResultSets = true
            };

            var constructorEntidad = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = constructorSql.ToString(),
                Metadata = "res://*/Modelo.BasePictionaryMusical.csdl|res://*/Modelo.BasePictionaryMusical.ssdl|res://*/Modelo.BasePictionaryMusical.msl"
            };

            return constructorEntidad.ToString();
        }
    }
}
