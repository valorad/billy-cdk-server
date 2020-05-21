namespace App.Database
{
  public class DBConfig : IDBConfig
  {
    public string User { get; set; }
    public string Password { get; set; }
    public string Host { get; set; }
    public string DataDB { get; set; }
    public string AuthDB { get; set; }

  }

}