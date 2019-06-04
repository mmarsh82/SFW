using System.ComponentModel;

namespace M2kClient
{
    public enum AdjustCode
    {
        [Description("Cycle Count")]
        CC = 0,
        [Description("Drop")]
        DRO = 1,
        [Description("Quality Scrap")]
        QSC = 2,
        [Description("Quality Testing")]
        TES = 3,
        [Description("Yield")]
        YIE = 4
    }
}
