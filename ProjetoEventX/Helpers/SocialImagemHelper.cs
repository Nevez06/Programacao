namespace ProjetoEventX.Helpers
{
    public static class SocialImagemHelper
    {
        public static string NormalizePublicImageUrl(string? rawUrl, string fallbackUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                return fallbackUrl;
            }

            var normalized = rawUrl.Trim().Replace('\\', '/');
            if (normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return normalized;
            }

            var uploadsIdx = normalized.IndexOf("/uploads/", StringComparison.OrdinalIgnoreCase);
            if (uploadsIdx >= 0)
            {
                normalized = normalized[uploadsIdx..];
            }
            else
            {
                uploadsIdx = normalized.IndexOf("uploads/", StringComparison.OrdinalIgnoreCase);
                if (uploadsIdx >= 0)
                {
                    normalized = "/" + normalized[uploadsIdx..];
                }
                else
                {
                    var webRootIdx = normalized.IndexOf("/wwwroot/", StringComparison.OrdinalIgnoreCase);
                    if (webRootIdx >= 0)
                    {
                        normalized = normalized[(webRootIdx + "/wwwroot".Length)..];
                    }
                    else
                    {
                        webRootIdx = normalized.IndexOf("wwwroot/", StringComparison.OrdinalIgnoreCase);
                        if (webRootIdx >= 0)
                        {
                            normalized = "/" + normalized[(webRootIdx + "wwwroot/".Length)..];
                        }
                    }
                }
            }

            if (!normalized.StartsWith('/'))
            {
                normalized = "/" + normalized.TrimStart('/');
            }

            return string.IsNullOrWhiteSpace(normalized) ? fallbackUrl : normalized;
        }
    }
}
