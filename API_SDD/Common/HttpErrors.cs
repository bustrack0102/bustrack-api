namespace ApiSDD.Common;

/// <summary>Base de errores de negocio mapeados a un código HTTP por <c>ExceptionHandlingMiddleware</c>.</summary>
public abstract class HttpErrorException : Exception
{
    protected HttpErrorException(string error, int statusCode, string? detail, Exception? inner = null)
        : base(detail ?? error, inner)
    {
        Error = error;
        StatusCode = statusCode;
    }

    public string Error { get; }

    public int StatusCode { get; }
}

/// <summary>Recurso no encontrado → 404.</summary>
public sealed class NotFoundException : HttpErrorException
{
    public NotFoundException()
        : this("Resource not found.")
    {
    }

    public NotFoundException(string detail)
        : base("not_found", StatusCodes.Status404NotFound, detail)
    {
    }

    public NotFoundException(string message, Exception inner)
        : base("not_found", StatusCodes.Status404NotFound, message, inner)
    {
    }
}

/// <summary>Acceso denegado → 403.</summary>
public sealed class ForbiddenException : HttpErrorException
{
    public ForbiddenException()
        : this("Access forbidden.")
    {
    }

    public ForbiddenException(string detail)
        : base("forbidden", StatusCodes.Status403Forbidden, detail)
    {
    }

    public ForbiddenException(string message, Exception inner)
        : base("forbidden", StatusCodes.Status403Forbidden, message, inner)
    {
    }
}

/// <summary>Conflicto de concurrencia/estado → 409.</summary>
public sealed class ConflictException : HttpErrorException
{
    public ConflictException()
        : this("State conflict.")
    {
    }

    public ConflictException(string detail)
        : base("conflict", StatusCodes.Status409Conflict, detail)
    {
    }

    public ConflictException(string message, Exception inner)
        : base("conflict", StatusCodes.Status409Conflict, message, inner)
    {
    }
}

/// <summary>Validación de negocio → 422.</summary>
public sealed class RequestValidationException : HttpErrorException
{
    public RequestValidationException()
        : this("Validation failed.")
    {
    }

    public RequestValidationException(string detail)
        : base("validation", StatusCodes.Status422UnprocessableEntity, detail)
    {
    }

    public RequestValidationException(string message, Exception inner)
        : base("validation", StatusCodes.Status422UnprocessableEntity, message, inner)
    {
    }
}

/// <summary>Credencial inválida → 401 idéntico (anti-enumeración, sin revelar causa).</summary>
public sealed class UnauthorizedException : HttpErrorException
{
    public UnauthorizedException()
        : this("Invalid credentials.")
    {
    }

    public UnauthorizedException(string detail)
        : base("invalid_credentials", StatusCodes.Status401Unauthorized, detail)
    {
    }

    public UnauthorizedException(string message, Exception inner)
        : base("invalid_credentials", StatusCodes.Status401Unauthorized, message, inner)
    {
    }
}
