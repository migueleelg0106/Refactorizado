using log4net;
using System;
using System.IO;
using System.ServiceModel;

namespace HostServidor
{
    class Principal
    {
        private static readonly ILog Bitacora = LogManager.GetLogger(typeof(Principal));

        static void Main()
        {
            Directory.CreateDirectory("Logs");

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                Bitacora.Fatal("Excepción no controlada.", (Exception)e.ExceptionObject);

            using (var hostCuenta = new ServiceHost(typeof(Servicios.Servicios.CuentaManejador)))
            using (var hostCodigo = new ServiceHost(typeof(Servicios.Servicios.CodigoVerificacionManejador)))
            using (var hostReenvio = new ServiceHost(typeof(Servicios.Servicios.ReenviarCodigoVerificacionManejador)))
            using (var hostAvatares = new ServiceHost(typeof(Servicios.Servicios.CatalogoAvatares)))
            using (var hostInicioSesion = new ServiceHost(typeof(Servicios.Servicios.InicioSesionManejador)))
            using (var hostCambioContrasena = new ServiceHost(typeof(Servicios.Servicios.CambiarContrasenaManejador)))
            using (var hostClasificacion = new ServiceHost(typeof(Servicios.Servicios.ClasificacionManejador)))
            using (var hostPerfil = new ServiceHost(typeof(Servicios.Servicios.PerfilManejador)))
            {
                try
                {
                    hostCuenta.Open();
                    Bitacora.Info("Servicio Cuenta iniciado.");
                    foreach (var ep in hostCuenta.Description.Endpoints)
                    {
                        Bitacora.Info($"Cuenta -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostCodigo.Open();
                    Bitacora.Info("Servicio Código de Verificación iniciado.");
                    foreach (var ep in hostCodigo.Description.Endpoints)
                    {
                        Bitacora.Info($"Código -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostReenvio.Open();
                    Bitacora.Info("Servicio Reenvío Código iniciado.");
                    foreach (var ep in hostReenvio.Description.Endpoints)
                    {
                        Bitacora.Info($"Reenvío -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostAvatares.Open();
                    Bitacora.Info("Servicio Avatares iniciado.");
                    foreach (var ep in hostAvatares.Description.Endpoints)
                    {
                        Bitacora.Info($"Avatares -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostInicioSesion.Open();
                    Bitacora.Info("Servicio Inicio sesion.");
                    foreach (var ep in hostInicioSesion.Description.Endpoints)
                    {
                        Bitacora.Info($"InicioSesion -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostCambioContrasena.Open();
                    Bitacora.Info("Servicio Cambio contraseña iniciado.");
                    foreach (var ep in hostCambioContrasena.Description.Endpoints)
                    {
                        Bitacora.Info($"CambioContraseña -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostClasificacion.Open();
                    Bitacora.Info("Servicio Clasificación iniciado.");
                    foreach (var ep in hostClasificacion.Description.Endpoints)
                    {
                        Bitacora.Info($"Clasificacion -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostPerfil.Open();
                    Bitacora.Info("Servicio Perfil iniciado.");
                    foreach (var ep in hostPerfil.Description.Endpoints)
                    {
                        Bitacora.Info($"Perfil -> {ep.Address} ({ep.Binding.Name})");
                    }

                    Console.WriteLine("Servicios arriba. ENTER para salir.");
                    Console.ReadLine();
                }
                catch (AddressAccessDeniedException ex)
                {
                    Bitacora.Error("Permisos insuficientes para abrir los puertos.", ex);
                }
                catch (AddressAlreadyInUseException ex)
                {
                    Bitacora.Error("Puerto en uso.", ex);
                }
                catch (TimeoutException ex)
                {
                    Bitacora.Error("Timeout al iniciar el host.", ex);
                }
                catch (CommunicationException ex)
                {
                    Bitacora.Error("Error de comunicación al iniciar el host.", ex);
                }
                finally
                {
                    CerrarFormaSegura(hostAvatares);
                    CerrarFormaSegura(hostReenvio);
                    CerrarFormaSegura(hostCodigo);
                    CerrarFormaSegura(hostCuenta);
                    CerrarFormaSegura(hostInicioSesion);
                    CerrarFormaSegura(hostCambioContrasena);
                    CerrarFormaSegura(hostClasificacion);
                    CerrarFormaSegura(hostPerfil);
                    Bitacora.Info("Host detenido.");
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
                Bitacora.Warn("Cierre no limpio por error de comunicación; abortando.", ex);
                host.Abort();
            }
            catch (TimeoutException ex)
            {
                Bitacora.Warn("Cierre no limpio por tiempo de espera; abortando.", ex);
                host.Abort();
            }
        }
    }
}
