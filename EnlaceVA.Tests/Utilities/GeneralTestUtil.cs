// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using EnlaceVA.Tests.Mocks;
using EnlaceVA.Tests.Utterances;
using Luis;
using Microsoft.Bot.Builder;
using System.Collections.Generic;

namespace EnlaceVA.Tests.Utilities
{
    public class GeneralTestUtil
    {
        private static readonly Dictionary<string, IRecognizerConvert> _utterances = new Dictionary<string, IRecognizerConvert>
        {
            { GeneralUtterances.Confirm, CreateIntent(GeneralUtterances.Cancel, GeneralLuis.Intent.Confirm) },
            { GeneralUtterances.Reject, CreateIntent(GeneralUtterances.Reject, GeneralLuis.Intent.Reject) },
            { GeneralUtterances.Emergency, CreateIntent(GeneralUtterances.Emergency, GeneralLuis.Intent.Emergencias) },
            { GeneralUtterances.Exemption, CreateIntent(GeneralUtterances.Exemption, GeneralLuis.Intent.Exencion_de_contribucion) },
            { GeneralUtterances.DebtTransfer, CreateIntent(GeneralUtterances.DebtTransfer, GeneralLuis.Intent.Traslado_de_deuda_o_desmonte) },
            { GeneralUtterances.Menu, CreateIntent(GeneralUtterances.Menu, GeneralLuis.Intent.Menu) },
            { GeneralUtterances.Abona_a_tu_factura, CreateIntent(GeneralUtterances.Abona_a_tu_factura, GeneralLuis.Intent.Abona_a_tu_factura) },
            { GeneralUtterances.Constancia_de_pago, CreateIntent(GeneralUtterances.Constancia_de_pago, GeneralLuis.Intent.Constancia_de_pago) },
            { GeneralUtterances.Cambio_de_nombre, CreateIntent(GeneralUtterances.Cambio_de_nombre, GeneralLuis.Intent.Cambio_de_nombre) },
            { GeneralUtterances.Terminación_de_contrato, CreateIntent(GeneralUtterances.Terminación_de_contrato, GeneralLuis.Intent.Terminacion_de_contrato) },
            { GeneralUtterances.Valor_fecha_limite_y_referencia_pago, CreateIntent(GeneralUtterances.Valor_fecha_limite_y_referencia_pago, GeneralLuis.Intent.Valor_fecha_limite_y_referencia_pago) },
            { GeneralUtterances.Estado_de_solicitud_PQRs, CreateIntent(GeneralUtterances.Estado_de_solicitud_PQRs, GeneralLuis.Intent.Estado_de_solicitud_PQRs) },
            { GeneralUtterances.Consumo_facturado, CreateIntent(GeneralUtterances.Consumo_facturado, GeneralLuis.Intent.Consumo_facturado) },
            { GeneralUtterances.Adquiere_tu_seguro, CreateIntent(GeneralUtterances.Adquiere_tu_seguro, GeneralLuis.Intent.Adquiere_tu_seguro) }
        };

        public static MockLuisRecognizer CreateRecognizer()
        {
            var recognizer = new MockLuisRecognizer(defaultIntent: CreateIntent(string.Empty, GeneralLuis.Intent.None));
            recognizer.RegisterUtterances(_utterances);
            return recognizer;
        }

        public static GeneralLuis CreateIntent(string userInput, GeneralLuis.Intent intent)
        {
            var result = new GeneralLuis
            {
                Text = userInput,
                Intents = new Dictionary<GeneralLuis.Intent, IntentScore>()
            };

            result.Intents.Add(intent, new IntentScore() { Score = 0.9 });

            result.Entities = new GeneralLuis._Entities
            {
                _instance = new GeneralLuis._Entities._Instance()
            };

            return result;
        }
    }
}
