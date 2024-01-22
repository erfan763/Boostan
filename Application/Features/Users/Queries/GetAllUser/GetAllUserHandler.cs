using Application.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Users.Queries.GetAllUser;

public sealed class GetAllUserHandler : IRequestHandler<GetAllUserRequest, List<GetAllUserResponse>>
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;

    public GetAllUserHandler(IMapper mapper, IUserRepository userRepository)
    {
        _mapper = mapper;
        _userRepository = userRepository;
    }

    public async Task<List<GetAllUserResponse>> Handle(GetAllUserRequest request, CancellationToken cancellationToken)
    {
        return _mapper.Map<List<GetAllUserResponse>>(await _userRepository.GetAllUser());
    }
}