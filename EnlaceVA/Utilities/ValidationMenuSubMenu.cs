
namespace EnlaceVA.Utilities
{
    using System.Globalization;
    using System;
    public static class ValidationMenuSubMenu
    {
        public static string ValidateSubmenuPagos(string text)
        {
            string subMenu;

            var question = text.ToLower(CultureInfo.CurrentCulture);

            if (question.Equals("1") || question.Contains("abona a tu factura", StringComparison.OrdinalIgnoreCase) || question.Contains("tu factura", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Abona_a_tu_factura;
            }
            else if (question.Equals("2") || question.Contains("solicita tu cupon de pago anticipado", StringComparison.OrdinalIgnoreCase)
               || question.Contains("cupon de pago anticipado", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Solicita_tu_cupon_de_pago_anticipado;
            }
            else if (question.Equals("3") || question.Contains("traslado de pago", StringComparison.OrdinalIgnoreCase)
                || question.Contains("traslado pago", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Traslado_de_pago;
            }
            else if (question.Equals("4") || question.Contains("certificado de pago", StringComparison.OrdinalIgnoreCase)
                || question.Contains("certificado pago", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Constancia_de_pago;
            }
            else if (question.Equals("5") || question.Contains("reconexion del servicio", StringComparison.OrdinalIgnoreCase)
                || question.Contains("reconexión servicio", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Reconexion_del_servicio;
            }
            else if (question.Equals("6") || question.Contains("estado reconexión por pago", StringComparison.OrdinalIgnoreCase)
                || question.Contains("estado reconexion por pago", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Estado_reconexión_por_pago;
            }
            else if (question.Equals("7") || question.Contains("puntos de pago", StringComparison.OrdinalIgnoreCase)
                || question.Contains("puntos pago", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Puntos_de_pagos;
            }

            else if (question.Equals("8") || question.Contains("pago con tarjeta credito", StringComparison.OrdinalIgnoreCase)
                || question.Contains("pago tarjeta credito", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Pago_con_tarjeta_credito;
            }
            else if(question.Equals("salir"))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Salir;
            }
            else
            {
                subMenu = MenuSubMenuEnum.Menu.Error;
            }

            return subMenu;
        }

        public static string ValidateSubmenuPagosQuavii(string text)
        {
            string subMenu;

            var question = text.ToLower(CultureInfo.CurrentCulture);

            if (question.Equals("1") || question.Contains("abona a tu factura", StringComparison.OrdinalIgnoreCase) || question.Contains("tu factura", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Abona_a_tu_factura;
            }
            else if (question.Equals("2") || question.Contains("solicita tu cupon de pago anticipado", StringComparison.OrdinalIgnoreCase)
               || question.Contains("cupon de pago anticipado", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Solicita_tu_cupon_de_pago_anticipado;
            }
            else if (question.Equals("3") || question.Contains("traslado de pago", StringComparison.OrdinalIgnoreCase)
                || question.Contains("traslado pago", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Traslado_de_pago;
            }
            else if (question.Equals("4") || question.Contains("constancia de pago", StringComparison.OrdinalIgnoreCase)
                || question.Contains("contancia pago", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Constancia_de_pago;
            }
            else if (question.Equals("5") || question.Contains("reconexion del servicio", StringComparison.OrdinalIgnoreCase)
                || question.Contains("reconexion servicio", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Reconexion_del_servicio;
            }
            else if (question.Equals("6") || question.Contains("estado reconexión por pago", StringComparison.OrdinalIgnoreCase)
                || question.Contains("estado reconexión por pago", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Estado_reconexión_por_pago;
            }
            else if (question.Equals("7") || question.Contains("puntos de pago", StringComparison.OrdinalIgnoreCase)
                || question.Contains("puntos pago", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Puntos_de_pagos;
            }
            else if (question.Equals("8") || question.Contains("pago con tarjeta credito", StringComparison.OrdinalIgnoreCase)
                || question.Contains("pago tarjeta credito", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Pago_con_tarjeta_credito;
            }
            else if (question.Equals("salir"))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Salir;
            }
            else
            {
                subMenu = MenuSubMenuEnum.Menu.Error;
            }

            return subMenu;
        }

        public static string ValidateSubMenuTu_Factura(string text)
        {
            string subMenu;
            var question = text.ToLower(CultureInfo.CurrentCulture);
            if (question.Equals("1") || question.Contains("consumo facturado", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Consumo_facturado;
            }
            else if (question.Equals("2") || question.Contains("ultima lectura", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Ultima_lectura;
            }
            else if (question.Equals("3") || question.Contains("consulta y generacion de factura", StringComparison.OrdinalIgnoreCase)
                 || question.Contains("consulta de factura", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Consulta_y_generacion_de_factura;
            }
            else if (question.Equals("4") || question.Contains("valor, fecha limite y referencia pago", StringComparison.OrdinalIgnoreCase)
                 || question.Contains("valor fecha limite y referencia pago", StringComparison.OrdinalIgnoreCase) )
            {
                subMenu = MenuSubMenuEnum.SubMenu.Valor_fecha_limite_y_referencia_pago;
            }
            else if (question.Equals("5") || question.Contains("deuda actual y diferida", StringComparison.OrdinalIgnoreCase)
                 || question.Contains("deuda actual", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Deuda_actual_y_diferida;
            }
            else if (question.Equals("6") || question.Contains("certificado de tus facturas", StringComparison.OrdinalIgnoreCase)
                 || question.Contains("certificado de facturas", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Certificado_de_tus_facturas;
            }
            else if (question.Equals("7") || question.Contains("certificado deuda diferida", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Certificado_deuda_diferida;
            }
            else if (question.Equals("8") || question.Contains("explicacion de la factura", StringComparison.OrdinalIgnoreCase)
                 || question.Contains("explicacion factura", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Explicacion_de_la_factura;
            }
            else if (question.Equals("9") || question.Contains("informacion tarifas", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Informacion_tarifas;
            }
            else if (question.Equals("10") || question.Contains("exencion de contribucion", StringComparison.OrdinalIgnoreCase)
                || question.Contains("exencion contribucion", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Exencion_de_contribucion;
            }
            else if (question.Equals("salir"))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Salir;
            }
            else
            {
                subMenu = MenuSubMenuEnum.Menu.Error;
            }
            return subMenu;
        }

        public static string ValidateSubMenuTu_Recibo(string text)
        {
            string subMenu;
            var question = text.ToLower(CultureInfo.CurrentCulture);
            if (question.Equals("1") || question.Contains("consumo facturado", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Consumo_facturado;
            }
            else if (question.Equals("2") || question.Contains("ultima lectura", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Ultima_lectura;
            }
            else if (question.Equals("3") || question.Contains("consulta y generacion de factura", StringComparison.OrdinalIgnoreCase)
                 || question.Contains("consulta de factura", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Consulta_y_generacion_de_factura;
            }
            else if (question.Equals("4") || question.Contains("valor, fecha limite y referencia pago", StringComparison.OrdinalIgnoreCase)
                 || question.Contains("valor fecha limite y referencia pago", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Valor_fecha_limite_y_referencia_pago;
            }
            else if (question.Equals("5") || question.Contains("deuda actual y diferida", StringComparison.OrdinalIgnoreCase)
                 || question.Contains("deuda actual", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Deuda_actual_y_diferida;
            }
            else if (question.Equals("6") || question.Contains("certificado de tus facturas", StringComparison.OrdinalIgnoreCase)
                 || question.Contains("certificado de facturas", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Certificado_de_tus_facturas;
            }
            else if (question.Equals("7") || question.Contains("certificado deuda diferida", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Certificado_deuda_diferida;
            }
            else if (question.Equals("8") || question.Contains("explicacion de la factura", StringComparison.OrdinalIgnoreCase)
                 || question.Contains("explicacion factura", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Explicacion_de_la_factura;
            }
            else if (question.Equals("9") || question.Contains("informacion tarifas", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Informacion_tarifas;
            }
            else if (question.Equals("10") || question.Contains("exencion de contribucion", StringComparison.OrdinalIgnoreCase)
                || question.Contains("exencion contribucion", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Exencion_de_contribucion;
            }
            else if (question.Equals("salir"))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Salir;
            }
            else
            {
                subMenu = MenuSubMenuEnum.Menu.Error;
            }
            return subMenu;
        }

        public static string ValidateSubMenuBrilla_y_seguros(string text)
        {
            string subMenu;
            var question = text.ToLower(CultureInfo.CurrentCulture);
            if (question.Equals("1") || question.Contains("¿que es brilla?", StringComparison.OrdinalIgnoreCase)
                                || question.Contains("que es brilla", StringComparison.OrdinalIgnoreCase)
                                || question.Contains("¿Qué es Brilla? ", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Que_es_Brilla;
            }
            else if (question.Equals("2") || question.Contains("portafolio brilla", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Portafolio_Brilla;
            }
            else if (question.Equals("3") || question.Contains("Cupo de brilla", StringComparison.OrdinalIgnoreCase)
                || question.Contains("cupo de brilla", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Cupo_de_brilla;
            }
            else if (question.Equals("4") || question.Contains("utiliza tu cupo", StringComparison.OrdinalIgnoreCase)
                || question.Contains("utiliza cupo", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Utiliza_tu_cupo;
            }
            else if (question.Equals("5") || question.Contains("adquiere tu seguro", StringComparison.OrdinalIgnoreCase)
               || question.Contains("adquiere seguro", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Adquiere_tu_seguro;
            }
            else if (question.Equals("6") || question.Contains("aliados brilla", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Aliados_Brilla;
            }
            else if (question.Equals("7") || question.Contains("une tu cupo brilla", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Une_tu_cupo_Brilla;
            }
            else if (question.Equals("8") || question.Contains("bloqueo brilla y seguros", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Bloqueo_Brilla_y_seguros;
            }
            else if (question.Equals("9") || question.Contains("haz efectiva tu poliza", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Haz_efectiva_tu_poliza;
            }
            else if (question.Equals("10") || question.Contains("linea aseguradora", StringComparison.OrdinalIgnoreCase)
                || question.Contains("aseguradora", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Linea_aseguradora;
            }
            else if (question.Equals("11") || question.Contains("garantias brilla", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Garantias_Brilla;
            }
            else if (question.Equals("12") || question.Contains("cancelacion de seguro", StringComparison.OrdinalIgnoreCase)
                || question.Contains("cancelacion seguro", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Cancelacion_de_seguro;
            }
            else if (question.Equals("salir"))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Salir;
            }
            else
            {
                subMenu = MenuSubMenuEnum.Menu.Error;
            }
            return subMenu;
        }

        public static string ValidateSubMenuInformacion_general(string text)
        {
            string subMenu;
            var question = text.ToLower(CultureInfo.CurrentCulture);
            if (question.Equals("1") || question.Contains("informacion de la empresa", StringComparison.OrdinalIgnoreCase)
                                || question.Contains("informacion empresa", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Informacion_de_la_empresa;
            }
            else if (question.Equals("2") || question.Contains("contrato de condiciones uniformes", StringComparison.OrdinalIgnoreCase)
                || question.Contains("contrato condiciones uniformes", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Contrato_de_condiciones_uniformes;
            }
            else if (question.Equals("3") || question.Contains("tratamiento de datos", StringComparison.OrdinalIgnoreCase)
                || question.Contains("tratamiento de datos", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Tratamiento_de_datos;
            }
            else if (question.Equals("4") || question.Contains("oficinas y horarios de atencion", StringComparison.OrdinalIgnoreCase)
                || question.Contains("oficinas horarios de atencion", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Oficinas_y_horarios_de_atencion;
            }
            else if (question.Equals("5") || question.Contains("tiempos de atencion pqrs", StringComparison.OrdinalIgnoreCase)
                || question.Contains("tiempos de atencion pqr", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Tiempos_de_atencion_PQRS;
            }
            else if (question.Equals("6") || question.Contains("estado de solicitud pqrs", StringComparison.OrdinalIgnoreCase)
                || question.Contains("estado de solicitud pqr", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Estado_de_solicitud_PQRS;
            }
            else if (question.Equals("7") || question.Contains("quejas y reclamos", StringComparison.OrdinalIgnoreCase)
                || question.Contains("quejas reclamos", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Quejas_y_reclamos;
            }
            else if (question.Equals("8") || question.Contains("cambio de nombre", StringComparison.OrdinalIgnoreCase)
                || question.Contains("cambio nombre", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Cambio_de_nombre;
            }
            else if (question.Equals("9") || question.Contains("cambio de estrato", StringComparison.OrdinalIgnoreCase)
                || question.Contains("cambio estrato", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Cambio_de_estrato;
            }
            else if (question.Equals("10") || question.Contains("cambio capacidad contratada", StringComparison.OrdinalIgnoreCase)
                || question.Contains("cambio de capacidad contratada", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Cambio_capacidad_contratada;
            }
            else if (question.Equals("11") || question.Contains("cambio de uso", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Cambio_de_uso;
            }
            else if (question.Equals("12") || question.Contains("terminacion de contrato", StringComparison.OrdinalIgnoreCase)
                || question.Contains("terminacion contrato", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Terminacion_de_contrato;
            }
            else if (question.Equals("13") || question.Contains("identidad de funcionario", StringComparison.OrdinalIgnoreCase)
                || question.Contains("identidad de funcionario", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Identidad_de_funcionario;
            }
            else if (question.Equals("14") || question.Contains("agendamiento", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Agendamiento;
            }
            else if (question.Equals("salir"))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Salir;
            }
            else
            {
                subMenu = MenuSubMenuEnum.Menu.Error;
            }
            return subMenu;
        }

        public static string ValidateSubMenuInformacion_generalQuavii(string text)
        {
            string subMenu;
            var question = text.ToLower(CultureInfo.CurrentCulture);
            if (question.Equals("1") || question.Contains("informacion de la empresa", StringComparison.OrdinalIgnoreCase)
                                || question.Contains("informacion empresa", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Informacion_de_la_empresa;
            }
            else if (question.Equals("2") || question.Contains("contrato de condiciones uniformes", StringComparison.OrdinalIgnoreCase)
                || question.Contains("contrato condiciones uniformes", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Contrato_de_condiciones_uniformes;
            }
            else if (question.Equals("3") || question.Contains("tratamiento de datos", StringComparison.OrdinalIgnoreCase)
                || question.Contains("tratamiento de datos", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Tratamiento_de_datos;
            }
            else if (question.Equals("4") || question.Contains("oficinas y horarios de atencion", StringComparison.OrdinalIgnoreCase)
                || question.Contains("oficinas horarios de atencion", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Oficinas_y_horarios_de_atencion;
            }
            else if (question.Equals("5") || question.Contains("tiempos de atencion pqrs", StringComparison.OrdinalIgnoreCase)
                || question.Contains("tiempos de atencion pqr", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Tiempos_de_atencion_PQRS;
            }
            else if (question.Equals("6") || question.Contains("estado de solicitud pqrs", StringComparison.OrdinalIgnoreCase)
                || question.Contains("estado de solicitud pqr", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Estado_de_solicitud_PQRS;
            }
            else if (question.Equals("7") || question.Contains("quejas y reclamos", StringComparison.OrdinalIgnoreCase)
                || question.Contains("quejas reclamos", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Quejas_y_reclamos;
            }
            else if (question.Equals("8") || question.Contains("cambio de nombre", StringComparison.OrdinalIgnoreCase)
                || question.Contains("cambio nombre", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Cambio_de_nombre;
            }
            else if (question.Equals("9") || question.Contains("cambio de estrato", StringComparison.OrdinalIgnoreCase)
                || question.Contains("cambio estrato", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Cambio_de_estrato;
            }
            else if (question.Equals("10") || question.Contains("cambio capacidad contratada", StringComparison.OrdinalIgnoreCase)
                || question.Contains("cambio de capacidad contratada", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Cambio_capacidad_contratada;
            }
            else if (question.Equals("11") || question.Contains("cambio de uso", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Cambio_de_uso;
            }
            else if (question.Equals("12") || question.Contains("terminacion de contrato", StringComparison.OrdinalIgnoreCase)
                || question.Contains("terminacion contrato", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Terminacion_de_contrato;
            }
            else if (question.Equals("13") || question.Contains("identidad de funcionario", StringComparison.OrdinalIgnoreCase)
                || question.Contains("identidad de funcionario", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Identidad_de_funcionario;
            }
            else if (question.Equals("14") || question.Contains("agendamiento", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Agendamiento;
            }
            else if (question.Equals("salir"))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Salir;
            }
            else
            {
                subMenu = MenuSubMenuEnum.Menu.Error;
            }
            return subMenu;
        }

        public static string ValidateSubMenuRevision_periodica(string text)
        {
            string subMenu;
            var question = text.ToLower(CultureInfo.CurrentCulture);
            if (question.Equals("1") || question.Contains("solicite su revision periodica", StringComparison.OrdinalIgnoreCase) || question.Contains("revision periodica", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Solicite_su_revision_periodica;
            }
            else if (question.Equals("2") || question.Contains("fecha proxima revision periodica", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Fecha_proxima_revision_periodica;
            }
            else if (question.Equals("salir"))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Salir;
            }
            else
            {
                subMenu = MenuSubMenuEnum.Menu.Error;
            }
            return subMenu;
        }

        public static string ValidateSubMenuServicios_tecnicos(string text)
        {
            string subMenu;
            var question = text.ToLower(CultureInfo.CurrentCulture);
            if (question.Equals("1") || question.Contains("Solicitud servicio tecnico", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Solicitud_servicio_tecnico;
            }
            else if (question.Equals("2") || question.Contains("Tiempo de atencion", StringComparison.OrdinalIgnoreCase)
                || question.Contains("tiempo atencion", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Tiempo_de_atencion;
            }
            else if (question.Equals("salir"))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Salir;
            }
            else
            {
                subMenu = MenuSubMenuEnum.Menu.Error;
            }
            return subMenu;
        }

        public static string ValidateSubMenuEmergencias(string text)
        {
            string subMenu;
            var question = text.ToLower(CultureInfo.CurrentCulture);
            if (question.Equals("2") || question.Contains("incendio en el hogar", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Incendio_en_el_hogar;
            }
            else if (question.Equals("3") || question.Contains("explosion en el hogar", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Explosion_en_el_hogar;
            }
            else if (question.Equals("4") || question.Contains("ausencia de gas en el hogar", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Ausencia_de_gas_en_el_hogar;
            }
            else if (question.Equals("5") || question.Contains("daño en la red", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Daño_en_la_red;
            }
            else if (question.Equals("6") || question.Contains("emergencia", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Emergencia;
            }
            else if (question.Equals("salir"))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Salir;
            }
            else
            {
                subMenu = MenuSubMenuEnum.Menu.Error;
            }
            return subMenu;
        }
        public static string ValidateSubMenuGestion_deuda(string text)
        {
            string subMenu;
            var question = text.ToLower(CultureInfo.CurrentCulture);
            if (question.Equals("1") || question.Contains("acuerdo de pago", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Acuerdo_de_Pago;
            }
            else if (question.Equals("2") || question.Contains("traslado de deuda o desmonte", StringComparison.OrdinalIgnoreCase)
                || question.Contains("traslado de deuda", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Traslado_de_deuda_o_desmonte;
            }
            else if (question.Equals("3") || question.Contains("devolucion saldo a favor", StringComparison.OrdinalIgnoreCase)
                || question.Contains("devolucion saldo", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Devolucion_saldo_a_favor;

            }
            else if (question.Equals("salir"))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Salir;
            }
            else
            {
                subMenu = MenuSubMenuEnum.Menu.Error;
            }
            return subMenu;

        }

        public static string ValidateSubMenuEmergenciasQuavii(string text)
        {
            string subMenu;
            var question = text.ToLower(CultureInfo.CurrentCulture);
            if (question.Equals("1") || question.Contains("fuga dentro del hogar", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Fuga_dentro_del_hogar;
            }
            else if (question.Equals("2") || question.Contains("incendio en el hogar", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Incendio_en_el_hogar;
            }
            else if (question.Equals("3") || question.Contains("explosion en el hogar", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Explosion_en_el_hogar;
            }
            else if (question.Equals("4") || question.Contains("ausencia de gas en el hogar", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Ausencia_de_gas_en_el_hogar;
            }
            else if (question.Equals("5") || question.Contains("daño en la red", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Daño_en_la_red;
            }
            else if (question.Equals("6") || question.Contains("emergencia", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Emergencia;
            }
            else if (question.Equals("salir"))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Salir;
            }
            else
            {
                subMenu = MenuSubMenuEnum.Menu.Error;
            }
            return subMenu;
        }

        public static string ValidateSubMenuConectate_al_servicio(string text)
        {
            string subMenu;
            var question = text.ToLower(CultureInfo.CurrentCulture);
            if (question.Equals("1") || question.Contains("solicitud de servicio", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Solicitud_de_servicio;
            }
            else if (question.Equals("2") || question.Contains("factibilidad del servicio", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Factibilidad_del_servicio;
            }
            else if (question.Equals("3") || question.Contains("Viabilidad/ disponibilidad", StringComparison.OrdinalIgnoreCase)
                 || question.Contains("Viabilidad", StringComparison.OrdinalIgnoreCase))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Viabilidad_disponibilidad;
            }
            else if (question.Equals("salir"))
            {
                subMenu = MenuSubMenuEnum.SubMenu.Salir;
            }
            else
            {
                subMenu = MenuSubMenuEnum.Menu.Error;
            }
            return subMenu;
        }
    }
}
