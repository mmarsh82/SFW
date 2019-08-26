using System.ComponentModel;

namespace M2kClient
{
    public enum AdjustCode
    {
        [Description("Cycle Count")]
        CC = 0,
        [Description("Reclaim")]
        REC = 1,
        [Description("Drop")]
        DRO = 2,
        [Description("Quality Scrap")]
        QSC = 3,
        [Description("Quality Testing")]
        TES = 4,
        [Description("Yield")]
        YIE = 5
    }
}
