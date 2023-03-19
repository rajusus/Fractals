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
    internal class FractalTree
    {
        private readonly MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
        private readonly Canvas Canvas;
        private double Length;
        private double LengthChange;
        private double Angle;
        private byte Recursion;
        private double canvasWidth;
        private double canvasHeight;
        private readonly SimpleUpDownControl upDownRecursion;
        private readonly SimpleUpDownControl upDownAngle;
        private readonly SimpleUpDownControl upDownLengthChange;
        private Slider sliderRecursion;
        private Slider sliderAngle;
        private Slider sliderLengthChange;
        private PointF begginingPoint;

        public FractalTree(double angle, byte levels, double lengthChange)
        {
            Canvas = mainWin.Canvas;
            Angle = angle * Math.PI / 180;
            Recursion = levels;
            LengthChange = lengthChange;

            upDownRecursion = new SimpleUpDownControl();
            upDownAngle = new SimpleUpDownControl();
            upDownLengthChange = new SimpleUpDownControl();
            sliderRecursion = new Slider();
            sliderAngle = new Slider();
            sliderLengthChange = new Slider();

            draw();
            if (mainWin.FractalChanged) loadControls();
        }

        public void draw()
        {
            canvasWidth = mainWin.canvasWidth;
            canvasHeight = mainWin.canvasHeight;
            Length = canvasHeight * 0.30;
            begginingPoint = new PointF((float)canvasWidth / 2, (float)canvasHeight - (float)canvasHeight / 10);
            Canvas.Children.Clear();
            if (Recursion == 0) return;
            canvasWidth = mainWin.canvasWidth;
            canvasHeight = mainWin.canvasHeight;
            int neededSpaceForBranches = (int)Math.Pow(2, Recursion - 1) / 2;
            PointF[] startingPoint = new PointF[neededSpaceForBranches];
            PointF[] savedPoints = new PointF[neededSpaceForBranches];
            double[] startingAngle = new double[neededSpaceForBranches];
            double[] savedAnlges = new double[neededSpaceForBranches];

            if (Recursion == 1)
            {
                startingPoint = new PointF[1];
                savedPoints = new PointF[1];
                startingAngle = new double[1];
                savedAnlges = new double[1];
            }

            startingPoint[0] = begginingPoint;
            startingAngle[0] = 90 * Math.PI / 180 - Angle;
            bool save = true;

            drawLine(startingPoint[0], startingAngle[0], Length, Recursion, true, savedPoints, savedAnlges, 0, save);

            for (int i = 0; i < Recursion; i++)
            {
                if (i == Recursion - 1)
                    save = false;
                int numberOfBranches = (int)Math.Pow(2, i);
                double length = Length * Math.Pow(LengthChange, -i);
                int helper = 0;
                for (int j = 0; j < numberOfBranches / 2; j++)
                {
                    drawLine(startingPoint[j], startingAngle[j], length, Recursion - i, true, savedPoints, savedAnlges, helper, save);
                    helper++;
                    drawLine(startingPoint[j], startingAngle[j], length, Recursion - i, false, savedPoints, savedAnlges, helper, save);
                    helper++;
                }
                if (save)
                    for (int j = 0; j < numberOfBranches; j++)
                    {
                        startingPoint[j] = savedPoints[j];
                        startingAngle[j] = savedAnlges[j];
                    }
            }
        }

        private void drawLine(PointF point, double angle, double length, double thickness, bool right, PointF[] savePoints, double[] savedAngles, int rank, bool save)
        {
            double newAngle = angle - Angle;
            if (right) newAngle = angle + Angle;

            Line line = new Line()
            {
                X1 = point.X,
                Y1 = point.Y,
                X2 = point.X + Math.Cos(newAngle) * length,
                Y2 = point.Y - Math.Sin(newAngle) * length,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(Colors.Black)
            };
            Canvas.Children.Add(line);

            if (save)
            {
                savePoints[rank] = new PointF((float)line.X2, (float)line.Y2);
                savedAngles[rank] = newAngle;
            }
        }

        public void loadControls()
        {
            upDownRecursion.tbNumber.Text = Recursion.ToString();
            upDownRecursion.tbText.Text = "Rekurze:";
            upDownRecursion.Up.Click += Recursion_Click;
            upDownRecursion.Down.Click += Recursion_Click;

            upDownAngle.tbNumber.Text = (Angle / Math.PI * 180).ToString();
            upDownAngle.tbText.Text = "Úhel:";
            upDownAngle.Up.Click += Angle_Click;
            upDownAngle.Down.Click += Angle_Click;

            upDownLengthChange.tbNumber.Text = LengthChange.ToString();
            upDownLengthChange.tbText.Text = "Zmenšení dceřiné větvě";
            upDownLengthChange.Up.Click += LengthChange_Click;
            upDownLengthChange.Down.Click += LengthChange_Click;

            sliderRecursion = new()
            {
                Maximum = 10,
                Minimum = 0,
                Value = Recursion
            };
            sliderRecursion.ValueChanged += SliderRecursion_ValueChanged;

            sliderAngle = new()
            {
                Maximum = 360,
                Minimum = 0,
                Value = Angle / Math.PI * 180,
            };
            sliderAngle.ValueChanged += SliderAngle_ValueChanged;

            sliderLengthChange = new()
            {
                Minimum = 1,
                Maximum = 10,
                Value = LengthChange
            };
            sliderLengthChange.ValueChanged += SliderLengthChange_ValueChanged;

            Grid.SetRow(upDownRecursion, 0);
            Grid.SetRow(sliderRecursion, 1);
            Grid.SetRow(upDownAngle, 3);
            Grid.SetRow(sliderAngle, 4);
            Grid.SetRow(upDownLengthChange, 6);
            Grid.SetRow(sliderLengthChange, 7);
            Grid.SetRow(mainWin.upDownZoom, 9);
            Grid.SetRow(mainWin.sliderZoom, 10);
            Grid.SetRow(mainWin.upDownZoomFactor, 12);
            Grid.SetRow(mainWin.sliderZoomFactor, 13);

            mainWin.ParaGrid.Children.Add(upDownRecursion);
            mainWin.ParaGrid.Children.Add(sliderRecursion);
            mainWin.ParaGrid.Children.Add(upDownAngle);
            mainWin.ParaGrid.Children.Add(sliderAngle);
            mainWin.ParaGrid.Children.Add(upDownLengthChange);
            mainWin.ParaGrid.Children.Add(sliderLengthChange);
            mainWin.ParaGrid.Children.Add(mainWin.upDownZoom);
            mainWin.ParaGrid.Children.Add(mainWin.sliderZoom);
            mainWin.ParaGrid.Children.Add(mainWin.upDownZoomFactor);
            mainWin.ParaGrid.Children.Add(mainWin.sliderZoomFactor);
        }

        private void LengthChange_Click(object sender, RoutedEventArgs e)
        {
            bool upperPressed = ((Button)sender).Name == "Up";

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (upperPressed && LengthChange <= sliderLengthChange.Maximum - 100) LengthChange += 100;
                else if (!upperPressed && LengthChange >= sliderLengthChange.Minimum + 100) LengthChange -= 100;
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (upperPressed && LengthChange <= sliderLengthChange.Maximum - 10) LengthChange += 10;
                else if (!upperPressed && LengthChange >= sliderLengthChange.Minimum + 10) LengthChange -= 10;
            }
            else if (upperPressed && LengthChange < sliderLengthChange.Maximum) LengthChange++;
            else if (!upperPressed && LengthChange > sliderLengthChange.Minimum) LengthChange--;

            upDownLengthChange.tbNumber.Text = LengthChange.ToString();
            sliderLengthChange.Value = LengthChange;
            draw();
            mainWin.parameters[2] = LengthChange;
        }

        private void Angle_Click(object sender, RoutedEventArgs e)
        {
            bool upperPressed = ((Button)sender).Name == "Up";
            double degrees = Angle * 180 / Math.PI;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {   //* Math.PI / 180 - ze stupnu na radiany
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
            draw();
            mainWin.parameters[0] = degrees;
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
            draw();
            mainWin.parameters[1] = Recursion;
        }

        private void SliderLengthChange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LengthChange = e.NewValue;
            upDownLengthChange.tbNumber.Text = LengthChange.ToString();
            draw();
            mainWin.parameters[2] = LengthChange;
        }

        private void SliderAngle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double angle = e.NewValue;
            upDownAngle.tbNumber.Text = angle.ToString();
            Angle = angle * Math.PI / 180;
            draw();
            mainWin.parameters[0] = angle;
        }

        private void SliderRecursion_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Recursion = (byte)e.NewValue;
            upDownRecursion.tbNumber.Text = Recursion.ToString();
            draw();
            mainWin.parameters[1] = Recursion;
        }
    }
}
