namespace Infrastructure.Authentication.Exceptions;

public class MissingJwtKeyException(string msg) : Exception(msg);