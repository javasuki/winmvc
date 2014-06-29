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
    public static class MvcAnyGridHelperExtension
    {
        public static IMvcAnyGrid ToMvc<T,TModel>(this T anyGridView, MvcHelper<TModel> mvcHelper, object gridViewRow)
            where TModel : class, new()
        {
            var helper = mvcHelper as IMvcHelper;
            helper.IsGridMvc = true;
            return new ControllerAction(helper, gridViewRow);
        }
    }

    public interface IMvcAnyGrid
    {
        /// <summary>
        /// 执行控制器中的模型新增操作
        /// <para>约定操作名称为 SaveNewModel 的无参方法。xx.ToMvc(anyGridView,helper, gridViewRow/*不能为空*/)</para>
        /// </summary>
        void ActionNew();

        /// <summary>
        /// 执行控制器中的模型编辑操作
        /// <para>约定操作名称为 SaveEditModel 的无参方法。xx.ToMvc(anyGridView,helper, gridViewRow/*不能为空*/)</para>
        /// </summary>
        void ActionEdit();

        /// <summary>
        /// 执行控制器中的模型删除操作
        /// <para>约定操作名称为 SaveEditModel 的无参方法。xx.ToMvc(anyGridView,helper, gridViewRow/*不能为空*/)</para>
        /// </summary>
        void ActionDelete();

        /// <summary>
        /// 执行控制器中的模型新增操作
        /// <para>约定操作名称为 SaveNewModel 的无参方法。</para>
        /// </summary>
        /// <param name="anonymousValues">方法参数，必须提供模型类型或匿名类型。</param>
        void ActionNew(object anonymousValues);
        void ActionEdit(object anonymousValues);
        void ActionDelete(object anonymousValues);

        void ActionNew(string actionName, object anonymousValues);
        void ActionEdit(string actionName, object anonymousValues);
        void ActionDelete(string actionName, object anonymousValues);
    }

    internal class ControllerAction : IMvcAnyGrid
    {
        IMvcHelper helper;
        object gridViewRow;
        public ControllerAction(IMvcHelper helper, object gridViewRow)
        {
            this.helper = helper;
            this.gridViewRow = gridViewRow;
        }

        public void ActionNew()
        {
            this.ActionNew("SaveNewModel", null);
        }

        public void ActionEdit()
        {
            this.ActionEdit("SaveEditModel", null);
        }

        public void ActionDelete()
        {
            this.ActionDelete("DeleteModel", null);
        }

        public void ActionNew(object anonymousValues)
        {
            this.ActionNew("SaveNewModel", anonymousValues);
        }

        public void ActionEdit(object anonymousValues)
        {
            this.ActionEdit("SaveEditModel", anonymousValues);
        }

        public void ActionDelete(object anonymousValues)
        {
            this.ActionDelete("DeleteModel", anonymousValues);
        }

        public void ActionNew(string actionName, object anonymousValues)
        {
            this.execAction(actionName, anonymousValues);
        }

        public void ActionEdit(string actionName, object anonymousValues)
        {
            this.execAction(actionName, anonymousValues);
        }

        public void ActionDelete(string actionName, object anonymousValues)
        {
            this.execAction(actionName, anonymousValues);
        }

        private void execAction(string actionName, object anonymousValues)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                throw new ArgumentNullException("actionName");

            if (anonymousValues == null && this.gridViewRow == null)
                throw new ArgumentNullException("gridViewRow","ToMvc方法中，必须提供 gridViewRow 参数值。");

            if (anonymousValues == null)
                this.helper.GridFirstColumnCellValue = this.GetKeyValue();
            this.helper.Action(actionName, anonymousValues);
        }

        private object GetKeyValue()
        {
            string keyFldName = string.Empty;
            object data = this.helper.GetDataSource();
            var tbl = data as DataTable;
            if (tbl != null)
            {
                if (tbl.PrimaryKey != null)
                {
                    if (tbl.PrimaryKey.Length > 0)
                        keyFldName = tbl.PrimaryKey[0].ColumnName;
                }
                if (string.IsNullOrWhiteSpace(keyFldName))
                    keyFldName = tbl.Columns[0].ColumnName;

                var row = this.gridViewRow as System.Data.DataRow;
                if (row == null)
                    row = (this.gridViewRow as System.Data.DataRowView).Row;

                var keyValue = row[keyFldName];
                return new { Name = keyFldName, Value = keyValue };
            }

            return this.gridViewRow;
        }
    }
}
