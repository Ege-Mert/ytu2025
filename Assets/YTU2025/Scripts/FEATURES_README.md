# Game Jam Features Implementation Guide

This document provides instructions for implementing the four requested features into your game jam project "Without Intention".

## 1. Tutorial System

### Setup Instructions

1. Create a new GameObject in each scene where you want a tutorial:
   ```
   Right-click in Hierarchy > Create Empty > Name it "TutorialSystem"
   ```

2. Add the TutorialSystem script to this GameObject:
   ```
   Add Component > TutorialSystem
   ```

3. Create a UI Canvas with the tutorial elements:
   - Create a Canvas (UI > Canvas)
   - Add a Panel as the background
   - Add one or more Image components for tutorial pages
   - Add a TextMeshProUGUI for the "Click to continue" hint

4. Assign the UI elements in the inspector:
   - Tutorial Canvas: Drag your Canvas
   - Tutorial Pages: Drag your Image components to this array
   - Click To Continue Text: Drag the hint Text

5. Configure your tutorial settings:
   - Pause Game During Tutorial: Typically leave checked
   - Show Tutorial Once: Enable to only show once per player
   - Tutorial Key: Unique identifier for this tutorial (or leave blank to use scene name)

6. For sound effects, make sure to add the sound name in the SoundManager (if used).

The tutorial will show immediately when the scene starts and pause the game. The player can dismiss it by clicking any mouse button or pressing any key.

## 2. Comic Panel Mechanic

### Setup Instructions

1. Create a new GameObject in your comic panel scenes:
   ```
   Right-click in Hierarchy > Create Empty > Name it "ComicPanelSystem"
   ```

2. Add the ComicPanelSystem script to this GameObject:
   ```
   Add Component > ComicPanelSystem
   ```

3. Create a UI Canvas with the comic panel elements:
   - Create a Canvas (UI > Canvas)
   - Add multiple Image components for each comic panel
   - Add a TextMeshProUGUI for captions (optional)
   - Add a TextMeshProUGUI for the "Click to continue" hint
   - Add a CanvasGroup for fading to black

4. Configure the Comic Panels in the inspector:
   - For each panel, add an entry to the Comic Panels list
   - Assign the Panel Rect (RectTransform of the Image)
   - Assign the Panel Image
   - Optionally add a sound effect name for this panel
   - Optionally add a caption

5. Set the Next Scene Name to specify which scene to load after all panels have been shown.

6. Make sure to add the required sound effects in the SoundManager:
   - Page Flip sound (default for all panels)
   - Custom panel sounds (if specified)

The comic panel system will start automatically when the scene loads, showing the first panel with a page-flip sound effect. The player can click to advance to the next panel.

## 3. Settings System

### Setup Instructions

1. Create a new GameObject in your main scene (the system will persist across scenes):
   ```
   Right-click in Hierarchy > Create Empty > Name it "SettingsManager"
   ```

2. Add the SettingsManager script to this GameObject:
   ```
   Add Component > SettingsManager
   ```

3. Create a UI Canvas with the settings elements:
   - Create a Canvas (UI > Canvas)
   - Add a Panel as the background
   - Add Sliders for mouse sensitivity and master volume
   - Add TextMeshProUGUI components for labels and values
   - Add Buttons for "Close" and "Reset to Defaults"

4. Assign the UI elements in the inspector:
   - Settings Canvas: Drag your Canvas
   - Mouse Sensitivity Slider: Drag the sensitivity slider
   - Master Volume Slider: Drag the volume slider
   - Sensitivity Value Text: Drag the text for showing sensitivity value
   - Volume Value Text: Drag the text for showing volume value
   - Close Button: Drag the close button
   - Reset To Defaults Button: Drag the reset button

5. Configure default settings and key bindings in the inspector.

6. The settings system will automatically connect to the FirstPersonController in each scene to adjust mouse sensitivity, and it will manage global audio volume through AudioListener.volume.

7. You can press the Escape key (or your configured key) to toggle the settings menu.

## 4. Final Scene: Dramatic Zoom-Out Ending

### Setup Instructions

1. Create a new scene for your final sequence.

2. Create a Canvas with the following UI elements:
   - Image for the character
   - Image for the background
   - TextMeshProUGUI for the variant text (optional)
   - A parent RectTransform for both images (for zooming)
   - A CanvasGroup for fading to black

3. Create a GameObject and add the FinalSceneController script:
   ```
   Right-click in Hierarchy > Create Empty > Name it "FinalSceneController"
   Add Component > FinalSceneController
   ```

4. Configure the Character Variants:
   - Add entries to the Character Variants list
   - For each variant, assign the Character Sprite and Background Sprite
   - Optionally add variant text to display

5. Configure the timing and zooming parameters:
   - Initial Delay: Time before sequence starts
   - Initial Swap Delay: Time between showing first variant and starting swaps
   - Min Swap Delay: Fastest swap time (before fast sequence)
   - Swap Delay Decrement: How much to speed up each swap
   - Fast Swap Loop Count: Number of rapid swaps at the end
   - Fast Swap Delay: Time between rapid swaps
   - Zoom In/Out Duration: How long the camera zoom takes
   - Zoom Target Scale: How much to zoom in
   - Zoom Target Position: Where to zoom to (character's face)

6. Configure audio parameters:
   - Background Music Name: Music to play during sequence
   - Music Fade Out Duration: Time to fade out music
   - Final Audio Cue Name: Sound to play at the very end

7. Assign the UI references in the inspector.

8. Make sure to add the required audio clips in the SoundManager:
   - Background music
   - Final audio cue

The final scene will play automatically when loaded, with the zoom-in/out effect, character variant swapping, and audio cues as configured.

## Important Notes

1. All four systems use the SoundManager for audio. Make sure appropriate sound effects are added to the SoundManager.

2. The SettingsManager is designed as a singleton that persists across scenes. Only add it once to your first scene.

3. Ensure you have DOTween in your project, as the animations use DOTween for smooth transitions.

4. For the tutorial system, use a unique Tutorial Key for each different tutorial to ensure they only show once.

5. The Comic Panel System and Final Scene Controller will automatically handle the transitions between panels/scenes.
