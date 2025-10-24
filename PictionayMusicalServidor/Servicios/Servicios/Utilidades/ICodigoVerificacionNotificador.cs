using System.Threading.Tasks;

namespace Servicios.Servicios.Utilidades
{
    public interface ICodigoVerificacionNotificador
    {
        Task<bool> NotificarAsincrono(string correoDestino, string codigo, string usuarioDestino);
    }
}
