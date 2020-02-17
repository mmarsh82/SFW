using System.ComponentModel;

namespace M2kClient
{
    public enum Database
    {
        [Description("\\\\172.16.0.10\\ERP\\ROI\\WCCO.MAIN\\")]
        WCCO = 0,
        [Description("\\\\172.16.0.10\\ERP\\ROI\\CSI.MAIN\\")]
        CSI = 1,
        [Description("\\\\172.16.0.10\\ERP\\ROI\\WCCO.TRAIN\\")]
        WCCOTRAIN = 2,
        [Description("\\\\172.16.0.10\\ERP\\ROI\\CSI.TRAIN\\")]
        CSITRAIN = 3
    }
}
