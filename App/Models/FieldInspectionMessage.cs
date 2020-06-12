namespace App.Models
{
  public class FieldInspectionMessage
  {
    public bool IsPassed { get; set; }
    public string FailedField { get; set; }
    public string Message { get; set; }
  }

}