using MySqlConnector;

namespace Ex10_GerenciamentoRestaurantes;

// Exercício 10 - Sistema de Gerenciamento de Restaurantes. Consome o banco "restaurante" via ADO.NET. A tabela Receitas é a junção N:N entre Pratos e Ingredientes, guardando a quantidade necessária.
internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var executando = true;
        while (executando)
        {
            Console.WriteLine();
            Console.WriteLine("===== RESTAURANTE =====");
            Console.WriteLine("1 - Listar ingredientes de um prato");
            Console.WriteLine("2 - Listar pratos");
            Console.WriteLine("3 - Listar ingredientes");
            Console.WriteLine("4 - Cadastrar ingrediente");
            Console.WriteLine("5 - Cadastrar prato (com receita)");
            Console.WriteLine("0 - Sair");

            switch (Db.LerTexto("Opção: "))
            {
                case "1": IngredientesDoPrato();  break;
                case "2": ListarPratos();         break;
                case "3": ListarIngredientes();   break;
                case "4": CadastrarIngrediente(); break;
                case "5": CadastrarPrato();       break;
                case "0": executando = false;     break;
                default:  Console.WriteLine("Opção inválida."); break;
            }
        }
    }

    // Consulta pedida: ingredientes de um prato com quantidade e unidade.
    private static void IngredientesDoPrato()
    {
        var pratoId = Db.LerInt("ID do prato: ");
        const string sql = @"
            SELECT p.nome AS prato, i.nome AS ingrediente,
                   r.quantidade_necessaria, i.unidade_medida
            FROM Receitas r
            INNER JOIN Pratos       p ON r.prato_id       = p.id
            INNER JOIN Ingredientes i ON r.ingrediente_id = i.id
            WHERE r.prato_id = @prato
            ORDER BY i.nome;";

        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@prato", pratoId);
            using var reader = cmd.ExecuteReader();
            Console.WriteLine("\n--- Ingredientes do prato ---");
            var algum = false;
            while (reader.Read())
            {
                algum = true;
                Console.WriteLine($"- {reader.GetString("ingrediente")}: " +
                                  $"{reader.GetDecimal("quantidade_necessaria"):0.##} {reader.GetString("unidade_medida")}");
            }
            if (!algum) Console.WriteLine("Nenhum ingrediente cadastrado para esse prato.");
        }
        catch (Exception ex) { Erro(ex); }
    }

    private static void ListarPratos() =>
        ListarSimples("SELECT id, nome, preco FROM Pratos ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")} - R$ {r.GetDecimal("preco"):0.00}", "Pratos");

    private static void ListarIngredientes() =>
        ListarSimples("SELECT id, nome, unidade_medida FROM Ingredientes ORDER BY nome;",
            r => $"[{r.GetInt32("id")}] {r.GetString("nome")} ({r.GetString("unidade_medida")})", "Ingredientes");

    private static void CadastrarIngrediente()
    {
        var nome    = Db.LerTexto("Nome: ");
        var unidade = Db.LerTexto("Unidade de medida (g, ml, unidade...): ");
        try
        {
            using var conn = Db.Abrir();
            using var cmd = new MySqlCommand(
                "INSERT INTO Ingredientes (nome, unidade_medida) VALUES (@n, @u);", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@u", unidade);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Ingrediente cadastrado (id = {cmd.LastInsertedId}).");
        }
        catch (Exception ex) { Erro(ex); }
    }

    // Cria o prato e sua receita (ingredientes + quantidades) numa transação.
    private static void CadastrarPrato()
    {
        var nome      = Db.LerTexto("Nome do prato: ");
        var descricao = Db.LerTexto("Descrição: ");
        var preco     = Db.LerDecimal("Preço: ");
        try
        {
            using var conn = Db.Abrir();
            using var tx = conn.BeginTransaction();

            using var cmdPrato = new MySqlCommand(
                "INSERT INTO Pratos (nome, descricao, preco) VALUES (@n, @d, @p);", conn, tx);
            cmdPrato.Parameters.AddWithValue("@n", nome);
            cmdPrato.Parameters.AddWithValue("@d", descricao);
            cmdPrato.Parameters.AddWithValue("@p", preco);
            cmdPrato.ExecuteNonQuery();
            var pratoId = cmdPrato.LastInsertedId;

            Console.WriteLine("Agora adicione a receita (ingredientes):");
            while (true)
            {
                var ingId = Db.LerInt("ID do ingrediente (0 para finalizar): ");
                if (ingId == 0) break;
                var qtd = Db.LerDecimal("Quantidade necessária: ");

                using var cmdRec = new MySqlCommand(@"
                    INSERT INTO Receitas (prato_id, ingrediente_id, quantidade_necessaria)
                    VALUES (@prato, @ing, @qtd);", conn, tx);
                cmdRec.Parameters.AddWithValue("@prato", pratoId);
                cmdRec.Parameters.AddWithValue("@ing", ingId);
                cmdRec.Parameters.AddWithValue("@qtd", qtd);
                cmdRec.ExecuteNonQuery();
            }

            tx.Commit();
            Console.WriteLine($"Prato cadastrado (id = {pratoId}).");
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
