using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Sesion;
using System;
using System.Linq;
using System.Windows.Input;

namespace PictionaryMusicalCliente.VistaModelo.Ajustes
{
    /// <summary>
    /// ViewModel para la ventana de configuracion global de la aplicacion.
    /// </summary>
    public class AjustesVistaModelo : BaseVistaModelo
    {
        /// <summary>
        /// Inicializa el ViewModel para configuracion global de la aplicacion.
        /// </summary>
        /// <param name="ventana">Servicio para gestionar ventanas.</param>
        /// <param name="localizador">Servicio de localizacion.</param>
        public AjustesVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador)
            : base(ventana, localizador)
        {
            ConfirmarComando = new ComandoDelegado(_ => EjecutarConfirmar());
            CerrarSesionComando = new ComandoDelegado(_ => EjecutarCerrarSesion());
        }

        /// <summary>
        /// Obtiene o establece el volumen global de la musica.
        /// </summary>
        public double Volumen
        {
            get => App.MusicaManejador.Volumen;
            set
            {
                if (Math.Abs(App.MusicaManejador.Volumen - value) > 0.0001)
                {
                    App.MusicaManejador.Volumen = value;
                    NotificarCambio(nameof(Volumen));
                }
            }
        }

        /// <summary>
        /// Indica si los efectos de sonido están silenciados en la aplicación.
        /// </summary>
        public bool SonidosSilenciados
        {
            get => App.SonidoManejador.Silenciado;
            set
            {
                if (App.SonidoManejador.Silenciado != value)
                {
                    App.SonidoManejador.Silenciado = value;
                    NotificarCambio(nameof(SonidosSilenciados));
                }
            }
        }

        /// <summary>
        /// Comando para guardar los cambios y cerrar.
        /// </summary>
        public ICommand ConfirmarComando { get; }

        /// <summary>
        /// Comando para solicitar el cierre de sesion.
        /// </summary>
        public ICommand CerrarSesionComando { get; }

        private void EjecutarConfirmar()
        {
            _ventana.CerrarVentana(this);
        }

        private void EjecutarCerrarSesion()
        {
            var terminacionSesionVM = new TerminacionSesionVistaModelo(
                App.VentanaServicio,
                App.Localizador,
                App.UsuarioGlobal);
            terminacionSesionVM.EjecutarCierreSesionYNavegacion = () =>
            {
                NavegarAInicioSesion();
            };
            _ventana.MostrarVentanaDialogo(terminacionSesionVM);
        }

        private void NavegarAInicioSesion()
        {
            App.MusicaManejador.Detener();

            var inicioSesionVM = new InicioSesion.InicioSesionVistaModelo(
                _ventana,
                _localizador,
                App.InicioSesionServicio,
                App.CambioContrasenaServicio,
                App.RecuperacionCuentaServicio,
                App.ServicioIdioma,
                App.SonidoManejador,
                App.AvisoServicio,
                App.GeneradorNombres,
                App.UsuarioGlobal,
                App.FabricaSalas);

            _ventana.MostrarVentana(inicioSesionVM);
            
            CerrarVentanaPrincipal();
            _ventana.CerrarVentana(this);
        }

        private void CerrarVentanaPrincipal()
        {
            var ventanaPrincipal = System.Windows.Application.Current.Windows
                .OfType<System.Windows.Window>()
                .FirstOrDefault(v => v.DataContext is VentanaPrincipal.VentanaPrincipalVistaModelo);
            
            ventanaPrincipal?.Close();
        }
    }
}