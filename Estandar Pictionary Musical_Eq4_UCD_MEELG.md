![En Transparencia | Rendición de cuentas](Aspose.Words.72704be1-326b-4c59-8598-56292180aca9.001.png)![Imagen que contiene Diagrama

El contenido generado por IA puede ser incorrecto.](Aspose.Words.72704be1-326b-4c59-8598-56292180aca9.002.png)

**Universidad Veracruzana**

**Facultad de Estadística e Informática**

**Licenciatura en Ingeniería de Software**






**	

**Estándar del Proyecto Pictionary Musical**




**Experiencia Educativa: Tecnologías para la Construcción de Software**

**Docente: Juan Carlos Pérez Arriaga**




**Estudiantes:** 

**Uriel Cendón Díaz**

**Miguel Eduardo Escobar Ladrón de Guevara**







**Fecha: 31 de Octubre del 2025**



# Contenido
[**Introducción**	1](#_toc212804965)

[**Propósito**	1](#_toc212804966)

[**Idioma**	1](#_toc212804967)

[**1. Reglas de Nombramiento**	2](#_toc212804968)

[**1.1 Variables**	2](#_toc212804969)

[**1.2 Constantes**	3](#_toc212804970)

[**1.3 Métodos**	3](#_toc212804971)

[**1.4 Clases**	5](#_toc212804972)

[**1.5 Namespaces**	5](#_toc212804973)

[**1.6 Propiedades (Getters y Setters)**	6](#_toc212804974)

[**2. Estilo de Código**	7](#_toc212804975)

[**2.1 Indentación**	7](#_toc212804976)

[**2.2 Líneas y espacios en blanco**	8](#_toc212804977)

[**2.3 Uso de llaves**	9](#_toc212804978)

[**3. Comentarios**	10](#_toc212804979)

[**3.1 Comentarios de API pública**	10](#_toc212804980)

[**3.2 Comentarios de bloque**	11](#_toc212804981)

[**3.3 Comentarios de línea única**	11](#_toc212804982)

[**4. Estructuras de control**	12](#_toc212804983)

[**4.1 if / else**	12](#_toc212804984)

[**4.2 else if**	12](#_toc212804985)

[**4.3 switch**	13](#_toc212804986)

[**4.4 for**	14](#_toc212804987)

[**4.5 foreach**	14](#_toc212804988)

[**4.6 while**	15](#_toc212804989)

[**4.7 do while**	15](#_toc212804990)

[**5. Excepciones y Errores**	16](#_toc212804991)

[**5.1 Uso de bloques try-catch**	16](#_toc212804992)

[**5.2 Bitácora**	18](#_toc212804993)

[**6. Pruebas**	19](#_toc212804994)

[**6.1 Pruebas unitarias**	19](#_toc212804995)

[**7. Seguridad**	20](#_toc212804996)

[**8. Prefijos**	20](#_toc212804997)

[**8.1 Capa gráfica (XAML)**	20](#_toc212804998)

[**9. Base de datos**	22](#_toc212804999)

[**9.1 Tablas**	22](#_toc212805000)

[**9.2 Llaves primarias**	22](#_toc212805001)

[**9.3 Atributos**	23](#_toc212805002)

[**9.4 Llaves foráneas**	23](#_toc212805003)

[**10. Bibliografía**	24](#_toc212805004)



# <a name="_toc206620909"></a><a name="_toc212804965"></a>**Introducción**
Un estándar de codificación es un conjunto de directrices, reglas y buenas prácticas para la programación en un lenguaje específico. Su objetivo es mejorar la calidad del software a través de la consistencia y la legibilidad del código fuente. 
# <a name="_toc206620910"></a><a name="_toc212804966"></a>**Propósito**
El propósito de este documento es establecer el estándar de codificación que se seguirá en el proyecto de desarrollo en C# de la materia Tecnologías para la Construcción de Software. Seguir estas directrices será fundamental para facilitar mejorar la** legibilidad, comprensión y mantenibilidad del código, así como para reducir la complejidad y la probabilidad de errores.
# <a name="_toc212804967"></a>**Idioma**
El idioma que se usará para el código fuente, incluidos los nombres de variables, métodos, clases, archivos creados y comentarios técnicos será el español. No se usarán tildes ni la letra “ñ”, siendo esta letra reemplazada por la letra “n”.

<a name="_toc206620911"></a>**
# <a name="_toc212804968"></a>**1. Reglas de Nombramiento**
Las reglas de nombramiento son criterios establecidos para asignar nombres de variables, constantes, métodos y clases de manera coherente y organizada. Estas reglas nos ayudan a mantener consistencia en la escritura del código y facilidad de comprensión para cualquier persona que lea el código. A continuación, se encuentran las pautas del estándar para los diferentes casos.
## <a name="_toc206620912"></a><a name="_toc212804969"></a>**1.1 Variables**
Las variables locales y los parámetros de métodos deberán tener nombres descriptivos usando la notación camelCase, así como no deben contener ningún carácter especial. Esto significa que la primera letra es minúscula y, si tiene más de una palabra, la primera letra de cada palabra subsecuente es mayúscula, a excepción de lo anterior, los campos privados de una clase, deben tener nombres descriptivos que empiecen con un guion bajo al inicio del nombrado de esta (\_), seguido de la misma notación antes establecida (camelCase).

Ejemplos:

- **Correcto**

  string nombreCompleto;

  int contadorDeCiclos;

  private string \_usuarioEdad;

  public void MiMetodo(int numeroDeItems)

  {

  `    `var itemActual = new object();

  }

- **Incorrecto**

  // No usar PascalCase

  string NombreCompleto; 

  //No poner \_ al inicio de un campo privado

  private string usuarioEdad;

  // No usar guiones bajos (snake\_case)

  int contador\_de\_ciclos; 

  // No usar prefijos de su tipo

  string strNombre; 

## <a name="_toc212804970"></a>**1.2 <a name="_toc206620914"></a>Constantes**
Las constantes son variables cuyo valor no cambiarán, tanto const como readonly, deben declararse utilizando la notación PascalCase.

Ejemplos:

- **Correcto**

  public const int TiempoMaximoEspera = 100;

  public readonly string UrlApiBase = "https://api.miempresa.com/";

- **Incorrecto** 

  // No usar camelCase

  public const int tiempoMaximoEspera = 100; 

  // No usar MAYÚSCULAS\_CON\_GUIONES\_BAJOS (común en otros lenguajes, pero no es el estándar de C#)

  public const int TIEMPO\_MAXIMO\_ESPERA = 100; 
## <a name="_toc206620915"></a><a name="_toc212804971"></a>**1.3 Métodos**
Los nombres de los métodos deben ser llamados según su funcionalidad para facilitar su lectura y comprensión, en forma de verbos o frases verbales y deben usar la notación PascalCase. Cada método debe mantener una complejidad ciclomática controlada, de manera que no exceda un valor de 10. Esto implica que las estructuras de control deben organizarse de forma que los métodos sean simples. Si un método sobrepasa dicho umbral, debe refactorizarse dividiéndolo en métodos más pequeños y específicos.

Todos los métodos asíncronos deben llevar la notación Contexto + “Async”. Esto difiere del nombrado en español, pero se decidió de esta manera debido a que la herramienta de generación de referencias de servicio WCF crea automáticamente métodos cliente asíncronos basados en los contratos de servicio, y consistentemente utilizan el sufijo Async. Para mantener la consistencia y coherencia en todo el código base del cliente y evitar una mezcla confusa de sufijos (como Async de métodos generados y Asincrono de métodos escritos manualmente), se optó por adoptar Async como el estándar único para todos los métodos asíncronos.

`	`Ejemplo:

- **Correcto**:

`		`public async Task GuardarConfiguracionAsync() 

{ 

`    `// ... lógica asíncrona ... 

}


public async Task<Usuario> ObtenerUsuarioAsync(int id)

{

`    `// ... lógica asíncrona ...

}


- **Incorrecto**:

`		`public async Task GuardarConfiguracionAsincrono() 

{ 

`    `// ... 

} 

public async Task<Usuario> ObtenerUsuario(int id) 

{

`    `// ...

}

Ejemplos:

- **Correcto** 

  public void EnviarNotificacion()

  {

  `    `// ...

  }

  public User GetUsuarioPorId(int id)

  {

  `    `// ...

  }

- **Incorrecto** 

  // No usar camelCase

  public void enviarNotificacion() 

  {

  `    `// ...

  }

  // No usar guiones bajos

  public User get\_usuario\_por\_id(int id) 

  {

  `    `// ...

  }





## <a name="_toc206620916"></a><a name="_toc212804972"></a>**1.4 Clases**
<a name="_toc206620917"></a>Las clases deben ser sustantivos o frases sustantivas en PascalCase. 

La lista de roles es una lista base y ampliable, si un rol no aparece o no se considera un prefijo o sufijo para alguno, se puede definir uno de estos siempre y cuando sea coherente y usado consistentemente en el proyecto.

Los sufijos/prefijos se aplican por rol del tipo, independientemente de la ubicación del archivo en el repositorio:

**VistaModelo**

- Sufijo obligatorio: VistaModelo detrás del contexto funcional.
- Ejemplos (Correcto): BuscarAmigoVistaModelo, VentanaPrincipalVistaModelo.
- Ejemplos (Incorrecto): BuscarAmigoVM, vistamodeloventanaprincipal, ViewModelBuscarAmigo.	

**Vista**

- El archivo code-behind parcial conserva el mismo nombre.
- Ejemplos (Correcto): VentanaPrincipal, AjustesPartida, VerificarCodigo.
- Ejemplos (Incorrecto): ventana\_principal, Ajustespartida, VentanaPrincipalView.

**Comandos**

- Sufijo obligatorio: Comando, y le sigue su contexto funcional (si tiene).
- Las interfaces de estos mantienen prefijo IComando, y le sigue el contexto funcional (si tiene).
- Ejemplo (Correcto): IComandoAsincrono, ComandoAsincrono.
- Ejemplos (Incorrecto): interfaz\_comando\_asincrono, Comandoasincrono, AsyncCommand.

**Convertidores (UI)**

- Clases de convertidores con un prefijo obligatorio: Convertidor, y le sigue el tipo que manejan.
- Ejemplos (Correcto): ConvertidorBooleanoVisibilidad, ConvertidorCadenaVaciaVisibilidad.
- Ejemplos (Incorrecto): BooleanToVisibilityConverter, convertidor\_booleano.

**Repositorios**

- Clases de repositorios con un sufijo obligatorio: Repositorio (si es una interfaz, prefijo I, seguido del contexto funcional y finalizando con Repositorio).
- Ejemplos (Correcto): UsuarioRepositorio, IUsuarioRepositorio.
- Ejemplos (Incorrecto): RepoUsuario, DALUsuario, UsuarioRepositorioInterface.

**Interfaces**

- Prefijo obligatorio: I, y le sigue su contexto funcional.
- Ejemplos (Correcto): IAmigosServicio, IComandoAsincrono, IUsuarioRepositorio.
- Ejemplos (Incorrecto): AmigosServicioInterfaz, InterfazComandoAsincrono, UsuarioRepositorioI.

**Data Transfer Object (DTO)**

- Sufijo obligatorio: DTO detrás del contexto funcional.
- Ejemplo (Correcto): UsuarioDTO, ResultadoOperacionDTO.
- Ejemplo (Incorrecto): UsuarioDataTransferObject, ResultadoOperacion.

**Servicios de cliente / proxies / adaptadores remotos**

- Sufijo obligatorio: Servicio, y le sigue su contexto funcional.
- Ejemplo (Correcto): AmigosServicio, PerfilServicio.
- Ejemplo (Incorrecto): ServicioAmigosWCF, ProxyPerfil.
## <a name="_toc212804973"></a>**1.5 Namespaces**
Los namespaces deben escribirse en la notación PascalCase. Se recomienda usar una jerarquía que empiece con el nombre del proyecto o compañía. No deben contener guiones bajos o caracteres especiales.

Ejemplos:

- **Correcto** 

  namespace ProyectoJuego.Personajes

  namespace ProyectoJuego.Escenarios

  namespace ProyectoJuego.Utilidades

- **Incorrecto** 

  namespace proyectoJuego.Personajes

  namespace Proyecto\_Juego.Escenarios

  namespace ProyectoJuego.utilidades

## <a name="_toc212804974"></a>**1.6 Propiedades (Getters y Setters)**
Los métodos explícitos como GetNombre() o SetNombre() serán propiedades. Las propiedades públicas deben usar la notación PascalCase. Pueden existir condicionales dentro del set. Se permite incluir lógica adicional en get o set, siempre que esta sea simple y esté relacionada directamente con la lectura o asignación del valor (por ejemplo, validaciones, restricciones o transformaciones básicas). No deben contener flujos de control complejos ni superar la simplicidad que se espera de una propiedad.

Ejemplos:

- **Correcto** 

  public class Usuario

  {

  `    `public string Nombre { get; set; }

  `    `private int \_edad;

  `    `public int Edad

  `    `{

  `        `get { return \_edad; }

  `        `set 

  `        `{

  `            `if (value < 0)

  `            `{

  `                `throw new ArgumentException("La edad no puede ser negativa.");

  `            `}

  `            `\_edad = value;

  `        `}

  `    `}

  }

  Los comentarios del ejemplo anterior son de uso informativo.

- **Incorrecto** 

  public class Usuario

  {

  `    `// No usar campos públicos

  `    `public string nombre;

  `    `// No usar métodos explícitos get/set al estilo de Java

  `    `public string getNombre()

  `    `{

  `        `return this.nombre;

  `    `}

  `    `public void setNombre(string nombre)

  `    `{

  `        `this.nombre = nombre;

  `    `}

  }
# <a name="_toc206620918"></a><a name="_toc212804975"></a>**2. Estilo de Código**
Un estilo de código uniforme facilita la lectura y la comprensión visual del programa. Se recomienda que cada línea tenga una capacidad máxima de 100 caracteres dependiendo el caso. Si las líneas llegaran a superar esta cantidad de caracteres mencionados entonces se debe dividir cuando se presenten estos caracteres:

- Paréntesis ()
- Signos de operación
## <a name="_toc206620919"></a><a name="_toc212804976"></a>**2.1 Indentación**
Se deben utilizar 4 espacios para cada nivel de indentación dentro de la clase, cada vez que se encuentre dentro del constructor y de métodos, se le suman otros 4 espacios, así como también dentro de los if, while, for y try. No se deben usar tabulaciones.

Ejemplos:

- **Correcto** 

  public class MiClase

  {

  `    `public void MiMetodo()

  `    `{

  `        `if (true)

  `        `{

  `            `Console.WriteLine("Indentación correcta.");

  `        `}

  `    `}

  }

- **Incorrecto** 

  public class MiClase

  {

  public void MiMetodo() // Sin indentación

  {

  if (true)

  {

  `  `Console.WriteLine("Indentación incorrecta."); // Indentación de 2 espacios

  }

  }

  }


## <a name="_toc206620920"></a><a name="_toc212804977"></a>**2.2 Líneas y espacios en blanco**
Usar líneas en blanco entre métodos y constructores, después de la última variable de instancia, antes de comentarios de una sola línea o en bloque y en secciones lógicas dentro de un método. Los espacios en blanco deben usarse en las siguientes circunstancias: 

- Una palabra clave seguida de un paréntesis debe contener un espacio, después de las comas en una lista de argumentos. 
- No se deben usar paréntesis entre el nombre de un método y su paréntesis de apertura.

Ejemplos:

- **Correcto** 

  public class Calculadora

  {

  `    `public int Sumar(int a, int b)

  `    `{

  `        `if (a < 0 || b < 0)

  `        `{

  `            `throw new ArgumentException("Solo se aceptan números positivos.");

  `        `}

  `        `int resultado = a + b;

  `        `return resultado;

  `    `}

  `    `public int Restar(int a, int b)

  `    `{

  `        `return a x- b;

  `    `}

  }







- **Incorrecto** 

  public class Calculadora

  {

  `    `public int Sumar(int a, int b)

  `    `{

  `        `// Todo el código está amontonado, es difícil de leer

  `        `if (a < 0 || b < 0)

  `        `{

  `            `throw new ArgumentException("Solo se aceptan números positivos.");

  `        `}

  `        `int resultado = a + b;

  `        `return resultado;

  `    `}

  `    `public int Restar(int a, int b)

  `    `{

  `        `return a - b;

  `    `}

  }
## <a name="_toc206620921"></a><a name="_toc212804978"></a>**2.3 Uso de llaves**
Las llaves de las clases, interfaces, constructores y métodos deben ir en su propia línea, en el caso de la llave de apertura ({) debe ser independiente a la línea en la que se hizo la declaración, en el caso de llave de cierre (}) debe estar en una línea sola e indentada para coincidir con la llave del apertura de la declaración (siguiendo el estilo Allman). Los bloques de flujo de control (ifs, whiles, for, entre otros.) siempre deben estar encerrados con llaves, aunque estos solo contengan una línea.

Ejemplos:

- **Correcto** 

  namespace MiProyecto

  {

  `    `public class MiClase

  `    `{

  `        `public void MiMetodo(bool condicion)

  `        `{

  `            `if (condicion)

  `            `{

  `                `Console.WriteLine("Correcto");

  `            `}

  `        `}

  `    `}

  }






- **Incorrecto** 

  namespace MiProyecto { // Llave en la misma línea

  `    `public class MiClase {

  `        `public void MiMetodo(bool condicion) {

  `            `if (condicion) {

  `                `Console.WriteLine("Incorrecto");

  `            `}

  `        `}

  `    `}

  }



# <a name="_toc206620922"></a><a name="_toc212804979"></a>**3. Comentarios**
Los comentarios deben explicar el porqué del código, no el qué.
## <a name="_toc206620923"></a><a name="_toc212804980"></a>**3.1 Comentarios de API pública**
Se deben usar comentarios de API pública (///) para documentar esta misma (clases, métodos, propiedades).

Ejemplos:

- **Correcto** 

  /// <summary>

  /// Calcula el factorial de un número entero no negativo.

  /// </summary>

  /// <param name="numero">El número para calcular el factorial.</param>

  /// <returns>El valor del factorial del número.</returns>

  /// <exception cref="ArgumentException">Se lanza si el número es negativo.</exception>

  public long CalcularFactorial(int numero)

  {

  `    `if (numero < 0)

  `    `{

  `        `throw new ArgumentException("El número no puede ser negativo.", nameof(numero));

  `    `}

  `    `long resultado = 1;

  `    `for (int i = 2; i <= numero; i++)

  `    `{

  `        `resultado \*= i;

  `    `}

  `    `return resultado;

  }



- **Incorrecto** 

  /\*

  Este método calcula el factorial.

  Recibe un número.

  Regresa el resultado.

  \*/

  public long CalcularFactorial(int numero)

  {

  `    `// ...

  }

## <a name="_toc206620924"></a><a name="_toc212804981"></a>**3.2 Comentarios de bloque**
Se deben usar comentarios de bloque (/\*…\*/) para explicar secciones que abarquen más de una línea de código. Estos comentarios deben explicar el por qué del fragmento de código que se comenta.

Ejemplos:

- **Correcto** 

  /\*Comentario de bloque.

  \* Buscaremos explicar el por qué

  \* de una sección de código en particular.

  \*/

- **Incorrecto** 

  /\* Inicializa i en 0 \*/

  int i = 0;

  /\* Incrementa i \*/

  i++;
## <a name="_toc212804982"></a>**3.3 Comentarios de línea única**
Se deben comentarios de línea única (//) como clarificaciones a secciones que sean complejas.

Ejemplos:

- **Correcto** 

  // Si el jugador retrocede demasiado se evita que su posición sea negativa para que pueda seguir jugando

  if (jugador.PosicionX < 0)

  {

  `    `jugador.PosicionX = 0;

  }

- **Incorrecto** 

  // Verifica si la posición es menor a 0

  if (jugador.PosicionX < 0)

  {

  `    `// Coloca la posición en 0

  `    `jugador.PosicionX = 0;

  }
# <a name="_toc206620925"></a><a name="_toc212804983"></a>**4. Estructuras de control**
## <a name="_toc206620926"></a><a name="_toc212804984"></a>**4.1 if / else**
Las estructuras de control if deben tener un espacio antes de la condición. Las llaves de las estructuras else deben ir en líneas independientes.

Ejemplos:

- **Correcto** 

  if (usuario.EstaActivo)

  {

  `    `Console.WriteLine("El usuario está activo.");

  }

  else

  {

  `    `Console.WriteLine("El usuario está inactivo.");

  }

- **Incorrecto** 

  // Sin llaves, propenso a errores

  if (usuario.EstaActivo)

  `    `Console.WriteLine("El usuario está activo.");

  else

  `    `Console.WriteLine("El usuario está inactivo.");
## <a name="_toc206620927"></a><a name="_toc212804985"></a>**4.2 else if**
Las estructuras de control else if debe ir en una línea independiente a la llave de cierre del if anterior, así como las estructuras de control else if deben tener un espacio antes de la condición.



Ejemplos:

- **Correcto**

  if (puntuacion > 90)

  {

  `    `calificacion = "A";

  }

  else if (puntuacion > 80)

  {

  `    `calificacion = "B";

  }

  else

  {

  `    `calificacion = "C";

  }

- **Incorrecto** 

  // Formato inconsistente y difícil de leer

  if (puntuacion > 90) { calificacion = "A"; }

  else if (puntuacion > 80) { calificacion = "B"; }

  else { calificacion = "C"; }
## <a name="_toc206620928"></a><a name="_toc212804986"></a>**4.3 switch**
La estructura de control Switch debe declararse en una línea nueva y debe incluir un espacio en blanco antes de su condición. En cada caso, se debe escribir un nombre descriptivo para el caso, si contiene más de una palabra, deben ir separadas mediante un punto, en formato de PascalCase seguido de dos puntos, y luego hacer un salto de línea con una indentación de cuatro espacios para el procedimiento correspondiente. Debe contener un break al momento de finalizar el caso y se realiza un salto de línea antes de iniciar uno nuevo.

Ejemplos:

- **Correcto** 

  switch (estadoPedido)

  {

  `    `case Estado.Procesando:

  `        `Console.WriteLine("El pedido está en proceso.");

  `        `break;

  `    `case Estado.Enviado:

  `        `Console.WriteLine("El pedido ha sido enviado.");

  `        `break;

  `    `default:

  `        `Console.WriteLine("Estado del pedido desconocido.");

  `        `break;

  }

- **Incorrecto** 

  // Mala indentación y falta de default

  switch (estadoPedido) {

  case Estado.Procesando:

  Console.WriteLine("El pedido está en proceso.");

  break;

  case Estado.Enviado:

  Console.WriteLine("El pedido ha sido enviado.");

  break;

  }
## <a name="_toc206620929"></a><a name="_toc212804987"></a>**4.4 for**
Las expresiones en una declaración for deben estar separadas por espacios en blanco. Después de la palabra reservada for debe existir un espacio en blanco antes de las expresiones y después de ellas. Se debe procurar usar for para evitar el uso del foreach cuando se requiere control del índice o modificaciones en la colección.

Ejemplos:

- **Correcto** 

  int puntajeMaximo = 0;

  for (int i = 0; i < puntajes.Length; i++)

  {

  `    `if (puntajes[i] > puntajeMaximo)

  `    `{

  `        `puntajeMaximo = puntajes[i];

  `    `}

  }

- **Incorrecto** 

  // Sin llaves y sin espacios, difícil de leer

  for(int i=0;i<puntajes.Length;i++) if(puntajes[i] > puntajeMaximo) puntajeMaximo = puntajes[i];
## <a name="_toc206620930"></a><a name="_toc212804988"></a>**4.5 foreach**
Las expresiones en una declaración foreach deben estar separadas por espacios en blanco. Después de la palabra reservada for debe existir un espacio en blanco antes de la variable de iteración y después de ella. Se debe usar el foreach cuando se trata como iterator sobre una colección, es decir, cuando se recorre la colección completa de manera secuencial y solo para lectura o procesamiento directo de sus elementos.


Ejemplos:

- **Correcto** 

  foreach (var enemigo in enemigos)

  {

  `    `Console.WriteLine($"Enemigo: {enemigo.Nombre}");

  }

- **Incorrecto** 

  // Sin llaves y sin espacios, difícil de leer

  foreach(var enemigo in enemigos) Console.WriteLine($"Enemigo: {enemigo.Nombre}");

  // Intentar modificar la colección dentro del foreach

  foreach (var enemigo in enemigos)

  {

  `    `if (enemigo.Nombre == "Villano")

  `    `{

  `        `enemigos.Remove(enemigo);

  `    `}

  }
## <a name="_toc212804989"></a>**4.6 while**
La estructura while debe incluir un espacio en blanco antes de la condición Las llaves de esta estructura deben ir en líneas independientes.

Ejemplos:

- **Correcto** 

  int contador = 0;

  while (contador < 5)

  {

  `    `Console.WriteLine("Iteración en while.");

  `    `contador++;

  }

- **Incorrecto** 

  // Sin llaves

  int contador = 0;

  while (contador < 5)

  `    `contador++;



## <a name="_toc206620931"></a><a name="_toc212804990"></a>**4.7 do while**
En la estructura do while, el do debe ir independiente de sus llaves, así como su llave de apertura debe incluir un salto de línea para el desarrollo del bloque de instrucciones. Al finalizar este bloque, se debe dar un salto de línea antes de la llave de cierre, y con un espacio de separación, mediante la escritura del while, con una separación posterior a esta palabra reservada para la condición a cumplir para finalizar el do while.

Ejemplos:

- **Correcto** 

  string entrada;

  do

  {

  `    `Console.WriteLine("Escribe 'salir' para terminar:");

  `    `entrada = Console.ReadLine();

  } while (entrada != "salir");


- **Incorrecto** 

  // Mala ubicación del while

  string entrada;

  do {

  `    `Console.WriteLine("Escribe 'salir' para terminar:");

  `    `entrada = Console.ReadLine();

  } 

  while (entrada != "salir");
# <a name="_toc206620932"></a><a name="_toc212804991"></a>**5. Excepciones y Errores**
Es crucial una estrategia robusta para el manejo de excepciones al momento de crear aplicaciones confiables y mantenibles. El objetivo es manejar errores inesperados de manera controlada, evitar la terminación abrupta del programa y facilitar la depuración mediante el registro de información útil.
## <a name="_toc212804992"></a>**5.1 Uso de bloques try-catch**
- Manejo Específico: Se deben capturar únicamente las excepciones que se puedan manejar de forma efectiva en el bloque catch. Se debe evitar capturar la clase base System.Exception a menos que sea en un manejador global de alto nivel para registrar el error antes de que la aplicación finalice o para relanzar la excepción.
- Evitar Ocultar Errores: No debe ignorarse una excepción que se captura (es decir, dejarla con un bloque catch vacío). Si no se puede manejar la excepción, se debe permitir su se propagación a un nivel superior en la pila de llamadas.
- Uso de finally: Se debe utilizar el bloque finally para liberar recursos, como conexiones a bases de datos o archivos, para garantizar que este código se ejecute incluso si ocurre una excepción.
- Prevenir Excepciones: Cuando sea posible, se debe lanzar una excepción controlando las condiciones de antemano. 

Ejemplos:

- **Correcto** 

  try

  {

  `    `// Lógica 

  }

  catch (FileNotFoundException ex)

  {

  `    `// Lógica

  `    `Logger.LogError("El archivo de configuración no fue encontrado.", ex);

  }

  catch (InvalidOperationException ex)

  {

  `    `// Lógica

  `    `Logger.LogError("La operación no pudo ser completada.", ex);

  }

- **Incorrecto** 

try

{

`    `// Lógica de la aplicación

}

catch (Exception ex) 

{

`    `// Esto es demasiado genérico y puede ocultar errores inesperados

`    `// que deberían ser manejados en niveles superiores o causar una falla controlada.

}
**\

## <a name="_toc212804993"></a>**5.2 Bitácora**
La bitácora servirá como el registro histórico del comportamiento de la aplicación, siendo una fuente indispensable para el diagnóstico de problemas, el monitoreo en producción y la auditoría de eventos.

Es importante notar que la siguiente tabla de niveles es una guía base. El equipo puede (y debe) ampliarla identificando nuevos tipos de eventos o excepciones y asignándolos a su Nivel correspondiente, siempre manteniendo la coherencia en toda la aplicación con la decisión y la consistencia en todo el proyecto según lo definido.

|**Nivel**|**Importancia**|**Descripción**|**Acción Requerida**|**Habilitado** en producción|**Ejemplos específicos**|
| :- | :- | :- | :- | :- | :- |
|**Fatal**|**Crítica**|Error catastrófico que detiene la aplicación.|**Inmediata.** Generar alertas.|**Sí**|<p>**Cualquier evento o excepción que detenga la total ejecución del programa.<br><br>-Excepciones no manejadas.**</p><p>**-OutOfMemoryException o StackOverflowException.**</p><p>**-Falla crítica al iniciar.**</p>|
|**Error**|**Alta**|Una operación falló, pero la aplicación sigue funcionando.|<p>**Prioritaria.** </p><p>Investigar.</p>|**Sí**|<p>**Cualquier evento o excepción que sucede durante la ejecución del programa, que es atrapado y muestra un mensaje (evitando la detención de la ejecución del programa)**</p><p></p><p>**-SQL: SqlException** </p><p>**- HttpRequestException, TimeoutException,** </p><p>**-FaultException**</p><p>**-InvalidOperationException.**</p><p>**-NullReferenceException, ArgumentException**</p>|
|**Info**|**Normal**|Registro de eventos de auditoría y el flujo normal de la aplicación.|**Solo para auditoría y monitoreo de flujo.** |**Sí (por auditoría)**|<p>**Información general sobre acciones o eventos que suceden en la ejecución:**</p><p></p><p>**-"Servicio XYZ iniciado correctamente."**</p><p>**-"Usuario 'user@email.com' inició sesión."**</p>|

# <a name="_toc212804994"></a>**6. Pruebas**
Las pruebas unitarias son una parte fundamental en el ciclo de vida del desarrollo de software. Su objetivo principal es verificar el correcto funcionamiento de las unidades de código más pequeñas y aisladas de una aplicación, como pueden ser funciones, métodos o clases. Al probar estos componentes de forma individual, se puede garantizar que cada pieza del software se comporta según lo esperado antes de ser integrada con el resto del sistema.
## <a name="_toc212804995"></a>**6.1 Pruebas unitarias**
Para mantener la consistencia y claridad en nuestro código, se establece una convención de nomenclatura obligatoria para todas las pruebas unitarias. Cada una de las pruebas deberán estar marcados con el atributo [TestClass]. Cada nombre de prueba deberá seguir la estructura Prefijo\_NombreDeLaFuncionalidad\_FlujoDeLaPrueba. En esta, el Prefijo será siempre "Prueba" para identificar el método, el NombreDeLaFuncionalidad corresponderá al método o componente bajo evaluación, y el FlujoDeLaPrueba describirá de forma concisa el escenario o el resultado esperado. Este estándar garantiza que el propósito de cada prueba sea evidente a simple vista, facilitando así la mantenibilidad y la comprensión del código.

Se debe evitar las dependencias a tiempo / IO real, en su lugar, se debe usar fakes o mocks.

Ejemplos:

- **Correcto** 

  [TestMethod]

  public void PruebaIniciarSesionCorrecto()

  {

  `    `// Lógica de la prueba

  }

  [TestMethod]

  public void PruebaIniciarSesionContrasenaIncorrecta()

  {

  `    `// Lógica de la prueba

  }


  [TestMethod]

  public void PruebaIniciarSesionCamposVacios()

  {

  `    `// Lógica de la prueba

  }

- **Incorrecto** 

  [TestMethod]

  public void PruebaLogin() 

  {

  `    `// Lógica de la prueba

  }
# <a name="_toc212804996"></a>**7. Seguridad**
En el aspecto de seguridad, se tendrán en cuenta aspectos sobre las siguientes directrices:

- Sobre las contraseñas: deberán tener un mínimo de 8 caracteres y un máximo de 15, así como incluir al menos una letra mayúscula, un número y un carácter especial. Su validación será mediante técnicas de escaping y de sanitización de entradas.
- Sobre la base de datos: se emplearán usuarios con permisos restringidos y específicos para cada necesidad, evitando el uso de cuentas genéricas o con privilegios excesivos.
- Sobre la gestión de contraseñas: todas las contraseñas serán almacenadas de forma segura mediante un algoritmo de hash.
- Para el acceso a credenciales: Se utilizará un archivo separado (app.config) para que las credenciales no se encuentren directamente en el código fuente. Este archivo utilizará variables de entorno para el almacenamiento de la información y estará encriptado.
- **Correcto** 

  Segur1dad!

  Cumple con: 9 caracteres, incluye mayúscula, número y carácter especial.

- **Incorrecto** 

  password123

  No cumple: es demasiado simple, no tiene carácter especial ni mayúscula.
# <a name="_toc212804997"></a>**8. Prefijos**
## <a name="_toc206620933"></a><a name="_toc212804998"></a>**8.1 Capa gráfica (XAML)**
La convención propuesta para nombrar los componentes de la interfaz de usuario en XAML seguirá el formato camelCase, combinando el nombre del componente con un nombre descriptivo específico, todo junto. La estructura será: nombreComponenteEspanolNombreEspecifico.

La lista de prefijos es una base y puede ser ampiable. Si se usa un control que no se define explícitamente aquí, se debe definir un prefijo coherente y usarlo consistemente en el proyecto.

1. Button**:** boton
1. TextBox**:** campoTexto
1. TextBlock**:** bloqueTexto
1. ComboBox**:** cuadroCombinado
1. CheckBox**:** casillaVerificacion
1. RadioButton**:** botonOpcion
1. ListBox**:** cuadroLista
1. Image**:** imagen
1. Grid**:** cuadricula
1. StackPanel**:** panelApilable
- **Correcto** 

  <StackPanel x:Name="panelApilablePrincipal">

  `    `<TextBlock x:Name="bloqueTextoTitulo" Text="Registro"/>

  `    `<TextBox x:Name="campoTextoNombreUsuario"/>

  `    `<Button x:Name="botonEnviarFormulario" Content="Enviar"/>

  </StackPanel>

- **Incorrecto** 

  <StackPanel x:Name="StackPanel1">

  `    `<TextBlock x:Name="textblock\_titulo" Text="Registro"/>

  `    `<TextBox x:Name="Nombre"/>

  `    `<Button x:Name="Button\_Enviar" Content="Enviar"/>

  </StackPanel>
**\

# <a name="_toc212804999"></a>**9. Base de datos**
Para la gestión de la base de datos del proyecto se utilizará SQL Server junto con su herramienta de administración SQL Server Management Studio. El diseño de la base de datos se basará en un modelo relacional, estableciendo conexiones entre las tablas a través de llaves primarias y foráneas para garantizar la integridad y consistencia de los datos.
## <a name="_toc212805000"></a>**9.1 Tablas**
El nombramiento de las tablas debe ser descriptivo y reflejar la entidad que almacenan. Se utilizará la notación PascalCase, donde el nombre de la tabla comienza con una letra mayúscula y, si se compone de varias palabras, estas irán unidas sin espacios y cada una comenzará con mayúscula.

Ejemplos:

- **Correcto** 

  CREATE TABLE Usuario ( ... );

  CREATE TABLE Jugador ( ... );

- **Incorrecto** 

  CREATE TABLE historial\_de\_partidas ( ... );

  CREATE TABLE TblUsuarios ( ... );
## <a name="_toc212805001"></a>**9.2 Llaves primarias**
Las llaves primarias son el identificador único de cada registro en una tabla. Para su nombramiento, se utilizará la notación camelCase con el prefijo "id", seguido del nombre de la entidad a la que pertenece la tabla.

Ejemplos:

- **Correcto** 

  CREATE TABLE Jugador (

  `    `idJugador INT PRIMARY KEY,

      ...

  );









- **Incorrecto** 

CREATE TABLE Jugador (

`    `Id\_Jugador INT PRIMARY KEY,

...

);

CREATE TABLE Jugador (

`    `id INT PRIMARY KEY,

...

);
## <a name="_toc212805002"></a>**9.3 Atributos**
Los atributos o columnas de una tabla deben tener nombres descriptivos. Se nombrarán utilizando PascalCase.

Ejemplos:

- **Correcto** 

  CREATE TABLE Partida (

  `    `idPartida INT PRIMARY KEY,

  `    `FechaCreacion DATETIME,

  `    `RondasGanadas INT

  );

- **Incorrecto** 

CREATE TABLE Partida (

`    `idPartida INT PRIMARY KEY,

`    `fecha\_Creacion DATETIME,

`    `rondas\_ganadas INT

);
## <a name="_toc212805003"></a>**9.4 Llaves foráneas**
Las llaves foráneas se utilizan para establecer una relación entre dos tablas. Su nombramiento seguirá el formato de las llaves primarias, pero se le añadirá como prefijo el nombre de la tabla de la cual es foránea, seguido de un guion bajo.

Ejemplos:

- **Correcto** 

  CREATE TABLE Puntuacion (

  `    `idPuntuacion INT PRIMARY KEY,

  `    `Jugador\_idJugador INT,

  `    `FOREIGN KEY (Jugador\_idJugador) REFERENCES Jugador(idJugador)

  );

- **Incorrecto** 

CREATE TABLE Puntuacion (

`    `idPuntuacion INT PRIMARY KEY,

`    `idDelJugador INT,

`    `FOREIGN KEY (idDelJugador) REFERENCES Jugador(idJugador)

);
# <a name="_toc206620934"></a><a name="_toc212805004"></a>**10. Bibliografía**
Declaración de uso de IA: se uso para corregir redacción y ejemplos demostrativos de los diferentes puntos de este estándar.

- BillWagner. (2023, December 15). *C# identifier names*. Learn.microsoft.com. <https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names>
- BillWagner. (2025, January 22). *Convenciones de código de .NET - C#*. Microsoft.com. <https://learn.microsoft.com/es-es/dotnet/csharp/fundamentals/coding-style/coding-conventions>
- BillWagner. (2025, July 31). *Comentarios de comentarios de documentación de la API XML: API de documento con comentarios /// - C# reference*. Microsoft.com. <https://learn.microsoft.com/es-es/dotnet/csharp/language-reference/xmldoc/>
- BillWagner(2025, June 19) *Excepciones y control de excepciones. [*https://learn.microsoft.com/es-es/dotnet/csharp/fundamentals/exceptions/*](https://learn.microsoft.com/es-es/dotnet/csharp/fundamentals/exceptions/)*
- BillWagner. (2025, July 31). palabra clave private - C# reference. Microsoft.com. <https://learn.microsoft.com/es-es/dotnet/csharp/language-reference/keywords/private>

2

