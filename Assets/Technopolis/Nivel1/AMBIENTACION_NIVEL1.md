# Ambientación del Nivel 1

La opción **Tools > Technopolis > Nivel 1 > 07 Completar ambientación del mapa** aproxima la composición del plano de referencia del barrio sobre la escena existente `Assets/Scenes/EscenaNivel1.unity`.

Antes de ejecutar, detener Play y guardar la escena. La opción crea un respaldo en `Assets/Scenes/Respaldos`, conserva `Jugador`, `suelo_base`, `Caminos` y los edificios existentes, y añade:

- Dos viviendas en el área norte y una al sur; puesto del mercado, dos huertos y grupos de objetos por zona.
- Árbol central con jardinera en la plaza y un contenedor grande en el callejón.
- Parque ampliado con columpios y resbaladilla; abre y recoloca la valla del paso sur.
- Muro perimetral visual y portón de la salida bloqueada.
- Capas independientes `Grid/Ambientacion_Parque`, `Grid/Ambientacion_Plaza` y `Grid/Ambientacion_Piso`: césped, concreto envejecido y detalles. No alteran los Tiles originales.

Las instancias propias de la opción 07 están agrupadas en `Ambientacion_PlanoReferencia` y `Ambientacion_Edificios`. Reejecutar la opción reconstruye solamente esos grupos y las tres capas de ambientación, después de hacer otro respaldo. También recoloca los dos árboles, la fuente, las cercas del parque y un árbol pequeño de la plaza.

Los ocho sprites nuevos en `Sprites/04_Objetos` son PNG con transparencia y punto de filtrado, a 32 píxeles por unidad: `columpios_viejos` (128×128), `resbaladilla_vieja` (128×128), `porton_salida_bloqueada` (192×96), `muro_perimetral` (128×48), `arbol_plaza_monumental` (224×240), `contenedor_basura_callejon` (160×128), `puesto_mercado_lona` (160×128) y `huerto_comunitario` (160×96). Se generaron para este proyecto a partir de una descripción del plano; las demás piezas provienen de los assets que ya estaban en el repositorio.

El plano compartido es una ilustración conceptual: sus etiquetas y perspectiva no forman tiles exactos. Comprobar la composición final en **Scene** y los accesos en **Play** antes de subir la escena editada.
