using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingPlanCalc.Models
{
    public static class House
    {
        /// <summary>
        /// Номер проекта A
        /// </summary>
        public static string ProjectName { get; set; }
        /// <summary>
        /// Имя менеджера B
        /// </summary>
        public static string ManagerName { get; set; }
        /// <summary>
        /// Количество этажей С
        /// </summary>
        public static int FloorsCount { get; set; } = 1;

        /// <summary>
        /// Количество комнат D
        /// </summary>
        public static int RoomCount { get; set; }
        /// <summary>
        /// Количество спален E
        /// </summary>
        public static int BedroomCount { get; set; }

        /// <summary>
        /// Площадь кухонь и гостиных F
        /// </summary>
        public static double KitchensSquare { get; set; }

        /// <summary>
        /// Высота цокольного этажа G
        /// </summary>
        public static double Floor0Height { get; set; }
        /// <summary>
        /// Высота первого этажа H
        /// </summary>
        public static double Floor1Height { get; set; }
        /// <summary>
        /// Высота второго этажа I
        /// </summary>
        public static double Floor2Height { get; set; }
        /// <summary>
        /// Высота третьего этажа J
        /// </summary>
        public static double Floor3Height { get; set; }
        /// <summary>
        /// Высота до конька крыши K
        /// </summary>
        public static double RoofHeight { get; set; }
        /// <summary>
        /// Минимальная высота стен последнего этажа L
        /// </summary>
        public static double RoofMinWallHeight { get; set; }
        /// <summary>
        /// Форма кровли M
        /// </summary>
        public static string RoofType { get; set; }
        /// <summary>
        /// Площадь накрытая основной кровлей N
        /// </summary>
        public static double RoofSquare { get; set; }
        /// <summary>
        /// Длина основной кровли O
        /// </summary>
        public static double RoofLength { get; set; }
        /// <summary>
        /// Площадь накрытая навесами P
        /// </summary>
        public static double CanopySquare { get; set; }
        /// <summary>
        /// Длина навесов вдоль общей стены Q
        /// </summary>
        public static double CanopyLength { get; set; }
        /// <summary>
        /// Площадь Перголы R
        /// </summary>
        public static double PergolaSquare { get; set; }
        /// <summary>
        /// Подшива торцов основной кровли S
        /// </summary>
        public static double HemmingButt { get; set; }
        /// <summary>
        /// Подшива свесов оснвоной кровли T
        /// </summary>
        public static double HemmingOverhangsSquare { get; set; }
        // Второй раздел
        /// <summary>
        /// Цокольный этаж. Площадь дверей и окон U
        /// </summary>
        public static double Floor0GlaseSq { get; set; }
        /// <summary>
        /// Цокольный этаж. Периметр дверей и окон V
        /// </summary>
        public static double Floor0GlaseP { get; set; }
        /// <summary>
        /// Первый этаж. Площадь дверей и окон W
        /// </summary>
        public static double Floor1GlaseSq { get; set; }
        /// <summary>
        /// Первый этаж. Периметр дверей и окон X
        /// </summary>
        public static double Floor1GlaseP { get; set; }
        /// <summary>
        /// Второй этаж. Площадь дверей и окон Y
        /// </summary>
        public static double Floor2GlaseSq { get; set; }
        /// <summary>
        /// Второй этаж. Периметр дверей и окон Z
        /// </summary>
        public static double Floor2GlaseP { get; set; }
        /// <summary>
        /// Третий этаж. Площадь дверей и окон AA
        /// </summary>
        public static double Floor3GlaseSq { get; set; }
        /// <summary>
        /// Третий этаж. Периметр дверей и окон AB
        /// </summary>
        public static double Floor3GlaseP { get; set; }
        /// <summary>
        /// Количество мансардных окон AC
        /// </summary>
        public static int WindowCount { get; set; }
        /// <summary>
        /// Площадь мансардных окон AD
        /// </summary>
        public static double WindowSquare { get; set; }
        
        // цокольный этаж
        /// <summary>
        /// Длина всех несущих стен на цокольном этаже AE
        /// </summary>
        public static double Floor0OutWallsLength { get; set; }
        /// <summary>
        /// Длина внутренних стен на цокольном этаже AF
        /// </summary>
        public static double Floor0InnerWallsLength { get; set; }
        /// <summary>
        /// Длина перегородок на цокольном этаже AG
        /// </summary>
        public static double Floor0LightWallsLength { get; set; }
        /// <summary>
        /// Разрывы в стене цокольного этажа АН
        /// </summary>
        public static double Floor0BreakWallsLength { get; set; }
        /// <summary>
        /// Площадь цокольного этажа AI
        /// </summary>
        public static double Floor0Square { get; set; }
        /// <summary>
        /// Количество металлических дверей в несущих стенах AJ
        /// </summary>
        public static int Floor0OutDoorsCount { get; set; }
        /// <summary>
        /// Длина металлических дверей в несущих стенах AK
        /// </summary>
        public static double Floor0OutDoorsLength { get; set; }
        /// <summary>
        /// Количество межкомнатных дверей в несущих стенах AL
        /// </summary>
        public static int Floor0InnerDoorsCount { get; set; }
        /// <summary>
        /// Длина межкомнатных дверей в несущих стенах AM
        /// </summary>
        public static double Floor0InnerDoorsLength { get; set; }
        /// <summary>
        /// Количество межкомнатных дверей в перегородках AN
        /// </summary>
        public static int Floor0PartitionsDoorsCount { get; set; }
        /// <summary>
        /// Длина межкомнатных дверей в перегородках AO
        /// </summary>
        public static double Floor0PartitionsDoorsLength { get; set; }
        /// <summary>
        /// Количество ворот AР
        /// </summary>
        public static int Floor0GatesCount { get; set; }
        /// <summary>
        /// Длина ворот AQ
        /// </summary>
        public static double Floor0GatesLength { get; set; }
        /// <summary>
        /// Площадь террасс AR
        /// </summary>
        public static double Floor0TerassesSquare { get; set; }
        /// <summary>
        /// Длина внешних террас и крылец AS
        /// </summary>
        public static double Floor0InnerTerassesLength { get; set; }
        /// <summary>
        /// Длина террасс и крылец AT
        /// </summary>
        public static double Floor0TerassesLength { get; set; }
        /// <summary>
        /// Длина перил и ограждений AU
        /// </summary>
        public static double Floor0RailingsLength { get; set; }
        /// <summary>
        /// Высота цоколя AV
        /// </summary>
        public static double Floor0PlinthHeight { get; set; }
        /// <summary>
        /// Сбоку цоколь открыт на % AW
        /// </summary>
        public static int PlinthOpenPerc { get; set; }
        /// <summary>
        /// Туалеты и ванные на цокольном этаже AX
        /// </summary>
        public static int Floor0BadroomCount { get; set; }
        /// <summary>
        /// Площадь туалетов и ванных на цокольном этаже AY
        /// </summary>
        public static double Floor0BadroomSquare { get; set; }
        /// <summary>
        /// Прочие помещения в кафеле AZ
        /// </summary>
        public static double Floor0TileSquare { get; set; }
        /// <summary>
        /// Периметр комнат с кафелем BA
        /// </summary>
        public static double Floor0TilePerimeter { get; set; }
        // 1 этаж
        /// <summary>
        /// Длина всех несущих стен на 1 этаже BB
        /// </summary>
        public static double Floor1OutWallsLength { get; set; }
        /// <summary>
        /// Длина внутренних стен на 1 этаже BC
        /// </summary>
        public static double Floor1InnerWallsLength { get; set; }
        /// <summary>
        /// Длина перегородок на 1 этаже BD
        /// </summary>
        public static double Floor1LightWallsLength { get; set; }
        /// <summary>
        /// Разрывы в стене 1 этажа BE
        /// </summary>
        public static double Floor1BreakWallsLength { get; set; }
        /// <summary>
        /// Площадь первого этажа BF
        /// </summary>
        public static double Floor1Square { get; set; }
        /// <summary>
        /// Количество металлических дверей в несущих стенах BG
        /// </summary>
        public static int Floor1OutDoorsCount { get; set; }
        /// <summary>
        /// Длина металлических дверей в несущих стенах BH
        /// </summary>
        public static double Floor1OutDoorsLength { get; set; }
        /// <summary>
        /// Количество межкомнатных дверей в несущих стенах BI
        /// </summary>
        public static int Floor1InnerDoorsCount { get; set; }
        /// <summary>
        /// Длина межкомнатных дверей в несущих стенах BJ
        /// </summary>
        public static double Floor1InnerDoorsLength { get; set; }
        /// <summary>
        /// Количество межкомнатных дверей в перегородках BK
        /// </summary>
        public static int Floor1PartitionsDoorsCount { get; set; }
        /// <summary>
        /// Длина межкомнатных дверей в перегородках BL
        /// </summary>
        public static double Floor1PartitionsDoorsLength { get; set; }
        /// <summary>
        /// Количество ворот BM
        /// </summary>
        public static int Floor1GatesCount { get; set; }
        /// <summary>
        /// Длина ворот BN
        /// </summary>
        public static double Floor1GatesLength { get; set; }
        /// <summary>
        /// Площадь террасс BO
        /// </summary>
        public static double Floor1TerassesSquare { get; set; }
        /// <summary>
        /// Длина внешних террас и крылец BP
        /// </summary>
        public static double Floor1InnerTerassesLength { get; set; }
        /// <summary>
        /// Длина террасс и крылец BQ
        /// </summary>
        public static double Floor1TerassesLength { get; set; }
        /// <summary>
        /// Длина перил и ограждений BR
        /// </summary>
        public static double Floor1RailingsLength { get; set; }
        /// <summary>
        /// Декаративные столбы высотой до 3 метров BS
        /// </summary>
        public static double Floor1DecatativePillarsLessCount { get; set; }
        /// <summary>
        /// Декоратвные столбы высотой более 3 метров BT
        /// </summary>
        public static double Floor1DecatativePillarsOverCount { get; set; }
        /// <summary>
        /// Туалеты и ванные на 1 этаже BU
        /// </summary>
        public static int Floor1BadroomCount { get; set; }
        /// <summary>
        /// Площадь туалетов и ванных на 1 этаже BV
        /// </summary>
        public static double Floor1BadroomSquare { get; set; }
        /// <summary>
        /// Прочие помещения в кафеле 1 этаж BW
        /// </summary>
        public static double Floor1TileSquare { get; set; }
        /// <summary>
        /// Периметр комнат с кафелем 1 этажа BX
        /// </summary>
        public static double Floor1TilePerimeter { get; set; }
        // 2 этаж
        /// <summary>
        /// Длина всех несущих стен на 2 этаже BY
        /// </summary>
        public static double Floor2OutWallsLength { get; set; }
        /// <summary>
        /// Длина внутренних стен на 2 этаже BZ
        /// </summary>
        public static double Floor2InnerWallsLength { get; set; }
        /// <summary>
        /// Длина перегородок на 2 этаже CA
        /// </summary>
        public static double Floor2LightWallsLength { get; set; }
        /// <summary>
        /// Разрывы в стене 2 этажа CB
        /// </summary>
        public static double Floor2BreakWallsLength { get; set; }
        /// <summary>
        /// Площадь 2 этажа CC
        /// </summary>
        public static double Floor2Square { get; set; }
        /// <summary>
        /// Дырка под второй свет 2 этажа СD
        /// </summary>
        public static double Floor2РHoleSecondLight { get; set; }
        /// <summary>
        /// Количество металлических дверей в несущих стенах CE
        /// </summary>
        public static int Floor2OutDoorsCount { get; set; }
        /// <summary>
        /// Длина металлических дверей в несущих стенах CF
        /// </summary>
        public static double Floor2OutDoorsLength { get; set; }
        /// <summary>
        /// Количество межкомнатных дверей в несущих стенах CG
        /// </summary>
        public static int Floor2InnerDoorsCount { get; set; }
        /// <summary>
        /// Длина межкомнатных дверей в несущих стенах CH
        /// </summary>
        public static double Floor2InnerDoorsLength { get; set; }
        /// <summary>
        /// Количество межкомнатных дверей в перегородках CI
        /// </summary>
        public static int Floor2PartitionsDoorsCount { get; set; }
        /// <summary>
        /// Длина межкомнатных дверей в перегородках CJ
        /// </summary>
        public static double Floor2PartitionsDoorsLength { get; set; }
        /// <summary>
        /// Площадь балконов CK
        /// </summary>
        public static double Floor2BalconySquare { get; set; }
        /// <summary>
        /// Длина балконов CL
        /// </summary>
        public static double Floor2BalconyLength { get; set; }
        /// <summary>
        /// Длина перил и ограждений CM
        /// </summary>
        public static double Floor2RailingsLength { get; set; }
        /// <summary>
        /// Туалеты и ванные на 2 этаже CN
        /// </summary>
        public static int Floor2BadroomCount { get; set; }
        /// <summary>
        /// Площадь туалетов и ванных на 2 этаже CO
        /// </summary>
        public static double Floor2BadroomSquare { get; set; }
        /// <summary>
        /// Прочие помещения в кафеле 2 этаж CP
        /// </summary>
        public static double Floor2TileSquare { get; set; }
        /// <summary>
        /// Периметр комнат с кафелем 2 этажа CQ
        /// </summary>
        public static double Floor2TilePerimeter { get; set; }
        // 3 этаж
        /// <summary>
        /// Длина всех несущих стен на 3 этаже CR
        /// </summary>
        public static double Floor3OutWallsLength { get; set; }
        /// <summary>
        /// Длина внутренних стен на 3 этаже CS
        /// </summary>
        public static double Floor3InnerWallsLength { get; set; }
        /// <summary>
        /// Длина перегородок на 3 этаже CT
        /// </summary>
        public static double Floor3LightWallsLength { get; set; }
        /// <summary>
        /// Разрывы в стене 3 этажа CU
        /// </summary>
        public static double Floor3BreakWallsLength { get; set; }
        /// <summary>
        /// Площадь 3 этажа CV
        /// </summary>
        public static double Floor3Square { get; set; }
        /// <summary>
        /// Дырка под второй свет 3 этажа CW
        /// </summary>
        public static double Floor3РHoleSecondLight { get; set; }
        /// <summary>
        /// Количество металлических дверей в несущих стенах CX
        /// </summary>
        public static int Floor3OutDoorsCount { get; set; }
        /// <summary>
        /// Длина металлических дверей в несущих стенах CY
        /// </summary>
        public static double Floor3OutDoorsLength { get; set; }
        /// <summary>
        /// Количество межкомнатных дверей в несущих стенах CZ
        /// </summary>
        public static int Floor3InnerDoorsCount { get; set; }
        /// <summary>
        /// Длина межкомнатных дверей в несущих стенах DA
        /// </summary>
        public static double Floor3InnerDoorsLength { get; set; }
        /// <summary>
        /// Количество межкомнатных дверей в перегородках DB
        /// </summary>
        public static int Floor3PartitionsDoorsCount { get; set; }
        /// <summary>
        /// Длина межкомнатных дверей в перегородках DC
        /// </summary>
        public static double Floor3PartitionsDoorsLength { get; set; }
        /// <summary>
        /// Площадь балконов DD
        /// </summary>
        public static double Floor3BalconySquare { get; set; }
        /// <summary>
        /// Длина балконов DE
        /// </summary>
        public static double Floor3BalconyLength { get; set; }
        /// <summary>
        /// Длина перил и ограждений DF
        /// </summary>
        public static double Floor3RailingsLength { get; set; }
        /// <summary>
        /// Туалеты и ванные на 3 этаже DG
        /// </summary>
        public static int Floor3BadroomCount { get; set; }
        /// <summary>
        /// Площадь туалетов и ванных на 3 этаже DH
        /// </summary>
        public static double Floor3BadroomSquare { get; set; }
        /// <summary>
        /// Прочие помещения в кафеле 3 этаж DI
        /// </summary>
        public static double Floor3TileSquare { get; set; }
        /// <summary>
        /// Периметр комнат с кафелем 3 этажа DJ
        /// </summary>
        public static double Floor3TilePerimeter { get; set; }
    }
}
