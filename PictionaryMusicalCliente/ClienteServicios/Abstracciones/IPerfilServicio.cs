using System.Collections.Generic;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    public interface IPerfilServicio
    {
        Task<DTOs.UsuarioDTO> ObtenerPerfilAsync(int usuarioId);

        Task<DTOs.ResultadoOperacionDTO> ActualizarPerfilAsync(DTOs.ActualizacionPerfilDTO solicitud);
    }
}
