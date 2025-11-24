# Plan Maestro de Pruebas Unitarias (Cobertura Extendida) - PictionaryMusicalServidor

Este documento define la estrategia para alcanzar una alta cobertura de pruebas en la lógica del servidor.

## 1. Reglas de Oro para las Pruebas
* **Validación de Entradas:** Nulls, cadenas vacías, cadenas de solo espacios, formatos inválidos (ej. emails sin @).
* **Lógica de Negocio:** Límites (ej. salas llenas, puntajes negativos), estados inválidos.
* **Infraestructura:** Simular errores de base de datos (`EntityException`, `Timeout`) usando Moq.
* **Seguridad:** Verificar que usuarios no autorizados no puedan ejecutar acciones de admin (ej. expulsar jugadores).

## 2. Backlog de Pruebas por Clase

### 🔐 Autenticación y Cuentas

- [ ] **InicioSesionManejador** (`InicioSesionServicio.cs`)
    * [ ] **Correcto:** Credenciales válidas retornan éxito y usuario completo.
    * [ ] **Datos:** Identificador nulo o vacío -> `ArgumentNullException` o Error controlado.
    * [ ] **Datos:** Contraseña nula o vacía -> Error controlado.
    * [ ] **Lógica:** Usuario no existe en BD -> Retorna `false` o mensaje específico.
    * [ ] **Lógica:** Contraseña incorrecta -> Retorna `false`.
    * [ ] **Infraestructura:** BD caída (`EntityException`) -> Retorna mensaje de "Error en el servidor", no la excepción cruda.
    * [ ] **Seguridad:** Verificar que no se devuelvan contraseñas en el objeto de retorno.

- [ ] **CuentaManejador** (Registro)
    * [ ] **Correcto:** Registro exitoso guarda en BD.
    * [ ] **Validación:** Correo con formato inválido (ej. "usuario.com") -> Retorna error de validación.
    * [ ] **Validación:** Nombre de usuario con caracteres prohibidos.
    * [ ] **Lógica:** Correo ya registrado (Duplicado).
    * [ ] **Lógica:** Nombre de usuario ya registrado (Duplicado).
    * [ ] **Lógica:** Fallo en `SaveChanges()` (simular error al guardar).

- [ ] **RecuperacionCuentaServicio**
    * [ ] **Correcto:** Flujo completo de envío de correo.
    * [ ] **Lógica:** Usuario no encontrado -> Retorna éxito falso (para no revelar existencia de cuentas) o error genérico.
    * [ ] **Infraestructura:** Fallo en el cliente SMTP (simular excepción de red al enviar correo).

### 🎮 Juego y Salas (Lógica Crítica)

- [ ] **SalasManejador** (Gestión de Salas)
    * [ ] **Correcto:** Crear sala retorna ID único.
    * [ ] **Lógica:** Unirse a sala -> Éxito si hay cupo.
    * [ ] **Límite:** Unirse a sala llena -> Retorna error "Sala llena".
    * [ ] **Lógica:** Unirse con contraseña incorrecta (si aplica).
    * [ ] **Lógica:** Usuario intenta unirse a dos salas al mismo tiempo.
    * [ ] **Seguridad:** Expulsar jugador -> Falla si quien solicita no es el anfitrión.
    * [ ] **Estado:** Intentar iniciar partida sin el mínimo de jugadores (ej. 1 jugador).

- [ ] **ClasificacionManejador** (Puntajes)
    * [ ] **Correcto:** Cálculo de puntaje estándar.
    * [ ] **Límite:** Puntaje máximo posible (overflow check).
    * [ ] **Lógica:** Jugador se desconecta antes de guardar puntaje.
    * [ ] **Lógica:** Empate técnico en primer lugar.

### 🤝 Social

- [ ] **AmigosManejador**
    * [ ] **Validación:** Enviar solicitud a usuario nulo/vacío.
    * [ ] **Lógica:** Enviar solicitud a uno mismo.
    * [ ] **Lógica:** Enviar solicitud a quien ya es amigo.
    * [ ] **Lógica:** Enviar solicitud a alguien que ya tiene una solicitud pendiente.
    * [ ] **Infraestructura:** Error de BD al recuperar lista de bloqueados.

### 🛠 Utilidades

- [ ] **VerificacionRegistroServicio**
    * [ ] Email válido complejo (ej. `nombre.apellido+tag@dominio.co.uk`).
    * [ ] Contraseña débil (muy corta, sin números).
    * [ ] Contraseña demasiado larga (buffer overflow potential).