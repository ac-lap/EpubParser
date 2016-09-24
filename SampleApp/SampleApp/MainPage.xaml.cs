using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SampleApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public StorageFile file;
        private EpubParser fileParser;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void loadFile_clicked(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            openPicker.FileTypeFilter.Add(".epub");

            file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                fileParser = new EpubParser(file);
                await fileParser.loadFile();

                Tuple<string, List<byte>> temp_text = fileParser.getTextOfNextPage();
                List<byte> BoldList;

                if (temp_text != null)
                {
                    BoldList = temp_text.Item2;
                    tb.Text = temp_text.Item1;
                }

                next.IsEnabled = prev.IsEnabled = true;
                t_total.Text = fileParser.pageCount().ToString();
                t_cur.Text = (fileParser.curPageNum() + 1).ToString();
            }
        }

        private void nextPage_click(object sender, RoutedEventArgs e)
        {
            Tuple<string, List<byte>> temp_text = fileParser.getTextOfNextPage();

            if (temp_text != null)
            {
                tb.Text = temp_text.Item1;
                t_cur.Text = (fileParser.curPageNum() + 1).ToString();
            }
        }

        private void prevPage_click(object sender, RoutedEventArgs e)
        {
            Tuple<string, List<byte>> temp_text = fileParser.getTextofPrevPage();

            if (temp_text != null)
            {
                tb.Text = temp_text.Item1;
                t_cur.Text = (fileParser.curPageNum() + 1).ToString();
            }
        }
    }
}
