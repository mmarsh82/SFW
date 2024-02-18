using System.ComponentModel;

namespace M2kClient
{
    public enum Database
    {
        [Description("\\\\10.20.177.21\\ERP\\ROI\\CSI.MAIN\\")]
        CSI = 0,
        [Description("\\\\10.20.177.21\\ERP\\ROI\\WCCO.MAIN\\")]
        WCCO = 1,
        [Description("\\\\10.20.177.21\\ERP\\ROI\\WCCO.TRAIN\\")]
        WCCOTRAIN = 2,
        [Description("\\\\10.20.177.21\\ERP\\ROI\\CSI.TRAIN\\")]
        CSITRAIN = 3,
        [Description("\\\\WAXAS001\\")]
        CONTI = 4,
        [Description("\\\\WAXAS001\\")]
        CONTITRAIN = 5
    }
}
