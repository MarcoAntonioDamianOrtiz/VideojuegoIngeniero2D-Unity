# Escena fiel a la referencia

Abre `Assets/Scenes/EscenaNivel1_FielReferencia.unity` después de actualizar el proyecto. La escena conserva el jugador, su cámara, el movimiento y las colisiones principales del plano de 64 x 64 unidades. El mapa visual se encuentra en `Assets/Technopolis/Nivel1/Sprites/04_Objetos/mapa_nivel1_fiel_referencia.png`.

El fondo se creó tomando la ilustración de referencia como guía visual. Respeta la posición relativa del callejón, el tren, las viviendas, los huertos, la casa abierta de Alex, la panadería, la miscelánea, la plaza, el taller, el parque y la salida cerrada. Se eliminaron las etiquetas numeradas y la leyenda editorial para que el mapa pueda usarse dentro del juego.

La imagen completa es una sola capa visual. Las 53 huellas de obstáculos del plano original permanecen como colliders, pero el dibujo generado no coincide exactamente con cada borde de collider. El personaje se dibuja delante del fondo; todavía no hay recortes de primer plano para ocultarlo detrás de techos o árboles. La escena `EscenaNivel1_Referencia.unity` conserva los sprites separados y sigue siendo la opción editable objeto por objeto.

Para regenerar la escena desde el plano se ejecuta `Tools/Nivel1/bake_scene.py --fiel`. El archivo PNG y su `.meta` están versionados, por lo que abrir la escena no requiere ejecutar Python.
