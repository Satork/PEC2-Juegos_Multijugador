# MULTIJUGADOR - PEC2

Es un juego multijugador en red de área local (LAN). En primer lugar se crea un servidor y a partir de ahí se añaden los jugadores para la partida. Se puede unir un jugador mientras el servidor siga activo en cualquier momento, aún con la partida iniciada. El que crea el servidor puede ser jugador también o no.
El juego es una versión modificada del juego Tanks! enlazada con el New Input System, por lo tanto, los controles del juego son los estándar, utilizando las flechas para moverse y la barra espaciadora para disparar. A medida que se destruyen los tanques se va reajustando la cámara para enfocar a los que quedan y el resto de jugadores quedan como espectadores.

Para iniciar la partida se hace desde la escena de Lobby donde al jugador se le permite elegir una de las tres opciones:
- Nuevo servidorS
- Nueva partida
- Unirse partida

# Nuevo servidor
El usuario crea un servidor donde podrán jugar el resto que se unan a la partida pero no será un jugador.
# Nueva partida
El usuario crea un servidor igual que en la opción anterior pero además él es un jugador más.
# Unirse partida
El usuario se puede unir a algún servidor existente, se le muestra un listado de los que hay para uqe pueda elegir.

Cando el usuario elige entrar en una partida, ya sea uniéndose o creándola, le aparece una pantalla donde puede seleccionar varias opciones. Puede cambiar el color de su tanque y su nombre de usuario, o bien elegir los colores por defecto, donde el tanque local se verá en color azul y los otros jugadores tendrán el tanque rojo desde el punto de vista de cada jugador. También puede escribir su nombre de usuario y este aparecerá encima de su tanque, si no escribe ninguno se mostrará uno por defecto. Cuando esté listo tendrá que pulsar el botón Start.

El juego se inicia desde la escena Lobby. Al entrar el primer jugador a la partida aparecen entre 0 y 4 NPCs de color amarillo con el nombre CPU, dependiendo de la cantidad que se haya marcado en el GameNetworkManager. Estos tanques están en un sitio fijo de donde no se desplazan pero sí rotan hacia la dirección donde detectan un jugador y le disparan, si el jugador se mueve, estos tanques rotan hacia su dirección siempre que se mantenga a una distancia corta.

Existe una flecha hacia la izquierda en la esquina superior izquierda que es el botón para volver a la pantalla de Lobby.

