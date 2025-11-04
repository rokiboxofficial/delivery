using CSharpFunctionalExtensions;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;

public sealed class MoveCouriersCommand : IRequest<UnitResult<Error>>;