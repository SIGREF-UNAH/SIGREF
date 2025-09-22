namespace SIGREF.API.Constants;

public class Env
{
    public PhirConfig Phir { get; set; } = new();
}

public class PhirConfig
{
    public string BaseUrl { get; set; } = string.Empty;
}