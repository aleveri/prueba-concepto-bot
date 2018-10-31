using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BasicBot.Dialogs
{
    public class PruebaDialog : ComponentDialog
    {
        private const string ProfileDialog = "profileDialog";
        private const string SegundoPasoPrompt = "promptSegundoPaso";

        public IStatePropertyAccessor<PruebaState> UserProfileAccessor { get; }

        public PruebaDialog(IStatePropertyAccessor<PruebaState> userProfileStateAccessor, ILoggerFactory loggerFactory)
            : base(nameof(PruebaDialog))
        {
            UserProfileAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));

            WaterfallStep[] waterfallSteps = new WaterfallStep[]
            {
                    PrimerPaso,
                    SegundoPaso,
            };

            AddDialog(new WaterfallDialog(ProfileDialog, waterfallSteps));
            AddDialog(new TextPrompt(SegundoPasoPrompt));
        }

        private async Task<DialogTurnResult> PrimerPaso(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            PruebaState pruebaStateOpt = await UserProfileAccessor.GetAsync(stepContext.Context, () => null);

            if (pruebaStateOpt != null)
            {
                await UserProfileAccessor.SetAsync(stepContext.Context, pruebaStateOpt);
            }

            if (pruebaStateOpt == null)
            {
                await UserProfileAccessor.SetAsync(stepContext.Context, new PruebaState());
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> SegundoPaso(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            PruebaState pruebaState = await UserProfileAccessor.GetAsync(stepContext.Context);

            // Logica de negocio necesaria en el flujo.
            if (pruebaState != null)
            {
                PromptOptions opts = new PromptOptions
                {
                    Prompt = new Activity
                    {
                        Type = ActivityTypes.Message,
                        Text = "Hola",
                    },
                };
                return await stepContext.PromptAsync(SegundoPasoPrompt, opts);
            }

            return await TercerPaso(stepContext);
        }

        private async Task<DialogTurnResult> TercerPaso(WaterfallStepContext stepContext)
        {
            ITurnContext context = stepContext.Context;
            PruebaState pruebaState = await UserProfileAccessor.GetAsync(context);
            await context.SendActivityAsync("¿ En que te puedo ayudar ?");
            return await stepContext.EndDialogAsync();
        }

    }
}
