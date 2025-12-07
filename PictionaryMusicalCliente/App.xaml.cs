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
using PictionaryMusicalCliente.Vista;
using System;
using System.Globalization;
using System.Windows;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Logica de interaccion para App.xaml. Raiz de composicion de la aplicacion.
    /// </summary>
    public partial class App : Application
    {
        private IUsuarioAutenticado _usuarioGlobal;

        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            log4net.Config.XmlConfigurator.Configure();

            IWcfClienteFabrica fabricaWcf = new WcfClienteFabrica();
            IWcfClienteEjecutor ejecutorWcf = new WcfClienteEjecutor();
            ISonidoManejador sonidoManejador = new SonidoManejador();
            IMusicaManejador musicaManejador = new MusicaManejador();
            IValidadorEntrada validador = new ValidadorEntrada();
            INombreInvitadoGenerador generadorNombres = new NombreInvitadoGenerador();

            _usuarioGlobal = new UsuarioAutenticado();
            ICatalogoAvatares catalogoAvatares = new CatalogoAvataresLocales();
            ICatalogoImagenesPerfil catalogoImagenes = new CatalogoImagenesPerfilLocales();

            ILocalizadorServicio localizador = new LocalizadorServicio(); 
            ILocalizacionServicio servicioIdioma = new LocalizacionServicio(); 
            IManejadorErrorServicio manejadorError = new ManejadorErrorServicio(localizador);
            IUsuarioMapeador usuarioMapeador = new UsuarioMapeador(_usuarioGlobal);
            IAvisoServicio avisoServicio = new AvisoServicio();

            try
            {
                servicioIdioma.EstablecerIdioma(Settings.Default.idiomaCodigo);
            }
            catch (CultureNotFoundException)
            {
                servicioIdioma.EstablecerCultura(CultureInfo.CurrentUICulture);
            }
            this.Resources.Add("Localizacion", new LocalizacionContexto(servicioIdioma));

            IInicioSesionServicio inicioSesionServicio = new InicioSesionServicio(
                ejecutorWcf, fabricaWcf, manejadorError, usuarioMapeador, localizador);

            ICambioContrasenaServicio cambioPassServicio = new CambioContrasenaServicio(
                ejecutorWcf, fabricaWcf, manejadorError, localizador);

            IVerificacionCodigoDialogoServicio verifCodigoDialogo =
                new VerificacionCodigoDialogoServicio();

            IRecuperacionCuentaServicio recupCuentaDialogo =
                new RecuperacionCuentaDialogoServicio(verifCodigoDialogo, avisoServicio, 
                validador, sonidoManejador, localizador);

            IPerfilServicio perfilServicio = new PerfilServicio(
                ejecutorWcf, fabricaWcf, manejadorError);

            IClasificacionServicio clasificacionServicio = new ClasificacionServicio(
                ejecutorWcf, fabricaWcf, manejadorError);

            IInvitacionesServicio invitacionesServicio = new InvitacionesServicio(
                ejecutorWcf, fabricaWcf, manejadorError, localizador);

            IReportesServicio reportesServicio = new ReportesServicio(
                ejecutorWcf, fabricaWcf, manejadorError, localizador);

            IListaAmigosServicio listaAmigosServicio = new ListaAmigosServicio(manejadorError, fabricaWcf);

            IAmigosServicio amigosServicio = new AmigosServicio(
                new SolicitudesAmistadAdministrador(), manejadorError, fabricaWcf);

            Func<ISalasServicio> fabricaSalas = () =>
                new SalasServicio(fabricaWcf, manejadorError);

            var ventanaInicio = new InicioSesion(
                musicaManejador,
                inicioSesionServicio,
                cambioPassServicio,
                recupCuentaDialogo,
                servicioIdioma,
                fabricaSalas,
                fabricaWcf,
                ejecutorWcf,
                manejadorError,
                localizador,
                avisoServicio,
                catalogoAvatares,
                sonidoManejador,
                validador,
                _usuarioGlobal,
                catalogoImagenes,
                verifCodigoDialogo,
                generadorNombres,
                perfilServicio,
                clasificacionServicio,
                invitacionesServicio,
                reportesServicio,
                listaAmigosServicio,
                amigosServicio
            );

            ventanaInicio.Show();
        }
    }
}