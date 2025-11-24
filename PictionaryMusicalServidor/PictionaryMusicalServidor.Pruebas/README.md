# PictionaryMusicalServidor.Pruebas

## Descripción
Este proyecto contiene las pruebas unitarias para el servidor de Pictionary Musical. Las pruebas siguen los mismos patrones establecidos en el proyecto `PictionaryMusicalCliente.Pruebas` para mantener consistencia en todo el código.

## Estructura de Pruebas

### PruebasUtilidades
Pruebas para clases utilitarias que no dependen de base de datos o servicios externos:

- **PruebaTokenGenerador**: Valida la generación de tokens únicos hexadecimales de 32 caracteres
- **PruebaCodigoVerificacionGenerador**: Valida la generación de códigos numéricos de verificación
- **PruebaValidadorNombreUsuario**: Valida las reglas de validación y normalización de nombres de usuario
- **PruebaEntradaComunValidador**: Pruebas exhaustivas para validadores de entrada (correo, contraseña, texto, tokens, códigos)

### PruebasManejadores
Pruebas para los manejadores de servicio (handlers) enfocadas en validación de entrada y manejo de errores:

- **PruebaCodigoVerificacionManejador**: Validación de solicitud, reenvío y confirmación de códigos
- **PruebaInicioSesionManejador**: Validación de credenciales de inicio de sesión
- **PruebaCambioContrasenaManejador**: Validación de recuperación y cambio de contraseña
- **PruebaPerfilManejador**: Validación de consulta y actualización de perfiles
- **PruebaCuentaManejador**: Validación de registro de nuevas cuentas
- **PruebaAmigosManejador**: Validación de suscripción a notificaciones de amistad
- **PruebaClasificacionManejador**: Validación de consulta de clasificaciones
- **PruebaListaAmigosManejador**: Validación de suscripción a lista de amigos

## Estadísticas
- **Total de archivos de prueba**: 13
- **Total de métodos de prueba**: 139
- **Framework**: MSTest

## Estrategia de Pruebas

### Áreas de Enfoque
1. **Validación de Entrada**: null, vacío, espacios en blanco, formato inválido, muy largo/corto
2. **Manejo de Errores**: respuestas de error apropiadas sin propagar excepciones
3. **Casos Límite**: condiciones de frontera y caracteres especiales
4. **Seguridad**: requisitos de contraseña, validación de formato de token

### Convenciones de Nomenclatura
Los nombres de prueba siguen el patrón:
```
Prueba_MetodoAProbar_Condicion_ComportamientoEsperado
```

Ejemplos:
- `Prueba_GenerarToken_DeberiaRetornarTokenValido`
- `Prueba_IniciarSesion_CredencialesNulas_DeberiaLanzarExcepcion`
- `Prueba_ValidarContrasena_ContrasenaMuyCorta_DeberiaRetornarFalse`

### Estructura de Prueba
```csharp
[TestClass]
public class PruebaClase
{
    private ClaseAProbar _instancia;

    [TestInitialize]
    public void Inicializar()
    {
        _instancia = new ClaseAProbar();
    }

    [TestMethod]
    public void Prueba_Metodo_Condicion_Resultado()
    {
        // Arrange (Preparar)
        var entrada = PrepararDatos();

        // Act (Actuar)
        var resultado = _instancia.Metodo(entrada);

        // Assert (Afirmar)
        Assert.AreEqual(esperado, resultado);
    }
}
```

## Limitaciones y Futuras Mejoras

### Pruebas que Requieren Contexto de Base de Datos
Las siguientes clases requieren pruebas de integración con una base de datos de prueba:
- `AmistadServicio` - operaciones CRUD de amistades
- `VerificacionRegistroServicio` - verificación de usuarios/correos existentes
- `RecuperacionCuentaServicio` - búsqueda de usuarios para recuperación
- Flujos completos de autenticación en `InicioSesionManejador`
- Flujos completos de registro en `CuentaManejador`

### Pruebas que Requieren Mocks de SMTP
- `NotificacionCodigosServicio` - envío de correos electrónicos
- `CorreoCodigoVerificacionNotificador` - construcción y envío de correos
- `CorreoInvitacionNotificador` - construcción y envío de invitaciones

### Pruebas que Requieren Callbacks Mock
- Pruebas completas de `AmigosManejador` con callbacks WCF
- Pruebas completas de `ListaAmigosManejador` con callbacks WCF
- Pruebas completas de `SalasManejador` con callbacks WCF

## Ejecutar las Pruebas

### Desde Visual Studio
1. Abrir el proyecto en Visual Studio
2. Ir a Test > Run All Tests
3. Ver resultados en Test Explorer

### Desde Línea de Comandos
```bash
dotnet test PictionaryMusicalServidor.sln --configuration Debug
```

O con MSTest:
```bash
vstest.console.exe PictionaryMusicalServidor.Pruebas.dll
```

## Contribuir

Al agregar nuevas pruebas:
1. Seguir las convenciones de nomenclatura establecidas
2. Organizar las pruebas en regiones lógicas (#region)
3. Incluir comentarios cuando el comportamiento no sea obvio
4. Documentar casos límite importantes
5. Mantener las pruebas independientes entre sí
6. No depender del orden de ejecución de las pruebas

## Referencias
- Proyecto de referencia: `PictionaryMusicalCliente.Pruebas`
- Documentación MSTest: https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest
- Plan de pruebas completo: `/planPruebas.md`
