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
        static readonly string tableID = "10VcNTGBZTrUn3Qcr_2kZz40hMoEaKYcrZPEqwcqUaAE";
        static readonly string listName = "Параметры проектов";
        static bool isFinded = false;

        public static void SaveData()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
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
                Action<int> WriteProjectData = (rowNumber) =>
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
                        House.Floor1DecatativePillarsLessCount,
                        House.Floor1DecatativePillarsOverCount,
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
                        House.Floor3TilePerimeter
                    };
                    valueRange.Values = new List<IList<object>> { oblist };

                    SpreadsheetsResource.ValuesResource.UpdateRequest update = service.Spreadsheets.Values.Update(valueRange, tableID, range2);
                    update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                    UpdateValuesResponse result2 = update.Execute();
                };

                foreach (var row in values)
                {
                    lineProjectInTable++;
                    if (House.ProjectName.ToLower() == row[0].ToString().ToLower())
                    {
                        isFinded = true;
                        WriteProjectData(1);
                        //WriteTable(House.ManagerName, "B");

                        MessageBox.Show("Данные проекта успешно сохранены", "Добавление данных в таблицу", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    }
                }




            }
            if (!isFinded)
                MessageBox.Show("Данные о проекте не найдены." + Environment.NewLine + "Проверьте правильность названия проекта или его наличие в таблице", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public static void LoadData()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
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
                    if (row[0].ToString().ToLower().Contains(House.ProjectName.ToLower())) 
                    {
                        isFinded = true;
                        // Define request parameters.
                        string findedRange = $"A{lineProjectInTable}:DJ{lineProjectInTable}";
                        SpreadsheetsResource.ValuesResource.GetRequest request2 =
                                service.Spreadsheets.Values.Get(tableID, findedRange);

                        ValueRange response2 = request2.Execute();
                        IList<IList<object>> findedValues = response2.Values;

                        var rowValues = findedValues[0];

                        // UNDONE : Присвоение данных с google классу House
                           
                        

                        MessageBox.Show("Данные проекта успешно загружены", "Загрузка данных", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    }
                }
            }
            if (!isFinded)
                MessageBox.Show("Данные о проекте не найдены." + Environment.NewLine + "Проверьте правильность названия проекта или его наличие в таблице", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Warning);

        }
    }
}
