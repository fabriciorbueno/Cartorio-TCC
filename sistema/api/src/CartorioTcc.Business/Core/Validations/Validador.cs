using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace CartorioTcc.Business.Core.Validations;

/// <summary>
/// Validações de campos definidas nas "Regras de Negócio" dos casos de uso do TCC
/// (CPF/CNPJ válidos, nome somente letras, CEP com 8 dígitos, telefone numérico, maioridade, etc.).
/// </summary>
public static partial class Validador
{
    public static string SomenteNumeros(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? string.Empty : new string(valor.Where(char.IsDigit).ToArray());

    public static bool CpfValido(string? cpf)
    {
        var numeros = SomenteNumeros(cpf);
        if (numeros.Length != 11 || numeros.Distinct().Count() == 1) return false;

        var digitos = numeros.Select(c => c - '0').ToArray();

        var soma = 0;
        for (var i = 0; i < 9; i++) soma += digitos[i] * (10 - i);
        var resto = soma % 11;
        var dv1 = resto < 2 ? 0 : 11 - resto;
        if (digitos[9] != dv1) return false;

        soma = 0;
        for (var i = 0; i < 10; i++) soma += digitos[i] * (11 - i);
        resto = soma % 11;
        var dv2 = resto < 2 ? 0 : 11 - resto;
        return digitos[10] == dv2;
    }

    public static bool CnpjValido(string? cnpj)
    {
        var numeros = SomenteNumeros(cnpj);
        if (numeros.Length != 14 || numeros.Distinct().Count() == 1) return false;

        int[] pesos1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] pesos2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        var digitos = numeros.Select(c => c - '0').ToArray();

        var soma = 0;
        for (var i = 0; i < 12; i++) soma += digitos[i] * pesos1[i];
        var resto = soma % 11;
        var dv1 = resto < 2 ? 0 : 11 - resto;
        if (digitos[12] != dv1) return false;

        soma = 0;
        for (var i = 0; i < 13; i++) soma += digitos[i] * pesos2[i];
        resto = soma % 11;
        var dv2 = resto < 2 ? 0 : 11 - resto;
        return digitos[13] == dv2;
    }

    /// <summary>Regra "O campo deve conter apenas letras" (acentos e espaços são permitidos).</summary>
    public static bool NomeValido(string? nome) =>
        !string.IsNullOrWhiteSpace(nome) && RegexNome().IsMatch(nome.Trim());

    public static bool CepValido(string? cep) => SomenteNumeros(cep).Length == 8;

    public static bool TelefoneValido(string? telefone)
    {
        var numeros = SomenteNumeros(telefone);
        return numeros.Length is >= 10 and <= 11;
    }

    public static bool EmailValido(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var endereco = new MailAddress(email.Trim());
            return endereco.Address.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase) && email.Contains('.');
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static bool UfValida(string? uf) =>
        !string.IsNullOrWhiteSpace(uf) && Ufs.Contains(uf.Trim().ToUpperInvariant());

    public static int CalcularIdade(DateOnly nascimento, DateOnly hoje)
    {
        var idade = hoje.Year - nascimento.Year;
        if (nascimento > hoje.AddYears(-idade)) idade--;
        return idade;
    }

    public static bool MaiorDeIdade(DateOnly nascimento, DateOnly hoje) => CalcularIdade(nascimento, hoje) >= 18;

    /// <summary>Regra de senha do UC-02: tamanho entre 6 e 11 caracteres.</summary>
    public static bool SenhaValida(string? senha) => senha is { Length: >= 6 and <= 11 };

    public static string FormatarCpf(string? cpf)
    {
        var n = SomenteNumeros(cpf);
        return n.Length == 11 ? Convert.ToUInt64(n).ToString(@"000\.000\.000\-00", CultureInfo.InvariantCulture) : n;
    }

    public static string FormatarCnpj(string? cnpj)
    {
        var n = SomenteNumeros(cnpj);
        return n.Length == 14 ? Convert.ToUInt64(n).ToString(@"00\.000\.000\/0000\-00", CultureInfo.InvariantCulture) : n;
    }

    private static readonly HashSet<string> Ufs =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA",
        "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    ];

    [GeneratedRegex(@"^[\p{L}][\p{L}'\.\s-]*$")]
    private static partial Regex RegexNome();
}
