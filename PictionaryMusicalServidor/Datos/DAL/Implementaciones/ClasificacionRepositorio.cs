using System;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using PictionaryMusicalServidor.Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Implementaciones
{
    public class ClasificacionRepositorio : IClasificacionRepositorio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClasificacionRepositorio));
        private readonly BaseDatosPruebaEntities1 _contexto;

        public ClasificacionRepositorio(BaseDatosPruebaEntities1 contexto)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public Clasificacion CrearClasificacionInicial()
        {
            try
            {
                var clasificacion = new Clasificacion
                {
                    Puntos_Ganados = 0,
                    Rondas_Ganadas = 0
                };

                _contexto.Clasificacion.Add(clasificacion);
                _contexto.SaveChanges();

                // No es estrictamente necesario un Info aquí si se llama siempre al crear usuario, 
                // pero ayuda a la trazabilidad si falla.

                return clasificacion;
            }
            catch (Exception ex)
            {
                _logger.Error("Error al crear la clasificación inicial.", ex);
                throw;
            }
        }
    }
}