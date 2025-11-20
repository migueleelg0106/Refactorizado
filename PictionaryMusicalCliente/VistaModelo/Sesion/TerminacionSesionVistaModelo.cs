using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Sesiones;
using System;
using System.Windows.Input;

namespace PictionaryMusicalCliente.VistaModelo.Sesion
{
    /// <summary>
    /// Gestiona la logica para la confirmacion del cierre de sesion del usuario.
    /// </summary>
    public class TerminacionSesionVistaModelo : BaseVistaModelo
    {
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
        public TerminacionSesionVistaModelo()
        {
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
            SesionUsuarioActual.CerrarSesion();
            EjecutarCierreSesionYNavegacion?.Invoke();

            OcultarDialogo?.Invoke();
        }

        private void EjecutarCancelar()
        {
            OcultarDialogo?.Invoke();
        }
    }
}