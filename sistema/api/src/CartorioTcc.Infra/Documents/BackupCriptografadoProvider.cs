using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CartorioTcc.Business.Interfaces;
using CartorioTcc.Infra.Data.Context;

namespace CartorioTcc.Infra.Documents;

/// <summary>
/// UC-03 / UC-05: Provedor de geração de Backup Criptografado (AES-256 + SHA-256)
/// do acervo notarial e base de dados do SAEC.
/// </summary>
public sealed class BackupCriptografadoProvider(CartorioTccContext context) : IBackupCriptografadoProvider
{
    private static readonly byte[] AesKey = SHA256.HashData(Encoding.UTF8.GetBytes("SAEC-2CartorioNotasCampinas-ChaveMestra-2015-2026"));

    public async Task<(string NomeArquivo, byte[] ConteudoCriptografado, string HashSha256)> GerarPacoteBackupCriptografadoAsync(
        string descricao,
        CancellationToken ct = default)
    {
        var snapshot = new
        {
            GeradoEmUtc = DateTime.UtcNow,
            Serventia = "2º Tabelionato de Notas de Campinas - SAEC",
            Descricao = descricao,
            Cartorios = await context.Cartorios.AsNoTracking().CountAsync(ct),
            Funcionarios = await context.Funcionarios.AsNoTracking().CountAsync(ct),
            Clientes = await context.Clientes.AsNoTracking().CountAsync(ct),
            Protocolos = await context.Protocolos.AsNoTracking().CountAsync(ct),
            Orcamentos = await context.Orcamentos.AsNoTracking().CountAsync(ct),
            Escrituras = await context.Escrituras.AsNoTracking().Select(e => new
            {
                e.Id,
                e.NumeroEscritura,
                e.NumeroLivro,
                e.FolhaInicial,
                e.FolhaFinal,
                e.StatusEscritura,
                e.DataLavratura
            }).ToListAsync(ct)
        };

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(snapshot, new JsonSerializerOptions { WriteIndented = true });

        using var aes = Aes.Create();
        aes.Key = AesKey;
        aes.GenerateIV();

        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);
        using (var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
        {
            cryptoStream.Write(jsonBytes, 0, jsonBytes.Length);
            cryptoStream.FlushFinalBlock();
        }

        var encryptedBytes = ms.ToArray();
        var hashHex = Convert.ToHexString(SHA256.HashData(encryptedBytes));
        var nomeArquivo = $"SAEC_BACKUP_{DateTime.Now:yyyyMMdd_HHmmss}.saec.enc";

        return (nomeArquivo, encryptedBytes, hashHex);
    }
}
