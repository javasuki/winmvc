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
    /// Dev.GridView行对象的MVC架构
    /// </summary>
    public interface IMvcGridViewRow
    {
        /// <summary>
        /// 获取当前表格行映射的模型。
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <returns>模型实例</returns>
        T GetModel<T>() where T : class, new();

        /// <summary>
        /// 获取当前表格行上指定单元格内(属性或字段)的值。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="index">指定单元格的索引。</param>
        /// <returns>指定单元格内的值</returns>
        T GetCellValue<T>(int index);

        /// <summary>
        /// 获取当前表格行上指定单元格内(属性或字段)的值。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="PropertyOrFieldName">属性或字段名称</param>
        /// <returns>指定单元格内的值</returns>
        T GetCellValue<T>(string PropertyOrFieldName);

        /// <summary>
        /// 获取表格行对应的原始对象。
        /// </summary>
        object CurrentRow { get; }
    }

    /// <summary>
    /// DevExpress.XtraGrid.GridControl的MVC框架
    /// </summary>
    public interface IMvcGridView
    {
        /// <summary>
        /// 执行控制器中的模型新增操作。
        /// <para>约定操作名称为 SaveNewModel 的无参方法。</para>
        /// </summary>
        IMvcGridView ActionNew();

        /// <summary>
        /// 执行控制器中的模型编辑操作。
        /// <para>约定操作名称为 SaveEditModel 的无参方法。</para>
        /// </summary>
        IMvcGridView ActionEdit();

        /// <summary>
        /// 执行控制器中的模型删除操作。
        /// <para>约定操作名称为 DeleteModel 的无参方法。</para>
        /// </summary>
        IMvcGridView ActionDelete();

        /// <summary>
        /// 执行控制器中的模型新增操作。
        /// <para>约定操作名称为 SaveNewModel 的方法。</para>
        /// </summary>
        /// <param name="funcActionParameterValues">提供方法参数值的委托</param>
        IMvcGridView ActionNew(Func<IMvcGridViewRow, object> funcActionParameterValues);

        /// <summary>
        /// 执行控制器中的模型编辑操作。
        /// <para>约定操作名称为 SaveEditModel 的方法。</para>
        /// </summary>
        /// <param name="funcActionParameterValues">提供方法参数值的委托</param>
        IMvcGridView ActionEdit(Func<IMvcGridViewRow, object> funcActionParameterValues);

        /// <summary>
        /// 执行控制器中的模型删除操作。
        /// <para>约定操作名称为 DeleteModel 的方法。</para>
        /// </summary>
        /// <param name="funcActionParameterValues">提供方法参数值的委托</param>
        IMvcGridView ActionDelete(Func<IMvcGridViewRow, object> funcActionParameterValues);

        /// <summary>
        /// 执行控制器中的模型新增操作。
        /// </summary>
        /// <param name="actionName">指定操作名称</param>
        /// <param name="funcActionParameterValues">提供方法参数值的委托</param>
        IMvcGridView ActionNew(string actionName, Func<IMvcGridViewRow, object> funcActionParameterValues);

        /// <summary>
        /// 执行控制器中的模型编辑操作。
        /// </summary>
        /// <param name="actionName">指定操作名称</param>
        /// <param name="funcActionParameterValues">提供方法参数值的委托</param>
        IMvcGridView ActionEdit(string actionName, Func<IMvcGridViewRow, object> funcActionParameterValues);

        /// <summary>
        /// 执行控制器中的模型删除操作。
        /// </summary>
        /// <param name="actionName">指定操作名称</param>
        /// <param name="funcActionParameterValues">提供方法参数值的委托</param>
        IMvcGridView ActionDelete(string actionName, Func<IMvcGridViewRow, object> funcActionParameterValues);

        IMvcGridView ActionAny(string actionName);
        IMvcGridView ActionAny(string actionName, Func<IMvcGridViewRow, object> funcActionParameterValues);
    }
    #endregion

    #region DevRow
    internal class DevRow : IMvcGridViewRow
    {
        Type modelType;
        public DevRow(object currentRow, Type modelType)
        {
            this.CurrentRow = currentRow;
            this.modelType = modelType;

        }

        public T GetModel<T>()
            where T : class,new()
        {
            if (typeof(T).IsSubclassOf(modelType) || typeof(T) == modelType)
                return (T)this.CurrentRow;

            var v = this.CurrentRow as System.Data.DataRowView;
            if (v != null)
                return v.GetModel<T>();
            else
            {
                var vr = this.CurrentRow as System.Data.DataRow;
                if (vr != null)
                    return vr.GetModel<T>();
            }

            throw new ArgumentNullException("无法获取T类型的模型实例");
        }

        public T GetCellValue<T>(int index)
        {
            if (this.CurrentRow == null)
                return default(T);

            if (this.CurrentRow.GetType().IsSubclassOf(modelType) || this.CurrentRow.GetType() == modelType)
            {
                var pinfos = this.CurrentRow.GetType().GetProperties();
                if (pinfos.Length == 0)
                    throw new ArgumentNullException("无法获取第" + index + "个属性的值。");
                return pinfos[index].GetValueEx<T>(this.CurrentRow);
            }
            
            object t = null;
            var v = this.CurrentRow as System.Data.DataRowView;
            if (v != null)
                t = v.Row[index];
            else
                throw new ArgumentNullException("无法获取非DataTable数据源的索引数据.");

            return (T)t;
        }

        public T GetCellValue<T>(string PropertyOrFieldName)
        {
            if (this.CurrentRow == null)
                return default(T);

            if (this.CurrentRow.GetType().IsSubclassOf(modelType) || this.CurrentRow.GetType() == modelType)
            {
                var pinfos = this.CurrentRow.GetType().GetProperties();
                if (pinfos.Length == 0)
                    throw new ArgumentNullException("无法获取" + PropertyOrFieldName + "属性的值。");

                var rs = from p in pinfos where p.Name.ToLower() == PropertyOrFieldName.ToLower() select p;
                if(rs.Count() == 0)
                    throw new ArgumentNullException("无法获取" + PropertyOrFieldName + "属性的值。");

                if (rs.Count() > 1)
                {
                    rs = from p in pinfos where p.Name == PropertyOrFieldName select p;
                    if (rs.Count() == 0)
                        throw new ArgumentNullException("无法获取" + PropertyOrFieldName + "属性的值。");
                }

                return rs.First().GetValueEx<T>(this.CurrentRow);
            }

            object t = null;
            var v = this.CurrentRow as System.Data.DataRowView;
            if (v != null)
                t = v.Row[PropertyOrFieldName];
            else
                throw new ArgumentNullException("无法获取非DataTable数据源的索引数据.");

            return (T)t;
        }

        public object CurrentRow
        {
            get;
            private set;
        }

        private bool IsModelType(Type tType)
        {
            bool b = (tType.IsSubclassOf(modelType) || tType == modelType);
            return b;
        }


        
    }
    #endregion

    internal class MvcGridControl : IMvcGridView
    {
        IMvcHelper iMvc;
        public MvcGridControl(object gv, IMvcHelper imvc)
        {
            this.iMvc = imvc;
            if (this.iMvc.IsRegistered) return;

            #region 事件处理
            Type gvType = gv.GetType();
            var eventInfoUpdated = gvType.GetEvent("RowUpdated");
            var eventInfoKeyDown = gvType.GetEvent("KeyDown");
            Action<dynamic, dynamic> actRowUpdated = (s, e) =>
            {
                bool isNew = s.IsNewItemRow(e.RowHandle);
                if (isNew)
                {
                    if (string.IsNullOrWhiteSpace(this.newActionName)) return;
                    object prmVal = null;
                    if (this.funcNewValues != null)
                    {
                        var dr = new DevRow(e.Row, iMvc.GetModelType());
                        prmVal = this.funcNewValues.Invoke(dr);
                    }
                    else
                        this.iMvc.GridFirstColumnCellValue = ControllerAction.GetKeyValue(e.Row, this.iMvc);
                    this.iMvc.Action(this.newActionName, prmVal);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(this.editActionName)) return;
                    object prmVal = null;
                    if (this.funcEditValues != null)
                    {
                        var dr = new DevRow(e.Row, iMvc.GetModelType());
                        prmVal = this.funcEditValues.Invoke(dr);
                    }
                    else
                        this.iMvc.GridFirstColumnCellValue = ControllerAction.GetKeyValue(e.Row, this.iMvc);
                    this.iMvc.Action(this.editActionName, prmVal);
                }
            };

            Action<dynamic, KeyEventArgs> actRowKeyDown = (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(this.deleteActionName)) return;
                if (s.IsEditing) return;
                if (e.KeyCode != Keys.Delete) return;

                int hwnd = s.GetSelectedRows()[0];
                var row = s.GetRow(hwnd);
                object prmVal = null;
                if (this.funcDeleteValues != null)
                {
                    var dr = new DevRow(row, iMvc.GetModelType());
                    prmVal = this.funcDeleteValues.Invoke(dr);
                }
                else
                    this.iMvc.GridFirstColumnCellValue = ControllerAction.GetKeyValue(row, this.iMvc);
                this.iMvc.Action(this.deleteActionName, prmVal);

                s.DeleteRow(hwnd);
            };
            #endregion

            #region 事件附加
            //附加 RowUpdated 事件
            var ewObjectUpdated = typeof(EventWrapper<>)
                                .MakeGenericType(gvType.Assembly.GetType("DevExpress.XtraGrid.Views.Base.RowObjectEventArgs"))
                                .GetMethod("GetWrapper", BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                                .Invoke(null, new object[] { actRowUpdated });
            var eventDelegateUpdated = Delegate.CreateDelegate(eventInfoUpdated.EventHandlerType, ewObjectUpdated, "Invoke");
            eventInfoUpdated.AddEventHandler(gv, eventDelegateUpdated);

            //附加 KeyDown 事件
            var eventDelegateKeyDown = Delegate.CreateDelegate(eventInfoKeyDown.EventHandlerType, new EventWrapper<KeyEventArgs>(actRowKeyDown), "Invoke");
            eventInfoKeyDown.AddEventHandler(gv, eventDelegateKeyDown);
            #endregion
        }


        string newActionName = "";
        string editActionName = "";
        string deleteActionName = "";
        List<string> lstAnyActionNames;

        Func<IMvcGridViewRow, object> funcNewValues;
        Func<IMvcGridViewRow, object> funcEditValues;
        Func<IMvcGridViewRow, object> funcDeleteValues;
        List<Func<IMvcGridViewRow, object>> lstFuncAnyValues;

        public IMvcGridView ActionNew()
        {
            return this.ActionNew(null);
        }

        public IMvcGridView ActionEdit()
        {
            return this.ActionEdit(null);
        }

        public IMvcGridView ActionDelete()
        {
            return this.ActionDelete(null);
        }

        public IMvcGridView ActionNew(Func<IMvcGridViewRow, object> funcActionParameterValues)
        {
            return this.ActionNew("SaveNewModel", funcActionParameterValues);
        }

        public IMvcGridView ActionEdit(Func<IMvcGridViewRow, object> funcActionParameterValues)
        {
            return this.ActionEdit("SaveEditModel", funcActionParameterValues);
        }

        public IMvcGridView ActionDelete(Func<IMvcGridViewRow, object> funcActionParameterValues)
        {
            return this.ActionDelete("DeleteModel", funcActionParameterValues);
        }

        public IMvcGridView ActionNew(string actionName, Func<IMvcGridViewRow, object> funcActionParameterValues)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                throw new ArgumentNullException("actionName");

            this.newActionName = actionName;
            this.funcNewValues = funcActionParameterValues;
            return this;
        }

        public IMvcGridView ActionEdit(string actionName, Func<IMvcGridViewRow, object> funcActionParameterValues)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                throw new ArgumentNullException("actionName");

            this.editActionName = actionName;
            this.funcEditValues = funcActionParameterValues;
            return this;
        }

        public IMvcGridView ActionDelete(string actionName, Func<IMvcGridViewRow, object> funcActionParameterValues)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                throw new ArgumentNullException("actionName");

            this.deleteActionName = actionName;
            this.funcDeleteValues = funcActionParameterValues;
            return this;
        }


        public IMvcGridView ActionAny(string actionName)
        {
            return this.ActionAny(actionName, null);
        }

        public IMvcGridView ActionAny(string actionName, Func<IMvcGridViewRow, object> funcActionParameterValues)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                throw new ArgumentNullException("actionName");

            if (this.lstAnyActionNames == null)
                this.lstAnyActionNames = new List<string>();
            if (this.lstFuncAnyValues == null)
                this.lstFuncAnyValues = new List<Func<IMvcGridViewRow, object>>();

            this.lstAnyActionNames.Add(actionName);
            this.lstFuncAnyValues.Add(funcActionParameterValues);
            return this;
        }
    }
}
