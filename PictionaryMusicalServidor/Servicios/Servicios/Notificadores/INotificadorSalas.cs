using System;
using PictionaryMusicalServidor.Servicios.Contratos;

namespace PictionaryMusicalServidor.Servicios.Servicios.Notificadores
{
    /// <summary>
    /// Define las operaciones para el sistema de notificaciones de la lista general de salas 
    /// (Lobby).
    /// </summary>
    public interface INotificadorSalas
    {
        /// <summary>
        /// Suscribe un callback a las notificaciones de la lista de salas.
        /// </summary>
        Guid Suscribir(ISalasManejadorCallback callback);

        /// <summary>
        /// Elimina una suscripcion especifica mediante su ID.
        /// </summary>
        void Desuscribir(Guid sesionId);

        /// <summary>
        /// Elimina todas las suscripciones asociadas a un callback especifico.
        /// </summary>
        void DesuscribirPorCallback(ISalasManejadorCallback callback);

        /// <summary>
        /// Envia la lista actualizada de salas unicamente al suscriptor indicado.
        /// </summary>
        void NotificarListaSalas(ISalasManejadorCallback callback);

        /// <summary>
        /// Envia la lista actualizada de salas a todos los clientes suscritos.
        /// </summary>
        void NotificarListaSalasATodos();
    }
}