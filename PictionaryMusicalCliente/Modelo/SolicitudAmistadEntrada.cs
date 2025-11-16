using System;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Modelo
{
    public class SolicitudAmistadEntrada(DTOs.SolicitudAmistadDTO solicitud, string nombreUsuario, bool puedeAceptar)
    {
        public DTOs.SolicitudAmistadDTO Solicitud { get; } = solicitud ?? throw new ArgumentNullException(nameof(solicitud));
        public string NombreUsuario { get; } = nombreUsuario;
        public bool PuedeAceptar { get; } = puedeAceptar;
    }
}
