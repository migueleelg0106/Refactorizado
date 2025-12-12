using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using log4net;
using System;
using System.Windows.Input;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;

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

        public Action EjecutarCierreSesionYNavegacion { get; set; }

        public TerminacionSesionVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            IUsuarioAutenticado usuarioSesion)
            : base(ventana, localizador)
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
            _logger.Info("Usuario confirmo el cierre de sesion.");
            _usuarioSesion.Limpiar();
            EjecutarCierreSesionYNavegacion?.Invoke();
            _ventana.CerrarVentana(this);
        }

        private void EjecutarCancelar()
        {
            _logger.Info("Usuario cancelo el cierre de sesion.");
            _ventana.CerrarVentana(this);
        }
    }
}