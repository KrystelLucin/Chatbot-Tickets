using System.Text.RegularExpressions;
using BotConversacional.Application.DTOs;
using BotConversacional.Application.Interfaces;
using BotConversacional.Domain.Entities;
using BotConversacional.Domain.Enums;

namespace BotConversacional.Application.Services;

public class ConversationService
{
    private readonly IConversationStateStore _conversationStateStore;
    private readonly ITicketClient _ticketClient;

    public ConversationService(
        IConversationStateStore conversationStateStore,
        ITicketClient ticketClient)
    {
        _conversationStateStore = conversationStateStore;
        _ticketClient = ticketClient;
    }

    public async Task<MessageResponse> HandleMessageAsync(MessageRequest request)
    {
        if (request is null)
        {
            return new MessageResponse
            {
                Reply = "La solicitud no puede ser nula."
            };
        }

        if (string.IsNullOrWhiteSpace(request.ConversationId))
        {
            return new MessageResponse
            {
                Reply = "El conversationId es obligatorio."
            };
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new MessageResponse
            {
                Reply = "El mensaje no puede estar vacío."
            };
        }

        var userMessage = request.Message.Trim();
        var normalizedMessage = userMessage.ToLowerInvariant();

        var state = _conversationStateStore.Get(request.ConversationId);

        if (normalizedMessage == "cancelar")
        {
            _conversationStateStore.Clear(request.ConversationId);

            return new MessageResponse
            {
                Reply = "El flujo actual fue cancelado y el estado de la conversación fue limpiado."
            };
        }

        if (TryHandleTicketStatusQuery(normalizedMessage, out var ticketId))
        {
            try
            {
                var status = await _ticketClient.GetTicketStatusAsync(ticketId);

                if (string.Equals(status, "NotFound", StringComparison.OrdinalIgnoreCase))
                {
                    return new MessageResponse
                    {
                        Reply = $"No encontre un ticket con el ID: {ticketId}"
                    };
                }

                return new MessageResponse
                {
                    Reply = $"El estado del ticket {ticketId} es: {status}"
                };
            }
            catch
            {
                return new MessageResponse
                {
                    Reply = $"Ocurrio un error al consultar el ticket {ticketId}. Intenta nuevamente mas tarde."
                };
            }
        }

        if (state.ActiveFlow == ActiveFlow.None)
        {
            if (WantsToCreateTicket(normalizedMessage))
            {
                state.ActiveFlow = ActiveFlow.CreateTicket;
                state.CurrentStep = ConversationStep.AskName;
                state.TicketDraft = new TicketDraft();

                _conversationStateStore.Save(request.ConversationId, state);

                return new MessageResponse
                {
                    Reply = "Claro, vamos a crear un ticket. ¿Cuál es tu nombre?"
                };
            }

            return new MessageResponse
            {
                Reply = "Puedo ayudarte a crear un ticket o consultar el estado de uno existente. Escribe, por ejemplo, 'quiero crear un ticket' o 'ver estado del ticket TCK-123'."
            };
        }

        if (state.ActiveFlow == ActiveFlow.CreateTicket)
        {
            return await HandleCreateTicketFlowAsync(request.ConversationId, userMessage, normalizedMessage, state);
        }

        return new MessageResponse
        {
            Reply = "No pude procesar tu solicitud."
        };
    }

    private async Task<MessageResponse> HandleCreateTicketFlowAsync(
        string conversationId,
        string userMessage,
        string normalizedMessage,
        ConversationState state)
    {
        switch (state.CurrentStep)
        {
            case ConversationStep.AskName:
                state.TicketDraft.Name = userMessage;
                state.CurrentStep = ConversationStep.AskEmail;

                _conversationStateStore.Save(conversationId, state);

                return new MessageResponse
                {
                    Reply = "Gracias. Ahora indícame tu correo electrónico."
                };

            case ConversationStep.AskEmail:
                if (!IsValidEmail(userMessage))
                {
                    return new MessageResponse
                    {
                        Reply = "El formato del email no es válido. Por favor, ingresa un correo electrónico válido."
                    };
                }

                state.TicketDraft.Email = userMessage;
                state.CurrentStep = ConversationStep.AskDescription;

                _conversationStateStore.Save(conversationId, state);

                return new MessageResponse
                {
                    Reply = "Perfecto. Ahora describe el problema."
                };

            case ConversationStep.AskDescription:
                state.TicketDraft.Description = userMessage;
                state.CurrentStep = ConversationStep.AwaitingConfirmation;

                _conversationStateStore.Save(conversationId, state);

                return new MessageResponse
                {
                    Reply =
                        $"Resumen del ticket:\n" +
                        $"- Nombre: {state.TicketDraft.Name}\n" +
                        $"- Email: {state.TicketDraft.Email}\n" +
                        $"- Descripción: {state.TicketDraft.Description}\n\n" +
                        $"¿Deseas confirmar la creación del ticket? Responde 'si' para confirmar o 'cancelar' para abortar."
                };

            case ConversationStep.AwaitingConfirmation:
                if (normalizedMessage is "si" or "sí")
                {
                    try
                    {
                        var ticketId = await _ticketClient.CreateTicketAsync(state.TicketDraft);

                        _conversationStateStore.Clear(conversationId);

                        return new MessageResponse
                        {
                            Reply = $"Tu ticket fue creado exitosamente con el ID: {ticketId}"
                        };
                    }
                    catch
                    {
                        return new MessageResponse
                        {
                            Reply = "No pude crear el ticket por un error del servicio. Puedes intentar nuevamente con 'si' o cancelar con 'cancelar'."
                        };
                    }
                }

                return new MessageResponse
                {
                    Reply = "No entendí tu respuesta. Escribe 'si' para confirmar o 'cancelar' para abortar el flujo."
                };

            default:
                return new MessageResponse
                {
                    Reply = "El estado actual de la conversación no es válido."
                };
        }
    }

    private static bool WantsToCreateTicket(string message)
    {
        return message.Contains("crear un ticket")
               || message.Contains("crear ticket")
               || message.Contains("quiero un ticket")
               || message.Contains("quiero crear un ticket");
    }

    private static bool TryHandleTicketStatusQuery(string message, out string ticketId)
    {
        ticketId = string.Empty;

        var match = Regex.Match(message, @"ver estado del ticket\s+([a-zA-Z0-9\-]+)", RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            return false;
        }

        ticketId = match.Groups[1].Value.ToUpperInvariant();
        return true;
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return Regex.IsMatch(
            email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase);
    }
}
