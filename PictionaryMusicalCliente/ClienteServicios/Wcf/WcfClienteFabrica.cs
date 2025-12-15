using System;
using System.ServiceModel;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Implementacion concreta de la fabrica de clientes WCF.
    /// Centraliza la instanciacion de los proxies generados.
    /// </summary>
    public class WcfClienteFabrica : IWcfClienteFabrica
    {
        private const string EndpointVerificacion = 
            "BasicHttpBinding_ICodigoVerificacionManejador";
        private const string EndpointCuenta = 
            "BasicHttpBinding_ICuentaManejador";
        private const string EndpointCambioPass = 
            "BasicHttpBinding_ICambioContrasenaManejador";
        private const string EndpointClasificacion = 
            "BasicHttpBinding_IClasificacionManejador";
        private const string EndpointInicioSesion = 
            "BasicHttpBinding_IInicioSesionManejador";
        private const string EndpointInvitaciones = 
            "BasicHttpBinding_IInvitacionesManejador1";
        private const string EndpointPerfil = 
            "BasicHttpBinding_IPerfilManejador";
        private const string EndpointReportes = 
            "BasicHttpBinding_IReportesManejador";
        private const string EndpointAmigos = 
            "NetTcpBinding_IAmigosManejador";
        private const string EndpointListaAmigos = 
            "NetTcpBinding_IListaAmigosManejador";
        private const string EndpointSalas = 
            "NetTcpBinding_ISalasManejador";
        private const string EndpointCursoPartida = 
            "NetTcpBinding_ICursoPartidaManejador";

        /// <summary>
        /// Crea una instancia del cliente para el servicio de verificación de códigos.
        /// </summary>
        /// <returns>Cliente WCF para la verificación de códigos.</returns>
        public PictionaryServidorServicioCodigoVerificacion.ICodigoVerificacionManejador
            CrearClienteVerificacion()
        {
            return new PictionaryServidorServicioCodigoVerificacion
                .CodigoVerificacionManejadorClient(EndpointVerificacion);
        }

        /// <summary>
        /// Crea una instancia del cliente para el servicio de gestión de cuentas de usuario.
        /// </summary>
        /// <returns>Cliente WCF para la administración de cuentas.</returns>
        public PictionaryServidorServicioCuenta.ICuentaManejador CrearClienteCuenta()
        {
            return new PictionaryServidorServicioCuenta
                .CuentaManejadorClient(EndpointCuenta);
        }

        /// <summary>
        /// Crea una instancia del cliente para el servicio de cambio de contraseña.
        /// </summary>
        /// <returns>Cliente WCF para el cambio de contraseña.</returns>
        public PictionaryServidorServicioCambioContrasena.ICambioContrasenaManejador
            CrearClienteCambioContrasena()
        {
            return new PictionaryServidorServicioCambioContrasena
                .CambioContrasenaManejadorClient(EndpointCambioPass);
        }

        /// <summary>
        /// Crea una instancia del cliente para consultar las clasificaciones (ranking).
        /// </summary>
        /// <returns>Cliente WCF para obtener clasificaciones.</returns>
        public PictionaryServidorServicioClasificacion.IClasificacionManejador
            CrearClienteClasificacion()
        {
            return new PictionaryServidorServicioClasificacion
                .ClasificacionManejadorClient(EndpointClasificacion);
        }

        /// <summary>
        /// Crea una instancia del cliente para el inicio de sesión y autenticación.
        /// </summary>
        /// <returns>Cliente WCF para el manejo de sesiones.</returns>
        public PictionaryServidorServicioInicioSesion.IInicioSesionManejador
            CrearClienteInicioSesion()
        {
            return new PictionaryServidorServicioInicioSesion
                .InicioSesionManejadorClient(EndpointInicioSesion);
        }

        /// <summary>
        /// Crea una instancia del cliente para el manejo de invitaciones por correo.
        /// </summary>
        /// <returns>Cliente WCF para enviar invitaciones.</returns>
        public PictionaryServidorServicioInvitaciones.IInvitacionesManejador
            CrearClienteInvitaciones()
        {
            return new PictionaryServidorServicioInvitaciones
                .InvitacionesManejadorClient(EndpointInvitaciones);
        }

        /// <summary>
        /// Crea una instancia del cliente para la gestión del perfil de usuario.
        /// </summary>
        /// <returns>Cliente WCF para consultar y modificar perfiles.</returns>
        public PictionaryServidorServicioPerfil.IPerfilManejador CrearClientePerfil()
        {
            return new PictionaryServidorServicioPerfil
                .PerfilManejadorClient(EndpointPerfil);
        }

        /// <summary>
        /// Crea una instancia del cliente para el reporte de jugadores.
        /// </summary>
        /// <returns>Cliente WCF para reportar usuarios.</returns>
        public PictionaryServidorServicioReportes.IReportesManejador CrearClienteReportes()
        {
            return new PictionaryServidorServicioReportes
                .ReportesManejadorClient(EndpointReportes);
        }

        /// <summary>
        /// Crea una instancia del cliente para la gestión de amigos (dúplex).
        /// </summary>
        /// <param name="callback">Contexto de instancia para recibir respuestas del servidor.
        /// </param>
        /// <returns>Cliente WCF para gestión de amigos.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si el callback es nulo.</exception>
        public PictionaryServidorServicioAmigos.IAmigosManejador
            CrearClienteAmigos(InstanceContext callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            return new PictionaryServidorServicioAmigos
                .AmigosManejadorClient(callback, EndpointAmigos);
        }

        /// <summary>
        /// Crea una instancia del cliente para consultar la lista de amigos conectados (dúplex).
        /// </summary>
        /// <param name="callback">Contexto de instancia para recibir actualizaciones de estado.
        /// </param>
        /// <returns>Cliente WCF para lista de amigos.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si el callback es nulo.</exception>
        public PictionaryServidorServicioListaAmigos.IListaAmigosManejador
            CrearClienteListaAmigos(InstanceContext callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            return new PictionaryServidorServicioListaAmigos
                .ListaAmigosManejadorClient(callback, EndpointListaAmigos);
        }

        /// <summary>
        /// Crea una instancia del cliente para la gestión de salas de juego (dúplex).
        /// </summary>
        /// <param name="callback">Contexto de instancia para eventos de la sala.</param>
        /// <returns>Cliente WCF para gestión de salas.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si el callback es nulo.</exception>
        public PictionaryServidorServicioSalas.ISalasManejador
            CrearClienteSalas(InstanceContext callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            return new PictionaryServidorServicioSalas
                .SalasManejadorClient(callback, EndpointSalas);
        }

        /// <summary>
        /// Crea una instancia del cliente para la lógica de la partida en curso (dúplex).
        /// </summary>
        /// <param name="callback">Contexto de instancia para eventos del juego en tiempo real.
        /// </param>
        /// <returns>Cliente WCF para el curso de la partida.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si el callback es nulo.</exception>
        public PictionaryServidorServicioCursoPartida.ICursoPartidaManejador
        CrearClienteCursoPartida(InstanceContext callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            return new PictionaryMusicalCliente.PictionaryServidorServicioCursoPartida
                .CursoPartidaManejadorClient(callback, EndpointCursoPartida);
        }
    }
}