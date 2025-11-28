using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace PictionaryMusicalServidor.Datos.Utilidades
{
    public static class Conexion
    {
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
