using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface ISeleccionarAvatarServicio
    {
        Task<ObjetoAvatar> SeleccionarAvatarAsync(string avatarSeleccionadoRutaRelativa = null);
    }
}
