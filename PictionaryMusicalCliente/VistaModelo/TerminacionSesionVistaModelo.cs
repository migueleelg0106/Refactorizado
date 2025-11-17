using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Sesiones; 
using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace PictionaryMusicalCliente.VistaModelo
{
    public class TerminacionSesionVistaModelo : BaseVistaModelo
    {
        public Action OcultarDialogo { get; set; }

        public TerminacionSesionVistaModelo()
        {
            AceptarComando = new ComandoDelegado(_ => EjecutarAceptar());
            CancelarComando = new ComandoDelegado(_ => EjecutarCancelar());
            CrearVentanaInicioSesion = () => new InicioSesion();
        }

        public ICommand AceptarComando { get; }
        public ICommand CancelarComando { get; }
        public Func<Window> CrearVentanaInicioSesion { get; set; }

        private void EjecutarAceptar()
        {
            SesionUsuarioActual.CerrarSesion();

            var ventanasActivas = Application.Current.Windows.Cast<Window>().ToList();
            var inicioSesion = CrearVentanaInicioSesion?.Invoke() ?? new InicioSesion();
            Application.Current.MainWindow = inicioSesion;
            inicioSesion.Show();

            foreach (var ventana in ventanasActivas.Where(v => v != inicioSesion))
            {
                ventana.Close();
            }

            OcultarDialogo?.Invoke();
        }

        private void EjecutarCancelar()
        {
            OcultarDialogo?.Invoke();
        }
    }
}