# Nivel 1: reconstrucción de la referencia

Abre `Assets/Scenes/EscenaNivel1_Referencia.unity` después de hacer Pull. La distribución está aplicada en esa escena: no necesitas ejecutar los menús anteriores. `EscenaNivel1.unity` permanece intacta.

La escena nueva conserva el jugador, sus animaciones y movimiento, la cámara que lo sigue y la iluminación de la escena existente. El barrio sigue ocupando 64 × 64 casillas, con límites en X = −40…24 y Y = −44…20. Su punto de inicio queda en el patio de Alex.

Se trazaron las posiciones y proporciones de las zonas sobre la imagen de referencia: callejón al noroeste, viviendas alrededor de patios, huertos al noreste, casa de Alex al oeste de la plaza, panadería y miscelánea al norte, taller al este, parque al sur y salida bloqueada al sureste. Hay 32 edificios y 660 elementos en el plano, además de los 32 tramos de muro exterior.

Se añadieron ocho sprites con transparencia: casa de Alex con interior visible, panadería con letrero, miscelánea, taller, túnel, cancha, vagón del borde norte y maleza verde. Las casas vecinas reutilizan los sprites del proyecto. Por ello la composición se acerca a la referencia, pero el arte de esas viviendas y muchos detalles pequeños no es una copia exacta de la ilustración.

`Vista_Previa_Referencia.png` es una composición de los sprites y tiles reales con las mismas coordenadas de la escena; no es una captura del editor ni prueba de ejecución en Unity.

## Aplicar sobre la escena original

Fuera de Play, abre `EscenaNivel1.unity` y usa **Tools → Technopolis → Nivel 1 → 08 Reconstruir diseño de la referencia**. El menú guarda un respaldo completo y deja inactivas las capas anteriores antes de crear `Grid/Barrio_Referencia_08`. Puede repetirse sin duplicar sus objetos. Todos los recursos se comprueban antes de modificar la escena.

No ejecutes las opciones 02–07 después de la 08. Aquellas opciones corresponden a la distribución anterior. Para regresar, abre la escena previa o el respaldo generado en `Assets/Scenes/Respaldos`.

## Verificación

Se revisaron la sintaxis C#, las referencias e identificadores serializados, las 4096 casillas de suelo, los sprites y sus límites alfa. Una comprobación geométrica con la huella del jugador confirma conexiones desde Alex hacia panadería, tienda, plaza, parque, callejón, huerto, taller y la zona anterior al portón. Esto no reemplaza probar las colisiones y la importación en Unity: este entorno no tiene el editor instalado.

El orden de dibujo del jugador cambia con su posición vertical mediante `TechnopolisOrdenBarrio`, para permitir que pase delante o detrás de los elementos. Los objetos decorativos pequeños no añaden obstáculos al recorrido; edificios, juegos y piezas grandes usan huellas acotadas.

## Mantener el plano

`Assets/Technopolis/Nivel1/Editor/PlanoReferenciaNivel1.json` contiene las posiciones, tamaños visibles, materiales y senderos. Sus coordenadas empiezan en la esquina superior izquierda: columnas hacia la derecha y filas hacia abajo, de 0 a 64. Las dimensiones se calculan sobre el contenido visible de cada PNG, no sobre sus márgenes transparentes.

Los auxiliares `Tools/Nivel1/build_layout.py` y `bake_scene.py` regeneran el plano/vista previa y la escena alternativa. Requieren Python con Pillow, NumPy y PyYAML; no son necesarios para abrir o jugar la escena en Unity.

## Procedencia de los sprites

Se crearon con la herramienta de generación de imágenes integrada, tomando la ilustración adjunta como referencia de estilo y tema. Indicaciones utilizadas: pixel art de barrio mexicano, vista RPG ortográfica, contornos oscuros, materiales gastados, sujeto completo y fondo alfa transparente. Se pidió cada pieza por separado: casa abierta con cama azul y escritorio, fachada PANADERIA, fachada MISCELANEA, fachada TALLER, túnel de concreto, cancha de baloncesto deteriorada, vagón amarillo largo y grupo de maleza verde. Se recortaron los márgenes y se redujeron con muestreo de vecino más cercano para importarlas como sprites sin compresión y con filtro Point.

Los ocho PNG se encuentran en `Assets/Technopolis/Nivel1/Sprites/04_Objetos` y `05_Edificios`, junto con sus archivos `.meta`.
