using BuildingPlanCalc.Interfaces;
using BuildingPlanCalc.Services;
using System;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BuildingPlanCalc.Models
{
    class ModifyTriangle : IShape
    {
        public Polygon Triangle { get; set; }
        public double Length { get; set; }
        public byte ObjType { get; set; }
        public double Square { get; set; }
        public double Perimetr { get; set; }
        public byte ShapeType { get; set; }
        public string ParrentCanvasName { get; set; }
        public ModifyTriangle(Polygon triangle, byte buildingObjType, byte shapeType, Brush brush, double koef, string parrentCanvasName)
        {
            ShapeType = shapeType;
            Triangle = triangle;
            ObjType = buildingObjType;
            ParrentCanvasName = parrentCanvasName;

            Triangle.Stroke = brush;
            Triangle.StrokeThickness = Settings.ShapeThickness;

            double a = Math.Sqrt(Math.Pow(triangle.Points[0].X - triangle.Points[1].X, 2) + Math.Pow(triangle.Points[0].Y - triangle.Points[1].Y, 2)) / koef / 1000;
            double b = Math.Sqrt(Math.Pow(triangle.Points[1].X - triangle.Points[2].X, 2) + Math.Pow(triangle.Points[1].Y - triangle.Points[2].Y, 2)) / koef / 1000;
            double c = Math.Sqrt(Math.Pow(triangle.Points[2].X - triangle.Points[0].X, 2) + Math.Pow(triangle.Points[2].Y - triangle.Points[0].Y, 2)) / koef / 1000;

            Length = Math.Max(a, Math.Max(b, c)) * koef / 100;
            Perimetr = a + b + c;
            double p = Perimetr / 2;
            Square = Math.Sqrt(p * (p - a) * (p - b) * (p - c));
        }
    }
}
