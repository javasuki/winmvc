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
    #region interface
    /// <summary>
    /// DataGridViewRow的MVC框架
    /// </summary>
    public interface IMvcDataGridViewRow
    {
        /// <summary>
        /// 当前操作的表格数据行
        /// </summary>
        DataGridViewRow CurrentRow { get; }
    }

    /// <summary>
    /// DataGridView的MVC框架
    /// </summary>
    public interface IMvcDataGridView : IMvcDataGridViewRow
    {
        /// <summary>
        /// 执行控制器中的模型新增操作
        /// </summary>
        /// <param name="actionName">操作名称，缺省时新增操作定义为无参方法。</param>
        /// <returns>当前框架</returns>
        IMvcDataGridView ActionNew(string actionName = "SaveNewModel");

        /// <summary>
        /// 执行控制器中的模型编辑操作
        /// </summary>
        /// <param name="actionName">操作名称，缺省时编辑操作定义为无参方法。</param>
        /// <returns>当前框架</returns>
        IMvcDataGridView ActionEdit(string actionName = "SaveEditModel");

        /// <summary>
        /// 执行控制器中的模型删除操作
        /// </summary>
        /// <param name="actionName">操作名称，缺省时删除操作定义为无参方法。</param>
        /// <returns>当前框架</returns>
        IMvcDataGridView ActionDelete(string actionName = "DeleteModel");

        /// <summary>
        /// 执行控制器中的模型新增操作
        /// </summary>
        /// <param name="actionName">操作名称</param>
        /// <param name="funcActionParameterValues">提供方法参数值的委托</param>
        /// <returns>当前框架</returns>
        IMvcDataGridView ActionNew(string actionName, Func<IMvcDataGridViewRow, object> funcActionParameterValues);

        /// <summary>
        /// 执行控制器中的模型编辑操作
        /// </summary>
        /// <param name="actionName">操作名称</param>
        /// <param name="funcActionParameterValues">提供方法参数值的委托</param>
        /// <returns>当前框架</returns>
        IMvcDataGridView ActionEdit(string actionName, Func<IMvcDataGridViewRow, object> funcActionParameterValues);

        /// <summary>
        /// 执行控制器中的模型删除操作
        /// </summary>
        /// <param name="actionName">操作名称</param>
        /// <param name="funcActionParameterValues">提供方法参数值的委托</param>
        /// <returns>当前框架</returns>
        IMvcDataGridView ActionDelete(string actionName, Func<IMvcDataGridViewRow, object> funcActionParameterValues);
    }
    #endregion

    /// <summary>
    /// MvcDataGridView
    /// </summary>
    internal class MvcDataGridView : IMvcDataGridView
    {
        int currChangeRowIndex = -1;
        bool bChged = false;
        bool bNew = false;
        IMvcHelper iMvc;

        public MvcDataGridView(DataGridView dataGridView, IMvcHelper imvc)
        {
            this.iMvc = imvc;
            if (this.iMvc.IsRegistered) return;

            #region 执行控制器的子操作
            Action<int, int> actExecPost = (iFlag, iCurrentRowIndex) =>
            {
                if (iCurrentRowIndex > -1)
                    this.CurrentRow = dataGridView.Rows[iCurrentRowIndex];
                else
                    this.CurrentRow = null;

                object oPrmValue = null;
                string currActionName = string.Empty;
                if (iFlag == 0)
                {
                    currActionName = this.newActionName;
                    oPrmValue = this.funcNewValues == null ? null : this.funcNewValues(this);
                }
                else if (iFlag == 1)
                {
                    currActionName = this.editActionName;
                    oPrmValue = this.funcEditValues == null ? null : this.funcEditValues(this);
                }
                else if (iFlag == 2)
                {
                    currActionName = this.deleteActionName;
                    oPrmValue = this.funcDeleteValues == null ? null : this.funcDeleteValues(this);
                }

                if (string.IsNullOrWhiteSpace(currActionName)) return;

                if (oPrmValue == null && (this.funcNewValues == null || this.funcEditValues == null || this.funcDeleteValues == null))
                {
                    if (this.CurrentRow != null)
                    {
                        this.iMvc.GridFirstColumnCellValue = this.GetKeyValue();
                    }
                    this.iMvc.Action(currActionName);
                }
                else
                    this.iMvc.Action(currActionName, oPrmValue);
            };
            #endregion

            #region DataGridView事件与属性
            dataGridView.VirtualMode = true;
            dataGridView.CellEnter += (s, e) =>
            {
                bool isSubmited = false;
                //当进入单元格时，判断是否添加或编辑，则引发对应子操作
                if (bNew && currChangeRowIndex != e.RowIndex)
                {
                    actExecPost(0, currChangeRowIndex); //System.Diagnostics.Debug.WriteLine("new row update:" + currChangeRowIndex);
                    isSubmited = true;
                }
                else if (currChangeRowIndex != e.RowIndex && bChged)
                {
                    actExecPost(1, currChangeRowIndex); //System.Diagnostics.Debug.WriteLine("edit update:" + currChangeRowIndex);
                    isSubmited = true;
                }

                if (isSubmited)
                {
                    bChged = false;
                    bNew = false;
                }
            };

            dataGridView.CellValueChanged += (s, e) =>
            {
                bChged = true;
            };

            dataGridView.CellLeave += (s, e) =>
            {
                currChangeRowIndex = e.RowIndex;
            };

            dataGridView.Leave += (s, e) =>
            {
                //当GRID失去焦点时，判断提交数据的改变，并引发对应子操作
                if (bNew)
                {
                    //System.Diagnostics.Debug.WriteLine("new row(leave) update:" + currChangeRowIndex);
                    actExecPost(0, currChangeRowIndex);
                    bNew = false;
                    bChged = false;
                }

                if (!bChged) return;
                //System.Diagnostics.Debug.WriteLine("ocx update:" + currChangeRowIndex);
                actExecPost(1, currChangeRowIndex);

                bChged = false;
            };

            dataGridView.UserAddedRow += (s, e) =>
            {
                bNew = true;
            };

            dataGridView.CancelRowEdit += (s, e) =>
            {
                bNew = false;
                bChged = false;
            };

            #region 行头单元格选中,切换编辑或行选中
            dataGridView.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (e.ColumnIndex < 0)
                {
                    dataGridView.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
                    dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dataGridView.ClearSelection();
                    dataGridView.EndEdit();
                    dataGridView.Rows[e.RowIndex].Selected = true;
                    return;
                }

                dataGridView.EditMode = DataGridViewEditMode.EditOnEnter;
                dataGridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
            };
            #endregion

            dataGridView.UserDeletingRow += (s, e) =>
            {
                //System.Diagnostics.Debug.WriteLine("delete:" + e.Row.Index);
                actExecPost(2, e.Row.Index);
            };
            #endregion
        }

        private object GetKeyValue()
        {
            string keyFldName = string.Empty;
            object data = this.iMvc.GetDataSource();
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

                var keyValue = this.CurrentRow.Cells[keyFldName];
                return new { Name = keyFldName, Value = keyValue };
            }

            keyFldName = this.CurrentRow.DataGridView.Columns[0].Name;
            return new { Name = keyFldName, Value = this.CurrentRow.Cells[0].Value };
        }

        string newActionName = "";
        string editActionName = "";
        string deleteActionName = "";

        Func<IMvcDataGridViewRow, object> funcNewValues;
        Func<IMvcDataGridViewRow, object> funcEditValues;
        Func<IMvcDataGridViewRow, object> funcDeleteValues;
        public IMvcDataGridView ActionNew(string actionName, Func<IMvcDataGridViewRow, object> funcActionParameterValues)
        {
            this.newActionName = actionName;
            this.funcNewValues = funcActionParameterValues;
            return this;
        }

        public IMvcDataGridView ActionEdit(string actionName, Func<IMvcDataGridViewRow, object> funcActionParameterValues)
        {
            this.editActionName = actionName;
            this.funcEditValues = funcActionParameterValues;
            return this;
        }

        public IMvcDataGridView ActionDelete(string actionName, Func<IMvcDataGridViewRow, object> funcActionParameterValues)
        {
            this.deleteActionName = actionName;
            this.funcDeleteValues = funcActionParameterValues;
            return this;
        }

        public IMvcDataGridView ActionNew(string actionName = "SaveNewModel")
        {
            return this.ActionNew(actionName, null);
        }

        public IMvcDataGridView ActionEdit(string actionName = "SaveEditModel")
        {
            return this.ActionEdit(actionName, null);
        }

        public IMvcDataGridView ActionDelete(string actionName = "DeleteModel")
        {
            return this.ActionDelete(actionName, null);
        }

        public DataGridViewRow CurrentRow
        {
            get;
            private set;
        }
    }
}
