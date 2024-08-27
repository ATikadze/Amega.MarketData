namespace Amega.MarketData.Core.DTOs.Response;

public class ResultResponse
{
    public ResultResponse(object result = null, List<string> errorMessages = null)
    {
        Result = result;
        ErrorMessages = errorMessages;
    }

    public object Result { get; set; }

    public List<string> ErrorMessages { get; set; }
}
