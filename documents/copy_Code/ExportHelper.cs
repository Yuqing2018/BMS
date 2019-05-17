using BackEndMS.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace BackEndMS.Helpers
{
    public class ExportHelper
    {
        private string[] filterFields = { "Id", "isFirst" };
        public static ExportHelper GetInstance()
        {
            return new ExportHelper();
        }
        public async void ExportToExcel(List<PageQueryEntry> datasource)
        {
            try
            {
                var fileName = "ExportPageQuery" + DateTime.Now.ToString("yyyyMMddhhmmss");
                var dsfile = await GenerateExcel(fileName+".xlsx");
                if (String.IsNullOrEmpty(dsfile))
                    return;
                ExcelData ds = new ExcelData()
                {
                    Headers = typeof(PageQueryEntry).GetProperties().Where(x => !filterFields.Contains(x.Name)).Select(x => x.Name).ToList(),
                    Values = datasource.Select(x => GetValusFromSource(x)).ToList()
                };
                InsertDataIntoSheet(dsfile, "sheet1", ds);
                var dialog = new ContentDialog()
                {
                    Title = "Tips!",
                    Content = new TextBlock()
                    {
                        Text= "download successfully！" + Environment.NewLine + "loaclPath:" + dsfile,
                        TextWrapping=Windows.UI.Xaml.TextWrapping.Wrap,
                        IsTextSelectionEnabled = true,
                    },
                    PrimaryButtonText = "OpenFile",
                    PrimaryButtonCommandParameter = dsfile,
                    SecondaryButtonText = "Cancel",
                    FullSizeDesired = false,
                };
                dialog.PrimaryButtonClick += Dialog_PrimaryButtonClick;
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
                return;
            }

        }

        public List<string> GetValusFromSource<T>(T data)
        {
            List<string> result = new List<String>();
            Type t = typeof(T);
            foreach (var p in t.GetProperties())
            {
                Object value = p.GetValue(data, null);
                if (value == null)
                {
                    result.Add("");
                    continue;
                }
                if (value.GetType() == typeof(long) && p.Name == "ModifyDate")
                    value = LinqHelper.GetDateTime(value as long?).ToString("yyyy-MM-dd HH:mm:ss");
                else if (value.GetType() == typeof(string[]))
                    value = String.Join("\n", (value as string[]));
                if (filterFields.Contains(p.Name))
                    continue;
                if (p.Name == "IsExpire")
                {
                    var entity = new IsExpireToStingConverter();
                    value = entity.Convert(value, null, null, "");
                }
                else if (p.Name == "ValidDays")
                    value = Enum.GetName(typeof(ValidDays), value);
                result.Add(value.ToString());
            }
            return result;
        }
        public async Task<string> GenerateExcel(string fileName)
        {
            try
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                string fullPath = Path.Combine(folder.Path, fileName);
                return await Task<String>.Run(() =>
                {
                    Task.Yield();
                    SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(fullPath, SpreadsheetDocumentType.Workbook);
                    WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };
                    sheets.Append(sheet);

                    workbookPart.Workbook.Save();
                    spreadsheetDocument.Close();
                    return fullPath;
                });
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
                return null;
            }
        }
        public async Task<string> GenerateExcel1(string fileName)
        {
            try
            {
                #region 选择保存文件位置、类型
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Excel workbook(*.xlsx,*.xls)", new List<string>() { ".xlsx", ".xls" });
                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = fileName;
                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                    Windows.Storage.CachedFileManager.DeferUpdates(file);
                    var folder = await file.GetParentAsync();
                    // if (folder == null)
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickSaveFolderToken", folder);
                    #region 对应文件夹下新建一个excel文件
                    return await Task<String>.Run(() =>
                    {
                        Task.Yield();
                        SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(file.Path, SpreadsheetDocumentType.Workbook, true);
                        WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
                        workbookPart.Workbook = new Workbook();

                        WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                        worksheetPart.Worksheet = new Worksheet(new SheetData());

                        Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                        Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };
                        sheets.Append(sheet);

                        workbookPart.Workbook.Save();
                        spreadsheetDocument.Close();
                        return file.Path;
                    });
                    #endregion
                }
                else
                    return "";
                #endregion
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
                return null;
            }
        }
        public async void InsertDataIntoSheet(string fileName, string sheetName, ExcelData data)
        {
            try
            {
                //Fix for https://github.com/OfficeDev/Open-XML-SDK/issues/221
                //  Environment.SetEnvironmentVariable("MONO_URI_DOTNETRELATIVEORABSOLUTE", "true");
                await Task.Run(() =>
                {
                    // Open the document for editing
                    using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, true))
                    {
                        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                        SetSheetStyle(ref workbookPart);
                        //Set the sheet name on the first sheet
                        Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
                        Sheet sheet = sheets.Elements<Sheet>().FirstOrDefault();

                        sheet.Name = sheetName;

                        WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();

                        SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

                        Row headerRow = ConstructHeader(data.Headers.ToArray());

                        sheetData.AppendChild(headerRow);//增加标题行
                        foreach (List<string> dataList in data.Values)
                        {
                            Row dataRow = sheetData.AppendChild(new Row());
                            foreach (string dataElement in dataList)
                            {
                                Cell cell = ConstructCell(dataElement, CellValues.String);
                                dataRow.Append(cell);
                            }
                        }

                        workbookPart.Workbook.Save();
                    }
                });
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
                return ;
            }
        }
        private void SetSheetStyle(ref WorkbookPart workbook)
        {
            var stylesPart = workbook.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = new Stylesheet();

            //Console.WriteLine("Creating styles");

            // blank font list
            stylesPart.Stylesheet.Fonts = new Fonts();
            stylesPart.Stylesheet.Fonts.Count = 1;
            stylesPart.Stylesheet.Fonts.AppendChild(new Font());

            // create fills
            stylesPart.Stylesheet.Fills = new Fills();

            // create a solid red fill
            var solidRed = new PatternFill() { PatternType = PatternValues.Solid };
            solidRed.ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("FFFF0000") }; // red fill
            solidRed.BackgroundColor = new BackgroundColor { Indexed = 64 };

            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } }); // required, reserved by Excel
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } }); // required, reserved by Excel
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = solidRed });
            stylesPart.Stylesheet.Fills.Count = 3;

            // blank border list
            stylesPart.Stylesheet.Borders = new Borders();
            stylesPart.Stylesheet.Borders.Count = 1;
            stylesPart.Stylesheet.Borders.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Border());

            // blank cell format list
            stylesPart.Stylesheet.CellStyleFormats = new CellStyleFormats();
            stylesPart.Stylesheet.CellStyleFormats.Count = 1;
            stylesPart.Stylesheet.CellStyleFormats.AppendChild(new CellFormat());

            // cell format list
            stylesPart.Stylesheet.CellFormats = new CellFormats();
            // empty one for index 0, seems to be required
            stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat());
            // cell format references style format 0, font 0, border 0, fill 2 and applies the fill
            stylesPart.Stylesheet.CellFormats.AppendChild(
                new CellFormat {
                    FormatId = 0,
                    FontId = 0,
                    BorderId = 0,
                    FillId = 2,
                    ApplyFill = true,
                }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Center });
            stylesPart.Stylesheet.CellFormats.Count = 2;

            stylesPart.Stylesheet.Save();

        }
        private async void Dialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string fileName = sender.PrimaryButtonCommandParameter.ToString();
            var file = await StorageFile.GetFileFromPathAsync(fileName);
            //ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
            //await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(fileName);

            if (file != null)
            {
                // Launch the retrieved file
                var success = await Windows.System.Launcher.LaunchFileAsync(file);
                if (success)
                    return;
            }
        }

        private Row ConstructHeader(params string[] headers)
        {
            Row row = new Row();
            foreach (var header in headers)
            {
                row.AppendChild(ConstructCell(header, CellValues.String));
            }
            return row;
        }

        private Cell ConstructCell(string value, CellValues dataType)
        {
            return new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataType),
            };
        }
        private Cell ConstructCell(string value, string cellReference, CellValues dataType)
        {
            return new Cell()
            {
                CellReference = cellReference,
                CellValue = new CellValue(value),
                DataType = dataType,
            };
        }

    }
    public class ExcelData
    {
        public List<string> Headers { get; set; } = new List<string>();
        public List<List<string>> Values { get; set; } = new List<List<string>>();
    }
}
