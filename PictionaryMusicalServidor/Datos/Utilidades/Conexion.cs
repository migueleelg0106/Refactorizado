using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace PictionaryMusicalServidor.Datos.Utilidades
{
    public static class Conexion
    {
        public static string ObtenerConexion()
        {
            string usuario = Environment.GetEnvironmentVariable("BD_USUARIO");
            string contrasena = Environment.GetEnvironmentVariable("BD_CONTRASENA");
            bool usarAutenticacionIntegrada = string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena);

            var constructorSql = new SqlConnectionStringBuilder
            {
                DataSource = Environment.GetEnvironmentVariable("BD_SERVIDOR") ?? "localhost",
                InitialCatalog = "BaseDatosPrueba",
                MultipleActiveResultSets = true,
                Encrypt = true,
                TrustServerCertificate = true
            };

            if (usarAutenticacionIntegrada)
            {
                constructorSql.IntegratedSecurity = true;
            }
            else
            {
                constructorSql.UserID = usuario;
                constructorSql.Password = contrasena;
            }

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
