using System;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using PictionaryMusicalServidor.Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Implementaciones
{
    public class ClasificacionRepositorio : IClasificacionRepositorio
    {
        private readonly BaseDatosPruebaEntities1 _contexto;

        public ClasificacionRepositorio(BaseDatosPruebaEntities1 contexto)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public Clasificacion CrearClasificacionInicial()
        {
            var clasificacion = new Clasificacion
            {
                Puntos_Ganados = 0,
                Rondas_Ganadas = 0
            };

            _contexto.Clasificacion.Add(clasificacion);
            _contexto.SaveChanges();

            return clasificacion;
        }
    }
}
