using Datos.Modelo;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Implementación concreta de la factoría de repositorios.
    /// Crea instancias de repositorios utilizando un contexto de base de datos proporcionado.
    /// Esta clase facilita las pruebas unitarias al permitir mockear la creación de repositorios.
    /// </summary>
    public class RepositorioFactoria : IRepositorioFactoria
    {
        /// <inheritdoc/>
        public IUsuarioRepositorio CrearUsuarioRepositorio(BaseDatosPruebaEntities contexto)
        {
            return new UsuarioRepositorio(contexto);
        }

        /// <inheritdoc/>
        public IAmigoRepositorio CrearAmigoRepositorio(BaseDatosPruebaEntities contexto)
        {
            return new AmigoRepositorio(contexto);
        }

        /// <inheritdoc/>
        public IClasificacionRepositorio CrearClasificacionRepositorio(BaseDatosPruebaEntities contexto)
        {
            return new ClasificacionRepositorio(contexto);
        }

        /// <inheritdoc/>
        public IReporteRepositorio CrearReporteRepositorio(BaseDatosPruebaEntities contexto)
        {
            return new ReporteRepositorio(contexto);
        }

        /// <inheritdoc/>
        public IJugadorRepositorio CrearJugadorRepositorio(BaseDatosPruebaEntities contexto)
        {
            return new JugadorRepositorio(contexto);
        }
    }
}
