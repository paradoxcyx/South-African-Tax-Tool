namespace southafricantaxtool.API.Models;

public class GenericResponseModel<T>
{
    public bool Success { get; set; }
    
    public string Message { get; set; }
    
    public T Data { get; set; }
    
}