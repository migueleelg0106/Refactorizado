using System.Threading.Tasks;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    public interface ICodigoVerificacionNotificador
    {
        Task<bool> NotificarAsync(string correoDestino, string codigo, string usuarioDestino);
    }
}
