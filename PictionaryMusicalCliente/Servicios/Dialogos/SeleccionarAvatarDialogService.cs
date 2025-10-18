using System.Linq;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using PictionaryMusicalCliente;

namespace PictionaryMusicalCliente.Servicios.Dialogos
{
    public class SeleccionarAvatarDialogService : ISeleccionarAvatarService
    {
        public Task<ObjetoAvatar> SeleccionarAvatarAsync(int? avatarSeleccionadoId = null)
        {
            var ventana = new SeleccionarAvatar();
            var avatares = CatalogoAvataresLocales.ObtenerAvatares();
            var vistaModelo = new SeleccionarAvatarVistaModelo(avatares);
            var finalizacion = new TaskCompletionSource<ObjetoAvatar>();

            if (avatarSeleccionadoId.HasValue)
            {
                vistaModelo.AvatarSeleccionado = vistaModelo.Avatares
                    .FirstOrDefault(a => a.Id == avatarSeleccionadoId.Value);
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

            return finalizacion.Task;
        }
    }
}
