# Court Scene Implementation Guide

This document provides detailed instructions for setting up and configuring the courtroom scene (Scene 3) for the game jam entry "Without Intention".

## Overview

The courtroom scene is a 2D side-view scene with a typing mini-game that includes a narrative twist at the end. The player must type words as prompted, with a timer adding pressure. In the final round, regardless of what the player types for the last word, the game will replace it with a different word that changes the meaning of the sentence.

## Setup Instructions

### 1. Scene Setup

1. Open the `CourtScene.unity` scene file
2. Create a Canvas (UI → Canvas) with CanvasScaler set to "Scale With Screen Size"
3. Add a black Image as a child of the Canvas and name it "FadePanel"
   - Add a CanvasGroup component to this panel
   - Use this for fade transitions
4. Create empty GameObjects for each character:
   - Player
   - Lawyer
   - Judge
   - Juror (5 of these)
5. Add SpriteRenderer components to each character
6. Add the `CourtCharacterAnimator` script to each character

### 2. Create Scriptable Objects

1. In Unity Editor, go to Game → Create Court Rounds
   - This will automatically create the 5 round configurations as defined in the requirements
   - Alternatively, you can create them manually:
     - Create → ScriptableObject → Game/Court Round Config
     - Configure each round with the specified words and times

### 3. Configure Scene Controller

1. Add an empty GameObject named "SceneController"
2. Add the `CourtSceneController` script to this GameObject
3. Configure the references in the Inspector:
   - Assign the main camera
   - Assign the FadePanel's CanvasGroup
   - Assign all character transforms
   - Create and assign chat bubble GameObjects for the lawyer and judge
   - Set up the camera positions for close-up and courtroom views
   - Assign the 5 round scriptable objects created earlier

### 4. Set Up Typing UI

1. Create a UI Panel for the typing game (as a child of the main Canvas)
2. Add a TMP_InputField for player typing
   - Ensure it has a TextMeshPro - Text component for the input text
   - Configure a TextMeshPro - Text for the placeholder text
3. Add a circular Image for the timer (configured to use as radial fill)
4. Add a TextMeshPro - Text for displaying completed sentences
5. Add the `CourtTypingSystem` script to the typing UI panel
6. Configure all references in the Inspector

### 5. Sound Setup

1. Add sounds to the SoundManager:
   - "gavel" sound (played at scene start and end)
   - "sirens" sound (looping ambient sound)
   - "gasp" sound (for the final twist reaction)
   - "correct" sound (for successful typing)
   - "incorrect" sound (for typing errors)

## Implementation Details

### Scene Flow

1. Scene begins with a fade-in from black with gavel sound
2. Camera shows close-up of lawyer saying "Repeat after me"
3. Camera pulls back to show full courtroom
4. Characters begin bouncing
5. Typing mini-game starts with Round 1
6. For each round:
   - Player types one word at a time
   - Timer counts down for each word
   - If all words are typed correctly, judge says "I dismiss your defense"
   - Scene moves to next round
7. On the final round's last word:
   - Input field shows "yapmadım"
   - Regardless of what player types, it displays "yaptım"
8. All bouncing stops
9. Characters show shocked expression
10. Judge says "You are sentenced to life imprisonment"
11. Fade to black with gavel sound

### Typing System

- One word displayed at a time in the placeholder text
- Player must type each word correctly
- Correct letters are green, incorrect letters are red
- Screen shakes on incorrect typing
- Timer is a radial fill that counts down
- If time runs out, scene restarts

### Script Responsibilities

- `CourtRoundConfig`: ScriptableObject storing words and timing data for each round
- `CourtSceneController`: Main controller managing scene flow and transitions
- `CourtTypingSystem`: Handles typing mini-game logic and feedback
- `CourtCharacterAnimator`: Controls character bouncing and sprite swapping

## Project Structure

- Scripts/
  - CourtRoundConfig.cs - ScriptableObject for round configuration
  - CourtSceneController.cs - Main scene controller
  - CourtTypingSystem.cs - Typing mini-game logic
  - CourtCharacterAnimator.cs - Character animation
  - CourtRoundsSetup.cs - Editor utility for creating rounds
- ScriptableObjects/
  - CourtRound1.asset - Round 1 configuration
  - CourtRound2.asset - Round 2 configuration
  - CourtRound3.asset - Round 3 configuration
  - CourtRound4.asset - Round 4 configuration
  - CourtRound5.asset - Round 5 configuration (with twist)
