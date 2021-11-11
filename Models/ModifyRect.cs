using BuildingPlanCalc.Interfaces;
using BuildingPlanCalc.Services;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BuildingPlanCalc.Models
{
    class ModifyRect : IShape
    {
        public Rectangle Rectangle { get; set; }
        public double Length { get; set; }
        public byte ObjType { get; set; }
        public double Square { get; set; }
        public double Perimetr { get; set; }
        public byte ShapeType { get; set; }
        public string ParrentCanvasName { get; set; }
        public ModifyRect(Rectangle rect, byte buildingObjType, byte shapeType, Brush brush, double koef, string parrentCanvasName)
        {
            ParrentCanvasName = parrentCanvasName;
            ShapeType = shapeType;
            Rectangle = rect;
            ObjType = buildingObjType;

            Rectangle.Stroke = brush;
            Rectangle.StrokeThickness = Settings.ShapeThickness;
            Perimetr = (Rectangle.Width * 2 / koef / 1000) + (Rectangle.Height * 2 / koef / 1000);
            Square = Rectangle.Width / koef / 1000 * (Rectangle.Height / koef / 1000);
        }
    }
}
