using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CtsWrapper.CtsObjects
{
    public class CTSResultSet
    {
        private List<CTSColumn> _header = new List<CTSColumn>();
        private ArrayList _itemArray = new ArrayList();

        public List<CTSColumn> Header
        {
            get
            {
                return _header;
            }
        }

        public ArrayList ItemArray
        {
            get
            {
                return _itemArray;
            }
        }

        

    }
}
