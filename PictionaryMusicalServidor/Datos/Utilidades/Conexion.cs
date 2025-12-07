using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace PictionaryMusicalServidor.Datos.Utilidades
{
    /// <summary>
    /// Clase de utilidad para la gestion y construccion de cadenas de conexion.
    /// </summary>
    public static class Conexion
    {
        /// <summary>
        /// Construye y obtiene la cadena de conexion completa para Entity Framework.
        /// Utiliza variables de entorno para las credenciales y el servidor.
        /// </summary>
        /// <returns>Cadena de conexion formateada para EntityClient.</returns>
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
                Metadata = "res://*/Modelo.BaseDatosPictionaryMusical.csdl|res://*/Modelo.BaseDatosPictionaryMusical.ssdl|res://*/Modelo.BaseDatosPictionaryMusical.msl"
            };

            return constructorEntidad.ToString();
        }
    }
}
