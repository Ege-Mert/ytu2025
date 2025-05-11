# Operation Scene Setup Guide

This guide explains how to set up the "Without Intention" operation puzzle scene in Unity.

## Overview

The operation scene consists of a timed puzzle where the player must connect 5 colored wires to matching sockets on a patient's "circuit board" chest. The player must complete all connections before time runs out.

## Scene Setup

1. **Create a new Scene**
   - Create a new scene and save it in the Scenes folder

2. **Canvas Setup**
   - Add a Canvas (UI > Canvas)
   - Set Canvas to "Screen Space - Overlay"
   - Add an EventSystem if not already present
   - Add a GraphicRaycaster component to the Canvas if not already present

3. **Patient Background**
   - Add a background Image under the Canvas for the patient's chest (circuit board style)
   - Position it to fill most of the screen, leaving room at the bottom for the wire tray

4. **Operation Manager**
   - Create an empty GameObject named "OperationManager"
   - Add the `OperationManager` script to it

5. **Timer Setup**
   - Create a UI Image object named "TimerBackground"
   - Create a child UI Image named "TimerFill"
     - Set the Image Type to "Filled"
     - Set the Fill Method to "Radial 360"
     - Set Fill Origin to "Top"
     - Check "Clockwise"
   - Add the `OperationTimer` script to the TimerBackground object
   - Assign the TimerFill to the Timer Fill Image field in the inspector

6. **Socket Container**
   - Create an empty GameObject named "SocketContainer" under the Canvas
   - This will hold all the socket objects

7. **Wire Container**
   - Create an empty GameObject named "WireContainer" under the Canvas
   - This will hold all the wire end objects
   - Position this at the bottom of the screen to act as the "tray"

8. **Operation Config**
   - Create an Operation Config asset via the menu (right-click in Project panel > Create > YTU2025 > Operation Config)
   - Configure the colors, timer settings, etc.
   - Assign this to the Operation Manager

## Creating Sockets and Wires

1. **Socket Prefab**
   - Create a UI Image with a circular sprite
   - Add the `SocketController` script to it
   - Configure the Socket Index and appearance
   - Create 5 instances of this in the Socket Container
   - Position them on the patient's chest area
   - Assign each a unique Socket Index (0-4)

2. **Wire Prefab**
   - Create a UI Image with a circular sprite
   - Add the `WireController` script to it
   - **Important**: For Line Renderer setup, choose one of the following options:
     - **Option 1 (UI Image Line)**: No need to add any extra components
     - **Option 2 (Line Renderer)**: Add a LineRenderer component to the wire object
       - Set the Material to a suitable line material
       - You don't need to set any other properties as they'll be configured at runtime
   - Configure each Wire Index (0-4) to match the corresponding socket
   - Position the wires in a row at the bottom of the screen in the "tray" area

3. **Wire-to-Socket Interaction**
   - The wires remain stationary in their original positions 
   - When the player clicks and drags from a wire, a line extends from it
   - The player drags the end of this line to the matching colored socket
   - The line stays connected between the wire and socket once properly connected

## Final Configuration

1. **Operation Manager References**
   - Assign the Socket Container, Wire Container, and Operation Timer in the OperationManager inspector
   - Assign the Fade Image (you'll need to create a full-screen black image for transitions)
   - Configure sound effect names to match what you'll add to the SoundManager

2. **Sound Effects**
   - Add the required sound effects to the SoundManager:
     - "scalpel_hit" - Sharp sound for the operation start
     - "connection_correct" - Soft "clip" sound for correct connections
     - "connection_incorrect" - "Buzz" sound for incorrect connections
     - "heartbeat_stabilize" - Success sound
     - "flatline" - Failure sound

3. **Scene Flow**
   - Configure the success and failure scene names in the TransitionAfterDelay coroutine

## Usage

The scene flow is:
1. Scene fades in from black
2. Scalpel hit sound plays
3. Timer starts counting down
4. Player must drag lines from the wires to matching colored sockets
5. All connections successful = victory
6. Timer runs out = failure

## Script Components Overview

- **OperationConfig** - ScriptableObject that defines all the configurable aspects
- **OperationManager** - Main controller that orchestrates the entire puzzle
- **WireController** - Handles wire line dragging and connections
- **SocketController** - Manages individual socket behavior and state
- **OperationTimer** - Controls the countdown timer with visual feedback

## Integration Tips

- The operation scene should be loaded after the visual novel choice twist mentioned in the specifications
- The scene will handle its own transitions to the next scene based on success/failure
- Sound effects are played through the existing SoundManager

## Testing

You can test the operation puzzle by:
1. Entering Play mode
2. Clicking and dragging from wire ends to sockets (test both correct and incorrect connections)
3. Testing timer expiration by waiting
4. Testing success by connecting all wires correctly

## Troubleshooting

- If lines aren't appearing when dragging, check that:
  - Each wire has either a LineRenderer component or is using the built-in UI line creation
  - The Canvas has a GraphicRaycaster component
  - The EventSystem is present in the scene
- If connections aren't working, verify that:
  - Wire and Socket indices match correctly
  - Wire and Socket colors match for the same indices
  - The snap radius in OperationConfig is large enough (default 50 pixels)
