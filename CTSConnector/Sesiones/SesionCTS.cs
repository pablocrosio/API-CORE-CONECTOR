using System;
using System.Collections.Generic;
using System.Text;

namespace CTSConnector.Sesiones
{
    public class SesionCTS
    {
        public String Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool SesionIniciada { get; internal set; } = false;
        public DateTime FechaExpiracion { get; internal set; }
    }
}
