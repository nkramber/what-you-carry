using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Core.Camera;

/// <summary>
/// Where the camera is and which way it looks, for one tick. The three directions are unit vectors: forward is
/// the look direction, right is horizontal, and up is at a right angle to both.
/// </summary>
/// <remarks>
/// Core derives a pose on each tick from the body position, the yaw and pitch sums, and the grid. It is not
/// state, and no hash reads it as one (D-245). The Game layer copies the position and the directions into the
/// camera node with no conversion (D-234).
/// </remarks>
public readonly record struct CameraPose(Vector3 Position, Vector3 Forward, Vector3 Right, Vector3 Up);
