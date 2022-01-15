using EnlaceVA.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static EnlaceVA.Utilities.MenuSubMenuEnum;

namespace EnlaceVA.Utilities
{
    public class DataValidations
    {
        protected DataValidations()
        {
        }

        public static string ValidateContract(string Contrato)
        {
            int n;
            string message;
            bool result = Int32.TryParse(Contrato, out n);
            if (result)
            {
                var rgx = new Regex(@"^[0-9]{1,10}$");
                if (rgx.IsMatch(n.ToString()))
                {
                    message = MessagesCertificadoFacturaDialog.TextContrato;
                }
                else
                {
                    message = MessagesCertificadoFacturaDialog.Error;
                }
            }
            else
            {
                message = MessagesCertificadoFacturaDialog.Error;
            }
            return message;
        }

        public static string ValidateTypeId(string TypeId, string company)
        {
            var message = MessagesCertificadoFacturaDialog.Error;
            bool result = Int32.TryParse(TypeId, out int n);

            string[] values = ValidateValueTypeId(company).Split(',');
            int Cantidadvalue = values.Count();

            if (result)
            {
                string Realvalue = ValidateCompanyForTypeId(n, company);
                for (int i = 0; i < Cantidadvalue; i++)
                {
                    if (Realvalue == values[i])
                    {
                        message = MessagesCertificadoFacturaDialog.TextTipoId;
                        break;
                    }
                }
            }
            else
            {
                message = MessagesCertificadoFacturaDialog.Error;
            }
            return message;
        }

        public static string ValidateCompanyForTypeId(int value, string Company)
        {
            switch (Company)
            {
                case CompanyName.Ceo:
                    value = 110 + value;
                    break;
                case CompanyName.Quavii:
                    value = value switch
                    {
                        1 => 2,
                        2 => 3,
                        3 => 6,
                        4 => 7,
                        _ => 0
                    };
                    break;
                default:
                    if (value == 5)
                        value = 110;
                    break;
            }

            return value.ToString();
        }

        public static string ValidateValueTypeId(string Company)
        {
            string message;
            message = Company switch
            {
                CompanyName.Gdo => "1,2,3,4,110",
                CompanyName.Ceo => "111,112,113,114,115,116",
                CompanyName.Stg => "1,2,3,4,110",
                CompanyName.Quavii => "2,3,6,7",
                _ => ""
            };

            return message;
        }

        public static string ValidateTypePolicy(string TypePolicy)
        {
            string message;
            var rgx = new Regex(@"^[1-4]{1}$");
            if (rgx.IsMatch(TypePolicy))
            {
                message = MessagesCertificadoFacturaDialog.TextTipoPolicy;
            }
            else
            {
                message = MessagesCertificadoFacturaDialog.Error;
            }
            return message;
        }

        public static string ValidateObservation(string Observation)
        {
            string message;
            if (!String.IsNullOrEmpty(Observation))
            {
                message = MessagesCertificadoFacturaDialog.TextObservation;
            }
            else
            {
                message = MessagesCertificadoFacturaDialog.Error;
            }
            return message;
        }

        public static string ValidateId(string id)
        {
            string message;
            var rgx = new Regex(@"^[0-9]{6,15}$");
            if (rgx.IsMatch(id))
            {
                message = MessagesCertificadoFacturaDialog.TextId;
            }
            else
            {
                message = MessagesCertificadoFacturaDialog.ErrorId;
            }

            return message;
        }

        public static string ValidateCardId(string id,string tipeId)
        {
            string message;
            var rgx = new Regex(@"^[a-zA-Z 0-9 a-zA-Z]{6,15}$");
            var rgxN = new Regex(@"^[0-9]{6,15}$");

            if (String.IsNullOrEmpty(tipeId))
            {
                tipeId = "1";
            }

            //if (tipeId == "2" || tipeId == "112" || tipeId == "7")
            if (tipeId == "2" || tipeId == "112" || tipeId == "3" || tipeId == "113" || tipeId == "7")
            {
                if (rgx.IsMatch(id))
                {
                    message = MessagesCertificadoFacturaDialog.TextId;
                }
                else
                {
                    message = MessagesCertificadoFacturaDialog.ErrorId;
                }
            }
            else
            {
                if (rgxN.IsMatch(id))
                {
                    message = MessagesCertificadoFacturaDialog.TextId;
                }
                else
                {
                    message = MessagesCertificadoFacturaDialog.ErrorId;
                }
            }
            

            return message;
        }

        public static string Validatename(string name)
        {
            string message;
            var rgx = new Regex(@"^[a-zA-Z  ]{1,100}$");
            if (rgx.IsMatch(name))
            {
                message = MessagesCertificadoFacturaDialog.TextName;
            }
            else
            {
                message = MessagesCertificadoFacturaDialog.Error;
            }
            return message;
        }

        public static string ValidateEmail(string Email)
        {
            string message;
            var rgx = new Regex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+){1,100}$");
            if (rgx.IsMatch(Email))
            {
                message = MessagesCertificadoFacturaDialog.TextEmail;
            }
            else
            {
                message = MessagesCertificadoFacturaDialog.ErrorEmail;
            }
            return message;
        }

        public static string ValidatePhone(string Phone)
        {
            string message;
            var rgx10 = new Regex(@"^[0-9]{7,10}$");
            var rgx7 = new Regex(@"^[0-9]{7,10}$");
            if (rgx10.IsMatch(Phone) || rgx7.IsMatch(Phone))
            {
                message = MessagesCertificadoFacturaDialog.TextPhone;
            }
            else
            {
                message = MessagesCertificadoFacturaDialog.ErrorTelefono;
            }
            return message;
        }

        public static string ValidateRequest(string request)
        {
            Int64 n;
            string message;
            bool result = Int64.TryParse(request, out n);
            if (result)
            {
                var rgx = new Regex(@"^[0-9]{1,15}$");
                if (rgx.IsMatch(n.ToString()))
                {
                    message = MessagesCertificadoFacturaDialog.TextSolicitud;
                }
                else
                {
                    message = MessagesCertificadoFacturaDialog.Error;
                }
            }
            else
            {
                message = MessagesCertificadoFacturaDialog.Error;
            }
            return message;
        }

        public static string ValidateProducto(string request)
        {
            Int64 n;
            string message;
            bool result = Int64.TryParse(request, out n);
            if (result)
            {
                var rgx = new Regex(@"^[0-9]{1,15}$");
                if (rgx.IsMatch(n.ToString()))
                {
                    message = MessagesCertificadoFacturaDialog.TextProducto;
                }
                else
                {
                    message = MessagesCertificadoFacturaDialog.Error;
                }
            }
            else
            {
                message = MessagesCertificadoFacturaDialog.Error;
            }
            return message;
        }

        public static string ValidateCompany(string company)
        {
            string message;
            company = company.ToLower(CultureInfo.CurrentCulture);
            switch (company)
            {
                case "gdo":
                    message = "https://vaenlacepromdev-bot.azurewebsites.net/Images/GDO.jpg";
                    return message;
                case "ceo":
                    message = "https://vaenlacepromdev-bot.azurewebsites.net/Images/ImagenContratoCEO.png";
                    return message;
                case "stg":
                    message = "https://vaenlacepromdev-bot.azurewebsites.net/Images/STG2.jpg";
                    return message;
                case "quavii":
                    message = "https://vaenlacepromdev-bot.azurewebsites.net/Images/QUAVII.PNG";
                    return message;
                default:
                    message = "";
                    return message;
            }
        }
        public static string ValidatePay(string pago, string deuda)
        {
            int n;
            string message;
            bool result = Int32.TryParse(pago, out n);
            if (result)
            {
                var rgx = new Regex(@"^[0-9]{1,13}$");
                if (rgx.IsMatch(n.ToString()))
                {
                    if(Int32.Parse(pago) < Int32.Parse(deuda))
                    {
                        message = MessagesCertificadoFacturaDialog.TextPay;
                    }
                    else
                    {
                        message = MessagesCertificadoFacturaDialog.TextPayError;
                    }
                }
                else
                {
                    message = MessagesCertificadoFacturaDialog.Error;
                }
            }
            else
            {
                message = MessagesCertificadoFacturaDialog.Error;
            }
            return message;
        }

       

        public static string ValidateConfirmPay(string ConfirmPay)
        {
            int n;
            string message;
            bool result = Int32.TryParse(ConfirmPay, out n);
            if (result)
            {
                var rgx = new Regex(@"^[1-2]{1}$");
                if (rgx.IsMatch(n.ToString()))
                {
                    message = MessagesCertificadoFacturaDialog.TextConfirmPay;
                }
                else
                {
                    message = MessagesCertificadoFacturaDialog.Error;
                }
            }
            else
            {
                message = MessagesCertificadoFacturaDialog.Error;
            }
            return message;
        }

        public static string ValidateCardContract(GeneralInformation data)
        {
            var message = "";

            if (data != null)
            {
                if (String.IsNullOrEmpty(data.contrato))
                {
                    message = "\n" + "Por favor digite un contrato" + "\n";
                }
                else
                {
                    if (ValidateContract(data.contrato) != MessagesCertificadoFacturaDialog.TextContrato)
                    {
                        message = "\n" + "Por favor digite una valor correcto para el contrato" + "\n";
                    }
                }
            }
            else
            {
                message = "El campo contrato no tiene datos";
            }

            return message;
        }

        public static string ValidateCardTypeId(GeneralInformation data)
        {
            var message = "";

            if (String.IsNullOrEmpty(data.tipoIdentificacion))
            {
                message = "\n" + "Tipo de identificacion" + "\n";
            }
            else
            {
                if (ValidateTypeId(data.tipoIdentificacion, data.company) != MessagesCertificadoFacturaDialog.TextTipoId)
                {
                    message = "\n" + "Tipo de identificacion" + "\n";
                }
            }

            return message;
        }

        public static string ValidateCardId(GeneralInformation data)
        {
            var message = "";

            if (String.IsNullOrEmpty(data.identificacion))
            {
                message = "\n" + "Numero de identificacion" + "\n";
            }
            else
            {
                if (ValidateCardId(data.identificacion,data.tipoIdentificacion) != MessagesCertificadoFacturaDialog.TextId)
                {
                    message = "\n" + "Numero de identificacion" + "\n";
                }
            }

            return message;
        }

        public static string ValidateCardName(GeneralInformation data)
        {
            var message = "";

            if (String.IsNullOrEmpty(data.cliente))
            {
                message = "\n" + "Nombres y Apellidos" + "\n";
            }
            else
            {
                if (Validatename(data.cliente) != MessagesCertificadoFacturaDialog.TextName)
                {
                    message = "\n" + "Nombres y Apellidos: Recuerde que no debe llevar tildes o caracteres especiales." + "\n";
                }
            }

            return message;
        }

        public static string ValidateCardEmail(GeneralInformation data)
        {
            var message = "";

            if (String.IsNullOrEmpty(data.correo))
            {
                message = "\n" + "Digite un Correo electronico" + "\n";
            }
            else
            {
                if (ValidateEmail(data.correo.Trim()) != MessagesCertificadoFacturaDialog.TextEmail)
                {
                    message = "\n" + ValidateEmail(data.correo.Trim()) + "\n";
                }
            }

            return message;
        }

        public static string ValidateCardPhone(GeneralInformation data)
        {
            var message = "";

            if (String.IsNullOrEmpty(data.telefono))
            {
                message = "\n" + "Digite un Teléfono de contacto " + "\n";
            }
            else
            {
                if (ValidatePhone(data.telefono) != MessagesCertificadoFacturaDialog.TextPhone)
                {
                    message = "\n" + ValidatePhone(data.telefono) + "\n";
                }
            }

            return message;
        }

        public static string ValidateCardTypePolicy(GeneralInformation data)
        {
            var message = "";

            if (String.IsNullOrEmpty(data.tipoPoliza))
            {
                message = "\n" + "Tipo de póliza" + "\n";
            }
            else
            {
                if (ValidateTypePolicy(data.tipoPoliza) != MessagesCertificadoFacturaDialog.TextTipoPolicy)
                {
                    message = "\n" + "Tipo de póliza" + "\n";
                }
            }

            return message;
        }

        public static string ValidateCardObservation(GeneralInformation data)
        {
            var message = "";

            if (String.IsNullOrEmpty(data.observacionSeguro))
            {
                message = "\n" + "Tipo de póliza que desea adquirir" + "\n";
            }
            else
            {
                if (ValidateObservation(data.observacionSeguro) != MessagesCertificadoFacturaDialog.TextObservation)
                {
                    message = "\n" + "Tipo de póliza que desea adquirir" + "\n";
                }
            }

            return message;
        }

        public static string ValidateCardObservationFinanciacion(GeneralInformation data)
        {
            var message = "";

            if (String.IsNullOrEmpty(data.observacionProducto))
            {
                message = "\n" + "Observación de producto" + "\n";
            }
            else
            {
                if (ValidateObservation(data.observacionProducto) != MessagesCertificadoFacturaDialog.TextObservation)
                {
                    message = "\n" + "Observación de producto" + "\n";
                }
            }

            return message;
        }

        public static string ValidateCardBasic(GeneralInformation data)
        {
            var message = "Los siguientes campos deben ser diligenciados o corregidos:\n";

            if (data != null)
            {
                var message1 = ValidateCardTypeId(data);
                var message2 = ValidateCardId(data);
                var message3 = ValidateCardName(data);
                var message4 = ValidateCardEmail(data);
                var message5 = ValidateCardPhone(data);

                if (!String.IsNullOrEmpty(message1))
                {
                    message += message1;
                }
                if (!String.IsNullOrEmpty(message2))
                {
                    message += message2;
                }
                if (!String.IsNullOrEmpty(message3))
                {
                    message += message3;
                }
                if (!String.IsNullOrEmpty(message4))
                {
                    message += message4;
                }
                if (!String.IsNullOrEmpty(message5))
                {
                    message += message5;
                }
            }
            else
            {
                message = "Todos los campos es vacios";
            }

            if (message == "Los siguientes campos deben ser diligenciados o corregidos:\n")
            {
                message = "";
            }

            return message;

        }

        public static string ValidateVentaSeguroCard(GeneralInformation data)
        {
            var message = "Los siguientes campos deben ser diligenciados o corregidos:\n";

            if (data != null)
            {
                var message1 = ValidateCardTypeId(data);
                var message2 = ValidateCardId(data);
                var message3 = ValidateCardName(data);
                var message4 = ValidateCardEmail(data);
                var message5 = ValidateCardPhone(data);
                var message6 = ValidateCardTypePolicy(data);
                var message7 = ValidateCardObservation(data);

                if (!String.IsNullOrEmpty(message1))
                {
                    message += message1;
                }
                if (!String.IsNullOrEmpty(message2))
                {
                    message += message2;
                }
                if (!String.IsNullOrEmpty(message3))
                {
                    message += message3;
                }
                if (!String.IsNullOrEmpty(message4))
                {
                    message += message4;
                }
                if (!String.IsNullOrEmpty(message5))
                {
                    message += message5;
                }
                if (!String.IsNullOrEmpty(message6))
                {
                    message += message6;
                }
                if (!String.IsNullOrEmpty(message7))
                {
                    message += message7;
                }
            }
            else
            {
                message = "Todos los campos es vacios";
            }

            if (message == "Los siguientes campos deben ser diligenciados o corregidos:\n")
            {
                message = "";
            }

            return message;

        }

        public static string ValidateVisitaFinanciacionCard(GeneralInformation data)
        {
            var message = "Los siguientes campos deben ser diligenciados o corregidos:\n";

            if (data != null)
            {
                var message6 = ValidateCardObservationFinanciacion(data);
                var message1 = ValidateCardTypeId(data);
                var message2 = ValidateCardId(data);
                var message3 = ValidateCardName(data);
                var message4 = ValidateCardEmail(data);
                var message5 = ValidateCardPhone(data);


                if (!String.IsNullOrEmpty(message1))
                {
                    message += message1;
                }
                if (!String.IsNullOrEmpty(message2))
                {
                    message += message2;
                }
                if (!String.IsNullOrEmpty(message3))
                {
                    message += message3;
                }
                if (!String.IsNullOrEmpty(message4))
                {
                    message += message4;
                }
                if (!String.IsNullOrEmpty(message5))
                {
                    message += message5;
                }
                if (!String.IsNullOrEmpty(message6))
                {
                    message += message6;
                }
            }
            else
            {
                message = "Todos los campos es vacios";
            }

            if (message == "Los siguientes campos deben ser diligenciados o corregidos:\n")
            {
                message = "";
            }

            return message;

        }

        public static string ValidateProductoCard(GeneralInformation data)
        {
            var message = "Debe escoger un producto";

            if (data.producto != null)
            {
                if (data.producto == "2" || data.producto == "4" || data.producto == "5" || data.producto == "9" || data.producto == "11")
                {
                    if (data.company == "gdo")
                    {
                        message = "Para realizar la solicitud de dicho producto debe ingresar a https://www.brilladegasesdeoccidente.com/solicita-la-visita-de-un-asesor";
                    }else
                    {
                        message = "Para realizar la solicitud de dicho producto debe ingresar a https://www.brilladesurtigas.com/que-puedo-financiar";
                    }
                }
                else
                {
                    message = "";
                }
            }
            else
            {
                message = "Debe escoger un producto";
            }

            return message;

        }

        public static string ValidateCardRequest(GeneralInformation data)
        {
            var message = "";

            if (data != null)
            {
                if (String.IsNullOrEmpty(data.solicitud))
                {
                    message = "\n" + "Por favor digite un numero de solicitud" + "\n";
                }
                else
                {
                    if (ValidateContract(data.solicitud) != MessagesCertificadoFacturaDialog.TextContrato)
                    {
                        message = "\n" + "Por favor digite una valor correcto para el numero de solicitud" + "\n";
                    }
                }
            }
            else
            {
                message = "El campo solicitud no tiene datos";
            }

            return message;
        }

        public static string ValidateCardGetPay(GeneralInformation data)
        {
            var message = "Los siguientes campos deben ser diligenciados o corregidos:\n";

            if (data != null)
            {
                var message1 = ValidateCardPay(data);

                if (!String.IsNullOrEmpty(message1))
                {
                    message += message1;
                }
            }
            else
            {
                message = "Todos los campos es vacios";
            }

            if (message == "Los siguientes campos deben ser diligenciados o corregidos:\n")
            {
                message = "ReturningConfirmPay";
            }

            return message;

        }

        public static string ValidateCardPay(GeneralInformation data)
        {
            var message = "";

            if (String.IsNullOrEmpty(data.pago))
            {
                message = "\n" + "Pago a realizar" + "\n";
            }
            else
            {
                if (ValidatePay(data.pago, data.saldoPendiente) != MessagesCertificadoFacturaDialog.TextPay)
                {
                    message = "\n" + "Pago a realizar" + "\n";
                }
            }

            return message;
        }

        public static string ValidatedataTreatment(string data)
        {
            data = data.ToLower(CultureInfo.CurrentCulture);

            if (data == "si")
            {
                data = "1";
            }
            else
            {
                data = "2";
            }

            return data;
        }

        public static class MessagesCertificadoFacturaDialog
        {
            public const string TextContrato = "Contrato";
            public const string TextProducto = "Producto";
            public const string TextTipoPolicy = "TipoPolicy";
            public const string TextObservation = "Observation";
            public const string TextTipoId = "TipoId";
            public const string TextId = "Id";
            public const string TextName = "Name";
            public const string TextEmail = "Email";
            public const string TextPhone = "Phone";
            public const string TextPay = "Pay";
            public const string TextPayError = "PayError";
            public const string TextConfirmPay = "ConfirmPay";
            public const string TextPagoErroneo = "PagoErroneo";
            public const string Error = "Por favor seleccione una opción válida, o digita salir para volver a preguntar.";
            public const string ErrorEmail = "El correo tiene un formato inválido, Recuerda usar un formato valido, ejemplo: 'example@test.###'";
            public const string ErrorTelefono = "Ingrese su número de contacto de 7 o 10 digitos sin espacios ni puntos.";
            public const string ErrorId = "Ingrese su número de identificación sin espacios ni puntos.";
            public const string TextSolicitud = "Solicitud";
        }
        public static string GetUrlPagosByCompany(string company)
        {
            string url;
            switch (company)
            {
                case CompanyName.Gdo:
                    {
                        url = "www.gdo.com.co/Paginas/Pago.aspx";
                        break;
                    }
                case CompanyName.Ceo:
                    {
                        url = "GetTypeIdCEO";
                        break;
                    }
                case CompanyName.Quavii:
                    {
                        url = "GetTypeIdQUAVII";
                        break;
                    }
                case CompanyName.Stg:
                    {
                        url = "www.surtigas.com.co/paga-tu-factura";
                        break;
                    }
                default:
                    {
                        url = "www.gdo.com.co/Paginas/Pago.aspx";
                        break;
                    }
            }
            return url;
        }
        public static string GetDesviacionCompany(string company, string tieneDesviacion)
        {
            string desviacion;
            switch (company)
            {
                case CompanyName.Gdo:
                    {
                        if (tieneDesviacion == "Y")
                        {
                            desviacion = "AplicaDesviacionGDO";
                        }
                        else
                        {
                            desviacion = "NoAplicaDesviacionGDO";
                        }

                        break;
                    }
                case CompanyName.Ceo:
                    {
                        if (tieneDesviacion == "Y")
                        {
                            desviacion = "AplicaDesviacionGDO";
                        }
                        else
                        {
                            desviacion = "NoAplicaDesviacionGDO";
                        }
                        break;
                    }
                case CompanyName.Quavii:
                    {
                        if (tieneDesviacion == "Y")
                        {
                            desviacion = "AplicaDesviacionGDO";
                        }
                        else
                        {
                            desviacion = "NoAplicaDesviacionGDO";
                        }
                        break;
                    }
                case CompanyName.Stg:
                    {
                        if (tieneDesviacion == "Y")
                        {
                            desviacion = "AplicaDesviacionSTG";
                        }
                        else
                        {
                            desviacion = "NoAplicaDesviacionSTG";
                        }
                        break;
                    }
                default:
                    {
                        if (tieneDesviacion == "Y")
                        {
                            desviacion = "AplicaDesviacionGDO";
                        }
                        else
                        {
                            desviacion = "NoAplicaDesviacionGDO";
                        }
                        break;
                    }
            }
            return desviacion;
        }
    }
}



