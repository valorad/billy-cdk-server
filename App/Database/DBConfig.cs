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

  // public class DBLocation : IDBLocation
  // {
    // public string data { get; set; }
    // public string auth { get; set; }
  // }

}