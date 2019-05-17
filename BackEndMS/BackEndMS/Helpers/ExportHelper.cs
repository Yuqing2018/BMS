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
        public static ExportHelper GetInstance()
        {
            return new ExportHelper();
        }
        public async void ExportToExcel(List<PageQueryEntry> datasource)
        {
            string[] filterFields = { "Id", "isFirst" };
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
        public async void GenerateTempOrderExcel<T>(List<T> datasource)
        {
            try
            {
                var fileName = "Export" + typeof(T).Name + DateTime.Now.ToString("yyyyMMddhhmmss");
                var dsfile = await GenerateExcel(fileName + ".xlsx");
                if (String.IsNullOrEmpty(dsfile))
                    return;
                var exportFields = typeof(T).GetProperties().Where(x => x.CustomAttributes.Select(t => t.AttributeType.Name).ToList().Contains("DisplayNameAttribute")).ToList();

                ExcelData ds = new ExcelData();
                if (typeof(T) == typeof(Order))
                {
                    var CelaMaxCount = (datasource as List<Order>).Select(x => x.CelaCounts).Max();
                    exportFields.ForEach(item =>
                  {
                      if (item.Name == "CelaExplains")
                      {
                          int i = 1;
                         
                          while (i <= CelaMaxCount)
                          {
                              ds.Headers.Add(String.Format("Cela建议{0} -指令详情",i));
                              ds.Headers.Add(String.Format("Cela建议{0} -处理方式",i));
                              i++;
                          }
                      }
                      else
                      {
                          var fieldName = item.CustomAttributes.Where(t => t.AttributeType.Name == "DisplayNameAttribute").Select(x => x.ConstructorArguments.FirstOrDefault().Value).FirstOrDefault();
                          ds.Headers.Add(fieldName.ToString() ?? item.Name);
                      }
                  });
                    ds.Values = (datasource as List<Order>).Select(x => GetValusFromSource(x, CelaMaxCount)).ToList();
                }
                else
                {
                    exportFields.ForEach(item =>
                    {
                        var fieldName = item.CustomAttributes.Where(t => t.AttributeType.Name == "DisplayNameAttribute").Select(x => x.ConstructorArguments.FirstOrDefault().Value).FirstOrDefault();
                        ds.Headers.Add(fieldName.ToString() ?? item.Name);
                    });
                    ds.Values = datasource.Select(x => GetValusFromSource(x)).ToList();
                }
                InsertDataIntoSheet(dsfile, "sheet1", ds);
                var dialog = new ContentDialog()
                {
                    Title = "Tips!",
                    Content = new TextBlock()
                    {
                        Text = "download successfully！" + Environment.NewLine + "loaclPath:" + dsfile,
                        TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
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
        public List<string> GetValusFromSource<T>(T data,int CelaMaxCount=0)
        {
            List<string> result = new List<String>();
            Type t = typeof(T);
            var exportFields = t.GetProperties().Where(x => x.CustomAttributes.Select(p => p.AttributeType.Name).ToList().Contains("DisplayNameAttribute")).ToList();
            foreach (var p in exportFields)
            {
                Object value = p.GetValue(data, null);
                if (value == null)
                {
                    result.Add("");
                    continue;
                }
                if (value.GetType() == typeof(long) && p.Name == "ModifyDate")
                {
                    result.Add(LinqHelper.GetDateTime(value as long?).ToString("yyyy-MM-dd HH:mm:ss"));
                    continue;
                }
                if (typeof(T) == typeof(Order))
                {
                    string[] uploadFields = { "FileContents", "FileDetails", "BaiduActions" };
                    if (p.Name == "CelaExplains")
                    {
                        int i = 1;
                        (value as System.Collections.ObjectModel.ObservableCollection<CelaExplain>).ToList().ForEach(
                            item =>
                        {
                            result.Add(item.Item);
                            if (null == item.Strategy)
                                result.Add("");
                            else
                                result.Add(String.Join("\n", item.Strategy));
                            i++;
                        });
                        while(i<= CelaMaxCount)
                        {
                            result.Add("");
                            result.Add("");
                            i++;
                        }
                        continue;
                    }
                    else if (value.GetType() == typeof(List<string>))
                    {
                         value = String.Join("\n", (value as List<string>));
                    }
                    else if (uploadFields.Contains(p.Name))
                    {
                        var list = (value as System.Collections.ObjectModel.ObservableCollection<UploadImage>).ToList()
                            .Select(x => String.Format("附件：{0}.{1}", x.Id, x.FileExtension)).ToList();
                        if (null != list && list.Count > 0)
                        {
                            value = String.Join("\n", list);
                        }else
                        {
                            value = "";
                        }
                    }
                }
                else if (typeof(T) == typeof(PageQueryEntry))
                {
                    if (p.Name == "IsExpire")
                    {
                        var entity = new IsExpireToStingConverter();
                        value = entity.Convert(value, null, null, "");
                    }
                    else if (p.Name == "ValidDays")
                        value = Enum.GetName(typeof(ValidDays), value);
                    else if (value.GetType() == typeof(string[]))
                        value = String.Join("\n", (value as string[]));
                }
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



        private async void GenerateTempOrderExcel(List<Order> recordList)
        {
            #region Save the Workbook
            StorageFile storageFile;
                if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons")))
                {
                    FileSavePicker savePicker = new FileSavePicker();
                    savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
                    savePicker.SuggestedFileName = "Sample";
                    savePicker.FileTypeChoices.Add("Excel Files", new List<string>() { ".xlsx" });
                    storageFile = await savePicker.PickSaveFileAsync();
                }
                else
                {
                    StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
                    storageFile = await local.CreateFileAsync("Sample.xlsx", CreationCollisionOption.ReplaceExisting);
                }

                if (storageFile != null)
                {
                    SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(storageFile.Path, SpreadsheetDocumentType.Workbook, true);
                    WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };
                    sheets.Append(sheet);

                    workbookPart.Workbook.Save();
                    spreadsheetDocument.Close();
            }
            #endregion
        }

    }
    public class ExcelData
    {
        public List<string> Headers { get; set; } = new List<string>();
        public List<List<string>> Values { get; set; } = new List<List<string>>();
    }
}
