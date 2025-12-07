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
        private const string EndpointVerificacion = "BasicHttpBinding_ICodigoVerificacionManejador";
        private const string EndpointCuenta = "BasicHttpBinding_ICuentaManejador";
        private const string EndpointCambioPass = "BasicHttpBinding_ICambioContrasenaManejador";
        private const string EndpointClasificacion = "BasicHttpBinding_IClasificacionManejador";
        private const string EndpointInicioSesion = "BasicHttpBinding_IInicioSesionManejador";
        private const string EndpointInvitaciones = "BasicHttpBinding_IInvitacionesManejador1";
        private const string EndpointPerfil = "BasicHttpBinding_IPerfilManejador";
        private const string EndpointReportes = "BasicHttpBinding_IReportesManejador";

        private const string EndpointAmigos = "NetTcpBinding_IAmigosManejador";
        private const string EndpointListaAmigos = "NetTcpBinding_IListaAmigosManejador";
        private const string EndpointSalas = "NetTcpBinding_ISalasManejador";
        private const string EndpointCursoPartida = "NetTcpBinding_ICursoPartidaManejador";

        public PictionaryServidorServicioCodigoVerificacion.ICodigoVerificacionManejador
            CrearClienteVerificacion()
        {
            return new PictionaryServidorServicioCodigoVerificacion
                .CodigoVerificacionManejadorClient(EndpointVerificacion);
        }

        public PictionaryServidorServicioCuenta.ICuentaManejador CrearClienteCuenta()
        {
            return new PictionaryServidorServicioCuenta
                .CuentaManejadorClient(EndpointCuenta);
        }

        public PictionaryServidorServicioCambioContrasena.ICambioContrasenaManejador
            CrearClienteCambioContrasena()
        {
            return new PictionaryServidorServicioCambioContrasena
                .CambioContrasenaManejadorClient(EndpointCambioPass);
        }

        public PictionaryServidorServicioClasificacion.IClasificacionManejador
            CrearClienteClasificacion()
        {
            return new PictionaryServidorServicioClasificacion
                .ClasificacionManejadorClient(EndpointClasificacion);
        }

        public PictionaryServidorServicioInicioSesion.IInicioSesionManejador
            CrearClienteInicioSesion()
        {
            return new PictionaryServidorServicioInicioSesion
                .InicioSesionManejadorClient(EndpointInicioSesion);
        }

        public PictionaryServidorServicioInvitaciones.IInvitacionesManejador
            CrearClienteInvitaciones()
        {
            return new PictionaryServidorServicioInvitaciones
                .InvitacionesManejadorClient(EndpointInvitaciones);
        }

        public PictionaryServidorServicioPerfil.IPerfilManejador CrearClientePerfil()
        {
            return new PictionaryServidorServicioPerfil
                .PerfilManejadorClient(EndpointPerfil);
        }

        public PictionaryServidorServicioReportes.IReportesManejador CrearClienteReportes()
        {
            return new PictionaryServidorServicioReportes
                .ReportesManejadorClient(EndpointReportes);
        }

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