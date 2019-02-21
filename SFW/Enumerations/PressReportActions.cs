using System.ComponentModel;

namespace SFW.Enumerations
{
    public enum PressReportActions
    {
        [Description("Submit")]
        New = 0,
        [Description("Submit")]
        StartShift = 1,
        [Description("Update")]
        ViewReport = 2,
        [Description("Update")]
        LogProgress = 3
    }
}
