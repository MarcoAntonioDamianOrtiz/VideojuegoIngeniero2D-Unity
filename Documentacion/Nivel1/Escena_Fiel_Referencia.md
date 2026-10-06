# Escena principal del nivel 1

`Assets/Scenes/EscenaNivel1_FielReferencia.unity` es la primera escena de Build Settings. Conserva el jugador, sus componentes de movimiento y la cámara Pixel Perfect. El mapa ocupa 64 × 64 tiles; el `Grid` mantiene una escala vertical de 1.25.

## Composición actual

El plano residencial `Tools/Nivel1/reference_home_layout.json` coloca 54 viviendas individuales en grupos de tamaño y posición similares a la referencia. Se usan tres fachadas originales nuevas (casa azul de dos pisos, casa roja de dos pisos y vivienda taller con lona) junto a las fachadas existentes. En total hay 58 edificios independientes: 54 viviendas, casa de Alex y Bit, panadería, miscelánea y taller. Las nueve zonas siguen reconocibles: callejón industrial al noroeste, viviendas al norte y a los lados, Alex al oeste central, comercios al este central, plaza en el centro, taller al este, parque al sur central y salida comercial bloqueada al sureste.

Las vallas de obra y cercas rotas antiguas se retiraron. Se conservaron 44 segmentos de cerca alrededor del parque, la plaza y la casa de Alex; se añadieron 32 tramos bajos de muro para separar parcelas residenciales sin cerrar los caminos. La fuente del parque se acercó a la posición de la referencia y el pasto se hizo más verde. El árbol central, los juegos, bancos, canteros, farolas y objetos de los patios son sprites separados del terreno.

El suelo `terreno_organico_nivel1.png` es original del proyecto y solo contiene materiales de piso, senderos, detalles de superficie y bordes de terreno. Los edificios y los objetos con volumen tienen sprites y colisiones independientes. Los 40 tramos de sendero y cinco terrazas conectan las zonas principales. Hay 18 escaleras transitables y 38 segmentos de borde sólido que impiden cruzar directamente los desniveles.

## Comprobaciones

- La vista previa completa está en `Documentacion/Nivel1/Vista_Previa_EscenaPrincipal.png`.
- `Tools/Nivel1/check_visual_layout.py`: 58 edificios, sin solapamientos importantes.
- `Tools/Nivel1/check_fiel_collisions.py`: 15 puntos de paso conectados; salida comercial cerrada.
- `Tools/Nivel1/check_relief_routes.py`: 18 escaleras abiertas y bordes bloqueados fuera de ellas.
- Unity 6000.3.24f1 abrió la escena en modo de validación: 919 sprites, 54 viviendas, terreno de 1122 × 1402 px, Tilemap de 64 × 64 y 202 colisionadores.

Para regenerar, ejecutar en orden `generate_terrain_relief.py`, `bake_scene.py --fiel` y `preview_escena_principal.py` desde `Tools/Nivel1`. `escena_principal_sprites.json` define senderos y objetos; `reference_home_layout.json` define las viviendas. Si Unity estaba abierto durante la regeneración de la escena, aceptar la recarga del archivo antes de guardar en el editor.
