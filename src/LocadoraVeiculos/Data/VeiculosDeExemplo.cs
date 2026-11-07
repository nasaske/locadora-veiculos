using LocadoraVeiculos.Models;
using LocadoraVeiculos.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Data;

/// <summary>
/// Frota de exemplo usada para a vitrine do front-end não nascer vazia.
/// Só é gravada quando a tabela de veículos está vazia e a configuração
/// "PopularVeiculosDeExemplo" está ligada (ambiente de desenvolvimento).
/// As fotos são do Wikimedia Commons (licenças livres).
/// </summary>
public static class VeiculosDeExemplo
{
    private const string Commons = "https://commons.wikimedia.org/wiki/Special:FilePath/";

    // Ids das cargas iniciais do ApplicationContext (PopularDadosIniciais).
    private const int Fiat = 1, Volkswagen = 2, Chevrolet = 3, Toyota = 4;
    private const int Hatch = 1, Sedan = 2, Suv = 3, Utilitario = 4;
    private const int Savassi = 1, Confins = 2, Contagem = 3;

    private sealed record Exemplo(
        string Modelo, int FabricanteId, int CategoriaId, int FilialId, string Placa,
        int Ano, string Cor, int Km, TipoCombustivel Combustivel, decimal Diaria, string Foto);

    // A ordem importa: o último da lista é o "mais recente" da vitrine.
    private static readonly Exemplo[] Frota =
    [
        new("Mobi Like 1.0", Fiat, Hatch, Contagem, "RFA1B21", 2022, "Branco", 38200, TipoCombustivel.Flex, 99.90m, "Fiat_Mobi_Like_1.0_Fire_flex.jpg"),
        new("Argo Drive 1.3", Fiat, Hatch, Savassi, "RFA2C32", 2023, "Vermelho", 21500, TipoCombustivel.Flex, 119.90m, "2023_Fiat_Argo_1.3_Drive_%28facelift%2C_Brazil%29.jpg"),
        new("Cronos Precision 1.3", Fiat, Sedan, Confins, "RFA3D43", 2024, "Cinza", 12800, TipoCombustivel.Flex, 159.90m, "2024_Fiat_Cronos_1.3_GSE_Precision.jpg"),
        new("Strada Volcano", Fiat, Utilitario, Contagem, "RFA4E54", 2023, "Branco", 30100, TipoCombustivel.Flex, 219.90m, "Fiat_Strada_2020_Volcano_in_Montevideo_%28front%29_%28cropped%29.jpg"),
        new("Toro Volcano", Fiat, Utilitario, Savassi, "RFA5F65", 2024, "Preto", 18700, TipoCombustivel.Diesel, 279.90m, "Fiat_Toro_Volcano.jpg"),
        new("Polo MSi 1.6", Volkswagen, Hatch, Confins, "RVW1A12", 2022, "Prata", 41200, TipoCombustivel.Flex, 124.90m, "2018_Volkswagen_Polo_1.6_MSi_Trendline_%28Brazil%2C_front%29.jpg"),
        new("Virtus Trendline", Volkswagen, Sedan, Savassi, "RVW2B23", 2024, "Branco", 9800, TipoCombustivel.Flex, 164.90m, "2024_Volkswagen_Virtus_1.6_MSi_Trendline_%28facelift%29.jpg"),
        new("T-Cross 200 TSI", Volkswagen, Suv, Confins, "RVW3C34", 2024, "Azul", 15400, TipoCombustivel.Flex, 239.90m, "Volkswagen_T-Cross_%282023%29_1X7A2500.jpg"),
        new("Onix RS 1.0 Turbo", Chevrolet, Hatch, Savassi, "RCH1D45", 2023, "Cinza", 26300, TipoCombustivel.Flex, 129.90m, "Chevrolet_Onix_Mk2_RS_2020_in_Maldonado_-_front.jpg"),
        new("Onix Plus Premier", Chevrolet, Sedan, Contagem, "RCH2E56", 2023, "Branco", 22900, TipoCombustivel.Flex, 169.90m, "2023_Chevrolet_Onix_Plus_1.0T_Premier_AT.jpg"),
        new("Tracker LTZ", Chevrolet, Suv, Savassi, "RCH3F67", 2024, "Vermelho", 11200, TipoCombustivel.Flex, 249.90m, "Chevrolet_Tracker_2021_%28front%29_%28cropped%29.png"),
        new("S10 LS 2.8", Chevrolet, Utilitario, Confins, "RCH4G78", 2022, "Prata", 52400, TipoCombustivel.Diesel, 299.90m, "2017_Chevrolet_S-10_2.8_4X2_LS_front_view.jpg"),
        new("Corolla XEi", Toyota, Sedan, Savassi, "RTY1H89", 2024, "Preto", 14600, TipoCombustivel.Flex, 199.90m, "Toyota_Corolla_Sedan_%28Brazil%29.jpg"),
        new("Corolla Cross Hybrid", Toyota, Suv, Confins, "RTY2J91", 2025, "Branco", 6200, TipoCombustivel.Hibrido, 289.90m, "Toyota_Corolla_Cross_Hybrid_1X7A1861.jpg"),
        new("Hilux SRV 4x4", Toyota, Utilitario, Contagem, "RTY3K12", 2024, "Prata", 23800, TipoCombustivel.Diesel, 339.90m, "Toyota_Hilux_4x4_G_2016_%281%29.jpg"),
    ];

    public static async Task<int> PopularAsync(ApplicationContext contexto, CancellationToken cancelamento = default)
    {
        if (await contexto.Veiculos.AnyAsync(cancelamento))
            return 0;

        var sequencia = 0;
        foreach (var exemplo in Frota)
        {
            sequencia++;
            contexto.Veiculos.Add(new Veiculo
            {
                Placa = exemplo.Placa,
                Chassi = $"{PrefixoChassi(exemplo.FabricanteId)}EXEMPLO{sequencia:D7}",
                Modelo = exemplo.Modelo,
                AnoFabricacao = exemplo.Ano,
                AnoModelo = exemplo.Ano + 1,
                Cor = exemplo.Cor,
                Quilometragem = exemplo.Km,
                Combustivel = exemplo.Combustivel,
                Status = StatusVeiculo.Disponivel,
                ValorDiaria = exemplo.Diaria,
                DataAquisicao = new DateTime(exemplo.Ano, 1, 10).AddDays(sequencia * 7),
                ImagemUrl = $"{Commons}{exemplo.Foto}?width=800",
                FabricanteId = exemplo.FabricanteId,
                CategoriaId = exemplo.CategoriaId,
                FilialId = exemplo.FilialId
            });

            // Um SaveChanges por veículo garante que os ids sigam a ordem da lista.
            await contexto.SaveChangesAsync(cancelamento);
        }

        return Frota.Length;
    }

    private static string PrefixoChassi(int fabricanteId) => fabricanteId switch
    {
        Fiat => "9BD",
        Volkswagen => "9BW",
        Chevrolet => "9BG",
        _ => "9BR"
    };
}
