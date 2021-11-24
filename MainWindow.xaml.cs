using BuildingPlanCalc.Interfaces;
using BuildingPlanCalc.Models;
using BuildingPlanCalc.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BuildingPlanCalc
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            InitializeComponent();
            if (!CheckAppLicense())
            {
                MessageBox.Show("Программа установлена не верно. Требуется переустановка", "Ошибка запуска приложения", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
            }
            Shapes = new List<IShape>();
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo,
                (sender, e) => { DeleteLastShape(); },
                (sender, e) => { e.CanExecute = true; }));

            InputBindings.Add(new KeyBinding(ApplicationCommands.Undo,
                new KeyGesture(Key.Z, ModifierKeys.Control)));


            CommandBindings.Add(new CommandBinding(ApplicationCommands.New,
                (sender, e) => { ClearAllProject(); },
                (sender, e) => { e.CanExecute = true; }));

            InputBindings.Add(new KeyBinding(ApplicationCommands.New,
                new KeyGesture(Key.N, ModifierKeys.Control)));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete,
                (sender, e) => { ClearSelectedLayout(); },
                (sender, e) => { e.CanExecute = true; }));

            InputBindings.Add(new KeyBinding(ApplicationCommands.Delete,
                new KeyGesture(Key.Q, ModifierKeys.Control)));
        }

        private Point point_s;
        private Point point_f;
        private bool isPainting = false;
        private bool isControlLine = false;
        private double coeffLength = 0.1;
        private double controlLineLength;
        private double realLength;
        private byte TrianglePointCount = 0;
        private Point firstTrianglePoint;
        private readonly List<IShape> Shapes;
        private Brush shapeColor;
        private Canvas selectedCanvas;

        // Переменные для выравнивания линии по осям
        private bool axisAligment;

        byte ShapeType { get; set; } = 1;
        byte tempShapeType;
        byte SelectedBuidingObj = 0;


        #region Управление окном
        private void AppClose(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            window.WindowState = WindowState.Minimized;
        }

        private bool mRestoreIfMove = false;

        private void SwitchWindowState()
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    {
                        WindowState = WindowState.Maximized;
                        TControl.Height = window.ActualHeight - 280;
                        break;
                    }
                case WindowState.Maximized:
                    {
                        WindowState = WindowState.Normal;
                        TControl.Height = window.ActualHeight - 275;
                        break;
                    }
            }
        }


        private void Header_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if ((ResizeMode == ResizeMode.CanResize) || (ResizeMode == ResizeMode.CanResizeWithGrip))
                {
                    SwitchWindowState();
                }

                return;
            }

            else if (WindowState == WindowState.Maximized)
            {
                mRestoreIfMove = true;
                return;
            }

            DragMove();
        }
        private void Header_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mRestoreIfMove = false;
        }
        private void Header_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mRestoreIfMove)
            {
                mRestoreIfMove = false;
                DragMove();
            }
        }
        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SwitchWindowState();
        }
        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TControl.Height = window.ActualHeight - 275;
        }
        #endregion

        #region Настройки количества этажей
        private void Floor0(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if ((bool)checkBox.IsChecked)
                {
                    Floor0HeighSetupBlock.Visibility = Visibility.Visible;
                    GB_0FloorBlock.Visibility = Visibility.Visible;

                    GB_0FloorBlock.Visibility = Visibility.Visible;

                    GB_Set0FloorF1Glase.Visibility = Visibility.Visible;
                    GB_Set0FloorF2Glase.Visibility = Visibility.Visible;
                    GB_Set0FloorF3Glase.Visibility = Visibility.Visible;
                    GB_Set0FloorF4Glase.Visibility = Visibility.Visible;
                }
                else
                {
                    Floor0HeighSetupBlock.Visibility = Visibility.Collapsed;
                    GB_0FloorBlock.Visibility = Visibility.Collapsed;

                    GB_0FloorBlock.Visibility = Visibility.Collapsed;

                    GB_Set0FloorF1Glase.Visibility = Visibility.Collapsed;
                    GB_Set0FloorF2Glase.Visibility = Visibility.Collapsed;
                    GB_Set0FloorF3Glase.Visibility = Visibility.Collapsed;
                    GB_Set0FloorF4Glase.Visibility = Visibility.Collapsed;
                }

            }
            // UpdateCBFloorSelect();
            CalcFloorsCount();
        }
        private void UpdateCBFloorSelect()
        {
            CB_FloorSelect.Items.Clear();

            if (Floor0Enabled.IsChecked == true)
            {
                CB_FloorSelect.Items.Add("Цокольный");
            }
            switch (SetFloorsHouse.SelectedIndex)
            {
                case 0:
                    CB_FloorSelect.Items.Add("Первый этаж");
                    break;
                case 1:
                    CB_FloorSelect.Items.Add("Первый этаж");
                    CB_FloorSelect.Items.Add("Второй этаж");
                    break;
                case 2:
                    CB_FloorSelect.Items.Add("Первый этаж");
                    CB_FloorSelect.Items.Add("Второй этаж");
                    CB_FloorSelect.Items.Add("Третий этаж");
                    break;
            }
            CB_FloorSelect.SelectedIndex = 0;
        }
        private void SetFloorsHouseChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Floor1HeighSetupBlock == null) return;
            Floor1HeighSetupBlock.Visibility = Visibility.Collapsed;
            Floor2HeighSetupBlock.Visibility = Visibility.Collapsed;
            Floor3HeighSetupBlock.Visibility = Visibility.Collapsed;

            GB_2FloorBlock.Visibility = Visibility.Collapsed;
            GB_3FloorBlock.Visibility = Visibility.Collapsed;

            GB_Set1FloorF1Glase.Visibility = Visibility.Collapsed;
            GB_Set1FloorF2Glase.Visibility = Visibility.Collapsed;
            GB_Set1FloorF3Glase.Visibility = Visibility.Collapsed;
            GB_Set1FloorF4Glase.Visibility = Visibility.Collapsed;
            GB_Set2FloorF1Glase.Visibility = Visibility.Collapsed;
            GB_Set2FloorF2Glase.Visibility = Visibility.Collapsed;
            GB_Set2FloorF3Glase.Visibility = Visibility.Collapsed;
            GB_Set2FloorF4Glase.Visibility = Visibility.Collapsed;
            GB_Set3FloorF1Glase.Visibility = Visibility.Collapsed;
            GB_Set3FloorF2Glase.Visibility = Visibility.Collapsed;
            GB_Set3FloorF3Glase.Visibility = Visibility.Collapsed;
            GB_Set3FloorF4Glase.Visibility = Visibility.Collapsed;


            switch (SetFloorsHouse.SelectedIndex)
            {
                case 0:
                    Floor1HeighSetupBlock.Visibility = Visibility.Visible;
                    GB_1FloorBlock.Visibility = Visibility.Visible;

                    GB_Set1FloorF1Glase.Visibility = Visibility.Visible;
                    GB_Set1FloorF2Glase.Visibility = Visibility.Visible;
                    GB_Set1FloorF3Glase.Visibility = Visibility.Visible;
                    GB_Set1FloorF4Glase.Visibility = Visibility.Visible;
                    break;
                case 1:
                    Floor1HeighSetupBlock.Visibility = Visibility.Visible;
                    Floor2HeighSetupBlock.Visibility = Visibility.Visible;

                    GB_1FloorBlock.Visibility = Visibility.Visible;
                    GB_2FloorBlock.Visibility = Visibility.Visible;

                    GB_Set1FloorF1Glase.Visibility = Visibility.Visible;
                    GB_Set1FloorF2Glase.Visibility = Visibility.Visible;
                    GB_Set1FloorF3Glase.Visibility = Visibility.Visible;
                    GB_Set1FloorF4Glase.Visibility = Visibility.Visible;
                    GB_Set2FloorF1Glase.Visibility = Visibility.Visible;
                    GB_Set2FloorF2Glase.Visibility = Visibility.Visible;
                    GB_Set2FloorF3Glase.Visibility = Visibility.Visible;
                    GB_Set2FloorF4Glase.Visibility = Visibility.Visible;
                    break;
                case 2:
                    Floor1HeighSetupBlock.Visibility = Visibility.Visible;
                    Floor2HeighSetupBlock.Visibility = Visibility.Visible;
                    Floor3HeighSetupBlock.Visibility = Visibility.Visible;

                    GB_1FloorBlock.Visibility = Visibility.Visible;
                    GB_2FloorBlock.Visibility = Visibility.Visible;
                    GB_3FloorBlock.Visibility = Visibility.Visible;

                    GB_Set1FloorF1Glase.Visibility = Visibility.Visible;
                    GB_Set1FloorF2Glase.Visibility = Visibility.Visible;
                    GB_Set1FloorF3Glase.Visibility = Visibility.Visible;
                    GB_Set1FloorF4Glase.Visibility = Visibility.Visible;

                    GB_Set2FloorF1Glase.Visibility = Visibility.Visible;
                    GB_Set2FloorF2Glase.Visibility = Visibility.Visible;
                    GB_Set2FloorF3Glase.Visibility = Visibility.Visible;
                    GB_Set2FloorF4Glase.Visibility = Visibility.Visible;

                    GB_Set3FloorF1Glase.Visibility = Visibility.Visible;
                    GB_Set3FloorF2Glase.Visibility = Visibility.Visible;
                    GB_Set3FloorF3Glase.Visibility = Visibility.Visible;
                    GB_Set3FloorF4Glase.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
            // UNDONE : Заказчик просил отключить. Скрыл пока выпадающий список из раздела "Этажи"
            // UpdateCBFloorSelect();
            CalcFloorsCount();
        }
        private void CB_FloorSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GB_0FloorBlock == null) return;
            GB_0FloorBlock.Visibility = Visibility.Collapsed;
            GB_1FloorBlock.Visibility = Visibility.Collapsed;
            GB_2FloorBlock.Visibility = Visibility.Collapsed;
            GB_3FloorBlock.Visibility = Visibility.Collapsed;

            switch (CB_FloorSelect.SelectedItem)
            {
                case "Цокольный":
                    GB_0FloorBlock.Visibility = Visibility.Visible;
                    break;
                case "Первый этаж":
                    GB_1FloorBlock.Visibility = Visibility.Visible;
                    break;
                case "Второй этаж":
                    GB_2FloorBlock.Visibility = Visibility.Visible;
                    break;
                case "Третий этаж":
                    GB_3FloorBlock.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Валидация
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex;

            int count = (sender as TextBox).Text.Where(c => c == '.').Count();
            int count2 = (sender as TextBox).Text.Where(c => c == ',').Count();

            if (count == 0 && count2 == 0)
                regex = new Regex("[^0-9.,]+");
            else
                regex = new Regex("[^0-9]+");

            e.Handled = regex.IsMatch(e.Text);
        }

        #endregion

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            point_s = Mouse.GetPosition(CanvasForPhantomShape);
            if (TrianglePointCount == 1 && ShapeType == (byte)GlobalVariables.ShapeTypeEnum.Triangle)
                point_s = point_f;


            if (TrianglePointCount == 0 && ShapeType == (byte)GlobalVariables.ShapeTypeEnum.Triangle)
                firstTrianglePoint = point_s;

            isPainting = true;
        }
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = Mouse.GetPosition(CanvasForPhantomShape);

            if (axisAligment)
            {
                double diffX = point_s.X - p.X;
                double diffY = point_s.Y - p.Y;
                // Горизонт
                if (Math.Abs(diffX) > Math.Abs(diffY))
                    p.Y = point_s.Y;
                // Вертикаль
                else
                    p.X = point_s.X;
                point_f = p;
            }
            DrawShape(p);
            Tb_CursorСoordinates.Text = $"X:{p.X} Y:{p.Y}";
        }
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ClearAllPhantomLine();
            if (!axisAligment)
                point_f = Mouse.GetPosition(CanvasForPhantomShape);
            // Если контрольная линия
            if (isControlLine)
            {
                if (!double.TryParse(TB_RealLength.Text, out realLength))
                {
                    axisAligment = false;
                    MessageBox.Show("Не верно указан размер", "Ошибка ввода данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Удаляем предыдущую линию
                if (ShapeType != (byte)GlobalVariables.ShapeTypeEnum.Triangle)
                {
                    for (int i = 0; i < CanvasForPhantomShape.Children.Count; i++)
                    {
                        if (CanvasForPhantomShape.Children[i] is Line line)
                        {
                            if (line.Name == "ControlLine")
                            {
                                CanvasForPhantomShape.Children.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }

                controlLineLength = Math.Sqrt(Math.Pow(point_f.X - point_s.X, 2) + Math.Pow(point_f.Y - point_s.Y, 2));

                CanvasForPhantomShape.Children.Add(new Line()
                {
                    Name = "ControlLine",
                    X1 = point_f.X,
                    Y1 = point_f.Y,
                    X2 = point_s.X,
                    Y2 = point_s.Y,
                    StrokeThickness = 2,
                    Stroke = Brushes.Red
                });

                isControlLine = false;
                //    Tb_Information.Visibility = Visibility.Collapsed;
                coeffLength = controlLineLength / realLength;
                if (CanvasForPhantomShape.Children.Count > 0)
                    Btn_DeleteLastLine.IsEnabled = true;
                Tb_Information.Text = "";

                ShapeType = tempShapeType;
                MessageBox.Show("Коэффициент размера успешно установлен", "Изменение коэффициента", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Если рисование
            if (isPainting && SelectedBuidingObj != 0)
            {
                if (coeffLength == 0.1)
                {
                    TB_RealLength.Focus();
                    axisAligment = false;
                    MessageBox.Show("Необходимо установить масштаб чертежа", "Предупреждение расчетов", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ShapeType == (byte)GlobalVariables.ShapeTypeEnum.Line)
                {
                    Shapes.Add(new ModifyLine(new Line() { X1 = point_s.X, Y1 = point_s.Y, X2 = point_f.X, Y2 = point_f.Y }, SelectedBuidingObj, ShapeType, shapeColor, coeffLength, selectedCanvas.Name));

                    ModifyLine shape = Shapes[Shapes.Count - 1] as ModifyLine;
                    selectedCanvas.Children.Add(shape.Line);
                }

                if (ShapeType == (byte)GlobalVariables.ShapeTypeEnum.Rect)
                {

                    double x = Math.Min(point_f.X, point_s.X);
                    double y = Math.Min(point_f.Y, point_s.Y);

                    Rectangle rect = new Rectangle
                    {
                        Width = Math.Max(point_f.X, point_s.X) - x,
                        Height = Math.Max(point_f.Y, point_s.Y) - y
                    };

                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);

                    Shapes.Add(new ModifyRect(rect, SelectedBuidingObj, ShapeType, shapeColor, coeffLength, selectedCanvas.Name));

                    ModifyRect shape = Shapes[Shapes.Count - 1] as ModifyRect;
                    selectedCanvas.Children.Add(shape.Rectangle);
                }
                //    // ЕСЛИ ТРЕУГОЛЬНИК
                if (ShapeType == (byte)GlobalVariables.ShapeTypeEnum.Triangle)
                {
                    TrianglePointCount++;
                    var line = new Line
                    {
                        X1 = point_s.X,
                        Y1 = point_s.Y,
                        X2 = point_f.X,
                        Y2 = point_f.Y,
                        Stroke = shapeColor,
                        StrokeThickness = Settings.ShapeThickness
                    };
                    selectedCanvas.Children.Add(line);

                    if (TrianglePointCount == 2)
                    {
                        line = new Line
                        {
                            X1 = point_f.X,
                            Y1 = point_f.Y,
                            X2 = firstTrianglePoint.X,
                            Y2 = firstTrianglePoint.Y,
                            Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(Settings.ShapeColorDefault),
                            StrokeThickness = Settings.ShapeThickness
                        };
                        selectedCanvas.Children.Add(line);
                        TrianglePointCount = 0;

                        var triange = new Polygon();
                        triange.Points.Add(firstTrianglePoint);
                        triange.Points.Add(point_s);
                        triange.Points.Add(point_f);


                        // Удалить фантомные линии
                        selectedCanvas.Children.RemoveAt(selectedCanvas.Children.Count - 1);
                        selectedCanvas.Children.RemoveAt(selectedCanvas.Children.Count - 1);
                        //SelectedCanvas.Children.RemoveAt(Walls.Count);

                        Shapes.Add(new ModifyTriangle(triange, SelectedBuidingObj, ShapeType, shapeColor, coeffLength, selectedCanvas.Name));
                        ModifyTriangle shape = Shapes[Shapes.Count - 1] as ModifyTriangle;
                        selectedCanvas.Children.Add(shape.Triangle);
                    }
                }

                if (selectedCanvas.Children.Count > 0)
                    Btn_DeleteLastLine.IsEnabled = true;
                CalcSize();
            }
            isPainting = false;
        }
        private void DrawShape(Point p)
        {
            if (isPainting)
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    ClearAllPhantomLine();
                    switch (ShapeType)
                    {
                        case (byte)GlobalVariables.ShapeTypeEnum.Line:
                        case (byte)GlobalVariables.ShapeTypeEnum.Triangle:
                            DrawPhantomLine(point_s, p);
                            break;
                        case (byte)GlobalVariables.ShapeTypeEnum.Rect:
                            DrawPhantomRect(point_s, p);
                            break;
                    }
                }
            }
        }
        private void ClearAllPhantomLine()
        {
            for (int i = 0; i < CanvasForPhantomShape.Children.Count; i++)
            {
                if (CanvasForPhantomShape.Children[i] is Line line)
                {
                    if (line.Name != "ControlLine")
                    {
                        CanvasForPhantomShape.Children.RemoveAt(i);
                        break;
                    }
                }
                if (CanvasForPhantomShape.Children[i] is Rectangle rect)
                {
                    if (rect.Name != "ControlLine")
                    {
                        CanvasForPhantomShape.Children.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        private void DeleteLastShape()
        {
            if (selectedCanvas.Children.Count > 0)
            {
                selectedCanvas.Children.RemoveAt(selectedCanvas.Children.Count - 1);
                if (Shapes.Count > 0)
                {
                    // Если треугольник, удаляем лишнюю линию
                    if (Shapes.Last().ShapeType == (byte)GlobalVariables.ShapeTypeEnum.Triangle && selectedCanvas.Children.Count > 0)
                    {
                        selectedCanvas.Children.RemoveAt(selectedCanvas.Children.Count - 1);
                    }

                    Shapes.Remove(Shapes.Last());
                    CalcSize();
                }
            }
            if (selectedCanvas.Children.Count == 0)
                Btn_DeleteLastLine.IsEnabled = false;
        }
        private void DrawPhantomLine(Point start, Point stop)
        {
            if (isPainting)
            {
                Line line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = start.X,
                    X2 = stop.X,
                    Y1 = start.Y,
                    Y2 = stop.Y
                };
                CanvasForPhantomShape.Children.Add(line);
            }
        }
        private void DrawPhantomRect(Point start, Point stop)
        {
            if (isPainting)
            {
                double x = Math.Min(stop.X, start.X);
                double y = Math.Min(stop.Y, start.Y);

                // Set the dimenssion of the rectangle
                double w = Math.Max(stop.X, start.X) - x;
                double h = Math.Max(stop.Y, start.Y) - y;

                Rectangle rectangle = new Rectangle
                {
                    Stroke = Brushes.Black,
                    Width = w,
                    Height = h
                };
                Canvas.SetLeft(rectangle, x);
                Canvas.SetTop(rectangle, y);
                CanvasForPhantomShape.Children.Add(rectangle);
            }
        }

        private void CalcSize()
        {
            byte roundTo = 2; //Округление до
            switch (SelectedBuidingObj)
            {
                #region Раздел "Общие"
                case (byte)GlobalVariables.ProjectObjEnum.Floor0Height:
                    House.Floor0Height = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0Height).Select(l => l.Length).Sum(), roundTo);
                    Tb_Set0FloorHeight.Text = $"{House.Floor0Height} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1Height:
                    House.Floor1Height = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1Height).Select(l => l.Length).Sum(), roundTo);
                    Tb_Set1FloorHeight.Text = $"{House.Floor1Height} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2Height:
                    House.Floor2Height = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2Height).Select(l => l.Length).Sum(), roundTo);
                    Tb_Set2FloorHeight.Text = $"{House.Floor2Height} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3Height:
                    House.Floor3Height = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3Height).Select(l => l.Length).Sum(), roundTo);
                    Tb_Set3FloorHeight.Text = $"{House.Floor3Height} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.KitchensSquare:
                    House.KitchensSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.KitchensSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_SetKitchenSquare.Text = $"{House.KitchensSquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.RoofHeight:
                    House.RoofHeight = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.RoofHeight).Select(l => l.Length).Sum(), roundTo);
                    Tb_SetFullHouseHeight.Text = $"{House.RoofHeight} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.RoofMinWallHeight:
                    House.RoofMinWallHeight = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.RoofMinWallHeight).Select(l => l.Length).Sum(), roundTo);
                    Tb_SetRoofMinWallHeight.Text = $"{House.RoofMinWallHeight} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.RoofSquare:
                    House.RoofSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.RoofSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_SetRoofSquare.Text = $"{House.RoofSquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.RoofLength:
                    House.RoofLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.RoofLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_SetRoofLength.Text = $"{House.RoofLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.CanopySquare:
                    House.CanopySquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.CanopySquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_SetCanopySquare.Text = $"{House.CanopySquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.CanopyLength:
                    House.CanopyLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.CanopyLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_SetCanopyLength.Text = $"{House.CanopyLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.PergolaSquare:
                    House.PergolaSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.PergolaSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_SetPergolaSquare.Text = $"{House.PergolaSquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.HemmingButt:
                    House.HemmingButt = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.HemmingButt).Select(l => l.Length).Sum(), roundTo);
                    Tb_SetHemmingButt.Text = $"{House.HemmingButt} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.HemmingOverhangsSquare:
                    House.HemmingOverhangsSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.HemmingOverhangsSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_SetHemmingOverhangsSquare.Text = $"{House.HemmingOverhangsSquare} кв.м.";
                    break;
                #endregion
                #region Раздел "Фасады - 1"
                case (byte)GlobalVariables.ProjectObjEnum.Floor0F1GlaseSq:
                    House.Floor0F1GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0F1GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor0F1GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0F1GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set0FloorF1GlaseSq.Text = $"{House.Floor0F1GlaseSq} кв.м.";
                    Tb_Set0FloorF1GlaseP.Text = $"{House.Floor0F1GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1F1GlaseSq:
                    House.Floor1F1GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1F1GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor1F1GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1F1GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set1FloorF1GlaseSq.Text = $"{House.Floor1F1GlaseSq} кв.м.";
                    Tb_Set1FloorF1GlaseP.Text = $"{House.Floor1F1GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2F1GlaseSq:
                    House.Floor2F1GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2F1GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor2F1GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2F1GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set2FloorF1GlaseSq.Text = $"{House.Floor2F1GlaseSq} кв.м.";
                    Tb_Set2FloorF1GlaseP.Text = $"{House.Floor2F1GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3F1GlaseSq:
                    House.Floor3F1GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3F1GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor3F1GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3F1GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set3FloorF1GlaseSq.Text = $"{House.Floor3F1GlaseSq} кв.м.";
                    Tb_Set3FloorF1GlaseP.Text = $"{House.Floor3F1GlaseP} м.";
                    break;
                #endregion
                #region Раздел "Фасады - 2"
                case (byte)GlobalVariables.ProjectObjEnum.Floor0F2GlaseSq:
                    House.Floor0F2GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0F2GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor0F2GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0F2GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set0FloorF2GlaseSq.Text = $"{House.Floor0F2GlaseSq} кв.м.";
                    Tb_Set0FloorF2GlaseP.Text = $"{House.Floor0F2GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1F2GlaseSq:
                    House.Floor1F2GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1F2GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor1F2GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1F2GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set1FloorF2GlaseSq.Text = $"{House.Floor1F2GlaseSq} кв.м.";
                    Tb_Set1FloorF2GlaseP.Text = $"{House.Floor1F2GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2F2GlaseSq:
                    House.Floor2F2GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2F2GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor2F2GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2F2GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set2FloorF2GlaseSq.Text = $"{House.Floor2F2GlaseSq} кв.м.";
                    Tb_Set2FloorF2GlaseP.Text = $"{House.Floor2F2GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3F2GlaseSq:
                    House.Floor3F2GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3F2GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor3F2GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3F2GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set3FloorF2GlaseSq.Text = $"{House.Floor3F2GlaseSq} кв.м.";
                    Tb_Set3FloorF2GlaseP.Text = $"{House.Floor3F2GlaseP} м.";
                    break;
                #endregion
                #region Раздел "Фасады - 3"
                case (byte)GlobalVariables.ProjectObjEnum.Floor0F3GlaseSq:
                    House.Floor0F3GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0F3GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor0F3GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0F3GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set0FloorF3GlaseSq.Text = $"{House.Floor0F3GlaseSq} кв.м.";
                    Tb_Set0FloorF3GlaseP.Text = $"{House.Floor0F3GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1F3GlaseSq:
                    House.Floor1F3GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1F3GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor1F3GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1F3GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set1FloorF3GlaseSq.Text = $"{House.Floor1F3GlaseSq} кв.м.";
                    Tb_Set1FloorF3GlaseP.Text = $"{House.Floor1F3GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2F3GlaseSq:
                    House.Floor2F3GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2F3GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor2F3GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2F3GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set2FloorF3GlaseSq.Text = $"{House.Floor2F3GlaseSq} кв.м.";
                    Tb_Set2FloorF3GlaseP.Text = $"{House.Floor2F3GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3F3GlaseSq:
                    House.Floor3F3GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3F3GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor3F3GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3F3GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set3FloorF3GlaseSq.Text = $"{House.Floor3F3GlaseSq} кв.м.";
                    Tb_Set3FloorF3GlaseP.Text = $"{House.Floor3F3GlaseP} м.";
                    break;
                #endregion
                #region Раздел "Фасады - 4"
                case (byte)GlobalVariables.ProjectObjEnum.Floor0F4GlaseSq:
                    House.Floor0F4GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0F4GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor0F4GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0F4GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set0FloorF4GlaseSq.Text = $"{House.Floor0F4GlaseSq} кв.м.";
                    Tb_Set0FloorF4GlaseP.Text = $"{House.Floor0F4GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1F4GlaseSq:
                    House.Floor1F4GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1F4GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor1F4GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1F4GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set1FloorF4GlaseSq.Text = $"{House.Floor1F4GlaseSq} кв.м.";
                    Tb_Set1FloorF4GlaseP.Text = $"{House.Floor1F4GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2F4GlaseSq:
                    House.Floor2F4GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2F4GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor2F4GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2F4GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set2FloorF4GlaseSq.Text = $"{House.Floor2F4GlaseSq} кв.м.";
                    Tb_Set2FloorF4GlaseP.Text = $"{House.Floor2F4GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3F4GlaseSq:
                    House.Floor3F4GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3F4GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor3F4GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3F4GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set3FloorF4GlaseSq.Text = $"{House.Floor3F4GlaseSq} кв.м.";
                    Tb_Set3FloorF4GlaseP.Text = $"{House.Floor3F4GlaseP} м.";
                    break;
                #endregion
                #region Раздел "Этажи"
                case (byte)GlobalVariables.ProjectObjEnum.Floor0PlinthHeight:
                    House.Floor0PlinthHeight = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0PlinthHeight).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0PlinthHeight.Text = $"{House.Floor0PlinthHeight} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0BadroomSquare:
                    House.Floor0BadroomSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0BadroomSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor0BadroomSquare.Text = $"{House.Floor0BadroomSquare} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0TileSquare:
                    House.Floor0TileSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0TileSquare).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0TileSquare.Text = $"{House.Floor0TileSquare} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0TilePerimeter:
                    House.Floor0TilePerimeter = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0TilePerimeter).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0TilePerimeter.Text = $"{House.Floor0TilePerimeter} м.";
                    break;

                case (byte)GlobalVariables.ProjectObjEnum.Floor0OutWallsLength:
                    House.Floor0OutWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0OutWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0OutWallsLength.Text = $"{House.Floor0OutWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0InnerWallsLength:
                    House.Floor0InnerWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0InnerWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0InnerWallsLength.Text = $"{House.Floor0InnerWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0LightWallsLength:
                    House.Floor0LightWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0LightWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0LightWallsLength.Text = $"{House.Floor0LightWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0BreakWallsLength:
                    House.Floor0BreakWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0BreakWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0BreakWallsLength.Text = $"{House.Floor0BreakWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0OutDoorsLength:
                    House.Floor0OutDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0OutDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0OutDoorsLength.Text = $"{House.Floor0OutDoorsLength} м.";
                    House.Floor0OutDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0OutDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor0OutDoorsCount.Text = $"{House.Floor0OutDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0InnerDoorsLength:
                    House.Floor0InnerDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0InnerDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0InnerDoorsLength.Text = $"{House.Floor0InnerDoorsLength} м.";
                    House.Floor0InnerDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0InnerDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor0InnerDoorsCount.Text = $"{House.Floor0InnerDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0PartitionsDoorsLength:
                    House.Floor0PartitionsDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0PartitionsDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0PartitionsDoorsLength.Text = $"{House.Floor0PartitionsDoorsLength} м.";
                    House.Floor0PartitionsDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0PartitionsDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor0PartitionsDoorsCount.Text = $"{House.Floor0PartitionsDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0GatesLength:
                    House.Floor0GatesLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0GatesLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0GatesLength.Text = $"{House.Floor0GatesLength} м.";
                    House.Floor0GatesCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0GatesLength).Select(l => l.Length).Count();
                    Tb_Floor0GatesCount.Text = $"Ворота - {House.Floor0GatesCount} шт.";
                    break;

                case (byte)GlobalVariables.ProjectObjEnum.Floor0TerassesSquare:
                    House.Floor0TerassesSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0TerassesSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor0TerassesSquare.Text = $"{House.Floor0TerassesSquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0InnerTerassesLength:
                    House.Floor0InnerTerassesLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0InnerTerassesLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0InnerTerassesLength.Text = $"{House.Floor0InnerTerassesLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0TerassesLength:
                    House.Floor0TerassesLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0TerassesLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0TerassesLength.Text = $"{House.Floor0TerassesLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0RailingsLength:
                    House.Floor0RailingsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0RailingsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0RailingsLength.Text = $"{House.Floor0RailingsLength} м.";
                    break;

                case (byte)GlobalVariables.ProjectObjEnum.Floor1BadroomSquare:
                    House.Floor1BadroomSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1BadroomSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor1BadroomSquare.Text = $"{House.Floor1BadroomSquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1TileSquare:
                    House.Floor1TileSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1TileSquare).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1TileSquare.Text = $"{House.Floor1TileSquare} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1TilePerimeter:
                    House.Floor1TilePerimeter = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1TilePerimeter).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1TilePerimeter.Text = $"{House.Floor1TilePerimeter} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1OutWallsLength:
                    House.Floor1OutWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1OutWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1OutWallsLength.Text = $"{House.Floor1OutWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1InnerWallsLength:
                    House.Floor1InnerWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1InnerWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1InnerWallsLength.Text = $"{House.Floor1InnerWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1LightWallsLength:
                    House.Floor1LightWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1LightWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1LightWallsLength.Text = $"{House.Floor1LightWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1BreakWallsLength:
                    House.Floor1BreakWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1BreakWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1BreakWallsLength.Text = $"{House.Floor1BreakWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1OutDoorsLength:
                    House.Floor1OutDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1OutDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1OutDoorsLength.Text = $"{House.Floor1OutDoorsLength} м.";
                    House.Floor1OutDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1OutDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor1OutDoorsCount.Text = $"{House.Floor1OutDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1InnerDoorsLength:
                    House.Floor1InnerDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1InnerDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1InnerDoorsLength.Text = $"{House.Floor1InnerDoorsLength} м.";
                    House.Floor1InnerDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1InnerDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor1InnerDoorsCount.Text = $"{House.Floor1InnerDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1PartitionsDoorsLength:
                    House.Floor1PartitionsDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1PartitionsDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1PartitionsDoorsLength.Text = $"{House.Floor1PartitionsDoorsLength} м.";
                    House.Floor1PartitionsDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1PartitionsDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor1PartitionsDoorsCount.Text = $"{House.Floor1PartitionsDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1GatesLength:
                    House.Floor1GatesLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1GatesLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1GatesLength.Text = $"{House.Floor1GatesLength} м.";
                    House.Floor1GatesCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1GatesLength).Select(l => l.Length).Count();
                    Tb_Floor1GatesCount.Text = $"Ворота - {House.Floor1GatesCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1TerassesSquare:
                    House.Floor1TerassesSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1TerassesSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor1TerassesSquare.Text = $"{House.Floor1TerassesSquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1InnerTerassesLength:
                    House.Floor1InnerTerassesLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1InnerTerassesLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1InnerTerassesLength.Text = $"{House.Floor1InnerTerassesLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1TerassesLength:
                    House.Floor1TerassesLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1TerassesLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1TerassesLength.Text = $"{House.Floor1TerassesLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1RailingsLength:
                    House.Floor1RailingsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1RailingsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1RailingsLength.Text = $"{House.Floor1RailingsLength} м.";
                    break;

                case (byte)GlobalVariables.ProjectObjEnum.Floor2РHoleSecondLight:
                    House.Floor2РHoleSecondLight = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2РHoleSecondLight).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor2РHoleSecondLight.Text = $"{House.Floor2РHoleSecondLight} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2BadroomSquare:
                    House.Floor2BadroomSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2BadroomSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor2BadroomSquare.Text = $"{House.Floor2BadroomSquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2TileSquare:
                    House.Floor2TileSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2TileSquare).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2TileSquare.Text = $"{House.Floor2TileSquare} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2TilePerimeter:
                    House.Floor2TilePerimeter = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2TilePerimeter).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2TilePerimeter.Text = $"{House.Floor2TilePerimeter} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2OutWallsLength:
                    House.Floor2OutWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2OutWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2OutWallsLength.Text = $"{House.Floor2OutWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2InnerWallsLength:
                    House.Floor2InnerWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2InnerWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2InnerWallsLength.Text = $"{House.Floor2InnerWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2LightWallsLength:
                    House.Floor2LightWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2LightWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2LightWallsLength.Text = $"{House.Floor2LightWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2BreakWallsLength:
                    House.Floor2BreakWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2BreakWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2BreakWallsLength.Text = $"{House.Floor2BreakWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2OutDoorsLength:
                    House.Floor2OutDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2OutDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2OutDoorsLength.Text = $"{House.Floor2OutDoorsLength} м.";
                    House.Floor2OutDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2OutDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor2OutDoorsCount.Text = $"{House.Floor2OutDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2InnerDoorsLength:
                    House.Floor2InnerDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2InnerDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2InnerDoorsLength.Text = $"{House.Floor2InnerDoorsLength} м.";
                    House.Floor2InnerDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2InnerDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor2InnerDoorsCount.Text = $"{House.Floor2InnerDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2PartitionsDoorsLength:
                    House.Floor2PartitionsDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2PartitionsDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2PartitionsDoorsLength.Text = $"{House.Floor2PartitionsDoorsLength} м.";
                    House.Floor2PartitionsDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2PartitionsDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor2PartitionsDoorsCount.Text = $"{House.Floor2PartitionsDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2BalconySquare:
                    House.Floor2BalconySquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2BalconySquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor2BalconySquare.Text = $"{House.Floor2BalconySquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2BalconyLength:
                    House.Floor2BalconyLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2BalconyLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2BalconyLength.Text = $"{House.Floor2BalconyLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2RailingsLength:
                    House.Floor2RailingsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2RailingsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2RailingsLength.Text = $"{House.Floor2RailingsLength} м.";
                    break;

                case (byte)GlobalVariables.ProjectObjEnum.Floor3РHoleSecondLight:
                    House.Floor3РHoleSecondLight = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3РHoleSecondLight).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor3РHoleSecondLight.Text = $"{House.Floor3РHoleSecondLight} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3BadroomSquare:
                    House.Floor3BadroomSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3BadroomSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor3BadroomSquare.Text = $"{House.Floor3BadroomSquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3TileSquare:
                    House.Floor3TileSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3TileSquare).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3TileSquare.Text = $"{House.Floor3TileSquare} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3TilePerimeter:
                    House.Floor3TilePerimeter = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3TilePerimeter).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3TilePerimeter.Text = $"{House.Floor3TilePerimeter} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3OutWallsLength:
                    House.Floor3OutWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3OutWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3OutWallsLength.Text = $"{House.Floor3OutWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3InnerWallsLength:
                    House.Floor3InnerWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3InnerWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3InnerWallsLength.Text = $"{House.Floor3InnerWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3LightWallsLength:
                    House.Floor3LightWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3LightWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3LightWallsLength.Text = $"{House.Floor3LightWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3BreakWallsLength:
                    House.Floor3BreakWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3BreakWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3BreakWallsLength.Text = $"{House.Floor3BreakWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3OutDoorsLength:
                    House.Floor3OutDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3OutDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3OutDoorsLength.Text = $"{House.Floor3OutDoorsLength} м.";
                    House.Floor3OutDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3OutDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor3OutDoorsCount.Text = $"{House.Floor3OutDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3InnerDoorsLength:
                    House.Floor3InnerDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3InnerDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3InnerDoorsLength.Text = $"{House.Floor3InnerDoorsLength} м.";
                    House.Floor3InnerDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3InnerDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor3InnerDoorsCount.Text = $"{House.Floor3InnerDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3PartitionsDoorsLength:
                    House.Floor3PartitionsDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3PartitionsDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3PartitionsDoorsLength.Text = $"{House.Floor3PartitionsDoorsLength} м.";
                    House.Floor3PartitionsDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3PartitionsDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor3PartitionsDoorsCount.Text = $"{House.Floor3PartitionsDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3BalconySquare:
                    House.Floor3BalconySquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3BalconySquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor3BalconySquare.Text = $"{House.Floor3BalconySquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3BalconyLength:
                    House.Floor3BalconyLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3BalconyLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3BalconyLength.Text = $"{House.Floor3BalconyLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3RailingsLength:
                    House.Floor3RailingsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3RailingsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3RailingsLength.Text = $"{House.Floor3RailingsLength} м.";
                    break;

                    #endregion
            }
        }
        private void ResetForm()
        {
            if (House.Floor0Height == 0)
                Floor0Enabled.IsChecked = false;
            else
                Floor0Enabled.IsChecked = true;

            Tb_Set0FloorHeight.Text = "0.00 м.";
            Tb_Set1FloorHeight.Text = "0.00 м.";
            Tb_Set2FloorHeight.Text = "0.00 м.";
            Tb_Set3FloorHeight.Text = "0.00 м.";
            Tb_SetKitchenSquare.Text = "0.00 кв.м.";
            Tb_SetFullHouseHeight.Text = "0.00 м.";
            Tb_SetRoofMinWallHeight.Text = "0.00 м.";
            Tb_SetRoofSquare.Text = "0.00 кв.м.";
            Tb_SetRoofLength.Text = "0.00 м.";
            Tb_SetCanopySquare.Text = "0.00 кв.м.";
            Tb_SetCanopyLength.Text = "0.00 м.";
            Tb_SetPergolaSquare.Text = "0.00 кв.м.";
            Tb_SetHemmingButt.Text = "0.00 м.";
            Tb_SetHemmingOverhangsSquare.Text = "0.00 кв.м.";

            Tb_Set0FloorF1GlaseSq.Text = "0.00 кв.м.";
            Tb_Set0FloorF1GlaseP.Text = "0.00 м.";
            Tb_Set1FloorF1GlaseSq.Text = "0.00 кв.м.";
            Tb_Set1FloorF1GlaseP.Text = "0.00 м.";
            Tb_Set2FloorF1GlaseSq.Text = "0.00 кв.м.";
            Tb_Set2FloorF1GlaseP.Text = "0.00 м.";
            Tb_Set3FloorF1GlaseSq.Text = "0.00 кв.м.";
            Tb_Set3FloorF1GlaseP.Text = "0.00 м.";

            Tb_Set0FloorF2GlaseSq.Text = "0.00 кв.м.";
            Tb_Set0FloorF2GlaseP.Text = "0.00 м.";
            Tb_Set1FloorF2GlaseSq.Text = "0.00 кв.м.";
            Tb_Set1FloorF2GlaseP.Text = "0.00 м.";
            Tb_Set2FloorF2GlaseSq.Text = "0.00 кв.м.";
            Tb_Set2FloorF2GlaseP.Text = "0.00 м.";
            Tb_Set3FloorF2GlaseSq.Text = "0.00 кв.м.";
            Tb_Set3FloorF2GlaseP.Text = "0.00 м.";

            Tb_Set0FloorF3GlaseSq.Text = "0.00 кв.м.";
            Tb_Set0FloorF3GlaseP.Text = "0.00 м.";
            Tb_Set1FloorF3GlaseSq.Text = "0.00 кв.м.";
            Tb_Set1FloorF3GlaseP.Text = "0.00 м.";
            Tb_Set2FloorF3GlaseSq.Text = "0.00 кв.м.";
            Tb_Set2FloorF3GlaseP.Text = "0.00 м.";
            Tb_Set3FloorF3GlaseSq.Text = "0.00 кв.м.";
            Tb_Set3FloorF3GlaseP.Text = "0.00 м.";

            Tb_Set0FloorF4GlaseSq.Text = "0.00 кв.м.";
            Tb_Set0FloorF4GlaseP.Text = "0.00 м.";
            Tb_Set1FloorF4GlaseSq.Text = "0.00 кв.м.";
            Tb_Set1FloorF4GlaseP.Text = "0.00 м.";
            Tb_Set2FloorF4GlaseSq.Text = "0.00 кв.м.";
            Tb_Set2FloorF4GlaseP.Text = "0.00 м.";
            Tb_Set3FloorF4GlaseSq.Text = "0.00 кв.м.";
            Tb_Set3FloorF4GlaseP.Text = "0.00 м.";

            Tb_Floor0PlinthHeight.Text = "0.00 м.";
            Tb_Floor0BadroomSquare.Text = "0.00 м.";
            Tb_Floor0TilePerimeter.Text = "0.00 м.";
            Tb_Floor0TileSquare.Text = "0.00 м.";
            Tb_Floor0TilePerimeter.Text = "0.00 м.";
            Tb_Floor0OutWallsLength.Text = "0.00 м.";
            Tb_Floor0InnerWallsLength.Text = "0.00 м.";
            Tb_Floor0LightWallsLength.Text = "0.00 м.";
            Tb_Floor0BreakWallsLength.Text = "0.00 м.";
            Tb_Floor0OutDoorsLength.Text = "0.00 м.";
            Tb_Floor0OutDoorsCount.Text = $"0 шт.";
            Tb_Floor0InnerDoorsLength.Text = "0.00 м.";
            Tb_Floor0InnerDoorsCount.Text = $"0 шт.";
            Tb_Floor0PartitionsDoorsLength.Text = "0.00 м.";
            Tb_Floor0PartitionsDoorsCount.Text = $"0 шт.";
            Tb_Floor0GatesLength.Text = "0.00 м.";
            Tb_Floor0GatesCount.Text = $"0 шт.";
            Tb_Floor0TerassesSquare.Text = "0.00 кв.м.";
            Tb_Floor0InnerTerassesLength.Text = "0.00 м.";
            Tb_Floor0TerassesLength.Text = "0.00 м.";
            Tb_Floor0RailingsLength.Text = "0.00 м.";

            Tb_Floor1BadroomSquare.Text = "0.00 кв.м.";
            Tb_Floor1TilePerimeter.Text = "0.00 м.";
            Tb_Floor1TileSquare.Text = "0.00 кв.м.";
            Tb_Floor1TilePerimeter.Text = "0.00 м.";
            Tb_Floor1OutWallsLength.Text = "0.00 м.";
            Tb_Floor1InnerWallsLength.Text = "0.00 м.";
            Tb_Floor1LightWallsLength.Text = "0.00 м.";
            Tb_Floor1BreakWallsLength.Text = "0.00 м.";
            Tb_Floor1OutDoorsLength.Text = "0.00 м.";
            Tb_Floor1OutDoorsCount.Text = $"0 шт.";
            Tb_Floor1InnerDoorsLength.Text = "0.00 м.";
            Tb_Floor1InnerDoorsCount.Text = $"0 шт.";
            Tb_Floor1PartitionsDoorsLength.Text = "0.00 м.";
            Tb_Floor1PartitionsDoorsCount.Text = $"0 шт.";
            Tb_Floor1GatesLength.Text = "0.00 м.";
            Tb_Floor1GatesCount.Text = $"0 шт.";
            Tb_Floor1TerassesSquare.Text = "0.00 кв.м.";
            Tb_Floor1InnerTerassesLength.Text = "0.00 м.";
            Tb_Floor1TerassesLength.Text = "0.00 м.";
            Tb_Floor1RailingsLength.Text = "0.00 м.";

            Tb_Floor2РHoleSecondLight.Text = "0.00 кв.м.";
            Tb_Floor2BadroomSquare.Text = "0.00 кв.м.";
            Tb_Floor2TilePerimeter.Text = "0.00 м.";
            Tb_Floor2TileSquare.Text = "0.00 кв.м.";
            Tb_Floor2TilePerimeter.Text = "0.00 м.";
            Tb_Floor2OutWallsLength.Text = "0.00 м.";
            Tb_Floor2InnerWallsLength.Text = "0.00 м.";
            Tb_Floor2LightWallsLength.Text = "0.00 м.";
            Tb_Floor2BreakWallsLength.Text = "0.00 м.";
            Tb_Floor2OutDoorsLength.Text = "0.00 м.";
            Tb_Floor2OutDoorsCount.Text = $"0 шт.";
            Tb_Floor2InnerDoorsLength.Text = "0.00 м.";
            Tb_Floor2InnerDoorsCount.Text = $"0 шт.";
            Tb_Floor2PartitionsDoorsLength.Text = "0.00 м.";
            Tb_Floor2PartitionsDoorsCount.Text = $"0 шт.";
            Tb_Floor2BalconySquare.Text = "0.00 кв.м.";
            Tb_Floor2BalconyLength.Text = "0.00 м.";
            Tb_Floor2RailingsLength.Text = "0.00 м.";

            Tb_Floor3РHoleSecondLight.Text = "0.00 кв.м.";
            Tb_Floor3BadroomSquare.Text = "0.00 кв.м.";
            Tb_Floor3TilePerimeter.Text = "0.00 м.";
            Tb_Floor3TileSquare.Text = "0.00 кв.м.";
            Tb_Floor3TilePerimeter.Text = "0.00 м.";
            Tb_Floor3OutWallsLength.Text = "0.00 м.";
            Tb_Floor3InnerWallsLength.Text = "0.00 м.";
            Tb_Floor3LightWallsLength.Text = "0.00 м.";
            Tb_Floor3BreakWallsLength.Text = "0.00 м.";
            Tb_Floor3OutDoorsLength.Text = "0.00 м.";
            Tb_Floor3OutDoorsCount.Text = $"0 шт.";
            Tb_Floor3InnerDoorsLength.Text = "0.00 м.";
            Tb_Floor3InnerDoorsCount.Text = $"0 шт.";
            Tb_Floor3PartitionsDoorsLength.Text = "0.00 м.";
            Tb_Floor3PartitionsDoorsCount.Text = $"0 шт.";
            Tb_Floor3BalconySquare.Text = "0.00 м.";
            Tb_Floor3BalconyLength.Text = "0.00 м.";
            Tb_Floor3RailingsLength.Text = "0.00 м.";


            // Ручные поля
            Tb_SetFloor1DecatativePillarsLessCount.Text = "";
            Tb_SetFloor1DecatativePillarsOverCount.Text = "";

            TB_RoomCount.Text = "";
            TB_BedroomCount.Text = "";

            Tb_WindowCount.Text = "";
            Tb_WindowSquare.Text = "";

            Tb_Set0FloorSquare.Text = "";
            Tb_SetFloor0BadroomCount.Text = "";

            Tb_SetFloor1Square.Text = "";
            Tb_SetFloor1BadroomCount.Text = "";

            Tb_SetFloor2Square.Text = "";
            Tb_SetFloor2BadroomCount.Text = "";

            Tb_SetFloor3Square.Text = "";
            Tb_SetFloor3BadroomCount.Text = "";

            SetFloorsHouse.SelectedIndex = 0;
            RoofType.SelectedIndex = -1;
        }
        private void LoadForm()
        {
            if (House.Floor0Height == 0)
                Floor0Enabled.IsChecked = false;
            else
                Floor0Enabled.IsChecked = true;
            TB_RoomCount.Text = $"{House.RoomCount}";
            TB_BedroomCount.Text = $"{House.BedroomCount}";
            Tb_WindowSquare.Text = $"{House.WindowSquare}";
            Tb_WindowCount.Text = $"{House.WindowCount}";

            Tb_SetFloor0BadroomCount.Text = $"{House.Floor0BadroomCount}";
            Tb_SetFloor1BadroomCount.Text = $"{House.Floor1BadroomCount}";
            Tb_SetFloor2BadroomCount.Text = $"{House.Floor2BadroomCount}";
            Tb_SetFloor3BadroomCount.Text = $"{House.Floor3BadroomCount}";


            Tb_SetFloor1Square.Text = $"{House.Floor1Square}";
            Tb_SetFloor2Square.Text = $"{House.Floor2Square}";
            Tb_SetFloor3Square.Text = $"{House.Floor3Square}";

            Tb_SetFloor1DecatativePillarsLessCount.Text = $"{House.Floor1DecatativePillarsLessCount}";
            Tb_SetFloor1DecatativePillarsOverCount.Text = $"{House.Floor1DecatativePillarsOverCount}";


            if (House.Floor3Height > 0)
                SetFloorsHouse.SelectedIndex = 2;
            else
            if (House.Floor2Height > 0)
                SetFloorsHouse.SelectedIndex = 1;
            else
                SetFloorsHouse.SelectedIndex = 0;

            foreach (ComboBoxItem item in RoofType.Items)
                if (item.Content.ToString() == House.RoofType)
                {
                    RoofType.SelectedValue = item;
                    break;
                }

            Tb_Set0FloorHeight.Text = $"{House.Floor0Height} м.";
            Tb_Set1FloorHeight.Text = $"{House.Floor1Height} м.";
            Tb_Set2FloorHeight.Text = $"{House.Floor2Height} м.";
            Tb_Set3FloorHeight.Text = $"{House.Floor3Height} м.";
            Tb_SetKitchenSquare.Text = $"{House.KitchensSquare} кв.м.";
            Tb_SetFullHouseHeight.Text = $"{House.RoofHeight} м.";
            Tb_SetRoofMinWallHeight.Text = $"{House.RoofMinWallHeight} м.";
            Tb_SetRoofSquare.Text = $"{House.RoofSquare} кв.м.";
            Tb_SetRoofLength.Text = $"{House.RoofLength} м.";
            Tb_SetCanopySquare.Text = $"{House.CanopySquare} кв.м.";
            Tb_SetCanopyLength.Text = $"{House.CanopyLength} м.";
            Tb_SetPergolaSquare.Text = $"{House.PergolaSquare} кв.м.";
            Tb_SetHemmingButt.Text = $"{House.HemmingButt} м.";
            Tb_SetHemmingOverhangsSquare.Text = $"{House.HemmingOverhangsSquare} кв.м.";
            Tb_Set0FloorF1GlaseSq.Text = $"{House.Floor0F1GlaseSq} кв.м.";
            Tb_Set0FloorF1GlaseP.Text = $"{House.Floor0F1GlaseP} м.";
            Tb_Set1FloorF1GlaseSq.Text = $"{House.Floor1F1GlaseSq} кв.м.";
            Tb_Set1FloorF1GlaseP.Text = $"{House.Floor1F1GlaseP} м.";
            Tb_Set2FloorF1GlaseSq.Text = $"{House.Floor2F1GlaseSq} кв.м.";
            Tb_Set2FloorF1GlaseP.Text = $"{House.Floor2F1GlaseP} м.";
            Tb_Set3FloorF1GlaseSq.Text = $"{House.Floor3F1GlaseSq} кв.м.";
            Tb_Set3FloorF1GlaseP.Text = $"{House.Floor3F1GlaseP} м.";
            Tb_Set0FloorF2GlaseSq.Text = $"{House.Floor0F2GlaseSq} кв.м.";
            Tb_Set0FloorF2GlaseP.Text = $"{House.Floor0F2GlaseP} м.";
            Tb_Set1FloorF2GlaseSq.Text = $"{House.Floor1F2GlaseSq} кв.м.";
            Tb_Set1FloorF2GlaseP.Text = $"{House.Floor1F2GlaseP} м.";
            Tb_Set2FloorF2GlaseSq.Text = $"{House.Floor2F2GlaseSq} кв.м.";
            Tb_Set2FloorF2GlaseP.Text = $"{House.Floor2F2GlaseP} м.";
            Tb_Set3FloorF2GlaseSq.Text = $"{House.Floor3F2GlaseSq} кв.м.";
            Tb_Set3FloorF2GlaseP.Text = $"{House.Floor3F2GlaseP} м.";
            Tb_Set0FloorF3GlaseSq.Text = $"{House.Floor0F3GlaseSq} кв.м.";
            Tb_Set0FloorF3GlaseP.Text = $"{House.Floor0F3GlaseP} м.";
            Tb_Set1FloorF3GlaseSq.Text = $"{House.Floor1F3GlaseSq} кв.м.";
            Tb_Set1FloorF3GlaseP.Text = $"{House.Floor1F3GlaseP} м.";
            Tb_Set2FloorF3GlaseSq.Text = $"{House.Floor2F3GlaseSq} кв.м.";
            Tb_Set2FloorF3GlaseP.Text = $"{House.Floor2F3GlaseP} м.";
            Tb_Set3FloorF3GlaseSq.Text = $"{House.Floor3F3GlaseSq} кв.м.";
            Tb_Set3FloorF3GlaseP.Text = $"{House.Floor3F3GlaseP} м.";
            Tb_Set0FloorF4GlaseSq.Text = $"{House.Floor0F4GlaseSq} кв.м.";
            Tb_Set0FloorF4GlaseP.Text = $"{House.Floor0F4GlaseP} м.";
            Tb_Set1FloorF4GlaseSq.Text = $"{House.Floor1F4GlaseSq} кв.м.";
            Tb_Set1FloorF4GlaseP.Text = $"{House.Floor1F4GlaseP} м.";
            Tb_Set2FloorF4GlaseSq.Text = $"{House.Floor2F4GlaseSq} кв.м.";
            Tb_Set2FloorF4GlaseP.Text = $"{House.Floor2F4GlaseP} м.";
            Tb_Set3FloorF4GlaseSq.Text = $"{House.Floor3F4GlaseSq} кв.м.";
            Tb_Set3FloorF4GlaseP.Text = $"{House.Floor3F4GlaseP} м.";
            Tb_Floor0PlinthHeight.Text = $"{House.Floor0PlinthHeight} м.";
            Tb_Floor0BadroomSquare.Text = $"{House.Floor0BadroomSquare} м.";
            Tb_Floor0TileSquare.Text = $"{House.Floor0TileSquare} м.";
            Tb_Floor0TilePerimeter.Text = $"{House.Floor0TilePerimeter} м.";
            Tb_Floor0OutWallsLength.Text = $"{House.Floor0OutWallsLength} м.";
            Tb_Floor0InnerWallsLength.Text = $"{House.Floor0InnerWallsLength} м.";
            Tb_Floor0LightWallsLength.Text = $"{House.Floor0LightWallsLength} м.";
            Tb_Floor0BreakWallsLength.Text = $"{House.Floor0BreakWallsLength} м.";
            Tb_Floor0OutDoorsLength.Text = $"{House.Floor0OutDoorsLength} м.";
            Tb_Floor0OutDoorsCount.Text = $"{House.Floor0OutDoorsCount} шт.";
            Tb_Floor0InnerDoorsLength.Text = $"{House.Floor0InnerDoorsLength} м.";
            Tb_Floor0InnerDoorsCount.Text = $"{House.Floor0InnerDoorsCount} шт.";
            Tb_Floor0PartitionsDoorsCount.Text = $"{House.Floor0PartitionsDoorsCount} шт.";
            Tb_Floor0GatesLength.Text = $"{House.Floor0GatesLength} м.";
            Tb_Floor0GatesCount.Text = $"Ворота - {House.Floor0GatesCount} шт.";
            Tb_Floor0TerassesSquare.Text = $"{House.Floor0TerassesSquare} кв.м.";
            Tb_Floor0InnerTerassesLength.Text = $"{House.Floor0InnerTerassesLength} м.";
            Tb_Floor0TerassesLength.Text = $"{House.Floor0TerassesLength} м.";
            Tb_Floor0RailingsLength.Text = $"{House.Floor0RailingsLength} м.";
            Tb_Floor1BadroomSquare.Text = $"{House.Floor1BadroomSquare} кв.м.";
            Tb_Floor1TileSquare.Text = $"{House.Floor1TileSquare} м.";
            Tb_Floor1TilePerimeter.Text = $"{House.Floor1TilePerimeter} м.";
            Tb_Floor1OutWallsLength.Text = $"{House.Floor1OutWallsLength} м.";
            Tb_Floor1InnerWallsLength.Text = $"{House.Floor1InnerWallsLength} м.";
            Tb_Floor1LightWallsLength.Text = $"{House.Floor1LightWallsLength} м.";
            Tb_Floor1BreakWallsLength.Text = $"{House.Floor1BreakWallsLength} м.";
            Tb_Floor1OutDoorsLength.Text = $"{House.Floor1OutDoorsLength} м.";
            Tb_Floor1OutDoorsCount.Text = $"{House.Floor1OutDoorsCount} шт.";
            Tb_Floor1InnerDoorsLength.Text = $"{House.Floor1InnerDoorsLength} м.";
            Tb_Floor1InnerDoorsCount.Text = $"{House.Floor1InnerDoorsCount} шт.";
            Tb_Floor1PartitionsDoorsLength.Text = $"{House.Floor1PartitionsDoorsLength} м.";
            Tb_Floor1PartitionsDoorsCount.Text = $"{House.Floor1PartitionsDoorsCount} шт.";
            Tb_Floor1GatesLength.Text = $"{House.Floor1GatesLength} м.";
            Tb_Floor1GatesCount.Text = $"Ворота - {House.Floor1GatesCount} шт.";
            Tb_Floor1TerassesSquare.Text = $"{House.Floor1TerassesSquare} кв.м.";
            Tb_Floor1InnerTerassesLength.Text = $"{House.Floor1InnerTerassesLength} м.";
            Tb_Floor1TerassesLength.Text = $"{House.Floor1TerassesLength} м.";
            Tb_Floor1RailingsLength.Text = $"{House.Floor1RailingsLength} м.";
            Tb_Floor2РHoleSecondLight.Text = $"{House.Floor2РHoleSecondLight} кв.м.";
            Tb_Floor2BadroomSquare.Text = $"{House.Floor2BadroomSquare} кв.м.";
            Tb_Floor2TileSquare.Text = $"{House.Floor2TileSquare} м.";
            Tb_Floor2TilePerimeter.Text = $"{House.Floor2TilePerimeter} м.";
            Tb_Floor2OutWallsLength.Text = $"{House.Floor2OutWallsLength} м.";
            Tb_Floor2InnerWallsLength.Text = $"{House.Floor2InnerWallsLength} м.";
            Tb_Floor2LightWallsLength.Text = $"{House.Floor2LightWallsLength} м.";
            Tb_Floor2BreakWallsLength.Text = $"{House.Floor2BreakWallsLength} м.";
            Tb_Floor2OutDoorsLength.Text = $"{House.Floor2OutDoorsLength} м.";
            Tb_Floor2OutDoorsCount.Text = $"{House.Floor2OutDoorsCount} шт.";
            Tb_Floor2InnerDoorsLength.Text = $"{House.Floor2InnerDoorsLength} м.";
            Tb_Floor2InnerDoorsCount.Text = $"{House.Floor2InnerDoorsCount} шт.";
            Tb_Floor2PartitionsDoorsLength.Text = $"{House.Floor2PartitionsDoorsLength} м.";
            Tb_Floor2PartitionsDoorsCount.Text = $"{House.Floor2PartitionsDoorsCount} шт.";
            Tb_Floor2BalconySquare.Text = $"{House.Floor2BalconySquare} кв.м.";
            Tb_Floor2BalconyLength.Text = $"{House.Floor2BalconyLength} м.";
            Tb_Floor2RailingsLength.Text = $"{House.Floor2RailingsLength} м.";
            Tb_Floor3РHoleSecondLight.Text = $"{House.Floor3РHoleSecondLight} кв.м.";
            Tb_Floor3BadroomSquare.Text = $"{House.Floor3BadroomSquare} кв.м.";
            Tb_Floor3TileSquare.Text = $"{House.Floor3TileSquare} м.";
            Tb_Floor3TilePerimeter.Text = $"{House.Floor3TilePerimeter} м.";
            Tb_Floor3OutWallsLength.Text = $"{House.Floor3OutWallsLength} м.";
            Tb_Floor3InnerWallsLength.Text = $"{House.Floor3InnerWallsLength} м.";
            Tb_Floor3LightWallsLength.Text = $"{House.Floor3LightWallsLength} м.";
            Tb_Floor3BreakWallsLength.Text = $"{House.Floor3BreakWallsLength} м.";
            Tb_Floor3OutDoorsLength.Text = $"{House.Floor3OutDoorsLength} м.";
            Tb_Floor3OutDoorsCount.Text = $"{House.Floor3OutDoorsCount} шт.";
            Tb_Floor3InnerDoorsLength.Text = $"{House.Floor3InnerDoorsLength} м.";
            Tb_Floor3InnerDoorsCount.Text = $"{House.Floor3InnerDoorsCount} шт.";
            Tb_Floor3PartitionsDoorsLength.Text = $"{House.Floor3PartitionsDoorsLength} м.";
            Tb_Floor3PartitionsDoorsCount.Text = $"{House.Floor3PartitionsDoorsCount} шт.";
            Tb_Floor3BalconySquare.Text = $"{House.Floor3BalconySquare} кв.м.";
            Tb_Floor3BalconyLength.Text = $"{House.Floor3BalconyLength} м.";
            Tb_Floor3RailingsLength.Text = $"{House.Floor3RailingsLength} м.";


        }
        private void Btn_SetRange_Click(object sender, RoutedEventArgs e)
        {
            tempShapeType = ShapeType;
            Tb_Information.Text = "Проведите контрольную линию";
            if (string.IsNullOrWhiteSpace(TB_RealLength.Text) || TB_RealLength.Text == "0")
            {
                MessageBox.Show("Не указана устанавливающая длина", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            isControlLine = true;
            ShapeType = (byte)GlobalVariables.ShapeTypeEnum.Line;
        }
        private void ShowProjDataToExcel(object sender, RoutedEventArgs e)
        {
            Excel.ShowProjectData();
        }
        private void ClearSelectedLayout()
        {
            selectedCanvas.Children.Clear();
            if (Shapes.Count > 0)
            {
                Shapes.RemoveAll(s => s.ParrentCanvasName == selectedCanvas.Name);
               
            }
            ResetSelectedCanvasStatistic();

        }
        private void ResetSelectedCanvasStatistic()
        {
            if (selectedCanvas == HeighLayout)
            {
                House.Floor0Height = 0;
                Tb_Set0FloorHeight.Text = "0.00 м";
                House.Floor1Height = 0;
                Tb_Set1FloorHeight.Text = "0.00 м";
                House.Floor2Height = 0;
                Tb_Set2FloorHeight.Text = "0.00 м";
                House.Floor3Height = 0;
                Tb_Set3FloorHeight.Text = "0.00 м";

                House.RoofHeight = 0;
                Tb_SetFullHouseHeight.Text = "0.00 м.";

                House.RoofMinWallHeight = 0;
                Tb_SetRoofMinWallHeight.Text = "0.00 м.";

                House.Floor1DecatativePillarsLessCount = 0;
                Tb_SetFloor1DecatativePillarsLessCount.Text = "";
                House.Floor1DecatativePillarsOverCount = 0;
                Tb_SetFloor1DecatativePillarsOverCount.Text = "";
            }
            if (selectedCanvas == RoomsLayout)
            {
                House.RoomCount = 0;
                TB_RoomCount.Text = "";
                House.BedroomCount = 0;
                TB_BedroomCount.Text = "";
                House.KitchensSquare = 0;
                Tb_SetKitchenSquare.Text = "0.00 кв.м.";
            }
            if (selectedCanvas == RoofLayout)
            {
                House.RoofType = "";
                RoofType.SelectedIndex = -1;

                House.RoofSquare = 0;
                Tb_SetRoofSquare.Text = "0.00 кв.м.";
                House.RoofLength = 0;
                Tb_SetRoofLength.Text = "0.00 м.";
                House.CanopySquare = 0;
                Tb_SetCanopySquare.Text = "0.00 кв.м.";
                House.CanopyLength = 0;
                Tb_SetCanopyLength.Text = "0.00 м.";
                House.PergolaSquare = 0;
                Tb_SetPergolaSquare.Text = "0.00 кв.м.";
            }
            if (selectedCanvas == HemmingLayout)
            {
                House.HemmingButt = 0;
                Tb_SetHemmingButt.Text = "0.00 м.";
                House.HemmingOverhangsSquare = 0;
                Tb_SetHemmingOverhangsSquare.Text = "0.00 м.";
            }
            if (selectedCanvas == Fasade0Layout)
            {
                House.Floor0F1GlaseP = 0;
                House.Floor1F1GlaseP = 0;
                House.Floor2F1GlaseP = 0;
                House.Floor3F1GlaseP = 0;
                House.Floor0F1GlaseSq = 0;
                House.Floor1F1GlaseSq = 0;
                House.Floor2F1GlaseSq = 0;
                House.Floor3F1GlaseSq = 0;

                Tb_Set0FloorF1GlaseSq.Text = "0.00 кв.м.";
                Tb_Set0FloorF1GlaseP.Text = "0.00 м.";
                Tb_Set1FloorF1GlaseSq.Text = "0.00 кв.м.";
                Tb_Set1FloorF1GlaseP.Text = "0.00 м.";
                Tb_Set2FloorF1GlaseSq.Text = "0.00 кв.м.";
                Tb_Set2FloorF1GlaseP.Text = "0.00 м.";
                Tb_Set3FloorF1GlaseSq.Text = "0.00 кв.м.";
                Tb_Set3FloorF1GlaseP.Text = "0.00 м.";
            }
            if (selectedCanvas == Fasade1Layout)
            {
                House.Floor0F2GlaseP = 0;
                House.Floor1F2GlaseP = 0;
                House.Floor2F2GlaseP = 0;
                House.Floor3F2GlaseP = 0;
                House.Floor0F2GlaseSq = 0;
                House.Floor1F2GlaseSq = 0;
                House.Floor2F2GlaseSq = 0;
                House.Floor3F2GlaseSq = 0;

                Tb_Set0FloorF2GlaseSq.Text = "0.00 кв.м.";
                Tb_Set0FloorF2GlaseP.Text = "0.00 м.";
                Tb_Set1FloorF2GlaseSq.Text = "0.00 кв.м.";
                Tb_Set1FloorF2GlaseP.Text = "0.00 м.";
                Tb_Set2FloorF2GlaseSq.Text = "0.00 кв.м.";
                Tb_Set2FloorF2GlaseP.Text = "0.00 м.";
                Tb_Set3FloorF2GlaseSq.Text = "0.00 кв.м.";
                Tb_Set3FloorF2GlaseP.Text = "0.00 м.";
            }
            if (selectedCanvas == Fasade2Layout)
            {
                House.Floor0F3GlaseP = 0;
                House.Floor1F3GlaseP = 0;
                House.Floor2F3GlaseP = 0;
                House.Floor3F3GlaseP = 0;
                House.Floor0F3GlaseSq = 0;
                House.Floor1F3GlaseSq = 0;
                House.Floor2F3GlaseSq = 0;
                House.Floor3F3GlaseSq = 0;

                Tb_Set0FloorF3GlaseSq.Text = "0.00 кв.м.";
                Tb_Set0FloorF3GlaseP.Text = "0.00 м.";
                Tb_Set1FloorF3GlaseSq.Text = "0.00 кв.м.";
                Tb_Set1FloorF3GlaseP.Text = "0.00 м.";
                Tb_Set2FloorF3GlaseSq.Text = "0.00 кв.м.";
                Tb_Set2FloorF3GlaseP.Text = "0.00 м.";
                Tb_Set3FloorF3GlaseSq.Text = "0.00 кв.м.";
                Tb_Set3FloorF3GlaseP.Text = "0.00 м.";
            }
            if (selectedCanvas == Fasade3Layout)
            {
                House.Floor0F4GlaseP = 0;
                House.Floor1F4GlaseP = 0;
                House.Floor2F4GlaseP = 0;
                House.Floor3F4GlaseP = 0;
                House.Floor0F4GlaseSq = 0;
                House.Floor1F4GlaseSq = 0;
                House.Floor2F4GlaseSq = 0;
                House.Floor3F4GlaseSq = 0;

                Tb_Set0FloorF4GlaseSq.Text = "0.00 кв.м.";
                Tb_Set0FloorF4GlaseP.Text = "0.00 м.";
                Tb_Set1FloorF4GlaseSq.Text = "0.00 кв.м.";
                Tb_Set1FloorF4GlaseP.Text = "0.00 м.";
                Tb_Set2FloorF4GlaseSq.Text = "0.00 кв.м.";
                Tb_Set2FloorF4GlaseP.Text = "0.00 м.";
                Tb_Set3FloorF4GlaseSq.Text = "0.00 кв.м.";
                Tb_Set3FloorF4GlaseP.Text = "0.00 м.";
            }
            if (selectedCanvas == Floor0ODLayout)
            {
                House.Floor0Square = 0;
                Tb_Set0FloorSquare.Text = "";
                House.Floor0PlinthHeight = 0;
                Tb_Floor0PlinthHeight.Text = "0.00 м.";
                House.PlinthOpenPerc = 0;
                PlinthOpenPerc.Value = 0;
                House.Floor0BadroomCount = 0;
                Tb_SetFloor0BadroomCount.Text = "";
                House.Floor0BadroomSquare = 0;
                Tb_Floor0BadroomSquare.Text = "0.00 кв.м.";
                House.Floor0TileSquare = 0;
                Tb_Floor0TileSquare.Text = "0.00 м.";
                House.Floor0TilePerimeter = 0;
                Tb_Floor0TilePerimeter.Text = "0 м.";
            }
            if (selectedCanvas == Floor0DoorsLayout)
            {
                House.Floor0OutWallsLength = 0;
                Tb_Floor0OutWallsLength.Text = "0.00 м.";
                House.Floor0InnerWallsLength = 0;
                Tb_Floor0InnerWallsLength.Text = "0.00 м.";
                House.Floor0LightWallsLength = 0;
                Tb_Floor0LightWallsLength.Text = "0.00 м.";
                House.Floor0BreakWallsLength = 0;
                Tb_Floor0BreakWallsLength.Text = "0.00 м.";
                House.Floor0OutDoorsLength = 0;
                Tb_Floor0OutDoorsLength.Text = "0.00 м.";
                House.Floor0OutDoorsCount = 0;
                Tb_Floor0OutDoorsCount.Text = "0 шт.";
                House.Floor0InnerDoorsLength = 0;
                Tb_Floor0InnerDoorsLength.Text = "0.00 м.";
                House.Floor0InnerDoorsCount = 0;
                Tb_Floor0InnerDoorsCount.Text = "0 шт.";
                House.Floor0PartitionsDoorsLength = 0;
                Tb_Floor0PartitionsDoorsLength.Text = "0.00 м.";
                House.Floor0PartitionsDoorsCount = 0;
                Tb_Floor0PartitionsDoorsCount.Text = "0 шт.";
                House.Floor0GatesLength = 0;
                Tb_Floor0GatesLength.Text = "0.00 м.";
                House.Floor0GatesCount = 0;
                Tb_Floor0GatesCount.Text = "0 шт.";
            }
            if (selectedCanvas == Floor0TerasesLayout)
            {
                House.Floor0TerassesSquare = 0;
                Tb_Floor0TerassesSquare.Text = "0.00 кв.м.";
                House.Floor0InnerTerassesLength = 0;
                Tb_Floor0InnerTerassesLength.Text = "0.00 м.";
                House.Floor0TerassesSquare = 0;
                Tb_Floor0TerassesLength.Text = "0.00 м.";
                House.Floor0RailingsLength = 0;
                Tb_Floor0RailingsLength.Text = "0.00 м.";
            }
            if (selectedCanvas == Floor1ODLayout)
            {
                House.Floor1Square = 0;
                Tb_SetFloor1Square.Text = "";
                House.Floor1BadroomCount = 0;
                Tb_SetFloor1BadroomCount.Text = "";
                House.Floor1BadroomSquare = 0;
                Tb_Floor1BadroomSquare.Text = "0.00 кв.м.";
                House.Floor1TileSquare = 0;
                Tb_Floor1TileSquare.Text = "0.00 м.";
                House.Floor1TilePerimeter = 0;
                Tb_Floor1TilePerimeter.Text = "0 м.";
            }
            if (selectedCanvas == Floor1DoorsLayout)
            {
                House.Floor1OutWallsLength = 0;
                Tb_Floor1OutWallsLength.Text = "0.00 м.";
                House.Floor1InnerWallsLength = 0;
                Tb_Floor1InnerWallsLength.Text = "0.00 м.";
                House.Floor1LightWallsLength = 0;
                Tb_Floor1LightWallsLength.Text = "0.00 м.";
                House.Floor1BreakWallsLength = 0;
                Tb_Floor1BreakWallsLength.Text = "0.00 м.";
                House.Floor1OutDoorsLength = 0;
                Tb_Floor1OutDoorsLength.Text = "0.00 м.";
                House.Floor1OutDoorsCount = 0;
                Tb_Floor1OutDoorsCount.Text = "0 шт.";

                House.Floor1InnerDoorsLength = 0;
                Tb_Floor1InnerDoorsLength.Text = "0.00 м.";
                House.Floor1InnerDoorsCount = 0;
                Tb_Floor1InnerDoorsCount.Text = "0 шт.";
                House.Floor1PartitionsDoorsLength = 0;
                Tb_Floor1PartitionsDoorsLength.Text = "0.00 м.";
                House.Floor1PartitionsDoorsCount = 0;
                Tb_Floor1PartitionsDoorsCount.Text = "0 шт.";
                House.Floor1GatesLength = 0;
                Tb_Floor1GatesLength.Text = "0.00 м.";
                House.Floor1GatesCount = 0;
                Tb_Floor1GatesCount.Text = "0 шт.";
            }
            if (selectedCanvas == Floor1TerasesLayout)
            {
                House.Floor1TerassesSquare = 0;
                Tb_Floor1TerassesSquare.Text = "0.00 кв.м.";
                House.Floor1InnerTerassesLength = 0;
                Tb_Floor1InnerTerassesLength.Text = "0.00 м.";
                House.Floor1TerassesLength = 0;
                Tb_Floor1TerassesLength.Text = "0.00 м.";
                House.Floor1RailingsLength = 0;
                Tb_Floor1RailingsLength.Text = "0.00 м.";

            }
            if (selectedCanvas == Floor2ODLayout)
            {
                House.Floor2Square = 0;
                Tb_SetFloor2Square.Text = "";
                House.Floor2BadroomCount = 0;
                Tb_SetFloor2BadroomCount.Text = "";
                House.Floor2РHoleSecondLight = 0;
                Tb_Floor2РHoleSecondLight.Text = "0.00 кв.м.";
                House.Floor2BadroomSquare = 0;
                Tb_Floor2BadroomSquare.Text = "0.00 кв.м.";
                House.Floor2TileSquare = 0;
                Tb_Floor2TileSquare.Text = "0.00 м.";
                House.Floor2TilePerimeter = 0;
                Tb_Floor2TilePerimeter.Text = "0 м.";
            }
            if (selectedCanvas == Floor2DoorsLayout)
            {
                House.Floor2OutWallsLength = 0;
                Tb_Floor2OutWallsLength.Text = "0.00 м.";
                House.Floor2InnerWallsLength = 0;
                Tb_Floor2InnerWallsLength.Text = "0.00 м.";
                House.Floor2LightWallsLength = 0;
                Tb_Floor2LightWallsLength.Text = "0.00 м.";
                House.Floor2BreakWallsLength = 0;
                Tb_Floor2BreakWallsLength.Text = "0.00 м.";
                House.Floor2OutDoorsLength = 0;
                Tb_Floor2OutDoorsLength.Text = "0.00 м.";
                House.Floor2OutDoorsCount = 0;
                Tb_Floor2OutDoorsCount.Text = "0 шт.";
                House.Floor2InnerDoorsLength = 0;
                Tb_Floor2InnerDoorsLength.Text = "0.00 м.";
                House.Floor2InnerDoorsCount = 0;
                Tb_Floor2InnerDoorsCount.Text = "0 шт.";
                House.Floor2PartitionsDoorsLength = 0;
                Tb_Floor2PartitionsDoorsLength.Text = "0.00 м.";
                House.Floor2PartitionsDoorsCount = 0;
                Tb_Floor2PartitionsDoorsCount.Text = "0 шт.";
            }
            if (selectedCanvas == Floor2BalconyLayout)
            {
                House.Floor2BadroomSquare = 0;
                Tb_Floor2BalconySquare.Text = "0.00 кв.м.";
                House.Floor2BalconyLength = 0;
                Tb_Floor2BalconyLength.Text = "0.00 м.";
                House.Floor2RailingsLength = 0;
                Tb_Floor2RailingsLength.Text = "0.00 м.";
            }
            if (selectedCanvas == Floor3ODLayout)
            {
                House.Floor3Square = 0;
                Tb_SetFloor3Square.Text = "";
                House.Floor3BadroomCount = 0;
                Tb_SetFloor3BadroomCount.Text = "";
                House.Floor3РHoleSecondLight = 0;
                Tb_Floor3РHoleSecondLight.Text = "0.00 кв.м.";
                House.Floor3BadroomSquare = 0;
                Tb_Floor3BadroomSquare.Text = "0.00 кв.м.";
                House.Floor3TileSquare = 0;
                Tb_Floor3TileSquare.Text = "0.00 м.";
                House.Floor3TilePerimeter = 0;
                Tb_Floor3TilePerimeter.Text = "0 м.";
            }
            if (selectedCanvas == Floor3DoorsLayout)
            {
                House.Floor3OutWallsLength = 0;
                Tb_Floor3OutWallsLength.Text = "0.00 м.";
                House.Floor3InnerWallsLength = 0;
                Tb_Floor3InnerWallsLength.Text = "0.00 м.";
                House.Floor3LightWallsLength = 0;
                Tb_Floor3LightWallsLength.Text = "0.00 м.";
                House.Floor3BreakWallsLength = 0;
                Tb_Floor3BreakWallsLength.Text = "0.00 м.";
                House.Floor3OutDoorsLength = 0;
                Tb_Floor3OutDoorsLength.Text = "0.00 м.";
                House.Floor3OutDoorsCount = 0;
                Tb_Floor3OutDoorsCount.Text = "0 шт.";
                House.Floor3InnerDoorsLength = 0;
                Tb_Floor3InnerDoorsLength.Text = "0.00 м.";
                House.Floor3InnerDoorsCount = 0;
                Tb_Floor3InnerDoorsCount.Text = "0 шт.";
                House.Floor3PartitionsDoorsLength = 0;
                Tb_Floor3PartitionsDoorsLength.Text = "0.00 м.";
                House.Floor3PartitionsDoorsCount = 0;
                Tb_Floor3PartitionsDoorsCount.Text = "0 шт.";
            }
            if (selectedCanvas == Floor3BalconyLayout)
            {
                House.Floor3BalconySquare = 0;
                Tb_Floor3BalconySquare.Text = "0.00 кв.м.";
                House.Floor3BalconyLength = 0;
                Tb_Floor3BalconyLength.Text = "0.00 м.";
                House.Floor3RailingsLength = 0;
                Tb_Floor3RailingsLength.Text = "0.00 м.";
            }
        }
        private void Btn_DeleteLastLine_Click(object sender, RoutedEventArgs e)
        {
            DeleteLastShape();
        }

        #region Работа с Google
        private async void SendDataToGoogleAsync(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate { Btn_SaveData.IsEnabled = false; }));

                if (House.ProjectName != null)
                {
                    GoogleSheets.SaveData();
                }

                Dispatcher.BeginInvoke(new ThreadStart(delegate { Btn_SaveData.IsEnabled = true; }));

            });
        }
        private async void LoadProjectDataAsync(object sender, RoutedEventArgs e)
        {
            // TODO : включить, как допишу присвоение
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate { Btn_LoadData.IsEnabled = false; }));
                if (House.ProjectName != null)
                {
                    GoogleSheets.LoadData();

                }
                Dispatcher.BeginInvoke(new ThreadStart(delegate { Btn_LoadData.IsEnabled = true; }));

            });
            LoadForm();
        }
        #endregion

        #region Установка типа выбранной геометрической фигуры
        /// <summary>
        /// Выбрать линейный объект
        /// </summary>
        /// <param name="buidObj">Тип объекта</param>
        private void SelectLineObj(byte buidObj)
        {
            ShapeType = (byte)GlobalVariables.ShapeTypeEnum.Line;
            if (SelectedBuidingObj != buidObj)
            {
                SelectedBuidingObj = buidObj;
            }
            isPainting = true;
        }
        private void SelectRectObj(byte buidObj)
        {
            ShapeType = (byte)GlobalVariables.ShapeTypeEnum.Rect;
            if (SelectedBuidingObj != buidObj)
            {
                //   ClearAllMainLine();
                SelectedBuidingObj = buidObj;
            }
            isPainting = true;
        }
        private void SelectTriangleObj(byte buidObj)
        {
            ShapeType = (byte)GlobalVariables.ShapeTypeEnum.Triangle;
            if (SelectedBuidingObj != buidObj)
            {
                //   ClearAllMainLine();
                SelectedBuidingObj = buidObj;
            }
            isPainting = true;
        }
        #endregion
        #region Ввод данных
        private void TB_ProjectName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TB_ProjectName.Text.Length > 0)
                House.ProjectName = "AS-" + TB_ProjectName.Text;
        }
        private void TB_UserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TB_UserName.Text.Length > 0)
                House.ManagerName = TB_UserName.Text;
        }
        private void TB_RoomCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TB_RoomCount.Text.Length > 0)
                House.RoomCount = int.Parse(TB_RoomCount.Text);
        }
        private void RoofType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)RoofType.SelectedItem;
            if (typeItem == null) return;
            House.RoofType = typeItem.Content.ToString();
        }
        private void TB_BedroomCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TB_BedroomCount.Text.Length > 0)
                House.BedroomCount = int.Parse(TB_BedroomCount.Text);
        }
        private void PlinthOpenPerc_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)PlinthOpenPerc.Value;
            TB_PlinthOpenPerc.Text = $"Сбоку цоколь открыт на {value}% (дом на склоне)";
            House.PlinthOpenPerc = value;
        }
        private void Tb_WindowCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Tb_WindowCount.Text.Length > 0)
                House.WindowCount = int.Parse(Tb_WindowCount.Text);
        }
        private void Tb_WindowSquare_TextChanged(object sender, TextChangedEventArgs e)
        {
            string value = (sender as TextBox).Text;
            value = value.Replace(" ", "");
            value = value.Replace('.', ',').Trim();
            // Если последний символ запятая, то добавляем ноль
            if (value.Length == 0)
                value = "0";

            double result = double.Parse(value);

            House.WindowSquare = result;
        }
        private void Tb_Set0FloorSquare_TextChanged(object sender, TextChangedEventArgs e)
        {

            string value = (sender as TextBox).Text;
            value = value.Replace(" ", "");
            value = value.Replace('.', ',').Trim();
            // Если последний символ запятая, то добавляем ноль
            if (value.Length == 0)
                value = "0";

            double result = double.Parse(value);

            House.Floor0Square = result;
        }
        private void Tb_SetFloor0BadroomCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Tb_SetFloor0BadroomCount.Text.Length > 0)
                House.Floor0BadroomCount = int.Parse(Tb_SetFloor0BadroomCount.Text);
        }
        private void Tb_SetFloor1Square_TextChanged(object sender, TextChangedEventArgs e)
        {

            string value = (sender as TextBox).Text;
            value = value.Replace(" ", "");
            value = value.Replace('.', ',').Trim();
            double result = 0.0;
            // Если последний символ запятая, то добавляем ноль
            if (value.Length == 0)
                value = "0";

            result = double.Parse(value);

            House.Floor1Square = result;
        }
        private void Tb_SetFloor1BadroomCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Tb_SetFloor1BadroomCount.Text.Length > 0)
                House.Floor1BadroomCount = int.Parse(Tb_SetFloor1BadroomCount.Text);
        }
        private void Tb_SetFloor1DecatativePillarsLessCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Tb_SetFloor1DecatativePillarsLessCount.Text.Length > 0)
                House.Floor1DecatativePillarsLessCount = int.Parse(Tb_SetFloor1DecatativePillarsLessCount.Text);
        }
        private void Tb_SetFloor1DecatativePillarsOverCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Tb_SetFloor1DecatativePillarsOverCount.Text.Length > 0)
                House.Floor1DecatativePillarsOverCount = int.Parse(Tb_SetFloor1DecatativePillarsOverCount.Text);
        }
        private void Tb_SetFloor2Square_TextChanged(object sender, TextChangedEventArgs e)
        {
            string value = (sender as TextBox).Text;
            value = value.Replace(" ", "");
            value = value.Replace('.', ',').Trim();
            // Если последний символ запятая, то добавляем ноль
            if (value.Length == 0)
                value = "0";

            double result = double.Parse(value);

            House.Floor2Square = result;
        }
        private void Tb_SetFloor2BadroomCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Tb_SetFloor2BadroomCount.Text.Length > 0)
                House.Floor2BadroomCount = int.Parse(Tb_SetFloor2BadroomCount.Text);
        }
        private void Tb_SetFloor3Square_TextChanged(object sender, TextChangedEventArgs e)
        {
            string value = (sender as TextBox).Text;
            value = value.Replace(" ", "");
            value = value.Replace('.', ',').Trim();
            // Если последний символ запятая, то добавляем ноль
            if (value.Length == 0)
                value = "0";

            double result = double.Parse(value);

            House.Floor3Square = result;
        }
        private void Tb_SetFloor3BadroomCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Tb_SetFloor3BadroomCount.Text.Length > 0)
                House.Floor3BadroomCount = int.Parse(Tb_SetFloor3BadroomCount.Text);
        }

        private void CalcFloorsCount()
        {
            int count = 0;
            if (Floor0Enabled.IsChecked == true)
            {
                count++;
            }
            switch (SetFloorsHouse.SelectedIndex)
            {
                case 0:
                    count++;
                    break;
                case 1:
                    count += 2;
                    break;
                case 2:
                    count += 3;
                    break;
            }
            House.FloorsCount = count;
        }

        #endregion
        #region Выбор объектов вкладки "Общие"
        /// <summary>
        /// Смена цвета тексблока при выборе объекта
        /// </summary>
        /// <param name="sender">Кнопка</param>
        private void ChangeSelectedBlockColor(object sender)
        {
            // Сброс всех
            foreach (var tb in FindVisualChildren<TextBlock>(window))
            {
                tb.Foreground = new SolidColorBrush(Colors.Black);
            }

            FrameworkElement parent = (FrameworkElement)((Button)sender).Parent;
            foreach (var tb in FindVisualChildren<TextBlock>(parent))
            {
                tb.Foreground = new SolidColorBrush(Colors.DarkOrange);
            }
        }
        private void Btn_Set0FloorHeight_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Высота цокольного этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetHeighSetupLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0Height);
        }
        private void Btn_Set1FloorHeight_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Высота первого этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetHeighSetupLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1Height);
        }
        private void Btn_Set2FloorHeight_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Высота второго этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetHeighSetupLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2Height);
        }
        private void Btn_Set3FloorHeight_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Высота третьего этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetHeighSetupLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3Height);
        }
        private void Btn_SetKitchenSquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Площадь кухонь и гостиных";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetRoomsLayout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.KitchensSquare);
        }
        private void Btn_SetFullHouseHeight_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Высота от пола верхнего этажа до конька";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetHeighSetupLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.RoofHeight);
        }
        private void Btn_SetRoofMinWallHeight_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Минимальная высота стен верхнего этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetHeighSetupLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.RoofMinWallHeight);
        }
        private void Btn_SetRoofSquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Площадь, накрытая основной кровлей";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetRoofLayout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.RoofSquare);
        }
        private void Btn_SetRoofLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Длина основной кровли";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetRoofLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.RoofLength);
        }
        private void Btn_SetCanopySquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Площадь, накрытая навесами";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetRoofLayout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.CanopySquare);
        }
        private void Btn_SetCanopyLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Длина навесов вдоль стены";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetRoofLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.CanopyLength);
        }
        private void Btn_SetPergolaSquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Площадь Перголы";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetRoofLayout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.PergolaSquare);
        }
        private void Btn_SetHemmingButt_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Подшива торцов основной кровли и навесов";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetHemmingLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.HemmingButt);
        }
        private void Btn_SetHemmingOverhangsSquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Подшива свесов основной кровли снизу";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetHemmingLayout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.HemmingOverhangsSquare);
        }
        #endregion
        #region Выбор объектов вкладки "Фасады"
        private void Btn_Set0FloorF1GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна цокольного этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade0Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor0F1GlaseSq);
        }
        private void Btn_Set1FloorF1GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна первого этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade0Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor1F1GlaseSq);
        }
        private void Btn_Set2FloorF1GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна второго этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade0Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor2F1GlaseSq);
        }
        private void Btn_Set3FloorF1GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна третьего этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade0Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor3F1GlaseSq);
        }
        private void Btn_Set0FloorF2GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна цокольного этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade1Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor0F2GlaseSq);
        }
        private void Btn_Set1FloorF2GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна первого этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade1Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor1F2GlaseSq);
        }
        private void Btn_Set2FloorF2GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна второго этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade1Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor2F2GlaseSq);
        }
        private void Btn_Set3FloorF2GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна третьего этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade1Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor3F2GlaseSq);
        }
        private void Btn_Set0FloorF3GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна цокольного этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade2Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor0F3GlaseSq);
        }
        private void Btn_Set1FloorF3GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна первого этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade2Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor1F3GlaseSq);
        }
        private void Btn_Set2FloorF3GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна второго этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade2Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor2F3GlaseSq);
        }
        private void Btn_Set3FloorF3GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна третьего этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade2Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor3F3GlaseSq);
        }
        private void Btn_Set0FloorF4GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна цокольного этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade3Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor0F4GlaseSq);
        }
        private void Btn_Set1FloorF4GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна первого этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade3Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor1F4GlaseSq);
        }
        private void Btn_Set2FloorF4GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна второго этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade3Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor2F4GlaseSq);
        }
        private void Btn_Set3FloorF4GlaseT_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Треугольные окна третьего этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade3Layout.IsChecked = true;
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor3F4GlaseSq);
        }

        private void Btn_Set0FloorF1GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна цокольного этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade0Layout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor0F1GlaseSq);
        }
        private void Btn_Set1FloorF1GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна первого этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade0Layout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor1F1GlaseSq);
        }
        private void Btn_Set2FloorF1GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна второго этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade0Layout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor2F1GlaseSq);
        }
        private void Btn_Set3FloorF1GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна третьего этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade0Layout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor3F1GlaseSq);
        }
        private void Btn_Set0FloorF2GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна цокольного этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade1Layout.IsChecked = true;
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor0F2GlaseSq);
        }
        private void Btn_Set1FloorF2GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна первого этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade1Layout.IsChecked = true;
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor1F2GlaseSq);
        }
        private void Btn_Set2FloorF2GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна второго этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade1Layout.IsChecked = true;
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor2F2GlaseSq);
        }
        private void Btn_Set3FloorF2GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна третьего этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade1Layout.IsChecked = true;
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor3F2GlaseSq);
        }
        private void Btn_Set0FloorF3GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна цокольного этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade2Layout.IsChecked = true;
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor0F3GlaseSq);
        }
        private void Btn_Set1FloorF3GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна первого этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade2Layout.IsChecked = true;
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor1F3GlaseSq);
        }
        private void Btn_Set2FloorF3GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна второго этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade2Layout.IsChecked = true;
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor2F3GlaseSq);
        }
        private void Btn_Set3FloorF3GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна третьего этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade2Layout.IsChecked = true;
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor3F3GlaseSq);
        }
        private void Btn_Set0FloorF4GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна цокольного этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade3Layout.IsChecked = true;
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor0F4GlaseSq);
        }
        private void Btn_Set1FloorF4GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна первого этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade3Layout.IsChecked = true;
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor1F4GlaseSq);
        }
        private void Btn_Set2FloorF4GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна второго этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade3Layout.IsChecked = true;
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor2F4GlaseSq);
        }
        private void Btn_Set3FloorF4GlaseQ_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прямоугольные окна третьего этажа";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFasade3Layout.IsChecked = true;
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor3F4GlaseSq);
        }
        #endregion
        #region Выбор объектов вкладки "Этажи"
        private void Btn_SetFloor0PlinthHeight_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Высота цоколя над землей до плиты перекрытия";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0ODLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0PlinthHeight);
        }
        private void Btn_SetFloor0BadroomSquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Площадь туалетов и ванных комнтат";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0ODLayout.IsChecked = true;
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor0BadroomSquare);
        }
        private void Btn_SetFloor0TileSquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прочие помещения в кафеле";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0ODLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0TileSquare);
        }
        private void Btn_SetFloor0OutWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Внешние несущие стены";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0DoorsLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0OutWallsLength);
        }
        private void Btn_SetFloor0InnerWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Внутренние несущие стены";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0DoorsLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0InnerWallsLength);
        }
        private void Btn_SetFloor0LightWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Перегородки";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0DoorsLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0LightWallsLength);
        }
        private void Btn_SetFloor0BreakWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Разрывы несущих стен более 2 метров";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0DoorsLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0BreakWallsLength);
        }
        private void Btn_SetFloor0OutDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Двери металлические в несущих конструкциях (снаружи)";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0DoorsLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0OutDoorsLength);
        }
        private void Btn_SetFloor0InnerDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Двери межкомнатные в несущих конструкциях (внутри)";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0DoorsLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0InnerDoorsLength);
        }
        private void Btn_SetFloor0PartitionsDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Двери межкомнатные в перегородках";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0DoorsLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0PartitionsDoorsLength);
        }
        private void Btn_SetFloor0GatesLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Ворота";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0DoorsLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0GatesLength);
        }
        private void Btn_SetFloor0TerassesSquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Площадь терасс и крылец";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0TerasesLayout.IsChecked = true;
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor0TerassesSquare);
        }
        private void Btn_SetFloor0InnerTerassesLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Длина внешних терасс и крылец";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0TerasesLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0InnerTerassesLength);
        }
        private void Btn_SetFloor0TerassesLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Длина терасс и крылец";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0TerasesLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0TerassesLength);
        }
        private void Btn_SetFloor0RailingsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Длина перил и ораждений";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0TerasesLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0RailingsLength);
        }

        private void Btn_SetFloor1BadroomSquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Площадь туалетов и ванных комнтат";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1ODLayout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor1BadroomSquare);
        }
        private void Btn_SetFloor1TileSquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прочие помещения в кафеле";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1ODLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1TileSquare);
        }
        private void Btn_SetFloor1OutWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Внешние несущие стены";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1OutWallsLength);
        }
        private void Btn_SetFloor1InnerWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Внутренние несущие стены";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1InnerWallsLength);
        }
        private void Btn_SetFloor1LightWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Перегородки";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1LightWallsLength);
        }
        private void Btn_SetFloor1BreakWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Разрывы несущих стен более 2 метров";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1BreakWallsLength);
        }
        private void Btn_SetFloor1OutDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Двери металлические в несущих конструкциях (снаружи)";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1OutDoorsLength);
        }
        private void Btn_SetFloor1InnerDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Двери межкомнатные в несущих конструкциях (внутри)";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1InnerDoorsLength);
        }
        private void Btn_SetFloor1PartitionsDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Двери межкомнатные в перегородках";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1PartitionsDoorsLength);
        }
        private void Btn_SetFloor1GatesLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Ворота";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1GatesLength);
        }
        private void Btn_SetFloor1TerassesSquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Площадь терасс и крылец";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1TerasesLayout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor1TerassesSquare);
        }
        private void Btn_SetFloor1InnerTerassesLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Длина внешних терасс и крылец";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1TerasesLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1InnerTerassesLength);
        }
        private void Btn_SetFloor1TerassesLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Длина терасс и крылец";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1TerasesLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1TerassesLength);
        }
        private void Btn_SetFloor1RailingsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Длина перил и ораждений";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1TerasesLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1RailingsLength);
        }

        private void Btn_SetFloor2РHoleSecondLight_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Отверстие в полу под второй свет";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor2ODLayout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor2РHoleSecondLight);
        }
        private void Btn_SetFloor2BadroomSquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Площадь туалетов и ванных комнтат";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor2ODLayout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor2BadroomSquare);
        }
        private void Btn_SetFloor2TileSquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прочие помещения в кафеле";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor2ODLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2TileSquare);
        }
        private void Btn_SetFloor2OutWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Внешние несущие стены";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor2DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2OutWallsLength);
        }
        private void Btn_SetFloor2InnerWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Внутренние несущие стены";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor2DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2InnerWallsLength);
        }
        private void Btn_SetFloor2LightWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Перегородки";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor2DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2LightWallsLength);
        }
        private void Btn_SetFloor2BreakWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Разрывы несущих стен более 2 метров";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor2DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2BreakWallsLength);
        }
        private void Btn_SetFloor2OutDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Двери металлические в несущих конструкциях (снаружи)";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor2DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2OutDoorsLength);
        }
        private void Btn_SetFloor2InnerDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Двери межкомнатные в несущих конструкциях (внутри)";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor2DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2InnerDoorsLength);
        }
        private void Btn_SetFloor2PartitionsDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Двери межкомнатные в перегородках";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor2DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2PartitionsDoorsLength);
        }
        private void Btn_SetFloor2BalconySquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Площадь балконов";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor2BalconyLayout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor2BalconySquare);
        }
        private void Btn_SetFloor2BalconyLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Длина балконов";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor2BalconyLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2BalconyLength);
        }
        private void Btn_SetFloor2RailingsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Перила и ораждения";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor2BalconyLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2RailingsLength);
        }

        private void Btn_SetFloor3РHoleSecondLight_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Отверстие в полу под второй свет";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor3ODLayout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor3РHoleSecondLight);
        }
        private void Btn_SetFloor3BadroomSquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Площадь туалетов и ванных комнтат";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor3ODLayout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor3BadroomSquare);
        }
        private void Btn_SetFloor3TileSquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Прочие помещения в кафеле";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor3ODLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3TileSquare);
        }
        private void Btn_SetFloor3OutWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Внешние несущие стены";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor3DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3OutWallsLength);
        }
        private void Btn_SetFloor3InnerWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Внутренние несущие стены";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor3DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3InnerWallsLength);
        }
        private void Btn_SetFloor3LightWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Перегородки";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor3DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3LightWallsLength);
        }
        private void Btn_SetFloor3BreakWallsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Разрывы несущих стен более 2 метров";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor3DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3BreakWallsLength);
        }
        private void Btn_SetFloor3OutDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Двери металлические в несущих конструкциях (снаружи)";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor3DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3OutDoorsLength);
        }
        private void Btn_SetFloor3InnerDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Двери межкомнатные в несущих конструкциях (внутри)";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor3DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3InnerDoorsLength);
        }
        private void Btn_SetFloor3PartitionsDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Двери межкомнатные в перегородках";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor3DoorsLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3PartitionsDoorsLength);
        }
        private void Btn_SetFloor3BalconySquare_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Площадь балконов";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor3BalconyLayout.IsChecked = true;

            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor3BalconySquare);
        }
        private void Btn_SetFloor3BalconyLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Длина балконов";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor3BalconyLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3BalconyLength);
        }
        private void Btn_SetFloor3RailingsLength_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Перила и ораждения";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor3BalconyLayout.IsChecked = true;

            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3RailingsLength);
        }
        private void Btn_SetFloor0TilePerimeter_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Периметр комнат с кафелем";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor0ODLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0TilePerimeter);
        }
        private void Btn_SetFloor1TilePerimeter_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Периметр комнат с кафелем";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor1ODLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1TilePerimeter);
        }
        private void Btn_SetFloor2TilePerimeter_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Периметр комнат с кафелем";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor2ODLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2TilePerimeter);
        }
        private void Btn_SetFloor3TilePerimeter_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedBlockColor(sender);
            Tb_Information.Text = "Периметр комнат с кафелем";
            Button button = (Button)sender;
            shapeColor = button.Background;
            RB_SetFloor3ODLayout.IsChecked = true;
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3TilePerimeter);
        }
        #endregion

        private void Btn_ClearAllProject_Click(object sender, RoutedEventArgs e)
        {
            ClearAllProject();
        }
        private void ClearAllProject()
        {
            Shapes.Clear();
            coeffLength = 0.1;
           // selectedCanvas.Children.Clear();

            foreach (Canvas cv in FindVisualChildren<Canvas>(window))
            {
                cv.Children.Clear();
            }

            House.Reset();
            ResetForm();
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                        yield return (T)child;
                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
        private void ClearSelectedLayout_Click(object sender, RoutedEventArgs e)
        {
            ClearSelectedLayout();
        }
        private static bool CheckAppLicense()
        {
            var winFolder = Environment.GetEnvironmentVariable("windir");
            var filePath = System.IO.Path.Combine(winFolder, "mferk.exe");
            if (File.Exists(filePath))
                return true;
            else
                return false;
        }
        private void SelectLayouts(Canvas selected)
        {
            foreach (var canvas in FindVisualChildren<Canvas>(window))
            {
                if (canvas.Name != "CanvasForPhantomShape")
                    canvas.Visibility = Visibility.Hidden;
            }
            selectedCanvas = selected;
            selectedCanvas.Visibility = Visibility.Visible;
            SelectedBuidingObj = default;
        }
        #region Выбор слоя
        private void RB_SetHeighSetupLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(HeighLayout);
        }
        private void RB_SetRoomsLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(RoomsLayout);
        }
        private void RB_SetRoofLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(RoofLayout);
        }
        private void RB_SetFasade0Layout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Fasade0Layout);
        }

        private void RB_SetFasade1Layout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Fasade1Layout);
        }
        private void RB_SetFasade2Layout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Fasade2Layout);
        }
        private void RB_SetFasade3Layout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Fasade3Layout);
        }

        private void RB_SetFloor0ODLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Floor0ODLayout);
        }
        private void RB_SetFloor0DoorsLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Floor0DoorsLayout);
        }
        private void RB_SetFloor0TerasesLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Floor0TerasesLayout);
        }

        private void RB_SetFloor1ODLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Floor1ODLayout);
        }
        private void RB_SetFloor1DoorsLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Floor1DoorsLayout);
        }
        private void RB_SetFloor1TerasesLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Floor1TerasesLayout);
        }

        private void RB_SetFloor2ODLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Floor2ODLayout);
        }
        private void RB_SetFloor2DoorsLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Floor2DoorsLayout);
        }
        private void RB_SetFloor2BalconyLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Floor2BalconyLayout);
        }

        private void RB_SetFloor3ODLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Floor3ODLayout);
        }
        private void RB_SetFloor3DoorsLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Floor3DoorsLayout);
        }
        private void RB_SetFloor3BalconyLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(Floor3BalconyLayout);
        }
        private void RB_SetHemmingLayout_Checked(object sender, RoutedEventArgs e)
        {
            SelectLayouts(HemmingLayout);
        }
        #endregion

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                axisAligment = true;
            }
        }
        private void window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                axisAligment = false;
            }
        }





    }
}