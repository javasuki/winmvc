using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Reflection;

using NXDO.Data.Reflection;
using NXDO.Data.Factory;
using NXDO.Data.Extension;

namespace NXDO.Data.MVCP
{
    /// <summary>
    /// 转换成模型扩展
    /// </summary>
    public static class MvcToDataModelExtension
    {
        /// <summary>
        /// 表格数据行转成模型
        /// </summary>
        /// <typeparam name="TModel">模型类型</typeparam>
        /// <param name="row">表格控件的数据行</param>
        /// <returns>模型实例</returns>
        public static TModel GetModel<TModel>(this DataGridViewRow row)
            where TModel : class, new()
        {
            TModel m = new TModel();
            if (row == null) return m;

            foreach (var ph in typeof(TModel).GetProperties())
            {
                object v = row.Cells[ph.Name].Value;
                if (v == null) continue;
                if (v == DBNull.Value) continue;
                ph.SetValueEx(m, v);
            }
            return m;
        }

        /// <summary>
        /// 获取模型
        /// </summary>
        /// <typeparam name="TModel">模型类型</typeparam>
        /// <param name="mvcRow">mvc框架的行对象</param>
        /// <returns>模型实例</returns>
        public static TModel GetModel<TModel>(this IMvcDataGridViewRow mvcRow)
            where TModel : class, new()
        {
            return mvcRow.CurrentRow.GetModel<TModel>();
        }

        /// <summary>
        /// 获取模型
        /// </summary>
        /// <typeparam name="TModel">模型类型</typeparam>
        /// <param name="rowView">行自定义视图</param>
        /// <returns>模型实例</returns>
        public static TModel GetModel<TModel>(this DataRowView rowView)
            where TModel : class, new()
        {
            return MvcToDataModelExtension.GetModel<TModel>(rowView.Row);
        }

        /// <summary>
        /// 获取模型
        /// </summary>
        /// <typeparam name="TModel">模型类型</typeparam>
        /// <param name="row">数据行</param>
        /// <returns>模型实例</returns>
        public static TModel GetModel<TModel>(this DataRow row)
            where TModel : class, new()
        {
            TModel m = new TModel();
            if (row == null) return m;

            foreach (var ph in typeof(TModel).GetProperties())
            {
                if (row.IsNull(ph.Name)) continue;
                object v = row[ph.Name];
                if (v == null) continue;
                if (v == DBNull.Value) continue;
                ph.SetValueEx(m, v);
            }
            return m;
        }


        /// <summary>
        /// 获取单元格的值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="mvcRow">单元格</param>
        /// <param name="index">索引</param>
        /// <returns>单元格的值</returns>
        public static T GetCellValue<T>(this IMvcDataGridViewRow mvcRow, int index)
        {
            if (index < 0 || index > mvcRow.CurrentRow.Cells.Count - 1)
                throw new IndexOutOfRangeException("index 超出单元格的索引范围。");
            object v = mvcRow.CurrentRow.Cells[index].Value;
            if (v == null) return default(T);
            return (T)v;
        }
    }
}
