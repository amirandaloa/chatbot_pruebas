using EnlaceVA.Models;
using EnlaceVA.Resources;
using EnlaceVA.Utilities;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Resources;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EnlaceVA.Services
{

    public class TransactionalServices
    {
        public SoapClient SoapClientServices { get; set; }
        public string UrlKeyFirst { get; set; }
        public string ServiceNamespace { get; set; }

        public MTConceptoFactura GetConceptoFactura (string company, DatosEntrada conceptoFacturaEntrada)
        {
            UrlKeyFirst = "ConceptoFactura";
            ServiceNamespace = GetServiceNamespace(company);
            string urlKey = UrlKeyFirst + company.ToUpper(CultureInfo.CurrentCulture);
            ResourceManager resourceManager = new ResourceManager(typeof(UrlsCompanies));
            string url = resourceManager.GetString(urlKey);
            SoapClientServices = new SoapClient(url, ServiceNamespace);
            MTConceptoFactura mT_ConceptoFactura_Res;

            try
            {
                HttpResponseMessage response = SoapClientServices.PostAsync("MT_ConceptoFactura_req", "MT_ConceptoFactura_res",
                    conceptoFacturaEntrada).Result;
                SoapError error ;
                string xmlResult;
                if (response.Content != null)
                {
                    xmlResult = response.Content.ReadAsStringAsync().Result;
                    error = GetError(xmlResult);
                }
                else 
                {
                    xmlResult = "";
                    error = new SoapError()
                    {
                        Message = "Ha ocurrido un error de conexion " + response.StatusCode
                    };
                }

                

                if (response.IsSuccessStatusCode)
                {
                    object conceptoFactura = GetData<object>(xmlResult, "MT_ConceptoFactura_res", ServiceNamespace);
                    mT_ConceptoFactura_Res = new MTConceptoFactura(conceptoFactura, company);
                }
                else
                {
                    mT_ConceptoFactura_Res = new MTConceptoFactura()
                    {
                        mensajes = new Mensaje()
                        {
                            CodMsj = response.StatusCode.ToString(),
                            descMsj = error?.Message
                        }
                    };
                }
                
            }
            catch (HttpRequestException exception)
            {
                Mensaje mensaje = new Mensaje
                {
                    CodMsj = "",
                    descMsj = exception.Message
                };

                mT_ConceptoFactura_Res = new MTConceptoFactura()
                {
                    mensajes = mensaje
                }; 
            }
            return mT_ConceptoFactura_Res;
        }

        public MTEstadoCuenta GetEstadoDeuda(string company, DatosEntrada estadoDeudaEntrada)
        {
            UrlKeyFirst = "EstadoCuenta";
            ServiceNamespace = GetServiceNamespace(company);
            string urlKey = UrlKeyFirst + company.ToUpper(CultureInfo.CurrentCulture);
            ResourceManager resourceManager = new ResourceManager(typeof(UrlsCompanies));
            string url = resourceManager.GetString(urlKey);
            SoapClientServices = new SoapClient(url, ServiceNamespace);
            MTEstadoCuenta mT_EstadoCuenta_res;
            try
            {   //ya esta revisado
                HttpResponseMessage response = SoapClientServices.PostAsync("MT_EstadoCuenta_req", "MT_EstadoCuenta_res",
                    estadoDeudaEntrada).Result;

                SoapError error;
                string xmlResult;

                if (response.Content != null)
                {
                    xmlResult = response.Content.ReadAsStringAsync().Result;
                    error = GetError(xmlResult);
                }
                else
                {
                    xmlResult = "";
                    error = new SoapError()
                    {
                        Message = "Ha ocurrido un error de conexion " + response.StatusCode
                    };
                }

                if (response.IsSuccessStatusCode)
                {
                    object estadoDeuda = GetData<object>(xmlResult, "MT_EstadoCuenta_res", ServiceNamespace);
                    mT_EstadoCuenta_res = new MTEstadoCuenta(estadoDeuda, company);
                }
                else 
                {
                    mT_EstadoCuenta_res = new MTEstadoCuenta()
                    {
                        mensajes = new Mensaje()
                        {
                            CodMsj = response.StatusCode.ToString(),
                            descMsj = error?.Message
                        }
                    };
                }
            }
            catch (HttpRequestException exception)
            {
                Mensaje mensaje = new Mensaje
                {
                    CodMsj = "",
                    descMsj = exception.Message
                };

                mT_EstadoCuenta_res = new MTEstadoCuenta()
                {
                    mensajes = mensaje
                };
            }
            return mT_EstadoCuenta_res;
        }
        public MTTomaLectura GetTomalectura(string company, DatosEntrada estadoDeudaEntrada)
        {
            UrlKeyFirst = "TomaLectura";
            
            string urlKey = UrlKeyFirst + company.ToUpper(CultureInfo.CurrentCulture);
            ResourceManager resourceManager = new ResourceManager(typeof(UrlsCompanies));
            string url = resourceManager.GetString(urlKey);


            // Se agrega este switch ya que la urn no es generica para todos los clientes
            // Nota: evaluar si se llama desde las urlCompanies y se llama o se deja así AML 2021-01-05
            switch (company)
            {
                case "ceo": 
                    ServiceNamespace = "urn:gaseras.ceoc.com:srvemp:chatbot";
                    break;

                case "quavii":
                    ServiceNamespace = "urn:gaseras.gdp.com:srvemp:chatbot";
                    break;

                case "stg":
                    ServiceNamespace = "urn:gaseras.stg.com:srvemp:chatbot";
                    break;

                default:
                    ServiceNamespace = GetServiceNamespace(company);
                    break;
            }

            SoapClientServices = new SoapClient(url, ServiceNamespace);
            MTTomaLectura MTTomaLectura_res;
            try
            {
                HttpResponseMessage response = SoapClientServices.PostAsync("MT_TomaLectura_req", "MT_TomaLectura_res",
                    estadoDeudaEntrada).Result;

                SoapError error;
                string xmlResult;

                if (response.Content != null)
                {
                    xmlResult = response.Content.ReadAsStringAsync().Result;
                    error = GetError(xmlResult);
                }
                else
                {
                    xmlResult = "";
                    error = new SoapError()
                    {
                        Message = "Ha ocurrido un error de conexion " + response.StatusCode
                    };
                }

                if (response.IsSuccessStatusCode)
                {
                    object tomaLectura = GetData<object>(xmlResult, "MT_TomaLectura_res", ServiceNamespace);
                    MTTomaLectura_res = new MTTomaLectura(tomaLectura,company);
                }
                else
                {
                    MTTomaLectura_res = new MTTomaLectura()
                    {
                        mensajes = new Mensaje()
                        {
                            CodMsj = response.StatusCode.ToString(),
                            descMsj = error?.Message
                        }
                    };
                }
            }
            catch (HttpRequestException exception)
            {
                Mensaje mensaje = new Mensaje
                {
                    CodMsj = "",
                    descMsj = exception.Message
                };

                MTTomaLectura_res = new MTTomaLectura()
                {
                    mensajes = mensaje
                };
            }
            return MTTomaLectura_res;
        }
        public MTConsultaEstado GetConsultaPQR(string company, DatosEntradaSolicitud estadoDeudaEntrada)
        {
            UrlKeyFirst = "ConsultaPQR";
            ServiceNamespace = GetServiceNamespace(company);
            string urlKey = UrlKeyFirst + company.ToUpper(CultureInfo.CurrentCulture);
            ResourceManager resourceManager = new ResourceManager(typeof(UrlsCompanies));
            string url = resourceManager.GetString(urlKey);
            SoapClientServices = new SoapClient(url, ServiceNamespace);
            MTConsultaEstado MTConsultaEstado_res;
            try
            {
                //dynamic vaTMP = new
                //{
                //    solicitud = string.IsNullOrEmpty(estadoDeudaEntrada.solicitud) ? "" : estadoDeudaEntrada.solicitud,
                //    tipoIdentificacion = string.IsNullOrEmpty(estadoDeudaEntrada.tipoIdentificacion) ? "" : estadoDeudaEntrada.tipoIdentificacion,
                //    identificacion = string.IsNullOrEmpty(estadoDeudaEntrada.identificacion) ? "" : estadoDeudaEntrada.identificacion,
                //    cliente = string.IsNullOrEmpty(estadoDeudaEntrada.cliente) ? "" : estadoDeudaEntrada.cliente,
                //    correo = string.IsNullOrEmpty(estadoDeudaEntrada.correo) ? "" : estadoDeudaEntrada.correo,
                //    telefono = string.IsNullOrEmpty(estadoDeudaEntrada.telefono) ? "" : estadoDeudaEntrada.telefono
                //};

                HttpResponseMessage response = SoapClientServices.PostAsync("MT_ConsultaEstado_req", "MT_ConsultaEstado_res",
                    estadoDeudaEntrada).Result;

                SoapError error;
                string xmlResult;

                if (response.Content != null)
                {
                    xmlResult = response.Content.ReadAsStringAsync().Result;
                    error = GetError(xmlResult);
                }
                else
                {
                    xmlResult = "";
                    error = new SoapError()
                    {
                        Message = "Ha ocurrido un error de conexion " + response.StatusCode
                    };
                }

                if (response.IsSuccessStatusCode)
                {
                    object tomaLectura = GetData<object>(xmlResult, "MT_ConsultaEstado_res", ServiceNamespace);
                    MTConsultaEstado_res = new MTConsultaEstado(tomaLectura);
                }
                else
                {
                    MTConsultaEstado_res = new MTConsultaEstado()
                    {
                        codError = error?.Message,
                    };
                }
            }
            catch (HttpRequestException exception)
            {

                MTConsultaEstado_res = new MTConsultaEstado()
                {
                    codError = exception.Message
                };
            }
            return MTConsultaEstado_res;
        }

        public MTConsumoFacturado GetConsumoFacturado(string company, DatosEntrada consumoFacturadoEntr)
        {
            UrlKeyFirst = "ConsumoFacturado";
            ServiceNamespace = GetServiceNamespace(company);
            string urlKey = UrlKeyFirst + company.ToUpper(CultureInfo.CurrentCulture);
            ResourceManager resourceManager = new ResourceManager(typeof(UrlsCompanies));
            string url = resourceManager.GetString(urlKey);
            SoapClientServices = new SoapClient(url, ServiceNamespace);
            MTConsumoFacturado MTConsumoFacturado_res;
            try
            {   //YA ESTA REVISADO
                HttpResponseMessage response = SoapClientServices.PostAsync("MT_ConsumoFacturado_req", "MT_ConsumoFacturado_res",
                    consumoFacturadoEntr).Result;

                SoapError error;
                string xmlResult;

                if (response.Content != null)
                {
                    xmlResult = response.Content.ReadAsStringAsync().Result;
                    error = GetError(xmlResult);
                }
                else
                {
                    xmlResult = "";
                    error = new SoapError()
                    {
                        Message = "Ha ocurrido un error de conexion " + response.StatusCode
                    };
                }

                if (response.IsSuccessStatusCode)
                {
                    //Cambiar urn
                    object consumoFacturado = GetData<object>(xmlResult, "MT_ConsumoFacturado_res", ServiceNamespace);
                    MTConsumoFacturado_res = new MTConsumoFacturado(consumoFacturado);
                }
                else
                {
                    MTConsumoFacturado_res = new MTConsumoFacturado()
                    {
                        mensajes = new Mensaje()
                        {
                            CodMsj = response.StatusCode.ToString(),
                            descMsj = error?.Message
                        }
                    };
                }
            }
            catch (HttpRequestException exception)
            {

                MTConsumoFacturado_res = new MTConsumoFacturado()
                {
                    mensajes = new Mensaje()
                    {
                        CodMsj = exception.Message
                    }
                };
            }
            return MTConsumoFacturado_res;
        }

        public MTPagoParcial GetPagoParcial(string company, DatosEntradaPagoParcial pagoParcialEntr)
        {
            UrlKeyFirst = "CuponPagoParcial";
            ServiceNamespace = GetServiceNamespace(company);
            string urlKey = UrlKeyFirst + company.ToUpper(CultureInfo.CurrentCulture);
            ResourceManager resourceManager = new ResourceManager(typeof(UrlsCompanies));
            string url = resourceManager.GetString(urlKey);
            SoapClientServices = new SoapClient(url, ServiceNamespace);
            MTPagoParcial MTPagoParcial_res;
            try
            {
                HttpResponseMessage response = SoapClientServices.PostAsync("MT_CuponPagoParcial_req", "MT_CuponPagoParcial_res",
                    pagoParcialEntr).Result;

                SoapError error;
                string xmlResult;

                if (response.Content != null)
                {
                    xmlResult = response.Content.ReadAsStringAsync().Result;
                    error = GetError(xmlResult);
                }
                else
                {
                    xmlResult = "";
                    error = new SoapError()
                    {
                        Message = "Ha ocurrido un error de conexion " + response.StatusCode
                    };
                }

                if (response.IsSuccessStatusCode)
                {
                    object pagoParcial = GetData<object>(xmlResult, "MT_CuponPagoParcial_res", ServiceNamespace);
                    MTPagoParcial_res = new MTPagoParcial(pagoParcial,company);
                }
                else
                {
                    MTPagoParcial_res = new MTPagoParcial()
                    {
                        mensajes = new Mensaje()
                        {
                            CodMsj = response.StatusCode.ToString()
                        }
                    };
                }
            }
            catch (HttpRequestException exception)
            {

                MTPagoParcial_res = new MTPagoParcial()
                {
                    mensajes = new Mensaje()
                    {
                        CodMsj = exception.Message
                    }
                };
            }
            return MTPagoParcial_res;
        }

        public MTVentaSeguros GetVentaSeguros(string company, DatosEntradaVentaSeguros ventaSegurosEntr)
        {
            UrlKeyFirst = "VentaSeguros";
            ServiceNamespace = GetServiceNamespace(company);
            string urlKey = UrlKeyFirst + company.ToUpper(CultureInfo.CurrentCulture);
            ResourceManager resourceManager = new ResourceManager(typeof(UrlsCompanies));
            string url = resourceManager.GetString(urlKey);
            SoapClientServices = new SoapClient(url, ServiceNamespace);
            MTVentaSeguros MTVentaSeguros_res;
            try
            {   
                // YA ESTA REVISADO
                HttpResponseMessage response = SoapClientServices.PostAsync("MT_VentaSeguros_req", "MT_VentaSeguros_res",
                    ventaSegurosEntr).Result;

                SoapError error;
                string xmlResult;

                if (response.Content != null)
                {
                    xmlResult = response.Content.ReadAsStringAsync().Result;
                    error = GetError(xmlResult);
                }
                else
                {
                    xmlResult = "";
                    error = new SoapError()
                    {
                        Message = "Ha ocurrido un error de conexion " + response.StatusCode
                    };
                }

                if (response.IsSuccessStatusCode)
                {
                    object ventaSeguros = GetData<object>(xmlResult, "MT_VentaSeguros_res", ServiceNamespace);
                    MTVentaSeguros_res = new MTVentaSeguros(ventaSeguros);
                }
                else
                {
                    MTVentaSeguros_res = new MTVentaSeguros()
                    {
                        mensajes = new Mensaje()
                        {
                            CodMsj = response.StatusCode.ToString()
                        }                        
                    };
                }
            }
            catch (HttpRequestException exception)
            {

                MTVentaSeguros_res = new MTVentaSeguros()
                {
                    mensajes = new Mensaje()
                    {
                        CodMsj = exception.Message
                    }
                };
            }
            return MTVentaSeguros_res;
        }

        public MTValidarContrato MTValidarcontrato (string company, DatosEntradaValidarContrato validarContratoEntr)
        {
            //Prioridad listo ok
            UrlKeyFirst = "ValidarContrato";
            ResourceManager resourceManager = new ResourceManager(typeof(UrlsCompanies));
            ServiceNamespace = resourceManager.GetString("UrnGeneral");
            string urlKey = UrlKeyFirst + company.ToUpper(CultureInfo.CurrentCulture);            
            string url = resourceManager.GetString(urlKey);
            SoapClientServices = new SoapClient(url, ServiceNamespace);
            MTValidarContrato mT_ValidarContrato_Res;
           
            try
            {
                HttpResponseMessage response = SoapClientServices.PostAsync("MT_ValidarContrato_req", "MT_ValidarContrato_res",
                    validarContratoEntr).Result;
                SoapError error;
                string xmlResult;
                if (response.Content != null)
                {
                    xmlResult = response.Content.ReadAsStringAsync().Result;
                    error = GetError(xmlResult);
                }
                else
                {
                    xmlResult = "";
                    error = new SoapError()
                    {
                        Message = "Ha ocurrido un error de conexion " + response.StatusCode
                    };
                }


                if (response.IsSuccessStatusCode)
                {
                    object ValidarContrato = GetData<object>(xmlResult, "MT_ValidarContrato_res", ServiceNamespace);
                    mT_ValidarContrato_Res = new MTValidarContrato(ValidarContrato);
                }
                else
                {
                    mT_ValidarContrato_Res = new MTValidarContrato()
                    {
                        mensajes = new Mensaje()
                        {
                            CodMsj = response.StatusCode.ToString(),
                            descMsj = error?.Message
                        }
                    };
                }

            }
            catch (HttpRequestException exception)
            {
                mT_ValidarContrato_Res = new MTValidarContrato()
                {
                    mensajes = new Mensaje()
                    {
                        CodMsj = exception.Message
                    }
                };
            }

            return mT_ValidarContrato_Res;
        }

        public MTValidarCliente MTValidarcliente(string company, DatosEntradaValidarCliente validarClienteEntr)
        {
            //Prioridad
            UrlKeyFirst = "ValidarCliente";
            ResourceManager resourceManager = new ResourceManager(typeof(UrlsCompanies));


            ServiceNamespace = resourceManager.GetString("UrnGeneral");
       

            string urlKey = UrlKeyFirst + company.ToUpper(CultureInfo.CurrentCulture);
            string url = resourceManager.GetString(urlKey);
            SoapClientServices = new SoapClient(url, ServiceNamespace);
            MTValidarCliente mT_ValidarCliente_Res;

            try
            {
                //    YA ESTA REVISADO x    
                // http://10.48.50.13:50000/XISOAPAdapter/MessageServlet?senderParty=&senderService=OSMFLEX_GDO&receiverParty=&receiverService=&interface=ValidarCliente_Out&interfaceNamespace=urn:gaseras.com:srvemp:bss:chatbot
               
                HttpResponseMessage response = SoapClientServices.PostAsync("MT_ValidarCliente_req", "MT_ValidarCliente_res",
                    validarClienteEntr).Result;
                SoapError error;
                string xmlResult;
                if (response.Content != null)
                {
                    xmlResult = response.Content.ReadAsStringAsync().Result;
                    error = GetError(xmlResult);
                }
                else
                {
                    xmlResult = "";
                    error = new SoapError()
                    {
                        Message = "Ha ocurrido un error de conexion " + response.StatusCode
                    };
                }


                if (response.IsSuccessStatusCode)
                {
                    object ValidarCliente = GetData<object>(xmlResult, "MT_ValidarCliente_res", ServiceNamespace);
                    mT_ValidarCliente_Res = new MTValidarCliente(ValidarCliente);
                }
                else
                {
                    mT_ValidarCliente_Res = new MTValidarCliente()
                    {
                        mensajes = new Mensaje()
                        {
                            CodMsj = response.StatusCode.ToString(),
                            descMsj = error?.Message
                        }
                    };
                }

            }
            catch (HttpRequestException exception)
            {
                mT_ValidarCliente_Res = new MTValidarCliente()
                {
                    mensajes = new Mensaje()
                    {
                        CodMsj = exception.Message
                    }
                };
            }

            return mT_ValidarCliente_Res;
        }

        public MTTratamientoDatos MTValidarTratamientoDatos(string company, DatosEntradaTratamientoDatos tratamientoDatosEntr)
        {
            //Prioridad
            UrlKeyFirst = "TratamientoDatos";
            ResourceManager resourceManager = new ResourceManager(typeof(UrlsCompanies));
            ServiceNamespace = resourceManager.GetString("UrnGeneral");
            string urlKey = UrlKeyFirst + company.ToUpper(CultureInfo.CurrentCulture);
            string url = resourceManager.GetString(urlKey);
            SoapClientServices = new SoapClient(url, ServiceNamespace);
            MTTratamientoDatos mT_TratamientoDatos_Res;
            dynamic vaTMP = new {
                comentario = string.IsNullOrEmpty(tratamientoDatosEntr.comentario) ? "ninguno" : tratamientoDatosEntr.comentario,
                tipoIdentificacion = tratamientoDatosEntr.tipoIdentificacion,
                identificacion = tratamientoDatosEntr.identificacion,
                tipoProducto = tratamientoDatosEntr.tipoProducto,
                estadoManejo = tratamientoDatosEntr.estadoManejo,
                cliente = "",
                correo = "",
                telefono ="",
                tipoDispositivo ="",
                ip ="",
                isbMAC=""
            };

            try
            {   //YA ESTA REVISADO
                HttpResponseMessage response = SoapClientServices.PostAsync("MT_TratamientoDatos_req", "MT_TratamientoDatos_res",
                    vaTMP).Result;
                SoapError error;
                string xmlResult;
                if (response.Content != null)
                {
                    xmlResult = response.Content.ReadAsStringAsync().Result;
                    error = GetError(xmlResult);
                }
                else
                {
                    xmlResult = "";
                    error = new SoapError()
                    {
                        Message = "Ha ocurrido un error de conexion " + response.StatusCode
                    };
                }


                if (response.IsSuccessStatusCode)
                {
                    object tratamientoDatos = GetData<object>(xmlResult, "MT_TratamientoDatos_res", ServiceNamespace);
                    mT_TratamientoDatos_Res = new MTTratamientoDatos(tratamientoDatos);
                }
                else
                {
                    mT_TratamientoDatos_Res = new MTTratamientoDatos()
                    {
                        mensajes = new Mensaje()
                        {
                            CodMsj = response.StatusCode.ToString(),
                            descMsj = error?.Message
                        }
                    };
                }

            }
            catch (HttpRequestException exception)
            {
                mT_TratamientoDatos_Res = new MTTratamientoDatos()
                {
                    mensajes = new Mensaje()
                    {
                        CodMsj = exception.Message
                    }
                };
            }

            return mT_TratamientoDatos_Res;
        }

        public MTVisitaFinNoBanca GetVisitaFinNoBanca(string company, DatosEntradaVisitaFinNoBanca visitaFinNoBancaEntr)
        {
            UrlKeyFirst = "VisitaFinNoBanca";
            ServiceNamespace = GetServiceNamespace(company);
            string urlKey = UrlKeyFirst + company.ToUpper(CultureInfo.CurrentCulture);
            ResourceManager resourceManager = new ResourceManager(typeof(UrlsCompanies));
            string url = resourceManager.GetString(urlKey);
            SoapClientServices = new SoapClient(url, ServiceNamespace);
            MTVisitaFinNoBanca MTVisitaFinNoBanca_res;
            try
            {
                HttpResponseMessage response = SoapClientServices.PostAsync("MT_VisitaFinNoBanca_req", "MT_VisitaFinNoBanca_res",
                    visitaFinNoBancaEntr).Result;

                SoapError error;
                string xmlResult;

                if (response.Content != null)
                {
                    xmlResult = response.Content.ReadAsStringAsync().Result;
                    error = GetError(xmlResult);
                }
                else
                {
                    xmlResult = "";
                    error = new SoapError()
                    {
                        Message = "Ha ocurrido un error de conexion " + response.StatusCode
                    };
                }

                if (response.IsSuccessStatusCode)
                {
                    object visitaFinNoBanca = GetData<object>(xmlResult, "MT_VisitaFinNoBanca_res", ServiceNamespace);
                    MTVisitaFinNoBanca_res = new MTVisitaFinNoBanca(visitaFinNoBanca);
                }
                else
                {
                    MTVisitaFinNoBanca_res = new MTVisitaFinNoBanca()
                    {
                        mensajes = new Mensaje()
                        {
                            CodMsj = response.StatusCode.ToString()
                        }
                    };
                }
            }
            catch (HttpRequestException exception)
            {

                MTVisitaFinNoBanca_res = new MTVisitaFinNoBanca()
                {
                    mensajes = new Mensaje()
                    {
                        CodMsj = exception.Message
                    }
                };
            }
            return MTVisitaFinNoBanca_res;
        }

        public MTReconexionPorPago GetReconexionPorPago(string company, DatosEntradaReconexionPorPago ReconexionPorPagoEntr)
        {
            UrlKeyFirst = "ReconexionPorPago";
            ServiceNamespace = GetServiceNamespace(company);
            string urlKey = UrlKeyFirst + company.ToUpper(CultureInfo.CurrentCulture);
            ResourceManager resourceManager = new ResourceManager(typeof(UrlsCompanies));
            if (company == "stg")
            {
                ServiceNamespace = "urn:gaseras.com:srvemp:chatbot";
            }
            string url = resourceManager.GetString(urlKey);
            SoapClientServices = new SoapClient(url, ServiceNamespace);
            MTReconexionPorPago MTReconexionPorPago_res;
            try
            {
                HttpResponseMessage response = SoapClientServices.PostAsync("MT_ReconexionServicioPago_req", "MT_ReconexionServicioPago_res",
                    ReconexionPorPagoEntr).Result;

                SoapError error;
                string xmlResult;

                if (response.Content != null)
                {
                    xmlResult = response.Content.ReadAsStringAsync().Result;
                    error = GetError(xmlResult);
                }
                else
                {
                    xmlResult = "";
                    error = new SoapError()
                    {
                        Message = "Ha ocurrido un error de conexion " + response.StatusCode
                    };
                }

                if (response.IsSuccessStatusCode)
                {
                    object ReconexionPago = GetData<object>(xmlResult, "MT_ReconexionServicioPago_res", ServiceNamespace);
                    MTReconexionPorPago_res = new MTReconexionPorPago(ReconexionPago);
                }
                else
                {
                    MTReconexionPorPago_res = new MTReconexionPorPago()
                    {
                        mensajes = new Mensaje()
                        {
                            CodMsj = response.StatusCode.ToString()
                        }
                    };
                }
            }
            catch (HttpRequestException exception)
            {

                MTReconexionPorPago_res = new MTReconexionPorPago()
                {
                    mensajes = new Mensaje()
                    {
                        CodMsj = exception.Message
                    }
                };
            }
            return MTReconexionPorPago_res;
        }

        public MTSaldoPendiente GetSaldoPendiente(string company, DatosEntradaSaldoPendiente SaldoPendienteEntr)
        {
            UrlKeyFirst = "SaldoPendiente";
            ServiceNamespace = GetServiceNamespace(company);
            string urlKey = UrlKeyFirst + company.ToUpper(CultureInfo.CurrentCulture);
            ResourceManager resourceManager = new ResourceManager(typeof(UrlsCompanies));
            string url = resourceManager.GetString(urlKey);
            SoapClientServices = new SoapClient(url, ServiceNamespace);
            MTSaldoPendiente MTSaldoPendiente_res;
            try
            {
                HttpResponseMessage response = SoapClientServices.PostAsync("MT_ConsultaSaldoPendiente_req", "MT_ConsultaSaldoPendiente_res",
                    SaldoPendienteEntr).Result;

                SoapError error;
                string xmlResult;

                if (response.Content != null)
                {
                    xmlResult = response.Content.ReadAsStringAsync().Result;
                    error = GetError(xmlResult);
                }
                else
                {
                    xmlResult = "";
                    error = new SoapError()
                    {
                        Message = "Ha ocurrido un error de conexion " + response.StatusCode
                    };
                }

                if (response.IsSuccessStatusCode)
                {
                    object ConsultaSaldoPendiente = GetData<object>(xmlResult, "MT_ConsultaSaldoPendiente_res", ServiceNamespace);
                    MTSaldoPendiente_res = new MTSaldoPendiente(ConsultaSaldoPendiente);
                }
                else
                {
                    MTSaldoPendiente_res = new MTSaldoPendiente()
                    {
                        mensajes = new Mensaje()
                        {
                            CodMsj = response.StatusCode.ToString()
                        }
                    };
                }
            }
            catch (HttpRequestException exception)
            {

                MTSaldoPendiente_res = new MTSaldoPendiente()
                {
                    mensajes = new Mensaje()
                    {
                        CodMsj = exception.Message
                    }
                };
            }
            return MTSaldoPendiente_res;
        }

        public MTConsultaCupoBrilla GetConsultaCupoBrilla(string company, DatosEntrada consultaCupoBrillaEntrada)
        {
            UrlKeyFirst = "ConsultaCupoBrilla";
            ServiceNamespace = GetServiceNamespace(company);
            string urlKey = UrlKeyFirst + company.ToUpper(CultureInfo.CurrentCulture);
            ResourceManager resourceManager = new ResourceManager(typeof(UrlsCompanies));
            string url = resourceManager.GetString(urlKey);
            if (company == "stg")
            {
                ServiceNamespace = "urn:gaseras.com:srvemp:chatbot";
            }
            SoapClientServices = new SoapClient(url, ServiceNamespace);
            MTConsultaCupoBrilla MTTConsultaCupoBrilla_res;
            try
            {
                HttpResponseMessage response = SoapClientServices.PostAsync("MT_ConsultaCupoBrilla_req", "MT_ConsultaCupoBrilla_res",
                    consultaCupoBrillaEntrada).Result;

                SoapError error;
                string xmlResult;

                if (response.Content != null)
                {
                    xmlResult = response.Content.ReadAsStringAsync().Result;
                    error = GetError(xmlResult);
                }
                else
                {
                    xmlResult = "";
                    error = new SoapError()
                    {
                        Message = "Ha ocurrido un error de conexion " + response.StatusCode
                    };
                }

                if (response.IsSuccessStatusCode)
                {
                    object consultaCupoBrilla = GetData<object>(xmlResult, "MT_ConsultaCupoBrilla_res", ServiceNamespace);
                    MTTConsultaCupoBrilla_res = new MTConsultaCupoBrilla(consultaCupoBrilla, company);
                }
                else
                {
                    MTTConsultaCupoBrilla_res = new MTConsultaCupoBrilla()
                    {
                        mensajes = new Mensaje()
                        {
                            CodMsj = response.StatusCode.ToString(),
                            descMsj = error?.Message
                        }
                    };
                }
            }
            catch (HttpRequestException exception)
            {
                Mensaje mensaje = new Mensaje
                {
                    CodMsj = "",
                    descMsj = exception.Message
                };

                MTTConsultaCupoBrilla_res = new MTConsultaCupoBrilla()
                {
                    mensajes = mensaje
                };
            }
            return MTTConsultaCupoBrilla_res;
        }

        public T GetData<T>(string xmlResult, string methodResult, string _serviceNamespace)
        {
            T result = DeserializeData<T>(
                methodResult,
                xmlResult,
                _serviceNamespace);
            return result;
        }
        private T DeserializeData<T>(string nodeName, string xml, string xmlns)
        {
            XElement xelement = XElement.Parse(xml);
            return xelement.ToObject<T>(nodeName, xmlns);
        }

        private SoapError GetError(string xml)
        {
            XDocument xmlDoc = XDocument.Parse(xml);
            XNamespace xmlns = "http://schemas.xmlsoap.org/soap/envelope/";
            var fault = xmlDoc.Descendants(xmlns + "Fault").FirstOrDefault();
            if (fault != null)
            {
                return new SoapError
                {
                    Code = fault.Element(xmlns + "faultcode")?.Value ??
                           fault.Element("faultcode")?.Value,
                    Message =
                        fault.Element(xmlns + "faultstring")?.Value ??
                        fault.Element("faultstring")?.Value,
                    Detail =
                        fault.Element(xmlns + "detail")?.Value ??
                        fault.Element("detail")?.Value
                };
            }

            xmlns = "http://www.w3.org/2003/05/soap-envelope";
            fault = xmlDoc.Descendants(xmlns + "Fault").FirstOrDefault();
            if (fault != null)
            {
                return new SoapError
                {
                    Code = fault.Element(xmlns + "Code")?.Value,
                    Message = fault.Element(xmlns + "Reason")?.Value,
                    Detail = fault.Element(xmlns + "Detail")?.Value
                };
            }

            return null;
        }

        public string GetServiceNamespace(string company)
        {
            string serviceNamespaceCompany;
            switch (company)
            {
                case MenuSubMenuEnum.CompanyName.Gdo:
                    {
                        serviceNamespaceCompany = "urn:gaseras.com:srvemp:chatbot";
                        break;
                    }
                case MenuSubMenuEnum.CompanyName.Stg:
                    {
                        serviceNamespaceCompany = "urn:gaseras.stg.com:srvemp:chatbot";
                        break;
                    }
                case MenuSubMenuEnum.CompanyName.Ceo:
                    {
                        serviceNamespaceCompany = "urn:gaseras.com:srvemp:chatbot";
                        break;
                    }
                case MenuSubMenuEnum.CompanyName.Quavii:
                    {
                        serviceNamespaceCompany = "urn:gaseras.com:srvemp:chatbot";
                        break;
                    }
                default:
                    {
                        serviceNamespaceCompany = "urn:gaseras.stg.com:srvemp:chatbot";
                        break;
                    }

            }
            return serviceNamespaceCompany;

        }


    }
}
