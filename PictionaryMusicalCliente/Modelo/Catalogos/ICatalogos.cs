using System.Collections.Generic;
using System.Windows.Media;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.Modelo.Catalogos
{
    /// <summary>
    /// Define el contrato para obtener la coleccion de avatares disponibles.
    /// </summary>
    public interface ICatalogoAvatares
    {
        /// <summary>
        /// Obtiene la lista completa de avatares cargados.
        /// </summary>
        /// <returns>Lista de lectura de objetos avatar.</returns>
        IReadOnlyList<ObjetoAvatar> ObtenerAvatares();

        /// <summary>
        /// Busca un avatar especifico por su identificador unico.
        /// </summary>
        /// <param name="id">Identificador del avatar.</param>
        /// <returns>El objeto avatar si existe, o null.</returns>
        ObjetoAvatar ObtenerPorId(int id);
    }

    /// <summary>
    /// Define el contrato para acceder a recursos graficos del perfil.
    /// </summary>
    public interface ICatalogoImagenesPerfil
    {
        /// <summary>
        /// Recupera el icono asociado a una red social especifica.
        /// </summary>
        /// <param name="nombre">Nombre clave de la red social.</param>
        /// <returns>El recurso grafico correspondiente o null si no existe.</returns>
        ImageSource ObtenerIconoRedSocial(string nombre);
    }
}