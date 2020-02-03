using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Homebrew;

namespace ZillowParser.Components
{
    public class ExcelWriter
    {
        public ExcelWriter(string fileName, List<Zillow> zillows)
        {
            Controls.DebugBox.WriteLine($"Приступаем к записи информации в {fileName}");
            Application ObjExcel = new Application();
            //Открываем книгу.                                                                                                                                                        
            Workbook ObjWorkBook = ObjExcel.Workbooks.Open(fileName, 0, false, 5, "", "", false, XlPlatform.xlWindows, "", true, false, 0, true, false, false);
            //Выбираем таблицу(лист).
            Worksheet ObjWorkSheet;
            ObjWorkSheet = (Worksheet)ObjWorkBook.Sheets[1];

            // Указываем номер столбца (таблицы Excel) из которого будут считываться данные.
            for (int i = 2; i < zillows.Count+2; i++)
            {
                ObjWorkSheet.Cells[i, 12] = zillows[i - 2].Zestimate;
                ObjWorkSheet.Cells[i, 13] = zillows[i - 2].Status;
                ObjWorkSheet.Cells[i, 14] = zillows[i - 2].SoldPrice;
                ObjWorkSheet.Cells[i, 15] = zillows[i - 2].URL;
            }

            ObjWorkSheet.SaveAs(fileName);
            ObjExcel.Visible = true;
            // Выходим из программы Excel.
            //ObjWorkBook.Close(true);
            //ObjExcel.Quit();
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(ObjWorkBook);
            //ObjExcel = null;
            //ObjWorkBook = null;
            //ObjWorkSheet = null;
            //System.GC.Collect();
        }
    }
}
