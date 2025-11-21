using Microsoft.VisualStudio.TestTools.UnitTesting;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;

namespace PictionaryMusicalServidor.Pruebas
{
    [TestClass]
    public class CorreoCodigoVerificacionNotificadorPruebas
    {
        [TestMethod]
        public void ConstruirCuerpoMensaje_DeberiaUsarTraduccionIngles()
        {
            string cuerpo = CorreoCodigoVerificacionNotificador.ConstruirCuerpoMensaje("Alex", "123456", "en-US");

            StringAssert.Contains(cuerpo, "Hello Alex,");
            StringAssert.Contains(cuerpo, "Your verification code is:");
            StringAssert.Contains(cuerpo, "If you did not request this code");
        }

        [TestMethod]
        public void ConstruirCuerpoMensaje_DeberiaUsarEspanolPorDefecto()
        {
            string cuerpo = CorreoCodigoVerificacionNotificador.ConstruirCuerpoMensaje("Lucia", "654321", null);

            StringAssert.Contains(cuerpo, "Hola Lucia,");
            StringAssert.Contains(cuerpo, "Tu código de verificación es:");
            StringAssert.Contains(cuerpo, "Si no solicitaste este código");
        }
    }

    [TestClass]
    public class CorreoInvitacionNotificadorPruebas
    {
        [TestMethod]
        public void ConstruirCuerpoMensajeInvitacion_DeberiaUsarTraduccionIngles()
        {
            string cuerpo = CorreoInvitacionNotificador.ConstruirCuerpoMensaje("ABCD", "Mariana", "en-US");

            StringAssert.Contains(cuerpo, "You have been invited to a Musical Pictionary game.");
            StringAssert.Contains(cuerpo, "Mariana has invited you to their room.");
            StringAssert.Contains(cuerpo, "Use the following code to join:");
        }

        [TestMethod]
        public void ConstruirCuerpoMensajeInvitacion_DeberiaUsarEspanolPorDefecto()
        {
            string cuerpo = CorreoInvitacionNotificador.ConstruirCuerpoMensaje("WXYZ", "Carlos", "es-MX");

            StringAssert.Contains(cuerpo, "Has sido invitado a una partida de Pictionary Musical.");
            StringAssert.Contains(cuerpo, "Carlos te ha invitado a su sala.");
            StringAssert.Contains(cuerpo, "Utiliza el siguiente código para unirte:");
        }
    }
}
