using BuildingPlanCalc.Interfaces;
using BuildingPlanCalc.Models;
using BuildingPlanCalc.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
            InitializeComponent();

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
                (sender, e) => { ClearAllMainLine(); },
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

        byte ShapeType { get; set; } = 1;

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
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr mWindowHandle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(mWindowHandle).AddHook(new HwndSourceHook(WindowProc));
        }
        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    break;
            }

            return IntPtr.Zero;
        }
        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            POINT lMousePosition;
            GetCursorPos(out lMousePosition);

            IntPtr lCurrentScreen = MonitorFromPoint(lMousePosition, MonitorOptions.MONITOR_DEFAULTTONEAREST);


            MINMAXINFO lMmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            MONITORINFO lCurrentScreenInfo = new MONITORINFO();
            if (GetMonitorInfo(lCurrentScreen, lCurrentScreenInfo) == false)
            {
                return;
            }

            //Position relative pour notre fenêtre
            lMmi.ptMaxPosition.X = lCurrentScreenInfo.rcWork.Left - lCurrentScreenInfo.rcMonitor.Left;
            lMmi.ptMaxPosition.Y = lCurrentScreenInfo.rcWork.Top - lCurrentScreenInfo.rcMonitor.Top;
            lMmi.ptMaxSize.X = lCurrentScreenInfo.rcWork.Right - lCurrentScreenInfo.rcWork.Left;
            lMmi.ptMaxSize.Y = lCurrentScreenInfo.rcWork.Bottom - lCurrentScreenInfo.rcWork.Top;

            Marshal.StructureToPtr(lMmi, lParam, true);
        }
        private void SwitchWindowState()
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    {
                        WindowState = WindowState.Maximized;
                        break;
                    }
                case WindowState.Maximized:
                    {
                        WindowState = WindowState.Normal;
                        break;
                    }
            }
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);
        private enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002
        }

        [DllImport("user32.dll")]
        static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
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

                double percentHorizontal = e.GetPosition(this).X / ActualWidth;
                double targetHorizontal = RestoreBounds.Width * percentHorizontal;

                double percentVertical = e.GetPosition(this).Y / ActualHeight;
                double targetVertical = RestoreBounds.Height * percentVertical;

                WindowState = WindowState.Normal;

                GetCursorPos(out POINT lMousePosition);

                Left = lMousePosition.X - targetHorizontal;
                Top = lMousePosition.Y - targetVertical;

                DragMove();
            }
        }
        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SwitchWindowState();
        }
        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TControl.Height = window.ActualHeight - 270;
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
                    Gb_Facade0Floor.Visibility = Visibility.Visible;

                    GB_0FloorBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    Floor0HeighSetupBlock.Visibility = Visibility.Collapsed;
                    GB_0FloorBlock.Visibility = Visibility.Collapsed;
                    Gb_Facade0Floor.Visibility = Visibility.Collapsed;

                    GB_0FloorBlock.Visibility = Visibility.Collapsed;
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

            Gb_Facade1Floor.Visibility = Visibility.Collapsed;
            Gb_Facade2Floor.Visibility = Visibility.Collapsed;
            Gb_Facade3Floor.Visibility = Visibility.Collapsed;

            // заказчик...




            switch (SetFloorsHouse.SelectedIndex)
            {
                case 0:
                    Floor1HeighSetupBlock.Visibility = Visibility.Visible;
                    GB_1FloorBlock.Visibility = Visibility.Visible;
                    Gb_Facade1Floor.Visibility = Visibility.Visible;
                    break;
                case 1:
                    Floor1HeighSetupBlock.Visibility = Visibility.Visible;
                    Floor2HeighSetupBlock.Visibility = Visibility.Visible;

                    GB_1FloorBlock.Visibility = Visibility.Visible;
                    GB_2FloorBlock.Visibility = Visibility.Visible;

                    Gb_Facade1Floor.Visibility = Visibility.Visible;
                    Gb_Facade2Floor.Visibility = Visibility.Visible;
                    break;
                case 2:
                    Floor1HeighSetupBlock.Visibility = Visibility.Visible;
                    Floor2HeighSetupBlock.Visibility = Visibility.Visible;
                    Floor3HeighSetupBlock.Visibility = Visibility.Visible;

                    GB_1FloorBlock.Visibility = Visibility.Visible;
                    GB_2FloorBlock.Visibility = Visibility.Visible;
                    GB_3FloorBlock.Visibility = Visibility.Visible;

                    Gb_Facade1Floor.Visibility = Visibility.Visible;
                    Gb_Facade2Floor.Visibility = Visibility.Visible;
                    Gb_Facade3Floor.Visibility = Visibility.Visible;
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

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            //TODO : Переписать регулярку для double
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
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
            DrawShape(p);
            Tb_CursorСoordinates.Text = $"X:{p.X} Y:{p.Y}";
        }
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ClearAllPhantomLine();

            point_f = Mouse.GetPosition(CanvasForPhantomShape);
            // Если контрольная линия
            if (isControlLine)
            {
                if (!double.TryParse(TB_RealLength.Text, out realLength))
                {
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

                //    Walls.Add(new ModifyLines(new Line() { X1 = point_s.X, Y1 = point_s.Y, X2 = point_f.X, Y2 = point_f.Y }, 255, 255));
                isControlLine = false;
                //    Tb_Information.Visibility = Visibility.Collapsed;
                coeffLength = controlLineLength / realLength;
                if (CanvasForPhantomShape.Children.Count > 0)
                    Btn_DeleteLastLine.IsEnabled = true;
                //     CaclWallsLength();
                MessageBox.Show("Коэффициент размера успешно установлен", "Изменение коэффициента", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Если рисование
            if (isPainting && SelectedBuidingObj != 0)
            {
                // HACK : Принудительная установка масштаба. Можно отключить при тестировании
                if (coeffLength == 0.1)
                {
                    TB_RealLength.Focus();
                    MessageBox.Show("Необходимо установить масштаб чертежа", "Предупреждение расчетов", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ShapeType == (byte)GlobalVariables.ShapeTypeEnum.Line)
                {
                    Shapes.Add(new ModifyLine(new Line() { X1 = point_s.X, Y1 = point_s.Y, X2 = point_f.X, Y2 = point_f.Y }, SelectedBuidingObj, ShapeType, coeffLength));

                    ModifyLine shape = Shapes[Shapes.Count - 1] as ModifyLine;
                    CanvasForMainShape.Children.Add(shape.Line);
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

                    Shapes.Add(new ModifyRect(rect, SelectedBuidingObj, ShapeType, coeffLength));

                    ModifyRect shape = Shapes[Shapes.Count - 1] as ModifyRect;
                    CanvasForMainShape.Children.Add(shape.Rectangle);
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
                        Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(Settings.ShapeColorDefault),
                        StrokeThickness = Settings.ShapeThickness
                    };
                    CanvasForMainShape.Children.Add(line);

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
                        CanvasForMainShape.Children.Add(line);
                        TrianglePointCount = 0;

                        var triange = new Polygon();
                        triange.Points.Add(firstTrianglePoint);
                        triange.Points.Add(point_s);
                        triange.Points.Add(point_f);


                        // Удалить фантомные линии
                        CanvasForMainShape.Children.RemoveAt(CanvasForMainShape.Children.Count - 1);
                        CanvasForMainShape.Children.RemoveAt(CanvasForMainShape.Children.Count - 1);
                        //SelectedCanvas.Children.RemoveAt(Walls.Count);

                        Shapes.Add(new ModifyTriangle(triange, SelectedBuidingObj, ShapeType, coeffLength));
                        ModifyTriangle shape = Shapes[Shapes.Count - 1] as ModifyTriangle;
                        CanvasForMainShape.Children.Add(shape.Triangle);
                    }
                }

                if (CanvasForMainShape.Children.Count > 0)
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
            if (CanvasForMainShape.Children.Count > 0)
            {
                CanvasForMainShape.Children.RemoveAt(CanvasForMainShape.Children.Count - 1);
                if (Shapes.Count > 0)
                {
                    // Если треугольник, удаляем лишнюю линию
                    if (Shapes.Last().ShapeType == (byte)GlobalVariables.ShapeTypeEnum.Triangle && CanvasForMainShape.Children.Count > 0)
                    {
                        CanvasForMainShape.Children.RemoveAt(CanvasForMainShape.Children.Count - 1);
                    }

                    Shapes.Remove(Shapes.Last());
                    CalcSize();
                }
            }
            if (CanvasForMainShape.Children.Count == 0)
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
                    Tb_Set0FloorHeight.Text = $"Цокольный - {House.Floor0Height} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1Height:
                    House.Floor1Height = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1Height).Select(l => l.Length).Sum(), roundTo);
                    Tb_Set1FloorHeight.Text = $"Первый - {House.Floor1Height} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2Height:
                    House.Floor2Height = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2Height).Select(l => l.Length).Sum(), roundTo);
                    Tb_Set2FloorHeight.Text = $"Второй - {House.Floor2Height} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3Height:
                    House.Floor3Height = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3Height).Select(l => l.Length).Sum(), roundTo);
                    Tb_Set3FloorHeight.Text = $"Третий - {House.Floor3Height} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.KitchensSquare:
                    House.KitchensSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.KitchensSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_SetKitchenSquare.Text = $"Площадь кухонь и гостиных - {House.KitchensSquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.RoofHeight:
                    House.RoofHeight = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.RoofHeight).Select(l => l.Length).Sum(), roundTo);
                    Tb_SetFullHouseHeight.Text = $"Высота от пола верхнего этажа до конька - {House.RoofHeight} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.RoofMinWallHeight:
                    House.RoofMinWallHeight = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.RoofMinWallHeight).Select(l => l.Length).Sum(), roundTo);
                    Tb_SetRoofMinWallHeight.Text = $"Минимальная высота стен верхнего этажа - {House.RoofMinWallHeight} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.RoofSquare:
                    House.RoofSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.RoofSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_SetRoofSquare.Text = $"Площадь, накрытая основной кровлей - {House.RoofSquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.RoofLength:
                    House.RoofLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.RoofLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_SetRoofLength.Text = $"Длина основной кровли - {House.RoofLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.CanopySquare:
                    House.CanopySquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.CanopySquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_SetCanopySquare.Text = $"Площадь, накрытая навесами - {House.CanopySquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.CanopyLength:
                    House.CanopyLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.CanopyLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_SetCanopyLength.Text = $"Длина навесов вдоль стены, общая - {House.CanopyLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.PergolaSquare:
                    House.PergolaSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.PergolaSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_SetPergolaSquare.Text = $"Площадь Перголы - {House.PergolaSquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.HemmingButt:
                    House.HemmingButt = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.HemmingButt).Select(l => l.Length).Sum(), roundTo);
                    Tb_SetHemmingButt.Text = $"Подшива торцов основной кровли и навесов - {House.HemmingButt} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.HemmingOverhangsSquare:
                    House.HemmingOverhangsSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.HemmingOverhangsSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_SetHemmingOverhangsSquare.Text = $"Подшива свесов основной кровли снизу - {House.HemmingOverhangsSquare} кв.м.";
                    break;
                #endregion
                #region Раздел "Фасады"
                case (byte)GlobalVariables.ProjectObjEnum.Floor0GlaseSq:
                    House.Floor0GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor0GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set0FloorGlaseSq.Text = $"Площадь окон и стекляных дверей - {House.Floor0GlaseSq} кв.м.";
                    Tb_Set0FloorGlaseP.Text = $"Периметр окон и стекляных дверей - {House.Floor0GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1GlaseSq:
                    House.Floor1GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor1GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set1FloorGlaseSq.Text = $"Площадь окон и стекляных дверей - {House.Floor1GlaseSq} кв.м.";
                    Tb_Set1FloorGlaseP.Text = $"Периметр окон и стекляных дверей - {House.Floor1GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2GlaseSq:
                    House.Floor2GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor2GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set2FloorGlaseSq.Text = $"Площадь окон и стекляных дверей - {House.Floor2GlaseSq} кв.м.";
                    Tb_Set2FloorGlaseP.Text = $"Периметр окон и стекляных дверей - {House.Floor2GlaseP} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3GlaseSq:
                    House.Floor3GlaseSq = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3GlaseSq).Select(l => l.Square).Sum(), roundTo);
                    House.Floor3GlaseP = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3GlaseSq).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Set3FloorGlaseSq.Text = $"Площадь окон и стекляных дверей - {House.Floor3GlaseSq} кв.м.";
                    Tb_Set3FloorGlaseP.Text = $"Периметр окон и стекляных дверей - {House.Floor3GlaseP} м.";
                    break;
                #endregion
                #region Раздел "Этажи"
                case (byte)GlobalVariables.ProjectObjEnum.Floor0PlinthHeight:
                    House.Floor0PlinthHeight = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0PlinthHeight).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0PlinthHeight.Text = $"Высота цоколя над землей до плиты перекрытия - {House.Floor0PlinthHeight} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0BadroomSquare:
                    House.Floor0BadroomSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0BadroomSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor0BadroomSquare.Text = $"Площадь туалетов и ванных комнтат - {House.Floor0BadroomSquare} м.";

                    House.Floor0TilePerimeter =
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0BadroomSquare).Select(l => l.Perimetr).Sum(), roundTo) +
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0TileSquare).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Floor0TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor0TilePerimeter} м.";

                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0TileSquare:
                    House.Floor0TileSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0TileSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor0TileSquare.Text = $"Прочие помещения в кафеле - {House.Floor0TileSquare} м.";

                    House.Floor0TilePerimeter =
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0BadroomSquare).Select(l => l.Perimetr).Sum(), roundTo) +
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0TileSquare).Select(l => l.Perimetr).Sum(), roundTo);
                    Tb_Floor0TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor0TilePerimeter} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0OutWallsLength:
                    House.Floor0OutWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0OutWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0OutWallsLength.Text = $"Длина внешних несущих стен - {House.Floor0OutWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0InnerWallsLength:
                    House.Floor0InnerWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0InnerWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0InnerWallsLength.Text = $"Длина внутренних несущих стен - {House.Floor0InnerWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0LightWallsLength:
                    House.Floor0LightWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0LightWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0LightWallsLength.Text = $"Длина перегородок - {House.Floor0LightWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0BreakWallsLength:
                    House.Floor0BreakWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0BreakWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0BreakWallsLength.Text = $"Разрывы в несущей стене > 2 м. - {House.Floor0BreakWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0OutDoorsLength:
                    House.Floor0OutDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0OutDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0OutDoorsLength.Text = $"Двери металлические в несущих стенах - {House.Floor0OutDoorsLength} м.";
                    House.Floor0OutDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0OutDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor0OutDoorsCount.Text = $"Двери металлические в несущих стенах - {House.Floor0OutDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0InnerDoorsLength:
                    House.Floor0InnerDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0InnerDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0InnerDoorsLength.Text = $"Двери межкомнатные в несущих стенах - {House.Floor0InnerDoorsLength} м.";
                    House.Floor0InnerDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0InnerDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor0InnerDoorsCount.Text = $"Двери межкомнатные в несущих стенах - {House.Floor0InnerDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0PartitionsDoorsLength:
                    House.Floor0PartitionsDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0PartitionsDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0PartitionsDoorsLength.Text = $"Двери межкомнатные в перегородках - {House.Floor0PartitionsDoorsLength} м.";
                    House.Floor0PartitionsDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0PartitionsDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor0PartitionsDoorsCount.Text = $"Двери межкомнатные в перегородках - {House.Floor0PartitionsDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0GatesLength:
                    House.Floor0GatesLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0GatesLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0GatesLength.Text = $"Ворота - {House.Floor0GatesLength} м.";
                    House.Floor0GatesCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0GatesLength).Select(l => l.Length).Count();
                    Tb_Floor0GatesCount.Text = $"Ворота - {House.Floor0GatesCount} шт.";
                    break;

                case (byte)GlobalVariables.ProjectObjEnum.Floor0TerassesSquare:
                    House.Floor0TerassesSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0TerassesSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor0TerassesSquare.Text = $"Площадь терасс и крылец - {House.Floor0TerassesSquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0InnerTerassesLength:
                    House.Floor0InnerTerassesLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0InnerTerassesLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0InnerTerassesLength.Text = $"Длина внешних стен терасс и крылец - {House.Floor0InnerTerassesLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0TerassesLength:
                    House.Floor0TerassesLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0TerassesLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0TerassesLength.Text = $"Длина терасс и крылец - {House.Floor0TerassesLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0RailingsLength:
                    House.Floor0RailingsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor0RailingsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor0RailingsLength.Text = $"Длина перил и ограждений - {House.Floor0RailingsLength} м.";
                    break;

                case (byte)GlobalVariables.ProjectObjEnum.Floor1BadroomSquare:
                    House.Floor1BadroomSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1BadroomSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor1BadroomSquare.Text = $"Площадь туалетов и ванных комнтат - {House.Floor1BadroomSquare} кв.м.";

                    House.Floor1TilePerimeter =
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1BadroomSquare).Select(l => l.Perimetr).Sum(), roundTo) +
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1TileSquare).Select(l => l.Perimetr).Sum(), roundTo);

                    Tb_Floor1TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor1TilePerimeter} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1TileSquare:
                    House.Floor1TileSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1TileSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor1TileSquare.Text = $"Прочие помещения в кафеле - {House.Floor1TileSquare} кв.м.";
                    House.Floor1TilePerimeter =
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1BadroomSquare).Select(l => l.Perimetr).Sum(), roundTo) +
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1TileSquare).Select(l => l.Perimetr).Sum(), roundTo);

                    Tb_Floor1TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor1TilePerimeter} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1OutWallsLength:
                    House.Floor1OutWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1OutWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1OutWallsLength.Text = $"Длина внешних несущих стен - {House.Floor1OutWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1InnerWallsLength:
                    House.Floor1InnerWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1InnerWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1InnerWallsLength.Text = $"Длина внутренних несущих стен - {House.Floor1InnerWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1LightWallsLength:
                    House.Floor1LightWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1LightWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1LightWallsLength.Text = $"Длина перегородок - {House.Floor1LightWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1BreakWallsLength:
                    House.Floor1BreakWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1BreakWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1BreakWallsLength.Text = $"Разрывы в несущей стене > 2 м. - {House.Floor1BreakWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1OutDoorsLength:
                    House.Floor1OutDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1OutDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1OutDoorsLength.Text = $"Двери металлические в несущих стенах - {House.Floor1OutDoorsLength} м.";
                    House.Floor1OutDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1OutDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor1OutDoorsCount.Text = $"Двери металлические в несущих стенах - {House.Floor1OutDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1InnerDoorsLength:
                    House.Floor1InnerDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1InnerDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1InnerDoorsLength.Text = $"Двери межкомнатные в несущих стенах - {House.Floor1InnerDoorsLength} м.";
                    House.Floor1InnerDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1InnerDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor1InnerDoorsCount.Text = $"Двери межкомнатные в несущих стенах - {House.Floor1InnerDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1PartitionsDoorsLength:
                    House.Floor1PartitionsDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1PartitionsDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1PartitionsDoorsLength.Text = $"Двери межкомнатные в перегородках - {House.Floor1PartitionsDoorsLength} м.";
                    House.Floor1PartitionsDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1PartitionsDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor1PartitionsDoorsCount.Text = $"Двери межкомнатные в перегородках - {House.Floor1PartitionsDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1GatesLength:
                    House.Floor1GatesLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1GatesLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1GatesLength.Text = $"Ворота - {House.Floor1GatesLength} м.";
                    House.Floor1GatesCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1GatesLength).Select(l => l.Length).Count();
                    Tb_Floor1GatesCount.Text = $"Ворота - {House.Floor1GatesCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1TerassesSquare:
                    House.Floor1TerassesSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1TerassesSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor1TerassesSquare.Text = $"Площадь терасс и крылец - {House.Floor1TerassesSquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1InnerTerassesLength:
                    House.Floor1InnerTerassesLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1InnerTerassesLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1InnerTerassesLength.Text = $"Длина внешних стен терасс и крылец - {House.Floor1InnerTerassesLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1TerassesLength:
                    House.Floor1TerassesLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1TerassesLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1TerassesLength.Text = $"Длина терасс и крылец - {House.Floor1TerassesLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1RailingsLength:
                    House.Floor1RailingsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor1RailingsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor1RailingsLength.Text = $"Длина перил и ограждений - {House.Floor1RailingsLength} м.";
                    break;

                case (byte)GlobalVariables.ProjectObjEnum.Floor2РHoleSecondLight:
                    House.Floor2РHoleSecondLight = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2РHoleSecondLight).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor2РHoleSecondLight.Text = $"Дырка в полу под второй свет - {House.Floor2РHoleSecondLight} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2BadroomSquare:
                    House.Floor2BadroomSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2BadroomSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor2BadroomSquare.Text = $"Площадь туалетов и ванных комнтат - {House.Floor2BadroomSquare} кв.м.";

                    House.Floor2TilePerimeter =
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2BadroomSquare).Select(l => l.Perimetr).Sum(), roundTo) +
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2TileSquare).Select(l => l.Perimetr).Sum(), roundTo);

                    Tb_Floor2TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor2TilePerimeter} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2TileSquare:
                    House.Floor2TileSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2TileSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor2TileSquare.Text = $"Прочие помещения в кафеле - {House.Floor2TileSquare} кв.м.";
                    House.Floor2TilePerimeter =
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2BadroomSquare).Select(l => l.Perimetr).Sum(), roundTo) +
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2TileSquare).Select(l => l.Perimetr).Sum(), roundTo);

                    Tb_Floor2TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor2TilePerimeter} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2OutWallsLength:
                    House.Floor2OutWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2OutWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2OutWallsLength.Text = $"Длина внешних несущих стен - {House.Floor2OutWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2InnerWallsLength:
                    House.Floor2InnerWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2InnerWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2InnerWallsLength.Text = $"Длина внутренних несущих стен - {House.Floor2InnerWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2LightWallsLength:
                    House.Floor2LightWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2LightWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2LightWallsLength.Text = $"Длина перегородок - {House.Floor2LightWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2BreakWallsLength:
                    House.Floor2BreakWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2BreakWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2BreakWallsLength.Text = $"Разрывы в несущей стене > 2 м. - {House.Floor2BreakWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2OutDoorsLength:
                    House.Floor2OutDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2OutDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2OutDoorsLength.Text = $"Двери металлические в несущих стенах - {House.Floor2OutDoorsLength} м.";
                    House.Floor2OutDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2OutDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor2OutDoorsCount.Text = $"Двери металлические в несущих стенах - {House.Floor2OutDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2InnerDoorsLength:
                    House.Floor2InnerDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2InnerDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2InnerDoorsLength.Text = $"Двери межкомнатные в несущих стенах - {House.Floor2InnerDoorsLength} м.";
                    House.Floor2InnerDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2InnerDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor2InnerDoorsCount.Text = $"Двери межкомнатные в несущих стенах - {House.Floor2InnerDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2PartitionsDoorsLength:
                    House.Floor2PartitionsDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2PartitionsDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2PartitionsDoorsLength.Text = $"Двери межкомнатные в перегородках - {House.Floor2PartitionsDoorsLength} м.";
                    House.Floor2PartitionsDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2PartitionsDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor2PartitionsDoorsCount.Text = $"Двери межкомнатные в перегородках - {House.Floor2PartitionsDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2BalconySquare:
                    House.Floor2BalconySquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2BalconySquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor2BalconySquare.Text = $"Площадь балконов - {House.Floor2BalconySquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2BalconyLength:
                    House.Floor2BalconyLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2BalconyLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2BalconyLength.Text = $"Длина балконов - {House.Floor2BalconyLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2RailingsLength:
                    House.Floor2RailingsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor2RailingsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor2RailingsLength.Text = $"Длина перил и ограждений - {House.Floor2RailingsLength} м.";
                    break;

                case (byte)GlobalVariables.ProjectObjEnum.Floor3РHoleSecondLight:
                    House.Floor3РHoleSecondLight = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3РHoleSecondLight).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor3РHoleSecondLight.Text = $"Дырка в полу под второй свет - {House.Floor3РHoleSecondLight} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3BadroomSquare:
                    House.Floor3BadroomSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3BadroomSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor3BadroomSquare.Text = $"Площадь туалетов и ванных комнтат - {House.Floor3BadroomSquare} кв.м.";

                    House.Floor3TilePerimeter =
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3BadroomSquare).Select(l => l.Perimetr).Sum(), roundTo) +
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3TileSquare).Select(l => l.Perimetr).Sum(), roundTo);

                    Tb_Floor3TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor3TilePerimeter} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3TileSquare:
                    House.Floor3TileSquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3TileSquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor3TileSquare.Text = $"Прочие помещения в кафеле - {House.Floor3TileSquare} кв.м.";
                    House.Floor3TilePerimeter =
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3BadroomSquare).Select(l => l.Perimetr).Sum(), roundTo) +
                        Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3TileSquare).Select(l => l.Perimetr).Sum(), roundTo);

                    Tb_Floor3TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor3TilePerimeter} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3OutWallsLength:
                    House.Floor3OutWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3OutWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3OutWallsLength.Text = $"Длина внешних несущих стен - {House.Floor3OutWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3InnerWallsLength:
                    House.Floor3InnerWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3InnerWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3InnerWallsLength.Text = $"Длина внутренних несущих стен - {House.Floor3InnerWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3LightWallsLength:
                    House.Floor3LightWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3LightWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3LightWallsLength.Text = $"Длина перегородок - {House.Floor3LightWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3BreakWallsLength:
                    House.Floor3BreakWallsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3BreakWallsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3BreakWallsLength.Text = $"Разрывы в несущей стене > 3 м. - {House.Floor3BreakWallsLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3OutDoorsLength:
                    House.Floor3OutDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3OutDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3OutDoorsLength.Text = $"Двери металлические в несущих стенах - {House.Floor3OutDoorsLength} м.";
                    House.Floor3OutDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3OutDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor3OutDoorsCount.Text = $"Двери металлические в несущих стенах - {House.Floor3OutDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3InnerDoorsLength:
                    House.Floor3InnerDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3InnerDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3InnerDoorsLength.Text = $"Двери межкомнатные в несущих стенах - {House.Floor3InnerDoorsLength} м.";
                    House.Floor3InnerDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3InnerDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor3InnerDoorsCount.Text = $"Двери межкомнатные в несущих стенах - {House.Floor3InnerDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3PartitionsDoorsLength:
                    House.Floor3PartitionsDoorsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3PartitionsDoorsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3PartitionsDoorsLength.Text = $"Двери межкомнатные в перегородках - {House.Floor3PartitionsDoorsLength} м.";
                    House.Floor3PartitionsDoorsCount = Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3PartitionsDoorsLength).Select(l => l.Length).Count();
                    Tb_Floor3PartitionsDoorsCount.Text = $"Двери межкомнатные в перегородках - {House.Floor3PartitionsDoorsCount} шт.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3BalconySquare:
                    House.Floor3BalconySquare = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3BalconySquare).Select(l => l.Square).Sum(), roundTo);
                    Tb_Floor3BalconySquare.Text = $"Площадь балконов - {House.Floor3BalconySquare} кв.м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3BalconyLength:
                    House.Floor3BalconyLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3BalconyLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3BalconyLength.Text = $"Длина балконов - {House.Floor3BalconyLength} м.";
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3RailingsLength:
                    House.Floor3RailingsLength = Math.Round(Shapes.Where(w => w.ObjType == (byte)GlobalVariables.ProjectObjEnum.Floor3RailingsLength).Select(l => l.Length).Sum(), roundTo);
                    Tb_Floor3RailingsLength.Text = $"Длина перил и ограждений - {House.Floor3RailingsLength} м.";
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
            Tb_Set0FloorHeight.Text = $"Цокольный - 0 м.";
            Tb_Set1FloorHeight.Text = $"Первый - 0 м.";
            Tb_Set2FloorHeight.Text = $"Второй - 0 м.";
            Tb_Set3FloorHeight.Text = $"Третий - 0 м.";
            Tb_SetKitchenSquare.Text = $"Площадь кухонь и гостиных - 0 кв.м.";
            Tb_SetFullHouseHeight.Text = $"Высота от пола верхнего этажа до конька - 0 м.";
            Tb_SetRoofMinWallHeight.Text = $"Минимальная высота стен верхнего этажа - 0 м.";
            Tb_SetRoofSquare.Text = $"Площадь, накрытая основной кровлей - 0 кв.м.";
            Tb_SetRoofLength.Text = $"Длина основной кровли - 0 м.";
            Tb_SetCanopySquare.Text = $"Площадь, накрытая навесами - 0 кв.м.";
            Tb_SetCanopyLength.Text = $"Длина навесов вдоль стены, общая - 0 м.";
            Tb_SetPergolaSquare.Text = $"Площадь Перголы - 0 кв.м.";
            Tb_SetHemmingButt.Text = $"Подшива торцов основной кровли и навесов - 0 м.";
            Tb_SetHemmingOverhangsSquare.Text = $"Подшива свесов основной кровли снизу - 0 кв.м.";
            Tb_Set0FloorGlaseSq.Text = $"Площадь окон и стекляных дверей - 0 кв.м.";
            Tb_Set0FloorGlaseP.Text = $"Периметр окон и стекляных дверей - 0 м.";
            Tb_Set1FloorGlaseSq.Text = $"Площадь окон и стекляных дверей - 0 кв.м.";
            Tb_Set1FloorGlaseP.Text = $"Периметр окон и стекляных дверей - 0 м.";
            Tb_Set2FloorGlaseSq.Text = $"Площадь окон и стекляных дверей - 0 кв.м.";
            Tb_Set2FloorGlaseP.Text = $"Периметр окон и стекляных дверей - 0 м.";
            Tb_Set3FloorGlaseSq.Text = $"Площадь окон и стекляных дверей - 0 кв.м.";
            Tb_Set3FloorGlaseP.Text = $"Периметр окон и стекляных дверей - 0 м.";
            Tb_Floor0PlinthHeight.Text = $"Высота цоколя над землей до плиты перекрытия - 0 м.";
            Tb_Floor0BadroomSquare.Text = $"Площадь туалетов и ванных комнтат - 0 м.";
            Tb_Floor0TilePerimeter.Text = $"Периметр комнат с кафелем - 0 м.";
            Tb_Floor0TileSquare.Text = $"Прочие помещения в кафеле - 0 м.";
            Tb_Floor0TilePerimeter.Text = $"Периметр комнат с кафелем - 0 м.";
            Tb_Floor0OutWallsLength.Text = $"Длина внешних несущих стен - 0 м.";
            Tb_Floor0InnerWallsLength.Text = $"Длина внутренних несущих стен - 0 м.";
            Tb_Floor0LightWallsLength.Text = $"Длина перегородок - 0 м.";
            Tb_Floor0BreakWallsLength.Text = $"Разрывы в несущей стене > 2 м. - 0 м.";
            Tb_Floor0OutDoorsLength.Text = $"Двери металлические в несущих стенах - 0 м.";
            Tb_Floor0OutDoorsCount.Text = $"Двери металлические в несущих стенах - 0 шт.";
            Tb_Floor0InnerDoorsLength.Text = $"Двери межкомнатные в несущих стенах - 0 м.";
            Tb_Floor0InnerDoorsCount.Text = $"Двери межкомнатные в несущих стенах - 0 шт.";
            Tb_Floor0PartitionsDoorsLength.Text = $"Двери межкомнатные в перегородках - 0 м.";
            Tb_Floor0PartitionsDoorsCount.Text = $"Двери межкомнатные в перегородках - 0 шт.";
            Tb_Floor0GatesLength.Text = $"Ворота - 0 м.";
            Tb_Floor0GatesCount.Text = $"Ворота - 0 шт.";
            Tb_Floor0TerassesSquare.Text = $"Площадь терасс и крылец - 0 кв.м.";
            Tb_Floor0InnerTerassesLength.Text = $"Длина внешних стен терасс и крылец - 0 м.";
            Tb_Floor0TerassesLength.Text = $"Длина терасс и крылец - 0 м.";
            Tb_Floor0RailingsLength.Text = $"Длина перил и ограждений - 0 м.";

            Tb_Floor1BadroomSquare.Text = $"Площадь туалетов и ванных комнтат - 0 кв.м.";
            Tb_Floor1TilePerimeter.Text = $"Периметр комнат с кафелем - 0 м.";
            Tb_Floor1TileSquare.Text = $"Прочие помещения в кафеле - 0 кв.м.";
            Tb_Floor1TilePerimeter.Text = $"Периметр комнат с кафелем - 0 м.";
            Tb_Floor1OutWallsLength.Text = $"Длина внешних несущих стен - 0 м.";
            Tb_Floor1InnerWallsLength.Text = $"Длина внутренних несущих стен - 0 м.";
            Tb_Floor1LightWallsLength.Text = $"Длина перегородок - 0 м.";
            Tb_Floor1BreakWallsLength.Text = $"Разрывы в несущей стене > 2 м. - 0 м.";
            Tb_Floor1OutDoorsLength.Text = $"Двери металлические в несущих стенах - 0 м.";
            Tb_Floor1OutDoorsCount.Text = $"Двери металлические в несущих стенах - 0 шт.";
            Tb_Floor1InnerDoorsLength.Text = $"Двери межкомнатные в несущих стенах - 0 м.";
            Tb_Floor1InnerDoorsCount.Text = $"Двери межкомнатные в несущих стенах - 0 шт.";
            Tb_Floor1PartitionsDoorsLength.Text = $"Двери межкомнатные в перегородках - 0 м.";
            Tb_Floor1PartitionsDoorsCount.Text = $"Двери межкомнатные в перегородках - 0 шт.";
            Tb_Floor1GatesLength.Text = $"Ворота - 0 м.";
            Tb_Floor1GatesCount.Text = $"Ворота - 0 шт.";
            Tb_Floor1TerassesSquare.Text = $"Площадь терасс и крылец - 0 кв.м.";
            Tb_Floor1InnerTerassesLength.Text = $"Длина внешних стен терасс и крылец - 0 м.";
            Tb_Floor1TerassesLength.Text = $"Длина терасс и крылец - 0 м.";
            Tb_Floor1RailingsLength.Text = $"Длина перил и ограждений - 0 м.";

            Tb_Floor2РHoleSecondLight.Text = $"Дырка в полу под второй свет - 0 кв.м.";
            Tb_Floor2BadroomSquare.Text = $"Площадь туалетов и ванных комнтат - 0 кв.м.";
            Tb_Floor2TilePerimeter.Text = $"Периметр комнат с кафелем - 0 м.";
            Tb_Floor2TileSquare.Text = $"Прочие помещения в кафеле - 0 кв.м.";
            Tb_Floor2TilePerimeter.Text = $"Периметр комнат с кафелем - 0 м.";
            Tb_Floor2OutWallsLength.Text = $"Длина внешних несущих стен - 0 м.";
            Tb_Floor2InnerWallsLength.Text = $"Длина внутренних несущих стен - 0 м.";
            Tb_Floor2LightWallsLength.Text = $"Длина перегородок - 0 м.";
            Tb_Floor2BreakWallsLength.Text = $"Разрывы в несущей стене > 2 м. - 0 м.";
            Tb_Floor2OutDoorsLength.Text = $"Двери металлические в несущих стенах - 0 м.";
            Tb_Floor2OutDoorsCount.Text = $"Двери металлические в несущих стенах - 0 шт.";
            Tb_Floor2InnerDoorsLength.Text = $"Двери межкомнатные в несущих стенах - 0 м.";
            Tb_Floor2InnerDoorsCount.Text = $"Двери межкомнатные в несущих стенах - 0 шт.";
            Tb_Floor2PartitionsDoorsLength.Text = $"Двери межкомнатные в перегородках - 0 м.";
            Tb_Floor2PartitionsDoorsCount.Text = $"Двери межкомнатные в перегородках - 0 шт.";
            Tb_Floor2BalconySquare.Text = $"Площадь балконов - 0 кв.м.";
            Tb_Floor2BalconyLength.Text = $"Длина балконов - 0 м.";
            Tb_Floor2RailingsLength.Text = $"Длина перил и ограждений - 0 м.";

            Tb_Floor3РHoleSecondLight.Text = $"Дырка в полу под второй свет - 0 кв.м.";
            Tb_Floor3BadroomSquare.Text = $"Площадь туалетов и ванных комнтат - 0 кв.м.";
            Tb_Floor3TilePerimeter.Text = $"Периметр комнат с кафелем - 0 м.";
            Tb_Floor3TileSquare.Text = $"Прочие помещения в кафеле - 0 кв.м.";
            Tb_Floor3TilePerimeter.Text = $"Периметр комнат с кафелем - 0 м.";
            Tb_Floor3OutWallsLength.Text = $"Длина внешних несущих стен - 0 м.";
            Tb_Floor3InnerWallsLength.Text = $"Длина внутренних несущих стен - 0 м.";
            Tb_Floor3LightWallsLength.Text = $"Длина перегородок - 0 м.";
            Tb_Floor3BreakWallsLength.Text = $"Разрывы в несущей стене > 3 м. - 0 м.";
            Tb_Floor3OutDoorsLength.Text = $"Двери металлические в несущих стенах - 0 м.";
            Tb_Floor3OutDoorsCount.Text = $"Двери металлические в несущих стенах - 0 шт.";
            Tb_Floor3InnerDoorsLength.Text = $"Двери межкомнатные в несущих стенах - 0 м.";
            Tb_Floor3InnerDoorsCount.Text = $"Двери межкомнатные в несущих стенах - 0 шт.";
            Tb_Floor3PartitionsDoorsLength.Text = $"Двери межкомнатные в перегородках - 0 м.";
            Tb_Floor3PartitionsDoorsCount.Text = $"Двери межкомнатные в перегородках - 0 шт.";
            Tb_Floor3BalconySquare.Text = $"Площадь балконов - 0 кв.м.";
            Tb_Floor3BalconyLength.Text = $"Длина балконов - 0 м.";
            Tb_Floor3RailingsLength.Text = $"Длина перил и ограждений - 0 м.";

            #region Пригодится для загрузки настроек
            //Tb_Set0FloorHeight.Text = $"Цокольный - {House.Floor0Height} м.";
            //Tb_Set1FloorHeight.Text = $"Первый - {House.Floor1Height} м.";
            //Tb_Set2FloorHeight.Text = $"Второй - {House.Floor2Height} м.";
            //Tb_Set3FloorHeight.Text = $"Третий - {House.Floor3Height} м.";
            //Tb_SetKitchenSquare.Text = $"Площадь кухонь и гостиных - {House.KitchensSquare} кв.м.";
            //Tb_SetFullHouseHeight.Text = $"Высота от пола верхнего этажа до конька - {House.RoofHeight} м.";
            //Tb_SetRoofMinWallHeight.Text = $"Минимальная высота стен верхнего этажа - {House.RoofMinWallHeight} м.";
            //Tb_SetRoofSquare.Text = $"Площадь, накрытая основной кровлей - {House.RoofSquare} кв.м.";
            //Tb_SetRoofLength.Text = $"Длина основной кровли - {House.RoofLength} м.";
            //Tb_SetCanopySquare.Text = $"Площадь, накрытая навесами - {House.CanopySquare} кв.м.";
            //Tb_SetCanopyLength.Text = $"Длина навесов вдоль стены, общая - {House.CanopyLength} м.";
            //Tb_SetPergolaSquare.Text = $"Площадь Перголы - {House.PergolaSquare} кв.м.";
            //Tb_SetHemmingButt.Text = $"Подшива торцов основной кровли и навесов - {House.HemmingButt} м.";
            //Tb_SetHemmingOverhangsSquare.Text = $"Подшива свесов основной кровли снизу - {House.HemmingOverhangsSquare} кв.м.";
            //Tb_Set0FloorGlaseSq.Text = $"Площадь окон и стекляных дверей - {House.Floor0GlaseSq} кв.м.";
            //Tb_Set0FloorGlaseP.Text = $"Периметр окон и стекляных дверей - {House.Floor0GlaseP} м.";
            //Tb_Set1FloorGlaseSq.Text = $"Площадь окон и стекляных дверей - {House.Floor1GlaseSq} кв.м.";
            //Tb_Set1FloorGlaseP.Text = $"Периметр окон и стекляных дверей - {House.Floor1GlaseP} м.";
            //Tb_Set2FloorGlaseSq.Text = $"Площадь окон и стекляных дверей - {House.Floor2GlaseSq} кв.м.";
            //Tb_Set2FloorGlaseP.Text = $"Периметр окон и стекляных дверей - {House.Floor2GlaseP} м.";
            //Tb_Set3FloorGlaseSq.Text = $"Площадь окон и стекляных дверей - {House.Floor3GlaseSq} кв.м.";
            //Tb_Set3FloorGlaseP.Text = $"Периметр окон и стекляных дверей - {House.Floor3GlaseP} м.";
            //Tb_Floor0PlinthHeight.Text = $"Высота цоколя над землей до плиты перекрытия - {House.Floor0PlinthHeight} м.";
            //Tb_Floor0BadroomSquare.Text = $"Площадь туалетов и ванных комнтат - {House.Floor0BadroomSquare} м.";
            //Tb_Floor0TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor0TilePerimeter} м.";
            //Tb_Floor0TileSquare.Text = $"Прочие помещения в кафеле - {House.Floor0TileSquare} м.";
            //Tb_Floor0TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor0TilePerimeter} м.";
            //Tb_Floor0OutWallsLength.Text = $"Длина внешних несущих стен - {House.Floor0OutWallsLength} м.";
            //Tb_Floor0InnerWallsLength.Text = $"Длина внутренних несущих стен - {House.Floor0InnerWallsLength} м.";
            //Tb_Floor0LightWallsLength.Text = $"Длина перегородок - {House.Floor0LightWallsLength} м.";
            //Tb_Floor0BreakWallsLength.Text = $"Разрывы в несущей стене > 2 м. - {House.Floor0BreakWallsLength} м.";
            //Tb_Floor0OutDoorsLength.Text = $"Двери металлические в несущих стенах - {House.Floor0OutDoorsLength} м.";
            //Tb_Floor0OutDoorsCount.Text = $"Двери металлические в несущих стенах - {House.Floor0OutDoorsCount} шт.";
            //Tb_Floor0InnerDoorsLength.Text = $"Двери межкомнатные в несущих стенах - {House.Floor0InnerDoorsLength} м.";
            //Tb_Floor0InnerDoorsCount.Text = $"Двери межкомнатные в несущих стенах - {House.Floor0InnerDoorsCount} шт.";
            //Tb_Floor0PartitionsDoorsLength.Text = $"Двери межкомнатные в перегородках - {House.Floor0PartitionsDoorsLength} м.";
            //Tb_Floor0PartitionsDoorsCount.Text = $"Двери межкомнатные в перегородках - {House.Floor0PartitionsDoorsCount} шт.";
            //Tb_Floor0GatesLength.Text = $"Ворота - {House.Floor0GatesLength} м.";
            //Tb_Floor0GatesCount.Text = $"Ворота - {House.Floor0GatesCount} шт.";
            //Tb_Floor0TerassesSquare.Text = $"Площадь терасс и крылец - {House.Floor0TerassesSquare} кв.м.";
            //Tb_Floor0InnerTerassesLength.Text = $"Длина внешних стен терасс и крылец - {House.Floor0InnerTerassesLength} м.";
            //Tb_Floor0TerassesLength.Text = $"Длина терасс и крылец - {House.Floor0TerassesLength} м.";
            //Tb_Floor0RailingsLength.Text = $"Длина перил и ограждений - {House.Floor0RailingsLength} м.";

            //Tb_Floor1BadroomSquare.Text = $"Площадь туалетов и ванных комнтат - {House.Floor1BadroomSquare} кв.м.";
            //Tb_Floor1TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor1TilePerimeter} м.";
            //Tb_Floor1TileSquare.Text = $"Прочие помещения в кафеле - {House.Floor1TileSquare} кв.м.";
            //Tb_Floor1TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor1TilePerimeter} м.";
            //Tb_Floor1OutWallsLength.Text = $"Длина внешних несущих стен - {House.Floor1OutWallsLength} м.";
            //Tb_Floor1InnerWallsLength.Text = $"Длина внутренних несущих стен - {House.Floor1InnerWallsLength} м.";
            //Tb_Floor1LightWallsLength.Text = $"Длина перегородок - {House.Floor1LightWallsLength} м.";
            //Tb_Floor1BreakWallsLength.Text = $"Разрывы в несущей стене > 2 м. - {House.Floor1BreakWallsLength} м.";
            //Tb_Floor1OutDoorsLength.Text = $"Двери металлические в несущих стенах - {House.Floor1OutDoorsLength} м.";
            //Tb_Floor1OutDoorsCount.Text = $"Двери металлические в несущих стенах - {House.Floor1OutDoorsCount} шт.";
            //Tb_Floor1InnerDoorsLength.Text = $"Двери межкомнатные в несущих стенах - {House.Floor1InnerDoorsLength} м.";
            //Tb_Floor1InnerDoorsCount.Text = $"Двери межкомнатные в несущих стенах - {House.Floor1InnerDoorsCount} шт.";
            //Tb_Floor1PartitionsDoorsLength.Text = $"Двери межкомнатные в перегородках - {House.Floor1PartitionsDoorsLength} м.";
            //Tb_Floor1PartitionsDoorsCount.Text = $"Двери межкомнатные в перегородках - {House.Floor1PartitionsDoorsCount} шт.";
            //Tb_Floor1GatesLength.Text = $"Ворота - {House.Floor1GatesLength} м.";
            //Tb_Floor1GatesCount.Text = $"Ворота - {House.Floor1GatesCount} шт.";
            //Tb_Floor1TerassesSquare.Text = $"Площадь терасс и крылец - {House.Floor1TerassesSquare} кв.м.";
            //Tb_Floor1InnerTerassesLength.Text = $"Длина внешних стен терасс и крылец - {House.Floor1InnerTerassesLength} м.";
            //Tb_Floor1TerassesLength.Text = $"Длина терасс и крылец - {House.Floor1TerassesLength} м.";
            //Tb_Floor1RailingsLength.Text = $"Длина перил и ограждений - {House.Floor1RailingsLength} м.";

            //Tb_Floor2РHoleSecondLight.Text = $"Дырка в полу под второй свет - {House.Floor2РHoleSecondLight} кв.м.";
            //Tb_Floor2BadroomSquare.Text = $"Площадь туалетов и ванных комнтат - {House.Floor2BadroomSquare} кв.м.";
            //Tb_Floor2TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor2TilePerimeter} м.";
            //Tb_Floor2TileSquare.Text = $"Прочие помещения в кафеле - {House.Floor2TileSquare} кв.м.";
            //Tb_Floor2TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor2TilePerimeter} м.";
            //Tb_Floor2OutWallsLength.Text = $"Длина внешних несущих стен - {House.Floor2OutWallsLength} м.";
            //Tb_Floor2InnerWallsLength.Text = $"Длина внутренних несущих стен - {House.Floor2InnerWallsLength} м.";
            //Tb_Floor2LightWallsLength.Text = $"Длина перегородок - {House.Floor2LightWallsLength} м.";
            //Tb_Floor2BreakWallsLength.Text = $"Разрывы в несущей стене > 2 м. - {House.Floor2BreakWallsLength} м.";
            //Tb_Floor2OutDoorsLength.Text = $"Двери металлические в несущих стенах - {House.Floor2OutDoorsLength} м.";
            //Tb_Floor2OutDoorsCount.Text = $"Двери металлические в несущих стенах - {House.Floor2OutDoorsCount} шт.";
            //Tb_Floor2InnerDoorsLength.Text = $"Двери межкомнатные в несущих стенах - {House.Floor2InnerDoorsLength} м.";
            //Tb_Floor2InnerDoorsCount.Text = $"Двери межкомнатные в несущих стенах - {House.Floor2InnerDoorsCount} шт.";
            //Tb_Floor2PartitionsDoorsLength.Text = $"Двери межкомнатные в перегородках - {House.Floor2PartitionsDoorsLength} м.";
            //Tb_Floor2PartitionsDoorsCount.Text = $"Двери межкомнатные в перегородках - {House.Floor2PartitionsDoorsCount} шт.";
            //Tb_Floor2BalconySquare.Text = $"Площадь балконов - {House.Floor2BalconySquare} кв.м.";
            //Tb_Floor2BalconyLength.Text = $"Длина балконов - {House.Floor2BalconyLength} м.";
            //Tb_Floor2RailingsLength.Text = $"Длина перил и ограждений - {House.Floor2RailingsLength} м.";

            //Tb_Floor3РHoleSecondLight.Text = $"Дырка в полу под второй свет - {House.Floor3РHoleSecondLight} кв.м.";
            //Tb_Floor3BadroomSquare.Text = $"Площадь туалетов и ванных комнтат - {House.Floor3BadroomSquare} кв.м.";
            //Tb_Floor3TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor3TilePerimeter} м.";
            //Tb_Floor3TileSquare.Text = $"Прочие помещения в кафеле - {House.Floor3TileSquare} кв.м.";
            //Tb_Floor3TilePerimeter.Text = $"Периметр комнат с кафелем - {House.Floor3TilePerimeter} м.";
            //Tb_Floor3OutWallsLength.Text = $"Длина внешних несущих стен - {House.Floor3OutWallsLength} м.";
            //Tb_Floor3InnerWallsLength.Text = $"Длина внутренних несущих стен - {House.Floor3InnerWallsLength} м.";
            //Tb_Floor3LightWallsLength.Text = $"Длина перегородок - {House.Floor3LightWallsLength} м.";
            //Tb_Floor3BreakWallsLength.Text = $"Разрывы в несущей стене > 3 м. - {House.Floor3BreakWallsLength} м.";
            //Tb_Floor3OutDoorsLength.Text = $"Двери металлические в несущих стенах - {House.Floor3OutDoorsLength} м.";
            //Tb_Floor3OutDoorsCount.Text = $"Двери металлические в несущих стенах - {House.Floor3OutDoorsCount} шт.";
            //Tb_Floor3InnerDoorsLength.Text = $"Двери межкомнатные в несущих стенах - {House.Floor3InnerDoorsLength} м.";
            //Tb_Floor3InnerDoorsCount.Text = $"Двери межкомнатные в несущих стенах - {House.Floor3InnerDoorsCount} шт.";
            //Tb_Floor3PartitionsDoorsLength.Text = $"Двери межкомнатные в перегородках - {House.Floor3PartitionsDoorsLength} м.";
            //Tb_Floor3PartitionsDoorsCount.Text = $"Двери межкомнатные в перегородках - {House.Floor3PartitionsDoorsCount} шт.";
            //Tb_Floor3BalconySquare.Text = $"Площадь балконов - {House.Floor3BalconySquare} кв.м.";
            //Tb_Floor3BalconyLength.Text = $"Длина балконов - {House.Floor3BalconyLength} м.";
            //Tb_Floor3RailingsLength.Text = $"Длина перил и ограждений - {House.Floor3RailingsLength} м.";
            #endregion
            SetFloorsHouse.SelectedIndex = 0;
            RoofType.SelectedIndex = 0;
        }
        private void Btn_SetRange_Click(object sender, RoutedEventArgs e)
        {
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
        private void ClearAllMainLine()
        {
            CanvasForMainShape.Children.Clear();
        }
        private void Btn_DeleteLastLine_Click(object sender, RoutedEventArgs e)
        {
            DeleteLastShape();
        }

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
                // HACK : Убрано принудительное стирание линий при смене типа объекта
                //   ClearAllMainLine();
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
            if (Tb_WindowSquare.Text.Length > 0)
                House.WindowSquare = double.Parse(Tb_WindowSquare.Text);
        }
        private void Tb_Set0FloorSquare_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Tb_Set0FloorSquare.Text.Length > 0)
                House.Floor0Square = double.Parse(Tb_Set0FloorSquare.Text);
        }
        private void Tb_SetFloor0BadroomCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Tb_SetFloor0BadroomCount.Text.Length > 0)
                House.Floor0BadroomCount = int.Parse(Tb_SetFloor0BadroomCount.Text);
        }
        private void Tb_SetFloor1Square_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Tb_SetFloor1Square.Text.Length > 0)
                House.Floor1Square = int.Parse(Tb_SetFloor1Square.Text);
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
            if (Tb_SetFloor2Square.Text.Length > 0)
                House.Floor2Square = double.Parse(Tb_SetFloor2Square.Text);
        }
        private void Tb_SetFloor2BadroomCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Tb_SetFloor2BadroomCount.Text.Length > 0)
                House.Floor2BadroomCount = int.Parse(Tb_SetFloor2BadroomCount.Text);
        }
        private void Tb_SetFloor3Square_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Tb_SetFloor3Square.Text.Length > 0)
                House.Floor3Square = double.Parse(Tb_SetFloor3Square.Text);
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
        private void Btn_Set0FloorHeight_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0Height);
        }
        private void Btn_Set1FloorHeight_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1Height);
        }
        private void Btn_Set2FloorHeight_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2Height);
        }
        private void Btn_Set3FloorHeight_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3Height);
        }
        private void Btn_SetKitchenSquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.KitchensSquare);
        }
        private void Btn_SetFullHouseHeight_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.RoofHeight);
        }
        private void Btn_SetRoofMinWallHeight_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.RoofMinWallHeight);
        }
        private void Btn_SetRoofSquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.RoofSquare);
        }
        private void Btn_SetRoofLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.RoofLength);
        }
        private void Btn_SetCanopySquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.CanopySquare);
        }
        private void Btn_SetCanopyLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.CanopyLength);
        }
        private void Btn_SetPergolaSquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.PergolaSquare);
        }
        private void Btn_SetHemmingButt_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.HemmingButt);
        }
        private void Btn_SetHemmingOverhangsSquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.HemmingOverhangsSquare);
        }


        #endregion
        #region Выбор объектов вкладки "Фасады"
        private void Btn_Set0FloorGlaseQ_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor0GlaseSq);
        }
        private void Btn_Set0FloorGlaseT_Click(object sender, RoutedEventArgs e)
        {
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor0GlaseSq);
        }
        private void Btn_Set1FloorGlaseQ_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor1GlaseSq);
        }
        private void Btn_Set1FloorGlaseT_Click(object sender, RoutedEventArgs e)
        {
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor1GlaseSq);
        }
        private void Btn_Set2FloorGlaseQ_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor2GlaseSq);
        }
        private void Btn_Set2FloorGlaseT_Click(object sender, RoutedEventArgs e)
        {
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor2GlaseSq);
        }
        private void Btn_Set3FloorGlaseQ_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor3GlaseSq);
        }
        private void Btn_Set3FloorGlaseT_Click(object sender, RoutedEventArgs e)
        {
            SelectTriangleObj((byte)GlobalVariables.ProjectObjEnum.Floor3GlaseSq);
        }

        #endregion
        #region Выбор объектов вкладки "Этажи"
        private void Btn_SetFloor0PlinthHeight_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0PlinthHeight);
        }
        private void Btn_SetFloor0BadroomSquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor0BadroomSquare);
        }
        private void Btn_SetFloor0TileSquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor0TileSquare);
        }
        private void Btn_SetFloor0OutWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0OutWallsLength);
        }
        private void Btn_SetFloor0InnerWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0InnerWallsLength);
        }
        private void Btn_SetFloor0LightWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0LightWallsLength);
        }
        private void Btn_SetFloor0BreakWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0BreakWallsLength);
        }
        private void Btn_SetFloor0OutDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0OutDoorsLength);
        }
        private void Btn_SetFloor0InnerDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0InnerDoorsLength);
        }
        private void Btn_SetFloor0PartitionsDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0PartitionsDoorsLength);
        }
        private void Btn_SetFloor0GatesLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0GatesLength);
        }
        private void Btn_SetFloor0TerassesSquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor0TerassesSquare);
        }
        private void Btn_SetFloor0InnerTerassesLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0InnerTerassesLength);
        }
        private void Btn_SetFloor0TerassesLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0TerassesLength);
        }
        private void Btn_SetFloor0RailingsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor0RailingsLength);
        }

        private void Btn_SetFloor1BadroomSquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor1BadroomSquare);
        }
        private void Btn_SetFloor1TileSquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor1TileSquare);
        }
        private void Btn_SetFloor1OutWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1OutWallsLength);
        }
        private void Btn_SetFloor1InnerWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1InnerWallsLength);
        }
        private void Btn_SetFloor1LightWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1LightWallsLength);
        }
        private void Btn_SetFloor1BreakWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1BreakWallsLength);
        }
        private void Btn_SetFloor1OutDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1OutDoorsLength);
        }
        private void Btn_SetFloor1InnerDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1InnerDoorsLength);
        }
        private void Btn_SetFloor1PartitionsDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1PartitionsDoorsLength);
        }
        private void Btn_SetFloor1GatesLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1GatesLength);
        }
        private void Btn_SetFloor1TerassesSquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor1TerassesSquare);
        }
        private void Btn_SetFloor1InnerTerassesLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1InnerTerassesLength);
        }
        private void Btn_SetFloor1TerassesLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1TerassesLength);
        }
        private void Btn_SetFloor1RailingsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor1RailingsLength);
        }

        private void Btn_SetFloor2РHoleSecondLight_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor2РHoleSecondLight);
        }
        private void Btn_SetFloor2BadroomSquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor2BadroomSquare);
        }
        private void Btn_SetFloor2TileSquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor2TileSquare);
        }
        private void Btn_SetFloor2OutWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2OutWallsLength);
        }
        private void Btn_SetFloor2InnerWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2InnerWallsLength);
        }
        private void Btn_SetFloor2LightWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2LightWallsLength);
        }
        private void Btn_SetFloor2BreakWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2BreakWallsLength);
        }
        private void Btn_SetFloor2OutDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2OutDoorsLength);
        }
        private void Btn_SetFloor2InnerDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2InnerDoorsLength);
        }
        private void Btn_SetFloor2PartitionsDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2PartitionsDoorsLength);
        }
        private void Btn_SetFloor2BalconySquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor2BalconySquare);
        }
        private void Btn_SetFloor2BalconyLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2BalconyLength);
        }

        private void Btn_SetFloor2RailingsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor2RailingsLength);
        }
        private void Btn_SetFloor3РHoleSecondLight_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor3РHoleSecondLight);
        }
        private void Btn_SetFloor3BadroomSquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor3BadroomSquare);
        }
        private void Btn_SetFloor3TileSquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor3TileSquare);
        }
        private void Btn_SetFloor3OutWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3OutWallsLength);
        }
        private void Btn_SetFloor3InnerWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3InnerWallsLength);
        }
        private void Btn_SetFloor3LightWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3LightWallsLength);
        }
        private void Btn_SetFloor3BreakWallsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3BreakWallsLength);
        }
        private void Btn_SetFloor3OutDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3OutDoorsLength);
        }
        private void Btn_SetFloor3InnerDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3InnerDoorsLength);
        }
        private void Btn_SetFloor3PartitionsDoorsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3PartitionsDoorsLength);
        }
        private void Btn_SetFloor3BalconySquare_Click(object sender, RoutedEventArgs e)
        {
            SelectRectObj((byte)GlobalVariables.ProjectObjEnum.Floor3BalconySquare);
        }
        private void Btn_SetFloor3BalconyLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3BalconyLength);
        }
        private void Btn_SetFloor3RailingsLength_Click(object sender, RoutedEventArgs e)
        {
            SelectLineObj((byte)GlobalVariables.ProjectObjEnum.Floor3RailingsLength);
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
            CanvasForMainShape.Children.Clear();

            foreach (var tb in FindVisualChildren<TextBox>(window))
            {
                tb.Text = "";
            }

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
        private void ClearAllMainLine_Click(object sender, RoutedEventArgs e)
        {
            ClearAllMainLine();
        }
        private void SendDataToGoogle(object sender, RoutedEventArgs e)
        {
            if (House.ProjectName != null)
                GoogleSheets.SaveData();
        }

        private void LoadProjectData(object sender, RoutedEventArgs e)
        {
            // TODO : включить, как допишу присвоение
           // GoogleSheets.LoadData();
        }
    }
}
