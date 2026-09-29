# Ambientación del Nivel 1

La opción **Tools > Technopolis > Nivel 1 > 07 Completar ambientación del mapa** aproxima la composición del plano de referencia del barrio sobre la escena existente `Assets/Scenes/EscenaNivel1.unity`.

Antes de ejecutar, detener Play y guardar la escena. La opción crea un respaldo en `Assets/Scenes/Respaldos`, conserva `Jugador`, `suelo_base`, `Caminos` y los edificios existentes, y añade:

- Dos viviendas en el área norte, mercado y grupos de objetos por zona.
- Columpios y resbaladilla en el parque; abre la valla que cerraba el paso sur.
- Muro perimetral visual y portón de la salida bloqueada.
- Una capa independiente `Grid/Ambientacion_Piso` con detalles sin colisión.

Las instancias propias de la opción 07 están agrupadas en `Ambientacion_PlanoReferencia` y `Ambientacion_Edificios`. Reejecutar la opción reconstruye solamente esos grupos y la capa de detalles, después de hacer otro respaldo. También recoloca los dos árboles y la fuente del parque.

Los cuatro sprites nuevos en `Sprites/04_Objetos` son RGBA con fondo transparente y punto de filtrado, a 32 píxeles por unidad: `columpios_viejos` (128×128), `resbaladilla_vieja` (128×128), `porton_salida_bloqueada` (192×96) y `muro_perimetral` (128×48). Se generaron para este proyecto a partir de una descripción del plano; las demás piezas provienen de los assets que ya estaban en el repositorio.

El plano compartido es una ilustración conceptual: sus etiquetas y perspectiva no forman tiles exactos. Comprobar la composición final en **Scene** y los accesos en **Play** antes de subir la escena editada.
