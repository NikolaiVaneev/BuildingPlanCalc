using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingPlanCalc.Interfaces
{
    internal interface IShape
    {
        double Length { get; set; }
        byte ObjType { get; set; }
        double Square { get; set; }
        double Perimetr { get; set; }

        byte ShapeType { get; set; }

    }
}
