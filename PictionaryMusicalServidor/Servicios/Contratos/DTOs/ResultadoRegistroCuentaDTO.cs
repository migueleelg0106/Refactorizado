using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para el resultado de un registro de cuenta.
    /// Contiene informacion sobre el exito o fallo del registro y posibles conflictos.
    /// </summary>
    [DataContract]
    public class ResultadoRegistroCuentaDTO
    {
        /// <summary>
        /// Indica si el registro de la cuenta fue exitoso.
        /// </summary>
        [DataMember]
        public bool RegistroExitoso { get; set; }

        /// <summary>
        /// Indica si el nombre de usuario ya esta registrado en el sistema.
        /// </summary>
        [DataMember]
        public bool UsuarioRegistrado { get; set; }

        /// <summary>
        /// Indica si el correo electronico ya esta registrado en el sistema.
        /// </summary>
        [DataMember]
        public bool CorreoRegistrado { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado del registro.
        /// </summary>
        [DataMember]
        public string Mensaje { get; set; }
    }
}