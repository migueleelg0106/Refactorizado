using System;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System.Windows.Markup;

namespace PictionaryMusicalCliente.ClienteServicios.Dialogos
{
    /// <summary>
    /// Gestiona el dialogo modal para que el usuario seleccione su avatar.
    /// </summary>
    public class SeleccionAvatarDialogoServicio : ISeleccionarAvatarServicio
    {
        private static readonly ILog _logger = 
            LogManager.GetLogger(typeof(SeleccionAvatarDialogoServicio));

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public SeleccionAvatarDialogoServicio()
        {
        }

        /// <summary>
        /// Abre la ventana de seleccion y retorna el avatar elegido por el usuario.
        /// </summary>
        public Task<ObjetoAvatar> SeleccionarAvatarAsync(int idAvatar)
        {
            _logger.Info("Iniciando proceso de selección de avatar.");
            var avatares = CatalogoAvataresLocales.ObtenerAvatares();

            if (avatares == null || avatares.Count == 0)
            {
                _logger.Warn("No se cargaron avatares locales.");
                AvisoAyudante.Mostrar("No se pudieron cargar los avatares.");
                return Task.FromResult<ObjetoAvatar>(null);
            }

            var finalizacion = new TaskCompletionSource<ObjetoAvatar>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var ventana = new SeleccionAvatar();
                    var vistaModelo = new SeleccionAvatarVistaModelo(avatares);

                    if (idAvatar > 0)
                    {
                        vistaModelo.AvatarSeleccionado = 
                        CatalogoAvataresLocales.ObtenerPorId(idAvatar);
                    }

                    vistaModelo.SeleccionConfirmada = avatar =>
                    {
                        _logger.InfoFormat("Avatar seleccionado: ID {0}", avatar?.Id);
                        finalizacion.TrySetResult(avatar);
                    };

                    vistaModelo.CerrarAccion = () => ventana.Close();

                    ventana.DataContext = vistaModelo;

                    ventana.Closed += (_, __) =>
                    {
                        if (!finalizacion.Task.IsCompleted)
                        {
                            finalizacion.TrySetResult(null);
                        }
                    };

                    ventana.ShowDialog();
                }
                catch (XamlParseException ex)
                {
                    _logger.Error("Error XAML al cargar la interfaz de selección de avatar.", ex);
                    finalizacion.TrySetException(
                        new InvalidOperationException(
                            "Error al cargar la interfaz de selección de avatar.",
                            ex));
                }
                catch (InvalidOperationException ex)
                {
                    _logger.Error("Operación inválida al mostrar diálogo de avatar.", ex);
                    finalizacion.TrySetException(ex);
                }
            });

            return finalizacion.Task;
        }
    }
}