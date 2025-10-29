using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Datos.DAL.Implementaciones;
using Datos.Modelo;
using Datos.Utilidades;
using Servicios.Contratos.DTOs;

namespace Servicios.Servicios
{
    /// <summary>
    /// Nueva clase interna estática.
    /// Contiene toda la lógica de negocio y acceso a datos para las relaciones de amistad.
    /// No sabe nada sobre WCF o callbacks.
    /// </summary>
    internal static class ServicioAmistad
    {
        /// <summary>
        /// Obtiene las solicitudes de amistad PENDIENTES que ha RECIBIDO un usuario.
        /// </summary>
        public static List<SolicitudAmistadDTO> ObtenerSolicitudesPendientesDTO(int usuarioId)
        {
            using (var contexto = CrearContexto())
            {
                var amigoRepositorio = new AmigoRepositorio(contexto);
                var solicitudesPendientes = amigoRepositorio.ObtenerSolicitudesPendientes(usuarioId);

                if (solicitudesPendientes == null || solicitudesPendientes.Count == 0)
                {
                    return new List<SolicitudAmistadDTO>();
                }

                var resultadoDTOs = new List<SolicitudAmistadDTO>();
                foreach (var solicitud in solicitudesPendientes)
                {
                    // La lógica original solo notificaba si el usuario es el RECEPTOR
                    if (solicitud.UsuarioReceptor != usuarioId)
                    {
                        continue;
                    }

                    string emisor = solicitud.Usuario?.Nombre_Usuario; // UsuarioEmisor
                    string receptor = solicitud.Usuario1?.Nombre_Usuario; // UsuarioReceptor

                    if (string.IsNullOrWhiteSpace(emisor) || string.IsNullOrWhiteSpace(receptor))
                    {
                        continue;
                    }

                    resultadoDTOs.Add(new SolicitudAmistadDTO
                    {
                        UsuarioEmisor = emisor,
                        UsuarioReceptor = receptor,
                        SolicitudAceptada = solicitud.Estado
                    });
                }
                return resultadoDTOs;
            }
        }

        /// <summary>
        /// Crea una nueva solicitud de amistad.
        /// Lanza excepciones si las reglas de negocio se violan.
        /// </summary>
        public static void CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId)
        {
            if (usuarioEmisorId == usuarioReceptorId)
            {
                throw new InvalidOperationException("No es posible enviarse una solicitud de amistad a sí mismo.");
            }

            using (var contexto = CrearContexto())
            {
                var amigoRepositorio = new AmigoRepositorio(contexto);
                if (amigoRepositorio.ExisteRelacion(usuarioEmisorId, usuarioReceptorId))
                {
                    throw new InvalidOperationException("Ya existe una solicitud o relación de amistad entre los usuarios.");
                }

                amigoRepositorio.CrearSolicitud(usuarioEmisorId, usuarioReceptorId);
            }
        }

        /// <summary>
        /// Acepta una solicitud de amistad.
        /// Lanza excepciones si las reglas de negocio se violan.
        /// </summary>
        public static void AceptarSolicitud(int usuarioEmisorId, int usuarioReceptorId)
        {
            using (var contexto = CrearContexto())
            {
                var amigoRepositorio = new AmigoRepositorio(contexto);
                var relacion = amigoRepositorio.ObtenerRelacion(usuarioEmisorId, usuarioReceptorId);

                if (relacion == null)
                {
                    throw new InvalidOperationException("No existe una solicitud de amistad entre los usuarios.");
                }

                // Valida que quien acepta es el receptor real de la solicitud
                if (relacion.UsuarioReceptor != usuarioReceptorId)
                {
                    throw new InvalidOperationException("Solo el receptor de la solicitud puede aceptarla.");
                }

                if (relacion.Estado)
                {
                    throw new InvalidOperationException("La solicitud de amistad ya fue aceptada con anterioridad.");
                }

                amigoRepositorio.ActualizarEstado(relacion, true);
            }
        }

        /// <summary>
        /// Elimina una relación de amistad o una solicitud.
        /// Lanza excepciones si no se encuentra.
        /// Retorna la relación eliminada para fines de notificación.
        /// </summary>
        public static Amigo EliminarAmistad(int usuarioAId, int usuarioBId)
        {
            if (usuarioAId == usuarioBId)
            {
                throw new InvalidOperationException("No es posible eliminar una relación consigo mismo.");
            }

            using (var contexto = CrearContexto())
            {
                var amigoRepositorio = new AmigoRepositorio(contexto);
                var relacion = amigoRepositorio.ObtenerRelacion(usuarioAId, usuarioBId);

                if (relacion == null)
                {
                    throw new InvalidOperationException("No existe una relación de amistad entre los usuarios.");
                }

                amigoRepositorio.EliminarRelacion(relacion);
                return relacion;
            }
        }

        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }

        // --- AGREGAR ESTE MÉTODO A ServicioAmistad.cs ---

        /// <summary>
        /// Obtiene la lista de amigos de un usuario (por su ID) y la mapea a DTOs.
        /// </summary>
        public static List<AmigoDTO> ObtenerAmigosDTO(int usuarioId)
        {
            using (var contexto = CrearContexto())
            {
                var amigoRepositorio = new AmigoRepositorio(contexto);
                IList<Usuario> amigos = amigoRepositorio.ObtenerAmigos(usuarioId);

                var resultado = new List<AmigoDTO>(amigos.Count);
                foreach (var amigo in amigos)
                {
                    if (amigo == null)
                    {
                        continue;
                    }

                    resultado.Add(new AmigoDTO
                    {
                        UsuarioId = amigo.idUsuario,
                        NombreUsuario = amigo.Nombre_Usuario
                    });
                }

                return resultado;
            }
        }

    }

}