using System.ComponentModel;

namespace M2kClient
{
    public enum Database
    {
        [Description("\\\\manage\\Manage\\ROI\\WCCO.MAIN\\BTI.TRANSACTIONS\\")]
        WCCO = 0,
        [Description("\\\\manage\\Manage\\ROI\\CSI.MAIN\\BTI.TRANSACTIONS\\")]
        CSI = 1,
        [Description("\\\\manage\\Manage\\ROI\\WCCO.TRAIN\\BTI.TRANSACTIONS\\")]
        WCCOTRAIN = 2,
        [Description("\\\\manage\\Manage\\ROI\\CSI.TRAIN\\BTI.TRANSACTIONS\\")]
        CSITRAIN = 3
    }
}
