using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Input;
using System.Data;

namespace SFW.Commands
{
    public class DevTesting : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Command for testing
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            new PrintBarLabels().Execute("123456");



            /*
            using (var _skuTable = new DataTable())
            {
                using (var dataAdapter = new SqlDataAdapter(@"SELECT
	                                                            a.[Part_Number]
                                                                ,a.[Description]
                                                                ,b.[Edi_Udef_2] as 'Seq'
                                                                ,b.[Sequence_Desc] as 'Type'
	                                                            ,ISNULL(b.[Core], 'No Core') as 'Core'
	                                                            ,CASE WHEN b.[Heating_Cans] = '.' THEN 'NO' ELSE ISNULL(b.[Heating_Cans], 'NO') END as 'Heating'
	                                                            ,CASE WHEN b.[Cooling_Cans] = '.' THEN 'NO' ELSE ISNULL(b.[Cooling_Cans], 'NO') END as 'Cooling'
	                                                            ,CASE WHEN b.[Spreader_Bar] = '.' THEN 'NO' ELSE ISNULL(b.[Spreader_Bar], 'NO') END as 'Spreader'
	                                                            ,CASE WHEN b.[Duster] = '.' THEN 'NO' ELSE ISNULL(b.[Duster], 'NO') END as 'Duster'
	                                                            ,ISNULL(b.[Rollup_In], 'Not Listed') as 'Rollup'
	                                                            ,ISNULL(b.[Tape_Rolls], 'NO') as 'Tape'
	                                                            ,ISNULL(b.[Sendto], 'Not Listed') as 'SendTo'
                                                            FROM
	                                                            [dbo].[IM-INIT] a
                                                            LEFT OUTER JOIN
	                                                            [dbo].[IM-UDEF-SPEC-INIT] b ON b.[Edi_Udef_1] = a.[Part_Number]
                                                            WHERE
	                                                            a.[Accounting_Status] != 'O' AND b.[ID] IS NOT NULL AND b.[Edi_Udef_2] IS NOT NULL
                                                            ORDER BY
	                                                            a.[Part_Number];", App.AppSqlCon))
                {
                    dataAdapter.Fill(_skuTable);
                }
                foreach (DataRow _row in _skuTable.Rows)
                {
                    #region Setup Export

                    var _setupInst = UdefSku.GetSetUpInstructions(_row.Field<string>("Part_Number"), _row.Field<string>("Seq"), App.AppSqlCon).Split(new string[1] { "\n" }, StringSplitOptions.None);
                    using (var _wpDoc = WordprocessingDocument.Create($@"\\csi-prime\Prints\Setup\{_row.Field<string>("Part_Number")}.docx", WordprocessingDocumentType.Document))
                    {
                        // Add a main document part.
                        MainDocumentPart mainPart = _wpDoc.AddMainDocumentPart();

                        // Create the document structure and add some text.
                        mainPart.Document = new Document();
                        Body body = mainPart.Document.AppendChild(new Body());
                        Paragraph para = body.AppendChild(new Paragraph());
                        ParagraphProperties paraProp = para.AppendChild(new ParagraphProperties(new Justification { Val = JustificationValues.Center }));
                        Run bRun = para.AppendChild(new Run());
                        RunProperties rPropBold = bRun.AppendChild(new RunProperties(new Bold(), new Underline { Val = UnderlineValues.Single }, new FontSize { Val = "75" }));
                        bRun.AppendChild(new Text("Setup Instructions"));
                        bRun.AppendChild(new Break());
                        Run bRun1 = para.AppendChild(new Run());
                        RunProperties rPropBold1 = bRun1.AppendChild(new RunProperties(new Bold(), new FontSize { Val = "50" }));
                        bRun1.AppendChild(new Text($"{_row.Field<string>("Part_Number")}  {_row.Field<string>("Description")}"));
                        bRun1.AppendChild(new Break());
                        Run run = para.AppendChild(new Run());
                        RunProperties runProp = run.AppendChild(new RunProperties(new FontSize { Val = "35" }));
                        run.AppendChild(new Break());
                        run.AppendChild(new Text($"Heating Cans: {_row.Field<string>("Heating")}"));
                        run.AppendChild(new Break());
                        run.AppendChild(new Text($"Cooling Cans: {_row.Field<string>("Cooling")}"));
                        run.AppendChild(new Break());
                        run.AppendChild(new Text($"Spreader Bar: {_row.Field<string>("Spreader")}"));
                        run.AppendChild(new Break());
                        run.AppendChild(new Text($"Duster: {_row.Field<string>("Duster")}"));
                        run.AppendChild(new Break());
                        Run bRun2 = para.AppendChild(new Run());
                        RunProperties rPropBold2 = bRun2.AppendChild(new RunProperties(new Bold(), new Underline { Val = UnderlineValues.Single }, new FontSize { Val = "50" }));
                        bRun2.AppendChild(new Break());
                        bRun2.AppendChild(new Text("Instructions"));
                        Run run2 = para.AppendChild(new Run());
                        RunProperties run2Prop = run2.AppendChild(new RunProperties(new FontSize { Val = "35" }));
                        foreach (var s in _setupInst)
                        {
                            run2.AppendChild(new Break());
                            run2.AppendChild(new Text(s));
                        }
                    }

                    #endregion

                    #region Work Instruction Export

                    var _packInst = UdefSku.GetPackInstructions(_row.Field<string>("Part_Number"), _row.Field<string>("Seq"), App.AppSqlCon).Split(new string[1] { "\n" }, StringSplitOptions.None);
                    using (var _wpDoc = WordprocessingDocument.Create($@"\\csi-prime\Prints\WI\{_row.Field<string>("Part_Number")}.docx", WordprocessingDocumentType.Document))
                    {
                        // Add a main document part.
                        MainDocumentPart mainPart = _wpDoc.AddMainDocumentPart();

                        // Create the document structure and add some text.
                        mainPart.Document = new Document();
                        Body body = mainPart.Document.AppendChild(new Body());
                        Paragraph para = body.AppendChild(new Paragraph());
                        ParagraphProperties paraProp = para.AppendChild(new ParagraphProperties(new Justification { Val = JustificationValues.Center }));
                        Run bRun = para.AppendChild(new Run());
                        RunProperties rPropBold = bRun.AppendChild(new RunProperties(new Bold(), new Underline { Val = UnderlineValues.Single }, new FontSize { Val = "75" }));
                        bRun.AppendChild(new Text("Packing Instructions"));
                        bRun.AppendChild(new Break());
                        Run run2 = para.AppendChild(new Run());
                        RunProperties run2Prop = run2.AppendChild(new RunProperties(new FontSize { Val = "35" }));
                        foreach (var s in _packInst)
                        {
                            run2.AppendChild(new Break());
                            run2.AppendChild(new Text(s));
                        }
                    }

                    #endregion

                    #region Part Info Export

                    var _passInfo = new List<UdefSkuPass>();
                    var _partInfo = new UdefSkuPass();
                    if (_row.Field<string>("Type").Contains("SLIT"))
                    {
                        _partInfo = new UdefSkuPass(_row.Field<string>("Part_Number"), _row.Field<string>("Seq"), App.AppSqlCon);
                    }
                    else
                    {
                        _passInfo = UdefSkuPass.GetUdefPassList(_row.Field<string>("Part_Number"), _row.Field<string>("Seq"), App.AppSqlCon);
                    }
                    using (var _wpDoc = WordprocessingDocument.Create($@"\\csi-prime\Prints\Part\{_row.Field<string>("Part_Number")}.docx", WordprocessingDocumentType.Document))
                    {
                        // Add a main document part.
                        MainDocumentPart mainPart = _wpDoc.AddMainDocumentPart();

                        // Create the document structure and add some text.
                        mainPart.Document = new Document();
                        Body body = mainPart.Document.AppendChild(new Body());
                        Paragraph para = body.AppendChild(new Paragraph());
                        ParagraphProperties paraProp = para.AppendChild(new ParagraphProperties(new Justification { Val = JustificationValues.Center }));
                        Run bRun = para.AppendChild(new Run());
                        RunProperties rPropBold = bRun.AppendChild(new RunProperties(new Bold(), new Underline { Val = UnderlineValues.Single }, new FontSize { Val = "75" }));
                        bRun.AppendChild(new Text("Part Information"));
                        bRun.AppendChild(new Break());
                        Run bRun1 = para.AppendChild(new Run());
                        RunProperties rPropBold1 = bRun1.AppendChild(new RunProperties(new Bold(), new FontSize { Val = "50" }));
                        bRun1.AppendChild(new Text($"{_row.Field<string>("Part_Number")}  {_row.Field<string>("Description")}"));
                        bRun1.AppendChild(new Break());
                        Run cRun = para.AppendChild(new Run());
                        RunProperties cProp = cRun.AppendChild(new RunProperties(new Bold(), new FontSize { Val = "30" }));
                        cRun.AppendChild(new Text($"Use Core: {_row.Field<string>("Core")}"));
                        cRun.AppendChild(new Break());

                        //Slit document template
                        if (_row.Field<string>("Type").Contains("SLIT") && !string.IsNullOrEmpty(_partInfo.Instructions))
                        {
                            var _partInst = _partInfo.Instructions.Split(new string[1] { "\n" }, StringSplitOptions.None);
                            Run lRun = para.AppendChild(new Run());
                            RunProperties lRunProp = lRun.AppendChild(new RunProperties(new FontSize { Val = "30" }));
                            lRun.AppendChild(new Text($"Gumwall: {_partInfo.GumWall}     OAG: {_partInfo.OAG}    Lb/Ft: {_partInfo.PoundPerFoot}"));
                            lRun.AppendChild(new Break());
                            lRun.AppendChild(new Text("Instructions:"));
                            foreach (var s in _partInst)
                            {
                                lRun.AppendChild(new Break());
                                lRun.AppendChild(new Text(s));
                            }
                        }

                        //Calendar document template
                        else
                        {
                            foreach (var _pass in _passInfo)
                            {
                                var _passInst = _pass.Instructions.Split(new string[1] { "\n" }, StringSplitOptions.None);
                                Run hRun = para.AppendChild(new Run());
                                RunProperties hRunProp = hRun.AppendChild(new RunProperties(new Bold(), new Underline { Val = UnderlineValues.Single }, new FontSize { Val = "40" }));
                                hRun.AppendChild(new Text(_pass.Pass));
                                hRun.AppendChild(new Break());
                                Run lRun = para.AppendChild(new Run());
                                RunProperties lRunProp = lRun.AppendChild(new RunProperties(new FontSize { Val = "30" }));
                                lRun.AppendChild(new Text($"Temperature: {_pass.Temperature}    Top: {_pass.TopTemp}    Center: {_pass.CenterTemp}      Bottom: {_pass.BottomTemp}"));
                                lRun.AppendChild(new Break());
                                lRun.AppendChild(new Text($"GumWall: {_pass.GumWall}      Line Speed: {_pass.LineSpeed}       AtTable: {_pass.AtTable}"));
                                lRun.AppendChild(new Break());
                                lRun.AppendChild(new Text($"OAG: {_pass.OAG}      Volume: {_pass.Volume}      Lb/Ft: {_pass.PoundPerFoot}"));
                                lRun.AppendChild(new Break());
                                lRun.AppendChild(new Text("Instructions:"));
                                foreach (var s in _passInst)
                                {
                                    lRun.AppendChild(new Break());
                                    lRun.AppendChild(new Text(s));
                                }
                                lRun.AppendChild(new Break());
                            }
                        }

                        Run fRun = para.AppendChild(new Run());
                        RunProperties fRunProp = fRun.AppendChild(new RunProperties(new FontSize { Val = "30" }));
                        fRun.AppendChild(new Break());
                        fRun.AppendChild(new Text($"Tape Rolls: {_row.Field<string>("Tape")}      Send To: {_row.Field<string>("SendTo")}"));
                    }

                    #endregion
                }
            }*/
        }
        public bool CanExecute(object parameter) => true;
    }
}
