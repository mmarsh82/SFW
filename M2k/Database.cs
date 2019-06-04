using System.ComponentModel;

namespace M2kClient
{
    public enum Database
    {
        [Description("\\\\ERP-WCCO\\ERP\\ROI\\WCCO.MAIN\\")]
        WCCO = 0,
        [Description("\\\\ERP-WCCO\\ERP\\ROI\\CSI.MAIN\\")]
        CSI = 1,
        [Description("\\\\ERP-WCCO\\ERP\\ROI\\WCCO.TRAIN\\")]
        WCCOTRAIN = 2,
        [Description("\\\\ERP-WCCO\\ERP\\ROI\\CSI.TRAIN\\")]
        CSITRAIN = 3
    }
}
