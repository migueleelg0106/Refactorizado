using System;
using System.ServiceModel;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para ejecutar operaciones en clientes WCF gestionando su ciclo de vida.
    /// </summary>
    public interface IWcfClienteEjecutor
    {
        /// <summary>
        /// Ejecuta una operacion asincrona asegurando el cierre o aborto del canal WCF.
        /// </summary>
        /// <typeparam name="TClient">Tipo del cliente que implementa ICommunicationObject.
        /// </typeparam>
        /// <typeparam name="TResult">Tipo del resultado esperado.</typeparam>
        /// <param name="cliente">Instancia del cliente WCF.</param>
        /// <param name="operacion">Funcion asincrona a ejecutar.</param>
        /// <returns>El resultado de la operacion.</returns>
        Task<TResult> EjecutarAsincronoAsync<TClient, TResult>(
            TClient cliente,
            Func<TClient, Task<TResult>> operacion)
            where TClient : class;
    }

    /// <summary>
    /// Define el contrato para la creacion abstracta de clientes WCF.
    /// </summary>
    public interface IWcfClienteFabrica
    {
        /// <summary>
        /// Crea una instancia del cliente para el manejo de codigos de verificacion.
        /// </summary>
        /// <returns>Cliente configurado.</returns>
        PictionaryServidorServicioCodigoVerificacion.ICodigoVerificacionManejador
            CrearClienteVerificacion();

        /// <summary>
        /// Crea una instancia del cliente para el manejo de cuentas.
        /// </summary>
        /// <returns>Cliente configurado.</returns>
        PictionaryServidorServicioCuenta.ICuentaManejador CrearClienteCuenta();

        /// <summary>
        /// Crea una instancia del cliente para el manejo de cambio de contrasena.
        /// </summary>
        /// <returns>Cliente configurado.</returns>
        PictionaryServidorServicioCambioContrasena.ICambioContrasenaManejador
            CrearClienteCambioContrasena();

        /// <summary>
        /// Crea una instancia del cliente para el manejo de la Clasificacion.
        /// </summary>
        /// <returns>Cliente configurado.</returns>
        PictionaryServidorServicioClasificacion.IClasificacionManejador
            CrearClienteClasificacion();

        /// <summary>
        /// Crea una instancia del cliente para el manejo del inicio de sesion.
        /// </summary>
        /// <returns>Cliente configurado.</returns>
        PictionaryServidorServicioInicioSesion.IInicioSesionManejador
            CrearClienteInicioSesion();

        /// <summary>
        /// Crea una instancia del cliente para el manejo de invitaciones.
        /// </summary>
        /// <returns>Cliente configurado.</returns>
        PictionaryServidorServicioInvitaciones.IInvitacionesManejador
            CrearClienteInvitaciones();

        /// <summary>
        /// Crea una instancia del cliente para el manejo del perfil.
        /// </summary>
        /// <returns>Cliente configurado.</returns>
        PictionaryServidorServicioPerfil.IPerfilManejador
            CrearClientePerfil();

        /// <summary>
        /// Crea una instancia del cliente para el manejo de los reportes.
        /// </summary>
        /// <returns>Cliente configurado.</returns>
        PictionaryServidorServicioReportes.IReportesManejador
            CrearClienteReportes();

        /// <summary>
        /// Crea una instancia del cliente para el manejo de los amigos.
        /// </summary>
        /// <returns>Cliente configurado.</returns>
        PictionaryServidorServicioAmigos.IAmigosManejador
            CrearClienteAmigos(InstanceContext callback);

        /// <summary>
        /// Crea una instancia del cliente para el manejo de la lista de amigos.
        /// </summary>
        /// <returns>Cliente configurado.</returns>
        PictionaryServidorServicioListaAmigos.IListaAmigosManejador
            CrearClienteListaAmigos(InstanceContext callback);

        /// <summary>
        /// Crea una instancia del cliente para el manejo de las salas.
        /// </summary>
        /// <returns>Cliente configurado.</returns>
        PictionaryServidorServicioSalas.ISalasManejador
            CrearClienteSalas(InstanceContext callback);

        /// <summary>
        /// Crea una instancia del cliente para el manejo de las salas.
        /// </summary>
        /// <returns>Cliente configurado.</returns>
        PictionaryServidorServicioCursoPartida.ICursoPartidaManejador
            CrearClienteCursoPartida(InstanceContext callback);
    }

    /// <summary>
    /// Define las operaciones relacionadas con la solicitud y validacion de codigos.
    /// </summary>
    public interface IVerificacionCodigoServicio
    {
        /// <summary>
        /// Solicita un codigo de registro para una nueva cuenta.
        /// </summary>
        /// <param name="solicitud">DTO con la informacion de la cuenta.</param>
        /// <returns>Resultado de la solicitud.</returns>
        Task<DTOs.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(
            DTOs.NuevaCuentaDTO solicitud);

        /// <summary>
        /// Solicita un codigo de recuperacion de cuenta.
        /// </summary>
        /// <param name="identificador">Correo o usuario.</param>
        /// <returns>Resultado de la solicitud.</returns>
        Task<DTOs.ResultadoSolicitudRecuperacionDTO> SolicitarCodigoRecuperacionAsync(
            string identificador);

        /// <summary>
        /// Valida el codigo de registro ingresado.
        /// </summary>
        /// <param name="tokenCodigo">Token del codigo.</param>
        /// <param name="codigoIngresado">Codigo numerico.</param>
        /// <returns>Resultado del registro.</returns>
        Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(
            string tokenCodigo,
            string codigoIngresado);

        /// <summary>
        /// Valida el codigo de recuperacion ingresado.
        /// </summary>
        /// <param name="tokenCodigo">Token del codigo.</param>
        /// <param name="codigoIngresado">Codigo numerico.</param>
        /// <returns>Resultado de la operacion.</returns>
        Task<DTOs.ResultadoOperacionDTO> ConfirmarCodigoRecuperacionAsync(
            string tokenCodigo,
            string codigoIngresado);

        /// <summary>
        /// Reenvia el codigo de verificacion de registro.
        /// </summary>
        /// <param name="tokenCodigo">Token previo.</param>
        /// <returns>Resultado de la solicitud.</returns>
        Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo);

        /// <summary>
        /// Reenvia el codigo de recuperacion de contrasena.
        /// </summary>
        /// <param name="tokenCodigo">Token previo.</param>
        /// <returns>Resultado de la solicitud.</returns>
        Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRecuperacionAsync(
            string tokenCodigo);
    }
}