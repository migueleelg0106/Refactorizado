using Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Interfaces
{
    /// <summary>
    /// Contrato del repositorio encargado de la persistencia de reportes de jugadores.
    /// Permite verificar duplicados y registrar nuevos reportes.
    /// </summary>
    public interface IReporteRepositorio
    {
        /// <summary>
        /// Verifica si existe un reporte previo de un usuario hacia otro.
        /// </summary>
        /// <param name="idReportante">Identificador del usuario que reporta.</param>
        /// <param name="idReportado">Identificador del usuario reportado.</param>
        /// <returns>True si ya existe un reporte previo, en caso contrario False.</returns>
        bool ExisteReporte(int idReportante, int idReportado);

        /// <summary>
        /// Guarda un nuevo reporte en la base de datos.
        /// </summary>
        /// <param name="reporte">Entidad de reporte a almacenar.</param>
        /// <returns>Entidad persistida con su identificador asignado.</returns>
        Reporte CrearReporte(Reporte reporte);

        /// <summary>
        /// Obtiene el numero de reportes recibidos por un usuario especifico.
        /// </summary>
        /// <param name="idReportado">Identificador del usuario reportado.</param>
        /// <returns>Total de reportes asociados al usuario.</returns>
        int ContarReportesRecibidos(int idReportado);
    }
}