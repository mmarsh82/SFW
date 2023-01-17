using System;

namespace SFW.Enumerations
{
    [Flags]
    public enum UsersControls
    {
        // The flag for the part information query
        PartInformation = 0,
        // The flag for the work order schedule module
        Schedule = 1,
        // The flag for the cycle count module
        CycleCount = 3,
        // The flag for the administration module
        Admin = 4,
        // The flag for the closed work order schedule module
        ClosedSchedule = 5,
        // The flag for the part detail query
        PartDetail = 6,
        // The flag for the part traceability query
        PartTrace = 7,
        // The flag for the sales order module
        SalesOrder = 8,
        // The flag for the Quality module
        Quality = 9

    }
}
