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
    internal class KochCurve
    {
        private readonly Canvas Canvas;
        private readonly MainWindow mainWin;
        private int Recursion;
        private readonly Brush Background;
        private double canvasWidth;
        private double canvasHeight;
        private double Length;
        private double Distance;
        private double Angle;
        Slider sliderRecursion;
        Slider sliderAngle;
        Slider sliderDistance;
        readonly SimpleUpDownControl upDownRecursion;
        readonly SimpleUpDownControl upDownAngle;
        readonly SimpleUpDownControl upDownDistance;

        public KochCurve(int recursion, double angle, double distance)
        {
            mainWin = (MainWindow)Application.Current.MainWindow;

            Recursion = recursion;
            Angle = angle * Math.PI / 180;
            Distance = distance;


            Canvas = mainWin.Canvas;
            Background = mainWin.Background;

            sliderRecursion = new Slider();
            sliderAngle = new();
            sliderDistance = new();
            upDownRecursion = new SimpleUpDownControl();
            upDownAngle = new();
            upDownDistance = new();
            sliderDistance.Value = Distance;

            masterDraw();
            if (mainWin.FractalChanged) loadElements();
        }


        public void masterDraw()
        {
            Canvas.Children.Clear();
            canvasWidth = mainWin.canvasWidth;
            canvasHeight = mainWin.canvasHeight;
            Length = canvasWidth / 4 * 3;

            PointF start = new PointF((float)canvasWidth / 2 - (float)Length / 2, (float)(canvasHeight / 6 * 5));
            PointF end = new PointF(start.X + (float)Length, start.Y);

            draw(start, end, Recursion);
        }

        private void draw(PointF start, PointF end, int recursion)
        {
            if (recursion == 0)
            {
                Line line = new Line
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    X1 = start.X,
                    Y1 = start.Y,
                    X2 = end.X,
                    Y2 = end.Y
                };
                Canvas.Children.Add(line);
                return;
            }

            PointF A = start;
            PointF B = getPointBD(start, end, Distance);
            PointF D = getPointBD(start, end, Distance * 2);
            PointF C = getPointC(B, D);
            PointF E = end;

            draw(A, B, recursion - 1);
            draw(B, C, recursion - 1);
            draw(C, D, recursion - 1);
            draw(D, E, recursion - 1);

        }

        private PointF getPointBD(PointF start, PointF end, double distance)
        {
            float x = start.X + (float)distance * (end.X - start.X);
            float y = start.Y + (float)distance * (end.Y - start.Y);

            return new PointF(x, y);
        }

        private PointF getPointC(PointF B, PointF D)
        {
            double length = Math.Sqrt(Math.Pow(calculateDistance(B, D), 2) - Math.Pow(calculateDistance(B, D) / 2, 2));
            float centerX = (B.X + D.X) / 2;
            float centerY = (B.Y + D.Y) / 2;

            double angle = Math.Atan2(D.Y - B.Y, D.X - B.X);

            double adjacent = Math.Cos(Angle + angle) * length;
            double opposite = Math.Sin(Angle + angle) * length;

            float x = centerX - (float)adjacent;
            float y = centerY - (float)opposite;

            return new PointF(x, y);
        }

        private double calculateDistance(PointF start, PointF end)
        {
            double xDifference = Math.Abs(start.X - end.X);
            double yDifference = Math.Abs(start.Y - end.Y);
            return Math.Sqrt(xDifference * xDifference + yDifference * yDifference);
        }

        private void loadElements()
        {
            sliderRecursion = new()
            {
                Minimum = 0,
                Maximum = 7,
                Value = Recursion
            };
            sliderRecursion.ValueChanged += SliderRecursion_ValueChanged;

            sliderAngle = new()
            {
                Minimum = 0,
                Maximum = 360,
                Value = Angle * 180 / Math.PI
            };
            sliderAngle.ValueChanged += SliderAngle_ValueChanged;

            sliderDistance = new()
            {
                Minimum = 0,
                Maximum = 1,
                Value = Distance
            };
            sliderDistance.ValueChanged += SliderDistance_ValueChanged;

            upDownRecursion.tbNumber.Text = Recursion.ToString();
            upDownRecursion.tbText.Text = "Rekurze:";
            upDownRecursion.Up.Click += Recursion_Click;
            upDownRecursion.Down.Click += Recursion_Click;

            upDownAngle.tbNumber.Text = (Angle * 180 / Math.PI).ToString();
            upDownAngle.tbText.Text = "Úhel:";
            upDownAngle.Up.Click += Angle_Click;
            upDownAngle.Down.Click += Angle_Click;

            upDownDistance.tbNumber.Text = Distance.ToString();
            upDownDistance.tbText.Text = "Výška:";
            upDownDistance.Up.Click += Distance_Click;
            upDownDistance.Down.Click += Distance_Click;

            Grid.SetRow(upDownRecursion, 0);
            Grid.SetRow(sliderRecursion, 1);
            Grid.SetRow(upDownAngle, 3);
            Grid.SetRow(sliderAngle, 4);
            Grid.SetRow(upDownDistance, 6);
            Grid.SetRow(sliderDistance, 7);
            Grid.SetRow(mainWin.upDownZoom, 9);
            Grid.SetRow(mainWin.sliderZoom, 10);
            Grid.SetRow(mainWin.upDownZoomFactor, 12);
            Grid.SetRow(mainWin.sliderZoomFactor, 13);

            mainWin.ParaGrid.Children.Add(upDownRecursion);
            mainWin.ParaGrid.Children.Add(sliderRecursion);
            mainWin.ParaGrid.Children.Add(upDownAngle);
            mainWin.ParaGrid.Children.Add(sliderAngle);
            mainWin.ParaGrid.Children.Add(upDownDistance);
            mainWin.ParaGrid.Children.Add(sliderDistance);
            mainWin.ParaGrid.Children.Add(mainWin.upDownZoom);
            mainWin.ParaGrid.Children.Add(mainWin.sliderZoom);
            mainWin.ParaGrid.Children.Add(mainWin.upDownZoomFactor);
            mainWin.ParaGrid.Children.Add(mainWin.sliderZoomFactor);
        }

        private void Distance_Click(object sender, RoutedEventArgs e)
        {
            bool upperPressed = ((Button)sender).Name == "Up";

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (upperPressed && Distance <= sliderDistance.Maximum - 100) Distance += 100;
                else if (!upperPressed && Distance >= sliderDistance.Minimum + 100) Distance -= 100;
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (upperPressed && Distance <= sliderDistance.Maximum - 10) Distance += 10;
                else if (!upperPressed && Distance >= sliderDistance.Minimum + 10) Distance -= 10;
            }
            else if (upperPressed && Distance < sliderDistance.Maximum) Distance++;
            else if (!upperPressed && Distance > sliderDistance.Minimum) Distance--;

            upDownDistance.tbNumber.Text = Distance.ToString();
            sliderDistance.Value = Distance;
            masterDraw();
            mainWin.parameters[2] = Distance;
        }

        private void Angle_Click(object sender, RoutedEventArgs e)
        {
            double degrees = Angle / Math.PI * 180;
            bool upperPressed = ((Button)sender).Name == "Up";

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (upperPressed && degrees <= sliderAngle.Maximum - 100) degrees += 100;
                else if (!upperPressed && degrees >= sliderAngle.Minimum + 100) degrees -= 100;
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (upperPressed && degrees <= sliderAngle.Maximum - 10) degrees += 10;
                else if (!upperPressed && degrees >= sliderAngle.Minimum + 10) degrees -= 10;
            }
            else if (upperPressed && degrees < sliderAngle.Maximum) degrees++;
            else if (!upperPressed && degrees > sliderAngle.Minimum) degrees--;

            upDownAngle.tbNumber.Text = degrees.ToString();
            sliderAngle.Value = degrees;
            Angle = degrees * Math.PI / 180;
            masterDraw();
            mainWin.parameters[1] = degrees;
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

        private void SliderRecursion_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Recursion = (int)e.NewValue;
            upDownRecursion.tbNumber.Text = Recursion.ToString();
            masterDraw();
            mainWin.parameters[0] = Recursion;
        }

        private void SliderDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Distance = sliderDistance.Value;
            upDownDistance.tbNumber.Text = Distance.ToString();
            masterDraw();
            mainWin.parameters[2] = Distance;
        }

        private void SliderAngle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double angle = e.NewValue;
            upDownAngle.tbNumber.Text = angle.ToString();
            Angle = angle * Math.PI / 180;
            masterDraw();
            mainWin.parameters[1] = angle;
        }
    }
}
