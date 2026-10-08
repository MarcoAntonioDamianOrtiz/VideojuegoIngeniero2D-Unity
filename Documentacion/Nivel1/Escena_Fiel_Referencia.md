# Escena principal del nivel 1

`Assets/Scenes/EscenaNivel1_FielReferencia.unity` es la primera escena de Build Settings. Conserva el jugador, sus componentes de movimiento y la cámara Pixel Perfect. El mapa ocupa 64 × 64 tiles; el `Grid` mantiene una escala vertical de 1.25.

## Composición actual

El plano residencial `Tools/Nivel1/reference_home_layout.json` coloca 54 viviendas individuales en grupos de tamaño y posición similares a la referencia. Se usan tres fachadas originales nuevas (casa azul de dos pisos, casa roja de dos pisos y vivienda taller con lona) junto a las fachadas existentes. En total hay 58 edificios independientes: 54 viviendas, casa de Alex y Bit, panadería, miscelánea y taller. Las nueve zonas siguen reconocibles: callejón industrial al noroeste, viviendas al norte y a los lados, Alex al oeste central, comercios al este central, plaza en el centro, taller al este, parque al sur central y salida comercial bloqueada al sureste.

Las vallas de obra y cercas rotas antiguas se retiraron. Solo quedan seis segmentos de madera junto a la casa de Alex. Los patios, el parque y el callejón se separan con 97 piezas de muro bajo de piedra y ladrillo cálido. Se generan desde 17 líneas continuas con huecos explícitos para accesos y escaleras; cada pieza tiene colisión ajustada a su longitud. El mural azul que aparecía suelto sobre el suelo se retiró. La fuente del parque se acercó a la posición de la referencia; el césped ahora tiene claros de tierra, manchas verdes y vegetación adicional. El árbol central, los juegos, bancos, canteros, farolas y objetos de los patios son sprites separados del terreno.

La panadería y la miscelánea tienen una escala y un frente peatonal revisados. El taller se amplió y ahora muestra un coche rojo original dentro del garaje. El túnel del callejón, sus muros y los residuos forman una zona industrial más cerrada. La salida comercial tiene asfalto desgastado integrado al suelo original, muros laterales, barrera, aviso y señal de prohibición visibles frente al portón cerrado.

El suelo `terreno_organico_nivel1.png` es original del proyecto y solo contiene materiales de piso, senderos, detalles de superficie y bordes de terreno. Los edificios y los objetos con volumen tienen sprites y colisiones independientes. Los 40 tramos de sendero se pintan con contraste más claro sobre patios de tierra oscurecida y llegan hasta los accesos de las escaleras. Las cinco terrazas se conectan mediante 18 escaleras originales de concreto o tierra, ahora de 2,5 a 2,8 tiles de ancho. Los bordes sólidos impiden cruzar directamente los desniveles. Se ajustaron las alturas de cuatro viviendas desproporcionadas; la más alta mide unas tres veces la altura visible del jugador.

## Integración de los cambios importados

La escena recibida de otra máquina contenía 63 viviendas del montaje anterior y había perdido los nodos de escaleras y bordes, aunque el plano actual definía 54 viviendas. La regeneración vuelve a usar el plano actual para edificios, objetos y colisiones. `bake_scene.py` conserva ahora los objetos externos al mapa que pertenecen al juego: `Ramona_Npc`, el `Canvas` de diálogo y terminal, el `EventSystem` y los componentes actualizados del jugador. Doña Ramona se colocó frente a la panadería en `(4, -10.4)` del mundo, con su zona de interacción y referencias al minijuego intactas. Las futuras regeneraciones conservan estos objetos y la cámara Pixel Perfect sin duplicarla.

La copia de la escena importada antes del ajuste está en `C:\Users\mo27m\Documents\Codex\2026-10-05\quiero-que-trabajes-sobre-la-escena\work\EscenaNivel1_importada_antes_ajuste.unity`.

## Comprobaciones

- La vista previa completa está en `Documentacion/Nivel1/Vista_Previa_EscenaPrincipal.png`.
- `Tools/Nivel1/check_visual_layout.py`: 58 edificios, sin solapamientos importantes.
- `Tools/Nivel1/check_fiel_collisions.py`: 15 puntos de paso conectados; salida comercial cerrada.
- `Tools/Nivel1/check_relief_routes.py`: 18 escaleras abiertas y bordes bloqueados fuera de ellas.
- Unity 6000.3.24f1 abrió la escena en modo de validación: 987 sprites, 54 viviendas, 97 muros de patio, seis vallas, nueve escaleras de concreto, nueve de tierra, terreno de 1122 × 1402 px, Tilemap de 64 × 64 y 273 colisionadores. Se comprobaron la NPC, el diálogo y el minijuego de la panadería.

Para regenerar, ejecutar en orden `generate_stair_assets.py`, `generate_terrain_relief.py`, `bake_scene.py --fiel` y `preview_escena_principal.py` desde `Tools/Nivel1`. `escena_principal_sprites.json` define senderos, objetos y recorridos de muros; `reference_home_layout.json` define las viviendas; `wall_layout.py` coloca las piezas continuas con los huecos de paso. Si Unity estaba abierto durante la regeneración de la escena, aceptar la recarga del archivo antes de guardar en el editor.
