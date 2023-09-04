using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using C1.WPF.C1Chart3D;
using DataGrid2DLibrary;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Microsoft.Win32;
using Excel = Microsoft.Office.Interop.Excel;

namespace MM_s
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            dataGrid2D.DataContext = this;
            double2DArray = new double[10, 10];
            CAtime.DataContext = this;
            this.AddHandler(Validation.ErrorEvent, new RoutedEventHandler(OnErrorEvent));
            tabControl.SelectedIndex = 0;


        }

        //private void CreateChart(int M, int N, double[] tArr, double[] xArr, double[,] CB)
        //{
        

        //    CB3dChart.Children.Clear();
        //    double stepx = 20.0;
        //    double stepy = 20.0;
        //    var zdata = new double[M / 20, N / 20];

        //    for (int i = 0; i < M / 20; i++)
        //    {
        //        for (int j = 0; j < N / 20; j++)
        //        {
        //            int x = Convert.ToInt32(tArr[i]);
        //            int z = Convert.ToInt32(xArr[j]);
        //            zdata[i, j] = CB[i, j];
        //        }
        //    }
        //    var ds = new GridDataSeries();
        //    ds.Start = new Point(0, 0);
        //    ds.Step = new Point(stepx, stepy);
        //    ds.ZData = zdata;

        //    CB3dChart.Children.Add(ds);
        //}

        private int errorCount;
        private void OnErrorEvent(object sender, RoutedEventArgs e)
        {
            var validationEventArgs = e as ValidationErrorEventArgs;
            if (validationEventArgs == null)
                throw new Exception("Данные некоректны");
            switch (validationEventArgs.Action)
            {
                case ValidationErrorEventAction.Added:
                    {
                        errorCount++;
                        break;
                    }
                case ValidationErrorEventAction.Removed:
                    {
                        errorCount--;
                        break;
                    }
                default:
                    {
                        throw new Exception("Неизвестное поведение");
                    }
            }
            button.IsEnabled = errorCount == 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InputItems input = new InputItems
            {
                D = 0.5,
                L = 60,
                Q = 250,
                CAin = 0.7,
                T = 257,
                k01 = 1.1,
                k02 = 3.7,
                Ea1 = 127170,
                Ea2 = 137270,
                DeltaX0 = 0.5,
                Ku = 2,
                eMax = 1.7,
                qMax = 10,
                step = 0.005
            };
            D_textBox.DataContext = input;
            L_textBox.DataContext = input;
            Q_textBox.DataContext = input;
            CAin_textBox.DataContext = input;
            T_textBox.DataContext = input;
            k01_textBox.DataContext = input;
            k02_textBox.DataContext = input;
            Ea1_textBox.DataContext = input;
            Ea2_textBox.DataContext = input;
            deltaX0_textBox.DataContext = input;
            Ku_textBox.DataContext = input;
            Emax_textBox.DataContext = input;
            qmax_textBox.DataContext = input;
            step_textBox.DataContext = input;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            double D = Convert.ToDouble(D_textBox.Text);
            double L = Convert.ToDouble(L_textBox.Text);
            double Q = Convert.ToDouble(Q_textBox.Text);
            double CAin = Convert.ToDouble(CAin_textBox.Text);
            double T = Convert.ToDouble(T_textBox.Text);
            double k01 = Convert.ToDouble(k01_textBox.Text) * 100000000000;
            double k02 = Convert.ToDouble(k02_textBox.Text) * 100000000000;
            double Ea1 = Convert.ToDouble(Ea1_textBox.Text);
            double Ea2 = Convert.ToDouble(Ea2_textBox.Text);
            double DeltaX0 = Convert.ToDouble(deltaX0_textBox.Text);
            double Ku = Convert.ToDouble(Ku_textBox.Text);
            double eMax = Convert.ToDouble(Emax_textBox.Text);
            double qMax = Convert.ToDouble(qmax_textBox.Text);
            double S = 0.25 * Math.PI * D * D;
            double u = (Q * 0.001) / S;
            double tR = L / u;
            double k1 = k01 * Math.Exp(-Ea1 / (8.31 * (T + 273)));
            double k2 = k02 * Math.Exp(-Ea2 / (8.31 * (T + 273)));
            double theta = 2 * tR;
            double q = 0;
            double eps = 2 * eMax;
            double deltaX = DeltaX0;
            double deltat = Ku * deltaX / u;
            int M = 0, M1 = 0, N = 0, N1 = 0;
            double[] xArr, tArr;
            double[,] CA, CB;
            double CBmax = 0, eA = 0;
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch watch2 = new System.Diagnostics.Stopwatch();
            for (; ; )
            {
                watch.Start();
                if (q == 0)
                {
                    deltaX = DeltaX0;
                    deltat = (Ku * deltaX) / u;
                    M = (int)Math.Round(L / deltaX);
                    N = (int)Math.Round(theta / deltat);
                    M1 = M;
                    N1 = N;
                }
                else
                {
                    deltaX /= 2;
                    deltat /= 2;
                    M *= 2;
                    N *= 2;
                }

                xArr = new double[M + 1];
                tArr = new double[N + 1];
                CA = new double[N + 1, M + 1];
                CB = new double[N + 1, M + 1];
                for (int i = 0; i < M + 1; i++)
                {
                    xArr[i] = i * deltaX;
                    CA[0, i] = 0;
                    CB[0, i] = 0;

                }
                for (int j = 0; j < N + 1; j++)
                {
                    tArr[j] = j * deltat;
                    CA[j, 0] = CAin;
                    CB[j, 0] = 0;

                }

                for (int j = 0; j < N; j++)
                {
                    for (int i = 1; i < M + 1; i++)
                    {
                        CA[j + 1, i] = (CA[j, i] + Ku * CA[j + 1, i - 1]) / (1 + Ku + k1 * deltat);
                        CB[j + 1, i] = (CB[j, i] + Ku * CB[j + 1, i - 1] + k1 * CA[j + 1, i] * deltat) / (1 + Ku + k2 * deltat);
                    }
                }

                double Sum = 0;

                if (q != 0)
                {

                    for (int j = 1; j < N1 + 1; j++)
                    {
                        for (int i = 1; i < M1 + 1; i++)
                        {
                            Sum += Math.Pow((CB[2 * j, 2 * i] - CB1[j, i]), 2);

                        }
                    }
                    double temp = (Sum / (M1 * N1));
                    eA = Math.Pow((temp), 0.5);
                    CBmax = CB.Cast<double>().Max();
                    eps = eA / CBmax * 100;
                }
                else
                {
                    CB1 = new double[N + 1, M + 1];
                    for (int j = 1; j <= N; j++)
                    {
                        for (int i = 1; i <= M; i++)
                        {

                            CB1[j, i] = CB[j, i];

                        }
                    }
                }

                CB1 = new double[N + 1, M + 1];
                for (int j = 1; j <= N; j++)
                {
                    for (int i = 1; i <= M; i++)
                    {

                        CB1[j, i] = CB[j, i];
                    }

                }
                M1 = M;
                N1 = N;
                q += 1;
                if ((eps <= eMax))
                {
                    watch.Stop();
                    watch2.Start();
                    CreateTable(N, M, CB);
                    CreateGraph1(tArr, M, CA);
                    CreateGraph2(tArr, M, CB);
                    CreateGraph3(xArr, M, CA);
                    CreateGraph4(xArr, M, CB);
                    //CreateChart(M, N, tArr, xArr, CB);
                    S_textBox.Text = Math.Round(S, 3).ToString();
                    u_textBox.Text = Math.Round(u, 3).ToString();
                    k1_textBox.Text = Math.Round(k1, 3).ToString();
                    k2_textBox.Text = Math.Round(k2, 3).ToString();
                    tR_textBox.Text = Math.Round(tR, 3).ToString();
                    theta_textBox.Text = Math.Round(theta, 3).ToString();
                    CBmax_textBox.Text = Math.Round(CBmax, 3).ToString();
                    deltax_textBox.Text = Math.Round(deltaX, 3).ToString();
                    deltat_textBox.Text = Math.Round(deltat, 3).ToString();
                    qout_textBox.Text = Math.Round((q - 1)).ToString();
                    epsa_textBox.Text = Math.Round(eA, 3).ToString();
                    eps_textBox.Text = Math.Round(eps, 3).ToString();
                    M_textBox.Text = M.ToString();
                    N_textBox.Text = N.ToString();
                    label_time.Content = "Время выполнения: " + (watch.ElapsedMilliseconds / 1000.0).ToString() + " с";
                    tabControl.SelectedIndex = 1;
                    watch2.Stop();
                    label_time2.Content = "Время визуализации: " + (watch2.ElapsedMilliseconds / 1000.0).ToString() + " с";
                    break;
                }
                if (q > qMax)
                {
                    System.Windows.MessageBox.Show(messageBoxText: "Решение с погрешностью, не превосходящей предельно допустимую погрешность, не получено!", caption: "Ошибка!",
                        button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
                    break;
                }
            }
        }

        public void CreateTable(int N, int M, double[,] CB)
        {
            StatusTable.Minimum = 0;
            StatusTable.Maximum = N * M;

            double2DArray = new double[N + 1, M + 1];
            for (int j = 1; j <= N; j++)
            {
                for (int i = 1; i <= M; i++)
                {
                    StatusTable.Value++;
                    double2DArray[j, i] = Math.Round(CB[j, i], 3);
                }

            }
            Binding datagrid2dBinding = new Binding();
            datagrid2dBinding.Path = new PropertyPath("double2DArray");
            dataGrid2D.SetBinding(DataGrid2D.ItemsSource2DProperty, datagrid2dBinding);
        }
        public void CreateGraph1(double[] tArr, int M, double[,] CA)
        {
            ChartValues<ObservablePoint> series1 = new ChartValues<ObservablePoint>();
            for (int i = 0; i < M; i++)
            {
                series1.Add(new ObservablePoint
                {
                    X = Math.Round(tArr[i], 3),
                    Y = Math.Round(CA[i, M], 3)
                });
            };
            var brush = new SolidColorBrush(Colors.Transparent)
            {
                Opacity = 0.25
            };
            var series = new LineSeries
            {
                Values = series1,
                Title = "График зависимости выходной концентрации сырьевого компонента А от времени",
                Stroke = new SolidColorBrush(Colors.Blue),
                LineSmoothness = 0,
                PointGeometry = null,
                Fill = brush
            };
            var seriesCollection = new SeriesCollection { series };
            if (CAtime.AxisY[0] != null && CAtime.AxisY[0].Labels != null)
                CAtime.AxisY[0].Labels.Clear();
            if (CAtime.Series != null)
                CAtime.Series.Clear();
            CAtime.AxisY[0].Title = "Выходная концентрация сырьевого компонента А, моль/л";
            CAtime.AxisX[0].Title = "Время, мин";
            CAtime.AxisX[0].MinValue = 0;
            CAtime.AxisY[0].MinValue = 0;
            CAtime.Series = seriesCollection;
        }

        public void CreateGraph2(double[] tArr, int M, double[,] CB)
        {
            ChartValues<ObservablePoint> series2 = new ChartValues<ObservablePoint>();
            for (int i = 0; i < M; i++)
            {
                series2.Add(new ObservablePoint
                {
                    X = Math.Round(tArr[i], 3),
                    Y = Math.Round(CB[i, M], 3)
                });
            }
            var brush = new SolidColorBrush(Colors.Transparent)
            {
                Opacity = 0.25
            };
            var series = new LineSeries
            {
                Values = series2,
                Title = "График зависимости выходной концентрации целевого компонента В от времени",
                Stroke = new SolidColorBrush(Colors.OrangeRed),
                LineSmoothness = 0,
                PointGeometry = null,
                Fill = brush
            };
            var seriesCollection = new SeriesCollection { series };
            if (CBtime.AxisY[0] != null && CBtime.AxisY[0].Labels != null)
                CBtime.AxisY[0].Labels.Clear();
            if (CBtime.Series != null)
                CBtime.Series.Clear();
            CBtime.AxisY[0].Title = "Выходная концентрация целевого компонента В, моль/л";
            CBtime.AxisX[0].Title = "Время, мин";
            CBtime.AxisX[0].MinValue = 0;
            CBtime.AxisY[0].MinValue = 0;
            CBtime.Series = seriesCollection;
        }

        public void CreateGraph3(double[] xArr, int M, double[,] CA)
        {
            ChartValues<ObservablePoint> series3 = new ChartValues<ObservablePoint>();
            for (int i = 0; i < M; i++)
            {
                series3.Add(new ObservablePoint
                {
                    X = Math.Round(xArr[i], 3),
                    Y = Math.Round(CA[M, i], 3)
                });
            }
            var brush = new SolidColorBrush(Colors.Transparent)
            {
                Opacity = 0.25
            };
            var series = new LineSeries
            {
                Values = series3,
                Title = "График зависимости конечной концентрации сырьевого компонента А от координаты по длине реактора",
                Stroke = new SolidColorBrush(Colors.BlueViolet),
                LineSmoothness = 0,
                PointGeometry = null,
                Fill = brush
            };
            var seriesCollection = new SeriesCollection { series };
            if (CAlen.AxisY[0] != null && CAlen.AxisY[0].Labels != null)
                CAlen.AxisY[0].Labels.Clear();
            if (CAlen.Series != null)
                CAlen.Series.Clear();
            CAlen.AxisY[0].Title = "Конечная концентрация сырьевого компонента А, моль/л";
            CAlen.AxisX[0].Title = "Координата по длине реактора, м";
            CAlen.AxisX[0].MinValue = 0;
            CAlen.AxisY[0].MinValue = 0;
            CAlen.Series = seriesCollection;
        }

        public void CreateGraph4(double[] xArr, int M, double[,] CB)
        {
            ChartValues<ObservablePoint> series4 = new ChartValues<ObservablePoint>();
            for (int i = 0; i < M; i++)
            {
                series4.Add(new ObservablePoint
                {
                    X = Math.Round(xArr[i], 3),
                    Y = Math.Round(CB[M, i], 3)
                });
            }
            var brush = new SolidColorBrush(Colors.Transparent)
            {
                Opacity = 0.25
            };
            var series = new LineSeries
            {
                Values = series4,
                Title = "График зависимости конечной концентрации целевого компонента В от координаты по длине реактора",
                Stroke = new SolidColorBrush(Colors.DarkSalmon),
                LineSmoothness = 0,
                PointGeometry = null,
                Fill = brush
            };
            var seriesCollection = new SeriesCollection { series };
            if (CBlen.AxisY[0] != null && CBlen.AxisY[0].Labels != null)
                CBlen.AxisY[0].Labels.Clear();
            if (CBlen.Series != null)
                CBlen.Series.Clear();
            CBlen.AxisY[0].Title = "Конечная концентрация целевого компонента А, моль/л";
            CBlen.AxisX[0].Title = "Координата по длине реактора, м";
            CBlen.AxisX[0].MinValue = 0;
            CBlen.AxisY[0].MinValue = 0;
            CBlen.Series = seriesCollection;
        }


        public double[,] double2DArray { get; set; }
        public double[,] CB1 { get; set; }
        public double[,] CB { get; set; }

        private void TextBox_Error(object sender, ValidationErrorEventArgs e)
        {
          
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl.SelectedIndex == 1)
            {
                InputItems inputItems = new InputItems();
                inputItems.D = Convert.ToDouble(D_textBox.Text);
                inputItems.L = Convert.ToDouble(L_textBox.Text);
                inputItems.Q = Convert.ToDouble(Q_textBox.Text);
                inputItems.CAin = Convert.ToDouble(CAin_textBox.Text);
                inputItems.T = Convert.ToDouble(T_textBox.Text);
                inputItems.k01 = Convert.ToDouble(k01_textBox.Text);
                inputItems.k02 = Convert.ToDouble(k02_textBox.Text);
                inputItems.Ea1= Convert.ToDouble(Ea1_textBox.Text);
                inputItems.Ea2 = Convert.ToDouble(Ea2_textBox.Text);
                inputItems.DeltaX0 = Convert.ToDouble(deltaX0_textBox.Text);
                inputItems.Ku = Convert.ToDouble(Ku_textBox.Text);
                inputItems.eMax = Convert.ToDouble(Emax_textBox.Text);
                inputItems.qMax = Convert.ToDouble(qmax_textBox.Text);
                int RowCount = Convert.ToInt32(M_textBox.Text);
                int ColumnCount = Convert.ToInt32(N_textBox.Text);
                for (int i = 0; i < Convert.ToInt32(N_textBox.Text); i++)
                {
                    for (int j = 0; j < Convert.ToInt32(N_textBox.Text); j++)
                    {

                        CB = (double[,])dataGrid2D.ItemsSource2D;
                    }
                }
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "xlsx files (*.xlsx)|*.xlsx|All files(*.*)|*.*";
                string file_name = string.Empty;

                Excel.Application excelapp = new Excel.Application();
                Excel.Workbook workbook = excelapp.Workbooks.Add();
                Excel.Worksheet worksheet = workbook.ActiveSheet;
                object misValue = System.Reflection.Missing.Value;
                if (save.ShowDialog() == true)
                {
                    file_name = save.FileName;
                    try
                    {
                        worksheet.Cells[1, 2] = "Геометрические параметры реактора";
                        worksheet.Cells[2, 1] = "Диаметр, м:";
                        worksheet.Cells[2, 6] = inputItems.D;
                        worksheet.Cells[4, 1] = "Длина, м:";
                        worksheet.Cells[4, 6] = inputItems.L;
                        worksheet.Cells[6, 2] = "Режимные параметры поцесса";
                        worksheet.Cells[7, 1] = "Расход потока через реактор, л/мин:";
                        worksheet.Cells[7, 6] = inputItems.Q;
                        worksheet.Cells[8, 1] = "Температура смеси в реакторе, °С";
                        worksheet.Cells[8, 6] = inputItems.T;
                        worksheet.Cells[9, 2] = "Варьируемый технологический параметр";
                        worksheet.Cells[10, 1] = "Входная концентрация компонента А, моль/л:";
                        worksheet.Cells[11, 6] = inputItems.CAin;
                        worksheet.Cells[12, 2] = "Эмпирические коэффициенты модели - кинетические параметры процесса";
                        worksheet.Cells[13, 1] = "Предэкспоненциальный множитель для константы первой реакции, 1/мин • 10¹¹:";
                        worksheet.Cells[13, 6] = inputItems.k01;
                        worksheet.Cells[14, 1] = "Предэкспоненциальный множитель для константы второй реакции, 1/мин • 10¹¹:";
                        worksheet.Cells[14, 6] = inputItems.k02;
                        worksheet.Cells[15, 1] = "Энергия активации первой реакции, Дж/моль:";
                        worksheet.Cells[15, 6] = inputItems.Ea1;
                        worksheet.Cells[16, 1] = "Энергия активации второй реакции, Дж/моль:";
                        worksheet.Cells[16, 6] = inputItems.Ea2;
                        worksheet.Cells[17, 2] = "Параметры метода решений уравнений";
                        worksheet.Cells[18, 1] = "Начальный шаг сетки по длине реактора, м:";
                        worksheet.Cells[18, 6] = inputItems.DeltaX0;
                        worksheet.Cells[19, 1] = "Сеточное число Куранта:";
                        worksheet.Cells[19, 6] = inputItems.Ku;
                        worksheet.Cells[20, 1] = "Максимальное число делений сетки пополам:";
                        worksheet.Cells[20, 6] = inputItems.qMax;
                        worksheet.Cells[21, 1] = "Предельно допустимая погрешность расчета, %:";
                        worksheet.Cells[21, 6] = inputItems.eMax;
                        worksheet.Cells[23, 2] = "Результаты расчета:";
                        worksheet.Cells[24, 1] = "S, м^2:";
                        worksheet.Cells[24, 4] = Convert.ToDouble(S_textBox.Text);
                        worksheet.Cells[25, 1] = "u, м/мин:";
                        worksheet.Cells[25, 4] = Convert.ToDouble(u_textBox.Text);
                        worksheet.Cells[26, 1] = "k1, 1/мин:";
                        worksheet.Cells[26, 4] = Convert.ToDouble(k1_textBox.Text);
                        worksheet.Cells[27, 1] = "k2, 1/мин:";
                        worksheet.Cells[27, 4] = Convert.ToDouble(k2_textBox.Text);
                        worksheet.Cells[28, 1] = "τR, мин:";
                        worksheet.Cells[28, 4] = Convert.ToDouble(tR_textBox.Text);
                        worksheet.Cells[29, 1] = "Θ, мин:";
                        worksheet.Cells[29, 4] = Convert.ToDouble(theta_textBox.Text);
                        worksheet.Cells[30, 1] = "M:";
                        worksheet.Cells[30, 4] = Convert.ToDouble(M_textBox.Text);
                        worksheet.Cells[24, 8] = "CBmax:";
                        worksheet.Cells[24, 11] = Convert.ToDouble(CBmax_textBox.Text);
                        worksheet.Cells[25, 8] = "Δx, м:";
                        worksheet.Cells[25, 11] = Convert.ToDouble(deltax_textBox.Text);
                        worksheet.Cells[26, 8] = "Δt, мин:";
                        worksheet.Cells[26, 11] = Convert.ToDouble(deltat_textBox.Text);
                        worksheet.Cells[27, 8] = "q:";
                        worksheet.Cells[27, 11] = Convert.ToDouble(qout_textBox.Text);
                        worksheet.Cells[28, 8] = "εa, моль/л:";
                        worksheet.Cells[28, 11] = Convert.ToDouble(epsa_textBox.Text);
                        worksheet.Cells[29, 8] = "ε, %:";
                        worksheet.Cells[29, 11] = Convert.ToDouble(eps_textBox.Text);
                        worksheet.Cells[30, 8] = "N:";
                        worksheet.Cells[30, 11] = Convert.ToDouble(N_textBox.Text);
                        worksheet.Cells[32, 2] = "CB:";
                        Excel.Range range = (Excel.Range)worksheet.Cells[34, 1];
                        range = range.Resize[RowCount, ColumnCount];
                        range.Value[Excel.XlRangeValueDataType.xlRangeValueDefault] = CB;
                        excelapp.AlertBeforeOverwriting = false;
                        workbook.SaveAs(file_name, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                            Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                        MessageBox.Show(messageBoxText: "Данные сохранены успешно!", caption: "Информация",
                            button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                    }
                    catch
                    {
                        MessageBox.Show(messageBoxText: "При сохранении данных произошла ошибка!", caption: "Ошибка!",
                            button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
                    }
                    finally
                    {
                        excelapp.Quit();
                        Marshal.ReleaseComObject(worksheet);
                        Marshal.ReleaseComObject(workbook);
                        Marshal.ReleaseComObject(excelapp);
                    }
                }
            }

            else
            {
                MessageBox.Show(messageBoxText: "Перейдите на вкладку результатов.", caption: "Внимание!", button: MessageBoxButton.OK, icon: MessageBoxImage.None);
            }
        }
    }


    public class InputItems
    {
        public double D { get; set; }
        public double L { get; set; }
        public double Q { get; set; }
        public double CAin { get; set; }
        public double T { get; set; }
        public double k01 { get; set; }
        public double k02 { get; set; }
        public double Ea1 { get; set; }
        public double Ea2 { get; set; }
        public double DeltaX0 { get; set; }
        public double Ku { get; set; }
        public double eMax { get; set; }
        public double qMax { get; set; }
        public double step { get; set; }
    }

    class DecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return value.ToString().Replace(".", ",");
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return value.ToString().Replace(".", ",");
            else
                return null;
        }
    }

    public class ValidateTextBoxRules : ValidationRule
    {
        public int Min { get; set; }

        public ValidateTextBoxRules()
        {
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double result = 0;

            try
            {
                if (((string)value).Length > 0)
                {
                    result = Double.Parse((String)value);
                        
                }      
            }
            catch
            {
                return new ValidationResult(false, $"Значение содержит недопустимые символы!");
            }

            if (result <= Min)
            {
                return new ValidationResult(false,
                  $"Значение должно быть больше 0!");
            }
            
            return ValidationResult.ValidResult;
        }
    }
    
}

