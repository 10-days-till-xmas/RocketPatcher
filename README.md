# RocketPatcher
Rocket clips in ultrakill have been around for 2 years now, and despite several attempts to patch them out, they still find some way to persist.
This mod changes the logic of how mounting rockets works so that (hopefully) rockets cannot be used to clip out of bounds.

## Theory
When riding rockets, the player is teleported a short distance to make it appear like its mounting the rocket, and this is what causes the player to be able to oob:
```cs
CurrentPlayerPos = base.transform.position + base.transform.up * 2f + base.transform.forward;
```
Here is one example of the decompiled code controlling rockets, and here it's teleporting the player to a position relative to the rocket, meaning that if the player mounts the rocket, theyd be placed above the rocket slightly, with only a few checks for if its possible, especially since rockets can be embedded in walls, or in non-convex geometry, leading to these checks being nearly useless. <br>
The mod however adds an extra check, by casting a capsule across through to where the player is expected to be, resulting in normal rocket behaviour, but with (most) rocket clips removed!
```cs
private static bool CapsuleCastCheck(Vector3 expectedPlayerPos, out Collider collision)
{
    CapsuleCollider playerCapsule = MonoSingleton<NewMovement>.Instance.playerCollider;
    Vector3 playerHeadLocal = Vector3.up * (playerCapsule.height / 2 - playerCapsule.radius);
    Vector3 playerFootLocal = Vector3.down * (playerCapsule.height / 2 - playerCapsule.radius);
    bool hit = Physics.CapsuleCast(
        CurrentPlayerPos + playerHeadLocal,
        CurrentPlayerPos + playerFootLocal,
        playerCapsule.radius,
        (expectedPlayerPos - CurrentPlayerPos).normalized,
        out RaycastHit hitInfo,
        (expectedPlayerPos - CurrentPlayerPos).magnitude, LayerMaskDefaults.Get(LMD.Environment));
    collision = hitInfo.collider;
    return hit;
}
```
In theory, this should remove all rocket clips other than untagged walls, and trigger-skips

## Notes
Please report to the [issues page](https://github.com/10-days-till-xmas/RocketPatcher/issues) if you find anywhere or any setup that allows for rocket clips, or if you find any bugs.<br>
Thanks to Debatable for sharing with me their findings and theories, Heckteck for the advice to use `CapsuleCast()`, and to anyone who playtests this mod.
