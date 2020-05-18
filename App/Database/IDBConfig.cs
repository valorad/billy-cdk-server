namespace App.Database
{

  // public interface IDBLocation
  // {
  //   string Data { get; set; }
  //   string Auth { get; set; }
  // }

  public interface IDBConfig
  {
    public string User { get; set; }
    public string Password { get; set; }

    public string Host { get; set; }
    public string DataDB { get; set; }
    public string AuthDB { get; set; }

  }
}