using System;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using InicioSesionSrv = PictionaryMusicalCliente.PictionaryServidorServicioInicioSesion;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class InicioSesionService : IInicioSesionService
    {
        public Task<InicioSesionSrv.ResultadoInicioSesionDTO> IniciarSesionAsync(InicioSesionSrv.CredencialesInicioSesionDTO credenciales)
        {
            if (credenciales == null)
            {
                throw new ArgumentNullException(nameof(credenciales));
            }

            var cliente = new InicioSesionSrv.InicioSesionManejadorClient("BasicHttpBinding_IInicioSesionManejador");
            return WcfClientHelper.UsarAsync(cliente, c => c.IniciarSesionAsync(credenciales));
        }
    }
}
