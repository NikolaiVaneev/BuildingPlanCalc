using System;
using System.Reflection;

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
        public static double Floor0F1GlaseSq { get; set; }
        /// <summary>
        /// Цокольный этаж. Периметр дверей и окон V
        /// </summary>
        public static double Floor0F1GlaseP { get; set; }
        /// <summary>
        /// Первый этаж. Площадь дверей и окон W
        /// </summary>
        public static double Floor1F1GlaseSq { get; set; }
        /// <summary>
        /// Первый этаж. Периметр дверей и окон X
        /// </summary>
        public static double Floor1F1GlaseP { get; set; }
        /// <summary>
        /// Второй этаж. Площадь дверей и окон Y
        /// </summary>
        public static double Floor2F1GlaseSq { get; set; }
        /// <summary>
        /// Второй этаж. Периметр дверей и окон Z
        /// </summary>
        public static double Floor2F1GlaseP { get; set; }
        /// <summary>
        /// Третий этаж. Площадь дверей и окон AA
        /// </summary>
        public static double Floor3F1GlaseSq { get; set; }
        /// <summary>
        /// Третий этаж. Периметр дверей и окон AB
        /// </summary>
        public static double Floor3F1GlaseP { get; set; }

        /// <summary>
        /// Цокольный этаж. Площадь дверей и окон U
        /// </summary>
        public static double Floor0F2GlaseSq { get; set; }
        /// <summary>
        /// Цокольный этаж. Периметр дверей и окон V
        /// </summary>
        public static double Floor0F2GlaseP { get; set; }
        /// <summary>
        /// Первый этаж. Площадь дверей и окон W
        /// </summary>
        public static double Floor1F2GlaseSq { get; set; }
        /// <summary>
        /// Первый этаж. Периметр дверей и окон X
        /// </summary>
        public static double Floor1F2GlaseP { get; set; }
        /// <summary>
        /// Второй этаж. Площадь дверей и окон Y
        /// </summary>
        public static double Floor2F2GlaseSq { get; set; }
        /// <summary>
        /// Второй этаж. Периметр дверей и окон Z
        /// </summary>
        public static double Floor2F2GlaseP { get; set; }
        /// <summary>
        /// Третий этаж. Площадь дверей и окон AA
        /// </summary>
        public static double Floor3F2GlaseSq { get; set; }
        /// <summary>
        /// Третий этаж. Периметр дверей и окон AB
        /// </summary>
        public static double Floor3F2GlaseP { get; set; }

        /// <summary>
        /// Цокольный этаж. Площадь дверей и окон U
        /// </summary>
        public static double Floor0F3GlaseSq { get; set; }
        /// <summary>
        /// Цокольный этаж. Периметр дверей и окон V
        /// </summary>
        public static double Floor0F3GlaseP { get; set; }
        /// <summary>
        /// Первый этаж. Площадь дверей и окон W
        /// </summary>
        public static double Floor1F3GlaseSq { get; set; }
        /// <summary>
        /// Первый этаж. Периметр дверей и окон X
        /// </summary>
        public static double Floor1F3GlaseP { get; set; }
        /// <summary>
        /// Второй этаж. Площадь дверей и окон Y
        /// </summary>
        public static double Floor2F3GlaseSq { get; set; }
        /// <summary>
        /// Второй этаж. Периметр дверей и окон Z
        /// </summary>
        public static double Floor2F3GlaseP { get; set; }
        /// <summary>
        /// Третий этаж. Площадь дверей и окон AA
        /// </summary>
        public static double Floor3F3GlaseSq { get; set; }
        /// <summary>
        /// Третий этаж. Периметр дверей и окон AB
        /// </summary>
        public static double Floor3F3GlaseP { get; set; }

        /// <summary>
        /// Цокольный этаж. Площадь дверей и окон U
        /// </summary>
        public static double Floor0F4GlaseSq { get; set; }
        /// <summary>
        /// Цокольный этаж. Периметр дверей и окон V
        /// </summary>
        public static double Floor0F4GlaseP { get; set; }
        /// <summary>
        /// Первый этаж. Площадь дверей и окон W
        /// </summary>
        public static double Floor1F4GlaseSq { get; set; }
        /// <summary>
        /// Первый этаж. Периметр дверей и окон X
        /// </summary>
        public static double Floor1F4GlaseP { get; set; }
        /// <summary>
        /// Второй этаж. Площадь дверей и окон Y
        /// </summary>
        public static double Floor2F4GlaseSq { get; set; }
        /// <summary>
        /// Второй этаж. Периметр дверей и окон Z
        /// </summary>
        public static double Floor2F4GlaseP { get; set; }
        /// <summary>
        /// Третий этаж. Площадь дверей и окон AA
        /// </summary>
        public static double Floor3F4GlaseSq { get; set; }
        /// <summary>
        /// Третий этаж. Периметр дверей и окон AB
        /// </summary>
        public static double Floor3F4GlaseP { get; set; }

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
        /// Деревянные столбы высотой до 3 метров BS
        /// </summary>
        public static double WoodenPillarsLessCount { get; set; }
        /// <summary>
        /// Деревянные столбы высотой более 3 метров BT
        /// </summary>
        public static double WoodenPillarsOverCount { get; set; }
        /// <summary>
        /// Бетонные столбы высотой до 3 метров DK
        /// </summary>
        public static double ConcretePillarsLessCount { get; set; }
        /// <summary>
        /// Бетонные столбы высотой выше 3 метров DL
        /// </summary>
        public static double ConcretePillarsOverCount { get; set; }
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
        /// <summary>
        /// Площадь дома с сайта
        /// </summary>
        public static double SiteHomeSquare { get; set; }
        /// <summary>
        /// Площадь кровли
        /// </summary>
        public static double RoofAreaSquare { get; set; }
        /// <summary>
        /// Ограждение из газобетона
        /// </summary>
        public static double ConcreteRailingLength { get; set; }

        public static void Reset()
        {
            ManagerName = string.Empty;
            FloorsCount = 0;
            RoomCount = 0;
            BedroomCount = 0;
            KitchensSquare = 0;
            Floor0Height = 0;
            Floor1Height = 0;
            Floor2Height = 0;
            Floor3Height = 0;
            RoofHeight = 0;
            RoofMinWallHeight = 0;
            RoofType = string.Empty;
            RoofSquare = 0;
            RoofLength = 0;
            CanopySquare = 0;
            CanopyLength = 0;
            PergolaSquare = 0;
            HemmingButt = 0;
            HemmingOverhangsSquare = 0;
            Floor0F1GlaseSq = 0;
            Floor0F1GlaseP = 0;
            Floor1F1GlaseSq = 0;
            Floor1F1GlaseP = 0;
            Floor2F1GlaseSq = 0;
            Floor2F1GlaseP = 0;
            Floor3F1GlaseSq = 0;
            Floor3F1GlaseP = 0;
            WindowCount = 0;
            WindowSquare = 0;
            Floor0OutWallsLength = 0;
            Floor0InnerWallsLength = 0;
            Floor0LightWallsLength = 0;
            Floor0BreakWallsLength = 0;
            Floor0Square = 0;
            Floor0OutDoorsCount = 0;
            Floor0OutDoorsLength = 0;
            Floor0InnerDoorsCount = 0;
            Floor0InnerDoorsLength = 0;
            Floor0PartitionsDoorsCount = 0;
            Floor0PartitionsDoorsLength = 0;
            Floor0GatesCount = 0;
            Floor0GatesLength = 0;
            Floor0TerassesSquare = 0;
            Floor0InnerTerassesLength = 0;
            Floor0TerassesLength = 0;
            Floor0RailingsLength = 0;
            Floor0PlinthHeight = 0;
            PlinthOpenPerc = 0;
            Floor0BadroomCount = 0;
            Floor0BadroomSquare = 0;;
            Floor0TileSquare = 0;
            Floor0TilePerimeter = 0;
            Floor1OutWallsLength = 0;
            Floor1InnerWallsLength = 0;
            Floor1LightWallsLength = 0;
            Floor1BreakWallsLength = 0;
            Floor1Square = 0;
            Floor1OutDoorsCount = 0;
            Floor1OutDoorsLength = 0;
            Floor1InnerDoorsCount = 0;
            Floor1InnerDoorsLength = 0;
            Floor1PartitionsDoorsCount = 0;
            Floor1PartitionsDoorsLength = 0;
            Floor1GatesCount = 0;
            Floor1GatesLength = 0;
            Floor1TerassesSquare = 0;
            Floor1InnerTerassesLength = 0;
            Floor1TerassesLength = 0;
            Floor1RailingsLength = 0;
            WoodenPillarsLessCount = 0;
            WoodenPillarsOverCount = 0;
            ConcretePillarsLessCount = 0;
            ConcretePillarsOverCount = 0;
            Floor1BadroomCount = 0;
            Floor1BadroomSquare = 0;
            Floor1TileSquare = 0;
            Floor1TilePerimeter = 0;
            Floor2OutWallsLength = 0;
            Floor2InnerWallsLength = 0;
            Floor2LightWallsLength = 0;
            Floor2BreakWallsLength = 0;
            Floor2Square = 0;
            Floor2РHoleSecondLight = 0;
            Floor2OutDoorsCount = 0;
            Floor2OutDoorsLength = 0;
            Floor2InnerDoorsCount = 0;
            Floor2InnerDoorsLength = 0;
            Floor2PartitionsDoorsCount = 0;
            Floor2PartitionsDoorsLength = 0;
            Floor2BalconySquare = 0;
            Floor2BalconyLength = 0;
            Floor2RailingsLength = 0;
            Floor2BadroomCount = 0;
            Floor2BadroomSquare = 0;
            Floor2TileSquare = 0;
            Floor2TilePerimeter = 0;
            Floor3OutWallsLength = 0;
            Floor3InnerWallsLength = 0;
            Floor3LightWallsLength = 0;
            Floor3BreakWallsLength = 0;
            Floor3Square = 0;
            Floor3РHoleSecondLight = 0;
            Floor3OutDoorsCount = 0;
            Floor3OutDoorsLength = 0;
            Floor3InnerDoorsCount = 0;
            Floor3InnerDoorsLength = 0;
            Floor3PartitionsDoorsCount = 0;
            Floor3PartitionsDoorsLength = 0;
            Floor3BalconySquare = 0;
            Floor3BalconyLength = 0;
            Floor3RailingsLength = 0;
            Floor3BadroomCount = 0;
            Floor3BadroomSquare = 0;
            Floor3TileSquare = 0;
            Floor3TilePerimeter = 0;
            SiteHomeSquare = 0;
            RoofAreaSquare = 0;
            ConcreteRailingLength = 0;
        }
    }
}
