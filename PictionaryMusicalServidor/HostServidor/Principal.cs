using System;
using System.IO;
using System.ServiceModel;
using log4net;

namespace HostServidor
{
    class Principal
    {
        private static readonly ILog _bitacora = LogManager.GetLogger(typeof(Principal));

        static void Main()
        {
            Directory.CreateDirectory("Logs");

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                _bitacora.Fatal("Excepcion no controlada.", (Exception)e.ExceptionObject);
            };

            using (var hostCuenta = new ServiceHost(typeof(Servicios.Servicios.CuentaManejador)))
            using (var hostCodigo = new ServiceHost(typeof(Servicios.Servicios.CodigoVerificacionManejador)))
            using (var hostAvatares = new ServiceHost(typeof(Servicios.Servicios.CatalogoAvataresManejador)))
            using (var hostInicioSesion = new ServiceHost(typeof(Servicios.Servicios.InicioSesionManejador)))
            using (var hostCambioContrasena = new ServiceHost(typeof(Servicios.Servicios.CambioContrasenaManejador)))
            using (var hostClasificacion = new ServiceHost(typeof(Servicios.Servicios.ClasificacionManejador)))
            using (var hostPerfil = new ServiceHost(typeof(Servicios.Servicios.PerfilManejador)))
            using (var hostAmigos = new ServiceHost(typeof(Servicios.Servicios.AmigosManejador)))
            using (var hostListaAmigos = new ServiceHost(typeof(Servicios.Servicios.ListaAmigosManejador)))
            {
                try
                {
                    hostCuenta.Open();
                    _bitacora.Info("Servicio Cuenta iniciado.");
                    foreach (var ep in hostCuenta.Description.Endpoints)
                    {
                        _bitacora.Info($"Cuenta -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostCodigo.Open();
                    _bitacora.Info("Servicio Codigo de Verificacion iniciado.");
                    foreach (var ep in hostCodigo.Description.Endpoints)
                    {
                        _bitacora.Info($"Codigo -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostAvatares.Open();
                    _bitacora.Info("Servicio Avatares iniciado.");
                    foreach (var ep in hostAvatares.Description.Endpoints)
                    {
                        _bitacora.Info($"Avatares -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostInicioSesion.Open();
                    _bitacora.Info("Servicio Inicio sesion.");
                    foreach (var ep in hostInicioSesion.Description.Endpoints)
                    {
                        _bitacora.Info($"InicioSesion -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostCambioContrasena.Open();
                    _bitacora.Info("Servicio Cambio contrasena iniciado.");
                    foreach (var ep in hostCambioContrasena.Description.Endpoints)
                    {
                        _bitacora.Info($"CambioContrasena -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostClasificacion.Open();
                    _bitacora.Info("Servicio Clasificacion iniciado.");
                    foreach (var ep in hostClasificacion.Description.Endpoints)
                    {
                        _bitacora.Info($"Clasificacion -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostPerfil.Open();
                    _bitacora.Info("Servicio Perfil iniciado.");
                    foreach (var ep in hostPerfil.Description.Endpoints)
                    {
                        _bitacora.Info($"Perfil -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostAmigos.Open();
                    _bitacora.Info("Servicio Amigos iniciado.");
                    foreach (var ep in hostAmigos.Description.Endpoints)
                    {
                        _bitacora.Info($"Amigos -> {ep.Address} ({ep.Binding.Name})");
                    }

                    hostListaAmigos.Open();
                    _bitacora.Info("Servicio Lista de amigos iniciado.");
                    foreach (var ep in hostListaAmigos.Description.Endpoints)
                    {
                        _bitacora.Info($"ListaAmigos -> {ep.Address} ({ep.Binding.Name})");
                    }

                    Console.WriteLine("Servicios arriba. ENTER para salir.");
                    Console.ReadLine();
                }
                catch (AddressAccessDeniedException ex)
                {
                    _bitacora.Error("Permisos insuficientes para abrir los puertos.", ex);
                }
                catch (AddressAlreadyInUseException ex)
                {
                    _bitacora.Error("Puerto en uso.", ex);
                }
                catch (TimeoutException ex)
                {
                    _bitacora.Error("Timeout al iniciar el host.", ex);
                }
                catch (CommunicationException ex)
                {
                    _bitacora.Error("Error de comunicacion al iniciar el host.", ex);
                }
                finally
                {
                    CerrarFormaSegura(hostAvatares);
                    CerrarFormaSegura(hostCodigo);
                    CerrarFormaSegura(hostCuenta);
                    CerrarFormaSegura(hostInicioSesion);
                    CerrarFormaSegura(hostCambioContrasena);
                    CerrarFormaSegura(hostClasificacion);
                    CerrarFormaSegura(hostPerfil);
                    CerrarFormaSegura(hostAmigos);
                    CerrarFormaSegura(hostListaAmigos);
                    _bitacora.Info("Host detenido.");
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
                _bitacora.Warn("Cierre no limpio por error de comunicacion; abortando.", ex);
                host.Abort();
            }
            catch (TimeoutException ex)
            {
                _bitacora.Warn("Cierre no limpio por tiempo de espera; abortando.", ex);
                host.Abort();
            }
        }
    }
}