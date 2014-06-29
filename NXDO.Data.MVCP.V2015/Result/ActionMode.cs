using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NXDO.Data.MVCP
{
    /// <summary>
    /// 指定标识符以指示模态视图的返回值
    /// </summary>
    public enum ActionMode : uint
    {
        None = 0,
        OK,
        Cancel,
        Abort,
        Retry,
        Ignore,
        Yes,
        No
    }

    internal static class ActionModeExtension
    {
        public static ActionMode GetActionMode(this int value)
        {
            var o = Enum.ToObject(typeof(ActionMode), value);
            return (ActionMode)o;
        }
    }

    /// <summary>
    /// 指定标识符以指示操作的执行类型
    /// </summary>
    public enum ToActionFlag
    {
        /// <summary>
        /// 普通视图操作
        /// </summary>
        View,

        /// <summary>
        /// 普通视图操作,已存在时直接激活。
        /// </summary>
        ViewActived,

        /// <summary>
        /// 模态视图操作
        /// </summary>
        Dialog,

        /// <summary>
        /// Mdi子视图操作。
        /// <para>不存在Mdi父视图时，作为普通视图呈现。</para>
        /// </summary>
        Child,

        /// <summary>
        /// Mdi子视图操作,已存在时直接激活。
        /// <para>不存在Mdi父视图时，作为普通视图呈现。</para>
        /// </summary>
        ChildActived
    }
}
