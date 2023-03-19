using Fraktaly;
using Fraktaly.Fractals;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fractals
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Grid? ParaGrid;
        public double canvasWidth;
        public double canvasHeight;
        public Brush canvasBackground;
        public string fractal;
        public double ZoomFactor;
        public double Zoom;
        public bool FractalChanged;
        public double[] parameters;
        public SimpleUpDownControl upDownZoomFactor;
        public Slider sliderZoomFactor;
        public SimpleUpDownControl upDownZoom;
        public Slider sliderZoom;

        public MainWindow()
        {
            InitializeComponent();

            WindowState = WindowState.Maximized;
            MinWidth = 1920;
            MinHeight = 1080;
            Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;

            ParaGrid = FindName("ParameterGrid") as Grid;
            Canvas = FindName("Canvas") as Canvas;

            parameters = new double[3];
            FractalChanged = true;
            fractal = "FractalTree";
            parameters[0] = 50;
            parameters[1] = 10;
            parameters[2] = 1.4;
            ZoomFactor = 2;
            Zoom = 1;

            upDownZoomFactor = new SimpleUpDownControl();
            upDownZoomFactor.tbText.Text = "Faktor přiblížení:";
            upDownZoomFactor.tbNumber.Text = ZoomFactor.ToString();
            upDownZoomFactor.Up.Click += ZoomFactor_Click;
            upDownZoomFactor.Down.Click += ZoomFactor_Click;

            sliderZoomFactor = new Slider();
            sliderZoomFactor.Value = ZoomFactor;
            sliderZoomFactor.Maximum = 4;
            sliderZoomFactor.Minimum = 1;
            sliderZoomFactor.ValueChanged += SliderZoomFactor_ValueChanged;

            upDownZoom = new SimpleUpDownControl();
            upDownZoom.tbText.Text = "Přiblížení:";
            upDownZoom.tbNumber.Text = Zoom.ToString() + "X";
            upDownZoom.Up.Click += UpDownZoom_Click;
            upDownZoom.Down.Click += UpDownZoom_Click;

            sliderZoom = new Slider();
            sliderZoom.Value = Zoom;
            sliderZoom.Maximum = 8;
            sliderZoom.Minimum = 1;
            sliderZoom.ValueChanged += SliderZoom_ValueChanged;

            canvasBackground = Brushes.Azure;

            ScrollViewer1.Content = Canvas;
            Canvas.Background = canvasBackground;
            Canvas.MouseUp += Canvas_MouseUp;
            Canvas.HorizontalAlignment = HorizontalAlignment.Stretch;
            Canvas.VerticalAlignment = VerticalAlignment.Stretch;
            Canvas.ClipToBounds = true;

            Canvas.Loaded += (s, e) =>
            {
                canvasWidth = Canvas.ActualWidth;
                canvasHeight = Canvas.ActualHeight;
                DrawSelectedFractal();
            };
        }

        private void SliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Zoom = e.NewValue;
            upDownZoom.tbNumber.Text = Zoom.ToString() + "X";

            if (fractal == "Mandelbrot") return;

            Canvas.Width = Zoom * ScrollViewer1.ActualWidth;
            Canvas.Height = Zoom * ScrollViewer1.ActualHeight;
            ScrollViewer1.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            ScrollViewer1.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            canvasWidth = Canvas.Width;
            canvasHeight = Canvas.Height;

            upDownZoom.tbNumber.Text = Zoom.ToString();
            sliderZoom.Value = Zoom;

            DrawSelectedFractal();
        }

        private void UpDownZoom_Click(object sender, RoutedEventArgs e)
        {
            bool upperPressed = ((Button)sender).Name == "Up";
            if (upperPressed && Zoom < sliderZoom.Maximum) Zoom++;
            else if (!upperPressed && Zoom > sliderZoom.Minimum) Zoom--;

            if (fractal == "Mandelbrot") return;

            Canvas.Width = Zoom * ScrollViewer1.ActualWidth;
            Canvas.Height = Zoom * ScrollViewer1.ActualHeight;
            ScrollViewer1.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            ScrollViewer1.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            canvasWidth = Canvas.Width;
            canvasHeight = Canvas.Height;

            upDownZoom.tbNumber.Text = Zoom.ToString();
            sliderZoom.Value = Zoom;

            DrawSelectedFractal();
        }

        private void SliderZoomFactor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ZoomFactor = e.NewValue;
            upDownZoomFactor.tbNumber.Text = ZoomFactor.ToString();
        }

        private void ZoomFactor_Click(object sender, RoutedEventArgs e)
        {
            bool upperPressed = ((Button)sender).Name == "Up";

            if (upperPressed && ZoomFactor < sliderZoomFactor.Maximum) ZoomFactor++;
            else if (!upperPressed && ZoomFactor > sliderZoomFactor.Minimum) ZoomFactor--;

            upDownZoomFactor.tbNumber.Text = ZoomFactor.ToString();
            sliderZoomFactor.Value = ZoomFactor;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!IsLoaded) return;
            Canvas.InvalidateMeasure();
            canvasWidth = Canvas.ActualWidth;
            canvasHeight = Canvas.ActualHeight;
            Canvas.Background = canvasBackground;
            DrawSelectedFractal();
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (fractal == "Mandelbrot") return;
            if (e.ChangedButton == MouseButton.Left)
            {
                Zoom *= ZoomFactor;

            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                Zoom /= ZoomFactor;
            }
            else return;

            if (Zoom > sliderZoom.Maximum)
            {
                Zoom = sliderZoom.Maximum;
            }
            else if (Zoom < 1) Zoom = 1;

            Canvas.Width = Zoom * ScrollViewer1.ActualWidth;
            Canvas.Height = Zoom * ScrollViewer1.ActualHeight;
            ScrollViewer1.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            ScrollViewer1.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            canvasWidth = Canvas.Width;
            canvasHeight = Canvas.Height;

            upDownZoomFactor.tbNumber.Text = ZoomFactor.ToString();
            upDownZoom.tbNumber.Text = Zoom.ToString();
            sliderZoomFactor.Value = ZoomFactor;
            sliderZoom.Value = Zoom;

            DrawSelectedFractal();
        }

        private void DrawSelectedFractal()
        {
            Canvas.Children.Clear();

            switch (fractal)
            {
                case "Mandelbrot":
                    MandelbrotSet mandelbrotSet = new MandelbrotSet(100);
                    break;

                case "Sierpinski":
                    SierpinskiTriangle sierpinskiTriangle = new SierpinskiTriangle((int)parameters[0]);
                    break;

                case "FractalTree":
                    FractalTree fractalTree = new FractalTree(parameters[0], (byte)parameters[1], parameters[2]);
                    break;

                case "DragonCurve":
                    DragonCurve dragonCurve = new DragonCurve((int)parameters[0]);
                    break;

                case "KochCurve":
                    KochCurve kochCurve = new KochCurve((int)parameters[0], parameters[1], parameters[2]);
                    break;
            }
            FractalChanged = false;
        }

        private void FractalChange(object sender, RoutedEventArgs e)
        {
            switch (((MenuItem)sender).Uid)
            {
                case "1": fractal = "Sierpinski";
                    parameters[0] = 7;
                    break;
                case "2": fractal = "Mandelbrot";
                    break;
                case "3": fractal = "FractalTree";
                    parameters[0] = 50;
                    parameters[1] = 10;
                    parameters[2] = 1.4;
                    break;
                case "4": fractal = "DragonCurve";
                    parameters[0] = 10;
                    break;
                case "5": fractal = "KochCurve";
                    parameters[0] = 5;
                    parameters[1] = 90;
                    parameters[2] = 1.0 / 3;
                    break;
            }

            ZoomFactor = 2;
            Zoom = 1;
            upDownZoomFactor.tbNumber.Text = ZoomFactor.ToString();
            sliderZoomFactor.Value = ZoomFactor;
            upDownZoom.tbNumber.Text = Zoom.ToString();
            sliderZoom.Value = Zoom;
            ParameterGrid.Children.Clear();
            FractalChanged = true;
            createNewCanvas();
        }

        private void createNewCanvas()
        {
            Canvas = new Canvas();
            Canvas.Width = ScrollViewer1.ActualWidth;
            Canvas.Height = ScrollViewer1.ActualHeight;
            ScrollViewer1.Content = Canvas;
            Canvas.Background = canvasBackground;
            Canvas.MouseUp += Canvas_MouseUp;
            Canvas.HorizontalAlignment = HorizontalAlignment.Stretch;
            Canvas.VerticalAlignment = VerticalAlignment.Stretch;
            Canvas.ClipToBounds = true;

            Canvas.Loaded += (s, e) =>
            {
                canvasWidth = Canvas.ActualWidth;
                canvasHeight = Canvas.ActualHeight;
                DrawSelectedFractal();
            };
        }

        private void MenuItem_Save_To_Image_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png";
            saveFileDialog.Title = "Save an Image File";
            saveFileDialog.FileName = fractal;

            if (saveFileDialog.ShowDialog() == true)
            {
                var bitmap = new RenderTargetBitmap((int)Canvas.ActualWidth, (int)Canvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(Canvas);

                var pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(bitmap));

                using (var stream = File.Create(saveFileDialog.FileName))
                {
                    pngEncoder.Save(stream);
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
