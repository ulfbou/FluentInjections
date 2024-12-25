namespace FluentInjections.Extensions;

public static class ConversionExtensions
{
    public static bool TryConvertTo<TTarget>(this object source, out TTarget result) where TTarget : class
    {
        result = default;

        // Try direct cast first using 'as'
        if (source is TTarget directCastResult)
        {
            result = directCastResult;
            return true;
        }

        // Fallback to compatibility check and reflection-based conversion if direct cast fails
        var sourceType = source.GetType();
        var targetType = typeof(TTarget);

        if (sourceType.IsCompatibleWith(targetType))
        {
            try
            {
                result = (TTarget)Convert.ChangeType(source, targetType);
                return true;
            }
            catch
            {
                // Conversion failed
            }
        }

        return false;
    }
} 
