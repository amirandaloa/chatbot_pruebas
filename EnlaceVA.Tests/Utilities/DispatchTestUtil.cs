// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using EnlaceVA.Tests.Mocks;
using EnlaceVA.Tests.Utterances;
using Luis;
using Microsoft.Bot.Builder;
using System.Collections.Generic;

namespace EnlaceVA.Tests.Utilities
{
    public class DispatchTestUtil
    {
        private static Dictionary<string, IRecognizerConvert> _utterances = new Dictionary<string, IRecognizerConvert>
        {
            { GeneralUtterances.Cancel, CreateIntent(GeneralUtterances.Cancel, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Escalate, CreateIntent(GeneralUtterances.Escalate, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.FinishTask, CreateIntent(GeneralUtterances.FinishTask, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.GoBack, CreateIntent(GeneralUtterances.GoBack, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Help, CreateIntent(GeneralUtterances.Help, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Repeat, CreateIntent(GeneralUtterances.Repeat, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.SelectAny, CreateIntent(GeneralUtterances.SelectAny, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.SelectItem, CreateIntent(GeneralUtterances.SelectItem, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.SelectNone, CreateIntent(GeneralUtterances.SelectNone, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.ShowNext, CreateIntent(GeneralUtterances.ShowNext, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.ShowPrevious, CreateIntent(GeneralUtterances.ShowPrevious, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.StartOver, CreateIntent(GeneralUtterances.StartOver, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Stop, CreateIntent(GeneralUtterances.Stop, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Reject, CreateIntent(GeneralUtterances.Reject, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Question, CreateIntent(GeneralUtterances.Question, DispatchLuis.Intent.q_Faq) },
            { GeneralUtterances.Emergency, CreateIntent(GeneralUtterances.Emergency, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Exemption, CreateIntent(GeneralUtterances.Exemption, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.DebtTransfer, CreateIntent(GeneralUtterances.DebtTransfer, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Hello, CreateIntent(GeneralUtterances.Hello, DispatchLuis.Intent.q_Chitchat) },
            { GeneralUtterances.Menu, CreateIntent(GeneralUtterances.Menu, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.ErrorQuestion, CreateIntent(GeneralUtterances.ErrorQuestion, DispatchLuis.Intent.q_Faq) },
            { GeneralUtterances.Abona_a_tu_factura, CreateIntent(GeneralUtterances.Abona_a_tu_factura, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Constancia_de_pago, CreateIntent(GeneralUtterances.Constancia_de_pago, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Cambio_de_nombre, CreateIntent(GeneralUtterances.Cambio_de_nombre, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Terminación_de_contrato, CreateIntent(GeneralUtterances.Terminación_de_contrato, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Valor_fecha_limite_y_referencia_pago, CreateIntent(GeneralUtterances.Valor_fecha_limite_y_referencia_pago, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Estado_de_solicitud_PQRs, CreateIntent(GeneralUtterances.Estado_de_solicitud_PQRs, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Consumo_facturado, CreateIntent(GeneralUtterances.Consumo_facturado, DispatchLuis.Intent.l_General) },
            { GeneralUtterances.Adquiere_tu_seguro, CreateIntent(GeneralUtterances.Adquiere_tu_seguro, DispatchLuis.Intent.l_General) }
        };

        public static MockLuisRecognizer CreateRecognizer()
        {
            var recognizer = new MockLuisRecognizer(defaultIntent: CreateIntent(string.Empty, DispatchLuis.Intent.None));
            recognizer.RegisterUtterances(_utterances);
            return recognizer;
        }

        public static DispatchLuis CreateIntent(string userInput, DispatchLuis.Intent intent)
        {
            var result = new DispatchLuis
            {
                Text = userInput,
                Intents = new Dictionary<DispatchLuis.Intent, IntentScore>()
            };

            result.Intents.Add(intent, new IntentScore() { Score = 0.9 });

            result.Entities = new DispatchLuis._Entities
            {
                _instance = new DispatchLuis._Entities._Instance()
            };

            return result;
        }
    }
}