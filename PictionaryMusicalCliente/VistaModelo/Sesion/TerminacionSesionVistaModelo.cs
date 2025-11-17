using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Sesiones; 
using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace PictionaryMusicalCliente.VistaModelo.Sesion
{
    public class TerminacionSesionVistaModelo : BaseVistaModelo
    {
        public Action OcultarDialogo { get; set; }
        public Action EjecutarCierreSesionYNavegacion { get; set; }

        public TerminacionSesionVistaModelo()
        {
            AceptarComando = new ComandoDelegado(_ => EjecutarAceptar());
            CancelarComando = new ComandoDelegado(_ => EjecutarCancelar());
        }

        public ICommand AceptarComando { get; }
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