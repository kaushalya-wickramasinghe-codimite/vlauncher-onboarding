using MediatR;
using VLauncher.Application.Common;
using VLauncher.Domain.Interfaces;

namespace VLauncher.Application.Users.Commands;

public record DeleteUserCommand(int UserId) : IRequest<Result>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);

        if (user == null)
            return Result.Failure("User not found");

        await _unitOfWork.Users.DeleteAsync(request.UserId);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
