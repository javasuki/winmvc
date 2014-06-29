using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NXDO.Data.MVCP
{
    public abstract class ActionResult
    {
    }

    public sealed class EmptyResult : ActionResult
    {
        internal EmptyResult()
        {
        }
    }
}
