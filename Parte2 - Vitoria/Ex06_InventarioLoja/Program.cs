using MySqlConnector;

namespace Ex06_InventarioLoja;

// Exercício 6 - Sistema de Inventário de Loja. Consome o banco "inventario_loja" via ADO.NET. O estoque atual NÃO é armazenado: é calculado a partir das movimentações (entradas - saídas).
internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var executando = true;
        while (executando)
        {
            Console.WriteLine();
            Console.WriteLine("===== INVENTÁRIO DA LOJA =====");
            Console.WriteLine("1 - Ver estoque atual de todos os produtos");
            Console.WriteLine("2 - Listar fornecedores");
            Console.WriteLine("3 - Listar produtos");
            Console.WriteLine("4 - Cadastrar fornecedor");
            Console.WriteLine("5 - Cadastrar produto");
            Console.WriteLine("6 - Registrar movimentação (entrada/saída)");
            Console.WriteLine("0 - Sair");

            switch (Db.LerTexto("Opção: "))
            {
                case "1": EstoqueAtual();         break;
                case "2": ListarFornecedores();   break;
                case "3": ListarProdutos();       break;
                case "4": CadastrarFornecedor();  break;
                case "5": CadastrarProduto();     break;
                case "6": RegistrarMovimentacao();break;
                case "0": executando = false;     break;
                default:  Console.WriteLine("Opção inválida."); break;
            }
        }
    }

    // Consulta pedida: estoque = soma(entradas) - soma(saídas).
    private static void EstoqueAtual()
    {
        const string sql = @"
            SELECT p.nome,
                   COALESCE(SUM(CASE WHEN t.tipo_transacao = 'entrada' THEN t.quantidade ELSE 0 END), 0)
                 - COALESCE(SUM(CASE WHEN t.tipo_transacao = 'saida'   THEN t.quantidade ELSE 0 END), 0)
                   AS estoque_atual
            FROM Produtos p
            LEFT JOIN TransacoesEstoque t ON t.produto_id = p.id
            GROUP BY p.id, p.nome
            ORDER BY p.nome;";

        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            Console.WriteLine("\n--- Estoque atual ---");
            while (reader.Read())
                Console.WriteLine($"{reader.GetString("nome"),-20} {reader.GetDecimal("estoque_atual"),6} un.");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void ListarFornecedores() =>
        ListarSimples("SELECT id, nome, telefone FROM Fornecedores ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")} - {r.GetString("telefone")}", "Fornecedores");

    private static void ListarProdutos() =>
        ListarSimples("SELECT id, nome, preco_venda FROM Produtos ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")} - R$ {r.GetDecimal("preco_venda"):0.00}", "Produtos");

    private static void CadastrarFornecedor()
    {
        var nome     = Db.LerTexto("Nome: ");
        var contato  = Db.LerTexto("Contato: ");
        var telefone = Db.LerTexto("Telefone: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(
                "INSERT INTO Fornecedores (nome, contato, telefone) VALUES (@n, @c, @t);", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@c", contato);
            cmd.Parameters.AddWithValue("@t", telefone);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Fornecedor cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void CadastrarProduto()
    {
        var nome         = Db.LerTexto("Nome: ");
        var descricao    = Db.LerTexto("Descrição: ");
        var precoCompra  = Db.LerDecimal("Preço de compra: ");
        var precoVenda   = Db.LerDecimal("Preço de venda: ");
        var fornecedorId = Db.LerInt("ID do fornecedor: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(@"
                INSERT INTO Produtos (nome, descricao, preco_compra, preco_venda, fornecedor_id)
                VALUES (@n, @d, @pc, @pv, @f);", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@d", descricao);
            cmd.Parameters.AddWithValue("@pc", precoCompra);
            cmd.Parameters.AddWithValue("@pv", precoVenda);
            cmd.Parameters.AddWithValue("@f", fornecedorId);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Produto cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void RegistrarMovimentacao()
    {
        var produtoId = Db.LerInt("ID do produto: ");
        var tipo      = Db.LerTexto("Tipo (entrada/saida): ").ToLower();
        if (tipo != "entrada" && tipo != "saida")
        {
            Console.WriteLine("Tipo inválido. Use 'entrada' ou 'saida'.");
            return;
        }
        var qtd = Db.LerInt("Quantidade: ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(@"
                INSERT INTO TransacoesEstoque (produto_id, tipo_transacao, quantidade)
                VALUES (@p, @tipo, @q);", conn);
            cmd.Parameters.AddWithValue("@p", produtoId);
            cmd.Parameters.AddWithValue("@tipo", tipo);
            cmd.Parameters.AddWithValue("@q", qtd);
            cmd.ExecuteNonQuery();
            Console.WriteLine("Movimentação registrada.");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void ListarSimples(string sql, Func<MySqlDataReader, string> formato, string titulo)
    {
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            Console.WriteLine($"\n--- {titulo} ---");
            while (reader.Read())
                Console.WriteLine(formato(reader));
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void Erro(Exception ex) => Console.WriteLine($"[ERRO] {ex.Message}");
}
