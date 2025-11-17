using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Utilidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Perfil
{
    public class CambioContrasenaVistaModelo : BaseVistaModelo
    {
        private readonly string _tokenCodigo;
        private readonly ICambioContrasenaServicio _cambioContrasenaServicio;

        private string _nuevaContrasena;
        private string _confirmacionContrasena;
        private bool _estaProcesando;

        public CambioContrasenaVistaModelo(string tokenCodigo, ICambioContrasenaServicio cambioContrasenaServicio)
        {
            _tokenCodigo = tokenCodigo ?? throw new ArgumentNullException(nameof(tokenCodigo));
            _cambioContrasenaServicio = cambioContrasenaServicio ?? throw new ArgumentNullException(nameof(cambioContrasenaServicio));

            ConfirmarComando = new ComandoAsincrono(async _ =>
            {
                ManejadorSonido.ReproducirClick();
                await ConfirmarAsync();
            }, _ => !EstaProcesando);

            CancelarComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                Cancelar();
            });
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
                    ((IComandoNotificable)ConfirmarComando).NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono ConfirmarComando { get; }
        public ICommand CancelarComando { get; }
        public Action<DTOs.ResultadoOperacionDTO> CambioContrasenaCompletado { get; set; }
        public Action Cancelado { get; set; }
        public Action<IList<string>> MostrarCamposInvalidos { get; set; }

        private async Task ConfirmarAsync()
        {
            MostrarCamposInvalidos?.Invoke(Array.Empty<string>()); 

            var camposInvalidos = ValidarEntradas();
            if (camposInvalidos != null && camposInvalidos.Count > 0)
            {
                ManejadorSonido.ReproducirError();
                MostrarCamposInvalidos?.Invoke(camposInvalidos);
                return; 
            }

            MostrarCamposInvalidos?.Invoke(Array.Empty<string>());
            EstaProcesando = true;

            try
            {
                DTOs.ResultadoOperacionDTO resultadoCambio = await RealizarCambioContrasenaAsync().ConfigureAwait(true);

                if (resultadoCambio != null && resultadoCambio.OperacionExitosa)
                {
                    CambioContrasenaCompletado?.Invoke(resultadoCambio);
                }
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private List<string> ValidarEntradas()
        {
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
                AvisoAyudante.Mostrar(Lang.errorTextoConfirmacionContrasenaRequerida);
                return camposInvalidos;
            }

            DTOs.ResultadoOperacionDTO validacion = ValidacionEntrada.ValidarContrasena(NuevaContrasena);
            if (validacion?.OperacionExitosa != true)
            {
                AvisoAyudante.Mostrar(validacion?.Mensaje ?? Lang.errorTextoContrasenaFormato);
                return new List<string> { nameof(NuevaContrasena) };
            }

            if (!string.Equals(NuevaContrasena, ConfirmacionContrasena, StringComparison.Ordinal))
            {
                AvisoAyudante.Mostrar(Lang.errorTextoContrasenasNoCoinciden);
                return new List<string> { nameof(NuevaContrasena), nameof(ConfirmacionContrasena) };
            }

            return camposInvalidos; 
        }

        private async Task<DTOs.ResultadoOperacionDTO> RealizarCambioContrasenaAsync()
        {
            try
            {
                DTOs.ResultadoOperacionDTO resultado = await _cambioContrasenaServicio
                    .ActualizarContrasenaAsync(_tokenCodigo, NuevaContrasena).ConfigureAwait(true);

                if (resultado == null)
                {
                    ManejadorSonido.ReproducirError();
                    AvisoAyudante.Mostrar(Lang.errorTextoActualizarContrasena);
                    return null; 
                }

                string mensaje = resultado.Mensaje ?? (resultado.OperacionExitosa ? Lang.avisoTextoContrasenaActualizada : Lang.errorTextoActualizarContrasena);
                if (resultado.OperacionExitosa)
                {
                    ManejadorSonido.ReproducirExito();
                }
                else
                {
                    ManejadorSonido.ReproducirError();
                }
                AvisoAyudante.Mostrar(mensaje);
                resultado.Mensaje = mensaje; 

                return resultado; 
            }
            catch (ServicioExcepcion ex)
            {
                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoActualizarContrasena);
                return null; 
            }
        }

        private void Cancelar()
        {
            Cancelado?.Invoke();
        }
    }
}