using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Core.Replay;

/// <summary>
/// A reader of the loop after each replayed frame. The replay calls it once per complete frame, after the step,
/// so a caller can fold a value of every replayed tick without a copy of the traversal.
/// </summary>
/// <remarks>
/// The bit-identity sweep folds the camera pose and the aim ray of every replayed tick through it, so the three
/// platforms compare the camera from the replay and not from a second live run (PR-8 exit test 5). A test folds
/// the same values and compares them with a live loop. Those are the two concrete callers that D-111 asks for.
/// </remarks>
public interface IReplayObserver
{
    /// <summary>Reads the loop after one replayed frame. The loop is at the tick after that frame.</summary>
    void AfterTick(SimulationLoop loop);
}
