using PictionaryMusicalCliente.Comandos;
using System;
using System.Windows.Input;
using log4net;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.VistaModelo.Sesion
{
    /// <summary>
    /// Gestiona la logica para la confirmacion del cierre de sesion del usuario.
    /// </summary>
    public class TerminacionSesionVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IUsuarioAutenticado _usuarioSesion;

        /// <summary>
        /// Accion delegada para cerrar el cuadro de dialogo.
        /// </summary>
        public Action OcultarDialogo { get; set; }

        /// <summary>
        /// Accion delegada para ejecutar la logica de cierre y navegacion a la pantalla de inicio.
        /// </summary>
        public Action EjecutarCierreSesionYNavegacion { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia del ViewModel.
        /// </summary>
        public TerminacionSesionVistaModelo(IUsuarioAutenticado usuarioSesion)
        {
            _usuarioSesion = usuarioSesion 
                ?? throw new ArgumentNullException(nameof(usuarioSesion));

            AceptarComando = new ComandoDelegado(_ => EjecutarAceptar());
            CancelarComando = new ComandoDelegado(_ => EjecutarCancelar());
        }

        /// <summary>
        /// Comando para confirmar el cierre de sesion.
        /// </summary>
        public ICommand AceptarComando { get; }

        /// <summary>
        /// Comando para cancelar la operacion y mantener la sesion activa.
        /// </summary>
        public ICommand CancelarComando { get; }

        private void EjecutarAceptar()
        {
            _logger.Info("Usuario confirmó el cierre de sesión.");
            _usuarioSesion.Limpiar();
            EjecutarCierreSesionYNavegacion?.Invoke();

            OcultarDialogo?.Invoke();
        }

        private void EjecutarCancelar()
        {
            _logger.Info("Usuario canceló el cierre de sesión.");
            OcultarDialogo?.Invoke();
        }
    }
}