# Armor overlays

Load this file at Step 5 of `asset-texture-creation` for an armor piece. PR-22 built the first three sets (D-753).

## The files of one piece

- The overlay model is `content/models/armor/<set>-<slot>.bbmodel`, and its paint file is next to it (D-300, D-508).
- The armor file is `content/armor/<set>-<slot>.json`. It holds the tier, the slot, the reduction, the weight, and the model path (D-763).
- The item file is `content/items/<set>-<slot>.json`. It names the armor file. The slot of the item and of the armor file match.
- `texture-gen` reads the models of `models/armor/` too. Run it after each overlay change, and commit the atlas and the layout.

## The rules of an overlay box

- Each box carries the name of the body box that it covers, and it encloses that box on every axis. An equal face counts as enclosed (D-300).
- A box also covers each body box of its bone that it fully encloses. Under a head piece, these are the brow, the nose, and the beard (D-767).
- A face with a texture of null is undrawn. It takes no place in the atlas and no mesh. An open head piece leaves its north face undrawn, so the face shows (D-767).
- The Game grows each overlay mesh by 2 millimeters on each side. A face in the plane of a body face then draws in front of it.

## The limits that the clip check found

Every pair of boxes counts, except a bone and its parent (D-301). These limits keep a piece free of a clip in every clip of the body:

- A chest box stays flush with the torso on X, at 5 units on each side. A wider box meets the lower arm, which is not a child of the torso.
- A leg piece covers the upper legs alone, and its top stays flush at Y 10. A higher top meets the lower arm.
- A leg box grows 0.25 units at most toward the other leg. The two legs stand 1 unit apart.
- A feet piece covers the lower leg and the toe. The front of its lower leg box stays flush at Z -2, and the back of its toe box meets it there. A deeper box meets the toe box of the same bone.

## The review

- The contact sheet shows the body in each set, from the front and from the back (D-306).
- `--armor <set>` starts a play session in one set, for example `--armor blast`.
- Run `asset-qa`. PR-22 exit test 5 asserts zero findings for every overlay.
