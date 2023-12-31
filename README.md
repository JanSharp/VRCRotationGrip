
# Rotation Grip

Rotate objects around an anchor point using VRC Pickups, synced.

# Installing

Head to my [VCC Listing](https://jansharp.github.io/vrc/vcclisting.xhtml) and follow the instructions there.

# One Time Setup

Drag the `UpdateManager` prefab from the JanSharp Common package into your scene. It must keep that exact name and be at the root of the hierarchy.

# Usage

The way the `RotationGrip` script works is it rotates a given object whenever the object the `RotationGrip` is picked up (using a VRC Pickup script). The object `To Rotate` then gets rotated such that it perfectly faces _away from_ the held object.

With that in mind the recommended structure for using the `RotationGrip` script looks like this:
```
MyObjectRoot
┣╸RotationOrigin (empty GameObject)
┃ ┣╸ActualObjectPart1
┃ ┣╸ActualObjectPart2
┃ ┗╸...
┗╸Grip
```
Where the `Grip` GameObject has the `RotationGrip` script on it and it's transform `To Rotate` is set to `RotationOrigin`.

The names are up to you of course.

# Restrictions

There are plenty. But one that's quite noteworthy is the fact that the max rotation deviation will always use the original rotation as a base line, and allow you to rotate the object in any direction away from the original rotation, until it reaches the max deviation. There's currently no way around it this, not even using a different script to set the rotation at Start (i mean, maybe with even more hacks, but no, don't.), it would require a new feature in this script. However I'm not sure how exactly that should look for it to even be somewhat intuitive, and all in all this script is a mess and I don't enjoy working on it. There's a reason why there's still TODOs in the file.

# Editor Tools

The `RotationGrip` script comes with 2 (technically 3) utilities in the form of buttons in the inspector for the script.

- Snap in Line (for positioning in the editor)
- Add VRC Pickup (for when you add a `RotationGrip` script to an object that doesn't have a VRC Pickup yet => fastest way to setup an object)
  - Configure VRC Pickup (similar to adding, but instead of adding it's only configuring existing components)

Each one has a descriptive **tooltip**.
