using BuildingPlanCalc.Interfaces;
using BuildingPlanCalc.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BuildingPlanCalc.Services
{
    public static class GoogleSheets
    {
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "BuildingPlanCalc";

        static bool isFinded = false;

        public static void SaveData(string tableID = "10VcNTGBZTrUn3Qcr_2kZz40hMoEaKYcrZPEqwcqUaAE", string listName = "Параметры проектов")
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            // Define request parameters.
            string range = $"A4:A1000";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(tableID, range);

            ValueRange response = request.Execute();
            IList<IList<object>> values = response.Values;

            //
            int lineProjectInTable = 3;
            if (values != null && values.Count > 0)
            {
                void WriteProjectData()
                {
                    Thread.Sleep(300);
                    // Стартовая ячейка
                    string range2 = $"{listName}!B{lineProjectInTable}";
                    // Как записывать массив? (в строку или колонку)
                    ValueRange valueRange = new ValueRange
                    {
                        MajorDimension = "ROWS"
                    };
                    // Записываемый массив
                    var oblist = new List<object>()
                    {
                        House.ManagerName,
                        House.FloorsCount,
                        House.RoomCount,
                        House.BedroomCount,
                        House.KitchensSquare,
                        House.Floor0Height,
                        House.Floor1Height,
                        House.Floor2Height,
                        House.Floor3Height,
                        House.RoofHeight,
                        House.RoofMinWallHeight,
                        House.RoofType,
                        House.RoofSquare,
                        House.RoofLength,
                        House.CanopySquare,
                        House.CanopyLength,
                        House.PergolaSquare,
                        House.HemmingButt,
                        House.HemmingOverhangsSquare,
                        House.Floor0F1GlaseSq + House.Floor0F2GlaseSq + House.Floor0F3GlaseSq + House.Floor0F4GlaseSq,
                        House.Floor0F1GlaseP + House.Floor0F2GlaseP + House.Floor0F3GlaseP + House.Floor0F4GlaseP,
                        House.Floor1F1GlaseSq + House.Floor1F2GlaseSq + House.Floor1F3GlaseSq + House.Floor1F4GlaseSq,
                        House.Floor1F1GlaseP + House.Floor1F2GlaseP + House.Floor1F3GlaseP + House.Floor1F4GlaseP,
                        House.Floor2F1GlaseSq + House.Floor2F2GlaseSq + House.Floor2F3GlaseSq + House.Floor2F4GlaseSq,
                        House.Floor2F1GlaseP + House.Floor2F2GlaseP + House.Floor2F3GlaseP + House.Floor2F4GlaseP,
                        House.Floor3F1GlaseSq + House.Floor3F2GlaseSq + House.Floor3F3GlaseSq + House.Floor3F4GlaseSq,
                        House.Floor3F1GlaseP + House.Floor3F2GlaseP + House.Floor3F3GlaseP + House.Floor3F4GlaseP,
                        House.WindowCount,
                        House.WindowSquare,
                        House.Floor0OutWallsLength,
                        House.Floor0InnerWallsLength,
                        House.Floor0LightWallsLength,
                        House.Floor0BreakWallsLength,
                        House.Floor0Square,
                        House.Floor0OutDoorsCount,
                        House.Floor0OutDoorsLength,
                        House.Floor0InnerDoorsCount,
                        House.Floor0InnerDoorsLength,
                        House.Floor0PartitionsDoorsCount,
                        House.Floor0PartitionsDoorsLength,
                        House.Floor0GatesCount,
                        House.Floor0GatesLength,
                        House.Floor0TerassesSquare,
                        House.Floor0InnerTerassesLength,
                        House.Floor0TerassesLength,
                        House.Floor0RailingsLength,
                        House.Floor0PlinthHeight,
                        House.PlinthOpenPerc,
                        House.Floor0BadroomCount,
                        House.Floor0BadroomSquare,
                        House.Floor0TileSquare,
                        House.Floor0TilePerimeter,
                        House.Floor1OutWallsLength,
                        House.Floor1InnerWallsLength,
                        House.Floor1LightWallsLength,
                        House.Floor1BreakWallsLength,
                        House.Floor1Square,
                        House.Floor1OutDoorsCount,
                        House.Floor1OutDoorsLength,
                        House.Floor1InnerDoorsCount,
                        House.Floor1InnerDoorsLength,
                        House.Floor1PartitionsDoorsCount,
                        House.Floor1PartitionsDoorsLength,
                        House.Floor1GatesCount,
                        House.Floor1GatesLength,
                        House.Floor1TerassesSquare,
                        House.Floor1InnerTerassesLength,
                        House.Floor1TerassesLength,
                        House.Floor1RailingsLength,
                        House.WoodenPillarsLessCount,
                        House.WoodenPillarsOverCount,
                        House.Floor1BadroomCount,
                        House.Floor1BadroomSquare,
                        House.Floor1TileSquare,
                        House.Floor1TilePerimeter,
                        House.Floor2OutWallsLength,
                        House.Floor2InnerWallsLength,
                        House.Floor2LightWallsLength,
                        House.Floor2BreakWallsLength,
                        House.Floor2Square,
                        House.Floor2РHoleSecondLight,
                        House.Floor2OutDoorsCount,
                        House.Floor2OutDoorsLength,
                        House.Floor2InnerDoorsCount,
                        House.Floor2InnerDoorsLength,
                        House.Floor2PartitionsDoorsCount,
                        House.Floor2PartitionsDoorsLength,
                        House.Floor2BalconySquare,
                        House.Floor2BalconyLength,
                        House.Floor2RailingsLength,
                        House.Floor2BadroomCount,
                        House.Floor2BadroomSquare,
                        House.Floor2TileSquare,
                        House.Floor2TilePerimeter,
                        House.Floor3OutWallsLength,
                        House.Floor3InnerWallsLength,
                        House.Floor3LightWallsLength,
                        House.Floor3BreakWallsLength,
                        House.Floor3Square,
                        House.Floor3РHoleSecondLight,
                        House.Floor3OutDoorsCount,
                        House.Floor3OutDoorsLength,
                        House.Floor3InnerDoorsCount,
                        House.Floor3InnerDoorsLength,
                        House.Floor3PartitionsDoorsCount,
                        House.Floor3PartitionsDoorsLength,
                        House.Floor3BalconySquare,
                        House.Floor3BalconyLength,
                        House.Floor3RailingsLength,
                        House.Floor3BadroomCount,
                        House.Floor3BadroomSquare,
                        House.Floor3TileSquare,
                        House.Floor3TilePerimeter,
                        House.ConcretePillarsLessCount,
                        House.ConcretePillarsOverCount,
                        DateTime.Now.ToString("d"),
                        House.ConcreteRailingLength,
                        House.RoofAreaSquare,
                        House.SiteHomeSquare,
                        House.Price,
                        House.FoundationPlateThickness,
                        House.TapeHeight,
                        House.TapeWidthUnderTheHouse,
                        House.NumberOfRebars,
                        House.SectionOfTheMainReinforcement,
                        House.RCFloorThickness,
                        House.FloorBeamThickness,
                        House.FloorBeamHeight,
                        House.InsulationThickness,
                        House.BearingWWllThickness,
                        House.BasementWallThickness,
                        House.RafterHeight,
                        House.RafterThickness,
                        House.InsulationThickness2
                    };

                    valueRange.Values = new List<IList<object>> { oblist };

                    SpreadsheetsResource.ValuesResource.UpdateRequest update = service.Spreadsheets.Values.Update(valueRange, tableID, range2);
                    update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                    update.Execute();
                }

                foreach (var row in values)
                {
                    lineProjectInTable++;
                    if (row.Count > 0)
                        if (House.ProjectName.ToLower() == row[0].ToString().ToLower())
                        {
                            isFinded = true;
                            WriteProjectData();
                            //WriteTable(House.ManagerName, "B");

                            MessageBox.Show("Данные проекта успешно сохранены", "Добавление данных в таблицу", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        }
                }




            }
            if (!isFinded)
                MessageBox.Show("Данные о проекте не найдены." + Environment.NewLine + "Проверьте правильность названия проекта или его наличие в таблице", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public static void LoadData(string tableID = "10VcNTGBZTrUn3Qcr_2kZz40hMoEaKYcrZPEqwcqUaAE")
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            // Define request parameters.
            string range = $"A4:A1000";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(tableID, range);

            ValueRange response = request.Execute();
            IList<IList<object>> values = response.Values;

            //
            int lineProjectInTable = 3;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    lineProjectInTable++;
                    if (row.Count > 0 && row[0].ToString().ToLower() == House.ProjectName.ToLower())
                    {
                        isFinded = true;
                        // Define request parameters.
                        string findedRange = $"A{lineProjectInTable}:EE{lineProjectInTable}";
                        SpreadsheetsResource.ValuesResource.GetRequest request2 =
                                service.Spreadsheets.Values.Get(tableID, findedRange);

                        ValueRange response2 = request2.Execute();
                        IList<IList<object>> findedValues = response2.Values;

                        var rowValues = findedValues[0];

                        // UNDONE : Присвоение данных с google классу House
                        try
                        {
                            House.ManagerName = rowValues[1].ToString();
                            House.FloorsCount = ExtractInt(rowValues[2].ToString());
                            House.RoomCount = ExtractInt(rowValues[3].ToString());
                            House.BedroomCount = ExtractInt(rowValues[4].ToString());
                            House.KitchensSquare = ExtractFloat(rowValues[5].ToString());
                            House.Floor0Height = ExtractFloat(rowValues[6].ToString());
                            House.Floor1Height = ExtractFloat(rowValues[7].ToString());
                            House.Floor2Height = ExtractFloat(rowValues[8].ToString());
                            House.Floor3Height = ExtractFloat(rowValues[9].ToString());
                            House.RoofHeight = ExtractFloat(rowValues[10].ToString());
                            House.RoofMinWallHeight = ExtractFloat(rowValues[11].ToString());
                            House.RoofType = rowValues[12].ToString();
                            House.RoofSquare = ExtractFloat(rowValues[13].ToString());
                            House.RoofLength = ExtractFloat(rowValues[14].ToString());
                            House.CanopySquare = ExtractFloat(rowValues[15].ToString());
                            House.CanopyLength = ExtractFloat(rowValues[16].ToString());
                            House.PergolaSquare = ExtractFloat(rowValues[17].ToString());
                            House.HemmingButt = ExtractFloat(rowValues[18].ToString());
                            House.HemmingOverhangsSquare = ExtractFloat(rowValues[19].ToString());
                            House.Floor0F1GlaseSq = ExtractFloat(rowValues[20].ToString());
                            House.Floor0F1GlaseP = ExtractFloat(rowValues[21].ToString());
                            House.Floor1F1GlaseSq = ExtractFloat(rowValues[22].ToString());
                            House.Floor1F1GlaseP = ExtractFloat(rowValues[23].ToString());
                            House.Floor2F1GlaseSq = ExtractFloat(rowValues[24].ToString());
                            House.Floor2F1GlaseP = ExtractFloat(rowValues[25].ToString());
                            House.Floor3F1GlaseSq = ExtractFloat(rowValues[26].ToString());
                            House.Floor3F1GlaseP = ExtractFloat(rowValues[27].ToString());
                            House.WindowCount = ExtractInt(rowValues[28].ToString());
                            House.WindowSquare = ExtractFloat(rowValues[29].ToString());
                            House.Floor0OutWallsLength = ExtractFloat(rowValues[30].ToString());
                            House.Floor0InnerWallsLength = ExtractFloat(rowValues[31].ToString());
                            House.Floor0LightWallsLength = ExtractFloat(rowValues[32].ToString());
                            House.Floor0BreakWallsLength = ExtractFloat(rowValues[33].ToString());
                            House.Floor0Square = ExtractFloat(rowValues[34].ToString());
                            House.Floor0OutDoorsCount = ExtractInt(rowValues[35].ToString());
                            House.Floor0OutDoorsLength = ExtractFloat(rowValues[36].ToString());
                            House.Floor0InnerDoorsCount = ExtractInt(rowValues[37].ToString());
                            House.Floor0InnerDoorsLength = ExtractFloat(rowValues[38].ToString());
                            House.Floor0PartitionsDoorsCount = ExtractInt(rowValues[39].ToString());
                            House.Floor0PartitionsDoorsLength = ExtractFloat(rowValues[40].ToString());
                            House.Floor0GatesCount = ExtractInt(rowValues[41].ToString());
                            House.Floor0GatesLength = ExtractFloat(rowValues[42].ToString());
                            House.Floor0TerassesSquare = ExtractFloat(rowValues[43].ToString());
                            House.Floor0InnerTerassesLength = ExtractFloat(rowValues[44].ToString());
                            House.Floor0TerassesLength = ExtractFloat(rowValues[45].ToString());
                            House.Floor0RailingsLength = ExtractFloat(rowValues[46].ToString());
                            House.Floor0PlinthHeight = ExtractFloat(rowValues[47].ToString());
                            House.PlinthOpenPerc = ExtractInt(rowValues[48].ToString());
                            House.Floor0BadroomCount = ExtractInt(rowValues[49].ToString());
                            House.Floor0BadroomSquare = ExtractFloat(rowValues[50].ToString());
                            House.Floor0TileSquare = ExtractFloat(rowValues[51].ToString());
                            House.Floor0TilePerimeter = ExtractFloat(rowValues[52].ToString());
                            House.Floor1OutWallsLength = ExtractFloat(rowValues[53].ToString());
                            House.Floor1InnerWallsLength = ExtractFloat(rowValues[54].ToString());
                            House.Floor1LightWallsLength = ExtractFloat(rowValues[55].ToString());
                            House.Floor1BreakWallsLength = ExtractFloat(rowValues[56].ToString());
                            House.Floor1Square = ExtractFloat(rowValues[57].ToString());
                            House.Floor1OutDoorsCount = ExtractInt(rowValues[58].ToString());
                            House.Floor1OutDoorsLength = ExtractFloat(rowValues[59].ToString());
                            House.Floor1InnerDoorsCount = ExtractInt(rowValues[60].ToString());
                            House.Floor1InnerDoorsLength = ExtractFloat(rowValues[61].ToString());
                            House.Floor1PartitionsDoorsCount = ExtractInt(rowValues[62].ToString());
                            House.Floor1PartitionsDoorsLength = ExtractFloat(rowValues[63].ToString());
                            House.Floor1GatesCount = ExtractInt(rowValues[64].ToString());
                            House.Floor1GatesLength = ExtractFloat(rowValues[65].ToString());
                            House.Floor1TerassesSquare = ExtractFloat(rowValues[66].ToString());
                            House.Floor1InnerTerassesLength = ExtractFloat(rowValues[67].ToString());
                            House.Floor1TerassesLength = ExtractFloat(rowValues[68].ToString());
                            House.Floor1RailingsLength = ExtractFloat(rowValues[69].ToString());
                            House.WoodenPillarsLessCount = ExtractInt(rowValues[70].ToString());
                            House.WoodenPillarsOverCount = ExtractInt(rowValues[71].ToString());
                            House.Floor1BadroomCount = ExtractInt(rowValues[72].ToString());
                            House.Floor1BadroomSquare = ExtractFloat(rowValues[73].ToString());
                            House.Floor1TileSquare = ExtractFloat(rowValues[74].ToString());
                            House.Floor1TilePerimeter = ExtractFloat(rowValues[75].ToString());
                            House.Floor2OutWallsLength = ExtractFloat(rowValues[76].ToString());
                            House.Floor2InnerWallsLength = ExtractFloat(rowValues[77].ToString());
                            House.Floor2LightWallsLength = ExtractFloat(rowValues[78].ToString());
                            House.Floor2BreakWallsLength = ExtractFloat(rowValues[79].ToString());
                            House.Floor2Square = ExtractFloat(rowValues[80].ToString());
                            House.Floor2РHoleSecondLight = ExtractFloat(rowValues[81].ToString());
                            House.Floor2OutDoorsCount = ExtractInt(rowValues[82].ToString());
                            House.Floor2OutDoorsLength = ExtractFloat(rowValues[83].ToString());
                            House.Floor2InnerDoorsCount = ExtractInt(rowValues[84].ToString());
                            House.Floor2InnerDoorsLength = ExtractFloat(rowValues[85].ToString());
                            House.Floor2PartitionsDoorsCount = ExtractInt(rowValues[86].ToString());
                            House.Floor2PartitionsDoorsLength = ExtractFloat(rowValues[87].ToString());
                            House.Floor2BalconySquare = ExtractFloat(rowValues[88].ToString());
                            House.Floor2BalconyLength = ExtractFloat(rowValues[89].ToString());
                            House.Floor2RailingsLength = ExtractFloat(rowValues[90].ToString());
                            House.Floor2BadroomCount = ExtractInt(rowValues[91].ToString());
                            House.Floor2BadroomSquare = ExtractFloat(rowValues[92].ToString());
                            House.Floor2TileSquare = ExtractFloat(rowValues[93].ToString());
                            House.Floor2TilePerimeter = ExtractFloat(rowValues[94].ToString());
                            House.Floor3OutWallsLength = ExtractFloat(rowValues[95].ToString());
                            House.Floor3InnerWallsLength = ExtractFloat(rowValues[96].ToString());
                            House.Floor3LightWallsLength = ExtractFloat(rowValues[97].ToString());
                            House.Floor3BreakWallsLength = ExtractFloat(rowValues[98].ToString());
                            House.Floor3Square = ExtractFloat(rowValues[99].ToString());
                            House.Floor3РHoleSecondLight = ExtractFloat(rowValues[100].ToString());
                            House.Floor3OutDoorsCount = ExtractInt(rowValues[101].ToString());
                            House.Floor3OutDoorsLength = ExtractFloat(rowValues[102].ToString());
                            House.Floor3InnerDoorsCount = ExtractInt(rowValues[103].ToString());
                            House.Floor3InnerDoorsLength = ExtractFloat(rowValues[104].ToString());
                            House.Floor3PartitionsDoorsCount = ExtractInt(rowValues[105].ToString());
                            House.Floor3PartitionsDoorsLength = ExtractFloat(rowValues[106].ToString());
                            House.Floor3BalconySquare = ExtractFloat(rowValues[107].ToString());
                            House.Floor3BalconyLength = ExtractFloat(rowValues[108].ToString());
                            House.Floor3RailingsLength = ExtractFloat(rowValues[109].ToString());
                            House.Floor3BadroomCount = ExtractInt(rowValues[110].ToString());
                            House.Floor3BadroomSquare = ExtractFloat(rowValues[111].ToString());
                            House.Floor3TileSquare = ExtractFloat(rowValues[112].ToString());
                            House.Floor3TilePerimeter = ExtractFloat(rowValues[113].ToString());
                            House.ConcretePillarsLessCount = ExtractInt(rowValues[114].ToString());
                            House.ConcretePillarsOverCount = ExtractInt(rowValues[115].ToString());

                            House.ConcreteRailingLength = ExtractFloat(rowValues[117].ToString());
                            House.RoofAreaSquare = ExtractFloat(rowValues[118].ToString());
                            House.SiteHomeSquare = ExtractFloat(rowValues[119].ToString());
                            House.Price = ExtractFloat(rowValues[120].ToString());

                            House.FoundationPlateThickness = ExtractFloat(rowValues[121].ToString());
                            House.TapeHeight = ExtractFloat(rowValues[122].ToString());
                            House.TapeWidthUnderTheHouse = ExtractFloat(rowValues[123].ToString());
                            House.NumberOfRebars = ExtractInt(rowValues[124].ToString());
                            House.SectionOfTheMainReinforcement = ExtractFloat(rowValues[125].ToString());
                            House.RCFloorThickness = ExtractFloat(rowValues[126].ToString());
                            House.FloorBeamThickness = ExtractFloat(rowValues[127].ToString());
                            House.FloorBeamHeight = ExtractFloat(rowValues[128].ToString());
                            House.InsulationThickness = ExtractFloat(rowValues[129].ToString());
                            House.BearingWWllThickness = ExtractFloat(rowValues[130].ToString());
                            House.BasementWallThickness = ExtractFloat(rowValues[131].ToString());
                            House.RafterHeight = ExtractFloat(rowValues[132].ToString());
                            House.RafterThickness = ExtractFloat(rowValues[133].ToString());
                            House.InsulationThickness2 = ExtractFloat(rowValues[134].ToString());
                        }
                        catch
                        {

                        }

                        MessageBox.Show("Данные проекта успешно загружены", "Загрузка данных", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    }
                }
            }
            if (!isFinded)
                MessageBox.Show("Данные о проекте не найдены." + Environment.NewLine + "Проверьте правильность названия проекта или его наличие в таблице", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Warning);

        }

        private static int ExtractInt(string obj)
        {
            if (string.IsNullOrEmpty(obj))
                return 0;

            if (obj.Contains(','))
            {
                obj = obj.Substring(0, obj.IndexOf(','));
            }

            var result = int.TryParse(obj, out int a);
            if (result) return a;
            else
                return 0;
        }
        private static double ExtractFloat(string obj)
        {
            if (string.IsNullOrEmpty(obj))
                return 0.0f;

            var result = float.TryParse(obj, out float a);
            if (result)
            {

                return Math.Round(a, 2);
            }
            else
                return 0.0f;
        }


    }
}
