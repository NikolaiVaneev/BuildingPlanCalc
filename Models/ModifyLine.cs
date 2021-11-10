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
        public ModifyLine(Line line, byte buildingObjType, byte shapeType, double koef)
        {
            ShapeType = shapeType;
            Line = line;
            ObjType = buildingObjType;

            string colorName;
            switch (buildingObjType)
            {
                case (byte)GlobalVariables.ProjectObjEnum.Floor0OutWallsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor1OutWallsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor2OutWallsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor3OutWallsLength:
                    colorName = Settings.ShapeColorLightRed;
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0InnerWallsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor1InnerWallsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor2InnerWallsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor3InnerWallsLength:
                    colorName = Settings.ShapeColorGreen;
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0LightWallsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor1LightWallsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor2LightWallsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor3LightWallsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor0Height:
                case (byte)GlobalVariables.ProjectObjEnum.Floor0RailingsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor1RailingsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor2RailingsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor3RailingsLength:
                    colorName = Settings.ShapeColorOrange;
                    break;

                case (byte)GlobalVariables.ProjectObjEnum.Floor0InnerTerassesLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor1InnerTerassesLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor2BalconyLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor3BalconyLength:
                    colorName = Settings.ShapeColorBlue;
                    break;

                case (byte)GlobalVariables.ProjectObjEnum.Floor0OutDoorsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor1OutDoorsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor2OutDoorsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor3OutDoorsLength:
                    colorName = Settings.ShapeColorRed;
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0InnerDoorsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor1InnerDoorsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor2InnerDoorsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor3InnerDoorsLength:
                    colorName = Settings.ShapeColorLightGreen;
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor0PartitionsDoorsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor1PartitionsDoorsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor2PartitionsDoorsLength:
                case (byte)GlobalVariables.ProjectObjEnum.Floor3PartitionsDoorsLength:
                    colorName = Settings.ShapeColorLightOrange;
                    break;

                case (byte)GlobalVariables.ProjectObjEnum.Floor1Height:
                    colorName = Settings.ShapeColorLightGreen;
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.Floor2Height:
                    colorName = Settings.ShapeColorYellow;
                    break;
                case (byte)GlobalVariables.ProjectObjEnum.RoofHeight:
                    colorName = Settings.ShapeColorPurpure;
                    break;
                default:
                    colorName = Settings.ShapeColorDefault;
                    break;
            }

            Length = Math.Sqrt(Math.Pow(Line.X2 - Line.X1, 2) + Math.Pow(Line.Y2 - Line.Y1, 2)) / koef / 1000;
            Line.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(colorName);
            Line.StrokeThickness = Settings.ShapeThickness;
        }
    }
}
