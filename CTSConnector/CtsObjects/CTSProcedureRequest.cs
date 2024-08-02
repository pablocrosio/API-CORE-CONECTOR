using System;
using System.Collections.Generic;
using System.Text;

namespace CtsWrapper.CtsObjects
{
    public class CTSProcedureRequest : CTSMessageData
    {
        private List<CTSParameter> _parametros = new List<CTSParameter>();

        public String SpName { get; set; }
        public List<CTSParameter> Parametros {
            get
            {
                return _parametros;
            }
        }
    }
}
