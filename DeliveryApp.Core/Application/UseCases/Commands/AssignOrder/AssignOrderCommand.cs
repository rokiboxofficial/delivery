using CSharpFunctionalExtensions;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;

public sealed class AssignOrderCommand : IRequest<UnitResult<Error>>;