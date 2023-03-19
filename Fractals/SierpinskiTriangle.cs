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
    internal class SierpinskiTriangle
    {
        private readonly Canvas Canvas;
        private int Recursion;
        private double canvasWidth;
        private double canvasHeight;
        private readonly MainWindow mainWin;
        private double Length;
        private PointF startingPoint;
        private Brush Background;
        private readonly SimpleUpDownControl upDownRecursion;
        readonly Slider sliderRecursion;

        public SierpinskiTriangle(int recursion)
        {
            mainWin = (MainWindow)Application.Current.MainWindow;
            Canvas = mainWin.Canvas;
            Recursion = recursion;
            Background = mainWin.canvasBackground;
            
            upDownRecursion = new();
            sliderRecursion = new Slider();

            draw();
            if (mainWin.FractalChanged) loadControls();
        }

        public void draw()
        {
            Canvas.Children.Clear();
            canvasWidth = mainWin.canvasWidth;
            canvasHeight = mainWin.canvasHeight;
            Length = canvasWidth / 4 * 3;
            startingPoint = new PointF((int)canvasWidth / 2 - (int)Length / 2,
                (int)(Math.Sqrt(3 * Math.Pow(Length, 2) / 4) + (canvasHeight - Math.Sqrt(3 * Math.Pow(Length, 2) / 4)) / 2));
            double triangleHeight = Math.Sqrt(3 * Math.Pow(Length, 2) / 4);

            PointF[] trianglePoints = new PointF[3];
            trianglePoints[0] = startingPoint;
            trianglePoints[1] = new PointF(trianglePoints[0].X + (float)Length, trianglePoints[0].Y);
            trianglePoints[2] = new PointF(trianglePoints[0].X + (float)Length / 2, trianglePoints[0].Y - (float)triangleHeight);

            drawFirstTriangle(trianglePoints);
            if (Recursion == 0) 
                return;
            drawRecursion(triangleHeight, trianglePoints[0]);
        }

        private void drawRecursion(double baseHeight, PointF basePoint)
        {
            double newHeight = baseHeight;
            double newSideLength = Length;
            int maxNumberOfTriangles = (int)Math.Pow(3, Recursion - 1);
            int[] whichWay = new int[maxNumberOfTriangles];
            PointF[] points = new PointF[maxNumberOfTriangles];
            points[0] = new PointF(basePoint.X + (float)Length / 2, basePoint.Y);
            whichWay[0] = 1;

            for (int i = 0; i < Recursion; i++)
            {
                newSideLength /= 2;
                newHeight /= 2;
                int helper = 0;

                PointF[] savePoint = new PointF[1];
                if (i != Recursion - 1) savePoint = new PointF[(int)Math.Pow(3, i) * 3];

                for (int j = 0; j < (int)Math.Pow(3, i); j++)
                {
                    PointF[] drawPoints = new PointF[3];
                    switch (whichWay[j])
                    {
                        case 1:
                            drawPoints[0] = points[j];
                            drawPoints[1] = new PointF(drawPoints[0].X + (float)newSideLength / 2, drawPoints[0].Y - (float)newHeight);
                            drawPoints[2] = new PointF(drawPoints[1].X - (float)newSideLength, drawPoints[1].Y);
                            break;

                        case 2:
                            drawPoints[0] = new PointF(points[j].X + (float)newSideLength / 2, points[j].Y + (float)newHeight);
                            drawPoints[1] = new PointF(drawPoints[0].X + (float)newSideLength / 2, drawPoints[0].Y - (float)newHeight);
                            drawPoints[2] = new PointF(drawPoints[1].X - (float)newSideLength, drawPoints[1].Y);
                            break;

                        case 3:
                            drawPoints[0] = new PointF(points[j].X - (float)newSideLength / 2, points[j].Y + (float)newHeight);
                            drawPoints[1] = new PointF(drawPoints[0].X + (float)newSideLength / 2, drawPoints[0].Y - (float)newHeight);
                            drawPoints[2] = new PointF(drawPoints[1].X - (float)newSideLength, drawPoints[1].Y);
                            break;
                    }

                    Polygon smallPolygon = new Polygon();
                    smallPolygon.Points = new PointCollection() { new System.Windows.Point(drawPoints[0].X, drawPoints[0].Y),
                        new System.Windows.Point(drawPoints[1].X, drawPoints[1].Y), new System.Windows.Point(drawPoints[2].X, drawPoints[2].Y)};
                    smallPolygon.Fill = Background;
                    smallPolygon.StrokeThickness = 1;
                    Canvas.Children.Add(smallPolygon);

                    if (i != Recursion - 1)
                    {
                        savePoint[helper] = new PointF(drawPoints[2].X + (float)newSideLength / 2, drawPoints[2].Y);
                        whichWay[helper] = 1;
                        helper++;

                        savePoint[helper] = new PointF(drawPoints[0].X + (float)newSideLength / 4, drawPoints[0].Y - (float)newHeight / 2);
                        whichWay[helper] = 2;
                        helper++;

                        savePoint[helper] = new PointF(drawPoints[0].X - (float)newSideLength / 4, drawPoints[0].Y - (float)newHeight / 2);
                        whichWay[helper] = 3;
                        helper++;
                    }
                }
                if (i != Recursion - 1)
                {
                    for (int j = 0; j < savePoint.Length; j++)
                    {
                        points[j] = savePoint[j];
                    }
                }
            }
        }

        private void drawFirstTriangle(PointF[] trio)
        {
            Polygon triangle = new Polygon();
            triangle.Points = new PointCollection() { new System.Windows.Point(trio[0].X, trio[0].Y),
            new System.Windows.Point(trio[1].X, trio[1].Y), new System.Windows.Point(trio[2].X, trio[2].Y)};
            triangle.Stroke = Brushes.Black;
            triangle.Fill = Brushes.Black;
            triangle.StrokeThickness = 1;
            Canvas.Children.Add(triangle);
        }

        public void loadControls()
        {
            sliderRecursion.Minimum = 0;
            sliderRecursion.Maximum = 8;
            sliderRecursion.Value = Recursion;
            sliderRecursion.ValueChanged += SliderLevels_ValueChanged;

            upDownRecursion.tbNumber.Text = Recursion.ToString();
            upDownRecursion.tbText.Text = "Rekurze:";
            upDownRecursion.Up.Click += Recursion_Click;
            upDownRecursion.Down.Click += Recursion_Click;

            TextBlock colorText = new TextBlock()
            {
                FontSize = 18,
                Text = "Barva:",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
            };

            ComboBox colorCombo = new ComboBox()
            {
                AllowDrop = true,
                Width = 200,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Left,
                SelectedValue = "Azurová"
            };
            colorCombo.Items.Add("Červená");
            colorCombo.Items.Add("Bílá");
            colorCombo.Items.Add("Žlutá");
            colorCombo.Items.Add("Azurová");
            colorCombo.SelectionChanged += ColorCombo_SelectionChanged;

            Grid.SetRow(upDownRecursion, 0);
            Grid.SetRow(sliderRecursion, 1);
            Grid.SetRow(colorText, 3);
            Grid.SetRow(colorCombo, 4);
            Grid.SetRow(mainWin.upDownZoom, 6);
            Grid.SetRow(mainWin.sliderZoom, 7);
            Grid.SetRow(mainWin.upDownZoomFactor, 9);
            Grid.SetRow(mainWin.sliderZoomFactor, 10);

            mainWin.ParaGrid.Children.Add(upDownRecursion);
            mainWin.ParaGrid.Children.Add(sliderRecursion);
            mainWin.ParaGrid.Children.Add(colorText);
            mainWin.ParaGrid.Children.Add(colorCombo);
            mainWin.ParaGrid.Children.Add(mainWin.upDownZoom);
            mainWin.ParaGrid.Children.Add(mainWin.sliderZoom);
            mainWin.ParaGrid.Children.Add(mainWin.upDownZoomFactor);
            mainWin.ParaGrid.Children.Add(mainWin.sliderZoomFactor);
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
            mainWin.parameters[0] = Recursion;
        }

        private void ColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            switch (cmb.SelectedItem)
            {
                case "Červená": Background = Brushes.Red; break;
                case "Bílá": Background = Brushes.White; break;
                case "Žlutá": Background = Brushes.Yellow; break;
                case "Azurová": Background = Brushes.Azure; break;
            }

            Canvas.Background = Background;
            draw();
        }

        private void SliderLevels_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Recursion = (int)e.NewValue;
            upDownRecursion.tbNumber.Text = Recursion.ToString();
            draw();
            mainWin.parameters[0] = Recursion;
        }
    }
}
