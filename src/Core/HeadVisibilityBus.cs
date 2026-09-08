
public static class HeadVisibilityBus
{
    private static bool _isHidden = false; 

    public static event Action<bool> OnVisibilityChanged;

    public static bool IsHidden => _isHidden;

    public static void SetVisibility(bool hidden)
    {
        if (_isHidden != hidden)
        {
            _isHidden = hidden;
            OnVisibilityChanged?.Invoke(_isHidden);
        }
    }

    public static void ToggleVisibility() => SetVisibility(!_isHidden);
}