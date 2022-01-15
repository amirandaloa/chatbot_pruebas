using EnlaceVA.Models;
using EnlaceVA.Resources;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using System;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using static EnlaceVA.Utilities.MenuSubMenuEnum;
using System.Text.RegularExpressions;

namespace EnlaceVA.Utilities
{
    public static class Utility
    {
        public static (string company, string channel) ValidateCompany(IActivity param, UserProfileState user)
        {
            string channel;
            string company;
            if (param != null && user != null)
            {
                channel = param.ChannelId;
                switch (param.ChannelId)
                {
                    case Channels.Directline:
                        {
                            company = user.Company ?? param.From.Name;
                            break;
                        }
                    case Channels.Emulator:
                        {
                            company = MenuSubMenuEnum.CompanyName.Gdo;
                            //channel = Channels.Directline;
                            break;
                        }
                    case Channels.Webchat:
                        {
                            company = user.Company ?? param.From.Name;
                            break;
                        }
                    default:
                        {
                            company = MenuSubMenuEnum.CompanyName.Quavii;
                            break;
                        }
                }
            }
            else
            {
                channel = Channels.Emulator;
                company = MenuSubMenuEnum.CompanyName.Quavii;
            }
            return (company, channel);
        }
        public class Company
        {
            public string Name { get; set; }
        }

        public static string GetMenuByCompany(UserProfileState user, string channel)
        {
            string menuName;
            if (channel != null && user != null)
            {
                var company = user.Company.ToLower(CultureInfo.CurrentCulture);

                if (channel == Channels.Directline)
                {
                    menuName = company != MenuSubMenuEnum.CompanyName.Quavii ? "WhatsappMenu" : "WhatsappMenuQuavii";
                }
                else
                {
                    menuName = company != MenuSubMenuEnum.CompanyName.Quavii ? "ReturningMenuAdaptiveCard" : "ReturningMenuQuaviiAdaptiveCard";
                }
            }
            else
            {
                menuName = MenuSubMenuEnum.Menu.Error;
            }
            return menuName;
        }

        public static string GetMenuPrincipalByCompany(UserProfileState user, string channel, string company)
        {
            string menuName;
            if (channel != null && user != null)
            {
                if (channel == Channels.Directline)
                {
                    menuName = "WhatsappMenuPrincipal";
                }
                else
                {
                    menuName = "ReturningMenuPrincipalAdaptiveCard";
                }
            }
            else
            {
                menuName = MenuSubMenuEnum.Menu.Error;
            }
            return menuName += company.ToUpper();
        }

        public static string FindQuestion(string[] list, string question)
        {
            return list.FirstOrDefault(x => x.Contains(question)).Split(")").LastOrDefault();
        }

        public static (string subMenu, bool subMenuFind) GetSubMenuByCompany(string text, string channel, string company, UserProfileState user)
        {
            string subMenuName;
            bool subMenuFind;


            if (text != null && channel != null && company != null)
            {
                var question = text.ToLower(CultureInfo.CurrentCulture);

                // error en submenu

                string menuOption = ValidateQuestion(question, company);

                if (menuOption != MenuSubMenuEnum.Menu.Error)
                {


                    if (Regex.IsMatch(question, @"^\d+$") && user.LastSubMenu != null)
                    {
                        question = FindQuestion(user.LastSubMenu, question);
                    }
                    subMenuFind = true;
                    subMenuName = GetSubmenuByChannelAndCompany(menuOption, company, channel);
                }
                else
                {
                    subMenuFind = true;
                    subMenuName = MenuSubMenuEnum.Menu.Error;
                }

            }
            else
            {
                subMenuName = MenuSubMenuEnum.Menu.Error;
                subMenuFind = false;
            }
            return (subMenuName, subMenuFind);
        }

        public static string GetSubMenuItem(string menu, string text, string company)
        {
            string subMenu;

            if (menu != null && text != null)
            {
                //menu = Regex.Replace(menu, "[" + company + "]", string.Empty);
                menu = menu.Remove((menu.Length - company.Length), company.Length);
                switch (menu)
                {
                    case MenuSubMenuEnum.Menu.Pagos:
                        {
                            subMenu = ValidationMenuSubMenu.ValidateSubmenuPagos(text);
                            break;
                        }
                    case MenuSubMenuEnum.Menu.PagosQuavii:
                        {
                            subMenu = ValidationMenuSubMenu.ValidateSubmenuPagosQuavii(text);
                            break;
                        }
                    case MenuSubMenuEnum.Menu.Tu_Factura:
                        {
                            subMenu = ValidationMenuSubMenu.ValidateSubMenuTu_Factura(text);
                            break;
                        }
                    case MenuSubMenuEnum.Menu.Tu_Recibo:
                        {
                            subMenu = ValidationMenuSubMenu.ValidateSubMenuTu_Recibo(text);
                            break;
                        }
                    case MenuSubMenuEnum.Menu.Brilla_y_seguros:
                        {
                            subMenu = ValidationMenuSubMenu.ValidateSubMenuBrilla_y_seguros(text);
                            break;
                        }
                    case MenuSubMenuEnum.Menu.Informacion_general:
                        {
                            subMenu = ValidationMenuSubMenu.ValidateSubMenuInformacion_general(text);
                            break;
                        }
                    case MenuSubMenuEnum.Menu.Informacion_generalQuavii:
                        {
                            subMenu = ValidationMenuSubMenu.ValidateSubMenuInformacion_generalQuavii(text);
                            break;
                        }
                    case MenuSubMenuEnum.Menu.Revision_periodica:
                        {
                            subMenu = ValidationMenuSubMenu.ValidateSubMenuRevision_periodica(text);
                            break;
                        }
                    case MenuSubMenuEnum.Menu.Servicios_tecnicos:
                        {
                            subMenu = ValidationMenuSubMenu.ValidateSubMenuServicios_tecnicos(text);
                            break;
                        }
                    case MenuSubMenuEnum.Menu.Gestion_deuda:
                        {
                            subMenu = ValidationMenuSubMenu.ValidateSubMenuGestion_deuda(text);
                            break;
                        }
                    case MenuSubMenuEnum.Menu.Emergencias:
                        {
                            subMenu = "salir";//ValidationMenuSubMenu.ValidateSubMenuEmergencias(text);
                            break;
                        }
                    case MenuSubMenuEnum.Menu.EmergenciasQuavii:
                        {
                            subMenu = ValidationMenuSubMenu.ValidateSubMenuEmergenciasQuavii(text);
                            break;
                        }
                    case MenuSubMenuEnum.Menu.Conectate_al_servicio:
                        {
                            subMenu = ValidationMenuSubMenu.ValidateSubMenuConectate_al_servicio(text);
                            break;
                        }
                    case MenuSubMenuEnum.Menu.Salir:
                        {
                            subMenu = MenuSubMenuEnum.Menu.Salir;
                            break;
                        }
                    default:
                        {
                            subMenu = MenuSubMenuEnum.Menu.Error;
                            break;
                        }
                }
            }
            else {
                subMenu = MenuSubMenuEnum.Menu.Error;
            }
            
            return subMenu;
        }

        public static string GetSubMenuItemWhatsapp(string menu, string text, string company)
        {
            string subMenu;

            //menu = Regex.Replace(menu, "[" + company + "]", string.Empty);
            menu = menu.Remove((menu.Length - company.Length), company.Length);
            switch (menu)
            {

                case MenuSubMenuEnum.MenuWhatsapp.Pagos :
                    {
                        subMenu = ValidationMenuSubMenu.ValidateSubmenuPagos(text);
                        break;
                    }
                case MenuSubMenuEnum.MenuWhatsapp.PagosQuavii:
                    {
                        subMenu = ValidationMenuSubMenu.ValidateSubmenuPagosQuavii(text);
                        break;
                    }
                case MenuSubMenuEnum.MenuWhatsapp.Tu_Factura:
                    {
                        subMenu = ValidationMenuSubMenu.ValidateSubMenuTu_Factura(text);
                        break;
                    }
                case MenuSubMenuEnum.MenuWhatsapp.Tu_Recibo:
                    {
                        subMenu = ValidationMenuSubMenu.ValidateSubMenuTu_Recibo(text);
                        break;
                    }
                case MenuSubMenuEnum.MenuWhatsapp.Brilla_y_seguros:
                    {
                        subMenu = ValidationMenuSubMenu.ValidateSubMenuBrilla_y_seguros(text);
                        break;
                    }
                case MenuSubMenuEnum.MenuWhatsapp.Informacion_general:
                    {
                        subMenu = ValidationMenuSubMenu.ValidateSubMenuInformacion_general(text);
                        break;
                    }
                case MenuSubMenuEnum.MenuWhatsapp.Informacion_generalQuavii:
                    {
                        subMenu = ValidationMenuSubMenu.ValidateSubMenuInformacion_generalQuavii(text);
                        break;
                    }
                case MenuSubMenuEnum.MenuWhatsapp.Revision_periodica:
                    {
                        subMenu = ValidationMenuSubMenu.ValidateSubMenuRevision_periodica(text);
                        break;
                    }
                case MenuSubMenuEnum.MenuWhatsapp.Servicios_tecnicos:
                    {
                        subMenu = ValidationMenuSubMenu.ValidateSubMenuServicios_tecnicos(text);
                        break;
                    }
                case MenuSubMenuEnum.MenuWhatsapp.Gestion_deuda:
                    {
                        subMenu = ValidationMenuSubMenu.ValidateSubMenuGestion_deuda(text);
                        break;
                    }
                case MenuSubMenuEnum.MenuWhatsapp.Emergencias:
                    {
                        subMenu = ValidationMenuSubMenu.ValidateSubMenuEmergencias(text);
                        break;
                    }
                case MenuSubMenuEnum.MenuWhatsapp.Conectate_al_servicio:
                    {
                        subMenu = ValidationMenuSubMenu.ValidateSubMenuConectate_al_servicio(text);
                        break;
                    }
                default:
                    {
                        subMenu = MenuSubMenuEnum.Menu.Error;
                        break;
                    }
            }
            return subMenu;
        }

        private static string ValidateQuestion(string question, string company)
        {

            string menu;
            if (question == "1" || question.Contains("emergencia", StringComparison.OrdinalIgnoreCase) || question.Contains("emer", StringComparison.OrdinalIgnoreCase))
            {

                menu = MenuSubMenuEnum.Menu.Emergencias;
            }
            else if (question == "2" || question.Contains("conectate", StringComparison.OrdinalIgnoreCase) || question.Contains("al Servicio", StringComparison.OrdinalIgnoreCase))
            {
                menu = MenuSubMenuEnum.Menu.Conectate_al_servicio;

            }
            else if (question == "3" || question.Contains("brilla y seguros", StringComparison.OrdinalIgnoreCase) || question.Contains("brilla y segu", StringComparison.OrdinalIgnoreCase))
            {
                menu = MenuSubMenuEnum.Menu.Brilla_y_seguros;
            }
            else if (question == "4" || question.Contains("factura", StringComparison.OrdinalIgnoreCase) || question.Contains("fact", StringComparison.OrdinalIgnoreCase))
            {
                menu = MenuSubMenuEnum.Menu.Tu_Factura;

            }
            else if (question == "5" || question.Contains("pagos", StringComparison.OrdinalIgnoreCase) || question.Contains("pago", StringComparison.OrdinalIgnoreCase))
            {
                menu = MenuSubMenuEnum.Menu.Pagos;

            }
            else if (question == "6" || question.Contains("revision", StringComparison.OrdinalIgnoreCase) || question.Contains("periodica", StringComparison.OrdinalIgnoreCase) )
            {
                menu = MenuSubMenuEnum.Menu.Revision_periodica;

            }
            else if (question == "7" || question.Contains("gestion", StringComparison.OrdinalIgnoreCase) || question.Contains("deuda", StringComparison.OrdinalIgnoreCase))
            {
                menu = MenuSubMenuEnum.Menu.Gestion_deuda;
            }
            else if
               (question == "8" || question.Contains("servicio", StringComparison.OrdinalIgnoreCase) || question.Contains("tecnico", StringComparison.OrdinalIgnoreCase))
            {
                menu = MenuSubMenuEnum.Menu.Servicios_tecnicos;

            }
            else if (question == "9" || question.Contains("informacion", StringComparison.OrdinalIgnoreCase) || question.Contains("general", StringComparison.OrdinalIgnoreCase) )
            {
                menu = MenuSubMenuEnum.Menu.Informacion_general;
            }
            else if (question.Equals("salir"))
            {
                menu = MenuSubMenuEnum.Menu.Salir;
            }
            else
            {
                menu = MenuSubMenuEnum.Menu.Error;
            }

            return menu;
        }

        private static string GetSubmenuByChannelAndCompany(string menu, string company, string channel)
        {
            string subMenuName;
            if (menu == MenuSubMenuEnum.Menu.Emergencias)
            {
                if (channel == Channels.Directline)
                {
                    //subMenuName = company != MenuSubMenuEnum.CompanyName.Quavii ? MenuSubMenuEnum.MenuWhatsapp.Emergencias : MenuSubMenuEnum.MenuWhatsapp.EmergenciasQuavii;
                    subMenuName = MenuSubMenuEnum.MenuWhatsapp.Emergencias;
                }
                else
                {
                    //subMenuName = company != MenuSubMenuEnum.CompanyName.Quavii ? MenuSubMenuEnum.Menu.Emergencias : MenuSubMenuEnum.Menu.EmergenciasQuavii;
                    subMenuName = MenuSubMenuEnum.Menu.Emergencias;
                }
            }
            else if (menu == MenuSubMenuEnum.Menu.Conectate_al_servicio)
            {
                subMenuName = channel == Channels.Directline ? MenuSubMenuEnum.MenuWhatsapp.Conectate_al_servicio : MenuSubMenuEnum.Menu.Conectate_al_servicio;
            }
            else if (menu == MenuSubMenuEnum.Menu.Brilla_y_seguros)
            {
                subMenuName = channel == Channels.Directline ? MenuSubMenuEnum.MenuWhatsapp.Brilla_y_seguros : MenuSubMenuEnum.Menu.Brilla_y_seguros;
                //subMenuName += company.ToUpper();
            }
            else if (menu == MenuSubMenuEnum.Menu.Tu_Factura)
            {
                if (channel == Channels.Directline)
                {
                    //subMenuName = company != MenuSubMenuEnum.CompanyName.Quavii ? MenuSubMenuEnum.MenuWhatsapp.Tu_Factura : MenuSubMenuEnum.MenuWhatsapp.Tu_Recibo;
                    subMenuName = MenuSubMenuEnum.MenuWhatsapp.Tu_Factura;
                }
                else
                {
                    subMenuName = MenuSubMenuEnum.Menu.Tu_Factura;
                }
            }
            else if (menu == MenuSubMenuEnum.Menu.Pagos)
            {
                if (channel == Channels.Directline)
                {
                    //subMenuName = company != MenuSubMenuEnum.CompanyName.Quavii ? MenuSubMenuEnum.MenuWhatsapp.Pagos : MenuSubMenuEnum.MenuWhatsapp.Tu_Recibo;
                    subMenuName = MenuSubMenuEnum.MenuWhatsapp.Pagos;
                }
                else
                {
                    //subMenuName = company != MenuSubMenuEnum.CompanyName.Quavii ? MenuSubMenuEnum.Menu.Pagos : MenuSubMenuEnum.Menu.PagosQuavii;
                    subMenuName = MenuSubMenuEnum.Menu.Pagos;
                }
            }
            else if (menu == MenuSubMenuEnum.Menu.Revision_periodica)
            {
                subMenuName = channel == Channels.Directline ? MenuSubMenuEnum.MenuWhatsapp.Revision_periodica : MenuSubMenuEnum.Menu.Revision_periodica;
            }
            else if (menu == MenuSubMenuEnum.Menu.Gestion_deuda)
            {
                subMenuName = channel == Channels.Directline ? MenuSubMenuEnum.MenuWhatsapp.Gestion_deuda : MenuSubMenuEnum.Menu.Gestion_deuda;
            }
            else if (menu == MenuSubMenuEnum.Menu.Servicios_tecnicos)
            {
                subMenuName = channel == Channels.Directline ? MenuSubMenuEnum.MenuWhatsapp.Servicios_tecnicos : MenuSubMenuEnum.Menu.Servicios_tecnicos;
            }
            else if (menu == MenuSubMenuEnum.Menu.Informacion_general)
            {
                if (channel == Channels.Directline)
                {
                    //subMenuName = company != MenuSubMenuEnum.CompanyName.Quavii ? MenuSubMenuEnum.MenuWhatsapp.Informacion_general : MenuSubMenuEnum.MenuWhatsapp.Informacion_generalQuavii;
                    subMenuName = MenuSubMenuEnum.MenuWhatsapp.Informacion_general ;
                }
                else
                {

                    //subMenuName = company != MenuSubMenuEnum.CompanyName.Quavii ? MenuSubMenuEnum.Menu.Informacion_general : MenuSubMenuEnum.Menu.Informacion_generalQuavii;
                    subMenuName = MenuSubMenuEnum.Menu.Informacion_general;
                }
            }
            else if (menu == MenuSubMenuEnum.Menu.Salir)
            {
                subMenuName = MenuSubMenuEnum.Menu.Salir;
            }
            else 
            {
                subMenuName = MenuSubMenuEnum.Menu.Error;
            }
            return subMenuName += company.ToUpper();
        }

        public static string RemoveAccents(this string text)
        {
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }

        public static string GetTypeIdByCompany(string company)
        {
            string typeId;
            switch (company)
            {
                case CompanyName.Gdo:
                    {
                        typeId = "GetTypeIdGDOSTG";
                        break;
                    }
                case CompanyName.Ceo:
                    {
                        typeId = "GetTypeIdCEO";
                        break;
                    }
                case CompanyName.Quavii:
                    {
                        typeId = "GetTypeIdQUAVII";
                        break;
                    }
                case CompanyName.Stg:
                    {
                        typeId = "GetTypeIdGDOSTG";
                        break;
                    }
                default:
                    {
                        typeId = "GetTypeIdGDOSTG";
                        break;
                    }
                }
            return typeId;
        }

        public static string GetTypePolicy(string company)
        {
            string text;
            company = company.ToLower(CultureInfo.CurrentCulture);

            text = company switch
            {
                MenuSubMenuEnum.CompanyName.Gdo => "GetTypePolicySTG",
                MenuSubMenuEnum.CompanyName.Stg => "GetTypePolicyGDO",
                _ => "ErrorResult"
            };

            return text;

        }
        public static string ValidateTypeidCompanyCard(string company)
        {
            string text;
            company = company.ToLower(CultureInfo.CurrentCulture);
            ResourceManager resourceManagerResponse = new ResourceManager(typeof(ResponsesResource));

            text = company switch
            {
                MenuSubMenuEnum.CompanyName.Gdo => resourceManagerResponse.GetString("TIGdoStg"),
                MenuSubMenuEnum.CompanyName.Ceo => resourceManagerResponse.GetString("TICeo"),
                MenuSubMenuEnum.CompanyName.Stg => resourceManagerResponse.GetString("TIGdoStg"),
                MenuSubMenuEnum.CompanyName.Quavii => resourceManagerResponse.GetString("TIQuavii"),
                _ => ""
            };

            return text;
        }

        public static void WriteLog(string text) 
        {

            string path = AppDomain.CurrentDomain.BaseDirectory + @"\LogServices.txt" ;

            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                string createText = text + Environment.NewLine;
                File.WriteAllText(path, createText);
            }
            else 
            {
                string appendText = text + Environment.NewLine;
                File.AppendAllText(path, appendText);
            }

            string readText = File.ReadAllText(path);
            Console.WriteLine(readText);

        }

        public static string FormatDate(string Fecha)
        {
            var message = Fecha;
            if (!String.IsNullOrEmpty(Fecha))
            {
                string[] date = Fecha.Split(' ');
                message = date[0];                
            }

            return message;
        }

        public static string Formatmoney(string number, string company)
        {
            if (!String.IsNullOrEmpty(number))
            {
                string Currency = string.Format(CultureInfo.InvariantCulture, "{0:#,###}", Convert.ToInt64(number));

                if (number=="0") 
                {
                    Currency = number;
                }

                if (company == CompanyName.Quavii)                               
                    Currency = "/S" + Currency;                                    
                else                                    
                    Currency = "$" + Currency;  
                
                return Currency;
            }
            else
            {
                return number;
            }

        }

        public static string GetIdCompany(string company)
        {
            string idCompany;
            switch (company)
            {
                case CompanyName.Gdo:
                    {
                        idCompany = "9999";
                        break;
                    }
                case CompanyName.Stg:
                    {
                        idCompany = "121212";
                        break;
                    }
                default:
                    {
                        idCompany = "";
                        break;
                    }
            }
            return idCompany;
        }

        public static string GetDescriptionProduct(string idTypeProduct)
        {
            string descriptionProduct;
            switch (idTypeProduct)
            {
                case "1":
                    {
                        descriptionProduct = "1GASODOMESTICOS ";
                        break;
                    }
                case "2":
                    {
                        descriptionProduct = "2MATERIALES PARA LA CONSTRUCCIÓN ";
                        break;
                    }
                case "3":
                    {
                        descriptionProduct = "3HERRAMIENTAS ";
                        break;
                    }
                case "4":
                    {
                        descriptionProduct = "4EDUCACIÓN ";
                        break;
                    }
                case "5":
                    {
                        descriptionProduct = "5CDA Y SERVITECAS ";
                        break;
                    }
                case "6":
                    {
                        descriptionProduct = "6ELECTRODOMESTICOS MAYORES Y MENORES ";
                        break;
                    }
                case "7":
                    {
                        descriptionProduct = "7AUDIO, VIDEO Y COMPUTADORES ";
                        break;
                    }
                case "8":
                    {
                        descriptionProduct = "8TELEFONIA MOVIL ";
                        break;
                    }
                case "9":
                    {
                        descriptionProduct = "9MOTOS ";
                        break;
                    }
                case "10":
                    {
                        descriptionProduct = "10BICICLETAS ";
                        break;
                    }
                case "11":
                    {
                        descriptionProduct = "11ROPA Y CALZADO ";
                        break;
                    }
                case "12":
                    {
                        descriptionProduct = "12MUEBLES Y COLCHONES ";
                        break;
                    }
                default:
                    {
                        descriptionProduct = "";
                        break;
                    }
            }
            return descriptionProduct;
        }

        public static string GetCompanyName(string company)
        {
            string contactMessage;
            switch (company)
            {
                case CompanyName.Gdo:
                    {
                        contactMessage = CompanyName.GdoName;
                        break;
                    }
                case CompanyName.Ceo:
                    {
                        contactMessage = CompanyName.CeoName;
                        break;
                    }
                case CompanyName.Quavii:
                    {
                        contactMessage = CompanyName.QuaviiName;
                        break;
                    }
                case CompanyName.Stg:
                    {
                        contactMessage = CompanyName.StgName;
                        break;
                    }
                default:
                    {
                        contactMessage = "";
                        break;
                    }
            }
            return contactMessage;
        }

        public static string GetContactInfo(string company)
        {
            string companyName;
            switch (company)
            {
                case CompanyName.Gdo:
                    {
                        companyName = "mensajeContactoGDO";
                        break;
                    }
                case CompanyName.Ceo:
                    {
                        companyName = "mensajeContactoCEO";
                        break;
                    }
                case CompanyName.Quavii:
                    {
                        companyName = "mensajeContactoQuavii";
                            
                        break;
                    }
                case CompanyName.Stg:
                    {
                        companyName = "mensajeContactoSTG";
                        break;
                    }
                default:
                    {
                        companyName = "";
                        break;
                    }
            }
            return companyName;
        }
    }
}
