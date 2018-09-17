using System;

namespace SFW.Enumerations
{
    [Flags]
    public enum UsersControls
    {
        // The flag for CSI Schedule is 0
        CSISchedule = 0,
        // The flag for CSI ShopRoute is 1
        CSIShopRoute = 1,
        // The flag for WCCO Schedule is 2
        WCCOSchedule = 2,
        // The flag for WCCO Schedule is 3
        WCCOShopRoute = 3,
        // The flag for Scheduler is 4
        Scheduler = 4,
        // The flag for Part Informtion is 5
        PartInfo = 5
    }
}
