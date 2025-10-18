using System.Threading.Tasks;

namespace Servicios.Servicios.Utilidades
{
    public interface ICodigoVerificacionNotificador
    {
        Task NotificarAsync(string correoDestino, string codigo, string usuarioDestino);
    }
}
