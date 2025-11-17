using System;
using System.Threading;

namespace PictionaryMusicalCliente.Pruebas.Utilidades
{
    internal static class StaTestHelper
    {
        public static void Ejecutar(Action accion)
        {
            Ejecutar<object>(() =>
            {
                accion?.Invoke();
                return null;
            });
        }

        public static T Ejecutar<T>(Func<T> accion)
        {
            if (accion == null)
            {
                throw new ArgumentNullException(nameof(accion));
            }

            T resultado = default;
            Exception excepcion = null;

            using (var espera = new ManualResetEventSlim(false))
            {
                Thread hilo = new Thread(() =>
                {
                    try
                    {
                        resultado = accion();
                    }
                    catch (Exception ex)
                    {
                        excepcion = ex;
                    }
                    finally
                    {
                        espera.Set();
                    }
                });

                hilo.SetApartmentState(ApartmentState.STA);
                hilo.Start();
                espera.Wait();
                hilo.Join();
            }

            if (excepcion != null)
            {
                throw excepcion;
            }

            return resultado;
        }
    }
}
