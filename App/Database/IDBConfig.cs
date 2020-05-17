namespace App.Database
{

  public interface IDBLocation
  {
    string Data { get; set; }
    string Auth { get; set; }
  }

  public interface IDBConfig
  {
    string User { get; set; }
    string Password { get; set; }

    string Host { get; set; }
    IDBLocation DB { get; set; }

  }
}