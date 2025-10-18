using System.Linq;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using PictionaryMusicalCliente;

namespace PictionaryMusicalCliente.Servicios.Dialogos
{
    public class SeleccionarAvatarDialogService : ISeleccionarAvatarService
    {
        public Task<ObjetoAvatar> SeleccionarAvatarAsync(string avatarSeleccionadoRutaRelativa = null)
        {
            var ventana = new SeleccionarAvatar();
            var avatares = CatalogoAvataresLocales.ObtenerAvatares();
            var vistaModelo = new SeleccionarAvatarVistaModelo(avatares);
            var finalizacion = new TaskCompletionSource<ObjetoAvatar>();

            if (!string.IsNullOrWhiteSpace(avatarSeleccionadoRutaRelativa))
            {
                vistaModelo.AvatarSeleccionado = vistaModelo.Avatares
                    .FirstOrDefault(a => AvatarHelper.SonRutasEquivalentes(
                        a.RutaRelativa,
                        avatarSeleccionadoRutaRelativa));
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
