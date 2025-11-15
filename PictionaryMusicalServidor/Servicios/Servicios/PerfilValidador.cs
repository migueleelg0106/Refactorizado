using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Servicios
{

    internal static class PerfilValidador
    {
        private const int LongitudMaximaRedSocial = 50;

        public static ResultadoOperacionDTO ValidarActualizacion(ActualizacionPerfilDTO solicitud)
        {
            if (solicitud == null)
            {
                return CrearResultadoFallo("La solicitud de actualización es obligatoria.");
            }

            if (solicitud.UsuarioId <= 0)
            {
                return CrearResultadoFallo("El identificador de usuario es inválido.");
            }

            string nombre = solicitud.Nombre?.Trim();
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 50)
            {
                return CrearResultadoFallo("El nombre es obligatorio y no debe exceder 50 caracteres.");
            }

            string apellido = solicitud.Apellido?.Trim();
            if (string.IsNullOrWhiteSpace(apellido) || apellido.Length > 50)
            {
                return CrearResultadoFallo("El apellido es obligatorio y no debe exceder 50 caracteres.");
            }

            if (solicitud.AvatarId <= 0)
            {
                return CrearResultadoFallo("Selecciona un avatar válido.");
            }

            ResultadoOperacionDTO validacionRedes = ValidarRedesSociales(solicitud);
            if (!validacionRedes.OperacionExitosa)
            {
                return validacionRedes;
            }

            return ResultadoOperacionExitoso();
        }

        private static ResultadoOperacionDTO ValidarRedesSociales(ActualizacionPerfilDTO solicitud)
        {
            ResultadoOperacionDTO resultado = ValidarRedSocial("Instagram", solicitud.Instagram);
            if (!resultado.OperacionExitosa)
            {
                return resultado;
            }

            resultado = ValidarRedSocial("Facebook", solicitud.Facebook);
            if (!resultado.OperacionExitosa)
            {
                return resultado;
            }

            resultado = ValidarRedSocial("X", solicitud.X);
            if (!resultado.OperacionExitosa)
            {
                return resultado;
            }

            return ValidarRedSocial("Discord", solicitud.Discord);
        }

        private static ResultadoOperacionDTO ValidarRedSocial(string nombre, string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return ResultadoOperacionExitoso();
            }

            string normalizado = valor.Trim();
            if (normalizado.Length > LongitudMaximaRedSocial)
            {
                return CrearResultadoFallo(
                    $"El identificador de {nombre} no debe exceder {LongitudMaximaRedSocial} caracteres.");
            }

            return ResultadoOperacionExitoso();
        }

        private static ResultadoOperacionDTO CrearResultadoFallo(string mensaje)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = mensaje
            };
        }

        private static ResultadoOperacionDTO ResultadoOperacionExitoso()
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = true
            };
        }
    }
}