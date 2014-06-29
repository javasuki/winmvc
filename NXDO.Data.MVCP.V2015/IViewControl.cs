using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NXDO.Data.Factory;

namespace NXDO.Data.MVCP
{
    public interface IViewControl<TModel> where TModel : class, new()
    {
        MvcHelper<TModel> Mvc { get; set; }

        void Register(MvcHelper<TModel> mvc);
    }
}
