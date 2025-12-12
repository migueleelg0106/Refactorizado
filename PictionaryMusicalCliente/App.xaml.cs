using log4net.Config;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Dialogos;
using PictionaryMusicalCliente.ClienteServicios.Idiomas;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Administrador;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Properties;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using PictionaryMusicalCliente.Utilidades.Idiomas;
using PictionaryMusicalCliente.VistaModelo.InicioSesion;
using System;
using System.Globalization;
using System.Windows;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Logica de interaccion para App.xaml. Raiz de composicion de la aplicacion.
    /// Contenedor estatico de servicios principales.
    /// </summary>
    public partial class App : Application
    {
        public static IWcfClienteFabrica WcfFabrica { get; private set; }
        public static IWcfClienteEjecutor WcfEjecutor { get; private set; }
        public static IManejadorErrorServicio ManejadorError { get; private set; }
        public static ILocalizadorServicio Localizador { get; private set; }
        public static ILocalizacionServicio ServicioIdioma { get; private set; }
        public static IVentanaServicio VentanaServicio { get; private set; }
        public static IAvisoServicio AvisoServicio { get; private set; }

        public static ISonidoManejador SonidoManejador { get; private set; }
        public static IMusicaManejador MusicaManejador { get; private set; }
        public static IValidadorEntrada Validador { get; private set; }
        public static INombreInvitadoGenerador GeneradorNombres { get; private set; }
        public static IUsuarioMapeador UsuarioMapeador { get; private set; }

        public static IUsuarioAutenticado UsuarioGlobal { get; private set; }
        public static ICatalogoAvatares CatalogoAvatares { get; private set; }
        public static ICatalogoImagenesPerfil CatalogoImagenes { get; private set; }

        public static IInicioSesionServicio InicioSesionServicio { get; private set; }
        public static ICambioContrasenaServicio CambioContrasenaServicio { get; private set; }
        public static IRecuperacionCuentaServicio RecuperacionCuentaServicio { get; private set; }
        public static IPerfilServicio PerfilServicio { get; private set; }
        public static IClasificacionServicio ClasificacionServicio { get; private set; }
        public static IInvitacionesServicio InvitacionesServicio { get; private set; }
        public static IReportesServicio ReportesServicio { get; private set; }
        public static IListaAmigosServicio ListaAmigosServicio { get; private set; }
        public static IAmigosServicio AmigosServicio { get; private set; }
        public static ISalasServicio SalasServicio { get; private set; }

        public static IVerificacionCodigoDialogoServicio VerificacionCodigoDialogo 
            { get; private set; }

        public static Func<ISalasServicio> FabricaSalas { get; private set; }

        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            XmlConfigurator.Configure();

            InicializarServicios();
            ConfigurarIdioma();

            var inicioSesionVM = new InicioSesionVistaModelo(
                VentanaServicio,
                Localizador,
                InicioSesionServicio,
                CambioContrasenaServicio,
                RecuperacionCuentaServicio,
                ServicioIdioma,
                SonidoManejador,
                AvisoServicio,
                GeneradorNombres,
                UsuarioGlobal,
                FabricaSalas
            );

            VentanaServicio.MostrarVentana(inicioSesionVM);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            MusicaManejador?.Detener();
            MusicaManejador?.Dispose();
            SonidoManejador?.Dispose();
            AmigosServicio?.Dispose();

            base.OnExit(e);
        }

        private void InicializarServicios()
        {
            WcfFabrica = new WcfClienteFabrica();
            WcfEjecutor = new WcfClienteEjecutor();
            Localizador = new LocalizadorServicio();
            ManejadorError = new ManejadorErrorServicio(Localizador);
            ServicioIdioma = new LocalizacionServicio();
            
            AvisoServicio = new AvisoServicio();
            VentanaServicio = new VentanaServicio();
            SonidoManejador = new SonidoManejador();
            MusicaManejador = new MusicaManejador();
            Validador = new ValidadorEntrada();
            GeneradorNombres = new NombreInvitadoGenerador();

            UsuarioGlobal = new UsuarioAutenticado();
            CatalogoAvatares = new CatalogoAvataresLocales();
            CatalogoImagenes = new CatalogoImagenesPerfilLocales();
            UsuarioMapeador = new UsuarioMapeador(UsuarioGlobal);

            InicioSesionServicio = new InicioSesionServicio(
                WcfEjecutor, WcfFabrica, ManejadorError, UsuarioMapeador, Localizador);

            CambioContrasenaServicio = new CambioContrasenaServicio(
                WcfEjecutor, WcfFabrica, ManejadorError, Localizador);

            PerfilServicio = new PerfilServicio(
                WcfEjecutor, WcfFabrica, ManejadorError);

            ClasificacionServicio = new ClasificacionServicio(
                WcfEjecutor, WcfFabrica, ManejadorError);

            InvitacionesServicio = new InvitacionesServicio(
                WcfEjecutor, WcfFabrica, ManejadorError, Localizador);

            ReportesServicio = new ReportesServicio(
                WcfEjecutor, WcfFabrica, ManejadorError, Localizador);

            ListaAmigosServicio = new ListaAmigosServicio(ManejadorError, WcfFabrica);

            AmigosServicio = new AmigosServicio(
                new SolicitudesAmistadAdministrador(), ManejadorError, WcfFabrica);

            SalasServicio = new SalasServicio(WcfFabrica, ManejadorError);
            FabricaSalas = () => new SalasServicio(WcfFabrica, ManejadorError);

            FabricaSalas = () => new SalasServicio(WcfFabrica, ManejadorError);

            VerificacionCodigoDialogo = new VerificacionCodigoDialogoServicio();

            RecuperacionCuentaServicio = new RecuperacionCuentaDialogoServicio(
                VerificacionCodigoDialogo, AvisoServicio,
                Validador, SonidoManejador, Localizador);
        }

        private void ConfigurarIdioma()
        {
            try
            {
                ServicioIdioma.EstablecerIdioma(Settings.Default.idiomaCodigo);
            }
            catch (CultureNotFoundException)
            {
                ServicioIdioma.EstablecerCultura(CultureInfo.CurrentUICulture);
            }
            this.Resources.Add("Localizacion", new LocalizacionContexto(ServicioIdioma));
        }
    }
}