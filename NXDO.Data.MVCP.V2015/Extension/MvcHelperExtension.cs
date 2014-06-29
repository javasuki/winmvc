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
    /// Mvc扩展
    /// </summary>
    public static class MvcHelperExtension
    {
        /// <summary>
        /// DataGridView的MVC框架
        /// </summary>
        /// <param name="dataGridView">表格</param>
        /// <returns>MVC框架</returns>
        public static IMvcDataGridView ToMvc(this DataGridView dataGridView)
        {
            dynamic dyForm = dataGridView.FindForm();
            var imvc = dyForm.Mvc as IMvcHelper;
            imvc.IsGridMvc = true;
            return new MvcDataGridView(dataGridView, imvc);
        }

        /// <summary>
        /// 第三方表格实现的MVC框架扩展
        /// </summary>
        /// <typeparam name="T">第三方表格类型</typeparam>
        /// <typeparam name="TModel">模型类型</typeparam>
        /// <param name="anyGridView">第三方表格实例</param>
        /// <param name="mvcHelper">视图帮助器实例</param>
        /// <param name="gridViewRow">第三方表格某行的实例对象</param>
        /// <returns>MVC框架</returns>
        public static IMvcAnyGrid ToMvcAnyGrid<T, TModel>(this T anyGridView, MvcHelper<TModel> mvcHelper, object gridViewRow)
            where TModel : class, new()
        {
            var helper = mvcHelper as IMvcHelper;
            helper.IsGridMvc = true;
            return new ControllerAction(helper, gridViewRow);
        }

        /// <summary>
        /// DevExpress.XtraGrid.GridControl 表格实现的MVC框架扩展
        /// </summary>
        /// <typeparam name="TControl">GridControl类型</typeparam>
        /// <param name="control">表格控件实例</param>
        /// <returns>MVC框架</returns>
        public static IMvcGridView ToMvcDXGrid<TControl>(this TControl control)
            where TControl : Control
        {
            if (control == null)
                throw new ArgumentNullException("control");

            if (typeof(TControl).FullName.CompareTo("DevExpress.XtraGrid.GridControl") != 0)
                throw new ArgumentException("必须是 DevExpress.XtraGrid.GridControl 类型的实例。", "control");

            dynamic dyForm = control.FindForm();
            var imvc = dyForm.Mvc as IMvcHelper;
            imvc.IsGridMvc = true;

            var gridView = ((dynamic)control).MainView;
            return new MvcGridControl(gridView, imvc);
        }
    }

}
