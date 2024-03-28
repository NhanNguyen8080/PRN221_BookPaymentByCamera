using AForge.Video;
using AForge.Video.DirectShow;
using BookPaymentByCamera.Repo.Implements;
using BookPaymentByCamera.Repo.Interfaces;
using IronOcr;
using IronSoftware.Drawing;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BookPaymentByCamera
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class BookDTO
    {
        public string bookName { get; set; }
        public decimal bookPrice { get; set; }
        public string authorName { get; set; }
        public string publisherName { get; set; }
    }

    public class BookDTOPayment
    {
        public string BookName { get; set; }
        public decimal BookPrice { get; set; }
        public int Quantity { get; set; }
        public decimal? Total { get; set; }
    }
    public partial class MainWindow : Window
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private IUnitOfWork unitOfWork = new UnitOfWork();
        List<BookDTO> listBooks;
        List<BookDTOPayment> listBooksPayment;
        string workingDirectory;
        string rootFolderPath;
        string folderPath;
        private static readonly string[] VietnameseSigns = new string[]
        {
            "aAeEoOuUiIdDyY",
            "áàạảãâấầậẩẫăắằặẳẵ",
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ",
            "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ",
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ",
            "ÚÙỤỦŨƯỨỪỰỬỮ",
            "íìịỉĩ",
            "ÍÌỊỈĨ",
            "đ",
            "Đ",
            "ýỳỵỷỹ",
            "ÝỲỴỶỸ"
        };

        public MainWindow()
        {
            InitializeComponent();
            btnCapture.IsEnabled = false;
            lvImage.ItemsSource = null;
            workingDirectory = Environment.CurrentDirectory;
            rootFolderPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            folderPath = System.IO.Path.Combine(rootFolderPath, "CapturedImages");
            List<ImageModel> list = GetImageModels(folderPath);
            //List<ImageModel> list = GetImageModels("C:\\Users\\T14\\source\\repos\\PRN221_BookPaymentByCamera-vunt(1)\\PRN221_BookPaymentByCamera-vunt\\BookPaymentByCamera\\CapturedImages");
            lvImage.ItemsSource = list;
            listBooks = new List<BookDTO>();
            listBooksPayment = new List<BookDTOPayment>();
        }

        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (btnStartStop.Content.Equals("Start"))
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count > 0)
                {
                    videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                    videoSource.NewFrame += VideoSource_NewFrame;
                    videoSource.Start();
                }
                else
                {
                    System.Windows.MessageBox.Show("Notification", "No video devices found!", MessageBoxButton.OK);
                }
                btnStartStop.Content = "Stop";
                btnCapture.IsEnabled = true;
            }

            else
            {
                if (videoSource != null && videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    imgWebcam.Source = null;
                }
                btnStartStop.Content = "Start";
                btnCapture.IsEnabled = false;
            }

        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            imgWebcam.Dispatcher.Invoke(() =>
            {
                imgWebcam.Source = ToBitmapImage(eventArgs.Frame);
            });


        }

        private BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private List<ImageModel> GetImageModels(string folderPath)
        {
            List<ImageModel> imageModels = new List<ImageModel>();

            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath);
                int id = 1;
                foreach (var file in files)
                {
                    var lastIndexOfPoint = file.LastIndexOf(".");
                    var extension = file.Substring(lastIndexOfPoint);
                    if (extension.Equals(".png") || extension.Equals(".jpg") || extension.Equals(".jpeg"))
                    {
                        Bitmap imageBitmap = new Bitmap(file);
                        try
                        {
                            var imageSource = ToBitmapImage(imageBitmap);
                            imageModels.Add(new ImageModel { Id = id, Image = imageSource });
                            id++;
                        }
                        finally
                        {
                            imageBitmap.Dispose();
                        }
                    }

                }
                return imageModels.ToList();
            }

            return null;

        }

        private void btnCapture_Click(object sender, EventArgs e)
        {

            if (videoSource.IsRunning && videoSource != null)
            {
                if (imgWebcam.Source != null && imgWebcam.Source is BitmapSource bitmapSource)
                {

                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    var randomString = Guid.NewGuid().ToString().Substring(0, 32);
                    var fileName = $"{randomString}_{DateTime.Now:yyyyMMddHHmmSS}.png";
                    string workingDirectory = Environment.CurrentDirectory;
                    var rootFolderPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
                    var folderPath = System.IO.Path.Combine(rootFolderPath, "CapturedImages");
                    var filePath = System.IO.Path.Combine(folderPath, fileName);

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }


                    lvImage.ItemsSource = null;
                    List<ImageModel> list = GetImageModels(folderPath);
                    lvImage.ItemsSource = list;

                    //int count = 1;

                    var ocrList = OcrScan(filePath);
                    bool found = false;
                    foreach (var item in ocrList)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            var itemCheck = RemoveSign4VietnameseString(item).ToLower();
                            //var check = unitOfWork.BookRepository.Get(_ => RemoveSign4VietnameseString(_.BookName).ToLower().Contains(itemCheck), null, "Author,Publisher").FirstOrDefault();
                            var items = unitOfWork.BookRepository.Get(null, null, "Author,Publisher").ToList();
                            var check = items.Where(b => RemoveSign4VietnameseString(b.BookName).ToLower().Contains(itemCheck)).FirstOrDefault();
                            if (check != null && found == false)
                            {
                                var checkExistBook = listBooks.Where(_ => _.bookName.Equals(check.BookName)).FirstOrDefault();
                                if (checkExistBook == null)
                                {
                                    listBooks.Add(new BookDTO() { bookName = check.BookName, bookPrice = (decimal)check.BookPrice, authorName = check.Author.FullName, publisherName = check.Publisher.Name });
                                }

                                var checkExistBookPayment = listBooksPayment.Where(_ => _.BookName.Equals(check.BookName)).FirstOrDefault();
                                if (checkExistBookPayment == null)
                                {
                                    listBooksPayment.Add(new BookDTOPayment() { BookName = check.BookName, BookPrice = (decimal)check.BookPrice, Quantity = 1, Total = check.BookPrice * 1 });
                                }
                                else
                                {
                                    ++checkExistBookPayment.Quantity;
                                    checkExistBookPayment.Total = checkExistBookPayment.BookPrice * checkExistBookPayment.Quantity;
                                }
                                
                                found = true;
                            }
                        }
                    }
                    lvPayment.ItemsSource = null;
                    lvDetail.ItemsSource = null;
                    lvPayment.ItemsSource = listBooksPayment;
                    lvDetail.ItemsSource = listBooks;
                    decimal? totalPrice = 0;
                    foreach (var book in listBooksPayment)
                    {
                        totalPrice += book.Total;
                    }
                    lblTotalPrice.Content = totalPrice.ToString();

                }

                else
                {
                    System.Windows.MessageBox.Show($"No image to save!");
                }
            }
            else
            {
                System.Windows.MessageBox.Show($"Your webcam is closing!");
            }

        }

        public static string RemoveSign4VietnameseString(string str)
        {
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                {
                    str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
                }
            }
            return str;
        }

        private List<string> OcrScan(string imageUrl)
        {
            var ironOcr = new IronTesseract();
            ironOcr.AddSecondaryLanguage(OcrLanguage.English);
            ironOcr.AddSecondaryLanguage(OcrLanguage.Vietnamese);
            //var image = new System.Drawing.Bitmap(imageUrl);
            using var imageInput = new OcrInput();
            imageInput.LoadImage("D:\\StudyDocuments\\PRN221\\GroupProject\\PRN221_BookPaymentByCamera\\BookPaymentByCamera\\CapturedImages\\Screenshot 2024-03-28 211143.png");
            System.Drawing.Rectangle textCropArea = imageInput.GetPages().First().FindTextRegion();
            var stamp = imageInput.StampCropRectangleAndSaveAs(textCropArea, System.Drawing.Color.Red, folderPath + "\\image_text_area", AnyBitmap.ImageFormat.Png);
            OcrResult result = ironOcr.Read("D:\\StudyDocuments\\PRN221\\GroupProject\\PRN221_BookPaymentByCamera\\BookPaymentByCamera\\CapturedImages\\Screenshot 2024-03-28 211143.png", textCropArea);
            //Paragraph paragraph = new Paragraph(new Run(result.Text));
            //rtxtOcr.Document.Blocks.Add(paragraph);

            string[] lines = result.Text.Split(new char[] { '\n' });
            List<string> resultList = new List<string>(lines);
            List<string> cleanedList = new List<string>();
            foreach (var word in resultList)
            {
                cleanedList.Add(word.Replace("\r", ""));
            }
            return cleanedList;
        }

        // Method to save ListView data to PDF
        private void SaveToPDF(System.Windows.Controls.ListView listView)
        {
            // Create a new PDF document
            var pdf = new IronPdf.ChromePdfRenderer();

            // Create HTML content for PDF
            string htmlContent = "<html><head><title>Invoice</title></head><body><h1>Invoice</h1><table border='1'><tr>";

            // Add header row
            foreach (GridViewColumn column in ((GridView)listView.View).Columns)
            {
                htmlContent += "<th>" + column.Header + "</th>";
            }
            htmlContent += "</tr>";

            // Add data rows
            foreach (object item in listView.Items)
            {
                htmlContent += "<tr>";
                foreach (var property in item.GetType().GetProperties())
                {
                    htmlContent += "<td>" + property.GetValue(item) + "</td>";
                }
                htmlContent += "</tr>";
            }

            htmlContent += "</table></body></html>";

            // Generate PDF from HTML content
            var pdfDoc = pdf.RenderHtmlAsPdf(htmlContent);

            // Specify the full path to the directory where you want to save the PDF file
            string pdfFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "invoice.pdf");

            // Save PDF to file
            pdfDoc.SaveAs(pdfFilePath);
        }

        private void btnPayment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if the folder exists
                if (Directory.Exists(folderPath))
                {
                    // Get all files in the folder
                    string[] files = Directory.GetFiles(folderPath);

                    // Get invoice pdf file
                    SaveToPDF(lvPayment);



                    // Iterate through each file and delete it
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }

                    System.Windows.MessageBox.Show("Files removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show("Folder does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                lvImage.ItemsSource = null;
                List<ImageModel> list = GetImageModels(folderPath);
                lvImage.ItemsSource = list;

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}