# Pruebas Unitarias - PictionaryMusicalServidor

## Descripción General

Este proyecto contiene las pruebas unitarias para la capa de servicios del servidor PictionaryMusical. Las pruebas están escritas utilizando MSTest y Moq, siguiendo las convenciones de nomenclatura y estructura establecidas en el proyecto cliente.

## Estructura de las Pruebas

### Convenciones de Nomenclatura

Todas las pruebas siguen el formato en español:
```
Prueba_NombreMetodo_Condición_ResultadoEsperado
```

**Ejemplos:**
- `Prueba_IniciarSesion_CredencialesNulas_LanzaExcepcion`
- `Prueba_IniciarSesion_IdentificadorVacio_RetornaCredencialesInválidas`
- `Prueba_IniciarSesion_UsuarioNoExiste_RetornaCuentaNoEncontrada`

### Organización con Regiones

Las pruebas están organizadas en regiones lógicas para facilitar la navegación:

1. **Validaciones de Argumentos Nulos** - Verifica que se lancen excepciones apropiadas
2. **Validaciones de Datos Inválidos** - Verifica validaciones de formato y longitud
3. **Validaciones de Normalización** - Verifica que los datos se limpien correctamente
4. **Casos de Usuario No Encontrado** - Escenarios donde el usuario no existe
5. **Casos de Contraseña Incorrecta** - Autenticación fallida por contraseña
6. **Casos de Éxito (Happy Path)** - Escenarios de éxito esperados
7. **Manejo de Excepciones de Base de Datos** - Manejo robusto de errores
8. **Pruebas de Seguridad y BCrypt** - Verificación de hashing de contraseñas
9. **Casos Límite y Edge Cases** - Valores en los límites de validación
10. **Pruebas de Consistencia de Mensajes** - Mensajes de error uniformes
11. **Pruebas de Múltiples Intentos** - Comportamiento con intentos repetidos
12. **Pruebas de Inmutabilidad del Manejador** - Sin estado entre llamadas

## Clase: PruebaInicioSesionManejador

### Cobertura de Pruebas

#### ✅ Implementadas y Ejecutables (19 pruebas)

**Validaciones de Entrada:**
- Credenciales nulas
- Identificador vacío, nulo, solo espacios
- Contraseña nula, vacía, solo espacios
- Identificador que excede longitud máxima (>50 caracteres)
- Ambos campos vacíos

**Normalización:**
- Identificador con espacios al inicio/final se normaliza
- Contraseña con espacios se normaliza

**Búsqueda de Usuario:**
- Usuario inexistente por nombre de usuario
- Usuario inexistente por correo electrónico

**Seguridad BCrypt:**
- Verificación correcta de contraseña hasheada
- Verificación incorrecta de contraseña
- Sensibilidad a mayúsculas/minúsculas

**Casos Límite:**
- Identificador con longitud exacta de 50 caracteres
- Contraseña con caracteres especiales
- Contraseña con espacios en medio
- Identificador con caracteres Unicode
- Correo con mayúsculas

**Consistencia:**
- Mensajes consistentes para diferentes tipos de datos inválidos
- Mensajes consistentes para usuarios inexistentes
- Múltiples intentos fallidos retornan errores consistentes
- Manejador sin estado entre llamadas

#### 📝 Documentadas pero Comentadas (7 pruebas)

Las siguientes pruebas están documentadas en el código pero comentadas porque requieren:

**Integración con Base de Datos de Pruebas:**
- Contraseña incorrecta para usuario existente
- Inicio de sesión exitoso con credenciales correctas
- Inicio de sesión con correo electrónico
- Mapeo completo de datos del usuario

**Mocking Avanzado de Entity Framework:**
- Manejo de EntityException (errores de conexión)
- Manejo de DataException (errores de datos)
- Manejo de InvalidOperationException (estado inconsistente)

### Limitaciones Actuales

#### 1. Dependencia Directa de Base de Datos

La clase `InicioSesionManejador` utiliza `ContextoFactory.CrearContexto()` directamente, lo cual dificulta el mocking completo. Para pruebas más exhaustivas, se recomienda:

**Opción A: Patrón Repositorio**
```csharp
public interface IUsuarioRepositorio
{
    Usuario BuscarPorNombreUsuario(string nombreUsuario);
    Usuario BuscarPorCorreo(string correo);
}

public class InicioSesionManejador
{
    private readonly IUsuarioRepositorio _repositorio;
    
    public InicioSesionManejador(IUsuarioRepositorio repositorio)
    {
        _repositorio = repositorio;
    }
    // ...
}
```

**Opción B: Inyección de Dependencias del Contexto**
```csharp
public interface IContextoFactory
{
    BaseDatosPruebaEntities1 CrearContexto();
}
```

#### 2. BCrypt en Pruebas

Actualmente, las pruebas de BCrypt son unitarias básicas. Para probar completamente el flujo de autenticación, se necesitarían:
- Datos de prueba en base de datos con contraseñas hasheadas conocidas
- O mocking del método `BCrypt.Verify()`

#### 3. Excepciones de Base de Datos

Las pruebas de manejo de excepciones (EntityException, DataException, etc.) están comentadas porque requieren mockear `ContextoFactory`, que actualmente es una clase estática.

## Dependencias del Proyecto

### NuGet Packages

```xml
<package id="BCrypt.Net-Next" version="4.0.3" targetFramework="net472" />
<package id="Castle.Core" version="5.1.1" targetFramework="net472" />
<package id="EntityFramework" version="6.5.1" targetFramework="net472" />
<package id="Moq" version="4.20.70" targetFramework="net472" />
<package id="MSTest.TestAdapter" version="2.2.10" targetFramework="net472" />
<package id="MSTest.TestFramework" version="2.2.10" targetFramework="net472" />
```

### Referencias de Proyecto

- **Servicios**: Contiene la lógica de negocio a probar
- **Datos**: Proporciona los modelos de entidades

## Ejecutar las Pruebas

### Desde Visual Studio
1. Abrir el Explorador de Pruebas (Test Explorer)
2. Hacer clic en "Ejecutar Todas" o seleccionar pruebas individuales
3. Ver resultados en el panel del Explorador de Pruebas

### Desde Línea de Comandos (con MSBuild)
```bash
# Restaurar paquetes NuGet
nuget restore PictionaryMusicalServidor.sln

# Compilar solución
msbuild PictionaryMusicalServidor.sln /p:Configuration=Debug

# Ejecutar pruebas
vstest.console.exe PictionaryMusicalServidor.Pruebas\bin\Debug\PictionaryMusicalServidor.Pruebas.dll
```

### Desde .NET CLI (si se migra a .NET Core/.NET 5+)
```bash
dotnet test PictionaryMusicalServidor.sln
```

## Recomendaciones para Pruebas Futuras

### 1. Implementar Patrón Repositorio

Refactorizar la capa de datos para usar interfaces, facilitando el mocking:

```csharp
// En Datos/Repositorios/IUsuarioRepositorio.cs
public interface IUsuarioRepositorio
{
    Usuario BuscarPorIdentificador(string identificador);
}

// En Datos/Repositorios/UsuarioRepositorio.cs
public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly BaseDatosPruebaEntities1 _contexto;
    
    public Usuario BuscarPorIdentificador(string identificador)
    {
        // Lógica actual de búsqueda
    }
}
```

### 2. Configurar Base de Datos de Pruebas

Crear una base de datos de pruebas con datos conocidos:

```csharp
[TestInitialize]
public void Inicializar()
{
    // Configurar datos de prueba
    var mockContexto = new Mock<BaseDatosPruebaEntities1>();
    var mockUsuarios = new Mock<DbSet<Usuario>>();
    
    var datosUsuarios = new List<Usuario>
    {
        new Usuario 
        { 
            idUsuario = 1,
            Nombre_Usuario = "usuarioTest",
            Contrasena = BCryptNet.HashPassword("Password123!"),
            Jugador = new Jugador { /* ... */ }
        }
    }.AsQueryable();
    
    // Configurar mock...
}
```

### 3. Implementar Pruebas de Integración

Además de las pruebas unitarias, crear pruebas de integración que:
- Usen una base de datos real (LocalDB o SQL Server Express)
- Verifiquen el flujo completo de autenticación
- Prueben la interacción real con Entity Framework

### 4. Agregar Pruebas de Rendimiento

Para servicios críticos como autenticación:

```csharp
[TestMethod]
public void Prueba_IniciarSesion_TiempoRespuesta_MenorA500ms()
{
    var stopwatch = Stopwatch.StartNew();
    
    _manejador.IniciarSesion(credenciales);
    
    stopwatch.Stop();
    Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500);
}
```

### 5. Implementar Pruebas de Concurrencia

Verificar comportamiento con múltiples solicitudes simultáneas:

```csharp
[TestMethod]
public void Prueba_IniciarSesion_SolicitudesConcurrentes_ManejaSinErrores()
{
    var tareas = new List<Task<ResultadoInicioSesionDTO>>();
    
    for (int i = 0; i < 10; i++)
    {
        tareas.Add(Task.Run(() => _manejador.IniciarSesion(credenciales)));
    }
    
    Task.WaitAll(tareas.ToArray());
    
    // Verificar que todas las tareas completaron sin excepciones
}
```

## Mejores Prácticas Aplicadas

### ✅ AAA Pattern (Arrange-Act-Assert)
Todas las pruebas siguen el patrón AAA claramente:
- **Arrange**: Preparar datos de prueba
- **Act**: Ejecutar el método a probar
- **Assert**: Verificar resultados esperados

### ✅ Una Aserción por Prueba (cuando es posible)
Cada prueba verifica un comportamiento específico.

### ✅ Nombres Descriptivos
Los nombres de las pruebas describen claramente qué se está probando.

### ✅ Independencia de Pruebas
Cada prueba es independiente y no depende del orden de ejecución.

### ✅ Cobertura de Casos Límite
Se prueban valores en los límites de validación (0, máximo, etc.).

### ✅ Pruebas de Seguridad
Se verifican aspectos de seguridad como el hashing de contraseñas.

## Contribuir

Al agregar nuevas pruebas:

1. Seguir la convención de nombres establecida
2. Organizar en regiones lógicas
3. Documentar con comentarios XML las pruebas complejas
4. Incluir casos de error y casos límite
5. Verificar que las pruebas sean independientes
6. Actualizar este README con información relevante

## Referencias

- [Documentación MSTest](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest)
- [Documentación Moq](https://github.com/moq/moq4)
- [BCrypt.Net Documentation](https://github.com/BcryptNet/bcrypt.net)
- Archivo de referencia: `PictionaryMusicalCliente.Pruebas/PruebasVistaModelo/InicioSesion/PruebaInicioSesionVistaModelo.cs`
