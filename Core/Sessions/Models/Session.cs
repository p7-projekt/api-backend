namespace Core.Sessions.Models;

public class Session
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int AuthorId { get; set; }
    
    public DateTime ExpirationTimeUtc { get; set; }

    public string SessionCode { get; set; } = string.Empty;

    public List<int> Exercises { get; set; } = new (); // this needs to be changed when exercises are available
}