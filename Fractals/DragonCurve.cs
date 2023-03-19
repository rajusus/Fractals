using Fractals;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Fraktaly.Fractals
{
    internal class DragonCurve
    {
        private readonly Canvas Canvas;
        private readonly MainWindow mainWin;
        private int Recursion;
        private double Length;
        private PointF BeginningPoint;
        private readonly bool Right;
        private readonly Brush Color;
        private readonly SimpleUpDownControl upDownRecursion;
        private Slider sliderRecursion;
        private double canvasWidth;
        private double canvasHeight;

        public DragonCurve(int recursion)
        {
            mainWin = (MainWindow)Application.Current.MainWindow;
            Canvas = mainWin.Canvas;
            Recursion = recursion;

            Color = Brushes.Black;
            Right = true;

            BeginningPoint = new PointF();

            upDownRecursion = new SimpleUpDownControl();
            sliderRecursion = new Slider();

            masterDraw();
            if (mainWin.FractalChanged) loadControls();
        }

        public void masterDraw()
        {
            canvasWidth = mainWin.canvasWidth;
            canvasHeight = mainWin.canvasHeight;
            Length = canvasWidth / 3;
            BeginningPoint = new PointF((float)(canvasWidth / 2 - Length / 2), (float)(canvasHeight / 2 - Length / 2));
            Canvas.Children.Clear();
            DrawRecursivly(Recursion, BeginningPoint.X, BeginningPoint.Y, Length, Length, Right);
        }

        public void DrawRecursivly(int level, double start, double end, double horizonatalDistance, double verticalDistance, bool right)
        {
            if (level == 0)
            {
                DrawLine(start, end, start + horizonatalDistance, end + verticalDistance);
                return;
            }

            double horizontalHalf = horizonatalDistance / 2;
            double verticalHalf = verticalDistance / 2;

            double nextX = horizontalHalf - verticalHalf;
            double nextY = horizontalHalf + verticalHalf;

            if (right)
            {
                DrawRecursivly(level - 1, start, end, nextX, nextY, true);
                double x2 = start + nextX;
                double y2 = end + nextY;
                DrawRecursivly(level - 1, x2, y2, nextY, -nextX, false);
            }
            else
            {
                DrawRecursivly(level - 1, start, end, nextY, -nextX, true);
                double x2 = start + nextY;
                double y2 = end - nextX;
                DrawRecursivly(level - 1, x2, y2, nextX, nextY, false);
            }
        }

        private void DrawLine(double x1, double y1, double x2, double y2)
        {
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = Color,
                StrokeThickness = 1
            };
            Canvas.Children.Add(line);
        }

        private void loadControls()
        {
            Button btnGenerate;

            upDownRecursion.tbNumber.Text = Recursion.ToString();
            upDownRecursion.tbText.Text = "Rekurze:";
            upDownRecursion.Up.Click += Recursion_Click;
            upDownRecursion.Down.Click += Recursion_Click;

            sliderRecursion = new()
            {
                Minimum = 0,
                Maximum = 16,
                Value = Recursion,
            };
            sliderRecursion.ValueChanged += SliderRecursion_ValueChanged;

            btnGenerate = new();
            btnGenerate.Content = "Vykreslit";
            btnGenerate.FontSize = 22;
            btnGenerate.Click += BtnGenerate_Click;

            Grid.SetRow(upDownRecursion, 0);
            Grid.SetRow(sliderRecursion, 1);
            Grid.SetRow(mainWin.upDownZoom, 3);
            Grid.SetRow(mainWin.sliderZoom, 4);
            Grid.SetRow(mainWin.upDownZoomFactor, 6);
            Grid.SetRow(mainWin.sliderZoomFactor, 7);
            Grid.SetRow(btnGenerate, 9);

            mainWin.ParaGrid.Children.Add(upDownRecursion);
            mainWin.ParaGrid.Children.Add(sliderRecursion);
            mainWin.ParaGrid.Children.Add(mainWin.upDownZoom);
            mainWin.ParaGrid.Children.Add(mainWin.sliderZoom);
            mainWin.ParaGrid.Children.Add(mainWin.upDownZoomFactor);
            mainWin.ParaGrid.Children.Add(mainWin.sliderZoomFactor);
            mainWin.ParaGrid.Children.Add(btnGenerate);
        }

        private void Recursion_Click(object sender, RoutedEventArgs e)
        {
            bool upperPressed = ((Button)sender).Name == "Up";

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (upperPressed && Recursion <= sliderRecursion.Maximum - 100) Recursion += 100;
                else if (!upperPressed && Recursion >= sliderRecursion.Minimum + 100) Recursion -= 100;
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (upperPressed && Recursion <= sliderRecursion.Maximum - 10) Recursion += 10;
                else if (!upperPressed && Recursion >= sliderRecursion.Minimum + 10) Recursion -= 10;
            }
            else if (upperPressed && Recursion < sliderRecursion.Maximum) Recursion++;
            else if (!upperPressed && Recursion > sliderRecursion.Minimum) Recursion--;

            upDownRecursion.tbNumber.Text = Recursion.ToString();
            sliderRecursion.Value = Recursion;
            masterDraw();
            mainWin.parameters[0] = Recursion;
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            masterDraw();
        }

        private void SliderRecursion_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Recursion = (int)e.NewValue;
            upDownRecursion.tbNumber.Text = Recursion.ToString();
            mainWin.parameters[0] = Recursion;
        }
    }
}
