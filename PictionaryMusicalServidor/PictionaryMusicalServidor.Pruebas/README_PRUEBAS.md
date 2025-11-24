# Plan de Pruebas Unitarias - PictionaryMusicalServidor

Este documento describe la implementación del plan de pruebas unitarias para el servidor de Pictionary Musical, según lo especificado en `planPruebas.md`.

## Estado Actual de las Pruebas

### ✅ Pruebas Implementadas (Sin Mock)

Las siguientes pruebas están **completamente implementadas** y pueden ejecutarse sin configuración adicional:

#### PruebaVerificacionRegistroServicio.cs
- ✅ Validación de correos electrónicos (simples, complejos, inválidos)
- ✅ Validación de contraseñas (débiles, fuertes, límites de longitud)
- ✅ Validación de datos de nueva cuenta
- ✅ Validación de tokens
- ✅ Validación de códigos de verificación

#### PruebaInicioSesionManejador.cs (Parcial)
- ✅ Validación de datos nulos y vacíos
- ✅ Manejo de ArgumentNullException
- ⚠️ Lógica de negocio (requiere mocking - comentado)

#### PruebaCuentaManejador.cs (Parcial)
- ✅ Validación de datos de entrada
- ✅ Validación de formato de correo
- ⚠️ Detección de duplicados (requiere mocking - comentado)
- ⚠️ Guardado en base de datos (requiere mocking - comentado)

#### PruebaCorreoCodigoVerificacionNotificador.cs
- ✅ Construcción de mensajes de correo en múltiples idiomas

### ⚠️ Pruebas Pendientes de Implementación (Requieren Mocking)

Las siguientes pruebas están documentadas como plantillas y requieren configuración de mocking:

#### PruebaRecuperacionCuentaServicio.cs
- ⏸️ Solicitud de código de recuperación
- ⏸️ Usuario no encontrado (debe retornar error genérico por seguridad)
- ⏸️ Fallo de SMTP (simular excepción de red)
- ⏸️ Reenvío de código
- ⏸️ Confirmación de código
- ⏸️ Actualización de contraseña

#### PruebaSalasYAmigosManejador.cs
- ⏸️ Creación de salas con códigos únicos
- ⏸️ Unirse a sala llena (debe retornar "Sala llena")
- ⏸️ Usuario intenta unirse a dos salas simultáneamente
- ⏸️ Expulsar jugador (solo el anfitrión puede hacerlo)
- ⏸️ Iniciar partida sin mínimo de jugadores
- ⏸️ Enviar solicitud de amistad a uno mismo
- ⏸️ Enviar solicitud a quien ya es amigo
- ⏸️ Solicitud pendiente duplicada
- ⏸️ Error de BD al recuperar lista de bloqueados
- ⏸️ Cálculo de puntaje estándar
- ⏸️ Puntaje máximo (overflow check)
- ⏸️ Jugador se desconecta antes de guardar puntaje
- ⏸️ Empate técnico en primer lugar

## Configuración Necesaria para Pruebas con Mock

### 1. Instalar Moq

Agregar el paquete NuGet Moq al proyecto de pruebas:

```bash
Install-Package Moq
```

O editar `packages.config`:

```xml
<package id="Moq" version="4.18.4" targetFramework="net472" />
```

### 2. Ejemplo de Mock para Entity Framework

```csharp
using Moq;
using System.Data.Entity;
using System.Linq;

// Mock del contexto
var mockContext = new Mock<BaseDatosPruebaEntities1>();

// Mock del DbSet
var data = new List<Usuario>
{
    new Usuario { idUsuario = 1, Nombre_Usuario = "test", Contrasena = "hashedpass" }
}.AsQueryable();

var mockSet = new Mock<DbSet<Usuario>>();
mockSet.As<IQueryable<Usuario>>().Setup(m => m.Provider).Returns(data.Provider);
mockSet.As<IQueryable<Usuario>>().Setup(m => m.Expression).Returns(data.Expression);
mockSet.As<IQueryable<Usuario>>().Setup(m => m.ElementType).Returns(data.ElementType);
mockSet.As<IQueryable<Usuario>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

// Configurar el contexto para retornar el mock del DbSet
mockContext.Setup(c => c.Usuario).Returns(mockSet.Object);
```

### 3. Ejemplo de Mock para WCF Callbacks

```csharp
using Moq;
using System.ServiceModel;

// Mock del callback
var mockCallback = new Mock<ISalasCallback>();
mockCallback.Setup(c => c.NotificarJugadorUnido(It.IsAny<string>())).Verifiable();

// Mock del OperationContext (más complejo, puede requerir wrapper)
// Alternativa: Refactorizar el código para usar inyección de dependencias
```

## Estrategia de Testing Recomendada

### Nivel 1: Validación de Datos (Sin Mock) ✅
Pruebas que validan entrada de datos, formatos, límites y normalizaciones:
- Correos electrónicos
- Contraseñas
- Tokens
- Códigos de verificación
- Campos obligatorios

**Estado:** Implementado y funcional

### Nivel 2: Lógica de Negocio (Con Mock) ⚠️
Pruebas que requieren simular base de datos pero no servicios externos:
- Búsqueda de usuarios
- Detección de duplicados
- Validación de estado (ej. sala llena)
- Verificación de permisos (ej. solo anfitrión puede expulsar)

**Estado:** Plantillas creadas, requiere configuración de Moq

### Nivel 3: Integración con Servicios (Con Mock) ⏸️
Pruebas que requieren simular servicios externos:
- Envío de correos (SMTP)
- Notificaciones en tiempo real (WCF callbacks)
- Timeouts y errores de red

**Estado:** Plantillas creadas, requiere configuración avanzada

### Nivel 4: Infraestructura (Con Mock) ⏸️
Pruebas de manejo de errores y excepciones:
- EntityException (BD caída)
- DbUpdateException (error al guardar)
- DbEntityValidationException (validación de entidades)
- TimeoutException (timeout de red)

**Estado:** Casos identificados en código comentado

## Métricas de Cobertura Objetivo

Según el plan maestro de pruebas:

- **Validación de Entradas:** 95%+ (casi completo ✅)
- **Lógica de Negocio:** 80%+ (en progreso ⚠️)
- **Infraestructura:** 70%+ (pendiente ⏸️)
- **Seguridad:** 90%+ (casos críticos identificados ⚠️)

## Casos de Seguridad Críticos

### ✅ Implementados
1. Contraseñas no retornadas en respuesta de inicio de sesión
2. Validación de formato de correo para prevenir inyección
3. Validación de longitud de contraseña (no demasiado larga - buffer overflow)

### ⚠️ Pendientes de Implementación
1. Verificar que errores no revelan existencia de cuentas (recuperación)
2. Verificar que usuarios no autorizados no pueden ejecutar acciones de admin
3. Verificar timeout en expresiones regulares (ReDoS protection)
4. Verificar que mensajes de error no exponen stack traces

## Cómo Ejecutar las Pruebas

### Usando Visual Studio
1. Abrir la solución en Visual Studio
2. Ir a Test > Run > All Tests
3. Ver resultados en Test Explorer

### Usando la línea de comandos
```bash
# Navegar al directorio del proyecto
cd PictionaryMusicalServidor

# Restaurar paquetes
nuget restore

# Compilar
msbuild /p:Configuration=Debug

# Ejecutar pruebas
vstest.console.exe PictionaryMusicalServidor.Pruebas\bin\Debug\PictionaryMusicalServidor.Pruebas.dll
```

## Próximos Pasos

1. **Inmediato:** Ejecutar las pruebas implementadas para verificar funcionalidad básica
2. **Corto plazo:** Configurar Moq e implementar pruebas de Nivel 2
3. **Mediano plazo:** Implementar pruebas de Nivel 3 y 4
4. **Continuo:** Mantener cobertura de pruebas al agregar nuevas funcionalidades

## Convenciones de Nomenclatura

- Métodos de prueba: `Prueba_[Metodo]_[Escenario]_Deberia[Resultado]`
- Clases de prueba: `Prueba[ClaseAProbar]`
- Comentarios `TODO:` indican donde se requiere configuración de mock
- Comentarios `NOTA:` indican consideraciones especiales

## Recursos Adicionales

- [Documentación de MSTest](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest)
- [Documentación de Moq](https://github.com/moq/moq4/wiki/Quickstart)
- [Mocking Entity Framework](https://docs.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking)

## Contacto

Para preguntas sobre las pruebas o sugerencias de mejora, consultar el archivo `planPruebas.md` o contactar al equipo de desarrollo.
