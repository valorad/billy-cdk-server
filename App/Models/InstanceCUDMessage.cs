using System.Collections.Generic;

namespace App.Models
{
  public class InstanceCUDMessage<T> : CUDMessage
  {
    public T Instance { get; set; }
    public List<T> Instances { get; set; }
  }

}