using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Homebrew;
using ZillowParser.Components;
using ZillowParser.Rucaptcha;

namespace ZillowParcer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Default settings
        public MainWindow()
        {
            InitializeComponent();
            Controls.DebugBox = DebugBox;
            Controls.WorkProgress = WorkProgress;
            Controls.WorkProgressLabel = WorkProgressLabel;
        }

        private void ProgramWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ExitProgram_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void TelegramButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://t.me/iKolesDev");
        }

        private void Developer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TelegramButton_MouseDown(sender, e);
        }
        #endregion
        #region DragNDrop
        private void BaseDropDown_Copy_DragEnter(object sender, DragEventArgs e)
        {
            DropLable.Set("Отпустите левую кнопку мыши.");
            BaseDropDown_Copy.Fill = new SolidColorBrush(Color.FromRgb(0, 111, 111));
        }

        private void BaseDropDown_Copy_DragLeave(object sender, DragEventArgs e)
        {
            DropLable.Set("Перетащите Excel-файл в данную область.");
            BaseDropDown_Copy.Fill = new SolidColorBrush(Color.FromRgb(99, 92, 92));
        }

        private void BaseDropDown_Copy_Drop(object sender, DragEventArgs e)
        {

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                if (files[0].ToLower().EndsWith(".xlsx"))
                {
                    StartWork(files[0]);
                    BaseDropDown.Visibility = Visibility.Hidden;
                    BaseDropDown_Copy.Visibility = Visibility.Hidden;
                    DropLable.Visibility = Visibility.Hidden;
                    ExcelImage.Visibility = Visibility.Hidden;
                }
                else
                {
                    BaseDropDown_Copy_DragLeave(sender, e);
                }
            }

        }

        private void DropLable_DragEnter(object sender, DragEventArgs e)
        {
            BaseDropDown_Copy_DragEnter(sender, e);
        }

        private void DropLable_DragLeave(object sender, DragEventArgs e)
        {
            BaseDropDown_Copy_DragLeave(sender, e);
        }
        private void DropLable_Drop(object sender, DragEventArgs e)
        {
            BaseDropDown_Copy_Drop(sender, e);
        }

        private void Image_DragEnter(object sender, DragEventArgs e)
        {
            BaseDropDown_Copy_DragEnter(sender, e);
        }

        private void Image_DragLeave(object sender, DragEventArgs e)
        {
            BaseDropDown_Copy_DragLeave(sender, e);
        }

        private void Image_Drop(object sender, DragEventArgs e)
        {
            BaseDropDown_Copy_Drop(sender, e);
        }
        #endregion
        private void StartWork(string fileName)
        {
            DebugBox.WriteLine($"Получен файл {fileName}.");
            Thread thread = new Thread(() =>
            {
                string savedFileName = fileName;
                ExcelReader excelReader = new ExcelReader(savedFileName);
                HouseParser houseParser = new HouseParser(excelReader.ResultValues);
                ExcelWriter excel = new ExcelWriter(fileName, houseParser.results);
                DebugBox.WriteLine("Работа парсера успешно завершена!");

            });
            thread.IsBackground = true;
            thread.Start();
        }

    }
}
