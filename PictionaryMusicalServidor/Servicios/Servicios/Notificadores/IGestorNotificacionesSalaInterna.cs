using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Servicios.Notificadores
{
    /// <summary>
    /// Define las operaciones para gestionar la comunicacion y notificaciones 
    /// con los clientes dentro de una sala.
    /// </summary>
    public interface IGestorNotificacionesSalaInterna
    {
        /// <summary>
        /// Registra un cliente para recibir notificaciones de la sala.
        /// </summary>
        void Registrar(string nombreUsuario, ISalasManejadorCallback callback);

        /// <summary>
        /// Elimina el registro de notificaciones de un cliente.
        /// </summary>
        void Remover(string nombreUsuario);

        /// <summary>
        /// Obtiene el callback asociado a un usuario especifico.
        /// </summary>
        ISalasManejadorCallback ObtenerCallback(string nombreUsuario);

        /// <summary>
        /// Limpia todos los registros de notificaciones.
        /// </summary>
        void Limpiar();

        /// <summary>
        /// Notifica a los integrantes que un nuevo jugador ha ingresado y envia la sala 
        /// actualizada.
        /// </summary>
        void NotificarIngreso(string codigoSala, string nombreUsuario, SalaDTO salaActualizada);

        /// <summary>
        /// Notifica a los integrantes que un jugador ha salido y envia la sala actualizada.
        /// </summary>
        void NotificarSalida(string codigoSala, string nombreUsuario, SalaDTO salaActualizada);

        /// <summary>
        /// Notifica una expulsion especifica al afectado y actualiza a los demas.
        /// </summary>
        void NotificarExpulsion(string codigoSala, string nombreExpulsado, 
            ISalasManejadorCallback callbackExpulsado, SalaDTO salaActualizada);

        /// <summary>
        /// Notifica a todos los integrantes que la sala ha sido cancelada.
        /// </summary>
        void NotificarCancelacion(string codigoSala);
    }
}