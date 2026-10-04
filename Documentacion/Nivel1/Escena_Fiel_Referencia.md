# Escena principal construida con sprites

`Assets/Scenes/EscenaNivel1_FielReferencia.unity` es la escena principal (indice 0 en Build Settings). Conserva el jugador, su movimiento y la camara. `Grid` queda al final de la jerarquia, con los tilemaps de suelo y senderos debajo de las construcciones y los objetos.

El escenario se compone de 4096 tiles de terreno, 593 tiles de camino y 876 sprites independientes, entre ellos 67 edificios. Las casas, comercios, arboles, mobiliario y muros se pueden seleccionar y editar por separado. Se anadieron tres fachadas transparentes: `vivienda_dos_pisos_ocre.png`, `vivienda_dos_pisos_ladrillo.png` y `vivienda_lona_azul.png`. El antiguo `mapa_nivel1_fiel_referencia.png` se conserva solo como referencia historica; la escena principal no lo renderiza.

`Grid` usa escala vertical 1.25 para igualar la proporcion alta del mapa ilustrado. Los caminos se componen de segmentos ligeramente curvos y de anchura variable; la casa de Alex, los comercios, la plaza, el parque y la salida siguen anclados a las mismas zonas. La vista previa de 1024 x 1280 px permite revisar esta composicion sin las etiquetas editoriales de la imagen de referencia.

Las colisiones se generan a partir de las piezas visibles, no de una silueta de la imagen completa. Las bases de las viviendas dejan libres los senderos; la casa de Alex conserva su puerta abierta; las zonas de parque y plaza tienen huellas pequenas en sus objetos. Los nuevos postes, arboles y cajones usan colisiones pequenas en su base. La salida comercial permanece bloqueada con muros laterales y una barrera alineada al porton visible. `Tools/Nivel1/colisiones_generadas_fiel.json` registra las 119 huellas usadas en la comprobacion de rutas.

La camara usa Pixel Perfect Camera (32 pixeles por unidad, referencia 640 x 360, filtro Point) para evitar el suavizado y la perdida de nitidez durante el juego. Los sprites y tiles tambien usan filtrado Point y compresion desactivada.

Para regenerar esta escena: `python Tools/Nivel1/bake_scene.py --fiel`. La composicion y sus ajustes estan en `Tools/Nivel1/escena_principal_sprites.json`. Para revisar el mapa sin abrir Unity: `python Tools/Nivel1/preview_escena_principal.py`. Para comprobar pasos, superposiciones de edificios y salida: `python Tools/Nivel1/check_fiel_collisions.py` y `python Tools/Nivel1/check_visual_layout.py`. La verificacion dentro de Unity se ejecuta con `TechnopolisVerificarEscenaFiel.Verificar` y comprueba sprites importados, tilemaps, colisionadores, camara y escena principal.

La vista previa esta en `Documentacion/Nivel1/Vista_Previa_EscenaPrincipal.png`. La escena anterior `EscenaNivel1_Referencia.unity` sigue disponible para comparar. Esta reconstruccion aproxima la distribucion y el lenguaje visual de la referencia, pero no copia pixel por pixel su ilustracion original. Tras actualizar el repositorio, abre la escena principal y deja que Unity termine de importar los assets antes de probar el movimiento.
