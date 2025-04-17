using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CG_lab1;

namespace CG_lab1
{
    public partial class Form1 : Form
    {
        Bitmap image;
        Stack<Bitmap> imageHistory;
        public Form1()
        {
            InitializeComponent();
            imageHistory = new Stack<Bitmap>();
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp | All Files (*.*) | *.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
            }
            pictureBox1.Image = image;
            pictureBox1.Refresh();
        }

        private void SaveImageHistory()
        {
            if (image != null)
            {
                Bitmap savedImage = new Bitmap(image);
                imageHistory.Push(savedImage);
            }
        }

        private void UndoLastChange()
        {
            if (imageHistory.Count > 0)
            {
                image = imageHistory.Pop();
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
        }

        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            Filters filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage(image, backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true)
            {
                image = newImage;
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;
        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void гауссToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void чернобелоеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            Filters filter = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            Filters filter = new SepiaFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void мАГАСИЯЙToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            Filters filter = new Brightness();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void собельToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            Filters filter = new Sobel();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            Filters filter = new Sharpness();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void тиснениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            Filters filter = new embossing();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs =
                    (System.IO.FileStream)saveFileDialog1.OpenFile();

                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        pictureBox1.Image.Save(fs,
                          System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                }

                fs.Close();
            }
        }

        private void dilationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            DilationFilter dilation = new DilationFilter(3);
            backgroundWorker1.RunWorkerAsync(dilation);

            //Bitmap resultImage = dilation.ApplyDilation(image);
            //pictureBox1.Image = resultImage;
            //pictureBox1.Refresh();
        }

        private void erosionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            ErosionFilter erosion = new ErosionFilter(3);
            backgroundWorker1.RunWorkerAsync(erosion);
        }

        private void openingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            OpeningFilter opening = new OpeningFilter(3);
            Bitmap resultImage = opening.ApplyOpening(image, backgroundWorker1);
            pictureBox1.Image = resultImage;
            pictureBox1.Refresh();
            backgroundWorker1.CancelAsync();

            //backgroundWorker1.RunWorkerAsync(opening);
        }

        private void closingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            OpeningFilter opening = new OpeningFilter(3);
            Bitmap resultImage = opening.ApplyOpening(image, backgroundWorker1);
            pictureBox1.Image = resultImage;
            pictureBox1.Refresh();
            backgroundWorker1.CancelAsync();

            //backgroundWorker1.RunWorkerAsync(closing);
        }

        private void градиентToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            Gradient gradient = new Gradient(3);
            Bitmap resultImage = gradient.ApplyGradient(image, backgroundWorker1);
            pictureBox1.Image = resultImage;
            pictureBox1.Refresh();
            backgroundWorker1.CancelAsync();
            //backgroundWorker1.RunWorkerAsync(gradient);
        }

        private void медианныйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            MedianFilter medianFilter = new MedianFilter(3);
            backgroundWorker1.RunWorkerAsync(medianFilter);


            //Bitmap resultImage = medianFilter.ApplyMedianFilter(image, backgroundWorker1);
            //pictureBox1.Image = resultImage;
            //pictureBox1.Refresh();
        }

        private void идеальныйОтражательToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            Filters filter = new Reflector();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void стеклоToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            SaveImageHistory();
            Filters filter = new GlassEffect();
            backgroundWorker1.RunWorkerAsync(filter);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            UndoLastChange();
        }

        private void линейноеРастяжениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageHistory();
            LinearStretching filter = new LinearStretching();

            //backgroundWorker1.RunWorkerAsync(filter);


            Bitmap resultImage = filter.ApplyLinearStretching(image); //, backgroundWorker1);

            pictureBox1.Image = resultImage;
            pictureBox1.Refresh();
            backgroundWorker1.CancelAsync();
        }
    }
}
