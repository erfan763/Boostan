using Application.Repositories;
using AutoMapper;
using DominClass.Entities.User;
using MediatR;

namespace Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserHandler : IRequestHandler<CreateUserRequest, CreateUserResponse>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public CreateUserHandler(IUnitOfWork unitOfWork, IUserRepository userRepository, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = _mapper.Map<User>(request);
        await _userRepository.CreateUser(user);
        await _unitOfWork.Save(cancellationToken);

        return _mapper.Map<CreateUserResponse>(user);
    }
}