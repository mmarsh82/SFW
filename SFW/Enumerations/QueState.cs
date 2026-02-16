using System.ComponentModel;

namespace SFW.Enumerations
{
    public enum QueState
    {
        [Description("U")]
        Unassigned = -1,
        [Description("Q")]
        InQue = 0,
        [Description("S")]
        Setup = 1,
        [Description("I")]
        Inspection = 2,
        [Description("R")]
        Running = 3,
        [Description("P")]
        Paused = 4,
        [Description("D")]
        Down = 5
    }
}
