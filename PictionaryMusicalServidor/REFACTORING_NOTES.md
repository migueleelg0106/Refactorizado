# Notas de Refactorización - AmigosManejador y AmistadServicio

## Resumen de Cambios

Este documento describe la refactorización realizada en las clases `AmigosManejador` y `AmistadServicio` para permitir pruebas unitarias aisladas mediante inyección de dependencias.

## Refactorización Realizada

### 1. AmistadServicio
**Antes:** Clase estática con métodos estáticos que usaban `ContextoFactory.CrearContexto()` directamente.

**Después:**
- Convertida a clase instanciable que implementa `IAmistadServicio`
- Recibe `IContextoFactory` a través del constructor
- Todos los métodos ahora son de instancia
- Permite inyectar un mock de la factoría en las pruebas

**Cambios específicos:**
```csharp
// Antes
internal static class AmistadServicio
{
    public static void CrearSolicitud(int emisorId, int receptorId)
    {
        using (var contexto = ContextoFactory.CrearContexto())
        {
            // ...
        }
    }
}

// Después
public class AmistadServicio : IAmistadServicio
{
    private readonly IContextoFactory _contextoFactory;
    
    public AmistadServicio(IContextoFactory contextoFactory)
    {
        _contextoFactory = contextoFactory;
    }
    
    public void CrearSolicitud(int emisorId, int receptorId)
    {
        using (var contexto = _contextoFactory.CrearContexto())
        {
            // ...
        }
    }
}
```

### 2. ContextoFactory
**Antes:** Clase estática con método estático.

**Después:**
- Convertida a clase instanciable que implementa `IContextoFactory`
- Método `CrearContexto()` ahora es de instancia
- Permite mockear la creación de contextos en pruebas

### 3. AmigosManejador
**Antes:** Instanciaba directamente `ContextoFactory` y llamaba métodos estáticos de `AmistadServicio`.

**Después:**
- Recibe `IContextoFactory` y `IAmistadServicio` a través del constructor
- Constructor por defecto mantiene compatibilidad con WCF
- Constructor adicional permite inyección de dependencias para pruebas
- Usa las instancias inyectadas en lugar de llamadas estáticas

**Cambios específicos:**
```csharp
// Añadido
private readonly IContextoFactory _contextoFactory;
private readonly IAmistadServicio _amistadServicio;

public AmigosManejador() : this(new ContextoFactory(), new AmistadServicio(new ContextoFactory()))
{
}

public AmigosManejador(IContextoFactory contextoFactory, IAmistadServicio amistadServicio)
{
    _contextoFactory = contextoFactory;
    _amistadServicio = amistadServicio;
}
```

## Interfaces Creadas

### IContextoFactory
```csharp
public interface IContextoFactory
{
    BaseDatosPruebaEntities CrearContexto();
}
```

### IAmistadServicio
```csharp
public interface IAmistadServicio
{
    List<SolicitudAmistadDTO> ObtenerSolicitudesPendientesDTO(int usuarioId);
    void CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId);
    void AceptarSolicitud(int usuarioEmisorId, int usuarioReceptorId);
    Amigo EliminarAmistad(int usuarioAId, int usuarioBId);
    List<AmigoDTO> ObtenerAmigosDTO(int usuarioId);
}
```

## Pruebas Unitarias Creadas

### PruebaAmistadServicio
Pruebas de lógica de negocio pura:

**CrearSolicitud:**
- ✅ Error: Usuario intenta enviarse solicitud a sí mismo
- ⚠️ Error: Ya existe una relación (estructura básica, requiere más refactorización)
- ⚠️ Éxito: Se agrega la solicitud correctamente (estructura básica)

**AceptarSolicitud:**
- ✅ Error: La solicitud no existe
- ✅ Error: El usuario no es el receptor correcto
- ✅ Error: La solicitud ya estaba aceptada
- ⚠️ Éxito: El estado cambia a aceptado (estructura básica)

**EliminarAmistad:**
- ✅ Error: Mismo usuario
- ✅ Error: Relación no existe
- ⚠️ Éxito: Eliminación correcta (estructura básica)

### PruebaAmigosManejador
Pruebas del servicio WCF:

**Suscribir:**
- ✅ Error: Nombre de usuario nulo
- ✅ Error: Nombre de usuario vacío
- ⚠️ Error: Usuario no encontrado (requiere mock de repositorio interno)
- ⚠️ Éxito: Usuario se normaliza correctamente (muy complejo, requiere refactorización adicional)

**EnviarSolicitudAmistad:**
- ⚠️ Error: Emisor no existe (estructura básica)
- ⚠️ Error: Receptor no existe (estructura básica)
- ✅ Error: Fallo de base de datos simulado
- ⚠️ Éxito: Solicitud enviada y servicio interno llamado (estructura básica)

**ResponderSolicitudAmistad:**
- ⚠️ Éxito: Aceptación correcta (estructura básica)
- ⚠️ Error: Usuarios no existen (estructura básica)

**EliminarAmigo:**
- ⚠️ Éxito: Eliminación correcta y notificación (estructura básica)
- ⚠️ Error: Usuarios no existen (estructura básica)

**CancelarSuscripcion:**
- ✅ Error: Nombre de usuario nulo

## Limitaciones y Trabajo Futuro

### Refactorización Adicional Necesaria

Para implementar completamente todas las pruebas, se requiere:

1. **Inyección de IUsuarioRepositorio en AmigosManejador:**
   - Actualmente, `AmigosManejador` crea instancias de `UsuarioRepositorio` internamente
   - Debería recibir `IUsuarioRepositorio` como dependencia para permitir mocking completo

2. **Abstracción de componentes estáticos:**
   - `ManejadorCallback` y `NotificadorAmigos` son estáticos
   - Difícil de mockear para pruebas
   - Requerirían interfaces y refactorización para pruebas completas

3. **Callbacks de WCF:**
   - `ManejadorCallback.ObtenerCallbackActual()` depende de `OperationContext.Current`
   - Muy complejo de mockear en pruebas unitarias
   - Consideraría refactorizar con abstracción de callback provider

4. **ListaAmigosManejador:**
   - Llamadas estáticas a `ListaAmigosManejador.NotificarCambioAmistad()`
   - Requeriría inyección de dependencia o event bus para testing completo

### Estado de las Pruebas

- ✅ **Completas:** Pruebas que validan excepciones y casos simples
- ⚠️ **Estructura Básica:** Pruebas que tienen la estructura pero requieren refactorización adicional del código de producción para funcionar completamente
- Las pruebas marcadas con ⚠️ incluyen comentarios explicando qué refactorización adicional se necesita

### Próximos Pasos Recomendados

1. Inyectar `IUsuarioRepositorio` en `AmigosManejador`
2. Crear abstracción para `ManejadorCallback` y `NotificadorAmigos`
3. Implementar un patrón de repositorio o Unit of Work para facilitar el testing
4. Considerar usar un framework de IoC (como Unity o Autofac) para WCF
5. Completar las pruebas marcadas con ⚠️ después de las refactorizaciones

## Ventajas del Enfoque Actual

1. **Compatibilidad con WCF:** El constructor por defecto mantiene el comportamiento existente
2. **Testabilidad:** Las interfaces permiten mockear dependencias
3. **Separación de Responsabilidades:** AmistadServicio ahora es claramente una capa de lógica de negocio
4. **Inversión de Dependencias:** Cumple con el principio SOLID
5. **Sin Cambios Breaking:** El código existente sigue funcionando sin modificaciones

## Uso en Pruebas

```csharp
// Ejemplo de uso en pruebas
[TestMethod]
public void MiPrueba()
{
    // Arrange
    var mockContextoFactory = new Mock<IContextoFactory>();
    var mockAmistadServicio = new Mock<IAmistadServicio>();
    
    // Configurar comportamiento esperado
    mockAmistadServicio.Setup(s => s.CrearSolicitud(1, 2)).Verifiable();
    
    // Crear instancia con dependencias mockeadas
    var manejador = new AmigosManejador(
        mockContextoFactory.Object, 
        mockAmistadServicio.Object
    );
    
    // Act & Assert
    // ...
}
```

## Conclusión

La refactorización establece una base sólida para pruebas unitarias, aunque algunas pruebas completas requerirían refactorización adicional del código de producción. El enfoque es incremental y no rompe el código existente, permitiendo mejoras graduales en la testabilidad del sistema.
