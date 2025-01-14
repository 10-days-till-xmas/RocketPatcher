# RocketPatcher
Rocket clips in ultrakill have been around for 2 years now, and despite several attempts to patch them out, they still find some way to persist.
This mod changes the logic of how mounting rockets works so that (hopefully) rockets cannot be used to clip out of bounds.

## Theory
When riding rockets, the player is teleported a short distance to make it appear like its mounting the rocket, and this is what causes the player to be able to oob:
```cs
CurrentPlayerPos = base.transform.position + base.transform.up * 2f + base.transform.forward;
```
Here is one example of the decompiled code controlling rockets, and here it's teleporting the player to a position relative to the rocket, meaning that if the player mounts the rocket, theyd be placed above the rocket slightly, with only a few checks for if its possible, especially since rockets can be embedded in walls, or in non-convex geometry, leading to these checks being nearly useless. <br>
The changes this mod makes however modifies the logic, gradually aligning the player with the rocket as they fly. (With some interesting effects too, which can be updated if desired)
```cs
expectedPlayerPos = base.transform.position + base.transform.up * 2f + base.transform.forward;
CurrentPlayerPos = Vector3.Lerp(CurrentPlayerPos, expectedPlayerPos, timeSpentAligning / timeToAlign);
```
With the testing me and some other speedrunners have made so far, this completely removes any and all rocket clips, and trigger skips too, now that theres no way the player can teleport without cheats.
(Except for some untagged wall clips but I can't fix those it seems)

<details>
  <summary>Some clips from playtesting</summary>
  
  https://github.com/user-attachments/assets/448e3078-9b8e-4711-8a7d-2b34bbe5fb90
  
  https://github.com/user-attachments/assets/addb6e8c-01e6-4798-9893-d967961673d6
</details>

## Notes
Please report to the [issues page](https://github.com/10-days-till-xmas/RocketPatcher/issues) if you find anywhere or any setup that allows for rocket clips, or if you find any bugs.<br>
Thanks to Debatable for sharing with me their findings and theories, and to anyone who playtests this mod.
