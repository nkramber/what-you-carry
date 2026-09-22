using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Game.Ui;

/// <summary>
/// The text of each element of the HUD, from the string table (G-8, D-98). The table holds each format, and the
/// code gives the numbers alone, so no text that a player reads lives in the code.
/// </summary>
/// <remarks>
/// The timer shows whole seconds, rounded up, so it shows <c>0:00</c> on the tick of expiry and never before
/// (D-443). The prompt names the interact input of the device of the last input (D-447, D-448).
/// </remarks>
public static class HudText
{
    /// <summary>The id of the format of the health number: the health, then the most health (D-442).</summary>
    public const string HealthId = "hud.health";

    /// <summary>The id of the format of the timer: the minutes, then the seconds (D-443).</summary>
    public const string TimerId = "hud.timer";

    /// <summary>The id of the paused mark under the timer (D-443).</summary>
    public const string PausedId = "hud.paused";

    /// <summary>The id of the name of the boss bar placeholder (D-445).</summary>
    public const string BossPlaceholderId = "hud.bossPlaceholder";

    /// <summary>The id of the format of a choice that a tap makes: the input, then the choice (D-448).</summary>
    public const string TapId = "hud.tap";

    /// <summary>The id of the format of a choice that a hold makes: the input, then the choice (D-448).</summary>
    public const string HoldId = "hud.hold";

    /// <summary>The id of the name of the interact key on the keyboard (D-289).</summary>
    public const string KeyboardInteractId = "input.keyboard.interact";

    /// <summary>The id of the name of the interact button on the controller (D-289).</summary>
    public const string ControllerInteractId = "input.controller.interact";

    /// <summary>The id of the descend choice (D-50).</summary>
    public const string DescendId = "run.descend";

    /// <summary>The id of the ascend choice (D-50).</summary>
    public const string AscendId = "run.ascend";

    /// <summary>The seconds in one minute.</summary>
    public const long SecondsPerMinute = 60;

    /// <summary>The message of the error for a count of ticks below zero.</summary>
    public const string NegativeTicksMessage = "The timer shows a count of ticks of zero or more.";

    private const string TicksField = "ticks";

    /// <summary>The health number, such as <c>72 / 100</c> (D-442).</summary>
    public static string Health(Strings strings, int health, int most)
    {
        return string.Format(CultureInfo.InvariantCulture, strings.Get(HealthId), health, most);
    }

    /// <summary>The timer of a count of ticks that remain, such as <c>4:07</c>, in whole seconds rounded up (D-443).</summary>
    /// <exception cref="ContextException">The count of ticks is below zero.</exception>
    public static string Timer(Strings strings, long remainingTicks)
    {
        if (remainingTicks < 0)
        {
            ContextException error = new(NegativeTicksMessage);
            error.AddContext(TicksField, remainingTicks.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        long seconds = (remainingTicks + SimulationLoop.TicksPerSecond - 1) / SimulationLoop.TicksPerSecond;
        return string.Format(CultureInfo.InvariantCulture, strings.Get(TimerId), seconds / SecondsPerMinute, seconds % SecondsPerMinute);
    }

    /// <summary>The descend line of the stairwell prompt: a tap of interact on the device of the last input (D-448).</summary>
    public static string Descend(Strings strings, bool controller)
    {
        return string.Format(CultureInfo.InvariantCulture, strings.Get(TapId), Interact(strings, controller), strings.Get(DescendId));
    }

    /// <summary>The ascend line of the stairwell prompt: a hold of interact on the device of the last input (D-448).</summary>
    public static string Ascend(Strings strings, bool controller)
    {
        return string.Format(CultureInfo.InvariantCulture, strings.Get(HoldId), Interact(strings, controller), strings.Get(AscendId));
    }

    /// <summary>The name of the interact input of one device.</summary>
    private static string Interact(Strings strings, bool controller)
    {
        return strings.Get(controller ? ControllerInteractId : KeyboardInteractId);
    }
}
