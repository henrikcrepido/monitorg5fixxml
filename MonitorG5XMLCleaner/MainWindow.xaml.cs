using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MonitorG5XMLCleaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void BtnSourceFolder_OnClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog() { IsFolderPicker = true })
            {
                var result = dialog.ShowDialog(this);

                if (result == CommonFileDialogResult.Ok)
                {
                    tbFolder.Text = dialog.FileName;
                    tbFolderTarget.Text = $"{dialog.FileName}\\rensade";
                }
            }

            btnLoadFiles.IsEnabled = true;
        }

        private void LoadFilesToGrid()
        {
            List<FileInfo> AllFiles = new DirectoryInfo(tbFolder.Text).GetFiles().ToList();

            dgSource.ItemsSource = AllFiles;

            btnCleanFile.IsEnabled = true;
        }

        private void BtnCleanFile_OnClick(object sender, RoutedEventArgs e)
        {
            CreateCleanDocuments();
        }

        private void CreateCleanDocuments()
        {
            if (!Directory.Exists(tbFolderTarget.Text))
            {
                Directory.CreateDirectory(tbFolderTarget.Text);
            }

            foreach (var item in dgSource.Items)
            {
                var fileInfo = ((FileInfo) item);

                XDocument xmlDoc = XDocument.Load(fileInfo.FullName);

                var rows = xmlDoc.Descendants("Row").ToList();

                foreach (var row in rows)
                {
                    var partNumber = row.Elements("Part").Select(x => x.Attributes("PartNumber").FirstOrDefault()?.Value)
                        ?.FirstOrDefault();
                    var text = row.Elements("Text").FirstOrDefault();
                    text.Value = partNumber;
                }

                var filename = $"{tbFolderTarget.Text}\\clean_{fileInfo.Name}";
                if (!File.Exists(filename))
                {
                    xmlDoc.Save(filename);
                }
                
            }

            btnOpenTarget.IsEnabled = true;
            btnCleanFile.IsEnabled = false;
        }

        private void BtnOpenTarget_OnClick(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(tbFolderTarget.Text))
            {
                Directory.CreateDirectory(tbFolderTarget.Text);
            }

            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = tbFolderTarget.Text;
            process.StartInfo.Verb = "Open";
            process.Start();
        }

        private void TbFolder_OnDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;

            var folder = GetFileName(e);
            if (folder != null && Directory.Exists(folder))
                e.Handled = true;
        }

        private void TbFolder_OnDrop(object sender, DragEventArgs e)
        {
            var folder = GetFileName(e);

            var textBox = sender as TextBox;
            if (textBox != null && Directory.Exists(folder))
            {
                textBox.Text = folder;
               
            }

            btnLoadFiles.IsEnabled = true;
            tbFolderTarget.Text = $"{textBox.Text}\\rensade";
        }

        private string GetFileName(DragEventArgs e)
        {
            var data = e.Data.GetData(DataFormats.FileDrop);

            var array = data as string[];
            if (array != null && array.Any())
            {
                return array.First();
            }
            return null;
        }


        private void TbFolder_OnPreviewDragOver(object sender, DragEventArgs e)
        {
            Mouse.SetCursor(Cursors.Hand);
            e.Handled = true;
        }

        private void BtnLoadFiles_OnClick(object sender, RoutedEventArgs e)
        {
            LoadFilesToGrid();
        }
    }
}
