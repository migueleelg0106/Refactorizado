using System;
using System.Linq;
using Datos.DAL.Implementaciones;
using Datos.Modelo;
using Datos.Utilidades;
using log4net;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;

namespace Servicios.Servicios
{
    public class CatalogoAvataresManejador : ICatalogoAvataresManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CatalogoAvataresManejador));

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
                            AvatarId = a.idAvatar,
                            Nombre = a.Nombre_Avatar,
                            RutaRelativa = a.Avatar_Ruta
                        })
                        .ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error al obtener los avatares disponibles", ex);
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