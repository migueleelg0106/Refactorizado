using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class CambioContrasenaVistaModelo : BaseVistaModelo
    {
        private readonly string _tokenCodigo;
        private readonly ICambioContrasenaService _cambioContrasenaService;

        private string _nuevaContrasena;
        private string _confirmacionContrasena;
        private bool _estaProcesando;

        public CambioContrasenaVistaModelo(string tokenCodigo, ICambioContrasenaService cambioContrasenaService)
        {
            _tokenCodigo = tokenCodigo ?? throw new ArgumentNullException(nameof(tokenCodigo));
            _cambioContrasenaService = cambioContrasenaService ?? throw new ArgumentNullException(nameof(cambioContrasenaService));

            ConfirmarCommand = new ComandoAsincrono(_ => ConfirmarAsync(), _ => !EstaProcesando);
            CancelarCommand = new ComandoDelegado(Cancelar);
        }

        public string NuevaContrasena
        {
            get => _nuevaContrasena;
            set => EstablecerPropiedad(ref _nuevaContrasena, value);
        }

        public string ConfirmacionContrasena
        {
            get => _confirmacionContrasena;
            set => EstablecerPropiedad(ref _confirmacionContrasena, value);
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ((IComandoNotificable)ConfirmarCommand).NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono ConfirmarCommand { get; }

        public ICommand CancelarCommand { get; }

        public Action<ResultadoOperacion> CambioContrasenaCompletado { get; set; }

        public Action Cancelado { get; set; }

        public Action<IList<string>> MostrarCamposInvalidos { get; set; }

        private async Task ConfirmarAsync()
        {
            MostrarCamposInvalidos?.Invoke(Array.Empty<string>());

            var camposInvalidos = new List<string>();

            if (string.IsNullOrWhiteSpace(NuevaContrasena))
            {
                camposInvalidos.Add(nameof(NuevaContrasena));
            }

            if (string.IsNullOrWhiteSpace(ConfirmacionContrasena))
            {
                camposInvalidos.Add(nameof(ConfirmacionContrasena));
            }

            if (camposInvalidos.Count > 0)
            {
                AvisoHelper.Mostrar(Lang.errorTextoConfirmacionContrasenaRequerida);
                MostrarCamposInvalidos?.Invoke(camposInvalidos);
                return;
            }

            ResultadoOperacion validacion = ValidacionEntradaHelper.ValidarContrasena(NuevaContrasena);
            if (!validacion.Exito)
            {
                AvisoHelper.Mostrar(validacion.Mensaje ?? Lang.errorTextoContrasenaFormato);
                MostrarCamposInvalidos?.Invoke(new[] { nameof(NuevaContrasena) });
                return;
            }

            if (!string.Equals(NuevaContrasena, ConfirmacionContrasena, StringComparison.Ordinal))
            {
                AvisoHelper.Mostrar(Lang.errorTextoContrasenasNoCoinciden);
                MostrarCamposInvalidos?.Invoke(new[] { nameof(NuevaContrasena), nameof(ConfirmacionContrasena) });
                return;
            }

            MostrarCamposInvalidos?.Invoke(Array.Empty<string>());

            EstaProcesando = true;

            try
            {
                ResultadoOperacion resultado = await _cambioContrasenaService
                    .ActualizarContrasenaAsync(_tokenCodigo, NuevaContrasena).ConfigureAwait(true);

                if (resultado == null)
                {
                    AvisoHelper.Mostrar(Lang.errorTextoActualizarContrasena);
                    return;
                }

                if (!resultado.Exito)
                {
                    AvisoHelper.Mostrar(resultado.Mensaje ?? Lang.errorTextoActualizarContrasena);
                    return;
                }

                string mensaje = resultado.Mensaje ?? Lang.avisoTextoContrasenaActualizada;
                AvisoHelper.Mostrar(mensaje);
                CambioContrasenaCompletado?.Invoke(ResultadoOperacion.Exitoso(mensaje));
            }
            catch (ServicioException ex)
            {
                AvisoHelper.Mostrar(ex.Message ?? Lang.errorTextoActualizarContrasena);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private void Cancelar()
        {
            Cancelado?.Invoke();
        }
    }
}
