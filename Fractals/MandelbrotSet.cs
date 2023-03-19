using Fractals;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fraktaly.Fractals
{
    internal class MandelbrotSet
    {
        private readonly MainWindow mainWin = (MainWindow)System.Windows.Application.Current.MainWindow;
        private readonly Canvas Canvas;
        private int Iterations;
        private double Scale;
        private WriteableBitmap Bitmap;
        private double CenterX;
        private double CenterY;
        private byte[] FractalColor = new byte[] { 0, 0, 0, 255 };
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly Label lblStopWatch = new Label();
        private bool ColourfulBackground = true;
        private Slider sliderIterations;
        private Slider sliderScale;
        private readonly ComplexUpDownControl upDownIterations = new();
        private readonly ComplexUpDownControl upDownScale = new();
        private ComboBox comboBoxFractalColor;
        private ComboBox comboBoxBackgroundColor;
        private Slider Rslider, Gslider, Bslider;
        private readonly System.Windows.Shapes.Rectangle rect;
        private readonly byte[] backgroundColors = new byte[3];
        private bool changingSliders = true;
        private readonly Image img = new Image();

        public MandelbrotSet(int iterations)
        {
            Iterations = iterations;

            Bitmap = new WriteableBitmap(10, 10, 96, 96, PixelFormats.Bgr32, null);
            Canvas = mainWin.Canvas;
            Scale = 400;
            CenterX = CenterY = 0;

            Canvas.MouseUp += Canvas_MouseUp;

            sliderIterations = new Slider();
            sliderScale = new Slider();
            upDownIterations = new ComplexUpDownControl();
            upDownScale = new ComplexUpDownControl();
            Rslider = new();
            Bslider = new();
            Gslider = new();
            comboBoxBackgroundColor = new();
            comboBoxFractalColor = new();

            upDownIterations.tbText.Text = "Iterace:";
            upDownIterations.tbNumber.Text = iterations.ToString();
            upDownIterations.Up.Click += Iterations_Click;
            upDownIterations.Down.Click += Iterations_Click;

            upDownScale.tbText.Text = "Přiblížení:";
            upDownScale.tbNumber.Text = Scale.ToString();
            upDownScale.Up.Click += Scale_Click;
            upDownScale.Down.Click += Scale_Click;

            lblStopWatch.FontSize = 16;
            lblStopWatch.HorizontalAlignment = HorizontalAlignment.Center;

            rect = new System.Windows.Shapes.Rectangle();
            rect.Width = 3;
            rect.Height = 3;
            rect.Stroke = new SolidColorBrush(Colors.Transparent);
            rect.Fill = new SolidColorBrush(Colors.Transparent);

            Canvas.SetLeft(rect, Canvas.ActualWidth / 2 - rect.Width / 2);
            Canvas.SetTop(rect, Canvas.ActualHeight / 2 - rect.Height / 2);
            Canvas.SetLeft(img, 0);
            Canvas.SetTop(img, 0);

            Canvas.Children.Add(img);
            Canvas.Children.Add(rect);

            backgroundColors[0] = 0;
            backgroundColors[1] = 0;
            backgroundColors[2] = 255;

            if (!ColourfulBackground) FractalColor = new byte[] { 255, 255, 255, 255 };

            draw();
            loadControls();
        }

        private void Iterations_Click(object sender, RoutedEventArgs e)
        {
            bool upperPressed = ((Button)sender).Name == "Up";

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (upperPressed && Iterations <= sliderIterations.Maximum - 100) Iterations += 100;
                else if(!upperPressed && Iterations >= sliderIterations.Minimum + 100) Iterations -= 100;
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (upperPressed && Iterations <= sliderIterations.Maximum - 10) Iterations += 10;
                else if (!upperPressed && Iterations >= sliderIterations.Minimum + 10) Iterations -= 10;
            }
            else if (upperPressed && Iterations < sliderIterations.Maximum) Iterations++;
            else if (!upperPressed && Iterations > sliderIterations.Minimum) Iterations--;

            upDownIterations.tbNewNumber.Text = Iterations.ToString();
            sliderIterations.Value = Iterations;
        }

        private void Scale_Click(object sender, RoutedEventArgs e)
        {
            bool upperPressed = ((Button)sender).Name == "Up";

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (upperPressed && Scale <= sliderScale.Maximum - 100) Scale += 100;
                else if (!upperPressed && Scale >= sliderScale.Minimum + 100) Scale -= 100;
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (upperPressed && Scale <= sliderScale.Maximum - 10) Scale += 10;
                else if (!upperPressed && Scale >= sliderScale.Minimum + 10) Scale -= 10;
            }
            else if (upperPressed && Scale < sliderScale.Maximum) Scale++;
            else if (!upperPressed && Scale > sliderScale.Minimum) Scale--;

            upDownScale.tbNewNumber.Text = Scale.ToString();
            sliderScale.Value = Scale;
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            double horizontalDifference = (e.GetPosition(Canvas).X - Bitmap.Width / 2) / Scale;
            double verticalDifference = -(e.GetPosition(Canvas).Y - Bitmap.Height / 2) / Scale;
            CenterX -= horizontalDifference;
            CenterY += verticalDifference;
            if (e.ChangedButton == MouseButton.Left)
            {
                if (mainWin.ZoomFactor != 0) Scale *= mainWin.ZoomFactor;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                if (mainWin.ZoomFactor != 0) Scale /= mainWin.ZoomFactor;
            }
            draw();

            upDownScale.tbNumber.Text = Scale.ToString();
            sliderScale.Value = Scale;
        }

        public void draw()
        {
            stopwatch.Restart();
            stopwatch.Start();
            Bitmap = new WriteableBitmap((int)mainWin.canvasWidth, (int)mainWin.canvasHeight, 96, 96, PixelFormats.Bgr32, null);

            for (int i = 0; i < Bitmap.Width; i++)
            {
                for (int j = 0; j < Bitmap.Height; j++)
                {
                    double coordinateX = i / Scale - (CenterX + (Bitmap.Width / 2) / Scale);
                    double coordinateY = -(j / Scale - (CenterY + (Bitmap.Height / 2) / Scale));

                    Complex c = new(coordinateX, coordinateY);
                    Complex[] results = new Complex[Iterations];
                    bool isInSet = true;

                    if (Iterations > 0) results[0] = 0;
                    for (int k = 0; k < Iterations - 1; k++)
                    {
                        results[k + 1] = results[k] * results[k] + c;
                        if (!Complex.IsFinite(results[k + 1]))
                        {
                            if (ColourfulBackground)
                            {
                                Bitmap.WritePixels(new Int32Rect(i, j, 1, 1), new byte[] { (byte)((double)(k + 1) / Iterations * backgroundColors[0]),
                                    (byte)((double)(k + 1) / Iterations * backgroundColors[1]),
                                    (byte)((double)(k + 1) / Iterations * backgroundColors[2]), 255 }, 10, 0);
                            }
                            isInSet = false;
                            break;
                        }
                    }
                    if (isInSet)
                        Bitmap.WritePixels(new Int32Rect(i, j, 1, 1), FractalColor, 10, 0);
                }
            }

            img.Source = Bitmap;
            stopwatch.Stop();
            lblStopWatch.Content = "Uběhlo: " + stopwatch.Elapsed.ToString() + " s";
        }

        public void loadControls()
        {
            CheckBox cbShowMiddle;
            CheckBox cbShowColors;
            Button btnDraw;
            Label lblBackgroundColor = new();
            Label lblFractalColor = new();
            StackPanel stackPanelForColorSliders = new StackPanel();
            Button btnFullScreen;
            Button btnReturnToStart;

            Rslider = new()
            {
                Minimum = 0,
                Maximum = 255,
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Stretch,
                Value = backgroundColors[0]
            };
            Rslider.ValueChanged += Rslider_ValueChanged;
            Gslider = new()
            {
                Minimum = 0,
                Maximum = 255,
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Stretch,
                Value = backgroundColors[1]
            };
            Gslider.ValueChanged += Gslider_ValueChanged;
            Bslider = new()
            {
                Minimum = 0,
                Maximum = 255,
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Stretch,
                Value = backgroundColors[2]
            };
            Bslider.ValueChanged += Bslider_ValueChanged;

            stackPanelForColorSliders.HorizontalAlignment = HorizontalAlignment.Stretch;
            stackPanelForColorSliders.VerticalAlignment = VerticalAlignment.Stretch;
            stackPanelForColorSliders.Orientation = Orientation.Horizontal;
            stackPanelForColorSliders.Children.Add(Rslider);
            stackPanelForColorSliders.Children.Add(Gslider);
            stackPanelForColorSliders.Children.Add(Bslider);
            stackPanelForColorSliders.HorizontalAlignment = HorizontalAlignment.Right;

            sliderIterations = new Slider()
            {
                Minimum = 0,
                Maximum = 1000,
                Value = Iterations,
                VerticalAlignment = VerticalAlignment.Center
            };
            sliderIterations.ValueChanged += SliderIterations_ValueChanged;

            sliderScale = new Slider()
            {
                Minimum = -1000000,
                Maximum = 1000000,
                Value = Scale,
                VerticalAlignment = VerticalAlignment.Center
            };
            sliderScale.ValueChanged += SliderScale_ValueChanged;

            cbShowMiddle = new CheckBox()
            {
                IsChecked = false,
                Content = "Ukázat střed",
                FontSize = 18,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            cbShowMiddle.Checked += CbShowMiddle_Checked;
            cbShowMiddle.Unchecked += CbShowMiddle_Unchecked;

            cbShowColors = new CheckBox();
            if (ColourfulBackground) cbShowColors.IsChecked = true;
            cbShowColors.Content = "Barevné pozadí";
            cbShowColors.FontSize = cbShowMiddle.FontSize;
            cbShowColors.Checked += CbShowColors_Checked;
            cbShowColors.Unchecked += CbShowColors_Unchecked;

            btnDraw = new Button()
            {
                Content = "Vygenerovat",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
            };
            btnDraw.Click += Drawit_Click;

            lblBackgroundColor.Content = "Barva pozadí:";
            lblBackgroundColor.FontSize = 18;
            lblBackgroundColor.VerticalAlignment = VerticalAlignment.Bottom;

            lblFractalColor.Content = "Barva fraktálu:";
            lblFractalColor.FontSize = lblBackgroundColor.FontSize;
            lblFractalColor.VerticalAlignment = VerticalAlignment.Bottom;

            comboBoxFractalColor = new ComboBox()
            {
                Items = { "Černá", "Bílá" },
                FontSize = 24,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            if (!ColourfulBackground)
            {
                comboBoxFractalColor.SelectedItem = "Bílá";
                comboBoxFractalColor.IsEnabled = false;
            }
            else comboBoxFractalColor.SelectedItem = "Černá";
            comboBoxFractalColor.SelectionChanged += ComboBoxFractalColor_SelectionChanged;

            comboBoxBackgroundColor = new ComboBox()
            {
                Items = { "Modrá", "Červená", "Zelená", "Žlutá", "Fialová", "Azurová", "Bílá", "Vlastní" },
                SelectedItem = "Červená",
                FontSize = 24,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            if (!ColourfulBackground) comboBoxBackgroundColor.IsEnabled = false;
            comboBoxBackgroundColor.SelectionChanged += ComboBoxBackgroundColor_SelectionChanged;

            btnFullScreen = new()
            {
                Content = "Uložit v rozlišení monitoru",
                FontSize = 26,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            btnFullScreen.Click += BtnFullScreen_Click;

            btnReturnToStart = new()
            {
                Content = "Vrátit na začátek",
                FontSize = 26,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            btnReturnToStart.Click += ReturnToStart_Click;

            Grid.SetRow(upDownIterations, 0);
            Grid.SetRow(sliderIterations, 1);
            Grid.SetRow(upDownScale, 2);
            Grid.SetRow(sliderScale, 3);
            Grid.SetRow(mainWin.upDownZoomFactor, 4);
            Grid.SetRow(mainWin.sliderZoomFactor, 5);
            Grid.SetRow(btnDraw, 6);
            Grid.SetRow(cbShowMiddle, 7);
            Grid.SetRow(cbShowColors, 8);
            Grid.SetRow(lblBackgroundColor, 9);
            Grid.SetRow(comboBoxBackgroundColor, 10);
            Grid.SetRow(stackPanelForColorSliders, 11);
            Grid.SetRow(lblFractalColor, 12);
            Grid.SetRow(comboBoxFractalColor, 13);
            Grid.SetRow(lblStopWatch, 14);
            Grid.SetRow(btnReturnToStart, 15);
            Grid.SetRow(btnFullScreen, 16);

            mainWin.ParaGrid.Children.Add(upDownIterations);
            mainWin.ParaGrid.Children.Add(sliderIterations);
            mainWin.ParaGrid.Children.Add(upDownScale);
            mainWin.ParaGrid.Children.Add(sliderScale);
            mainWin.ParaGrid.Children.Add(mainWin.upDownZoomFactor);
            mainWin.ParaGrid.Children.Add(mainWin.sliderZoomFactor);
            mainWin.ParaGrid.Children.Add(cbShowMiddle);
            mainWin.ParaGrid.Children.Add(cbShowColors);
            mainWin.ParaGrid.Children.Add(lblBackgroundColor);
            mainWin.ParaGrid.Children.Add(comboBoxBackgroundColor);
            mainWin.ParaGrid.Children.Add(stackPanelForColorSliders);
            mainWin.ParaGrid.Children.Add(lblFractalColor);
            mainWin.ParaGrid.Children.Add(comboBoxFractalColor);
            mainWin.ParaGrid.Children.Add(btnDraw);
            mainWin.ParaGrid.Children.Add(lblStopWatch);
            mainWin.ParaGrid.Children.Add(btnFullScreen);
            mainWin.ParaGrid.Children.Add(btnReturnToStart);
        }

        private void ReturnToStart_Click(object sender, RoutedEventArgs e)
        {
            CenterX = 0;
            CenterY = 0;
            Scale = 400;

            draw();
        }

        private void BtnFullScreen_Click(object sender, RoutedEventArgs e)
        {
            WriteableBitmap superBitmap = new WriteableBitmap((int)SystemParameters.WorkArea.Width, (int)SystemParameters.WorkArea.Height, 96, 96, PixelFormats.Bgr32, null);
            for (int i = 0; i < superBitmap.Width; i++)
            {
                for (int j = 0; j < superBitmap.Height; j++)
                {
                    double coordinateX = i / Scale - (CenterX + (superBitmap.Width / 2) / Scale);
                    double coordinateY = -(j / Scale - (CenterY + (superBitmap.Height / 2) / Scale));

                    Complex c = new(coordinateX, coordinateY);
                    Complex[] results = new Complex[Iterations];
                    bool isInSet = true;

                    if (Iterations > 0) results[0] = 0;
                    for (int k = 0; k < Iterations - 1; k++)
                    {
                        results[k + 1] = results[k] * results[k] + c;
                        if (!Complex.IsFinite(results[k + 1]))
                        {
                            if (ColourfulBackground)
                            {
                                superBitmap.WritePixels(new Int32Rect(i, j, 1, 1), new byte[] { (byte)((double)(k + 1) / Iterations * backgroundColors[0]),
                                    (byte)((double)(k + 1) / Iterations * backgroundColors[1]),
                                    (byte)((double)(k + 1) / Iterations * backgroundColors[2]), 255 }, 10, 0);
                            }
                            isInSet = false;
                            break;
                        }
                    }
                    if (isInSet)
                        superBitmap.WritePixels(new Int32Rect(i, j, 1, 1), FractalColor, 10, 0);
                }
            }
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "PNG Imgae|*.png";
            saveFileDialog.Title = "Save an Image File";
            saveFileDialog.FileName = mainWin.fractal;

            if (saveFileDialog.ShowDialog() == true)
            {
                var pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(superBitmap));

                using (var stream = File.Create(saveFileDialog.FileName))
                {
                    pngEncoder.Save(stream);
                }
            }
        }

        private void Bslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            backgroundColors[2] = (byte)e.NewValue;
            if (changingSliders) comboBoxBackgroundColor.SelectedItem = "Vlastní";
        }

        private void Gslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            backgroundColors[1] = (byte)e.NewValue;
            if (changingSliders) comboBoxBackgroundColor.SelectedItem = "Vlastní";
        }

        private void Rslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            backgroundColors[0] = (byte)e.NewValue;
            if (changingSliders) comboBoxBackgroundColor.SelectedItem = "Vlastní";

        }


        private void SliderScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Scale = e.NewValue;
            upDownScale.tbNewNumber.Text = Scale.ToString();
        }

        private void ComboBoxFractalColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((ComboBox)sender).SelectedItem)
            {
                case "Černá": FractalColor = new byte[] { 0, 0, 0, 255 }; break;
                case "Bílá": FractalColor = new byte[] { 255, 255, 255, 255 }; break;
            }
        }

        private void ComboBoxBackgroundColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (((ComboBox)sender).SelectedItem)
            {
                case "Modrá":
                    backgroundColors[0] = 255;
                    backgroundColors[1] = backgroundColors[2] = 0;
                    break;
                case "Červená":
                    backgroundColors[2] = 255;
                    backgroundColors[0] = backgroundColors[1] = 0;
                    break;
                case "Zelená":
                    backgroundColors[1] = 255;
                    backgroundColors[0] = backgroundColors[1] = 0;
                    break;
                case "Azurová":
                    backgroundColors[0] = backgroundColors[1] = 255;
                    backgroundColors[2] = 0;
                    break;
                case "Fialová":
                    backgroundColors[0] = backgroundColors[2] = 255;
                    backgroundColors[1] = 0;
                    break;
                case "Žlutá":
                    backgroundColors[0] = 0;
                    backgroundColors[1] = backgroundColors[2] = 255;
                    break;
                case "Bílá":
                    backgroundColors[0] = backgroundColors[1] = backgroundColors[2] = 255;
                    break;
            }

            changingSliders = false;
            Rslider.Value = backgroundColors[0];
            Gslider.Value = backgroundColors[1];
            Bslider.Value = backgroundColors[2];
            changingSliders = true;
        }

        private void CbShowMiddle_Unchecked(object sender, RoutedEventArgs e)
        {
            rect.Stroke = new SolidColorBrush(Colors.Transparent);
            rect.Fill = new SolidColorBrush(Colors.Transparent);
        }

        private void CbShowColors_Unchecked(object sender, RoutedEventArgs e)
        {
            ColourfulBackground = false;
            FractalColor = new byte[] { 255, 255, 255, 255 };
            comboBoxFractalColor.IsEnabled = comboBoxBackgroundColor.IsEnabled = false;
        }

        private void CbShowColors_Checked(object sender, RoutedEventArgs e)
        {
            ColourfulBackground = true;
            comboBoxFractalColor.IsEnabled = comboBoxBackgroundColor.IsEnabled = true;
            FractalColor = new byte[] { 0, 0, 0, 255 };
            comboBoxFractalColor.SelectedItem = "Černá";
        }

        private void CbShowMiddle_Checked(object sender, RoutedEventArgs e)
        {
            rect.Stroke = new SolidColorBrush(Colors.Red);
            rect.Fill = new SolidColorBrush(Colors.Red);
        }

        private void SliderIterations_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Iterations = (int)e.NewValue;
            upDownIterations.tbNewNumber.Text = Iterations.ToString();
        }

        private void Drawit_Click(object sender, RoutedEventArgs e)
        {
            if (Scale <= 0) Scale = 1;
            upDownScale.tbNumber.Text = Scale.ToString();
            upDownIterations.tbNumber.Text = Iterations.ToString();
            upDownIterations.tbNewNumber.Text = "";
            upDownScale.tbNewNumber.Text = "";
            draw();
        }
    }
}
