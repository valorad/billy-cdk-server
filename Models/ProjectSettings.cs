namespace BillyCDK.Models
{
  public interface IProjectSettings
  {
    string ConnectionString { get; set; }
    public string DBName { get; set; }

  }
  public class ProjectSettings : IProjectSettings
  {
    public string ConnectionString { get; set; }
    public string DBName { get; set; }
  }
}