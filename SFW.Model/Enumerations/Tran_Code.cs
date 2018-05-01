using System.ComponentModel;

namespace SFW.Model.Enumerations
{
    public enum Tran_Code
    {
        [Description("No Record")]
        NA = 0,
        [Description("Wip Receipt")]
        WIP_REC = 40,
        [Description("PO Receipt")]
        PO_REC = 41,
        [Description("Scrap")]
        SCRAP = 42,
        [Description("Misc. Receipt")]
        MISC_REC = 43,
        [Description("Issue")]
        ISSUE = 44,
        [Description("RMA Receipt")]
        RMA_REC = 45,
        [Description("Sales Consignment Receipt")]
        SCON_REC = 46,
        [Description("GL Issue")]
        GL_ISSUE = 47,
        [Description("Sales Consignment Shipment")]
        SCON = 48,
        [Description("Sales Shipment")]
        SALE = 49,
        [Description("Inventory Adjustment")]
        ADJUST = 50,
        [Description("MTO Shipment")]
        TRAN_OUT = 51,
        [Description("MTO Receipt")]
        TRAN_IN = 52,
        [Description("Location Transfer")]
        LOC_XFER = 53,
        [Description("Production Issue")]
        PROD_ISSUE = 54,
        [Description("Production Receipt")]
        PROD_REC = 55,
        [Description("RMA Shipment")]
        RMA_SHIP = 56,
        [Description("Purchase Consignment Receipt")]
        PCON_REC = 57,
        [Description("Purchase Consignment Usage")]
        PCON_USE = 58,
        [Description("Purchase Consignment Usage Receipt")]
        PCON_UREC = 59
    }
}
