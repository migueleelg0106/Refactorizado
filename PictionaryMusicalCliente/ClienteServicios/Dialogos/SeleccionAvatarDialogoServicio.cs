using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using System.Windows.Markup;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Vista;
using PictionaryMusicalCliente.Utilidades.Abstracciones;

namespace PictionaryMusicalCliente.ClienteServicios.Dialogos
{
    /// <summary>
    /// Gestiona el dialogo modal para que el usuario seleccione su avatar.
    /// </summary>
    public class SeleccionAvatarDialogoServicio : ISeleccionarAvatarServicio
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(SeleccionAvatarDialogoServicio));
        private readonly IAvisoServicio _avisoServicio;
        private readonly ICatalogoAvatares _catalogoAvatares;
        private readonly ISonidoManejador _sonidoManejador;

        public SeleccionAvatarDialogoServicio(
            IAvisoServicio avisoServicio,
            ICatalogoAvatares catalogoAvatares,
            ISonidoManejador sonidoManejador)
        {
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));
            _catalogoAvatares = catalogoAvatares ??
                throw new ArgumentNullException(nameof(catalogoAvatares));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
        }

        /// <summary>
        /// Abre la ventana de seleccion y retorna el avatar elegido por el usuario.
        /// </summary>
        public Task<ObjetoAvatar> SeleccionarAvatarAsync(int idAvatar)
        {
            var avatares = ObtenerAvataresLocales();
            if (avatares == null || avatares.Count == 0)
            {
                return ManejarErrorCargaAvatares();
            }

            var finalizacion = new TaskCompletionSource<ObjetoAvatar>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    MostrarVentanaAvatar(avatares, idAvatar, finalizacion);
                }
                catch (XamlParseException ex)
                {
                    ManejarErrorXaml(ex, finalizacion);
                }
                catch (InvalidOperationException ex)
                {
                    ManejarErrorInvalido(ex, finalizacion);
                }
            });

            return finalizacion.Task;
        }

        private IList<ObjetoAvatar> ObtenerAvataresLocales()
        {
            return (IList<ObjetoAvatar>)_catalogoAvatares.ObtenerAvatares();
        }

        private Task<ObjetoAvatar> ManejarErrorCargaAvatares()
        {
            _logger.Warn("No se cargaron avatares locales.");
            _avisoServicio.Mostrar(Lang.errorTextoNoCargaronAvatares);
            return Task.FromResult<ObjetoAvatar>(null);
        }

        private void MostrarVentanaAvatar(
            IList<ObjetoAvatar> avatares,
            int idAvatarPreseleccionado,
            TaskCompletionSource<ObjetoAvatar> finalizacion)
        {
            var ventana = new SeleccionAvatar();
            var vistaModelo = new SeleccionAvatarVistaModelo(avatares, _avisoServicio,
                _sonidoManejador);

            ConfigurarPreseleccion(vistaModelo, idAvatarPreseleccionado);
            ConfigurarEventosViewModel(vistaModelo, ventana, finalizacion);
            ConfigurarEventosVentana(ventana, finalizacion);

            ventana.DataContext = vistaModelo;
            ventana.ShowDialog();
        }

        private void ConfigurarPreseleccion(
            SeleccionAvatarVistaModelo vistaModelo,
            int idAvatar)
        {
            if (idAvatar > 0)
            {
                vistaModelo.AvatarSeleccionado =
                    _catalogoAvatares.ObtenerPorId(idAvatar);
            }
        }

        private void ConfigurarEventosViewModel(
            SeleccionAvatarVistaModelo vistaModelo,
            Window ventana,
            TaskCompletionSource<ObjetoAvatar> finalizacion)
        {
            vistaModelo.SeleccionConfirmada = avatar =>
            {
                finalizacion.TrySetResult(avatar);
            };

            vistaModelo.CerrarAccion = () => ventana.Close();
        }

        private void ConfigurarEventosVentana(
            Window ventana,
            TaskCompletionSource<ObjetoAvatar> finalizacion)
        {
            ventana.Closed += (_, __) =>
            {
                if (!finalizacion.Task.IsCompleted)
                {
                    finalizacion.TrySetResult(null);
                }
            };
        }

        private void ManejarErrorXaml(
            Exception ex,
            TaskCompletionSource<ObjetoAvatar> finalizacion)
        {
            _logger.Error("Error XAML al cargar la interfaz de seleccion de avatar.", ex);
            finalizacion.TrySetException(
                new InvalidOperationException(
                    "Error al cargar la interfaz de seleccion de avatar.",
                    ex));
        }

        private void ManejarErrorInvalido(
            Exception ex,
            TaskCompletionSource<ObjetoAvatar> finalizacion)
        {
            _logger.Error("Operacion invalida al mostrar dialogo de avatar.", ex);
            finalizacion.TrySetException(ex);
        }
    }
}