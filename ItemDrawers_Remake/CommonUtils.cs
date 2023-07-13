
using System.Reflection;
using System.Runtime.CompilerServices;

public static class CommonUtils
{
  public static T Clone<T>(
    T obj)
  {
    return (T) obj.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic).Invoke((object) obj, (object[]) null);
  }
}
