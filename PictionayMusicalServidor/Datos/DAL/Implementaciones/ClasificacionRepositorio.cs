using System;
using Datos.DAL.Interfaces;
using Datos.Modelo;

namespace Datos.DAL.Implementaciones
{
    public class ClasificacionRepositorio : IClasificacionRepositorio
    {
        private readonly BaseDatosPruebaEntities1 contexto;

        public ClasificacionRepositorio(BaseDatosPruebaEntities1 contexto)
        {
            this.contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public Clasificacion CrearClasificacionInicial()
        {
            var clasificacion = new Clasificacion
            {
                Puntos_Ganados = 0,
                Rondas_Ganadas = 0
            };

            contexto.Clasificacion.Add(clasificacion);
            contexto.SaveChanges();

            return clasificacion;
        }
    }
}
