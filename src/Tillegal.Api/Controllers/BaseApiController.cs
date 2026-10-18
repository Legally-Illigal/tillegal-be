using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Tillegal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController(ISender mediator, IMapper mapper) : ControllerBase
{
    protected ISender Mediator { get; } = mediator;

    protected IMapper Mapper { get; } = mapper;
}
