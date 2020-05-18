namespace App.Database
{
  public interface IProjectionOption
  {
      
  }

  public interface IPaginationOption
  {
      
  }

  public interface ISortOption
  {
      
  }

  public interface IViewOption : IProjectionOption, IPaginationOption, ISortOption  {}

  public static class View
  {
    
  }
}