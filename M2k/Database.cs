using System.ComponentModel;

namespace M2kClient
{
    public enum Database
    {
        [Description("\\\\manage\\Manage\\ROI\\WCCO.MAIN\\")]
        WCCO = 0,
        [Description("\\\\manage\\Manage\\ROI\\CSI.MAIN\\")]
        CSI = 1,
        [Description("\\\\manage\\Manage\\ROI\\WCCO.TRAIN\\")]
        WCCOTRAIN = 2,
        [Description("\\\\manage\\Manage\\ROI\\CSI.TRAIN\\")]
        CSITRAIN = 3
    }
}
