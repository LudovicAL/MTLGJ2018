#Xbox controllers player manager for Unity
By Ludovic Aubut-Lussier
------------------------------------------

The 'Xbox controllers players manager for Unity' package is designed to manage player controls
in multiplayer games played on a single Window computer. As implied by its name, the package
is provided with an input map (located in the 'InputManager.asset' file) corresponding the
Xbox controller joysticks and buttons.

To install the 'Xbox controllers players manager for Unity' package, follow these steps:

1. Replace the 'InputManager.asset' file located in your project folder under 'ProjectSettings'
with the corresponding included file.

2. Import our .unitypackage file in your Unity project.

3. Drag and drop the 'Canvas PlayerControllerPrefab' in any of your scene.

Installation is then complete.

Press play to launch the game and observe how players can now join the game by a push of the Xbox
controller 'A' button. A player may also leave the game by pressing the Xbox controller 'B' button.
Up to 11 players may join the game, provided enough Xbox controllers are connected to the computer.

Use the listOfPlayers variable to access the list of players that have joined the game. Every 'Player'
object provides its own 'Controller' object that is publicly accessible. The 'Controller' object
let you access the different joysticks and buttons assigned to that player. The following code
sample would therefore provide information on the current value of the first players 'A' button:

```C#
PlayersManager playersManager = GameObject.Find("Canvas PlayerControllerPrefab").GetComponent<PlayersManager>();
bool buttonAValue = Input.GetButton (playersManager.listOfPlayers [0].controller.buttonA);
```

Note that the 'InputManager.asset' map all the controls for 11 different Xbox controllers as well
as the default controls usually provided by default in a Unity project. The only exceptions are the
two trigger buttons (axis 9 and 10 of a Xbox controller). It is a known issue with Window's drivers
that the behaviour of these two buttons becomes unpredictable when more then one Xbox controller is
plugged in a Window machine.

