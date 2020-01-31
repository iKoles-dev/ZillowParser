using Homebrew;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

namespace ZillowParser.Components
{
    public class ExcelReader:Parser
    {
        public List<string> ResultValues { get; private set; }
        public ExcelReader(string fileName)
        {
            ResultValues = new List<string>();
            DebugBox.WriteLine($"Приступаем к чтению {fileName}");
            Application ObjExcel = new Application();
            //Открываем книгу.                                                                                                                                                        
            Workbook ObjWorkBook = ObjExcel.Workbooks.Open(fileName, 0, false, 5, "", "", false, XlPlatform.xlWindows, "", true, false, 0, true, false, false);
            //Выбираем таблицу(лист).
            Worksheet ObjWorkSheet;
            ObjWorkSheet = (Worksheet)ObjWorkBook.Sheets[1];

            // Указываем номер столбца (таблицы Excel) из которого будут считываться данные.
            int numCol = 3;

            Range usedColumn = ObjWorkSheet.UsedRange.Columns[numCol];
            Array myvalues = (Array)usedColumn.Cells.Value2;
            List<string> allValues = new List<string>(myvalues.OfType<object>().Select(o => o.ToString()).ToArray());
            numCol = 5;
            usedColumn = ObjWorkSheet.UsedRange.Columns[numCol];
            myvalues = (Array)usedColumn.Cells.Value2;
            List<string> allAdditionalValues = new List<string>(myvalues.OfType<object>().Select(o => o.ToString()).ToArray());            
            for (int i = 0; allValues.Count>i; i++)
            {
                if (allAdditionalValues.Count <= i)
                {
                    break;
                }
                ResultValues.Add(allValues[i] + " " + allAdditionalValues[i]);
            }
            DebugBox.WriteLine($"Чтение файла завершено, найдено {ResultValues.Count} строк.");
            // Выходим из программы Excel.
            ObjExcel.Quit();
        }
    }
}
