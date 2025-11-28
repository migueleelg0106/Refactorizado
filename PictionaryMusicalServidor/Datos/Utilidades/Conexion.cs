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
        private const string VariableServidor = "BD_SERVIDOR";
        private const string VariableUsuario = "BD_USUARIO";
        private const string VariableContrasena = "BD_CONTRASENA";
        private const string ServidorPorDefecto = "localhost";
        private const string NombreBaseDatos = "BaseDatosPrueba";

        /// <summary>
        /// Obtiene la cadena de conexión para Entity Framework usando variables de entorno.
        /// </summary>
        /// <remarks>
        /// Las siguientes variables de entorno deben estar configuradas:
        /// <list type="bullet">
        ///   <item><description>BD_SERVIDOR: Nombre del servidor o instancia de SQL Server (por ejemplo: localhost o EQUIPO\SQLEXPRESS). Por defecto: localhost</description></item>
        ///   <item><description>BD_USUARIO: Usuario de la base de datos SQL (requerido)</description></item>
        ///   <item><description>BD_CONTRASENA: Contraseña del usuario de la base de datos SQL (requerido)</description></item>
        /// </list>
        /// </remarks>
        /// <returns>Cadena de conexión de Entity Framework configurada para la base de datos.</returns>
        /// <exception cref="InvalidOperationException">Se lanza cuando las variables de entorno requeridas no están configuradas.</exception>
        public static string ObtenerConexion()
        {
            string servidor = Environment.GetEnvironmentVariable(VariableServidor);
            string usuario = Environment.GetEnvironmentVariable(VariableUsuario);
            string contrasena = Environment.GetEnvironmentVariable(VariableContrasena);

            if (string.IsNullOrEmpty(usuario))
            {
                throw new InvalidOperationException(
                    $"La variable de entorno '{VariableUsuario}' es requerida pero no está configurada.");
            }

            if (string.IsNullOrEmpty(contrasena))
            {
                throw new InvalidOperationException(
                    $"La variable de entorno '{VariableContrasena}' es requerida pero no está configurada.");
            }

            var constructorSql = new SqlConnectionStringBuilder
            {
                DataSource = string.IsNullOrEmpty(servidor) ? ServidorPorDefecto : servidor,
                InitialCatalog = NombreBaseDatos,
                UserID = usuario,
                Password = contrasena,
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
