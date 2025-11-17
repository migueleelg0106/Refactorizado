using System;
using System.Threading.Tasks;
using System.Windows;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System.Windows.Markup;

namespace PictionaryMusicalCliente.ClienteServicios.Dialogos
{
    public class SeleccionAvatarDialogoServicio : ISeleccionarAvatarServicio
    {
        public SeleccionAvatarDialogoServicio()
        {
        }

        public Task<ObjetoAvatar> SeleccionarAvatarAsync(int idAvatar)
        {
            var avatares = CatalogoAvataresLocales.ObtenerAvatares();

            if (avatares == null || avatares.Count == 0)
            {
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
                        vistaModelo.AvatarSeleccionado = CatalogoAvataresLocales.ObtenerPorId(idAvatar);
                    }

                    vistaModelo.SeleccionConfirmada = avatar =>
                    {
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
                    finalizacion.TrySetException(new InvalidOperationException("Error al cargar la interfaz de selección de avatar.", ex));
                }
                catch (InvalidOperationException ex)
                {
                    finalizacion.TrySetException(ex);
                }
            });

            return finalizacion.Task;
        }
    }
}