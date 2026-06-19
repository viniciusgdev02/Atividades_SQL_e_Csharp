using MySqlConnector;

namespace Ex04_BlogSimples;

// Conexao com o banco + uns helpers pra ler do console.
public static class Db
{
    // ajustar user/senha conforme o ambiente (ou usar a variavel MYSQL_CONN)
    private const string ConexaoPadrao =
        "Server=localhost;Port=3306;Database=blog;User ID=root;Password=;";

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("MYSQL_CONN") ?? ConexaoPadrao;

    // Abre e retorna uma conexao pronta para uso.
    public static MySqlConnection Abrir()
    {
        var conexao = new MySqlConnection(ConnectionString);
        conexao.Open();
        return conexao;
    }

    // ----------------------- Utilitarios de console -----------------------

    // Le um texto nao-nulo do usuario.
    public static string LerTexto(string rotulo)
    {
        Console.Write(rotulo);
        return (Console.ReadLine() ?? string.Empty).Trim();
    }

    // Le um inteiro, repetindo ate receber um valor valido.
    public static int LerInt(string rotulo)
    {
        while (true)
        {
            Console.Write(rotulo);
            if (int.TryParse(Console.ReadLine(), out var valor))
                return valor;
            Console.WriteLine("  Valor invalido. Digite um numero inteiro.");
        }
    }

    // Le um decimal (aceita virgula ou ponto), repetindo ate validar.
    public static decimal LerDecimal(string rotulo)
    {
        while (true)
        {
            Console.Write(rotulo);
            var texto = (Console.ReadLine() ?? string.Empty).Replace(',', '.');
            if (decimal.TryParse(texto, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out var valor))
                return valor;
            Console.WriteLine("  Valor invalido. Digite um numero (ex.: 12.50).");
        }
    }
}
