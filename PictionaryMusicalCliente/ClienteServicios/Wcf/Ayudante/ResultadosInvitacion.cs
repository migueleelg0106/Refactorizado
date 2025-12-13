using PictionaryMusicalCliente.VistaModelo.Amigos;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante
{
    /// <summary>
    /// Resultado estandarizado de las invitaciones por correo.
    /// </summary>
    public class InvitacionCorreoResultado
    {
        /// <summary>
        /// Indica si la operacion de invitacion fue exitosa.
        /// </summary>
        public bool Exitoso { get; private set; }

        /// <summary>
        /// Obtiene el mensaje descriptivo del resultado de la operacion.
        /// </summary>
        public string Mensaje { get; private set; }

        private InvitacionCorreoResultado(bool exitoso, string mensaje)
        {
            Exitoso = exitoso;
            Mensaje = mensaje;
        }

        /// <summary>
        /// Crea una instancia de resultado exitoso.
        /// </summary>
        /// <param name="mensaje">Mensaje de exito asociado.</param>
        /// <returns>Una nueva instancia indicando exito.</returns>
        public static InvitacionCorreoResultado Exito(string mensaje)
        {
            return new InvitacionCorreoResultado(true, mensaje);
        }

        /// <summary>
        /// Crea una instancia de resultado fallido.
        /// </summary>
        /// <param name="mensaje">Mensaje de error asociado.</param>
        /// <returns>Una nueva instancia indicando fallo.</returns>
        public static InvitacionCorreoResultado Fallo(string mensaje)
        {
            return new InvitacionCorreoResultado(false, mensaje);
        }
    }

    /// <summary>
    /// Resultado estandarizado al preparar invitaciones de amigos.
    /// </summary>
    public class InvitacionAmigosResultado
    {
        /// <summary>
        /// Indica si la preparacion de la invitacion fue exitosa.
        /// </summary>
        public bool Exitoso { get; private set; }

        /// <summary>
        /// Obtiene el mensaje de error en caso de fallo, o vacio en caso de exito.
        /// </summary>
        public string Mensaje { get; private set; }

        /// <summary>
        /// Obtiene la vista modelo preparada para la invitacion de amigos.
        /// </summary>
        public InvitarAmigosVistaModelo VistaModelo { get; private set; }

        private InvitacionAmigosResultado(
            bool exitoso,
            string mensaje,
            InvitarAmigosVistaModelo vistaModelo)
        {
            Exitoso = exitoso;
            Mensaje = mensaje;
            VistaModelo = vistaModelo;
        }

        /// <summary>
        /// Crea una instancia de resultado exitoso con el ViewModel preparado.
        /// </summary>
        /// <param name="vistaModelo">ViewModel de invitacion de amigos inicializado.</param>
        /// <returns>Una nueva instancia indicando exito.</returns>
        public static InvitacionAmigosResultado Exito(InvitarAmigosVistaModelo vistaModelo)
        {
            return new InvitacionAmigosResultado(true, string.Empty, vistaModelo);
        }

        /// <summary>
        /// Crea una instancia de resultado fallido.
        /// </summary>
        /// <param name="mensaje">Mensaje de error asociado.</param>
        /// <returns>Una nueva instancia indicando fallo y sin ViewModel.</returns>
        public static InvitacionAmigosResultado Fallo(string mensaje)
        {
            return new InvitacionAmigosResultado(false, mensaje, null);
        }
    }
}