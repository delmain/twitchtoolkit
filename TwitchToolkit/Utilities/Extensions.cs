namespace TwitchToolkit.Utilities
{
    public static class Extensions
    {
        public static UnityEngine.Color ToUnityColor(this string hexColor)
        {
            if (UnityEngine.ColorUtility.TryParseHtmlString(hexColor, out UnityEngine.Color unityColor))
                return unityColor;
            else
                return new UnityEngine.Color(0, 0, 0, 1);
        }
    }
}
