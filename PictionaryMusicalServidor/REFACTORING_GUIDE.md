# Guía de Refactorización: AmigosManejador y AmistadServicio

## Resumen de Cambios

Este documento describe las refactorizaciones realizadas a las clases `AmigosManejador` y `AmistadServicio` para habilitar pruebas unitarias mediante inyección de dependencias.

## Cambios Realizados

### 1. Interfaces Creadas

#### IContextoFactory
```csharp
public interface IContextoFactory
{
    BaseDatosPruebaEntities1 CrearContexto();
}
```

**Propósito:** Permite inyectar y simular (mock) la creación de contextos de base de datos en las pruebas.

#### IAmistadServicio
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

**Propósito:** Define el contrato del servicio de lógica de negocio de amistades, permitiendo su inyección y simulación.

### 2. Clases Refactorizadas

#### ContextoFactory
**Antes:**
```csharp
internal static class ContextoFactory
{
    public static BaseDatosPruebaEntities1 CrearContexto() { ... }
}
```

**Después:**
```csharp
public class ContextoFactory : IContextoFactory
{
    public BaseDatosPruebaEntities1 CrearContexto() { ... }
}
```

**Cambio:** Convertida de clase estática a clase instanciable que implementa `IContextoFactory`.

#### AmistadServicio
**Antes:**
```csharp
internal static class AmistadServicio
{
    public static void CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId) 
    {
        using (var contexto = ContextoFactory.CrearContexto()) { ... }
    }
    // ... otros métodos estáticos
}
```

**Después:**
```csharp
public class AmistadServicio : IAmistadServicio
{
    private readonly IContextoFactory _contextoFactory;

    public AmistadServicio(IContextoFactory contextoFactory)
    {
        _contextoFactory = contextoFactory ?? throw new ArgumentNullException(nameof(contextoFactory));
    }

    public void CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId)
    {
        using (var contexto = _contextoFactory.CrearContexto()) { ... }
    }
    // ... otros métodos de instancia
}
```

**Cambios:**
- Convertida de clase estática a clase instanciable
- Implementa la interfaz `IAmistadServicio`
- Recibe `IContextoFactory` por inyección de dependencias en el constructor
- Todos los métodos convertidos de estáticos a de instancia

#### AmigosManejador
**Antes:**
```csharp
public class AmigosManejador : IAmigosManejador
{
    public void EnviarSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor)
    {
        using (var contexto = ContextoFactory.CrearContexto())
        {
            // ...
            AmistadServicio.CrearSolicitud(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario);
        }
    }
}
```

**Después:**
```csharp
public class AmigosManejador : IAmigosManejador
{
    private readonly IContextoFactory _contextoFactory;
    private readonly IAmistadServicio _amistadServicio;

    public AmigosManejador(IContextoFactory contextoFactory, IAmistadServicio amistadServicio)
    {
        _contextoFactory = contextoFactory ?? throw new ArgumentNullException(nameof(contextoFactory));
        _amistadServicio = amistadServicio ?? throw new ArgumentNullException(nameof(amistadServicio));
    }

    public void EnviarSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor)
    {
        using (var contexto = _contextoFactory.CrearContexto())
        {
            // ...
            _amistadServicio.CrearSolicitud(usuarioEmisor.idUsuario, usuarioReceptor.idUsuario);
        }
    }
}
```

**Cambios:**
- Recibe `IContextoFactory` y `IAmistadServicio` por inyección de dependencias
- Usa las instancias inyectadas en lugar de llamadas estáticas

## Uso en Producción

### Inicialización de Servicios

```csharp
// Crear las dependencias
var contextoFactory = new ContextoFactory();
var amistadServicio = new AmistadServicio(contextoFactory);

// Crear el manejador con inyección de dependencias
var amigosManejador = new AmigosManejador(contextoFactory, amistadServicio);

// Usar el servicio normalmente
amigosManejador.EnviarSolicitudAmistad("usuario1", "usuario2");
```

### Configuración en WCF Service Host

```csharp
// En el método de configuración del host
var contextoFactory = new ContextoFactory();
var amistadServicio = new AmistadServicio(contextoFactory);
var amigosManejador = new AmigosManejador(contextoFactory, amistadServicio);

serviceHost.AddServiceEndpoint(
    typeof(IAmigosManejador),
    binding,
    address,
    amigosManejador  // Usar instancia configurada
);
```

## Pruebas Unitarias

### Ejemplo: Probar AmigosManejador

```csharp
[TestClass]
public class PruebaAmigosManejador
{
    private Mock<IContextoFactory> _mockContextoFactory;
    private Mock<IAmistadServicio> _mockAmistadServicio;
    private AmigosManejador _manejador;

    [TestInitialize]
    public void Inicializar()
    {
        _mockContextoFactory = new Mock<IContextoFactory>();
        _mockAmistadServicio = new Mock<IAmistadServicio>();
        _manejador = new AmigosManejador(_mockContextoFactory.Object, _mockAmistadServicio.Object);
    }

    [TestMethod]
    [ExpectedException(typeof(FaultException))]
    public void Prueba_EnviarSolicitudAmistad_DeberiaLanzarExcepcionConNombreNulo()
    {
        // Act
        _manejador.EnviarSolicitudAmistad(null, "receptor");
    }
}
```

### Ejemplo: Probar AmistadServicio

```csharp
[TestClass]
public class PruebaAmistadServicio
{
    private Mock<IContextoFactory> _mockContextoFactory;
    private AmistadServicio _servicio;

    [TestInitialize]
    public void Inicializar()
    {
        _mockContextoFactory = new Mock<IContextoFactory>();
        _servicio = new AmistadServicio(_mockContextoFactory.Object);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Prueba_CrearSolicitud_DeberiaLanzarExcepcionConAutoSolicitud()
    {
        // Act
        _servicio.CrearSolicitud(1, 1);
    }
}
```

## Escenarios de Prueba Cubiertos

### PruebaAmigosManejador
- **Suscribir:**
  - ✓ Nombre de usuario nulo/vacío/solo espacios
  - ✓ Usuario no encontrado en BD
  - ✓ Error de base de datos (EntityException)

- **EnviarSolicitudAmistad:**
  - ✓ Nombre emisor/receptor nulo
  - ✓ Emisor no existe
  - ✓ Receptor no existe
  - ✓ Fallo de base de datos (DataException)
  - ✓ Relación ya existe (InvalidOperationException)

- **ResponderSolicitudAmistad:**
  - ✓ Nombres nulos
  - ✓ Usuarios no existen
  - ✓ Fallo de base de datos
  - ✓ Solicitud no existe

- **EliminarAmigo:**
  - ✓ Nombres nulos
  - ✓ Usuarios no existen
  - ✓ Fallo de base de datos
  - ✓ Relación no existe

- **CancelarSuscripcion:**
  - ✓ Nombre de usuario nulo/vacío/solo espacios

### PruebaAmistadServicio
- **CrearSolicitud:**
  - ✓ Usuario intenta enviarse solicitud a sí mismo
  - ✓ Relación ya existe
  - ✓ Creación exitosa

- **AceptarSolicitud:**
  - ✓ Solicitud no existe
  - ✓ Solicitud no corresponde al usuario
  - ✓ Solicitud ya aceptada
  - ✓ Aceptación exitosa

- **EliminarAmistad:**
  - ✓ Mismo usuario (IDs iguales)
  - ✓ Relación no existe
  - ✓ Eliminación exitosa

- **ObtenerSolicitudesPendientesDTO:**
  - ✓ Lista vacía cuando no hay solicitudes
  - ✓ Retorna solicitudes pendientes correctamente

- **ObtenerAmigosDTO:**
  - ✓ Lista vacía cuando no hay amigos
  - ✓ Retorna lista de amigos
  - ✓ Filtra amigos nulos

## Dependencias Agregadas

### packages.config
```xml
<package id="Moq" version="4.20.69" targetFramework="net472" />
<package id="Castle.Core" version="5.1.1" targetFramework="net472" />
<package id="System.Runtime.CompilerServices.Unsafe" version="4.5.3" targetFramework="net472" />
<package id="System.Threading.Tasks.Extensions" version="4.5.4" targetFramework="net472" />
```

## Beneficios de la Refactorización

1. **Testabilidad:** Las dependencias pueden ser simuladas (mocked) usando Moq
2. **Mantenibilidad:** Código más modular y fácil de mantener
3. **SOLID:** Cumple con los principios de Inversión de Dependencias y Responsabilidad Única
4. **Flexibilidad:** Facilita cambios futuros en la implementación de contextos o servicios
5. **Cobertura:** Permite probar lógica de negocio sin acceso a base de datos real

## Notas Importantes

- La refactorización mantiene **compatibilidad hacia atrás** en la API pública
- Los métodos estáticos de `NotificadorAmigos` y `ManejadorCallback` se mantienen para compatibilidad con WCF
- Las pruebas requieren .NET Framework 4.7.2 y MSTest
- El entorno de build debe soportar restauración de paquetes NuGet

## Ejecución de Pruebas

```bash
# Restaurar paquetes
nuget restore PictionaryMusicalServidor.sln

# Compilar solución
msbuild PictionaryMusicalServidor.sln /p:Configuration=Debug

# Ejecutar pruebas
vstest.console.exe PictionaryMusicalServidor.Pruebas\bin\Debug\PictionaryMusicalServidor.Pruebas.dll
```

## Próximos Pasos Recomendados

1. Considerar inyectar también los repositorios (`IUsuarioRepositorio`, `IAmigoRepositorio`) para mayor testabilidad
2. Implementar un contenedor de IoC (e.g., Unity, Autofac) para gestión automática de dependencias
3. Agregar pruebas de integración que validen el comportamiento completo con BD real
4. Documentar patrones de inyección para otros servicios del sistema
