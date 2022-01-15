﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnlaceVA.Models
{
    public class DatosEntradaPagoParcial
    {        
        public string contrato { get; set; }
        public string pagoParcial { get; set; }
        public string tipoIdentificacion { get; set; }
        public string identificacion { get; set; }
        public string cliente { get; set; }
        public string correo { get; set; }
        public string telefono { get; set; }
    }
}
