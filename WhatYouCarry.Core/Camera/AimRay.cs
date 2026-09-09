using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Core.Camera;

/// <summary>
/// Where a shot aims: an origin and a unit direction (D-77). The ray starts at the camera position and runs along
/// the look direction, so it is the crosshair (D-247). Aim assist gives back a ray with the same origin and a
/// pulled direction (D-244).
/// </summary>
public readonly record struct AimRay(Vector3 Origin, Vector3 Direction);
