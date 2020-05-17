namespace App.Models
{
  public class CUDMessage
  {
    public bool OK { get; set; }
    public long NumAffected { get; set; }
    public string Message { get; set; }
  }
}