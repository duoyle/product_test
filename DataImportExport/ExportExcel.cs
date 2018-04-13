using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace DataImportExport
{
    public class ExportExcel
    {
        /// <summary>
        /// 把DataTable数据填充至Excel
        /// </summary>
        private static MemoryStream CreateExcel(DataTable dt)
        {
            MemoryStream ms = new MemoryStream();

            IWorkbook iwbExcel = new HSSFWorkbook();
            ISheet iSheet = iwbExcel.CreateSheet(dt.TableName);

            try
            {
                if (dt == null)
                    return null;

                #region 设置数据的列标题
                IRow header = iSheet.CreateRow(0);
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    ICell cell = header.CreateCell(i);
                    string colCaption = dt.Columns[i].Caption ?? dt.Columns[i].ColumnName;
                    cell.SetCellValue(colCaption);
                }
                #endregion

                #region 填充数据行
                int rowIndex = 1;
                foreach (DataRow dRow in dt.Rows)
                {
                    IRow iRow = iSheet.CreateRow(rowIndex);
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        iRow.CreateCell(i).SetCellValue(dRow[i].ToString());
                    }
                    rowIndex++;
                }
                #endregion

                #region 设置表格的格式
                iSheet.SetColumnWidth(3, 5000);
                //iSheet.SetColumnWidth(4, 4000);
                //iSheet.SetColumnWidth(9, 4000);
                //iSheet.SetColumnWidth(13, 4000);
                //iSheet.GetRow(0).Height = 400;
                //iSheet.GetRow(1).Height = 300;
                //iSheet.GetRow(2).Height = 300;
                //设置第一行单元格样式和字体
                ICellStyle icStyle = iwbExcel.CreateCellStyle();
                icStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.CENTER;
                IFont iFont = iwbExcel.CreateFont();
                //iFont.Boldweight = short.MaxValue;
                iFont.FontHeightInPoints = (short)16;
                icStyle.SetFont(iFont);
                iSheet.GetRow(0).RowStyle = icStyle;

                #endregion

                iwbExcel.Write(ms);
                ms.Flush();
                ms.Position = 0;
                return ms;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 创建对应工作薄的工作表
        /// </summary>
        private static void createSheet(IWorkbook iwbExcel, DataTable dt, string sheetName)
        {
            if (iwbExcel == null || dt == null || string.IsNullOrEmpty(sheetName)) return;
            try
            {
                #region 设置数据的列标题
                ISheet iSheet = iwbExcel.CreateSheet(sheetName);//创建工作表实例
                IRow header = iSheet.CreateRow(0);//创建数据行实例
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    ICell cell = header.CreateCell(i);
                    string colCaption = dt.Columns[i].Caption ?? dt.Columns[i].ColumnName;
                    cell.SetCellValue(colCaption);
                }
                #endregion

                #region 填充数据行
                int rowIndex = 1;
                foreach (DataRow dRow in dt.Rows)
                {
                    IRow iRow = iSheet.CreateRow(rowIndex);
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        iRow.CreateCell(i).SetCellValue(dRow[i].ToString());
                    }
                    rowIndex++;
                }
                #endregion

                #region 设置表格的格式
                //iSheet.SetColumnWidth(0, 5000);//第一个参数表示第几列，第二个为宽度值

                //iSheet.GetRow(0).Height = 400;//取得行索引并设置改行的高度
                
                //设置第一行单元格样式和字体
                ICellStyle icStyle = iwbExcel.CreateCellStyle();
                icStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.CENTER;
                IFont iFont = iwbExcel.CreateFont();
                //iFont.Boldweight = short.MaxValue;
                iFont.FontHeightInPoints = (short)16;
                icStyle.SetFont(iFont);
                iSheet.GetRow(0).RowStyle = icStyle;
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 数据集中的数据导出到指定的Excel文件中
        /// </summary>
        /// <param name="fileName">文件全名（含路径名）</param>
        /// <param name="dsSource">数据集实例</param>
        /// <param name="mess">反馈信息</param>
        /// <returns>导出成功返回true，失败返回false</returns>
        public static bool DataToExcel(string fileName, DataSet dsSource, ref string mess)
        {
            if (string.IsNullOrEmpty(fileName) || dsSource == null || dsSource.Tables == null) return false;
            if (mess == null) mess = string.Empty;
            try
            {
                IWorkbook iwbExcel = new HSSFWorkbook();
                int sheetIndex = 1;//用于为工作表编号（无名称时使用）
                foreach (DataTable dtItem in dsSource.Tables)
                {
                    if (dtItem == null) continue;
                    string sheetName = string.IsNullOrEmpty(dtItem.TableName) ? "sheet" + sheetIndex++ : dtItem.TableName;
                    ExportExcel.createSheet(iwbExcel, dtItem, sheetName);
                }
                MemoryStream ms = new MemoryStream();
                iwbExcel.Write(ms);
                ms.Flush();
                ms.Position = 0;

                #region 判断文件状态
                int fileStat = Dongle.Utilities.FileTools.GetFileStat(fileName);
                if (fileStat == 1)
                {
                    mess += "文件被占用！";
                    return false;
                }
                #endregion

                using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    byte[] data = ms.ToArray();
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                    data = null;
                }
                ms.Close();
                return true;
            }
            catch (Exception ex)
            {
                mess += ex.Message;
                return false;
            }
        }
    }
}
