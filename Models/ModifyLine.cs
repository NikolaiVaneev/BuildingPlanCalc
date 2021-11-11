using BuildingPlanCalc.Interfaces;
using BuildingPlanCalc.Services;
using System;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BuildingPlanCalc.Models
{
    class ModifyLine : IShape
    {
        public Line Line { get; set; }
        public double Length { get; set; }
        public byte ObjType { get; set; }
        public double Square { get; set; }
        public double Perimetr { get; set; }
        public byte ShapeType { get; set; }
        public string ParrentCanvasName { get; set; }
        public ModifyLine(Line line, byte buildingObjType, byte shapeType, Brush brush, double koef, string parrentCanvasName)
        {
            ParrentCanvasName = parrentCanvasName;
            ShapeType = shapeType;
            Line = line;
            ObjType = buildingObjType;

            Length = Math.Sqrt(Math.Pow(Line.X2 - Line.X1, 2) + Math.Pow(Line.Y2 - Line.Y1, 2)) / koef / 1000;
            Line.Stroke = brush;
            Line.StrokeThickness = Settings.ShapeThickness;
        }
    }
}
