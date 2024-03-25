using AForge.Video;
using AForge.Video.DirectShow;
using BookPaymentByCamera.Repo.Implements;
using BookPaymentByCamera.Repo.Interfaces;
using IronOcr;
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
        public decimal bookPrice {  get; set; }
        public string authorName {  get; set; }
        public string publisherName {  get; set; }
    }
    public partial class MainWindow : Window
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private System.Timers.Timer captureTimer;
        private System.Timers.Timer stopTimer;
        private IUnitOfWork unitOfWork = new UnitOfWork();
        public MainWindow()
        {
            InitializeComponent();
            btnCapture.IsEnabled = false;
            lvImage.ItemsSource = null;
            List<ImageModel> list = GetImageModels("D:\\StudyDocuments\\PRN221\\GroupProject\\BookPaymentByCamera\\BookPaymentByCamera\\CapturedImages");
            lvImage.ItemsSource = list;
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
                //captureTimer.Stop();
                //captureTimer.Dispose();
            }
            //InitializeTimer();

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
        //private void InitializeTimer()
        //{
        //    // Set the capture interval to 1 second (1000 milliseconds)
        //    int captureInterval = 1000 / 60;

        //    captureTimer = new System.Timers.Timer(captureInterval);
        //    //captureTimer.Interval = TimeSpan.FromMilliseconds(captureInterval);
        //    captureTimer.Elapsed += Capture;
        //    captureTimer.Start();

        //    // Set the stop interval to 1 second (1000 milliseconds)
        //    int stopInterval = 1000;

        //    stopTimer = new System.Timers.Timer(stopInterval);
        //    stopTimer.Elapsed += StopCaptureTimer;
        //    stopTimer.Start();
        //}

        //private void StopCaptureTimer(object sender, ElapsedEventArgs e)
        //{
        //    // Stop the captureTimer after 1 second
        //    captureTimer.Stop();
        //    stopTimer.Stop();
        //    stopTimer.Dispose();
        //}


        private void Capture(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (videoSource.IsRunning && videoSource != null)
                {
                    Thread.Sleep(1000);
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

                        //System.Windows.MessageBox.Show($"Image saved to: {filePath}");

                        //lvImage.ItemsSource = null;
                        //List<ImageModel> list = GetImageModels(txtBrowse.Text);
                        //lvImage.ItemsSource = list;
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
            });
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

                    var ocrList = OcrScan(filePath);
				    foreach (var item in ocrList)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
							var check = unitOfWork.BookRepository.Get(_ => _.BookName.ToLower().Contains("harry".ToLower()), null, "Author,Publisher").FirstOrDefault();
							if (check != null)
							{
								List<BookDTO> listBooks = new List<BookDTO>();
								listBooks.Add(new BookDTO() { bookName = check.BookName, bookPrice = (decimal)check.BookPrice, authorName = check.Author.FullName, publisherName = check.Publisher.Name });
								lvDetail.ItemsSource = listBooks;
							}
						}
                    }
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

        private List<string> OcrScan(string imageUrl)
        {
            var ironOcr = new IronTesseract();
            var image = new System.Drawing.Bitmap(imageUrl);
            var result = ironOcr.Read(image);
            Paragraph paragraph = new Paragraph(new Run(result.Text));
            //rtxtOcr.Document.Blocks.Add(paragraph);

            string[] lines = result.Text.Split(new char[] { '\n' });
            List<string> resultList = new List<string>(lines);
            return resultList;
        }

        //private void PaymentCheck()
        //{
        //    List<BookDTO> books = new List<BookDTO>();
        //    foreach (var item in lvPayment.Items)
        //    {
        //        if (!(item == null))
        //        {
        //            books.Add(new BookDTO
        //            {
                        
        //            });
        //        }
        //    }

        //    //save invoice
        //    string invoiceFilePath = SaveInvoiceToPdf(books);
        //    // Reset payment list
        //    lvPayment.Items.Clear();
        //    // Open the invoice PDF
        //    if (File.Exists(invoiceFilePath))
        //    {
        //        System.Diagnostics.Process.Start(invoiceFilePath);
        //    }
        //    else
        //    {
        //        System.Windows.MessageBox.Show("Invoice not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        //private string SaveInvoiceToPdf(List<BookDTO> books)
        //{
        //    // Create a new PDF document
        //    var pdf = new IronPdf.ChromePdfRenderer();

        //    // Construct HTML content for the invoice
        //    string htmlContent = "<h1>Invoice</h1><table><tr><th>Book Name</th><th>Author</th><th>Publisher</th><th>Price</th></tr>";
        //    foreach (var book in books)
        //    {
        //        htmlContent += $"<tr><td>{book.bookName}</td><td>{book.authorName}</td><td>{book.publisherName}</td><td>{book.bookPrice}</td></tr>";
        //    }
        //    htmlContent += "</table>";

        //    // Save HTML content to PDF file
        //    string invoiceFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "invoice.pdf");
        //    pdf.RenderHtmlAsPdf(htmlContent).SaveAs(invoiceFilePath);

        //    return invoiceFilePath;
        //}

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

            // Save PDF to file
            pdfDoc.SaveAs("invoice.pdf");
        }

        private void btnPayment_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = "D:\\StudyDocuments\\PRN221\\GroupProject\\BookPaymentByCamera\\BookPaymentByCamera\\CapturedImages";
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