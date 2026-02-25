namespace RouteEvidence.Domain.ValueObjects;

public sealed class Location
{
    public double Latitude { get; }
    public double Longitude { get; }

    private Location(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static Result<Location> Create(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
            return Result.Failure<Location>("Latitude must be between -90 and 90.");
        if (longitude is < -180 or > 180)
            return Result.Failure<Location>("Longitude must be between -180 and 180.");
        return Result.Success(new Location(latitude, longitude));
    }
}

public static class Result
{
    public static Result<T> Success<T>(T value) => new(value);
    public static Result<T> Failure<T>(string error) => new(error);
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    internal Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    internal Result(string error)
    {
        IsSuccess = false;
        Error = error;
    }
}
