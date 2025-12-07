using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Modelo;
using System;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante
{
    /// <summary>
    /// Expone operaciones auxiliares para mantener sincronizada la sesion del usuario.
    /// </summary>
    public class UsuarioMapeador : IUsuarioMapeador
    {
        private readonly IUsuarioAutenticado _usuarioSesion;

        /// <summary>
        /// Inicializa el mapeador inyectando la sesion actual.
        /// </summary>
        public UsuarioMapeador(IUsuarioAutenticado usuarioSesion)
        {
            _usuarioSesion = usuarioSesion ??
                throw new ArgumentNullException(nameof(usuarioSesion));
        }
        /// <summary>
        /// Actualiza la sesion del usuario actual a partir del DTO recibido del servidor.
        /// </summary>
        /// <param name="dto">Datos del usuario autenticado.</param>
        public void ActualizarSesion(DTOs.UsuarioDTO dto)
        {
            if (dto == null)
            {
                _usuarioSesion.Limpiar();
                return;
            }

            _usuarioSesion.CargarDesdeDTO(dto);
        }
    }
}