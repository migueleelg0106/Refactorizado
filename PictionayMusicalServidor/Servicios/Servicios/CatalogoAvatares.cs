using System;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Datos.DAL.Implementaciones;
using Datos.Modelo;
using Datos.Utilidades;
using log4net;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;

namespace Servicios.Servicios
{
    public class CatalogoAvatares : ICatalogoAvatares
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CatalogoAvatares));

        public AvatarDTO[] ObtenerAvataresDisponibles()
        {
            try
            {
                using (var contexto = CrearContexto())
                {
                    var repositorio = new AvatarRepositorio(contexto);
                    return repositorio.ObtenerAvatares()
                        .Select(a => new AvatarDTO
                        {
                            Id = a.idAvatar,
                            Nombre = a.Nombre_Avatar,
                            RutaRelativa = a.Avatar_Ruta
                        })
                        .ToArray();
                }
            }
            catch (DataException ex)
            {
                Logger.Error("Error de datos al obtener los avatares disponibles", ex);
                return Array.Empty<AvatarDTO>();
            }
            catch (EntityException ex)
            {
                Logger.Error("Error de entidad al obtener los avatares disponibles", ex);
                return Array.Empty<AvatarDTO>();
            }
            catch (DbUpdateException ex)
            {
                Logger.Error("Error al actualizar la base de datos al obtener los avatares disponibles", ex);
                return Array.Empty<AvatarDTO>();
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Error inesperado al obtener los avatares disponibles", ex);
                return Array.Empty<AvatarDTO>();
            }
        }

        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }
    }
}
