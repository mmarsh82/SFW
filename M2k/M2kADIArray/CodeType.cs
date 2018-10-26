using System.ComponentModel;

namespace M2kClient.M2kADIArray
{
    public enum CodeType
    {
        [Description("Stock")]
        S = 0,
        [Description("Inspection")]
        I = 1,
        [Description("General Ledger Account")]
        G = 2,
        [Description("Work Order")]
        W = 3
    }
}
