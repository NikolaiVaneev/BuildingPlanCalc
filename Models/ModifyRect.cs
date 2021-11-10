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
        public ModifyRect(Rectangle rect, byte buildingObjType, byte shapeType, double koef)
        {
            ShapeType = shapeType;
            Rectangle = rect;
            ObjType = buildingObjType;


            string colorName;
            switch (buildingObjType)
            {
                case (byte)GlobalVariables.ProjectObjEnum.Floor0GlaseSq:
                    colorName = Settings.ShapeColorOrange;
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor1GlaseSq:
                    colorName = Settings.ShapeColorLightGreen;
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2GlaseSq:
                    colorName = Settings.ShapeColorYellow;
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor3GlaseSq:
                    colorName = Settings.ShapeColorDefault;
                    break;

                case (byte)GlobalVariables.ProjectObjEnum.KitchensSquare:
                    colorName = Settings.ShapeColorYellow;
                    break;
                default:
                    colorName = Settings.ShapeColorDefault;
                    break;
            }


            Rectangle.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(colorName);
            Rectangle.StrokeThickness = Settings.ShapeThickness;
            Perimetr = (Rectangle.Width * 2 / koef / 1000) + (Rectangle.Height * 2 / koef / 1000);
            Square = Rectangle.Width / koef / 1000 * (Rectangle.Height / koef / 1000);
        }
    }
}
