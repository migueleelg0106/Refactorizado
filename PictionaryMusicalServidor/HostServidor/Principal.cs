using System;
using System.IO;
using System.ServiceModel;
using log4net;

namespace PictionaryMusicalServidor.HostServidor
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

            using (var hostCuenta = new ServiceHost(typeof(PictionaryMusicalServidor.Servicios.Servicios.CuentaManejador)))
            using (var hostCodigo = new ServiceHost(typeof(PictionaryMusicalServidor.Servicios.Servicios.CodigoVerificacionManejador)))
            using (var hostInicioSesion = new ServiceHost(typeof(PictionaryMusicalServidor.Servicios.Servicios.InicioSesionManejador)))
            using (var hostCambioContrasena = new ServiceHost(typeof(PictionaryMusicalServidor.Servicios.Servicios.CambioContrasenaManejador)))
            using (var hostClasificacion = new ServiceHost(typeof(PictionaryMusicalServidor.Servicios.Servicios.ClasificacionManejador)))
            using (var hostPerfil = new ServiceHost(typeof(PictionaryMusicalServidor.Servicios.Servicios.PerfilManejador)))
            using (var hostAmigos = new ServiceHost(typeof(PictionaryMusicalServidor.Servicios.Servicios.AmigosManejador)))
            using (var hostListaAmigos = new ServiceHost(typeof(PictionaryMusicalServidor.Servicios.Servicios.ListaAmigosManejador)))
            using (var hostSalas = new ServiceHost(typeof(PictionaryMusicalServidor.Servicios.Servicios.SalasManejador)))
            using (var hostInvitaciones = new ServiceHost(typeof(PictionaryMusicalServidor.Servicios.Servicios.InvitacionesManejador)))
            {
                try
                {
                    hostCuenta.Open();
                    _bitacora.Info("Servicio Cuenta iniciado.");
                    foreach (var ep in hostCuenta.Description.Endpoints)
                    {
                        _bitacora.InfoFormat("Cuenta -> {0} ({1})", ep.Address, ep.Binding.Name);
                    }

                    hostCodigo.Open();
                    _bitacora.Info("Servicio Codigo de Verificacion iniciado.");
                    foreach (var ep in hostCodigo.Description.Endpoints)
                    {
                        _bitacora.InfoFormat("Codigo -> {0} ({1})", ep.Address, ep.Binding.Name);
                    }

                    hostInicioSesion.Open();
                    _bitacora.Info("Servicio Inicio sesion.");
                    foreach (var ep in hostInicioSesion.Description.Endpoints)
                    {
                        _bitacora.InfoFormat("InicioSesion -> {0} ({1})", ep.Address, ep.Binding.Name);
                    }

                    hostCambioContrasena.Open();
                    _bitacora.Info("Servicio Cambio contrasena iniciado.");
                    foreach (var ep in hostCambioContrasena.Description.Endpoints)
                    {
                        _bitacora.InfoFormat("CambioContrasena -> {0} ({1})", ep.Address, ep.Binding.Name);
                    }

                    hostClasificacion.Open();
                    _bitacora.Info("Servicio Clasificacion iniciado.");
                    foreach (var ep in hostClasificacion.Description.Endpoints)
                    {
                        _bitacora.InfoFormat("Clasificacion -> {0} ({1})", ep.Address, ep.Binding.Name);
                    }

                    hostPerfil.Open();
                    _bitacora.Info("Servicio Perfil iniciado.");
                    foreach (var ep in hostPerfil.Description.Endpoints)
                    {
                        _bitacora.InfoFormat("Perfil -> {0} ({1})", ep.Address, ep.Binding.Name);
                    }

                    hostAmigos.Open();
                    _bitacora.Info("Servicio Amigos iniciado.");
                    foreach (var ep in hostAmigos.Description.Endpoints)
                    {
                        _bitacora.InfoFormat("Amigos -> {0} ({1})", ep.Address, ep.Binding.Name);
                    }

                    hostListaAmigos.Open();
                    _bitacora.Info("Servicio Lista de amigos iniciado.");
                    foreach (var ep in hostListaAmigos.Description.Endpoints)
                    {
                        _bitacora.InfoFormat("ListaAmigos -> {0} ({1})", ep.Address, ep.Binding.Name);
                    }

                    hostSalas.Open();
                    _bitacora.Info("Servicio Salas iniciado.");
                    foreach (var ep in hostSalas.Description.Endpoints)
                    {
                        _bitacora.InfoFormat("Salas -> {0} ({1})", ep.Address, ep.Binding.Name);
                    }

                    hostInvitaciones.Open();
                    _bitacora.Info("Servicio Invitaciones iniciado.");
                    foreach (var ep in hostInvitaciones.Description.Endpoints)
                    {
                        _bitacora.InfoFormat("Invitaciones -> {0} ({1})", ep.Address, ep.Binding.Name);
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
