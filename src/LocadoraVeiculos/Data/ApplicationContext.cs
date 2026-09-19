using LocadoraVeiculos.Models;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Data;

/// <summary>
/// Contexto do Entity Framework Core responsável pelo mapeamento das entidades
/// para as tabelas do SQL Server Express.
/// </summary>
public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    public DbSet<Fabricante> Fabricantes => Set<Fabricante>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Filial> Filiais => Set<Filial>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Aluguel> Alugueis => Set<Aluguel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurarFabricante(modelBuilder);
        ConfigurarCategoria(modelBuilder);
        ConfigurarFilial(modelBuilder);
        ConfigurarVeiculo(modelBuilder);
        ConfigurarCliente(modelBuilder);
        ConfigurarAluguel(modelBuilder);

        PopularDadosIniciais(modelBuilder);
    }

    private static void ConfigurarFabricante(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fabricante>(entidade =>
        {
            entidade.ToTable("Fabricantes");

            // Chave primária
            entidade.HasKey(f => f.FabricanteId);
            entidade.Property(f => f.FabricanteId).UseIdentityColumn();

            entidade.Property(f => f.Nome)
                    .IsRequired()
                    .HasColumnType("varchar(60)");

            entidade.Property(f => f.PaisOrigem)
                    .HasColumnType("varchar(50)");

            entidade.Property(f => f.Ativo)
                    .HasDefaultValue(true);

            // Não pode existir duas marcas com o mesmo nome
            entidade.HasIndex(f => f.Nome)
                    .IsUnique()
                    .HasDatabaseName("UQ_Fabricante_Nome");
        });
    }

    private static void ConfigurarCategoria(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entidade =>
        {
            entidade.ToTable("Categorias", tabela =>
                tabela.HasCheckConstraint("CK_Categoria_ValorDiariaBase", "[ValorDiariaBase] > 0"));

            entidade.HasKey(c => c.CategoriaId);
            entidade.Property(c => c.CategoriaId).UseIdentityColumn();

            entidade.Property(c => c.Nome)
                    .IsRequired()
                    .HasColumnType("varchar(40)");

            entidade.Property(c => c.Descricao)
                    .HasColumnType("varchar(200)");

            entidade.Property(c => c.ValorDiariaBase)
                    .IsRequired()
                    .HasPrecision(10, 2);

            entidade.HasIndex(c => c.Nome)
                    .IsUnique()
                    .HasDatabaseName("UQ_Categoria_Nome");
        });
    }

    private static void ConfigurarFilial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Filial>(entidade =>
        {
            entidade.ToTable("Filiais");

            entidade.HasKey(f => f.FilialId);
            entidade.Property(f => f.FilialId).UseIdentityColumn();

            entidade.Property(f => f.Nome)
                    .IsRequired()
                    .HasColumnType("varchar(80)");

            entidade.Property(f => f.Logradouro)
                    .IsRequired()
                    .HasColumnType("varchar(120)");

            entidade.Property(f => f.Cidade)
                    .IsRequired()
                    .HasColumnType("varchar(60)");

            entidade.Property(f => f.Uf)
                    .IsRequired()
                    .HasColumnType("char(2)");

            entidade.Property(f => f.Cep).HasColumnType("varchar(8)");
            entidade.Property(f => f.Telefone).HasColumnType("varchar(15)");

            entidade.HasIndex(f => new { f.Cidade, f.Nome })
                    .HasDatabaseName("IX_Filial_Cidade_Nome");
        });
    }

    private static void ConfigurarVeiculo(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Veiculo>(entidade =>
        {
            entidade.ToTable("Veiculos", tabela =>
            {
                tabela.HasCheckConstraint("CK_Veiculo_AnoFabricacao", "[AnoFabricacao] BETWEEN 1900 AND 2100");
                tabela.HasCheckConstraint("CK_Veiculo_Quilometragem", "[Quilometragem] >= 0");
                tabela.HasCheckConstraint("CK_Veiculo_ValorDiaria", "[ValorDiaria] > 0");
            });

            entidade.HasKey(v => v.VeiculoId);
            entidade.Property(v => v.VeiculoId).UseIdentityColumn();

            entidade.Property(v => v.Placa)
                    .IsRequired()
                    .HasColumnType("varchar(8)");

            entidade.Property(v => v.Chassi)
                    .IsRequired()
                    .HasColumnType("varchar(17)");

            entidade.Property(v => v.Modelo)
                    .IsRequired()
                    .HasColumnType("varchar(60)");

            entidade.Property(v => v.Cor).HasColumnType("varchar(30)");

            entidade.Property(v => v.Quilometragem).HasDefaultValue(0);

            // Enums gravados como número inteiro no banco
            entidade.Property(v => v.Combustivel).HasConversion<int>();
            entidade.Property(v => v.Status).HasConversion<int>().HasDefaultValue(Models.Enums.StatusVeiculo.Disponivel);

            entidade.Property(v => v.ValorDiaria).HasPrecision(10, 2);
            entidade.Property(v => v.DataAquisicao).HasColumnType("date");

            entidade.HasIndex(v => v.Placa)
                    .IsUnique()
                    .HasDatabaseName("UQ_Veiculo_Placa");

            entidade.HasIndex(v => v.Chassi)
                    .IsUnique()
                    .HasDatabaseName("UQ_Veiculo_Chassi");

            // Chave estrangeira: Veiculo (N) -> Fabricante (1)
            entidade.HasOne(v => v.Fabricante)
                    .WithMany(f => f.Veiculos)
                    .HasForeignKey(v => v.FabricanteId)
                    .HasConstraintName("FK_Veiculo_Fabricante")
                    .OnDelete(DeleteBehavior.Restrict);

            // Chave estrangeira: Veiculo (N) -> Categoria (1)
            entidade.HasOne(v => v.Categoria)
                    .WithMany(c => c.Veiculos)
                    .HasForeignKey(v => v.CategoriaId)
                    .HasConstraintName("FK_Veiculo_Categoria")
                    .OnDelete(DeleteBehavior.Restrict);

            // Chave estrangeira: Veiculo (N) -> Filial (1)
            entidade.HasOne(v => v.Filial)
                    .WithMany(f => f.Veiculos)
                    .HasForeignKey(v => v.FilialId)
                    .HasConstraintName("FK_Veiculo_Filial")
                    .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarCliente(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entidade =>
        {
            entidade.ToTable("Clientes", tabela =>
                tabela.HasCheckConstraint("CK_Cliente_Cpf", "LEN([Cpf]) = 11"));

            entidade.HasKey(c => c.ClienteId);
            entidade.Property(c => c.ClienteId).UseIdentityColumn();

            entidade.Property(c => c.Nome)
                    .IsRequired()
                    .HasColumnType("varchar(120)");

            entidade.Property(c => c.Cpf)
                    .IsRequired()
                    .HasColumnType("char(11)");

            entidade.Property(c => c.Email)
                    .IsRequired()
                    .HasColumnType("varchar(120)");

            entidade.Property(c => c.Telefone).HasColumnType("varchar(15)");
            entidade.Property(c => c.NumeroCnh).IsRequired().HasColumnType("varchar(11)");
            entidade.Property(c => c.Endereco).HasColumnType("varchar(120)");
            entidade.Property(c => c.Cidade).HasColumnType("varchar(60)");
            entidade.Property(c => c.Uf).HasColumnType("char(2)");

            entidade.Property(c => c.DataCadastro)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETDATE()");

            entidade.HasIndex(c => c.Cpf)
                    .IsUnique()
                    .HasDatabaseName("UQ_Cliente_Cpf");

            entidade.HasIndex(c => c.Email)
                    .IsUnique()
                    .HasDatabaseName("UQ_Cliente_Email");

            entidade.HasIndex(c => c.NumeroCnh)
                    .IsUnique()
                    .HasDatabaseName("UQ_Cliente_Cnh");
        });
    }

    private static void ConfigurarAluguel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aluguel>(entidade =>
        {
            entidade.ToTable("Alugueis", tabela =>
            {
                tabela.HasCheckConstraint("CK_Aluguel_Periodo",
                    "[DataDevolucaoPrevista] > [DataRetirada]");

                tabela.HasCheckConstraint("CK_Aluguel_Quilometragem",
                    "[QuilometragemFinal] IS NULL OR [QuilometragemFinal] >= [QuilometragemInicial]");

                tabela.HasCheckConstraint("CK_Aluguel_ValorDiaria", "[ValorDiaria] > 0");
            });

            entidade.HasKey(a => a.AluguelId);
            entidade.Property(a => a.AluguelId).UseIdentityColumn();

            entidade.Property(a => a.DataRetirada).HasColumnType("datetime2").IsRequired();
            entidade.Property(a => a.DataDevolucaoPrevista).HasColumnType("datetime2").IsRequired();
            entidade.Property(a => a.DataDevolucaoEfetiva).HasColumnType("datetime2");

            entidade.Property(a => a.QuilometragemInicial).IsRequired();

            entidade.Property(a => a.ValorDiaria).HasPrecision(10, 2).IsRequired();
            entidade.Property(a => a.ValorMulta).HasPrecision(10, 2);
            entidade.Property(a => a.ValorTotal).HasPrecision(10, 2);

            entidade.Property(a => a.Status)
                    .HasConversion<int>()
                    .HasDefaultValue(Models.Enums.StatusAluguel.EmAndamento);

            entidade.Property(a => a.Observacoes).HasColumnType("varchar(300)");

            entidade.Ignore(a => a.DiasContratados);
            entidade.Ignore(a => a.QuilometragemRodada);

            // Chave estrangeira: Aluguel (N) -> Cliente (1)
            entidade.HasOne(a => a.Cliente)
                    .WithMany(c => c.Alugueis)
                    .HasForeignKey(a => a.ClienteId)
                    .HasConstraintName("FK_Aluguel_Cliente")
                    .OnDelete(DeleteBehavior.Restrict);

            // Chave estrangeira: Aluguel (N) -> Veiculo (1)
            entidade.HasOne(a => a.Veiculo)
                    .WithMany(v => v.Alugueis)
                    .HasForeignKey(a => a.VeiculoId)
                    .HasConstraintName("FK_Aluguel_Veiculo")
                    .OnDelete(DeleteBehavior.Restrict);

            // Chave estrangeira: Aluguel (N) -> Filial (1)
            entidade.HasOne(a => a.Filial)
                    .WithMany(f => f.Alugueis)
                    .HasForeignKey(a => a.FilialId)
                    .HasConstraintName("FK_Aluguel_Filial")
                    .OnDelete(DeleteBehavior.Restrict);

            entidade.HasIndex(a => new { a.VeiculoId, a.DataRetirada })
                    .HasDatabaseName("IX_Aluguel_Veiculo_DataRetirada");

            entidade.HasIndex(a => a.ClienteId)
                    .HasDatabaseName("IX_Aluguel_Cliente");
        });
    }

    /// <summary>
    /// Carga inicial das tabelas de apoio, para o banco não nascer vazio.
    /// </summary>
    private static void PopularDadosIniciais(ModelBuilder modelBuilder)
    {
        // Objetos anônimos são usados aqui para que o HasData não esbarre
        // nas propriedades de navegação das entidades.
        modelBuilder.Entity<Fabricante>().HasData(
            new { FabricanteId = 1, Nome = "Fiat", PaisOrigem = "Itália", AnoFundacao = (int?)1899, Ativo = true },
            new { FabricanteId = 2, Nome = "Volkswagen", PaisOrigem = "Alemanha", AnoFundacao = (int?)1937, Ativo = true },
            new { FabricanteId = 3, Nome = "Chevrolet", PaisOrigem = "Estados Unidos", AnoFundacao = (int?)1911, Ativo = true },
            new { FabricanteId = 4, Nome = "Toyota", PaisOrigem = "Japão", AnoFundacao = (int?)1937, Ativo = true }
        );

        modelBuilder.Entity<Categoria>().HasData(
            new { CategoriaId = 1, Nome = "Hatch Compacto", Descricao = "Carros de entrada, 4 portas e ar-condicionado", ValorDiariaBase = 110.00m },
            new { CategoriaId = 2, Nome = "Sedan", Descricao = "Porta-malas maior, indicado para viagens", ValorDiariaBase = 165.00m },
            new { CategoriaId = 3, Nome = "SUV", Descricao = "Veículos altos, 5 lugares", ValorDiariaBase = 240.00m },
            new { CategoriaId = 4, Nome = "Utilitário", Descricao = "Picapes e furgões para carga", ValorDiariaBase = 290.00m }
        );

        modelBuilder.Entity<Filial>().HasData(
            new { FilialId = 1, Nome = "Matriz Savassi", Logradouro = "Rua Pernambuco, 1200", Cidade = "Belo Horizonte", Uf = "MG", Cep = "30130151", Telefone = "3132221010" },
            new { FilialId = 2, Nome = "Filial Aeroporto Confins", Logradouro = "Rodovia MG-010, s/n", Cidade = "Confins", Uf = "MG", Cep = "33500000", Telefone = "3136891020" },
            new { FilialId = 3, Nome = "Filial Contagem", Logradouro = "Av. João César de Oliveira, 500", Cidade = "Contagem", Uf = "MG", Cep = "32210000", Telefone = "3133912030" }
        );
    }
}
