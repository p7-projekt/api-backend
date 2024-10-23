namespace Infrastructure;

public enum PostgresExceptions
{
    UniqueConstraintViolation = 23505,
    ForeignKeyViolation = 23503
}