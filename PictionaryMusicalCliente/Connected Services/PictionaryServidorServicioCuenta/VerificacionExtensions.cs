// Archivo complementario generado manualmente para extender las operaciones de verificación.
#pragma warning disable 1591
namespace PictionaryMusicalCliente.PictionaryServidorServicioCuenta
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Threading.Tasks;

    [DebuggerStepThroughAttribute()]
    [GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [DataContractAttribute(Name = "ResultadoSolicitudCodigoDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
    [SerializableAttribute()]
    public partial class ResultadoSolicitudCodigoDTO : object, IExtensibleDataObject, INotifyPropertyChanged
    {
        [NonSerializedAttribute()]
        private ExtensionDataObject extensionDataField;

        [OptionalFieldAttribute()]
        private bool CodigoEnviadoField;

        [OptionalFieldAttribute()]
        private string MensajeField;

        [OptionalFieldAttribute()]
        private string TokenCodigoField;

        [OptionalFieldAttribute()]
        private bool CorreoYaRegistradoField;

        [OptionalFieldAttribute()]
        private bool UsuarioYaRegistradoField;

        [BrowsableAttribute(false)]
        public ExtensionDataObject ExtensionData
        {
            get { return extensionDataField; }
            set { extensionDataField = value; }
        }

        [DataMemberAttribute()]
        public bool CodigoEnviado
        {
            get { return CodigoEnviadoField; }
            set
            {
                if (!CodigoEnviadoField.Equals(value))
                {
                    CodigoEnviadoField = value;
                    RaisePropertyChanged("CodigoEnviado");
                }
            }
        }

        [DataMemberAttribute()]
        public string Mensaje
        {
            get { return MensajeField; }
            set
            {
                if (!object.ReferenceEquals(MensajeField, value))
                {
                    MensajeField = value;
                    RaisePropertyChanged("Mensaje");
                }
            }
        }

        [DataMemberAttribute()]
        public string TokenCodigo
        {
            get { return TokenCodigoField; }
            set
            {
                if (!object.ReferenceEquals(TokenCodigoField, value))
                {
                    TokenCodigoField = value;
                    RaisePropertyChanged("TokenCodigo");
                }
            }
        }

        [DataMemberAttribute()]
        public bool CorreoYaRegistrado
        {
            get { return CorreoYaRegistradoField; }
            set
            {
                if (!CorreoYaRegistradoField.Equals(value))
                {
                    CorreoYaRegistradoField = value;
                    RaisePropertyChanged("CorreoYaRegistrado");
                }
            }
        }

        [DataMemberAttribute()]
        public bool UsuarioYaRegistrado
        {
            get { return UsuarioYaRegistradoField; }
            set
            {
                if (!UsuarioYaRegistradoField.Equals(value))
                {
                    UsuarioYaRegistradoField = value;
                    RaisePropertyChanged("UsuarioYaRegistrado");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [DebuggerStepThroughAttribute()]
    [GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [DataContractAttribute(Name = "ReenviarCodigoVerificacionDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
    [SerializableAttribute()]
    public partial class ReenviarCodigoVerificacionDTO : object, IExtensibleDataObject, INotifyPropertyChanged
    {
        [NonSerializedAttribute()]
        private ExtensionDataObject extensionDataField;

        [OptionalFieldAttribute()]
        private string TokenCodigoField;

        [BrowsableAttribute(false)]
        public ExtensionDataObject ExtensionData
        {
            get { return extensionDataField; }
            set { extensionDataField = value; }
        }

        [DataMemberAttribute()]
        public string TokenCodigo
        {
            get { return TokenCodigoField; }
            set
            {
                if (!object.ReferenceEquals(TokenCodigoField, value))
                {
                    TokenCodigoField = value;
                    RaisePropertyChanged("TokenCodigo");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [DebuggerStepThroughAttribute()]
    [GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [DataContractAttribute(Name = "ConfirmarCodigoVerificacionDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
    [SerializableAttribute()]
    public partial class ConfirmarCodigoVerificacionDTO : object, IExtensibleDataObject, INotifyPropertyChanged
    {
        [NonSerializedAttribute()]
        private ExtensionDataObject extensionDataField;

        [OptionalFieldAttribute()]
        private string TokenCodigoField;

        [OptionalFieldAttribute()]
        private string CodigoIngresadoField;

        [BrowsableAttribute(false)]
        public ExtensionDataObject ExtensionData
        {
            get { return extensionDataField; }
            set { extensionDataField = value; }
        }

        [DataMemberAttribute()]
        public string TokenCodigo
        {
            get { return TokenCodigoField; }
            set
            {
                if (!object.ReferenceEquals(TokenCodigoField, value))
                {
                    TokenCodigoField = value;
                    RaisePropertyChanged("TokenCodigo");
                }
            }
        }

        [DataMemberAttribute()]
        public string CodigoIngresado
        {
            get { return CodigoIngresadoField; }
            set
            {
                if (!object.ReferenceEquals(CodigoIngresadoField, value))
                {
                    CodigoIngresadoField = value;
                    RaisePropertyChanged("CodigoIngresado");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial interface ICuentaManejador
    {
        [OperationContractAttribute(Action = "http://tempuri.org/ICuentaManejador/SolicitarCodigoVerificacion", ReplyAction = "http://tempuri.org/ICuentaManejador/SolicitarCodigoVerificacionResponse")]
        ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta);

        [OperationContractAttribute(Action = "http://tempuri.org/ICuentaManejador/SolicitarCodigoVerificacion", ReplyAction = "http://tempuri.org/ICuentaManejador/SolicitarCodigoVerificacionResponse")]
        Task<ResultadoSolicitudCodigoDTO> SolicitarCodigoVerificacionAsync(NuevaCuentaDTO nuevaCuenta);

        [OperationContractAttribute(Action = "http://tempuri.org/ICuentaManejador/ReenviarCodigoVerificacion", ReplyAction = "http://tempuri.org/ICuentaManejador/ReenviarCodigoVerificacionResponse")]
        ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenviarCodigoVerificacionDTO solicitud);

        [OperationContractAttribute(Action = "http://tempuri.org/ICuentaManejador/ReenviarCodigoVerificacion", ReplyAction = "http://tempuri.org/ICuentaManejador/ReenviarCodigoVerificacionResponse")]
        Task<ResultadoSolicitudCodigoDTO> ReenviarCodigoVerificacionAsync(ReenviarCodigoVerificacionDTO solicitud);

        [OperationContractAttribute(Action = "http://tempuri.org/ICuentaManejador/ConfirmarCodigoVerificacion", ReplyAction = "http://tempuri.org/ICuentaManejador/ConfirmarCodigoVerificacionResponse")]
        ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmarCodigoVerificacionDTO confirmacion);

        [OperationContractAttribute(Action = "http://tempuri.org/ICuentaManejador/ConfirmarCodigoVerificacion", ReplyAction = "http://tempuri.org/ICuentaManejador/ConfirmarCodigoVerificacionResponse")]
        Task<ResultadoRegistroCuentaDTO> ConfirmarCodigoVerificacionAsync(ConfirmarCodigoVerificacionDTO confirmacion);
    }

    public partial class CuentaManejadorClient
    {
        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            return base.Channel.SolicitarCodigoVerificacion(nuevaCuenta);
        }

        public Task<ResultadoSolicitudCodigoDTO> SolicitarCodigoVerificacionAsync(NuevaCuentaDTO nuevaCuenta)
        {
            return base.Channel.SolicitarCodigoVerificacionAsync(nuevaCuenta);
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenviarCodigoVerificacionDTO solicitud)
        {
            return base.Channel.ReenviarCodigoVerificacion(solicitud);
        }

        public Task<ResultadoSolicitudCodigoDTO> ReenviarCodigoVerificacionAsync(ReenviarCodigoVerificacionDTO solicitud)
        {
            return base.Channel.ReenviarCodigoVerificacionAsync(solicitud);
        }

        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmarCodigoVerificacionDTO confirmacion)
        {
            return base.Channel.ConfirmarCodigoVerificacion(confirmacion);
        }

        public Task<ResultadoRegistroCuentaDTO> ConfirmarCodigoVerificacionAsync(ConfirmarCodigoVerificacionDTO confirmacion)
        {
            return base.Channel.ConfirmarCodigoVerificacionAsync(confirmacion);
        }
    }
}
#pragma warning restore 1591
