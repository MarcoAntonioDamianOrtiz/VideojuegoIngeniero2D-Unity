# Escena fiel a la referencia

`Assets/Scenes/EscenaNivel1_FielReferencia.unity` es la escena principal (indice 0 en Build Settings). Conserva el jugador, su camara y su movimiento. El mapa visual se encuentra en `Assets/Technopolis/Nivel1/Sprites/04_Objetos/mapa_nivel1_fiel_referencia.png`.

El fondo se creó tomando la ilustración de referencia como guía visual. Respeta la posición relativa del callejón, el tren, las viviendas, los huertos, la casa abierta de Alex, la panadería, la miscelánea, la plaza, el taller, el parque y la salida cerrada. Se eliminaron las etiquetas numeradas y la leyenda editorial para que el mapa pueda usarse dentro del juego.

La imagen completa es una sola capa visual debajo del jugador. `Grid` queda al final de la jerarquia y el fondo tiene orden de dibujo -1000. Las colisiones de esta escena ya no reutilizan el plano anterior: las 49 formas especificas de la ilustracion estan en `Tools/Nivel1/colisiones_mapa_fiel.json`, agrupadas por edificios y objetos. Los muros de la casa de Alex dejan libre su puerta, y la salida cerrada tiene barrera y laterales. El personaje se dibuja delante del fondo; todavia no hay recortes de primer plano para ocultarlo detras de techos o arboles. La escena `EscenaNivel1_Referencia.unity` conserva los sprites separados y sigue siendo la opcion editable objeto por objeto.

Para regenerar la escena se ejecuta `Tools/Nivel1/bake_scene.py --fiel`. Antes de publicarla se ejecuta `Tools/Nivel1/check_fiel_collisions.py`, que comprueba la puerta, catorce puntos de paso y que la salida cerrada no se pueda rodear. El archivo PNG y su `.meta` estan versionados, por lo que abrir la escena no requiere ejecutar Python.
