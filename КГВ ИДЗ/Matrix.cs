using System;
using System.Drawing;
using System.IO;

namespace ЕМПІ_СККР
{

    public class Matrix
    {
        static Bitmap inputimage;
        public static int width, height;
        static byte[,,] FirstPicture; //Матрица яркости первоначального изображения
        static byte[,,] SecondPicture; //Матрица яркости обработанного изображения
        double[,] CountR = new double[256, 1]; //Подсчет компоненты R
        double[,] CountG = new double[256, 1]; //Подсчет компоненты G
        double[,] CountB = new double[256, 1]; //Подсчет компоненты B
        double[,] CountA = new double[256, 1];
        double[,] SMatrix =              //Матрица нормального (Гауссового) распределения 5х5
            {
                {0.000789, 0.006581, 0.013347, 0.006581, 0.000789 },
                {0.006581, 0.054901, 0.111345, 0.054901, 0.006581 },
                {0.013347, 0.111345, 0.225821, 0.111345, 0.013347 },
                {0.006581, 0.054901, 0.111345, 0.054901, 0.006581 },
                {0.000789, 0.006581, 0.013347, 0.006581, 0.000789}
            };
        public Matrix(string imagepath) //Конструктор для создания экземпляра класса по ссылке с первоначальными значениями
        {
            inputimage=new Bitmap(imagepath); 
        }

        public void GetPixels() //Метод для получения значений яркости 3х компонент каждого пикселя изображения в массив
        {
            width = inputimage.Width;
            height = inputimage.Height;
            FirstPicture = new byte[3, height, width];
            SecondPicture = new byte[3, height, width];
            for (int j=0; j<height; j++)
            {
                for (int i=0; i<width; i++)
                {
                    Color color = inputimage.GetPixel(i, j); //Получаем значения пикселя с указанным адресом
                    FirstPicture[0, j, i] = color.R; //Красный
                    FirstPicture[1, j, i] = color.G; //Зелёный
                    FirstPicture[2, j, i] = color.B; //Синий
                    SecondPicture[0, j, i] = color.R;
                    SecondPicture[1, j, i] = color.G;
                    SecondPicture[2, j, i] = color.B;
                }
            }
        }

        public void Writer(int filecounter) //Метод для создания текстового файла и записи в него матрицы яркости изображения
        {
            string Line="";
            string FileName = Convert.ToString(filecounter);
            byte[,,] Cache = FirstPicture;
            using (FileStream writer = new FileStream(FileName + ".txt", FileMode.OpenOrCreate))
            {
                for (int i = 0; i < height; i++) //Цикл для последовательной записи строк
                {
                    for (int j = 0; j < width; j++) //Цикл для последовательной записи значений в строку
                    {
                        Line = Line + Convert.ToString(" [R: " + Cache[0, i, j] + ", G: " + Cache[1, i, j] + ", B: " + Cache[2, i, j] + "]"); //Конкатенация значений и форматирование
                    }
                    byte[] array = System.Text.Encoding.Default.GetBytes(Line);
                    writer.Write(array, 0, array.Length);
                    Line = "\n";
                }
                writer.Close();
            }
        }

        public double [,] CountBrightness(bool matswr, int counter) //Подсчет яркости компонент каждого пикселя для построения диаграммы яркости
        {
            double Rcoef = 0.299, Gcoef = 0.587, Bcoef = 0.114;
            byte[,,] Array;
            byte CurrentColor;
            if (matswr == false) Array = FirstPicture;
            else Array = SecondPicture;
            for (int i=0; i<256; i++)
            {
                CountR[i, 0] = 0;
                CountG[i, 0] = 0;
                CountB[i, 0] = 0;
                CountA[i, 0] = 0;
            }
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                    CurrentColor = Convert.ToByte(Convert.ToInt32(Array[0, j, i] * Rcoef + Array[1, j, i] * Gcoef + Array[2, j, i] * Bcoef));
                    CountR[Array[0, j, i], 0]++;
                    CountG[Array[1, j, i], 0]++;
                    CountB[Array[2, j, i], 0]++;
                    CountA[CurrentColor, 0]++;
                    }
                }
                switch(counter)
            {
                case 0:
                    return CountA;
                case 1:
                    return CountR;
                case 2:
                    return CountG;
                case 3:
                    return CountB;
            }
            return CountA;
        }

        public Bitmap BitmapToImage(byte[,,] secondPicture) //Преобразование массива значений яркости в набор битов для дальнейшего сохранения в виде изображения
        {
            Bitmap secondpicture = new Bitmap(width, height);
            
            for (int j=0; j<height; j++)
            {
                for (int i=0; i<width; i++)
                {
                    secondpicture.SetPixel(i, j, Color.FromArgb(secondPicture[0, j, i], secondPicture[1, j, i], secondPicture[2, j, i]));
                }
            }
            return secondpicture;
        }

        public void Binarization(ref int filecounter) //Бинаризация изображения
        {
            byte Value;
            double Rcoef = 0.299, Gcoef = 0.587, Bcoef = 0.114;
            for (int j=0; j<height; j++)
            {
                for (int i=0; i<width; i++)
                {
                    Value = Convert.ToByte(FirstPicture[0, j, i] * Rcoef + FirstPicture[1, j, i] * Gcoef + FirstPicture[2, j, i] * Bcoef);
                    if (Value>127 & Value>127 & Value>127)
                    {
                        SecondPicture[0, j, i] = 255;
                        SecondPicture[1, j, i] = 255;
                        SecondPicture[2, j, i] = 255;
                    }
                    else
                    {
                        SecondPicture[0, j, i] = 0;
                        SecondPicture[1, j, i] = 0;
                        SecondPicture[2, j, i] = 0;
                    }
                }
            }
            FileSaver(ref filecounter);
        }

        public void ToGreyGradation(int Function, ref int filecounter) //Градации серого
        {
            double Rcoef = 0.299, Gcoef = 0.587, Bcoef = 0.114; //Коэффициенты для перехода к градациям серого
            byte CurrentColor;
            for (int j=0; j<height; j++)
            {
                for (int i=0; i<width; i++)
                {
                    CurrentColor = Convert.ToByte(Convert.ToInt32(FirstPicture[0, j, i]*Rcoef+FirstPicture[1, j, i]*Gcoef+FirstPicture[2, j, i]*Bcoef));
                    if (Function == 1)
                    {
                        SecondPicture[0, j, i] = CurrentColor;
                        SecondPicture[1, j, i] = CurrentColor;
                        SecondPicture[2, j, i] = CurrentColor;
                    }
                    if (Function==2)
                    {
                        SecondPicture[0, j, i] = Convert.ToByte(255-FirstPicture[0, j, i]);
                        SecondPicture[1, j, i] = Convert.ToByte(255- FirstPicture[1, j, i]);
                        SecondPicture[2, j, i] = Convert.ToByte(255- FirstPicture[2, j, i]);
                    }
                }
            }
            FileSaver(ref filecounter);
        }

        public void Smoothing(ref int filecounter) //Сглаживание
        {
            byte Value0, Value1, Value2;
            for (int j=2; j<height-2; j++)
            {
                for (int i=2; i<width-2; i++)
                {
                    Value0 = 0;
                    Value1 = 0;
                    Value2 = 0;
                    for (int k=-2; k<3; k++)
                    {
                        for(int l=-2; l<3; l++)
                        {
                            Value0 += Convert.ToByte(SMatrix[k+2, l+2] * Convert.ToDouble(SecondPicture[0, j + k, i + l]));
                            Value1 += Convert.ToByte(SMatrix[k+2, l+2] * Convert.ToDouble(SecondPicture[1, j + k, i + l]));
                            Value2 += Convert.ToByte(SMatrix[k+2, l+2] * Convert.ToDouble(SecondPicture[2, j + k, i + l]));
                        }
                    }
                    SecondPicture[0, j, i] = Value0;
                    SecondPicture[1, j, i] = Value1;
                    SecondPicture[2, j, i] = Value2;
                }
            }
            FileSaver(ref filecounter);
        }

        public void FileSaver(ref int filecounter)
        {
            BitmapToImage(SecondPicture).Save(Convert.ToString(++filecounter + ".bmp"));
        }
        
    }
}
