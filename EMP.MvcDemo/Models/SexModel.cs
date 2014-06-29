using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMP.MvcDemo.Models
{
    class SexModel
    {

        public string Name
        {
            get;
            set;
        }

        public bool Value
        {
            get;
            set;
        }

        public static List<SexModel> Data
        {
            get
            {
                if (_Sexs == null)
                {
                    _Sexs = new List<SexModel>
                    {
                        new SexModel { Name="man", Value= true},
                        new SexModel { Name="woman", Value= false}
                    };
                }
                return _Sexs;
            }
        }static List<SexModel> _Sexs;
    }

    
}
