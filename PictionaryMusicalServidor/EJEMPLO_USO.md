# Ejemplos de Uso - AmigosManejador y AmistadServicio Refactorizados

## Ejemplo 1: Configuración en Producción

```csharp
using PictionaryMusicalServidor.Servicios.Servicios;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;

// Método de inicialización del host WCF
public void InicializarServicios()
{
    // Crear las dependencias concretas
    IContextoFactory contextoFactory = new ContextoFactory();
    IAmistadServicio amistadServicio = new AmistadServicio(contextoFactory);
    
    // Crear el manejador con inyección de dependencias
    var amigosManejador = new AmigosManejador(contextoFactory, amistadServicio);
    
    // Configurar el host WCF con la instancia
    var serviceHost = new ServiceHost(amigosManejador, baseAddress);
    serviceHost.Open();
}
```

## Ejemplo 2: Uso Normal del Servicio

```csharp
// Una vez configurado, el servicio se usa igual que antes
var amigosManejador = ObtenerManejadorConfigurado();

// Suscribir usuario a notificaciones
amigosManejador.Suscribir("usuario1");

// Enviar solicitud de amistad
amigosManejador.EnviarSolicitudAmistad("usuario1", "usuario2");

// Responder solicitud de amistad
amigosManejador.ResponderSolicitudAmistad("usuario1", "usuario2");

// Eliminar amigo
amigosManejador.EliminarAmigo("usuario1", "usuario2");

// Cancelar suscripción
amigosManejador.CancelarSuscripcion("usuario1");
```

## Ejemplo 3: Uso Directo del Servicio de Amistad

```csharp
IContextoFactory contextoFactory = new ContextoFactory();
IAmistadServicio amistadServicio = new AmistadServicio(contextoFactory);

// Crear solicitud de amistad
amistadServicio.CrearSolicitud(usuarioEmisorId: 1, usuarioReceptorId: 2);

// Aceptar solicitud
amistadServicio.AceptarSolicitud(usuarioEmisorId: 1, usuarioReceptorId: 2);

// Obtener amigos de un usuario
var amigos = amistadServicio.ObtenerAmigosDTO(usuarioId: 1);
foreach (var amigo in amigos)
{
    Console.WriteLine($"Amigo: {amigo.NombreUsuario} (ID: {amigo.UsuarioId})");
}

// Obtener solicitudes pendientes
var solicitudes = amistadServicio.ObtenerSolicitudesPendientesDTO(usuarioId: 1);
foreach (var solicitud in solicitudes)
{
    Console.WriteLine($"Solicitud de: {solicitud.UsuarioEmisor}");
}

// Eliminar amistad
var relacionEliminada = amistadServicio.EliminarAmistad(usuarioAId: 1, usuarioBId: 2);
```

## Ejemplo 4: Prueba Unitaria Básica

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class PruebaEjemplo
{
    [TestMethod]
    public void Ejemplo_Probar_EnviarSolicitud_UsuariosValidos()
    {
        // Arrange - Configurar mocks
        var mockContextoFactory = new Mock<IContextoFactory>();
        var mockAmistadServicio = new Mock<IAmistadServicio>();
        var mockContexto = new Mock<BaseDatosPruebaEntities1>();
        
        mockContextoFactory.Setup(f => f.CrearContexto()).Returns(mockContexto.Object);
        
        var manejador = new AmigosManejador(mockContextoFactory.Object, mockAmistadServicio.Object);
        
        // Configurar comportamiento esperado
        mockAmistadServicio.Setup(s => s.CrearSolicitud(1, 2));
        
        // Act & Assert - Ejecutar y verificar
        // En una prueba real, configurarías también el repositorio de usuarios
        // para que devuelva los usuarios mockeados
    }
}
```

## Ejemplo 5: Prueba con Validación de Errores

```csharp
[TestClass]
public class PruebaValidacionesEjemplo
{
    private Mock<IContextoFactory> _mockContextoFactory;
    private Mock<IAmistadServicio> _mockAmistadServicio;
    private AmigosManejador _manejador;
    
    [TestInitialize]
    public void Configurar()
    {
        _mockContextoFactory = new Mock<IContextoFactory>();
        _mockAmistadServicio = new Mock<IAmistadServicio>();
        _manejador = new AmigosManejador(_mockContextoFactory.Object, _mockAmistadServicio.Object);
    }
    
    [TestMethod]
    [ExpectedException(typeof(FaultException))]
    public void Ejemplo_Validar_NombreUsuarioNulo_LanzaExcepcion()
    {
        // Act - Intentar suscribir con nombre nulo
        _manejador.Suscribir(null);
        
        // Assert - La excepción esperada se valida con el atributo [ExpectedException]
    }
    
    [TestMethod]
    public void Ejemplo_Validar_CrearSolicitudDuplicada()
    {
        // Arrange
        _mockAmistadServicio
            .Setup(s => s.CrearSolicitud(1, 2))
            .Throws(new InvalidOperationException("Ya existe una relación de amistad"));
        
        // Act & Assert
        try
        {
            _mockAmistadServicio.Object.CrearSolicitud(1, 2);
            Assert.Fail("Debería haber lanzado InvalidOperationException");
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Ya existe una relación de amistad", ex.Message);
        }
    }
}
```

## Ejemplo 6: Verificación de Llamadas con Moq

```csharp
[TestMethod]
public void Ejemplo_VerificarLlamadaAServicio()
{
    // Arrange
    var mockContextoFactory = new Mock<IContextoFactory>();
    var mockAmistadServicio = new Mock<IAmistadServicio>();
    var mockContexto = new Mock<BaseDatosPruebaEntities1>();
    
    mockContextoFactory.Setup(f => f.CrearContexto()).Returns(mockContexto.Object);
    
    var manejador = new AmigosManejador(mockContextoFactory.Object, mockAmistadServicio.Object);
    
    // Simular usuarios existentes
    // (En una implementación completa, también mockearías el UsuarioRepositorio)
    
    // Act
    // manejador.EnviarSolicitudAmistad("usuario1", "usuario2");
    
    // Assert - Verificar que se llamó al método del servicio
    mockAmistadServicio.Verify(
        s => s.CrearSolicitud(It.IsAny<int>(), It.IsAny<int>()), 
        Times.Once(),
        "El servicio de amistad debería ser llamado exactamente una vez"
    );
}
```

## Ejemplo 7: Prueba del Servicio de Amistad con Validaciones

```csharp
[TestClass]
public class PruebaAmistadServicioEjemplo
{
    private Mock<IContextoFactory> _mockContextoFactory;
    private AmistadServicio _servicio;
    
    [TestInitialize]
    public void Configurar()
    {
        _mockContextoFactory = new Mock<IContextoFactory>();
        _servicio = new AmistadServicio(_mockContextoFactory.Object);
    }
    
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Ejemplo_CrearSolicitud_AutoEnvio_LanzaExcepcion()
    {
        // Act - Intentar crear solicitud para el mismo usuario
        _servicio.CrearSolicitud(usuarioEmisorId: 5, usuarioReceptorId: 5);
        
        // Assert - La excepción es validada por el atributo
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Ejemplo_Constructor_ContextoFactoryNulo_LanzaExcepcion()
    {
        // Act - Intentar crear servicio sin factoría
        new AmistadServicio(null);
        
        // Assert - Validado por el atributo
    }
}
```

## Patrones de Uso Recomendados

### 1. Separación de Responsabilidades
```csharp
// Clase de configuración
public class ConfiguracionServicios
{
    public static IAmigosManejador CrearAmigosManejador()
    {
        var contextoFactory = new ContextoFactory();
        var amistadServicio = new AmistadServicio(contextoFactory);
        return new AmigosManejador(contextoFactory, amistadServicio);
    }
}

// Uso en el host
var amigosManejador = ConfiguracionServicios.CrearAmigosManejador();
```

### 2. Manejo de Excepciones
```csharp
try
{
    amigosManejador.EnviarSolicitudAmistad("usuario1", "usuario2");
}
catch (FaultException ex)
{
    // Manejar errores de validación o negocio
    Console.WriteLine($"Error: {ex.Message}");
}
catch (Exception ex)
{
    // Manejar errores inesperados
    Console.WriteLine($"Error inesperado: {ex.Message}");
}
```

### 3. Validación de Entrada
```csharp
public void EnviarSolicitudSegura(string emisor, string receptor)
{
    // Validación previa
    if (string.IsNullOrWhiteSpace(emisor) || string.IsNullOrWhiteSpace(receptor))
    {
        throw new ArgumentException("Los nombres de usuario no pueden estar vacíos");
    }
    
    if (emisor.Equals(receptor, StringComparison.OrdinalIgnoreCase))
    {
        throw new ArgumentException("No puedes enviarte una solicitud a ti mismo");
    }
    
    // Llamar al servicio
    amigosManejador.EnviarSolicitudAmistad(emisor, receptor);
}
```

## Notas de Migración

Si estás migrando código existente:

**Antes:**
```csharp
var manejador = new AmigosManejador();
```

**Después:**
```csharp
var contextoFactory = new ContextoFactory();
var amistadServicio = new AmistadServicio(contextoFactory);
var manejador = new AmigosManejador(contextoFactory, amistadServicio);
```

La API pública de los métodos permanece sin cambios, por lo que el código cliente no necesita modificaciones.
