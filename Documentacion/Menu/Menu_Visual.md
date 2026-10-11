# Menú principal de Technopolis

La escena `Assets/Scenes/ESCENA NUEVA.unity` se carga de forma aditiva sobre el nivel. Solo contiene `MenuCanvas`; la cámara, el jugador y el mundo siguen en `EscenaNivel1_FielReferencia`. El Canvas usa `Screen Space - Overlay`, orden 1000 y una referencia de 1920 × 1080. La ventana predeterminada del juego usa 1280 × 720 para mantener 16:9. El paisaje de tejados cubre visualmente el mapa cenital mientras el menú está abierto.

Para jugar, abrir `Assets/Scenes/EscenaNivel1_FielReferencia.unity` y pulsar **Play**. El menú aparece automáticamente. Para revisar solo la composición en el editor, usar **Technopolis > Menu > Ver barrio con menú (aditivo)**. Las capturas reales de Unity están en `Documentacion/Menu/Game_1920x1080.png`, `Options_1920x1080.png` y `Credits_1920x1080.png`.

## Organización

- `EscenarioMenu/TejadosAlAtardecer`: imagen del paisaje separada de la interfaz, basada en la referencia facilitada y editada para retirar sus personajes.
- `PersonajesMenu/JugadorSentado` y `PerroBitSentado`: poses transparentes independientes. El perro está basado en la hoja de sprites proporcionada por el usuario; Ramona ya no aparece en la portada.
- `TituloPrincipal`: palabras `TECHNO` y `POLIS` como textos editables con color, borde oscuro y profundidad.
- `Subtitulo`: `EL SEMESTRE DEL HAMBRE`, editable.
- `PanelMenu/BotonesMenu`: cinco filas independientes con selección visible y acciones asignadas al controlador `TechnopolisMenuRuntime`.
- `PanelOpciones` y `PanelCreditos`: pantallas independientes dentro del mismo Canvas.
- `Version`: muestra `v1.0` en la esquina inferior derecha. Se retiraron la barra y los indicadores inferiores de inicio/selección.
- `DecoracionesUI`: grupo reservado para ornamentos visuales sin comportamiento.

El marco metálico, la selección y la flecha son sprites originales en `Assets/Sprites/Menu/UI`. La fuente editable es Pixelify Sans, distribuida bajo la licencia SIL Open Font License en `Assets/Sprites/Menu/Fonts/OFL.txt`.

La antigua rama `PanelPrincipal` de la escena jugable se dejó **inactiva**, sin eliminarla. Contenía la imagen de referencia en un `RawImage` diminuto y botones anteriores; no pertenece al Canvas aditivo. Las ramas `PanelDialogo` y `PanelTerminal`, junto con el jugador, Ramona y la cámara original, permanecen en la escena jugable.

`Tools/Menu/generate_menu_ui.py` regenera los ornamentos. `TechnopolisMenuVisualBuilder.cs` reconstruye la escena, comprueba las referencias y captura sus tres pantallas. `TechnopolisGameFlow.cs` abre y cierra la escena aditiva, pausa el nivel y conserva la posición del jugador. `TechnopolisMenuRuntime.cs` gestiona la selección y las opciones. El JPEG original suministrado por el usuario no se carga en tiempo de ejecución.

## Controles y guardado

Usar **flechas o W/S** para cambiar de fila, **Enter o Espacio** para confirmar y **Escape o Retroceso** para volver desde Opciones o Créditos. El ratón permite señalar y pulsar filas. El mando admite cruceta o stick, botón inferior para confirmar, botón derecho para volver y Start para abrir el menú durante la partida. En Opciones, izquierda/derecha cambian el volumen o la pantalla completa.

**Nueva partida** inicia el nivel desde su posición inicial y borra el guardado anterior. **Continuar** aparece deshabilitado hasta que exista una partida y retoma la posición guardada del jugador. Durante el juego, **Escape** abre el menú y guarda esa posición. También se guarda periódicamente y al salir. Este guardado se limita a la posición en el nivel actual; aún no conserva misiones, inventario ni estado de NPC. **Salir** cierra la aplicación, o detiene Play en el editor.

La escena jugable sigue siendo un mapa visto desde arriba. El paisaje lateral y las poses sentadas son de presentación; los personajes del nivel mantienen sus posiciones y comportamiento originales.
