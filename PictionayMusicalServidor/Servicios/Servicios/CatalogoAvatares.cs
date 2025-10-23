using System;
using System.Data;
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
                using var contexto = CrearContexto();
                var repositorio = new AvatarRepositorio(contexto);
                return [.. repositorio.ObtenerAvatares()
                        .Select(a => new AvatarDTO
                        {
                            Id = a.idAvatar,
                            Nombre = a.Nombre_Avatar,
                            RutaRelativa = a.Avatar_Ruta
                        })];
            }
            catch (DataException ex)
            {
                Logger.Error("Error de datos al obtener los avatares disponibles", ex);
                return [];
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Error inesperado al obtener los avatares disponibles", ex);
                return [];
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
