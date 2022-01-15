
namespace EnlaceVA.Utilities
{
    public static class MenuSubMenuEnum
    {
        public class Menu
        {
            public const string Tu_Factura = "ReturningTu_Factura";
            public const string Tu_Recibo = "ReturningTu_Recibo";
            public const string Brilla_y_seguros = "ReturningBrilla_Seguro";
            public const string Informacion_general = "ReturningInformacion_general";
            public const string Informacion_generalQuavii = "ReturningInformacion_generalQuavii";
            public const string Revision_periodica = "ReturningRevision_periodica";
            public const string Conectate_al_servicio = "ReturningConectate";
            public const string Pagos = "ReturningPagos";
            public const string PagosQuavii = "ReturningPagosQuavii";
            public const string Gestion_deuda = "ReturningGestion_Deuda";
            public const string Emergencias = "EmergenciaMessage";
            //public const string Emergencias = "ReturningEmergencias";
            public const string Servicios_tecnicos = "ReturningServicios_tecnicos";
            public const string EmergenciasQuavii = "ReturningEmergenciasQuavii";
            public const string Error = "Error";
            public const string Salir = "Salir";
            protected Menu()
            {
            }
        };

        public class Topic
        {
            public const string Tu_Factura = "Tu_Factura";
            public const string Tu_Recibo = "Tu_Recibo";
            public const string Brilla_y_seguros = "Brilla_y_seguros";
            public const string Informacion_general = "Informacion_general";
            public const string Informacion_generalQuavii = "Informacion_generalQuavii";
            public const string Revision_periodica = "Revision_periodica";
            public const string Conectate_al_servicio = "Conectate_al_servicio";
            public const string Pagos = "Pagos";
            public const string PagosQuavii = "PagosQuavii";
            public const string Gestion_deuda = "Gestion_deuda";
            public const string Emergencias = "Emergencias";
            public const string Servicios_tecnicos = "Servicios_tecnicos";
            public const string EmergenciasQuavii = "EmergenciasQuavii";           
            protected Topic()
            {
            }
        };

        public enum QuestionType
        {
            Informativas,
            Transaccionales,
            Chitchat,
            None
        };

        public enum userState
        {
            none,
            active,
            inactive
        };

        public class MenuWhatsapp
        {
            public const string Tu_Factura = "WhatsappTu_Factura";
            public const string Tu_Recibo = "WhatsappTu_Recibo";
            public const string Brilla_y_seguros = "WhatsappBrilla_Seguro";
            public const string Informacion_general = "WhatsappInformacion_general";
            public const string Informacion_generalQuavii = "WhatsappInformacion_generalQuavii";
            public const string Revision_periodica = "WhatsappRevision_periodica";
            public const string Conectate_al_servicio = "WhatsappConectate";
            public const string Pagos = "WhatsappPagos";
            public const string PagosQuavii = "WhatsappPagosQuavii";
            public const string Gestion_deuda = "WhatsappGestion_Deuda";
            public const string Emergencias = "WhatsappEmergencias";
            public const string Servicios_tecnicos = "WhatsappServicios_tecnicos";
            public const string EmergenciasQuavii = "WhatsappEmergenciasQuavii";
            protected MenuWhatsapp()
            {
            }
        };

        public class SubMenu
        {
            public const string Fuga_dentro_del_hogar = "Fuga dentro del hogar";
            public const string Incendio_en_el_hogar = "Incendio en el hogar";
            public const string Explosion_en_el_hogar = "Explosion en el hogar";
            public const string Ausencia_de_gas_en_el_hogar = "Ausencia de gas en el hogar";
            public const string Daño_en_la_red = "Daño en la red";
            public const string Solicitud_de_servicio = "Solicitud de servicio";
            public const string Factibilidad_del_servicio = "Factibilidad del servicio";
            public const string Viabilidad_disponibilidad = "Viabilidad";
            public const string Que_es_Brilla = "Que es Brilla";
            public const string Cupo_de_brilla = "como puedo consultar mi cupo brilla";
            public const string Portafolio_Brilla = "Portafolio de productos y servicios";
            public const string Utiliza_tu_cupo = "visita de financiacion no bancaria";
            public const string Adquiere_tu_seguro = "Adquiere tu seguro";
            public const string Aliados_Brilla = "Listado de aliados brilla";
            public const string Une_tu_cupo_Brilla = "Une tu cupo Brilla";
            public const string Bloqueo_Brilla_y_seguros = "Bloqueo Brilla y seguros";
            public const string Haz_efectiva_tu_poliza = "Haz efectiva tu poliza";
            public const string Linea_aseguradora = "Linea aseguradora";
            public const string Garantias_Brilla = "Garantía de productos Brilla";
            public const string Cancelacion_de_seguro = "Cancelacion de seguro";
            public const string Consumo_facturado = "Consumo facturado";
            public const string Ultima_lectura = "Ultima lectura";
            public const string Consulta_y_generacion_de_factura = "Consulta y generacion de factura";
            public const string Valor_fecha_limite_y_referencia_pago = "Valor_fecha_limite_y_referencia_pago";
            public const string Deuda_actual_y_diferida = "Deuda actual y diferida";
            public const string Certificado_de_tus_facturas = "Certificado de tus facturas";
            public const string Certificado_deuda_diferida = "Certificado deuda actual y diferida";
            public const string Explicacion_de_la_factura = "Explicacion de la factura";
            public const string Informacion_tarifas = "Informacion tarifas";
            public const string Exencion_de_contribucion = "Exencion de contribucion";
            public const string Abona_a_tu_factura = "Abona a tu factura";
            public const string Solicita_tu_cupon_de_pago_anticipado = "Solicita tu cupon de pago anticipado";
            public const string Traslado_de_pago = "Traslado de pago";
            public const string Constancia_de_pago = "Constancia de pago";
            public const string Reconexion_del_servicio = "Reconexion del servicio";
            public const string Estado_reconexión_por_pago = "cuando me realizan la reconexion del servicio";
            public const string Puntos_de_pagos = "Puntos de pagos";
            public const string Pago_con_tarjeta_credito = "Pago con tarjeta credito";
            public const string Solicite_su_revision_periodica = "Solicite su revision periodica";
            public const string Fecha_proxima_revision_periodica = "Fecha proxima revision periodica";
            public const string Acuerdo_de_Pago = "Acuerdo de Pago";
            public const string Traslado_de_deuda_o_desmonte = "Traslado de deuda o desmonte";
            public const string Devolucion_saldo_a_favor = "Devolucion saldo a favor";
            public const string Solicitud_servicio_tecnico = "Solicitud servicio tecnico";
            public const string Tiempo_de_atencion = "Tiempo de atencion";
            public const string Informacion_de_la_empresa = "Informacion de la empresa";
            public const string Contrato_de_condiciones_uniformes = "Contrato de condiciones uniformes";
            public const string Tratamiento_de_datos = "Tratamiento de datos";
            public const string Oficinas_y_horarios_de_atencion = "Oficinas y horarios de atencion";
            public const string Tiempos_de_atencion_PQRS = "Tiempos de atencion PQR'S";
            public const string Estado_de_solicitud_PQRS = "Estado de solicitud PQR'S";
            public const string Quejas_y_reclamos = "Quejas y reclamos";
            public const string Cambio_de_nombre = "Cambio de nombre";
            public const string Cambio_de_estrato = "Cambio de estrato";
            public const string Cambio_capacidad_contratada = "Cambio_capacidad_contratada";
            public const string Cambio_de_uso = "Cambio de uso";
            public const string Terminacion_de_contrato = "Terminacion de contrato";
            public const string Identidad_de_funcionario = "Identidad de funcionario";
            public const string Agendamiento = "Agendamiento";
            public const string Emergencia = "Emergencia";
            public const string Salir = "Salir";
            protected SubMenu()
            {
            }
        };
        public class CompanyName
        {
            public const string Ceo = "ceo"; //Compañía Energética de Occidente
            public const string CeoName = "CEO";
            public const string Gdo = "gdo"; // 
            public const string GdoName = "GdO";
            public const string Stg = "stg"; // Surtigas
            public const string StgName = "Surtigas";
            public const string Quavii = "quavii"; // Quavii **
            public const string QuaviiName = "Quavii";

            protected CompanyName()
            {

            }
        };

    }
}
