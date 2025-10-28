using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente.Servicios.Dialogos
{
    public class SeleccionAvatarDialogoServicio : ISeleccionarAvatarServicio
    {
        private readonly IAvatarServicio _avatarServicio;

        public SeleccionAvatarDialogoServicio(IAvatarServicio avatarServicio)
        {
            _avatarServicio = avatarServicio ?? throw new ArgumentNullException(nameof(avatarServicio));
        }

        public async Task<ObjetoAvatar> SeleccionarAvatarAsync(string avatarSeleccionadoRutaRelativa = null)
        {
            (IReadOnlyList<ObjetoAvatar> avatares, string mensajeError, bool mostrarMensaje) = await ObtenerAvataresAsync()
                .ConfigureAwait(true);

            if (mostrarMensaje && !string.IsNullOrWhiteSpace(mensajeError))
            {
                AvisoAyudante.Mostrar(mensajeError);
            }

            if (avatares == null || avatares.Count == 0)
            {
                return null;
            }

            var ventana = new SeleccionAvatar();
            var vistaModelo = new SeleccionAvatarVistaModelo(avatares);
            var finalizacion = new TaskCompletionSource<ObjetoAvatar>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (!string.IsNullOrWhiteSpace(avatarSeleccionadoRutaRelativa))
            {
                vistaModelo.AvatarSeleccionado = vistaModelo.Avatares
                    .FirstOrDefault(a => AvatarAyudante.SonRutasEquivalentes(
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

            return await finalizacion.Task.ConfigureAwait(true);
        }

        private async Task<(IReadOnlyList<ObjetoAvatar> Avatares, string MensajeError, bool MostrarMensaje)> ObtenerAvataresAsync()
        {
            string mensajeError = null;

            try
            {
                IReadOnlyList<ObjetoAvatar> avatares = await _avatarServicio.ObtenerCatalogoAsync()
                    .ConfigureAwait(true);

                if (avatares != null && avatares.Count > 0)
                {
                    AvatarAyudante.ActualizarCatalogo(avatares);
                    return (avatares, null, false);
                }
            }
            catch (ExcepcionServicio ex)
            {
                mensajeError = ex.Message ?? Lang.errorTextoServidorInformacionAvatar;
            }

            IReadOnlyList<ObjetoAvatar> locales = AvatarAyudante.ObtenerAvatares();

            if (locales == null || locales.Count == 0)
            {
                return (Array.Empty<ObjetoAvatar>(), mensajeError ?? Lang.errorTextoServidorInformacionAvatar, true);
            }

            return (locales, mensajeError, false);
        }
    }
}
