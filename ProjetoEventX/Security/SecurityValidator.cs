using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProjetoEventX.Security
{
    public static class SecurityValidator
    {
        private static readonly string[] DangerousPatterns = {
            "<script[^>]*>.*?</script>", // Scripts HTML
            "javascript:", // JavaScript inline
            "vbscript:", // VBScript
            "onload\s*=", // Event handlers
            "onerror\s*=",
            "onclick\s*=",
            "onmouseover\s*=",
            "<iframe[^>]*>",
            "<object[^>]*>",
            "<embed[^>]*>",
            "<form[^>]*>",
            "' OR '", // SQL Injection básico
            "' OR 1=1",
            "UNION SELECT",
            "DROP TABLE",
            "INSERT INTO",
            "DELETE FROM",
            "UPDATE ",
            "EXEC(",
            "xp_", // Extended procedures SQL
            "sp_", // Stored procedures
            "../", // Path traversal
            "..\\",
            "%2e%2e", // URL encoded path traversal
            "0x3c", // Hex encoded <
            "0x3e", // Hex encoded >
            "&#x3c;", // HTML entity encoded
            "&#x3e;",
            "&#60;",
            "&#62;"
        };

        private static readonly string[] AllowedFileExtensions = {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg",
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".txt", ".csv", ".zip", ".rar"
        };

        public static bool IsValidInput(string input, bool allowHtml = false)
        {
            if (string.IsNullOrWhiteSpace(input))
                return true;

            var trimmedInput = input.Trim();

            // Verificar tamanho máximo
            if (trimmedInput.Length > 10000)
                return false;

            // Se permitir HTML, usar validação mais permissiva
            if (allowHtml)
            {
                return IsSafeHtml(trimmedInput);
            }

            // Validação estrita - não permite HTML
            return !ContainsDangerousContent(trimmedInput, DangerousPatterns);
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return emailRegex.IsMatch(email) && email.Length <= 256;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
            return AllowedFileExtensions.Contains(extension) && fileName.Length <= 255;
        }

        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // Opcional

            // Remove caracteres não numéricos
            var numericPhone = Regex.Replace(phone, @"[^\d]", "");
            
            // Valida números brasileiros
            return numericPhone.Length >= 10 && numericPhone.Length <= 11;
        }

        public static bool IsValidCPF(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            // Remove caracteres especiais
            cpf = Regex.Replace(cpf, @"[^\d]", "");

            // Verifica se tem 11 dígitos
            if (cpf.Length != 11)
                return false;

            // Verifica se são todos dígitos iguais
            if (cpf.Distinct().Count() == 1)
                return false;

            // Calcula dígitos verificadores
            return IsValidCPFCheck(cpf);
        }

        private static bool IsValidCPFCheck(string cpf)
        {
            try
            {
                // Primeiro dígito verificador
                int soma = 0;
                for (int i = 0; i < 9; i++)
                    soma += int.Parse(cpf[i].ToString()) * (10 - i);
                
                int resto = soma % 11;
                int digito1 = resto < 2 ? 0 : 11 - resto;

                // Segundo dígito verificador
                soma = 0;
                for (int i = 0; i < 10; i++)
                    soma += int.Parse(cpf[i].ToString()) * (11 - i);
                
                resto = soma % 11;
                int digito2 = resto < 2 ? 0 : 11 - resto;

                return cpf.EndsWith(digito1.ToString() + digito2.ToString());
            }
            catch
            {
                return false;
            }
        }

        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove caracteres de controle perigosos
            var sanitized = Regex.Replace(input, @"[\x00-\x1F\x7F]", "");
            
            // Limita tamanho
            if (sanitized.Length > 10000)
                sanitized = sanitized.Substring(0, 10000);

            return sanitized.Trim();
        }

        public static string SanitizeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Remove scripts e eventos perigosos
            var sanitized = Regex.Replace(html, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"on\w+\s*=\s*['\"][^'\"]*['\"]", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"javascript:", "", RegexOptions.IgnoreCase);
            
            return sanitized;
        }

        private static bool ContainsDangerousContent(string input, string[] patterns)
        {
            var lowerInput = input.ToLowerInvariant();
            return patterns.Any(pattern => 
                Regex.IsMatch(lowerInput, pattern.ToLowerInvariant(), RegexOptions.IgnoreCase));
        }

        private static bool IsSafeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return true;

            // Tags HTML permitidas
            var allowedTags = new[] { "p", "br", "strong", "b", "em", "i", "u", "h1", "h2", "h3", "h4", "h5", "h6", "ul", "ol", "li", "a", "img" };
            
            // Verifica se contém apenas tags permitidas
            var tagRegex = new Regex(@"<\/?([a-zA-Z][a-zA-Z0-9]*)\b[^>]*>", RegexOptions.IgnoreCase);
            var matches = tagRegex.Matches(html);
            
            foreach (Match match in matches)
            {
                var tagName = match.Groups[1].Value.ToLowerInvariant();
                if (!allowedTags.Contains(tagName))
                    return false;
            }

            return true;
        }
    }
}