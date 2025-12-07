using log4net;
using log4net.Config;
using PictionaryMusicalServidor.Servicios.Servicios;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;

namespace PictionaryMusicalServidor.HostServidor
{
    /// <summary>
    /// Clase principal que gestiona el ciclo de vida del servidor y los servicios WCF.
    /// </summary>
    static class Principal
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Principal));
        private static readonly List<ServiceHost> _listaHosts = new List<ServiceHost>();

        /// <summary>
        /// Punto de entrada principal de la aplicacion.
        /// </summary>
        static void Main()
        {
            ConfigurarLogging();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                _logger.Fatal("Excepcion no controlada en el dominio de la aplicacion.", 
                    (Exception)e.ExceptionObject);
            };

            try
            {
                InicializarHosts();
                AbrirServicios();

                _logger.Info(
                    "Todos los servicios estan arriba y escuchando. Presiona ENTER para salir.");
                Console.ReadLine();
            }
            catch (AddressAccessDeniedException ex)
            {
                _logger.Error("Permisos insuficientes para abrir los puertos de red.", ex);
            }
            catch (AddressAlreadyInUseException ex)
            {
                _logger.Error("Uno o mas puertos ya estan en uso por otra aplicacion.", ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Tiempo de espera agotado al iniciar los servicios.", ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicacion al iniciar el host WCF.", ex);
            }
            catch (Exception ex)
            {
                _logger.Fatal("Error critico inesperado al iniciar el servidor.", ex);
            }
            finally
            {
                CerrarServicios();
                _logger.Info("El servidor se ha detenido.");
            }
        }

        private static void ConfigurarLogging()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));
            Directory.CreateDirectory("Logs");
        }

        private static void InicializarHosts()
        {
            var tiposServicios = new List<Type>
            {
                typeof(CuentaManejador),
                typeof(CodigoVerificacionManejador),
                typeof(InicioSesionManejador),
                typeof(CambioContrasenaManejador),
                typeof(ClasificacionManejador),
                typeof(PerfilManejador),
                typeof(AmigosManejador),
                typeof(ListaAmigosManejador),
                typeof(SalasManejador),
                typeof(InvitacionesManejador),
                typeof(CursoPartidaManejador),
                typeof(ChatManejador),
                typeof(ReportesManejador)
            };

            foreach (var tipo in tiposServicios)
            {
                _listaHosts.Add(new ServiceHost(tipo));
            }
        }

        private static void AbrirServicios()
        {
            foreach (var host in _listaHosts)
            {
                host.Open();

                string nombreServicio = host.Description?.ServiceType?.Name ??
                    host.GetType().Name;

                _logger.InfoFormat(
                    "Servicio {0} levantado correctamente.",
                    nombreServicio);
            }

            _logger.Info("Servicios iniciados correctamente.");
        }

        private static void CerrarServicios()
        {
            foreach (var host in _listaHosts)
            {
                CerrarHostIndividual(host);
            }

            _listaHosts.Clear();
        }

        private static void CerrarHostIndividual(ServiceHost host)
        {
            if (host == null)
            {
                return;
            }

            try
            {
                if (host.State != CommunicationState.Closed)
                {
                    host.Close();
                }
            }
            catch (CommunicationException ex)
            {
                _logger.Warn("Cierre no limpio por error de comunicacion; abortando host.", ex);
                host.Abort();
            }
            catch (TimeoutException ex)
            {
                _logger.Warn("Cierre no limpio por tiempo de espera; abortando host.", ex);
                host.Abort();
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al intentar cerrar un host.", ex);
                host.Abort();
            }
        }
    }
}