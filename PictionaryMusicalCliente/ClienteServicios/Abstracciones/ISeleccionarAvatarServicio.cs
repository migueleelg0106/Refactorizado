using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    public interface ISeleccionarAvatarServicio
    {
        Task<ObjetoAvatar> SeleccionarAvatarAsync(int idAvatar);
    }
}
