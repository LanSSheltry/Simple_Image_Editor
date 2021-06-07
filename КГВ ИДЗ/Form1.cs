using System;
using System.Drawing;
using System.IO;
using ZedGraph;
using System.Windows.Forms;

namespace ЕМПІ_СККР
{

    public partial class Form1 : Form
    {
        Matrix matrix;
        int filecounter = 0; //Счётчик для сохранения сгенерированных файлов
        string imagepath; //Путь к выбранному изображению
        bool matrixswtchr = false; //Значения для обработчика нажатия на определенное изображение
        double[,] ColorCount; //Массив значений для отрисовки значений диаграммы яркости 2 изображения
        double[,] ColorCount1; //Массив значений для отрисовки значений диаграммы яркости 1 изображения
        int counterF = -1, counterS=-1;
        GraphPane pane; //Экземпляр класса для отображения графика

        public Form1()
        {
            InitializeComponent();
            pane = zedGraphControl1.GraphPane;
            pane.Fill.Type = FillType.Solid;
            pane.Fill.Color = Color.Gray;
            pane.Chart.Fill.Type = FillType.Solid;
            pane.Chart.Fill.Color = Color.DarkGray;
            pane.XAxis.MajorGrid.IsVisible = true;
            pane.YAxis.MajorGrid.IsVisible = true;
            pane.XAxis.MajorGrid.Color = Color.LightBlue;
            pane.YAxis.MajorGrid.Color = Color.LightBlue;
        }

        private void Browse_Click(object sender, EventArgs e) //Нажатие на кнопку "Обзор..."
        {
            counterF = -1;
            counterS = -1;
                progressBar1.Maximum = 100;
                progressBar1.Minimum = 0;
                openFileDialog1.ShowDialog();
                imagepath = openFileDialog1.FileName;
                textBox1.Text = imagepath;
            try
            {
                matrix = new Matrix(imagepath); //Создаём экземпляр класса для обработки
                
                matrix.GetPixels(); //Вытаскиваем значения пикселей изображения в массив
                pictureBox1.ImageLocation = imagepath;
            }
            catch { textBox1.Text = "Не удалось открыть файл!"; }
        }

        private void Brightness_Click(object sender, EventArgs e)
        {
            matrix.Writer(++filecounter); //Записываем матрицу в текстовый файл
        }

        void DrawGraph(double[,] ColorCount, int index) //Отрисовка значений на диаграмме яркости
        {
            Color color = new Color();
            string name="Default";
            switch (index)
            {
                case 0:
                    name = "Гистограма яскравості";
                    pane.Title.Text = "Brightness";
                    color = Color.Yellow;
                    break;
                case 1:
                    pane.Title.Text = "Red";
                    name = "Красный";
                    color = Color.Red;
                    break;
                case 2:
                    pane.Title.Text = "Green";
                    name = "Зелёный";
                    color = Color.Green;
                    break;
                case 3:
                    pane.Title.Text = "Blue";
                    name = "Синий";
                    color = Color.Blue;
                    break;
            }

            pane.CurveList.Clear();
            int itemscount = 256;
            string[] names = new string[itemscount];
            double[] values = new double[itemscount];

            for (int i = 0; i < itemscount; i++)
            {
                names[i] = Convert.ToString(i);
                values[i] = ColorCount[i, 0];
            }

            pane.BarSettings.MinClusterGap = 0.0f;
            pane.YAxis.Title.Text = "Кількість пікселів";
            pane.XAxis.Title.Text = "Значення яскравості";
            BarItem curve = pane.AddBar(name, null, values, color);
            pane.XAxis.Type = AxisType.Text;
            pane.XAxis.Scale.TextLabels = names;
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }

        private void Binarization_Click(object sender, EventArgs e) //Нажатие на кнопку "Бинаризация"
        {
            ProgressBar_Set(ref progressBar1, 10, "Будь ласка, зачекайте...");
            matrix.GetPixels();
            ProgressBar_Set(ref progressBar1, 30);
            matrix.Binarization(ref filecounter);
            ProgressBar_Set(ref progressBar1, 50);
            LastStepOfAction();
        }

        private void Grey(object sender, EventArgs e) //Нажатие на кнопку "Градации серого"
        {
            ProgressBar_Set(ref progressBar1, 10, "Будь ласка, зачекайте...");
            matrix.GetPixels();
            ProgressBar_Set(ref progressBar1, 30);
            matrix.ToGreyGradation(1, ref filecounter);
            ProgressBar_Set(ref progressBar1, 80);
            LastStepOfAction();
        }

        private void Picture1_Click(object sender, EventArgs e) //Обработчик события нажатия на изображение 1
        {
            counterS = -1;
            counterF++;
            counterF %= 4;
            matrixswtchr = false;
            ColorCount1 = matrix.CountBrightness(matrixswtchr, counterF);
            DrawGraph(ColorCount1, counterF);
        }

        private void Picture2_Click(object sender, EventArgs e)
        {
            counterF = -1;
            counterS++; 
            counterS %= 4;
            matrixswtchr = true;
            ColorCount = matrix.CountBrightness(matrixswtchr, counterS);
            DrawGraph(ColorCount, counterS);
        }

        private void Smoothing_Click(object sender, EventArgs e) //Нажатие на кнопку "сглаживание"
        {
            ProgressBar_Set(ref progressBar1, 10, "Будь ласка, зачекайте...");
            matrix.GetPixels();
            ProgressBar_Set(ref progressBar1, 30);
            matrix.Smoothing(ref filecounter);
            ProgressBar_Set(ref progressBar1, 80);
            LastStepOfAction();
        }

        private void Negative_Click(object sender, EventArgs e)
        {
            ProgressBar_Set(ref progressBar1, 10, "Будь ласка, зачекайте...");
            matrix.GetPixels();
            ProgressBar_Set(ref progressBar1, 30);
            matrix.ToGreyGradation(2, ref filecounter);
            ProgressBar_Set(ref progressBar1, 50);
            LastStepOfAction();
        }

        private void Delete_Click(object sender, EventArgs e) //Нажатие на кнопку "Удалить" все изображения в папке назначения
        {
            ProgressBar_Set(ref progressBar1, 10, "Будь ласка, зачекайте...");
            for (int i=0; i<=filecounter; i++)
            {
                progressBar1.Value += 5;
                try 
                { 
                    File.Delete(Convert.ToString(i + ".bmp"));
                    File.Delete(Convert.ToString(i+".txt"));
                }
                catch { continue; }
            }
            ProgressBar_Set(ref progressBar1, 100, "Виконано!");
        }

        private void ProgressBar_Set(ref ProgressBar pb, int value, string text="")
        {
            pb.Value = value;
            if (text != "") { label1.Text = text; label1.Refresh(); }
        }

        private void LastStepOfAction()
        {
            pictureBox2.ImageLocation = Convert.ToString(filecounter + ".bmp");
            ProgressBar_Set(ref progressBar1, 100, "Виконано!");
        }
    }
}
