using System;
using System.IO;
using System.ServiceModel;
using log4net;
using log4net.Config;

namespace PictionaryMusicalServidor.HostServidor
{
    static class Principal
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Principal));

        static void Main()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));
            Directory.CreateDirectory("Logs");

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                _logger.Fatal("Excepcion no controlada.", (Exception)e.ExceptionObject);
            };

            using (var hostCuenta = new ServiceHost(typeof(Servicios.Servicios.CuentaManejador)))
            using (var hostCodigo = new ServiceHost(typeof(Servicios.Servicios.CodigoVerificacionManejador)))
            using (var hostInicioSesion = new ServiceHost(typeof(Servicios.Servicios.InicioSesionManejador)))
            using (var hostCambioContrasena = new ServiceHost(typeof(Servicios.Servicios.CambioContrasenaManejador)))
            using (var hostClasificacion = new ServiceHost(typeof(Servicios.Servicios.ClasificacionManejador)))
            using (var hostPerfil = new ServiceHost(typeof(Servicios.Servicios.PerfilManejador)))
            using (var hostAmigos = new ServiceHost(typeof(Servicios.Servicios.AmigosManejador)))
            using (var hostListaAmigos = new ServiceHost(typeof(Servicios.Servicios.ListaAmigosManejador)))
            using (var hostSalas = new ServiceHost(typeof(Servicios.Servicios.SalasManejador)))
            using (var hostInvitaciones = new ServiceHost(typeof(Servicios.Servicios.InvitacionesManejador)))
            {
                try
                {
                    hostCuenta.Open();
                    foreach (var endpoint in hostCuenta.Description.Endpoints)
                    {
                        _logger.InfoFormat("Cuenta -> {0} ({1})", endpoint.Address, endpoint.Binding.Name);
                    }

                    hostCodigo.Open();
                    foreach (var endpoint in hostCodigo.Description.Endpoints)
                    {
                        _logger.InfoFormat("Codigo -> {0} ({1})", endpoint.Address, endpoint.Binding.Name);
                    }

                    hostInicioSesion.Open();
                    foreach (var endpoint in hostInicioSesion.Description.Endpoints)
                    {
                        _logger.InfoFormat("InicioSesion -> {0} ({1})", endpoint.Address, endpoint.Binding.Name);
                    }

                    hostCambioContrasena.Open();
                    foreach (var endpoint in hostCambioContrasena.Description.Endpoints)
                    {
                        _logger.InfoFormat("CambioContrasena -> {0} ({1})", endpoint.Address, endpoint.Binding.Name);
                    }

                    hostClasificacion.Open();
                    foreach (var endpoint in hostClasificacion.Description.Endpoints)
                    {
                        _logger.InfoFormat("Clasificacion -> {0} ({1})", endpoint.Address, endpoint.Binding.Name);
                    }

                    hostPerfil.Open();
                    foreach (var endpoint in hostPerfil.Description.Endpoints)
                    {
                        _logger.InfoFormat("Perfil -> {0} ({1})", endpoint.Address, endpoint.Binding.Name);
                    }

                    hostAmigos.Open();
                    foreach (var endpoint in hostAmigos.Description.Endpoints)
                    {
                        _logger.InfoFormat("Amigos -> {0} ({1})", endpoint.Address, endpoint.Binding.Name);
                    }

                    hostListaAmigos.Open();
                    foreach (var endpoint in hostListaAmigos.Description.Endpoints)
                    {
                        _logger.InfoFormat("ListaAmigos -> {0} ({1})", endpoint.Address, endpoint.Binding.Name);
                    }

                    hostSalas.Open();
                    foreach (var endpoint in hostSalas.Description.Endpoints)
                    {
                        _logger.InfoFormat("Salas -> {0} ({1})", endpoint.Address, endpoint.Binding.Name);
                    }

                    hostInvitaciones.Open();
                    foreach (var endpoint in hostInvitaciones.Description.Endpoints)
                    {
                        _logger.InfoFormat("Invitaciones -> {0} ({1})", endpoint.Address, endpoint.Binding.Name);
                    }

                    _logger.Info("Todos los servicios están arriba y escuchando. Presiona ENTER para salir.");
                    Console.ReadLine();
                }
                catch (AddressAccessDeniedException ex)
                {
                    _logger.Error("Permisos insuficientes para abrir los puertos.", ex);
                }
                catch (AddressAlreadyInUseException ex)
                {
                    _logger.Error("Puerto en uso.", ex);
                }
                catch (TimeoutException ex)
                {
                    _logger.Error("Timeout al iniciar el host.", ex);
                }
                catch (CommunicationException ex)
                {
                    _logger.Error("Error de comunicacion al iniciar el host.", ex);
                }
                catch (Exception ex) 
                {
                    _logger.Fatal("Error crítico inesperado al iniciar el servidor.", ex);
                }
                finally
                {
                    CerrarFormaSegura(hostCodigo);
                    CerrarFormaSegura(hostCuenta);
                    CerrarFormaSegura(hostInicioSesion);
                    CerrarFormaSegura(hostCambioContrasena);
                    CerrarFormaSegura(hostClasificacion);
                    CerrarFormaSegura(hostPerfil);
                    CerrarFormaSegura(hostAmigos);
                    CerrarFormaSegura(hostListaAmigos);
                    CerrarFormaSegura(hostSalas);
                    CerrarFormaSegura(hostInvitaciones);
                    _logger.Info("Host detenido.");
                }
            }
        }

        private static void CerrarFormaSegura(ServiceHost host)
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
                _logger.Warn("Cierre no limpio por error de comunicacion; abortando.", ex);
                host.Abort();
            }
            catch (TimeoutException ex)
            {
                _logger.Warn("Cierre no limpio por tiempo de espera; abortando.", ex);
                host.Abort();
            }
        }
    }
}