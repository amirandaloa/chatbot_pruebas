using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnlaceVA.Models
{
    public class Datos
    {
        public string contrato { get; set; }
        public string solicitud { get; set; }

        public string fechaDeLectura { get; set; }

        public string valorDeLectura { get; set; }

        public Mensaje mensajes { get; set; }

    }
}
