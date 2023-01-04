using System;

namespace M2kClient.M2kADIArray
{
    public class DirectLabor
    {
        #region Properties

        /// <summary>
        /// Field 1
        /// Transaction Type
        /// Statically set to LD
        /// </summary>
        public string TranType { get { return "LD"; } }

        /// <summary>
        /// Field 2
        /// Transaction Station ID
        /// </summary>
        public string StationId { get; set; }

        /// <summary>
        /// Field 3
        /// Transaction Time
        /// Typically set to the time of the transaction on a 24 hour clock
        /// </summary>
        public string TranTime { get; set; }

        /// <summary>
        /// Field 4
        /// Transaction Date
        /// Typically set to DateTime.Today but could vary based on over night shift hours
        /// Transaction must use the MM-dd-yyyy format
        /// </summary>
        public string TranDate { get; set; }

        /// <summary>
        /// Field 6
        /// Employee number
        /// Must exist in the EMPLOYEE.MASTER file
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// Field 7
        /// Clock in or out
        /// Must be either 'I' or 'O' to pass validation
        /// </summary>
        public char ClockTransaction { get; set; }

        /// <summary>
        /// Field 8 Header
        /// Parent Work Order Number
        /// Will be added to the sequence during string creation
        /// </summary>
        public string WorkOrderNbr { get; set; }

        /// <summary>
        /// Field 8 Footer
        /// Work order operation or sequence
        /// Will be added to the work order during string creation
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Field 9
        /// Quantity Completed during labor transaction
        /// </summary>
        public int QtyCompleted { get; set; }

        /// <summary>
        /// Field 10
        /// Setups Completed during labor transaction
        /// </summary>
        public int SetCompleted { get; set; }

        /// <summary>
        /// Field 11
        /// Machine or Work center number
        /// </summary>
        public string MachineId { get; set; }

        /// <summary>
        /// Field 12
        /// Completion Flag
        /// </summary>
        public CompletionFlag CFlag { get; set; }

        /// <summary>
        /// Field 13
        /// Crew Size
        /// By default will pass as empty so that it can be determined by the routing for the sequence
        /// </summary>
        public int CrewSize { get; set; }

        /// <summary>
        /// Field 45
        /// Facility Code
        /// </summary>
        public string FacilityCode { get; set; }

        #endregion

        /// <summary>
        /// M2k LD ADI Array overloaded constructor
        /// Creates the object for passing data into M2k ERP software
        /// </summary>
        /// <param name="stationId">Station ID</param>
        /// <param name="empId">Employee ID</param>
        /// <param name="clockTran">Clock in or out transaction</param>
        /// <param name="tranTime">Clock transaction time</param>
        /// <param name="woNbr">Work order number</param>
        /// <param name="seq">Work order operation</param>
        /// <param name="qtyComp">Quantity completed</param>
        /// <param name="setComp">Setups completed</param>
        /// <param name="machId">Machine or work center number</param>
        /// <param name="cFlag">Completion flag</param>
        /// <param name="crew">Optional: Crew size, default will be determined by ERP</param>
        /// <param name="tranDate">Optional: Date of transaction</param>
        /// <param name="facCode">Optional: Facility code, default is 01</param>
        public DirectLabor(string stationId, string empId, char clockTran, string tranTime, string woNbr, string seq, int qtyComp, int setComp, string machId, CompletionFlag cFlag, string facCode, int crew = 0, string tranDate = "")
        {
            StationId = stationId;
            EmployeeId = empId;
            ClockTransaction = clockTran;
            TranTime = tranTime;
            WorkOrderNbr = woNbr;
            Operation = seq;
            QtyCompleted = qtyComp;
            SetCompleted = setComp;
            MachineId = machId;
            CFlag = cFlag;
            CrewSize = crew;
            FacilityCode = facCode;
            TranDate = string.IsNullOrEmpty(tranDate) ? DateTime.Today.ToString("MM-dd-yyyy") : tranDate;
        }

        /// <summary>
        /// Method Override
        /// Takes the object and deliminates it along with adding in the referenced field tag numbers
        /// </summary>
        /// <returns>Standard Direct Labor (LD) ADI string needed for the BTI to read</returns>
        public override string ToString()
        {
            //Transaction Template
            //1~Transaction Type~2~Station ID~3~Time~4~Date~6~Employee Number~7~Clock Transaction~8~Work Order Number*Operation~9~Quantity Completed~10~Setups Completed~11~Machine Number~12~Complete Flag~45~Facility Code~99~COMPLETE
            
            return CrewSize == 0 
                ? $"1~{TranType}~2~{StationId}~3~{TranTime}~4~{TranDate}~6~{EmployeeId}~7~{ClockTransaction}~8~{WorkOrderNbr}*{Operation}~9~{QtyCompleted}~10~{SetCompleted}~11~{MachineId}~12~{CFlag.ToString()}~45~{FacilityCode}~99~COMPLETE"
                : $"1~{TranType}~2~{StationId}~3~{TranTime}~4~{TranDate}~6~{EmployeeId}~7~{ClockTransaction}~8~{WorkOrderNbr}*{Operation}~9~{QtyCompleted}~10~{SetCompleted}~11~{MachineId}~12~{CFlag.ToString()}~13~{CrewSize}~45~{FacilityCode}~99~COMPLETE";
        }
    }
}
