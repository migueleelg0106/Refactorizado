using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Datos.Utilidades;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Servicios
{

    internal static class ServicioAmistad
    {

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
                    if (solicitud.UsuarioReceptor != usuarioId)
                    {
                        continue;
                    }

                    string emisor = solicitud.Usuario?.Nombre_Usuario; 
                    string receptor = solicitud.Usuario1?.Nombre_Usuario; 

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

                if (relacion.UsuarioReceptor != usuarioReceptorId)
                {
                    throw new InvalidOperationException("No fue posible aceptar la solicitud de amistad.");
                }

                if (relacion.Estado)
                {
                    throw new InvalidOperationException("La solicitud de amistad ya fue aceptada con anterioridad.");
                }

                amigoRepositorio.ActualizarEstado(relacion, true);
            }
        }

        public static Amigo EliminarAmistad(int usuarioAId, int usuarioBId)
        {
            if (usuarioAId == usuarioBId)
            {
                throw new InvalidOperationException("No fue posible eliminar la relación de amistad.");
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