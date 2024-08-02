using System;
using System.Collections.Generic;
using System.Text;

namespace CtsWrapper.CtsObjects
{
    public class CTSProcedureResponse : CTSMessageData
    {
        private List<MensajeMQ> _mensajesMQ = new List<MensajeMQ>();
        private List<CTSResultSet> _resultsets = new List<CTSResultSet>();
        private List<CTSParameter> _outputParams = new List<CTSParameter>();

        public String Return { get; set; }

        public List<CTSResultSet> ResultSet {
            get
            {
                return _resultsets;
            }
        }
        public List<MensajeMQ> MensajesMQ {
            get
            {
                return _mensajesMQ;
            }
        }

        public List<CTSParameter> OutputParams
        {
            get
            {
                return _outputParams;
            }
        }
    }
}
