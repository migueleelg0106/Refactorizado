using System.Threading.Tasks;

namespace PictionaryMusical.Servicios.Servicios.Utilidades
{
    public interface ICodigoVerificacionNotificador
    {
        Task<bool> NotificarAsync(string correoDestino, string codigo, string usuarioDestino);
    }
}
