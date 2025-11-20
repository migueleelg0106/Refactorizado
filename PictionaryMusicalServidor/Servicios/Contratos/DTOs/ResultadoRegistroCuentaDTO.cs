using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta el resultado del registro de una nueva cuenta.
    /// </summary>
    [DataContract]
    public class ResultadoRegistroCuentaDTO
    {
        /// <summary>
        /// Indica si el registro fue exitoso.
        /// </summary>
        [DataMember]
        public bool RegistroExitoso { get; set; }

        /// <summary>
        /// Indica si el nombre de usuario ya esta registrado.
        /// </summary>
        [DataMember]
        public bool UsuarioRegistrado { get; set; }

        /// <summary>
        /// Indica si el correo electronico ya esta registrado.
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