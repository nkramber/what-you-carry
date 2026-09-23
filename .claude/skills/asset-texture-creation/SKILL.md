---
name: asset-texture-creation
description: The art steps of every asset, from a Meshy prompt to a finished box model. Write the prompt, review the 2D image, ask for the 3D screenshots, review them, and build the asset as boxes, recipes, and a paint file. Load it for each new model or look change: a body, an enemy, an item, a weapon, armor, a prop, or a block.
---

# Asset texture creation skill

The owner makes the look reference of each asset in Meshy (D-496, D-509). The agent writes the prompts and reviews each result. The agent then builds the asset in this repository. A Meshy mesh never loads, because the model loader reads cube and locator elements alone. Meshy output is a look reference, and nothing more.

The body of PR-74 is the first asset of this procedure. Its owner answers, D-496 to D-503, are the worked example.

## Terms

| Term | What it names |
|---|---|
| concept image | the 2D image of Meshy text to image |
| 3D reference | the Meshy model of image to 3D, and its screenshots |
| look reference | the concept image and the 3D reference together |
| reference folder | `artifacts/reference/<asset>-<date>/`, which git ignores |
| recipe | a file of paint layers under `content/textures/recipes/` (D-505, D-507) |
| paint file | `content/models/<model>.paint.json`, the recipe of each box (D-508) |
| unit | one sixteenth of a meter, the unit of a `.bbmodel` file |

## Rules that bind every asset

- Avoid the Minecraft tells (D-83). No eye whites, and no flat pixel face layout.
- Keep the cuboid style and the shared proportion set (D-82). A body change needs an owner decision.
- Use the palette of D-304 alone. A new color needs a decision.
- Each face has 32 texels per meter (D-308). One unit is 2 texels, so an edge on a whole texel sits on a multiple of 0.5 units.
- The game light and the vertex occlusion shade the boxes (D-81). A recipe paints no shade.
- Every fix goes through the model JSON, a recipe, or a paint file (D-86). Blockbench is for review alone.
- The reference folder stays out of git. Commit an image only when the owner asks.
- Every open question belongs to the owner (D-124). Ask with `AskUserQuestion`, and record each answer as a D-#.

## Procedure

### Step 1: The prompt for the concept image

1. Read the roadmap entry of the asset, and the decisions that it cites.
2. Fill the template below with the subject, the proportions, and the colors.
3. Give the prompt to the owner in one fenced block, ready to paste into Meshy text to image.
4. State the reason for each choice in the prompt in one short line.

Template for a body or an enemy:

```
Front view of a single [SUBJECT], full body, centered, standing straight,
arms straight down with a small gap from the torso, feet apart.
Blocky cuboid character: every part is a rectangular box with hard square
edges. Head [H] units, torso [W] wide by [H] high, arms [W] thick, legs [W]
thick, total height [H] units, [N] heads tall.
[FEATURES: for example a heavy brow bar, small dark eyes with no whites, a
dark beard block on the lower third of the face.]
Low resolution pixel texture with visible square texels, flat colors, light
mottling. Colors: [PART] [color name] [hex], [PART] [color name] [hex].
Dark fantasy mine setting. Even flat light, no cast shadows, no shading
painted into the texture. Plain light grey background. No props, no ground,
no text.
Avoid: eye whites, a flat pixel face, rounded shapes, bevels, smooth curves,
glossy materials.
```

For an item, a weapon, or a prop, replace the first two lines with "Three-quarter view from slightly above of a single [SUBJECT], centered". Keep the other lines. If the Meshy form has a separate negative prompt field, put the avoid list there.

### Step 2: The review of the concept image

1. Save the image in the reference folder as `01-concept-<note>.png`, then `02-...` for each next try.
2. Read the image with the Read tool.
3. Check it against the concept checklist below.
4. Give one verdict: approved, or changes. For changes, give the new prompt in full.

The concept checklist:

- The D-83 tells: eye whites, a flat pixel face layout, a pixel beard with no depth.
- The proportions: compare with the model, not with a guess. Give the measured ratio in heads.
- The silhouette at game zoom: 5.4 pixels per texel on the Deck (D-306). A detail under 2 texels does not read.
- The parts that become boxes (a brow, a nose, a toe), and the parts that stay paint.
- The colors: name the nearest ramp step of D-304 for each part.
- The shade painted into the image. The recipe ignores it (D-81).

### Step 3: The 3D reference

1. Tell the owner to run Meshy image to 3D on the approved concept image.
2. Ask for these screenshots: front, back, left side, right side, both three-quarter views, and the front unlit.
3. Ask for a top view and a close front view of the head when the asset has a face.

### Step 4: The review of the screenshots

1. Save each screenshot in the reference folder with its angle in the name, such as `05-3d-side.png`.
2. Read each screenshot with the Read tool.
3. Measure each box and depth feature in units (see "Measure a screenshot").
4. Sample the colors from the unlit front view.
5. List the Meshy artifacts that the build ignores.
6. Give one verdict: approved, more angles, or changes. Name each missing angle and the reason.

Meshy artifacts to ignore:

- Shade painted into the texture.
- A front detail mirrored onto the back.
- Skin patches on the head sides.
- Cut corners, and a texture brighter than the concept.

### Step 5: The build

1. Write the owner questions: the proportions, each box feature, the paint details, and the noise.
2. Ask them with `AskUserQuestion`, and give options, reasons, and a recommendation.
3. Record each answer in `docs/decisions.md` with the next D-# and the local date.
4. Edit the `.bbmodel` file. Keep the rest pose, rotation zero, and a unique name for each box.
5. Put each new box flush on its neighbor: a shared face, and no penetration (D-301).
6. Write the recipes under `content/textures/recipes/`, and the paint file of the model.
7. Run `texture-gen` with `--root .`, and commit the atlas and the layout together.
8. Run the build, the full test suite, and `asset-qa`.
9. Render the contact sheet, and show it beside the approved concept image.
10. Record the approval of the owner as a D-#. The exit test of the asset needs it.

## Measure a screenshot

1. Find a box of known size in the view, such as the head of 8 units.
2. Divide its height in pixels by its size in units. The result is pixels per unit.
3. Measure each feature in pixels, and divide by pixels per unit.
4. Read the depth of a feature from a side view alone. A front view hides depth.
5. Round each edge to a multiple of 0.5 units, so the edge sits on a whole texel.
6. Give each number as "about", and let the contact sheet settle it.

## The build: recipes and paint files

A recipe is an ordered list of layers (D-507):

| Kind | Fields | Use |
|---|---|---|
| `fill` | `color`, `noise`, `seed` | The first layer, and only the first: the base material |
| `edge` | `steps` | A darker outer ring, as on hewn stone |
| `rect` | `x`, `y`, `width`, `height`, `color`, `noise`, `seed` | A face feature, a cuff, a collar |
| `band` | `side`, `depth`, `shift` | A darker hem, a boot band, grime at one side |

A recipe can extend another with a ramp swap: `{"extends": "skin", "swap": {"bone": "lichen"}}`. Use a swap for an enemy family or an armor tier that keeps the shape and changes the colors.

A rectangle counts from the top left of the face canvas, in texels. A face canvas is the face size at 32 texels per meter, rounded up. A head face of 8 units is 16 by 16 texels. A rectangle wholly outside a canvas is an error. Bind that face to another recipe in the paint file.

The paint file gives each box a recipe, and a recipe for a single face where that face differs:

```
{
  "model": "models/player.bbmodel",
  "boxes": {
    "head_box": {"recipe": "hair", "faces": {"north": "face"}}
  }
}
```

Each seed is a whole number from 1 upward. The generator mixes the seed with the face name, so two faces of one recipe show two draws of the noise.

## Bindings for later PRs

- A new box on the body binds PR-22: every armor overlay of that part must enclose it (D-300). Record the binding in the decision.
- A clip check runs at the rest pose and at every keyframe of every clip of the model (PR-57). Check a new box against the other limbs in the swing and the dodge.
- A new recipe kind needs two users and a decision (D-111, D-507).
