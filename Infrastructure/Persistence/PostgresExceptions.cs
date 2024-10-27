namespace Infrastructure.Persistence;

public static class PostgresExceptions
{
    public const string UniqueConstraintViolation = "23505";
    public const string ForeignKeyViolation = "23503";
}