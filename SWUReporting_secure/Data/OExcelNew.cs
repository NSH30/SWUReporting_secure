using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using System.Data;

namespace SWUReporting
{
    class OExcelNew
    {
        #region Test Code


        public static void testNewDoc()
        {
            //MemoryStream ms = new MemoryStream();
            string filepath = "E:\\testing\\MyWorkbook.xlsx";
            SpreadsheetDocument xl = SpreadsheetDocument.Create(filepath, SpreadsheetDocumentType.Workbook);
            WorkbookPart wbp = xl.AddWorkbookPart();
            wbp.Workbook = new Workbook();
            Sheets sheets = xl.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            WorksheetPart wsp = wbp.AddNewPart<WorksheetPart>();
            //wsp.ChangeIdOfPart()
            wbp.ChangeIdOfPart(wsp, "rId1");
            Worksheet ws = new Worksheet();
            SheetData sd = new SheetData();
            Row rowData = new Row();
            for (int i = 0; i < 5; i++)
            {
                Cell c = new Cell();
                c.DataType = CellValues.Number;
                c.CellValue = new CellValue(i);
                rowData.AppendChild(c);
            }
            sd.AppendChild(rowData);
            ws.Append(sd);
            wsp.Worksheet = ws;
            wsp.Worksheet.Save();
            Sheet sheet = new Sheet();
            sheet.Name = "One";
            sheet.SheetId = 1;
            sheet.Id = wbp.GetIdOfPart(wsp);
            sheets.Append(sheet);

            WorksheetPart wsp2 = wbp.AddNewPart<WorksheetPart>();
            wbp.ChangeIdOfPart(wsp2, "rId2");
            Worksheet ws2 = new Worksheet();
            SheetData sd2 = new SheetData();
            Row rowHeader = new Row();
            for (int i = 0; i < 5; i++)
            {
                Cell c = new Cell();
                c.DataType = CellValues.String;
                c.CellValue = new CellValue("Header" + i.ToString());
                rowHeader.AppendChild(c);
            }
            sd2.AppendChild(rowHeader);
            Row rowData2 = new Row();
            for (int i = 0; i < 5; i++)
            {
                Cell c = new Cell();
                c.DataType = CellValues.Number;
                c.CellValue = new CellValue(i);
                rowData2.AppendChild(c);
            }
            sd2.AppendChild(rowData2);
            ws2.AppendChild(sd2);
            wsp2.Worksheet = ws2;
            wsp2.Worksheet.Save();
            Sheet sheet2 = new Sheet();
            sheet2.Name = "Two";
            sheet2.SheetId = 2;
            sheet2.Id = wbp.GetIdOfPart(wsp2);
            sheets.Append(sheet2);
            xl.Close();



        }
        #endregion
        public static string exportDocument(string fileName, DataSet tableSet, string[] sheetNames, string folderName = "")
        {
            WorkbookPart wBookPart = null;
            //string filePath = Path.GetTempPath() + fileName;
            string filePath = ReportIO.savePath + folderName + fileName;  //added folderName 8/17/2021 for download all vAR dashboards
            string folderPath = System.IO.Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
            {
                //create it
                Directory.CreateDirectory(folderPath);
            }
            using (SpreadsheetDocument spreadsheetDoc = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                wBookPart = spreadsheetDoc.AddWorkbookPart();
                wBookPart.Workbook = new Workbook();

                uint sheetId = 1;
                spreadsheetDoc.WorkbookPart.Workbook.Sheets = new Sheets();
                Sheets sheets = spreadsheetDoc.WorkbookPart.Workbook.GetFirstChild<Sheets>();

                foreach (DataTable table in tableSet.Tables)
                {
                    int i = (int)sheetId;

                    WorksheetPart wSheetPart = wBookPart.AddNewPart<WorksheetPart>();

                    Sheet sheet = new Sheet() { Id = spreadsheetDoc.WorkbookPart.GetIdOfPart(wSheetPart), SheetId = sheetId, Name = sheetNames[i - 1] };
                    sheets.Append(sheet);

                    SheetData sheetData = new SheetData();
                    wSheetPart.Worksheet = new Worksheet(sheetData);

                    Row headerRow = new Row();
                    foreach (DataColumn column in table.Columns)
                    {
                        Cell cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(column.ColumnName);
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);

                    foreach (DataRow dr in table.Rows)
                    {
                        Row row = new Row();
                        foreach (DataColumn column in table.Columns)
                        {
                            Cell cell = new Cell();
                            cell.DataType = CellValues.String;
                            cell.CellValue = new CellValue(dr[column].ToString());
                            row.AppendChild(cell);
                        }
                        sheetData.AppendChild(row);
                    }
                    sheetId++;
                }
            }
            return filePath;
        }

        // Given text and a SharedStringTablePart, creates a SharedStringItem with the specified text 
        // and inserts it into the SharedStringTablePart. If the item already exists, returns its index.
        private static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
        {
            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }

            int i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }

                i++;
            }

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
            shareStringPart.SharedStringTable.Save();

            return i;
        }

        private static IEnumerable<KeyValuePair<string, Worksheet>> GetNamedWorksheets(WorkbookPart workbookPart)
        {
            return workbookPart.Workbook.Sheets.Elements<Sheet>()
                .Select(sheet => new KeyValuePair<string, Worksheet>
                    (sheet.Name, GetWorkSheetFromSheet(workbookPart, sheet)));
        }

        private static Worksheet GetWorkSheetFromSheet(WorkbookPart workbookPart, Sheet sheet)
        {
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
            return worksheetPart.Worksheet;
        }

        public static void ReadDocumentSheets(string filePath)
        {
            using (SpreadsheetDocument ssD = SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart wbP = ssD.WorkbookPart;
                var res = GetNamedWorksheets(wbP);
                foreach (var name in res)
                {
                    Console.WriteLine(name.Key);
                }
            }
        }

        public static void ReadDocument(string filePath)
        {
            using (SpreadsheetDocument ssD = SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart wbP = ssD.WorkbookPart;
                //WorksheetPart wsP = wbP.WorksheetParts.First();
                var sheets = wbP.Workbook.Descendants<Sheet>();

            }
        }

        public static void ReadTwoDocs(string[] filePath)
        {
            dynamic sheets;
            dynamic sheets2;
            using (SpreadsheetDocument ssD = SpreadsheetDocument.Open(filePath[0], false))
            {
                WorkbookPart wbP = ssD.WorkbookPart;
                //WorksheetPart wsP = wbP.WorksheetParts.First();
                sheets = wbP.Workbook.Descendants<Sheet>();
            }
            using (SpreadsheetDocument ssD = SpreadsheetDocument.Open(filePath[1], false))
            {
                WorkbookPart wbP = ssD.WorkbookPart;
                //WorksheetPart wsP = wbP.WorksheetParts.First();
                sheets2 = wbP.Workbook.Descendants<Sheet>();
            }
        }
    }


}