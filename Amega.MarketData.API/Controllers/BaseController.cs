using Amega.MarketData.Core.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace Amega.MarketData.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    protected JsonResult Success()
    {
        return Success(null);
    }

    protected JsonResult Success(object result)
    {
        return JsonResponse(result);
    }

    private JsonResult JsonResponse(object response, List<string> errorMessages = null)
    {
        return new JsonResult(new ResultResponse(response, errorMessages));
    }
}
