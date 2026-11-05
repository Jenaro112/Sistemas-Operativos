# TP5 — Respuestas teóricas y de análisis

Los valores citados salen de los registros de la carpeta `registros/`.

---

## Ejercicio 1 — Algoritmo de Cristian

### ¿Cómo influye la latencia de red en la precisión de la sincronización?

Cristian estima la hora como `Ts + RTT/2`: supone que la respuesta tardó en volver **la mitad** del
tiempo de ida y vuelta. Por eso la precisión depende directamente de la latencia:

- El error máximo posible es **±RTT/2** (todo el retardo pudo estar en la ida o todo en la vuelta).
  Con RTT ≈ 1 ms la precisión fue de ±0,5 ms; con RTT ≈ 550 ms bajó a ±275 ms.
- Si la red es **asimétrica** (ida ≠ vuelta), aparece un error sistemático de `(ida − vuelta)/2`.
  En la ronda 4 se simuló ida = 450 ms y vuelta = 50 ms: el servidor-B corrigió **+200,4 ms** y el C
  **+212,1 ms** de más, justamente ≈ (450 − 50)/2 = 200 ms.
- La **variación** de la latencia (jitter) hace que cada muestra dé un resultado distinto. Por eso el
  cliente toma varias muestras y se queda con la de **menor RTT**, que es la más acotada (en la primera
  ronda la primera muestra tardó entre 150 y 337 ms por el arranque en frío y las siguientes ~1 ms).

### ¿Qué sucede si el tiempo de transmisión de ida y vuelta es muy elevado?

La incertidumbre (±RTT/2) se vuelve más grande que la corrección que se quiere hacer, y además la hora
recibida ya está "vieja" al llegar. Ajustar el reloj con ese dato puede **empeorarlo**. La solución
implementada es descartar las mediciones con `RTT > MAX_RTT_MS`: en la ronda 5 (RTT ≈ 1200 ms > 1000 ms)
los tres clientes registraron *"NO se ajusta el reloj"* y conservaron la corrección anterior. Otras
mejoras posibles: repetir la medición, promediar las de menor RTT o elegir un servidor de tiempo más
cercano.

### ¿Qué ocurre si el servidor de tiempo deja de responder?

El servidor es un **punto único de falla**. En la ronda 6 todas las solicitudes vencieron por timeout
(*"Servidor de tiempo NO RESPONDE"*). El cliente no se bloquea: mantiene la última corrección y sigue
funcionando, pero su reloj de aplicación avanza con la deriva del reloj local (−800 ppm en C, +1000 ppm
en B) y el error crece con el tiempo. Cuando el servidor volvió (ronda 7) se resincronizó solo: el
servidor-C corrigió −179 ms acumulados durante la caída. Para evitarlo en producción se usan varios
servidores de tiempo (como NTP con múltiples fuentes) y reintentos con backoff.

---

## Ejercicio 2 — Algoritmo de Berkeley

### Resultados de las pruebas

| Escenario | Dispersión antes | Después de la ronda 1 | Después de la ronda 2 |
|---|---|---|---|
| 01 · nodos con +5, −10 y +20 s | 30,0 s | 34 ms | 0,2 ms |
| 02 · nodo-3 con +5 min, promedio simple | 310,0 s | 37 ms | 0,1 ms |
| 03 · nodo-3 con +5 min, promedio tolerante | 310,0 s | 33 ms | 0,2 ms |
| 04 · nodo-2 desconectado en rondas 2-3 | 30,0 s | 35 ms | 1,7 ms (sin nodo-2) |

El residuo de ~35 ms en la primera ronda viene del RTT alto de la primera solicitud (arranque en frío),
que hace menos exacta la compensación RTT/2; desde la segunda ronda queda por debajo de 2 ms.

**Nodo con 5 minutos de diferencia (escenario 02):** el promedio fue
`(0 + 5,0 − 10,0 + 300,0) / 4 = +73,8 s`. Todos quedaron de acuerdo entre sí, pero **todos se adelantaron
73,8 s** (el coordinador se ajustó +73,8 s, el nodo-2 +83,7 s), arrastrados por el nodo defectuoso.
Con el **promedio tolerante a fallas** (escenario 03, se descartan relojes a más de 30 s de la mediana)
el nodo-3 quedó EXCLUIDO del promedio: `(0 + 5,0 − 10,0) / 3 = −1,6 s`. El grupo apenas se movió y al
nodo-3 igual se le envió su ajuste (−301,7 s) para corregirlo.

**Desconexión (escenario 04):** el nodo-2 no respondió en las rondas 2 y 3; el coordinador lo marcó
*SIN RESPUESTA*, calculó el promedio con los otros tres y siguió funcionando. Al reconectarse se volvió a
incluir y recibió su ajuste en la ronda 4.

### ¿Qué diferencias existen entre Cristian y Berkeley?

| | Cristian | Berkeley |
|---|---|---|
| Iniciativa | El **cliente** pregunta la hora (pull) | El **coordinador** consulta a todos (poll) |
| Referencia | El servidor de tiempo se supone **exacto** (ej. con UTC) | Ningún reloj es la referencia: se usa el **promedio** |
| Objetivo | Sincronización **externa** (con una fuente de hora real) | Sincronización **interna** (que coincidan entre sí) |
| Qué se envía | La **hora** del servidor | El **ajuste** (+/−) que cada nodo debe aplicar |
| ¿Quién se ajusta? | Solo los clientes | Todos, incluido el coordinador |

Enviar un ajuste relativo en lugar de una hora absoluta hace que Berkeley no dependa tanto de la demora
del mensaje de respuesta.

### ¿Qué sucede cuando un nodo presenta un reloj considerablemente desfasado?

Con el promedio simple, el valor extremo **desplaza el promedio** y todo el grupo termina corrido (+73,8 s
en la prueba). Los relojes quedan sincronizados entre sí, pero lejos de la hora real. Por eso Berkeley
propone un **promedio tolerante a fallas**: descartar los relojes que se alejan demasiado de los demás
(aquí, respecto de la mediana) y corregir igual a los nodos descartados.

### ¿Qué ocurre si el coordinador deja de funcionar?

No se realizan más rondas: cada nodo sigue con su propio reloj y la deriva vuelve a separarlos (en el
escenario 05, a los 12 s sin ajustes cada nodo registró *"Sin ajustes del coordinador… El grupo deja de
estar sincronizado"*). Los servicios no se caen, pero se pierde la sincronización. El coordinador es un
punto único de falla.

### ¿Qué modificaciones permitirían tolerar la falla del coordinador?

- **Elección de un nuevo coordinador** (algoritmo del **matón/Bully** o de **anillo**) cuando los nodos
  detectan que no reciben ajustes; el `CoordinatorWatchdog` de cada nodo es el punto donde se iniciaría.
- **Heartbeats** entre coordinador y nodos para detectar la caída rápido.
- **Coordinador de respaldo** (primario/secundario) que tome el control si el primario no responde.
- **Algoritmos descentralizados** (promedio distribuido): cada nodo consulta a los demás y ajusta su reloj
  con el promedio, sin coordinador.
- Persistir la configuración del grupo para que el nuevo coordinador conozca a los participantes.

---

## Ejercicio 3 — Relojes lógicos de Lamport

En la traza (`registros/ejercicio3-lamport/traza-completa.txt`) InventoryService (reloj −4 s) y
PaymentsService (−2 s) registran **recepciones con hora física anterior al envío**, por ejemplo:
*Orders envía a las 19:35:45.511 (L=2) → Inventory recibe a las 19:35:41.755 (L=3)*. Ordenando por
reloj físico el resultado es imposible; ordenando por Lamport siempre se cumple envío < recepción.

### ¿Por qué los valores de Lamport no representan la hora real?

Porque son **contadores de eventos**, no medidas de tiempo. Solo aumentan cuando ocurre algo (un evento
interno, un envío o una recepción) y no tienen unidad: L = 12 no significa "12 segundos", y entre L = 7
y L = 8 pudieron pasar 1 ms o 1 hora. Lo único que garantizan es una relación de orden coherente con la
causalidad, independientemente de los relojes físicos.

### ¿Qué ocurre cuando un proceso recibe un mensaje con un valor lógico superior al suyo?

Adelanta su reloj: `L = max(L_local, L_mensaje) + 1`. Ejemplo del registro: PaymentsService estaba en 1
y recibió la solicitud de pago con marca 7 → pasó a `max(1, 7) + 1 = 8`. Así se asegura que la recepción
tenga una marca mayor que el envío, aunque el receptor haya tenido pocos eventos propios.

### ¿Cómo permite Lamport mantener el orden causal de los eventos?

Por la relación "sucedió antes" (→): si a y b son del mismo proceso y a ocurre antes, o si a es el envío
y b la recepción del mismo mensaje, entonces a → b. Las tres reglas garantizan la **condición de reloj**:

> si a → b  entonces  L(a) < L(b)

Las marcas crecen dentro de cada proceso y cada recepción queda por encima de su envío, así que toda
cadena causal tiene marcas crecientes. Ordenando por (L, nombre de proceso) se obtiene un **orden total**
que respeta la causalidad (es lo que muestra `GET /api/trace/text`).

### ¿Es posible determinar si dos eventos son concurrentes utilizando únicamente sus marcas de Lamport?

**No.** La implicación es en un solo sentido: `L(a) < L(b)` **no** implica `a → b`.

- PaymentsService "Conciliación bancaria" (L = 1) y OrdersService "Pedido recibido P-001" (L = 1) tienen
  la misma marca y son concurrentes: con marcas iguales sí se puede afirmar que no hay relación causal.
- InventoryService "Recuento de depósito" (L = 6) y OrdersService "Pedido recibido P-002" (L = 13) también
  son concurrentes (no hay mensajes que los conecten), aunque 6 < 13. Solo con las marcas no se puede
  distinguir este caso de uno causal.

Para detectar concurrencia se necesitan **relojes vectoriales**: a ∥ b si ni V(a) ≤ V(b) ni V(b) ≤ V(a).

---

## Ejercicio 4 — Comparación de algoritmos

| Característica | Cristian | Berkeley | Lamport |
|---|---|---|---|
| **Tipo de reloj** | Físico | Físico | Lógico (contador) |
| **Objetivo principal** | Sincronización externa: ajustar cada cliente a la hora de un servidor confiable | Sincronización interna: que todos los relojes del grupo coincidan entre sí (promedio) | Ordenar eventos según la causalidad, sin importar la hora real |
| **Requiere servidor central** | Sí, servidor de tiempo | Sí, coordinador (puede elegirse dinámicamente) | No, cada proceso mantiene su contador |
| **Utiliza mensajes** | Sí: solicitud/respuesta de hora (2 por cliente) | Sí: consulta de hora + envío de ajuste a cada nodo (≈3 por nodo y ronda) | Sí, pero aprovecha los mensajes de la aplicación agregándoles la marca L |
| **Sincroniza hora física** | Sí | Sí (respecto del promedio, no necesariamente de la hora real) | No |
| **Establece orden causal** | No (solo aproxima; con errores de ±RTT/2 el orden puede invertirse) | No | Sí: a → b ⇒ L(a) < L(b) (orden total con desempate) |
| **Comportamiento ante fallas** | El servidor es punto único de falla; los clientes siguen con su reloj y acumulan deriva. Sensible a latencias altas o asimétricas | El coordinador es punto único de falla (se mitiga con elección). Tolera la caída de nodos (se excluyen). Un reloj muy desfasado distorsiona el promedio si no se usa promedio tolerante | No hay punto central. Si un proceso cae, los demás siguen. Un mensaje perdido solo afecta a esa relación causal. No detecta concurrencia |

### 1. ¿Qué algoritmo utilizaría para sincronizar los relojes de varios servidores?

Depende de si existe una fuente de hora confiable:

- Si hay una fuente de UTC confiable (un servidor NTP o un reloj GPS): **Cristian**, o directamente
  **NTP**, que es su evolución jerárquica con varias fuentes, filtrado de muestras y corrección gradual.
- Si lo importante es que los servidores **coincidan entre sí** y no hay una fuente externa confiable:
  **Berkeley**, con promedio tolerante a fallas y elección de coordinador.

### 2. ¿Qué algoritmo utilizaría para establecer el orden causal de operaciones entre microservicios?

**Lamport** (o **relojes vectoriales** si además hay que detectar operaciones concurrentes o conflictos).
Los relojes físicos de los microservicios nunca están perfectamente sincronizados, y como se vio en el
Ejercicio 3, un desfase de pocos segundos ya invierte el orden envío → recepción. Lamport no depende de
esos relojes: solo agrega un entero a cada mensaje.

### 3. ¿Qué diferencias existen entre sincronización física y sincronización lógica?

| Sincronización física | Sincronización lógica |
|---|---|
| Ajusta los relojes para que marquen la **misma hora** (real o acordada) | No ajusta la hora: asigna **marcas de orden** a los eventos |
| Responde "¿cuándo ocurrió?" | Responde "¿qué ocurrió antes?" |
| Siempre tiene un error residual (latencia, deriva) | Es exacta respecto de la causalidad, sin error de medición |
| Necesita intercambiar mensajes periódicos y suele tener un servidor central | Viaja en los mensajes de la propia aplicación, sin servidor central |
| Ej.: Cristian, Berkeley, NTP | Ej.: Lamport, relojes vectoriales |

### 4. ¿Es posible combinar diferentes algoritmos en una misma arquitectura distribuida? Justifique.

**Sí, y es lo habitual**, porque resuelven problemas distintos y se complementan:

- Los servidores sincronizan su reloj físico con **NTP/Cristian** contra una fuente externa (o con
  **Berkeley** dentro de un cluster aislado), para que logs, auditorías, expiración de tokens y
  certificados tengan una hora razonable.
- Al mismo tiempo, los microservicios agregan marcas de **Lamport/vectoriales** a sus mensajes para
  ordenar causalmente las operaciones, ya que la sincronización física nunca es perfecta.
- Se pueden combinar dentro de un mismo dato: los **Hybrid Logical Clocks (HLC)** guardan la hora física
  junto a un contador lógico, y Google Spanner usa TrueTime (física con intervalo de error) para ordenar
  transacciones.

En el caso de la empresa del TP: Cristian/Berkeley para que la columna "fecha y hora" de los registros
sea legible y aproximadamente correcta, y Lamport para garantizar que el orden de las operaciones
relacionadas (pedido → inventario → pago) sea siempre el correcto.
