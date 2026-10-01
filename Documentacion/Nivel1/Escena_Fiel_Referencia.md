# Escena fiel a la referencia

`Assets/Scenes/EscenaNivel1_FielReferencia.unity` es la escena principal (indice 0 en Build Settings). Conserva el jugador, su movimiento y la camara. `Grid` queda al final de la jerarquia, con los tilemaps de suelo y senderos debajo de las construcciones y los objetos.

El escenario se compone de 4096 tiles de terreno, 628 tiles de camino y 785 sprites independientes. Las casas, comercios, arboles, mobiliario y muros se pueden seleccionar y editar por separado. La nueva `vivienda_ocre_lamina.png` aporta otra variante de vivienda. El antiguo `mapa_nivel1_fiel_referencia.png` se conserva solo como referencia historica; la escena principal no lo renderiza.

Las colisiones se generan a partir de las piezas visibles, no de una silueta de la imagen completa. Las bases de las viviendas dejan libres los senderos; la casa de Alex conserva su puerta abierta; las zonas de parque y plaza tienen huellas pequenas en sus objetos. La salida comercial permanece bloqueada con muros laterales y barrera. `Tools/Nivel1/colisiones_generadas_fiel.json` registra las huellas usadas en la comprobacion de rutas.

La camara usa Pixel Perfect Camera (32 pixeles por unidad, referencia 640 x 360, filtro Point) para evitar el suavizado y la perdida de nitidez durante el juego. Los sprites y tiles tambien usan filtrado Point y compresion desactivada.

Para regenerar esta escena: `python Tools/Nivel1/bake_scene.py --fiel`. La composicion y sus ajustes estan en `Tools/Nivel1/escena_principal_sprites.json`. Para revisar el mapa sin abrir Unity: `python Tools/Nivel1/preview_escena_principal.py`. Para comprobar pasos y salida: `python Tools/Nivel1/check_fiel_collisions.py`. La verificacion dentro de Unity se ejecuta con `TechnopolisVerificarEscenaFiel.Verificar` y comprueba sprites, tilemaps, colisionadores, camara y escena principal.

La vista previa esta en `Documentacion/Nivel1/Vista_Previa_EscenaPrincipal.png`. La escena anterior `EscenaNivel1_Referencia.unity` sigue disponible para comparar. Tras actualizar el repositorio, abre la escena principal y deja que Unity termine de importar los assets antes de probar el movimiento.
