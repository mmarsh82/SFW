using System.ComponentModel;

namespace M2kClient
{
    public enum Database
    {
        [Description("\\\\172.16.0.10\\ERP\\ROI\\CSI.MAIN\\")]
        CSI = 0,
        [Description("\\\\172.16.0.10\\ERP\\ROI\\WCCO.MAIN\\")]
        WCCO = 1,
        [Description("\\\\172.16.0.10\\ERP\\ROI\\WCCO.TRAIN\\")]
        WCCOTRAIN = 2,
        [Description("\\\\172.16.0.10\\ERP\\ROI\\CSI.TRAIN\\")]
        CSITRAIN = 3,
        [Description("\\\\172.16.0.10\\ERP\\ROI\\CONTI.MAIN\\")]
        CONTI = 4
    }
}
