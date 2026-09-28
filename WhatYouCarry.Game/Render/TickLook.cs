namespace WhatYouCarry.Game.Render;

/// <summary>
/// The look of one tick: the yaw sum and the pitch sum of the loop, in hundredths of a degree (D-227, D-241). A frame
/// interpolates the look between two ticks, and not the camera pose, so the view stays on the boom circle (D-724).
/// </summary>
public readonly record struct TickLook(int Yaw, int Pitch);
