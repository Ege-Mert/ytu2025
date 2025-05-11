# Arm Wrestling Game Implementation

This README provides instructions for setting up the Arm Wrestling minigame and Visual Novel components.

## Visual Novel Choice Mechanism

The visual novel features a unique choice mechanism:

1. When the player makes a choice (A or B), the text of the choices visually swaps places
2. The player's character changes to their "choice" sprite
3. The chosen button receives a brief highlight animation
4. After a brief pause, the choice buttons hide
5. The dialogue shows the text of the *opposite* choice (not what the player selected)
6. After the response duration, the screen fades to black and transitions to the next scene

## Setup Instructions

### 1. Create Opponent Configurations

1. In the Project panel, right-click and select `Create > Game > Arm Wrestle Opponent`
2. Create 3 opponent configurations (one for each prisoner)
3. Configure each opponent with:
   - Name
   - Opponent state sprites:
     - Normal sprite (middle stamina range)
     - Near loss sprite (close to losing)
     - Loss sprite (when they lose)
     - Near win sprite (close to winning)
     - Win sprite (when they win)
   - Sprite transition thresholds (customizable in the inspector)
   - Opponent state sounds:
     - Near loss sound (played when close to losing)
     - Near win sound (played when close to winning)
     - Loss sound (played when they lose)
     - Win sound (played when they win)
   - Difficulty parameters (sweet spot size, drift speed, etc.)
   - Visual Novel dialog lines and choices
   - Player character sprites (normal and after choice)

### 2. Scene Setup

1. Create a new scene or use an existing scene
2. Add the required components to the scene:

   - Add a GameObject with the `ArmWrestleSceneController` component
   - Add a GameObject with the `ArmWrestleController` component
   - Add a GameObject with the `VisualNovelController` component

3. Set up the UI elements:

   **Arm Wrestling UI:**
   - Create a Canvas with these UI elements:
     - Sweet Spot Bar (green zone) - RectTransform
     - Player Bar (gray zone) - RectTransform
     - Stamina Meter (container for opponent's stamina) - RectTransform
     - Stamina Fill (Image with Vertical Fill type) - Set Fill Method to Vertical and Fill Origin to Bottom
     - Vignette Effect (screen edge red effect) - Image with alpha transparency
     - Timer Text - TextMeshProUGUI
     - Round Number Text - TextMeshProUGUI
     - Round Complete Panel with Image - Panel with RectTransform
     - Countdown Panel with TextMeshProUGUI for countdown text

   **Visual Novel UI:**
   - Create a separate Canvas for the Visual Novel section (keep it disabled initially)
   - Within the Visual Novel Canvas:
     - Visual Novel Panel
     - Opponent Character Sprite (left side, starts transparent and fades in)
     - Player Character Sprite (right side, starts transparent and fades in)
     - Dialogue Text
     - Click-to-Continue Icon
     - Choice Panel with:
       - Choice A Button and Text
       - Choice B Button and Text
     - **Transition Fade Image** - A full-screen black Image with alpha set to 0

4. Configure the `ArmWrestleController`:
   - Add your 3 opponent configurations to the Opponents array
   - Link all the UI elements to their respective fields
   - Configure round transition settings:
     - Entry, hold, and exit positions for the round complete image
     - Animation easing types and durations
   - Configure countdown settings:
     - Countdown panel and text
     - Animation scale and durations
   - Configure screen effects and sound effect names

5. Configure the `VisualNovelController`:
   - Link the Visual Novel Canvas
   - Link all the VN UI elements
   - Set the Transition Fade Image and fade duration
   - Configure character sprite fade-in settings
   - Set text speed and transition settings
   - Configure click indicator settings

6. Configure the `ArmWrestleSceneController`:
   - Add prison door sound clip
   - Set fade durations

### 3. Sound Manager Setup

Ensure the following sounds are added to the SoundManager:
- "ArmWrestleStruggle" (looping sound during gameplay)
- "ArmWrestleWin" (default sound when player wins)
- "ArmWrestleLose" (default sound when player loses)
- "ArmWrestleTwitch" (sound for sweet spot twitching)
- "PrisonDoor" (sound for scene intro)

Additionally, you can add custom sounds for each opponent and reference them in the opponent configuration:
- Near loss sounds
- Near win sounds
- Loss sounds
- Win sounds

## Gameplay Settings

### Difficulty Scaling

For each opponent, you can adjust these parameters to increase difficulty:
- Decrease `sweetSpotSize` (smaller target)
- Increase `driftSpeed` (faster movement)
- Decrease `staminaDrainInside` (slower stamina drain)
- Increase `staminaRecoverOutside` (faster stamina recovery)
- Increase `fallSpeed` (player falls faster)
- Decrease `riseSpeed` (player rises slower)
- Enable `twitching` for unpredictable movements

### Visual Effects

- The screen edges turn red (vignette) when nearing a loss
- The screen shakes when close to losing
- The sweet spot can twitch at intervals

## Implementation Notes

- The arm wrestling mechanic is a data-driven implementation based on Stardew Valley's fishing minigame
- All parameters are exposed in the inspector for easy tuning
- DOTween is used for UI animations and transitions
- The Visual Novel section follows the arm wrestling and uses a text-swapping choice mechanic
- A mouse icon appears after a delay if no interaction happens
- Winning all three rounds transitions to the Visual Novel
- Losing at any point restarts the scene
