using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface ISeleccionarAvatarService
    {
        Task<ObjetoAvatar> SeleccionarAvatarAsync(string avatarSeleccionadoRutaRelativa = null);
    }
}
